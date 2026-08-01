using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using Uvel.WPF;

namespace Uvel
{
    /// <summary>
    /// DevTools HTTP Server for Uvel Framework
    /// Provides REST API + simple WebSocket-like polling for dev tools
    /// Compatible with .NET Framework 4.0+ (no async/await, no $"")
    /// </summary>
    public class DevToolsServer
    {
        private WpfEngine _engine;
        private HttpListener _listener;
        private Thread _serverThread;
        private int _port;
        private bool _isRunning;
        private List<string> _connectedClients;
        private readonly object _clientLock = new object();
        private int _totalConnections;

        public bool IsRunning { get { return _isRunning; } }

        public int ClientCount
        {
            get { lock (_clientLock) { return _connectedClients.Count; } }
        }

        public int TotalConnections
        {
            get { return _totalConnections; }
        }

        // Events
        public event Action<string> OnRequest;
        public event Action<string> OnResponse;
        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;

        public DevToolsServer(WpfEngine engine, int port)
        {
            _engine = engine;
            _port = port;
            _connectedClients = new List<string>();
            _totalConnections = 0;
        }

        #region Start / Stop

        public void Start()
        {
            if (_isRunning) return;

            _serverThread = new Thread(RunServer);
            _serverThread.IsBackground = true;
            _serverThread.Name = "UvelDevToolsServer";
            _serverThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                if (_listener != null)
                {
                    _listener.Stop();
                    _listener.Close();
                }
            }
            catch { }

            lock (_clientLock)
            {
                _connectedClients.Clear();
            }
        }

        private void RunServer()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");

                try
                {
                    _listener.Start();
                }
                catch (HttpListenerException ex)
                {
                    // Port might be in use, try next port
                    _port = _port + 1;
                    _listener = new HttpListener();
                    _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
                    try
                    {
                        _listener.Start();
                    }
                    catch (Exception innerEx)
                    {
                        if (_engine != null)
                        {
                            _engine.Log("ERROR", "DevTools server failed to start: " + innerEx.Message);
                        }
                        return;
                    }
                }

                _isRunning = true;

                if (_engine != null)
                {
                    _engine.Log("INFO", "DevTools server started on port " + _port.ToString());
                }

