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
            if (type == "ping")
            {
                ws.Send(Json("pong", "ok"));
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
                if (string.IsNullOrEmpty(code))
                {
                    ws.Send(Json("error", "No XML code received"));
                    return;
                }

                string safeFile = SafeFileName(fileName);
                _currentXmlPath = Path.Combine(_workspaceDir, safeFile);
                File.WriteAllText(_currentXmlPath, code, new UTF8Encoding(false));
                Log("WORKSPACE", "Saved " + _currentXmlPath);

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
                    // In dev mode Uvel watches the file. Rewriting App.xml is
                    // the hot reload signal; no restart is necessary.
                    Broadcast(Json("status", "Hot reload file updated"));
                }
                return;
            }

            ws.Send(Json("error", "Unknown command: " + type));
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
