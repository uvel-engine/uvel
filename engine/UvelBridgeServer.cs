using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace Uvel
{
    /// <summary>
    /// Localhost bridge for uflow.uz/uvel Workspace.
    ///
    /// Browser security does not allow a website to launch an EXE directly.
    /// The bridge is the explicit user-approved local process: it listens only
    /// on 127.0.0.1, accepts WebSocket connections from the browser, writes the
    /// submitted XML to a local workspace file, and runs uvel.exe in dev mode.
    ///
    /// Supported JSON messages:
    ///   { "type":"run",     "fileName":"App.xml", "code":"..." }
    ///   { "type":"reload",  "fileName":"App.xml", "code":"..." }
    ///   { "type":"restart", "fileName":"App.xml", "code":"..." }
    ///   { "type":"stop" }
    ///   { "type":"ping" }
    /// </summary>
    public class UvelBridgeServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private bool _running;
        private readonly List<WebSocketClient> _clients = new List<WebSocketClient>();
        private readonly object _clientsLock = new object();
        private readonly object _processLock = new object();
        private Process _child;
        private string _workspaceDir;
        private string _currentXmlPath;
        private int _devtoolsPort = 9328;

        public UvelBridgeServer(int port)
        {
            _port = port;
            _workspaceDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Uvel", "Workspace");
            _currentXmlPath = Path.Combine(_workspaceDir, "App.xml");
        }

        public void Start()
        {
            Directory.CreateDirectory(_workspaceDir);
            EnsureWorkspaceLibraries();
            _running = true;
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);
            _listener.Start();

            Log("BRIDGE", "Uvel Bridge listening on ws://127.0.0.1:" + _port + "/workspace");
            Log("BRIDGE", "Workspace: " + _workspaceDir);
            Log("BRIDGE", "Open https://uflow.uz/uvel and press Connect.");

            while (_running)
            {
                try
                {
                    TcpClient tcp = _listener.AcceptTcpClient();
                    Thread t = new Thread(delegate() { HandleClient(tcp); });
                    t.IsBackground = true;
                    t.Start();
                }
                catch (SocketException)
                {
                    if (_running) Log("ERROR", "Socket accept failed");
                }
                catch (Exception ex)
                {
                    Log("ERROR", "Accept failed: " + ex.Message);
                }
            }
        }


        private void EnsureWorkspaceLibraries()
        {
            try
            {
                string dir = Path.Combine(_workspaceDir, "uvel");
                Directory.CreateDirectory(dir);
                WriteAlways(Path.Combine(dir, "ui.xml"), "<UvelLibrary Name=\"uvel.ui\" Version=\"1.0\"><Logic><Handler Name=\"uvel.ui.toast.success\"><Toast Message=\"{message}\" Type=\"success\" /></Handler><Handler Name=\"uvel.ui.toast.info\"><Toast Message=\"{message}\" Type=\"info\" /></Handler><Handler Name=\"uvel.ui.clear.status\"><Set Target=\"status\" Property=\"Text\" Value=\"\" /></Handler></Logic></UvelLibrary>");
                WriteAlways(Path.Combine(dir, "backend.xml"), "<UvelLibrary Name=\"uvel.backend\" Version=\"1.0\"><Logic><Var Name=\"uvel.backend.ready\" Value=\"true\" Type=\"string\" /><Handler Name=\"uvel.backend.ping\"><Set Target=\"status\" Property=\"Text\" Value=\"Backend ready\" /></Handler><Handler Name=\"uvel.backend.time\"><Plugin Name=\"DatePlugin\" Method=\"now\" ToState=\"uvel.backend.now\" /><Set Target=\"status\" Property=\"Text\" Value=\"{uvel.backend.now}\" /></Handler></Logic></UvelLibrary>");
                WriteAlways(Path.Combine(dir, "net.xml"), "<UvelLibrary Name=\"uvel.net\" Version=\"1.0\"><Logic><Handler Name=\"uvel.net.online\"><Set Target=\"status\" Property=\"Text\" Value=\"Network module loaded\" /></Handler></Logic></UvelLibrary>");
                WriteAlways(Path.Combine(dir, "data.xml"), "<UvelLibrary Name=\"uvel.data\" Version=\"1.0\"><Logic><Var Name=\"uvel.data.loaded\" Value=\"true\" Type=\"string\" /><Handler Name=\"uvel.data.ready\"><Set Target=\"status\" Property=\"Text\" Value=\"Data module loaded\" /></Handler></Logic></UvelLibrary>");
                WriteAlways(Path.Combine(dir, "icons.xml"), "<UvelLibrary Name=\"uvel.icons\" Version=\"1.0\"><Logic><Var Name=\"uvel.icons.source\" Value=\"offline: uvel/icons/uflow-icons.js\" Type=\"string\" /><Handler Name=\"uvel.icons.ready\"><Set Target=\"status\" Property=\"Text\" Value=\"Uvel Icons loaded offline\" /></Handler></Logic></UvelLibrary>");
                WriteAlways(Path.Combine(dir, "all.xml"), "<UvelLibrary Name=\"uvel.all\" Version=\"1.0\"><Import Package=\"uvel.ui\" /><Import Package=\"uvel.backend\" /><Import Package=\"uvel.net\" /><Import Package=\"uvel.data\" /><Import Package=\"uvel.icons\" /></UvelLibrary>");
                Directory.CreateDirectory(Path.Combine(dir, "icons"));
                WriteIfMissing(Path.Combine(dir, "icons", "README.txt"), "Offline Uvel Icons cache. Full package ships in engine/bin/uvel/icons after build.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[BRIDGE] Library setup failed: " + ex.Message);
            }
        }

        private void WriteIfMissing(string path, string content)
        {
            if (!File.Exists(path)) File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        private void WriteAlways(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        public void Stop()
        {
            _running = false;
            try { if (_listener != null) _listener.Stop(); } catch { }
            StopChild();
            lock (_clientsLock)
            {
                foreach (WebSocketClient c in _clients.ToArray()) c.Close();
                _clients.Clear();
            }
        }

        private void HandleClient(TcpClient tcp)
        {
            WebSocketClient ws = null;
            try
            {
                NetworkStream stream = tcp.GetStream();
                string request = ReadHttpHeader(stream);
                if (request.IndexOf("Upgrade: websocket", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    WriteHttp(stream, "400 Bad Request", "Expected WebSocket");
                    tcp.Close();
                    return;
                }

                string key = Header(request, "Sec-WebSocket-Key");
                if (string.IsNullOrEmpty(key))
                {
                    WriteHttp(stream, "400 Bad Request", "Missing key");
                    tcp.Close();
                    return;
                }

                string accept = WebSocketAccept(key);
                string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                                  "Upgrade: websocket\r\n" +
                                  "Connection: Upgrade\r\n" +
                                  "Sec-WebSocket-Accept: " + accept + "\r\n" +
                                  "Access-Control-Allow-Origin: *\r\n" +
                                  "\r\n";
                byte[] bytes = Encoding.ASCII.GetBytes(response);
                stream.Write(bytes, 0, bytes.Length);

                ws = new WebSocketClient(tcp, stream, this);
                lock (_clientsLock) _clients.Add(ws);
                Log("CLIENT", "Workspace connected");
                ws.Send(Json("hello", "Connected to Uvel Bridge", "port", _port.ToString()));
                ws.Send(Json("status", IsChildRunning() ? "Uvel app is running" : "Ready", "workspace", _workspaceDir));

                while (_running && tcp.Connected)
                {
                    string message = ws.ReadTextFrame();
                    if (message == null) break;
                    OnMessage(ws, message);
                }
            }
            catch (Exception ex)
            {
                Log("CLIENT", "Disconnected: " + ex.Message);
            }
            finally
            {
                if (ws != null)
                {
                    lock (_clientsLock) _clients.Remove(ws);
                    ws.Close();
                }
            }
        }

        private void OnMessage(WebSocketClient ws, string message)
        {
            string type = ExtractJsonString(message, "type").ToLower();
            if (type == "ping" || type == "status" || type == "verify")
            {
                ws.Send(StatusJson("ready"));
                return;
            }
            if (type == "openfolder" || type == "open-folder" || type == "open_folder")
            {
                OpenWorkspaceFolder();
                ws.Send(StatusJson("Workspace folder opened"));
                return;
            }
            if (type == "createproject" || type == "create-project" || type == "create_project")
            {
                CreateDefaultProject();
                ws.Send(StatusJson("Uvel project files created"));
                ws.Send(FilesJson());
                return;
            }
            if (type == "list" || type == "listfiles" || type == "list-files")
            {
                ws.Send(FilesJson());
                return;
            }
            if (type == "read" || type == "readfile" || type == "read-file")
            {
                string fileName = ExtractJsonString(message, "fileName");
                string path = ResolveWorkspacePath(fileName);
                if (!File.Exists(path)) { ws.Send(Json("error", "File not found: " + fileName)); return; }
                ws.Send(FileJson(RelativePath(path), File.ReadAllText(path, Encoding.UTF8)));
                return;
            }
            if (type == "write" || type == "writefile" || type == "write-file")
            {
                string fileName = ExtractJsonString(message, "fileName");
                string code = ExtractJsonString(message, "code");
                string path = ResolveWorkspacePath(fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, code, new UTF8Encoding(false));
                Log("WORKSPACE", "Saved " + path);
                ws.Send(StatusJson("Saved " + RelativePath(path)));
                return;
            }
            if (type == "stop")
            {
                StopChild();
                Broadcast(Json("status", "Stopped"));
                return;
            }

            if (type == "run" || type == "reload" || type == "restart")
            {
                string code = ExtractJsonString(message, "code");
                string fileName = ExtractJsonString(message, "fileName");
                if (string.IsNullOrEmpty(fileName)) fileName = "App.xml";
                string path = ResolveWorkspacePath(fileName);

                if (!string.IsNullOrEmpty(code))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, code, new UTF8Encoding(false));
                    Log("WORKSPACE", "Saved " + path);
                }
                else if (!File.Exists(path))
                {
                    ws.Send(Json("error", "No XML code received and file does not exist: " + fileName));
                    return;
                }

                _currentXmlPath = path;

                if (type == "restart")
                {
                    Broadcast(Json("status", "Hot restart requested"));
                    StopChild();
                    StartChild();
                }
                else if (!IsChildRunning())
                {
                    Broadcast(Json("status", "Starting Uvel app"));
                    StartChild();
                }
                else
                {
                    Broadcast(Json("status", "Hot reload file updated"));
                }
                return;
            }

            ws.Send(Json("error", "Unknown command: " + type));
        }

        private void OpenWorkspaceFolder()
        {
            try
            {
                Directory.CreateDirectory(_workspaceDir);
                Process.Start("explorer.exe", _workspaceDir);
                Log("WORKSPACE", "Opened folder " + _workspaceDir);
            }
            catch (Exception ex)
            {
                Log("ERROR", "Open folder failed: " + ex.Message);
            }
        }

        private void CreateDefaultProject()
        {
            Directory.CreateDirectory(_workspaceDir);
            Directory.CreateDirectory(Path.Combine(_workspaceDir, "components"));
            EnsureWorkspaceLibraries();

            WriteAlways(Path.Combine(_workspaceDir, "App.xml"), DefaultAppXml());
            WriteAlways(Path.Combine(_workspaceDir, "components", "README.xml"), "<UvelLibrary Name=\"components\"><Logic><Handler Name=\"components.ready\"><Set Target=\"status\" Property=\"Text\" Value=\"Components folder is ready\" /></Handler></Logic></UvelLibrary>");
            _currentXmlPath = Path.Combine(_workspaceDir, "App.xml");
            Log("WORKSPACE", "Default project created");
        }

        private string DefaultAppXml()
        {
            return "<App Name=\"Uvel Workspace App\" Width=\"860\" Height=\"540\" Theme=\"Dark\">\n" +
                   "  <Import Package=\"uvel.ui\" />\n" +
                   "  <Import Package=\"uvel.backend\" />\n" +
                   "  <Import Package=\"uvel.icons\" />\n" +
                   "  <UI>\n" +
                   "    <Grid Background=\"#0B0F19\">\n" +
                   "      <StackPanel VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" Width=\"520\">\n" +
                   "        <Border Background=\"#FFFFFF14\" BorderBrush=\"#FFFFFF24\" BorderThickness=\"1\" CornerRadius=\"28\" Padding=\"28\">\n" +
                   "          <StackPanel>\n" +
                   "            <TextBlock Text=\"Uvel Workspace\" FontSize=\"34\" Foreground=\"White\" FontWeight=\"Bold\" HorizontalAlignment=\"Center\"/>\n" +
                   "            <TextBlock Name=\"status\" Text=\"Ready. Click the button.\" FontSize=\"14\" Foreground=\"#94A3B8\" Margin=\"0,12,0,22\" HorizontalAlignment=\"Center\"/>\n" +
                   "            <Button Content=\"Call uvel.backend.ping\" onClick=\"uvel.backend.ping\" Background=\"#34C759\" Foreground=\"White\" CornerRadius=\"18\" Padding=\"18,10\"/>\n" +
                   "          </StackPanel>\n" +
                   "        </Border>\n" +
                   "      </StackPanel>\n" +
                   "    </Grid>\n" +
                   "  </UI>\n" +
                   "</App>\n";
        }

        private string ResolveWorkspacePath(string fileName)
        {
            string rel = SafeRelativePath(fileName);
            string full = Path.GetFullPath(Path.Combine(_workspaceDir, rel));
            string root = Path.GetFullPath(_workspaceDir);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid workspace path");
            return full;
        }

        private string SafeRelativePath(string name)
        {
            if (string.IsNullOrEmpty(name)) name = "App.xml";
            name = name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string[] parts = name.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            List<string> safe = new List<string>();
            foreach (string part in parts)
            {
                if (part == "." || part == "..") continue;
                string p = part;
                foreach (char c in Path.GetInvalidFileNameChars()) p = p.Replace(c, '_');
                if (!string.IsNullOrEmpty(p)) safe.Add(p);
            }
            if (safe.Count == 0) safe.Add("App.xml");
            string rel = Path.Combine(safe.ToArray());
            if (!rel.ToLower().EndsWith(".xml")) rel += ".xml";
            return rel;
        }

        private string RelativePath(string full)
        {
            string root = Path.GetFullPath(_workspaceDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string f = Path.GetFullPath(full);
            if (f.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return f.Substring(root.Length).Replace(Path.DirectorySeparatorChar, '/');
            return Path.GetFileName(full);
        }

        private string[] XmlFiles()
        {
            if (!Directory.Exists(_workspaceDir)) return new string[0];
            string[] files = Directory.GetFiles(_workspaceDir, "*.xml", SearchOption.AllDirectories);
            Array.Sort(files);
            for (int i = 0; i < files.Length; i++) files[i] = RelativePath(files[i]);
            return files;
        }

        private string StatusJson(string message)
        {
            return "{\"type\":\"status\",\"message\":\"" + Escape(message) + "\",\"version\":\"3.0.0\",\"workspace\":\"" + Escape(_workspaceDir) + "\",\"running\":" + (IsChildRunning() ? "true" : "false") + ",\"currentFile\":\"" + Escape(RelativePath(_currentXmlPath)) + "\"}";
        }

        private string FilesJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"type\":\"files\",\"files\":[");
            string[] files = XmlFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append("\"").Append(Escape(files[i])).Append("\"");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private string FileJson(string fileName, string content)
        {
            return "{\"type\":\"file\",\"fileName\":\"" + Escape(fileName) + "\",\"code\":\"" + Escape(content) + "\"}";
        }

        private void StartChild()
        {
            lock (_processLock)
            {
                if (IsChildRunning()) return;
                string exe = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exe;
                psi.Arguments = "dev \"" + _currentXmlPath + "\" --port " + _devtoolsPort + " --no-debug";
                psi.WorkingDirectory = _workspaceDir;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = false;

                _child = new Process();
                _child.StartInfo = psi;
                _child.EnableRaisingEvents = true;
                _child.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null) Log("APP", e.Data);
                };
                _child.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null) Log("APP-ERR", e.Data);
                };
                _child.Exited += delegate
                {
                    Log("APP", "Uvel app exited with code " + _child.ExitCode);
                    Broadcast(Json("status", "Uvel app exited"));
                };

                _child.Start();
                _child.BeginOutputReadLine();
                _child.BeginErrorReadLine();
                Log("APP", "Started: " + psi.FileName + " " + psi.Arguments);
                Broadcast(Json("status", "Uvel app running", "devtools", "http://127.0.0.1:" + _devtoolsPort + "/"));
            }
        }

        private void StopChild()
        {
            lock (_processLock)
            {
                try
                {
                    if (_child != null && !_child.HasExited)
                    {
                        Log("APP", "Stopping Uvel app...");
                        _child.Kill();
                        _child.WaitForExit(2000);
                    }
                }
                catch (Exception ex)
                {
                    Log("ERROR", "Stop failed: " + ex.Message);
                }
                finally
                {
                    _child = null;
                }
            }
        }

        private bool IsChildRunning()
        {
            try { return _child != null && !_child.HasExited; }
            catch { return false; }
        }

        private void Log(string scope, string message)
        {
            string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] [" + scope + "] " + message;
            Console.WriteLine(line);
            Broadcast(Json("log", line, "scope", scope));
        }

        private void Broadcast(string json)
        {
            lock (_clientsLock)
            {
                foreach (WebSocketClient c in _clients.ToArray())
                {
                    try { c.Send(json); }
                    catch { _clients.Remove(c); }
                }
            }
        }

        private static string ReadHttpHeader(NetworkStream stream)
        {
            List<byte> data = new List<byte>();
            byte[] b = new byte[1];
            while (stream.Read(b, 0, 1) == 1)
            {
                data.Add(b[0]);
                int n = data.Count;
                if (n >= 4 && data[n - 4] == 13 && data[n - 3] == 10 && data[n - 2] == 13 && data[n - 1] == 10) break;
                if (data.Count > 16384) break;
            }
            return Encoding.ASCII.GetString(data.ToArray());
        }

        private static string Header(string request, string name)
        {
            string[] lines = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                int idx = line.IndexOf(':');
                if (idx <= 0) continue;
                if (string.Compare(line.Substring(0, idx).Trim(), name, true) == 0)
                    return line.Substring(idx + 1).Trim();
            }
            return "";
        }

        private static string WebSocketAccept(string key)
        {
            string raw = key.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            SHA1 sha = SHA1.Create();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(raw));
            return Convert.ToBase64String(hash);
        }

        private static void WriteHttp(NetworkStream s, string status, string body)
        {
            byte[] b = Encoding.UTF8.GetBytes(body);
            string h = "HTTP/1.1 " + status + "\r\nContent-Type: text/plain\r\nContent-Length: " + b.Length + "\r\n\r\n";
            byte[] hb = Encoding.ASCII.GetBytes(h);
            s.Write(hb, 0, hb.Length);
            s.Write(b, 0, b.Length);
        }

        private static string SafeFileName(string name)
        {
            name = Path.GetFileName(name);
            if (string.IsNullOrEmpty(name)) name = "App.xml";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            if (!name.ToLower().EndsWith(".xml")) name += ".xml";
            return name;
        }

        private static string ExtractJsonString(string json, string key)
        {
            Match m = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"((?:\\\\.|[^\\\"])*)\\\"");
            if (!m.Success) return "";
            return JsonUnescape(m.Groups[1].Value);
        }

        private static string JsonUnescape(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    if (n == 'n') sb.Append('\n');
                    else if (n == 'r') sb.Append('\r');
                    else if (n == 't') sb.Append('\t');
                    else if (n == 'b') sb.Append('\b');
                    else if (n == 'f') sb.Append('\f');
                    else if (n == 'u' && i + 4 < s.Length)
                    {
                        string hex = s.Substring(i + 1, 4);
                        try { sb.Append((char)Convert.ToInt32(hex, 16)); } catch { }
                        i += 4;
                    }
                    else sb.Append(n);
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static string Json(string type, string message)
        {
            return "{\"type\":\"" + Escape(type) + "\",\"message\":\"" + Escape(message) + "\"}";
        }

        private static string Json(string type, string message, string extraKey, string extraValue)
        {
            return "{\"type\":\"" + Escape(type) + "\",\"message\":\"" + Escape(message) + "\",\"" + Escape(extraKey) + "\":\"" + Escape(extraValue) + "\"}";
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }

    internal class WebSocketClient
    {
        private TcpClient _tcp;
        private NetworkStream _stream;
        private UvelBridgeServer _server;
        private readonly object _sendLock = new object();

        public WebSocketClient(TcpClient tcp, NetworkStream stream, UvelBridgeServer server)
        {
            _tcp = tcp;
            _stream = stream;
            _server = server;
        }

        public string ReadTextFrame()
        {
            int b1 = _stream.ReadByte();
            if (b1 < 0) return null;
            int b2 = _stream.ReadByte();
            if (b2 < 0) return null;

            int opcode = b1 & 0x0F;
            if (opcode == 8) return null; // close
            bool masked = (b2 & 0x80) != 0;
            ulong len = (ulong)(b2 & 0x7F);
            if (len == 126)
            {
                byte[] ext = ReadExact(2);
                len = (ulong)((ext[0] << 8) | ext[1]);
            }
            else if (len == 127)
            {
                byte[] ext = ReadExact(8);
                len = 0;
                for (int i = 0; i < 8; i++) len = (len << 8) + ext[i];
            }
            if (len > 1024 * 1024) throw new Exception("Frame too large");

            byte[] mask = masked ? ReadExact(4) : null;
            byte[] payload = ReadExact((int)len);
            if (masked)
            {
                for (int i = 0; i < payload.Length; i++) payload[i] = (byte)(payload[i] ^ mask[i % 4]);
            }
            if (opcode != 1) return "";
            return Encoding.UTF8.GetString(payload);
        }

        public void Send(string text)
        {
            byte[] payload = Encoding.UTF8.GetBytes(text);
            List<byte> frame = new List<byte>();
            frame.Add(0x81);
            if (payload.Length < 126)
            {
                frame.Add((byte)payload.Length);
            }
            else if (payload.Length <= 65535)
            {
                frame.Add(126);
                frame.Add((byte)((payload.Length >> 8) & 255));
                frame.Add((byte)(payload.Length & 255));
            }
            else
            {
                frame.Add(127);
                ulong len = (ulong)payload.Length;
                for (int i = 7; i >= 0; i--) frame.Add((byte)((len >> (8 * i)) & 255));
            }
            frame.AddRange(payload);
            byte[] data = frame.ToArray();
            lock (_sendLock)
            {
                _stream.Write(data, 0, data.Length);
            }
        }

        public void Close()
        {
            try { _tcp.Close(); } catch { }
        }

        private byte[] ReadExact(int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int n = _stream.Read(buffer, offset, count - offset);
                if (n <= 0) throw new IOException("Socket closed");
                offset += n;
            }
            return buffer;
        }
    }
}