                while (_isRunning)
                {
                    try
                    {
                        HttpListenerContext context = _listener.GetContext();
                        ThreadPool.QueueUserWorkItem(HandleRequestCallback, context);
                    }
                    catch (HttpListenerException)
                    {
                        // Listener was stopped
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (_isRunning && _engine != null)
                        {
                            _engine.Log("ERROR", "DevTools request error: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_engine != null)
                {
                    _engine.Log("ERROR", "DevTools server thread error: " + ex.Message);
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void HandleRequestCallback(object state)
        {
            HttpListenerContext context = (HttpListenerContext)state;
            HandleRequest(context);
        }

        #endregion

        #region Request Handler

        private void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath.ToLower().TrimEnd('/');
            string method = context.Request.HttpMethod;
            string clientId = context.Request.RemoteEndPoint.ToString();

            if (OnRequest != null)
            {
                try { OnRequest.Invoke(method + " " + path); } catch { }
            }

            string responseBody = "";
            string contentType = "application/json; charset=utf-8";
            int statusCode = 200;

            try
            {
                switch (path)
                {
                    case "":
                    case "/":
                        contentType = "text/html; charset=utf-8";
                        responseBody = GetDevToolsHTML();
                        break;

                    case "/status":
                        responseBody = HandleStatus();
                        break;

                    case "/state":
                        responseBody = HandleState();
                        break;

                    case "/controls":
                        responseBody = HandleControls();
                        break;

                    case "/handlers":
                        responseBody = HandleHandlers();
                        break;

                    case "/logs":
                        responseBody = HandleLogs(context.Request);
                        break;

                    case "/clicks":
                        responseBody = HandleClicks();
                        break;

                    case "/switches":
                        if (method == "POST")
                        {
                            responseBody = HandleSetSwitch(context.Request);
                        }
                        else
                        {
                            responseBody = HandleSwitches();
                        }
                        break;

                    case "/reload":
                        responseBody = HandleReload();
                        break;

                    case "/plugins":
                        responseBody = HandlePlugins();
                        break;

                    case "/cache":
                        responseBody = HandleCache();
                        break;

                    case "/bindings":
                        responseBody = HandleBindings();
                        break;

                    case "/timers":
                        responseBody = HandleTimers();
                        break;

                    case "/exec":
                        if (method == "POST")
                        {
                            responseBody = HandleExec(context.Request);
                        }
                        else
                        {
                            responseBody = "{\"error\":\"Use POST method\"}";
                            statusCode = 405;
                        }
                        break;

                    case "/set":
                        if (method == "POST")
                        {
                            responseBody = HandleSet(context.Request);
                        }
                        else
                        {
                            responseBody = "{\"error\":\"Use POST method\"}";
                            statusCode = 405;
                        }
                        break;

                    case "/get":
                        responseBody = HandleGet(context.Request);
                        break;

                    case "/cmd":
                        if (method == "POST")
                        {
                            responseBody = HandleCmd(context.Request);
                        }
                        else
                        {
                            responseBody = "{\"error\":\"Use POST method\"}";
                            statusCode = 405;
                        }
                        break;

                    case "/connect":
                        responseBody = HandleConnect(clientId);
                        break;

                    case "/disconnect":
                        responseBody = HandleDisconnect(clientId);
                        break;

                    case "/poll":
                        responseBody = HandlePoll();
                        break;

                    default:
                        responseBody = "{\"error\":\"Not found: " + EscapeJson(path) + "\"}";
                        statusCode = 404;
                        break;
                }
            }
            catch (Exception ex)
            {
                responseBody = "{\"error\":\"" + EscapeJson(ex.Message) + "\"}";
                statusCode = 500;
            }

            // Send response
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = contentType;
                context.Response.ContentLength64 = buffer.Length;

                // CORS headers
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Close();

                if (OnResponse != null)
                {
                    try { OnResponse.Invoke(statusCode.ToString() + " " + path + " (" + buffer.Length.ToString() + "B)"); }
                    catch { }
                }
            }
            catch { }
        }

        #endregion

        #region API Handlers

        private string HandleStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"status\":\"running\",");
            sb.Append("\"engine\":\"Uvel Engine v" + _engine.EngineVersion + "\",");
            sb.Append("\"devMode\":" + (_engine.IsDevMode ? "true" : "false") + ",");
            sb.Append("\"os\":\"" + EscapeJson(_engine.SystemInfo.Name) + "\",");
            sb.Append("\"pid\":" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString() + ",");
            sb.Append("\"port\":" + _port.ToString() + ",");
            sb.Append("\"clients\":" + ClientCount.ToString() + ",");
            sb.Append("\"controls\":" + _engine.GetControlsList().Count.ToString() + ",");
            sb.Append("\"handlers\":" + _engine.GetHandlersList().Count.ToString() + ",");

            object pluginCount = _engine.GetVariable("_pluginCount");
            sb.Append("\"plugins\":" + (pluginCount != null ? pluginCount.ToString() : "0"));

            sb.Append("}");
            return sb.ToString();
        }

        private string HandleState()
        {
            Dictionary<string, object> state = _engine.GetStateSnapshot();

            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            int i = 0;
            foreach (KeyValuePair<string, object> kvp in state)
            {
                if (i > 0) sb.Append(",");
                string val = kvp.Value != null ? EscapeJson(kvp.Value.ToString()) : "null";
                sb.Append("\"" + EscapeJson(kvp.Key) + "\":\"" + val + "\"");
                i++;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string HandleControls()
        {
            List<string> controls = _engine.GetControlsList();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"controls\":[");

            for (int i = 0; i < controls.Count; i++)
            {
                if (i > 0) sb.Append(",");

                string typeName = "Unknown";
                FrameworkElement ctrl = _engine.GetControl(controls[i]);
                if (ctrl != null)
                {
                    typeName = ctrl.GetType().Name;
                }

                sb.Append("{\"name\":\"" + EscapeJson(controls[i]) + "\",\"type\":\"" + typeName + "\"}");
            }

            sb.Append("],\"count\":" + controls.Count.ToString() + "}");
            return sb.ToString();
        }

        private string HandleHandlers()
        {
            List<string> handlers = _engine.GetHandlersList();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"handlers\":[");

            for (int i = 0; i < handlers.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("\"" + EscapeJson(handlers[i]) + "\"");
            }

            sb.Append("],\"count\":" + handlers.Count.ToString() + "}");
            return sb.ToString();
        }

        private string HandleLogs(HttpListenerRequest request)
        {
            int count = 50;
            string countParam = request.QueryString["count"];
            if (!string.IsNullOrEmpty(countParam))
            {
                int.TryParse(countParam, out count);
            }

            string levelFilter = request.QueryString["level"];

            List<DevLogEntry> logs;
            lock (_engine.DevLogs)
            {
                logs = new List<DevLogEntry>(_engine.DevLogs);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"logs\":[");

            int start = Math.Max(0, logs.Count - count);
            int written = 0;

            for (int i = start; i < logs.Count; i++)
            {
                DevLogEntry log = logs[i];

                // Filter by level if specified
                if (!string.IsNullOrEmpty(levelFilter) &&
                    log.Level.ToLower() != levelFilter.ToLower())
                {
                    continue;
                }

                if (written > 0) sb.Append(",");

                sb.Append("{\"time\":\"" + log.Timestamp.ToString("HH:mm:ss.fff") + "\",");
                sb.Append("\"level\":\"" + EscapeJson(log.Level) + "\",");
                sb.Append("\"msg\":\"" + EscapeJson(log.Message) + "\"}");
                written++;
            }

            sb.Append("],\"total\":" + logs.Count.ToString() + ",\"returned\":" + written.ToString() + "}");
            return sb.ToString();
        }

        private string HandleClicks()
        {
            List<ClickEvent> clicks;
            lock (_engine.ClickHistory)
            {
                clicks = new List<ClickEvent>(_engine.ClickHistory);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"clicks\":[");

            int start = Math.Max(0, clicks.Count - 50);
            int written = 0;

            for (int i = start; i < clicks.Count; i++)
            {
                if (written > 0) sb.Append(",");

                ClickEvent click = clicks[i];
                sb.Append("{\"time\":\"" + click.Timestamp.ToString("HH:mm:ss") + "\",");
                sb.Append("\"name\":\"" + EscapeJson(click.ControlName) + "\",");
                sb.Append("\"type\":\"" + EscapeJson(click.ControlType) + "\"}");
                written++;
            }

            sb.Append("],\"total\":" + clicks.Count.ToString() + "}");
            return sb.ToString();
        }

        private string HandleSwitches()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            int i = 0;
            foreach (KeyValuePair<string, bool> kvp in _engine.DebugSwitches)
            {
                if (i > 0) sb.Append(",");
                sb.Append("\"" + kvp.Key + "\":" + (kvp.Value ? "true" : "false"));
                i++;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string HandleSetSwitch(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            if (string.IsNullOrEmpty(body))
            {
                return "{\"error\":\"Empty body\"}";
            }

            // Simple parse: name=value
            string[] parts = body.Split('=');
            if (parts.Length == 2)
            {
                string name = parts[0].Trim();
                string value = parts[1].Trim().ToLower();

                if (_engine.DebugSwitches.ContainsKey(name))
                {
                    _engine.DebugSwitches[name] = (value == "true" || value == "on" || value == "1");
                    return "{\"result\":\"ok\",\"" + name + "\":" + (_engine.DebugSwitches[name] ? "true" : "false") + "}";
                }
                else
                {
                    return "{\"error\":\"Switch not found: " + EscapeJson(name) + "\"}";
                }
            }

            return "{\"error\":\"Invalid format. Use: name=value\"}";
        }

        private string HandleReload()
        {
            try
            {
                if (Application.Current != null)
                {
                    bool success = false;
                    Exception reloadError = null;

                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        try
                        {
                            _engine.ReloadUI();
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            reloadError = ex;
                        }
                    }));

                    if (success)
                    {
                        return "{\"result\":\"ok\",\"message\":\"Hot reload successful\"}";
                    }
                    else
                    {
                        string errorMsg = reloadError != null ? reloadError.Message : "Unknown error";
                        return "{\"result\":\"error\",\"message\":\"" + EscapeJson(errorMsg) + "\"}";
                    }
                }
                else
                {
                    return "{\"result\":\"error\",\"message\":\"Application not running\"}";
                }
            }
            catch (Exception ex)
            {
                return "{\"result\":\"error\",\"message\":\"" + EscapeJson(ex.Message) + "\"}";
            }
        }

        private string HandlePlugins()
        {
            List<PluginInfo> plugins = _engine.GetLoadedPlugins();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"plugins\":[");

            for (int i = 0; i < plugins.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{\"name\":\"" + EscapeJson(plugins[i].Name) + "\",");
                sb.Append("\"version\":\"" + EscapeJson(plugins[i].Version) + "\",");
                sb.Append("\"description\":\"" + EscapeJson(plugins[i].Description) + "\"}");
            }

            sb.Append("],\"count\":" + plugins.Count.ToString() + "}");
            return sb.ToString();
        }

        private string HandleCache()
        {
            CacheStats stats = _engine.GetCacheStats();

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"entries\":" + stats.TotalEntries.ToString() + ",");
            sb.Append("\"hits\":" + stats.TotalHits.ToString());
            sb.Append("}");

            return sb.ToString();
        }

        private string HandleBindings()
        {
            Dictionary<string, List<BindingInfo>> bindings = _engine.GetAllBindings();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"bindings\":[");

            int written = 0;
            foreach (KeyValuePair<string, List<BindingInfo>> kvp in bindings)
            {
                foreach (BindingInfo binding in kvp.Value)
                {
                    if (written > 0) sb.Append(",");
                    sb.Append("{\"source\":\"" + EscapeJson(binding.StateKey) + "\",");
                    sb.Append("\"target\":\"" + EscapeJson(binding.ControlName) + "\",");
                    sb.Append("\"property\":\"" + EscapeJson(binding.Property) + "\",");
                    sb.Append("\"active\":" + (binding.IsActive ? "true" : "false") + "}");
                    written++;
                }
            }

            sb.Append("],\"count\":" + written.ToString() + "}");
            return sb.ToString();
        }

        private string HandleTimers()
        {
            // Timer names are stored in state with _timer_ prefix
            Dictionary<string, object> state = _engine.GetStateSnapshot();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"timers\":[");

            int written = 0;
            foreach (KeyValuePair<string, object> kvp in state)
            {
                if (kvp.Key.StartsWith("_timer_"))
                {
                    if (written > 0) sb.Append(",");
                    string timerName = kvp.Key.Substring(7);
                    sb.Append("\"" + EscapeJson(timerName) + "\"");
                    written++;
                }
            }

            sb.Append("],\"count\":" + written.ToString() + "}");
            return sb.ToString();
        }

        private string HandleExec(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            if (string.IsNullOrEmpty(body))
            {
                return "{\"error\":\"Empty body. Send handler name.\"}";
            }

            string handlerName = body.Trim();

            try
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        _engine.ExecuteHandler(handlerName);
                    }));

                    return "{\"result\":\"ok\",\"handler\":\"" + EscapeJson(handlerName) + "\"}";
                }

                return "{\"error\":\"Application not running\"}";
            }
            catch (Exception ex)
            {
                return "{\"error\":\"" + EscapeJson(ex.Message) + "\"}";
            }
        }

        private string HandleSet(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            if (string.IsNullOrEmpty(body))
            {
                return "{\"error\":\"Empty body. Format: name=value\"}";
            }

            int eqIndex = body.IndexOf('=');
            if (eqIndex > 0)
            {
                string name = body.Substring(0, eqIndex).Trim();
                string value = body.Substring(eqIndex + 1).Trim();

                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        _engine.SetVariable(name, value);
                    }));
                }
                else
                {
                    _engine.SetVariable(name, value);
                }

                return "{\"result\":\"ok\",\"" + EscapeJson(name) + "\":\"" + EscapeJson(value) + "\"}";
            }

            return "{\"error\":\"Invalid format. Use: name=value\"}";
        }

        private string HandleGet(HttpListenerRequest request)
        {
            string name = request.QueryString["name"];
            if (string.IsNullOrEmpty(name))
            {
                name = request.QueryString["key"];
            }

            if (string.IsNullOrEmpty(name))
            {
                return "{\"error\":\"Specify ?name=varName\"}";
            }

            object value = _engine.GetVariable(name);
            string strValue = value != null ? EscapeJson(value.ToString()) : "null";

            return "{\"name\":\"" + EscapeJson(name) + "\",\"value\":\"" + strValue + "\",\"exists\":" + (value != null ? "true" : "false") + "}";
        }

        private string HandleCmd(HttpListenerRequest request)
        {
            string body = ReadRequestBody(request);
            if (string.IsNullOrEmpty(body))
            {
                return "{\"error\":\"Empty body. Send command.\"}";
            }

            string command = body.Trim();
            string output = _engine.RunCmd(command);

            return "{\"command\":\"" + EscapeJson(command) + "\",\"output\":\"" + EscapeJson(output) + "\"}";
        }

        private string HandleConnect(string clientId)
        {
            lock (_clientLock)
            {
                if (!_connectedClients.Contains(clientId))
                {
                    _connectedClients.Add(clientId);
                    _totalConnections++;
                }
            }

            if (OnClientConnected != null)
            {
                try { OnClientConnected.Invoke(clientId); } catch { }
            }

            return "{\"result\":\"connected\",\"clientId\":\"" + EscapeJson(clientId) + "\"}";
        }

        private string HandleDisconnect(string clientId)
        {
            lock (_clientLock)
            {
                _connectedClients.Remove(clientId);
            }

            if (OnClientDisconnected != null)
            {
                try { OnClientDisconnected.Invoke(clientId); } catch { }
            }

            return "{\"result\":\"disconnected\"}";
        }

        private string HandlePoll()
        {
            // Return latest state for polling clients
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            // Last 5 logs
            sb.Append("\"recentLogs\":[");
            List<DevLogEntry> logs;
            lock (_engine.DevLogs)
            {
                logs = new List<DevLogEntry>(_engine.DevLogs);
            }

            int logStart = Math.Max(0, logs.Count - 5);
            for (int i = logStart; i < logs.Count; i++)
            {
                if (i > logStart) sb.Append(",");
                sb.Append("{\"time\":\"" + logs[i].Timestamp.ToString("HH:mm:ss") + "\",");
                sb.Append("\"level\":\"" + EscapeJson(logs[i].Level) + "\",");
                sb.Append("\"msg\":\"" + EscapeJson(logs[i].Message) + "\"}");
            }
            sb.Append("],");

            // Control count
            sb.Append("\"controlCount\":" + _engine.GetControlsList().Count.ToString() + ",");

            // Handler count
            sb.Append("\"handlerCount\":" + _engine.GetHandlersList().Count.ToString() + ",");

            // Time
            sb.Append("\"serverTime\":\"" + DateTime.Now.ToString("HH:mm:ss") + "\"");

            sb.Append("}");
            return sb.ToString();
        }

        #endregion

        #region Broadcast Methods (for Program.cs compatibility)

        public void BroadcastLog(DevLogEntry entry)
        {
            // In HTTP-based server, logs are available via /logs endpoint
            // No active push needed - clients poll
        }

        public void BroadcastReload()
        {
            // Reload event - clients can detect via /poll
        }

        public void BroadcastClick(ClickEvent click)
        {
            // Click events available via /clicks endpoint
        }

        #endregion

        #region DevTools HTML UI

        private string GetDevToolsHTML()
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"UTF-8\">");
            html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("<title>Uvel DevTools</title>");
            html.AppendLine("<style>");
            html.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            html.AppendLine("body { background: #1e1e1e; color: #d4d4d4; font-family: 'Segoe UI', Consolas, monospace; }");
            html.AppendLine(".header { background: #007acc; padding: 12px 20px; display: flex; align-items: center; justify-content: space-between; }");
            html.AppendLine(".header h1 { color: white; font-size: 18px; font-weight: 600; }");
            html.AppendLine(".header .status { color: #90ee90; font-size: 12px; }");
            html.AppendLine(".container { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; padding: 10px; }");
            html.AppendLine(".panel { background: #252526; border: 1px solid #3c3c3c; border-radius: 6px; overflow: hidden; }");
            html.AppendLine(".panel-header { background: #2d2d30; padding: 8px 12px; border-bottom: 1px solid #3c3c3c; display: flex; justify-content: space-between; align-items: center; }");
            html.AppendLine(".panel-header h3 { font-size: 13px; color: #cccccc; }");
            html.AppendLine(".panel-body { padding: 10px; max-height: 300px; overflow-y: auto; font-size: 12px; }");
            html.AppendLine("pre { white-space: pre-wrap; word-break: break-all; }");
            html.AppendLine(".log-entry { padding: 2px 0; border-bottom: 1px solid #2a2a2a; }");
            html.AppendLine(".log-time { color: #666; }");
            html.AppendLine(".log-ERROR { color: #f44747; }");
            html.AppendLine(".log-WARN { color: #cca700; }");
            html.AppendLine(".log-INFO { color: #3dc9b0; }");
            html.AppendLine(".log-DEBUG { color: #888; }");
            html.AppendLine(".log-HANDLER { color: #c586c0; }");
            html.AppendLine(".log-CLICK { color: #dcdcaa; }");
            html.AppendLine(".log-PLUGIN { color: #4ec9b0; }");
            html.AppendLine(".log-RELOAD { color: #90ee90; }");
            html.AppendLine(".btn { background: #0e639c; color: white; border: none; padding: 6px 14px; border-radius: 3px; cursor: pointer; font-size: 12px; margin: 3px; }");
            html.AppendLine(".btn:hover { background: #1177bb; }");
            html.AppendLine(".btn-danger { background: #a1260d; }");
            html.AppendLine(".btn-danger:hover { background: #c4260d; }");
            html.AppendLine(".btn-success { background: #16825d; }");
            html.AppendLine(".input { background: #3c3c3c; border: 1px solid #555; color: #d4d4d4; padding: 5px 8px; border-radius: 3px; font-size: 12px; width: 100%; }");
            html.AppendLine(".toolbar { padding: 8px 10px; background: #2d2d30; border-bottom: 1px solid #3c3c3c; display: flex; gap: 5px; flex-wrap: wrap; }");
            html.AppendLine(".kv-row { display: flex; padding: 3px 0; border-bottom: 1px solid #2a2a2a; }");
            html.AppendLine(".kv-key { color: #9cdcfe; min-width: 120px; }");
            html.AppendLine(".kv-val { color: #ce9178; }");
            html.AppendLine(".full-width { grid-column: 1 / -1; }");
            html.AppendLine("::-webkit-scrollbar { width: 8px; }");
            html.AppendLine("::-webkit-scrollbar-track { background: #1e1e1e; }");
            html.AppendLine("::-webkit-scrollbar-thumb { background: #555; border-radius: 4px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine("<div class=\"header\">");
            html.AppendLine("  <h1>Uvel DevTools</h1>");
            html.AppendLine("  <span class=\"status\" id=\"statusText\">Connecting...</span>");
            html.AppendLine("</div>");

            // Toolbar
            html.AppendLine("<div class=\"toolbar\">");
            html.AppendLine("  <button class=\"btn\" onclick=\"loadAll()\">Refresh All</button>");
            html.AppendLine("  <button class=\"btn btn-success\" onclick=\"doReload()\">Hot Reload</button>");
            html.AppendLine("  <button class=\"btn btn-danger\" onclick=\"clearLogs()\">Clear Logs</button>");
            html.AppendLine("  <input class=\"input\" id=\"cmdInput\" placeholder=\"Enter command (set var=val | exec handler | cmd dir)\" style=\"width:300px\" onkeydown=\"if(event.key==='Enter')runCmd()\">");
            html.AppendLine("  <button class=\"btn\" onclick=\"runCmd()\">Run</button>");
            html.AppendLine("</div>");

            // Panels
            html.AppendLine("<div class=\"container\">");

            // Status
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>Status</h3></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"statusPanel\">Loading...</div>");
            html.AppendLine("</div>");

            // State Variables
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>State Variables</h3><button class=\"btn\" onclick=\"loadState()\">Refresh</button></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"statePanel\">Loading...</div>");
            html.AppendLine("</div>");

            // Controls
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>Controls</h3></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"controlsPanel\">Loading...</div>");
            html.AppendLine("</div>");

            // Handlers
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>Handlers</h3></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"handlersPanel\">Loading...</div>");
            html.AppendLine("</div>");

            // Plugins
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>Plugins</h3></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"pluginsPanel\">Loading...</div>");
            html.AppendLine("</div>");

            // Switches
            html.AppendLine("<div class=\"panel\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>Debug Switches</h3></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"switchesPanel\">Loading...</div>");
            html.AppendLine("</div>");

            // Logs (full width)
            html.AppendLine("<div class=\"panel full-width\">");
            html.AppendLine("  <div class=\"panel-header\"><h3>Logs</h3><button class=\"btn\" onclick=\"loadLogs()\">Refresh</button></div>");
            html.AppendLine("  <div class=\"panel-body\" id=\"logsPanel\" style=\"max-height:400px\">Loading...</div>");
            html.AppendLine("</div>");

            html.AppendLine("</div>"); // container

            // JavaScript
            html.AppendLine("<script>");
            html.AppendLine("function api(path, opts) {");
            html.AppendLine("  return fetch(path, opts).then(function(r) { return r.json(); }).catch(function(e) { return {error: e.message}; });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadAll() {");
            html.AppendLine("  loadStatus(); loadState(); loadControls(); loadHandlers(); loadLogs(); loadPlugins(); loadSwitches();");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadStatus() {");
            html.AppendLine("  api('/status').then(function(d) {");
            html.AppendLine("    if (d.error) { document.getElementById('statusText').textContent = 'Error'; return; }");
            html.AppendLine("    document.getElementById('statusText').textContent = 'Connected - ' + d.engine;");
            html.AppendLine("    var h = '';");
            html.AppendLine("    for (var k in d) { h += '<div class=\"kv-row\"><span class=\"kv-key\">' + k + '</span><span class=\"kv-val\">' + d[k] + '</span></div>'; }");
            html.AppendLine("    document.getElementById('statusPanel').innerHTML = h;");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadState() {");
            html.AppendLine("  api('/state').then(function(d) {");
            html.AppendLine("    var h = '';");
            html.AppendLine("    for (var k in d) { h += '<div class=\"kv-row\"><span class=\"kv-key\">' + k + '</span><span class=\"kv-val\">' + d[k] + '</span></div>'; }");
            html.AppendLine("    document.getElementById('statePanel').innerHTML = h || '(empty)';");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadControls() {");
            html.AppendLine("  api('/controls').then(function(d) {");
            html.AppendLine("    if (!d.controls) return;");
            html.AppendLine("    var h = '';");
            html.AppendLine("    d.controls.forEach(function(c) { h += '<div class=\"kv-row\"><span class=\"kv-key\">' + c.name + '</span><span class=\"kv-val\">' + c.type + '</span></div>'; });");
            html.AppendLine("    h += '<div style=\"margin-top:5px;color:#888\">Total: ' + d.count + '</div>';");
            html.AppendLine("    document.getElementById('controlsPanel').innerHTML = h || '(none)';");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadHandlers() {");
            html.AppendLine("  api('/handlers').then(function(d) {");
            html.AppendLine("    if (!d.handlers) return;");
            html.AppendLine("    var h = '';");
            html.AppendLine("    d.handlers.forEach(function(name) {");
            html.AppendLine("      h += '<div class=\"kv-row\"><span class=\"kv-key\">' + name + '</span><button class=\"btn\" style=\"padding:2px 8px;font-size:11px\" onclick=\"execHandler(\\'' + name + '\\')\">Run</button></div>';");
            html.AppendLine("    });");
            html.AppendLine("    document.getElementById('handlersPanel').innerHTML = h || '(none)';");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadLogs() {");
            html.AppendLine("  api('/logs?count=100').then(function(d) {");
            html.AppendLine("    if (!d.logs) return;");
            html.AppendLine("    var h = '';");
            html.AppendLine("    d.logs.forEach(function(log) {");
            html.AppendLine("      h += '<div class=\"log-entry\"><span class=\"log-time\">' + log.time + '</span> <span class=\"log-' + log.level + '\">[' + log.level + ']</span> ' + log.msg + '</div>';");
            html.AppendLine("    });");
            html.AppendLine("    var el = document.getElementById('logsPanel');");
            html.AppendLine("    el.innerHTML = h || '(no logs)';");
            html.AppendLine("    el.scrollTop = el.scrollHeight;");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadPlugins() {");
            html.AppendLine("  api('/plugins').then(function(d) {");
            html.AppendLine("    if (!d.plugins) return;");
            html.AppendLine("    var h = '';");
            html.AppendLine("    d.plugins.forEach(function(p) { h += '<div class=\"kv-row\"><span class=\"kv-key\">' + p.name + ' v' + p.version + '</span><span class=\"kv-val\">' + p.description + '</span></div>'; });");
            html.AppendLine("    h += '<div style=\"margin-top:5px;color:#888\">Total: ' + d.count + '</div>';");
            html.AppendLine("    document.getElementById('pluginsPanel').innerHTML = h || '(no plugins)';");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function loadSwitches() {");
            html.AppendLine("  api('/switches').then(function(d) {");
            html.AppendLine("    var h = '';");
            html.AppendLine("    for (var k in d) {");
            html.AppendLine("      var color = d[k] ? '#90ee90' : '#f44747';");
            html.AppendLine("      var label = d[k] ? 'ON' : 'OFF';");
            html.AppendLine("      h += '<div class=\"kv-row\"><span class=\"kv-key\">' + k + '</span><span style=\"color:' + color + ';cursor:pointer\" onclick=\"toggleSwitch(\\'' + k + '\\',' + (!d[k]) + ')\">' + label + '</span></div>';");
            html.AppendLine("    }");
            html.AppendLine("    document.getElementById('switchesPanel').innerHTML = h;");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function doReload() {");
            html.AppendLine("  api('/reload').then(function(d) { alert(d.message || d.error); loadAll(); });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function execHandler(name) {");
            html.AppendLine("  api('/exec', {method:'POST', body: name}).then(function(d) {");
            html.AppendLine("    if (d.error) alert('Error: ' + d.error);");
            html.AppendLine("    setTimeout(loadAll, 500);");
            html.AppendLine("  });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function toggleSwitch(name, val) {");
            html.AppendLine("  api('/switches', {method:'POST', body: name + '=' + val}).then(function() { loadSwitches(); });");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("function clearLogs() { document.getElementById('logsPanel').innerHTML = '(cleared)'; }");
            html.AppendLine("");
            html.AppendLine("function runCmd() {");
            html.AppendLine("  var input = document.getElementById('cmdInput');");
            html.AppendLine("  var cmd = input.value.trim();");
            html.AppendLine("  if (!cmd) return;");
            html.AppendLine("  input.value = '';");
            html.AppendLine("");
            html.AppendLine("  if (cmd.indexOf('set ') === 0) {");
            html.AppendLine("    var parts = cmd.substring(4);");
            html.AppendLine("    api('/set', {method:'POST', body: parts}).then(function() { loadState(); });");
            html.AppendLine("  } else if (cmd.indexOf('exec ') === 0) {");
            html.AppendLine("    var name = cmd.substring(5);");
            html.AppendLine("    execHandler(name);");
            html.AppendLine("  } else if (cmd.indexOf('cmd ') === 0) {");
            html.AppendLine("    var c = cmd.substring(4);");
            html.AppendLine("    api('/cmd', {method:'POST', body: c}).then(function(d) { alert(d.output || d.error); });");
            html.AppendLine("  } else {");
            html.AppendLine("    alert('Commands: set key=val | exec handlerName | cmd <shell command>');");
            html.AppendLine("  }");
            html.AppendLine("}");
            html.AppendLine("");
            html.AppendLine("// Auto refresh");
            html.AppendLine("loadAll();");
            html.AppendLine("setInterval(function() { loadLogs(); loadState(); }, 3000);");
            html.AppendLine("setInterval(loadStatus, 10000);");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        #endregion

        #region Helpers

        private string ReadRequestBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody) return "";

            try
            {
                using (StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return "";
            }
        }

        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        #endregion
    }
}