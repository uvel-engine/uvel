using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Net;
using Uvel.WPF;
using System.Xml;
namespace Uvel
{
    /// <summary>
    /// Uvel CLI v3.0 - Command Line Interface with DevTools
    /// Compatible with .NET Framework 4.0+ (no $ strings, no async/await)
    /// </summary>
    public class Program
    {
        private static WpfEngine _engine;
        private static DevToolsServer _devServer;
        private static Thread _appThread;
        private static bool _isRunning = true;
        private static int _requestCount = 0;
        private static int _wsClientCount = 0;

        [STAThread]
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Uvel Framework v3.0";

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "run":
                    RunCommand(args);
                    break;
                case "dev":
                    DevCommand(args);
                    break;
                case "build":
                    BuildCommand(args);
                    break;
                case "bridge":
                    BridgeCommand(args);
                    break;
                case "install-protocol":
                    InstallProtocolCommand();
                    break;
                case "new":
                    NewCommand(args);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                case "--version":
                case "-v":
                    Console.WriteLine("Uvel Framework v3.0.0");
                    break;
                default:
                    if (command.EndsWith(".xml") && File.Exists(command))
                    {
                        RunApp(command, false);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown command: " + command);
                        Console.ResetColor();
                        ShowHelp();
                    }
                    break;
            }
        }

        #region Commands

        private static void RunCommand(string[] args)
        {
            string xmlPath = GetXmlPath(args, 1);
            if (string.IsNullOrEmpty(xmlPath)) return;

            SafeConsoleClear();
            PrintMiniHeader();
            Console.WriteLine("  Running: " + Path.GetFileName(xmlPath));
            Console.WriteLine();

            RunApp(xmlPath, false);
        }

        private static void DevCommand(string[] args)
        {
            string xmlPath = GetXmlPath(args, 1);
            if (string.IsNullOrEmpty(xmlPath)) return;

            int port = 9222;
            bool showDebugPanel = true;

            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if ((arg == "-p" || arg == "--port") && i + 1 < args.Length)
                {
                    int.TryParse(args[++i], out port);
                }
                else if (arg == "--no-debug")
                {
                    showDebugPanel = false;
                }
            }

            SafeConsoleClear();
            PrintDevBanner(xmlPath, port);

            // Create engine
            _engine = new WpfEngine();
            _engine.IsDevMode = true;

            // Subscribe to engine events
            _engine.OnLogMessage += OnEngineLog;
            _engine.OnHotReload += OnHotReload;
            _engine.OnControlClick += OnControlClick;

            // Start DevTools HTTP Server
            _devServer = new DevToolsServer(_engine, port);
            _devServer.OnRequest += OnServerRequest;
            _devServer.OnResponse += OnServerResponse;
            _devServer.OnClientConnected += OnWsClientConnected;
            _devServer.OnClientDisconnected += OnWsClientDisconnected;

            _devServer.Start();

            // Run app in separate STA thread
            _appThread = new Thread(delegate()
            {
                try
                {
                    _engine.Run(xmlPath, false);
                }
                catch (Exception ex)
                {
                    PrintError("App crashed: " + ex.Message);
                }
                finally
                {
                    _isRunning = false;
                }
            });

            _appThread.SetApartmentState(ApartmentState.STA);
            _appThread.Start();

            // Print server info
            Console.WriteLine();
            PrintServerInfo(port);
            Console.WriteLine();

            // Console command loop (DevTools)
            if (showDebugPanel)
            {
                RunDevConsole();
            }
            else
            {
                Console.WriteLine("  Press Ctrl+C to stop...");
                while (_isRunning && _appThread.IsAlive)
                {
                    Thread.Sleep(100);
                }
            }

            // Cleanup
            if (_devServer != null)
            {
                _devServer.Stop();
            }
            Console.WriteLine();
            PrintInfo("Session ended.");
        }


        private static void BridgeCommand(string[] args)
        {
            int port = 9327;
            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if ((arg == "-p" || arg == "--port") && i + 1 < args.Length)
                {
                    int.TryParse(args[++i], out port);
                }
            }

            SafeConsoleClear();
            PrintMiniHeader();
            PrintInfo("Starting Uvel Bridge...");
            PrintInfo("Local WebSocket: ws://127.0.0.1:" + port + "/workspace");
            PrintInfo("Press Ctrl+C to stop.");
            Console.WriteLine();

            UvelBridgeServer bridge = new UvelBridgeServer(port);
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                bridge.Stop();
                Environment.Exit(0);
            };
            bridge.Start();
        }

        private static void InstallProtocolCommand()
        {
            try
            {
                string exe = Process.GetCurrentProcess().MainModule.FileName;
                string command = "\"" + exe + "\" bridge";
                Microsoft.Win32.RegistryKey root = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Classes\\uvel");
                root.SetValue("", "URL:Uvel Workspace Protocol");
                root.SetValue("URL Protocol", "");
                Microsoft.Win32.RegistryKey icon = root.CreateSubKey("DefaultIcon");
                icon.SetValue("", exe + ",1");
                Microsoft.Win32.RegistryKey cmd = root.CreateSubKey("shell\\open\\command");
                cmd.SetValue("", command);
                cmd.Close();
                icon.Close();
                root.Close();
                PrintInfo("Installed uvel:// protocol for Uvel Workspace.");
                PrintInfo("Command: " + command);
            }
            catch (Exception ex)
            {
                PrintError("Protocol install failed: " + ex.Message);
            }
        }

        private static void BuildCommand(string[] args)
        {
            string xmlPath = GetXmlPath(args, 1);
            if (string.IsNullOrEmpty(xmlPath)) return;

            string outputDir = "./build";
            string appName = Path.GetFileNameWithoutExtension(xmlPath);
            string iconPath = "";
            bool showConsole = false;
            bool compress = false;

            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                if ((arg == "-o" || arg == "--output") && i + 1 < args.Length)
                    outputDir = args[++i];
                else if ((arg == "-n" || arg == "--name") && i + 1 < args.Length)
                    appName = args[++i];
                else if ((arg == "-i" || arg == "--icon") && i + 1 < args.Length)
                    iconPath = args[++i];
                else if (arg == "--console")
                    showConsole = true;
                else if (arg == "--compress")
                    compress = true;
            }

            UvelBuildSystem builder = new UvelBuildSystem();
            builder.XmlPath = xmlPath;
            builder.OutputDirectory = outputDir;
            builder.AppName = appName;
            builder.IconPath = iconPath;
            builder.ShowConsole = showConsole;
            builder.Compress = compress;

            builder.Build();
        }

        private static void NewCommand(string[] args)
        {
            string projectName = "MyUvelApp";
            string template = "default";

            if (args.Length > 1) projectName = args[1];
            if (args.Length > 2) template = args[2];

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Creating New Uvel Project");
            Console.ResetColor();

            string projectDir = Path.Combine(Environment.CurrentDirectory, projectName);

            if (Directory.Exists(projectDir))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Directory already exists: " + projectName);
                Console.ResetColor();
            }
            else
            {
                Directory.CreateDirectory(projectDir);
            }

            // Create plugins folder
            string pluginsDir = Path.Combine(projectDir, "plugins");
            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }

            string appXml = CreateTemplateApp(projectName, template);
            File.WriteAllText(Path.Combine(projectDir, "App.xml"), appXml, Encoding.UTF8);

            // Create uvel.json config
            StringBuilder config = new StringBuilder();
            config.AppendLine("{");
            config.AppendLine("  \"name\": \"" + projectName + "\",");
            config.AppendLine("  \"version\": \"1.0.0\",");
            config.AppendLine("  \"entry\": \"App.xml\",");
            config.AppendLine("  \"build\": {");
            config.AppendLine("    \"output\": \"./dist\",");
            config.AppendLine("    \"icon\": \"\",");
            config.AppendLine("    \"console\": false");
            config.AppendLine("  }");
            config.AppendLine("}");
            File.WriteAllText(Path.Combine(projectDir, "uvel.json"), config.ToString(), Encoding.UTF8);

            // Create sample plugin
            StringBuilder pluginCode = new StringBuilder();
            pluginCode.AppendLine("using System;");
            pluginCode.AppendLine("using System.Xml;");
            pluginCode.AppendLine("using Uvel.WPF;");
            pluginCode.AppendLine("");
            pluginCode.AppendLine("/// <summary>");
            pluginCode.AppendLine("/// Sample Uvel Plugin");
            pluginCode.AppendLine("/// Place .cs files in the plugins/ folder");
            pluginCode.AppendLine("/// They will be compiled and loaded automatically");
            pluginCode.AppendLine("/// </summary>");
            pluginCode.AppendLine("public class SamplePlugin : IUvelPlugin");
            pluginCode.AppendLine("{");
            pluginCode.AppendLine("    public string Name { get { return \"SamplePlugin\"; } }");
            pluginCode.AppendLine("    public string Version { get { return \"1.0\"; } }");
            pluginCode.AppendLine("    public string Description { get { return \"A sample plugin\"; } }");
            pluginCode.AppendLine("");
            pluginCode.AppendLine("    public void OnLoad(WpfEngine engine)");
            pluginCode.AppendLine("    {");
            pluginCode.AppendLine("        engine.Log(\"PLUGIN\", \"SamplePlugin loaded!\");");
            pluginCode.AppendLine("    }");
            pluginCode.AppendLine("");
            pluginCode.AppendLine("    public void OnUnload(WpfEngine engine) { }");
            pluginCode.AppendLine("    public void OnEvent(WpfEngine engine, string eventName, object data) { }");
            pluginCode.AppendLine("}");
            File.WriteAllText(Path.Combine(pluginsDir, "SamplePlugin.cs"), pluginCode.ToString(), Encoding.UTF8);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Project created: " + projectName);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Structure:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    " + projectName + "/");
            Console.WriteLine("      App.xml");
            Console.WriteLine("      uvel.json");
            Console.WriteLine("      plugins/");
            Console.WriteLine("        SamplePlugin.cs");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Next steps:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    cd " + projectName);
            Console.WriteLine("    uvel dev App.xml");
            Console.ResetColor();
            Console.WriteLine();
        }

        #endregion

        #region Dev Console (Interactive)

        private static void RunDevConsole()
{
    PrintDevHelp();

    while (_isRunning && _appThread != null && _appThread.IsAlive)
    {
        // Faqat input tayyor bo'lganda so'rash
        if (!Console.KeyAvailable)
        {
            Thread.Sleep(200);
            continue;
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("  uvel> ");
        Console.ResetColor();

        string input = "";
        try
        {
            input = Console.ReadLine();
        }
        catch
        {
            Thread.Sleep(200);
            continue;
        }

        if (string.IsNullOrWhiteSpace(input)) continue;

        string[] parts = input.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "help":
            case "?":
                PrintDevHelp();
                break;

            case "clear":
            case "cls":
                SafeConsoleClear();
                PrintMiniHeader();
                break;

            case "state":
                PrintState();
                break;

            case "controls":
                PrintControls();
                break;

            case "handlers":
                PrintHandlers();
                break;

            case "logs":
                PrintLogs(parts.Length > 1 ? parts[1] : "20");
                break;

            case "clicks":
                PrintClicks();
                break;

            case "switches":
                if (parts.Length == 1)
                    PrintSwitches();
                else if (parts.Length >= 3)
                    SetSwitch(parts[1], parts[2]);
                break;

            case "reload":
            case "r":
                ForceReload();
                break;

            case "exec":
            case "run":
                if (parts.Length > 1)
                    ExecuteHandler(parts[1]);
                else
                    Console.WriteLine("    Usage: exec <handlerName>");
                break;

            case "set":
                if (parts.Length >= 3)
                    SetVariable(parts[1], JoinArgs(parts, 2));
                else
                    Console.WriteLine("    Usage: set <varName> <value>");
                break;

            case "get":
                if (parts.Length > 1)
                    GetVariable(parts[1]);
                else
                    Console.WriteLine("    Usage: get <varName>");
                break;

            case "cmd":
                if (parts.Length > 1)
                {
                    string cmdStr = JoinArgs(parts, 1);
                    string output = _engine.RunCmd(cmdStr);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(output);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("    Usage: cmd <command>");
                }
                break;

            case "plugins":
                PrintPlugins();
                break;

            case "cache":
                PrintCacheStats();
                break;

            case "bindings":
                PrintBindings();
                break;

            case "clients":
            case "ws":
                PrintWsClients();
                break;

            case "stats":
                PrintStats();
                break;

            case "exit":
            case "quit":
            case "q":
                _isRunning = false;
                Console.WriteLine("    Stopping...");
                break;

            default:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    Unknown command: " + cmd + " (type 'help')");
                Console.ResetColor();
                break;
        }
    }
}

        private static string JoinArgs(string[] parts, int startIndex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < parts.Length; i++)
            {
                if (i > startIndex) sb.Append(" ");
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }

        private static bool WaitForInput(int ms)
        {
            int waited = 0;
            while (waited < ms)
            {
                if (Console.KeyAvailable) return true;
                Thread.Sleep(10);
                waited += 10;
            }
            return Console.KeyAvailable;
        }

        private static void PrintDevHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  DEV TOOLS COMMANDS:");
            Console.ResetColor();
            Console.WriteLine("  state        - Show all state variables");
            Console.WriteLine("  controls     - List all registered controls");
            Console.WriteLine("  handlers     - List all event handlers");
            Console.WriteLine("  logs [n]     - Show last n log entries");
            Console.WriteLine("  clicks       - Show click history");
            Console.WriteLine("  switches     - Show/set debug switches");
            Console.WriteLine("  plugins      - Show loaded plugins");
            Console.WriteLine("  bindings     - Show data bindings");
            Console.WriteLine("  cache        - Show cache statistics");
            Console.WriteLine("  reload (r)   - Force hot reload");
            Console.WriteLine("  exec <h>     - Execute handler by name");
            Console.WriteLine("  set <k> <v>  - Set state variable");
            Console.WriteLine("  get <k>      - Get state variable");
            Console.WriteLine("  cmd <c>      - Run shell command");
            Console.WriteLine("  clients      - Show connected clients");
            Console.WriteLine("  stats        - Show server statistics");
            Console.WriteLine("  clear        - Clear console");
            Console.WriteLine("  exit (q)     - Stop dev server");
            Console.WriteLine();
        }

        private static void PrintState()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    STATE VARIABLES:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            Dictionary<string, object> state = _engine.GetStateSnapshot();
            if (state.Count == 0)
            {
                Console.WriteLine("    (empty)");
                return;
            }

            foreach (KeyValuePair<string, object> kvp in state)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("    " + kvp.Key);
                Console.ResetColor();
                Console.Write(" = ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(kvp.Value != null ? kvp.Value.ToString() : "null");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        private static void PrintControls()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    REGISTERED CONTROLS:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            List<string> controls = _engine.GetControlsList();
            if (controls.Count == 0)
            {
                Console.WriteLine("    (none)");
                return;
            }

            foreach (string name in controls)
            {
                FrameworkElement ctrl = _engine.GetControl(name);
                string typeName = ctrl != null ? ctrl.GetType().Name : "?";

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("    " + name);
                Console.ResetColor();
                Console.Write(" : ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(typeName);
                Console.ResetColor();
            }
            Console.WriteLine("    Total: " + controls.Count);
            Console.WriteLine();
        }

        private static void PrintHandlers()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    EVENT HANDLERS:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            List<string> handlers = _engine.GetHandlersList();
            if (handlers.Count == 0)
            {
                Console.WriteLine("    (none)");
                return;
            }

            foreach (string name in handlers)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("    -> " + name);
                Console.ResetColor();
            }
            Console.WriteLine("    Total: " + handlers.Count);
            Console.WriteLine();
        }

        private static void PrintLogs(string countStr)
        {
            int count = 20;
            int.TryParse(countStr, out count);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    LAST " + count.ToString() + " LOG ENTRIES:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            List<DevLogEntry> logs;
            lock (_engine.DevLogs)
            {
                logs = new List<DevLogEntry>(_engine.DevLogs);
            }

            if (logs.Count == 0)
            {
                Console.WriteLine("    (no logs)");
                return;
            }

            int start = Math.Max(0, logs.Count - count);
            for (int i = start; i < logs.Count; i++)
            {
                DevLogEntry log = logs[i];
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("    " + log.Timestamp.ToString("HH:mm:ss") + " ");

                switch (log.Level)
                {
                    case "ERROR": Console.ForegroundColor = ConsoleColor.Red; break;
                    case "WARN": Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case "INFO": Console.ForegroundColor = ConsoleColor.Cyan; break;
                    case "DEBUG": Console.ForegroundColor = ConsoleColor.DarkGray; break;
                    case "HANDLER": Console.ForegroundColor = ConsoleColor.Magenta; break;
                    case "RELOAD": Console.ForegroundColor = ConsoleColor.Green; break;
                    case "CLICK": Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                    case "PLUGIN": Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                    default: Console.ForegroundColor = ConsoleColor.White; break;
                }
                Console.Write("[" + log.Level + "] ");
                Console.ResetColor();
                Console.WriteLine(log.Message);
            }
            Console.WriteLine();
        }

        private static void PrintClicks()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    CLICK HISTORY:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            List<ClickEvent> clicks;
            lock (_engine.ClickHistory)
            {
                clicks = new List<ClickEvent>(_engine.ClickHistory);
            }

            if (clicks.Count == 0)
            {
                Console.WriteLine("    (no clicks recorded)");
                return;
            }

            int start = Math.Max(0, clicks.Count - 20);
            for (int i = start; i < clicks.Count; i++)
            {
                ClickEvent click = clicks[i];
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("    " + click.Timestamp.ToString("HH:mm:ss") + " ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(click.ControlName);
                Console.ResetColor();
                Console.WriteLine(" (" + click.ControlType + ")");
            }
            Console.WriteLine();
        }

        private static void PrintSwitches()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    DEBUG SWITCHES:");
            Console.ResetColor();

            if (_engine == null) return;

            foreach (KeyValuePair<string, bool> kvp in _engine.DebugSwitches)
            {
                Console.Write("    " + kvp.Key + ": ");
                if (kvp.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("ON");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("OFF");
                }
                Console.ResetColor();
            }
            Console.WriteLine("    Usage: switches <name> <on|off>");
            Console.WriteLine();
        }

        private static void SetSwitch(string name, string value)
        {
            if (_engine == null) return;

            bool val = value.ToLower() == "on" || value.ToLower() == "true" || value == "1";

            if (_engine.DebugSwitches.ContainsKey(name))
            {
                _engine.DebugSwitches[name] = val;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    " + name + " = " + (val ? "ON" : "OFF"));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    Switch not found: " + name);
                Console.ResetColor();
            }
        }

        private static void PrintPlugins()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    LOADED PLUGINS:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            List<PluginInfo> plugins = _engine.GetLoadedPlugins();

            if (plugins.Count == 0)
            {
                Console.WriteLine("    (no plugins)");
                return;
            }

            foreach (PluginInfo pi in plugins)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("    " + pi.Name);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" v" + pi.Version);
                Console.ResetColor();
                Console.WriteLine(" - " + pi.Description);
            }
            Console.WriteLine("    Total: " + plugins.Count);
            Console.WriteLine();
        }

        private static void PrintCacheStats()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    CACHE STATISTICS:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            CacheStats stats = _engine.GetCacheStats();
            Console.WriteLine("    Entries: " + stats.TotalEntries.ToString());
            Console.WriteLine("    Hits:    " + stats.TotalHits.ToString());
            Console.WriteLine();
        }

        private static void PrintBindings()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    DATA BINDINGS:");
            Console.ResetColor();

            if (_engine == null)
            {
                Console.WriteLine("    (engine not ready)");
                return;
            }

            Dictionary<string, List<BindingInfo>> bindings = _engine.GetAllBindings();
            int total = 0;

            foreach (KeyValuePair<string, List<BindingInfo>> kvp in bindings)
            {
                foreach (BindingInfo bi in kvp.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("    " + bi.StateKey);
                    Console.ResetColor();
                    Console.Write(" -> ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(bi.ControlName + "." + bi.Property);
                    Console.ResetColor();
                    Console.WriteLine(bi.IsActive ? " (active)" : " (inactive)");
                    total++;
                }
            }

            if (total == 0)
            {
                Console.WriteLine("    (no bindings)");
            }
            else
            {
                Console.WriteLine("    Total: " + total.ToString());
            }
            Console.WriteLine();
        }

        private static void ForceReload()
        {
            if (_engine == null) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("    Reloading... ");
            Console.ResetColor();

            try
            {
                Dictionary<string, Window> windows = _engine.Windows;
                foreach (KeyValuePair<string, Window> kvp in windows)
                {
                    kvp.Value.Dispatcher.Invoke(new Action(delegate
                    {
                        _engine.ReloadUI();
                    }));
                    break;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed: " + ex.Message);
                Console.ResetColor();
            }
        }

        private static void ExecuteHandler(string name)
        {
            if (_engine == null) return;

            List<string> handlers = _engine.GetHandlersList();
            if (handlers.Contains(name))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    Executing: " + name);
                Console.ResetColor();

                Dictionary<string, Window> windows = _engine.Windows;
                foreach (KeyValuePair<string, Window> kvp in windows)
                {
                    kvp.Value.Dispatcher.Invoke(new Action(delegate
                    {
                        _engine.ExecuteHandler(name);
                    }));
                    break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("    Handler not found: " + name);
                Console.ResetColor();
            }
        }

        private static void SetVariable(string name, string value)
        {
            if (_engine == null) return;

            _engine.SetVariable(name, value);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("    " + name + " = " + value);
            Console.ResetColor();
        }

        private static void GetVariable(string name)
        {
            if (_engine == null) return;

            object val = _engine.GetVariable(name);
            if (val != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("    " + name);
                Console.ResetColor();
                Console.Write(" = ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(val.ToString());
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("    " + name + " = (undefined)");
                Console.ResetColor();
            }
        }

        private static void PrintWsClients()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    CONNECTED CLIENTS:");
            Console.ResetColor();

            if (_devServer == null)
            {
                Console.WriteLine("    Server not running");
                return;
            }

            Console.WriteLine("    Connected: " + _devServer.ClientCount.ToString());
            Console.WriteLine("    Total connections: " + _devServer.TotalConnections.ToString());
            Console.WriteLine();
        }

        private static void PrintStats()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    SERVER STATISTICS:");
            Console.ResetColor();

            Console.WriteLine("    HTTP Requests:    " + _requestCount.ToString());
            Console.WriteLine("    Clients:          " + (_devServer != null ? _devServer.ClientCount.ToString() : "0"));
            Console.WriteLine("    Total Conns:      " + (_devServer != null ? _devServer.TotalConnections.ToString() : "0"));

            if (_engine != null)
            {
                int logCount = 0;
                lock (_engine.DevLogs) { logCount = _engine.DevLogs.Count; }
                int clickCount = 0;
                lock (_engine.ClickHistory) { clickCount = _engine.ClickHistory.Count; }

                Console.WriteLine("    Log Entries:      " + logCount.ToString());
                Console.WriteLine("    Click Events:     " + clickCount.ToString());
                Console.WriteLine("    Controls:         " + _engine.GetControlsList().Count.ToString());
                Console.WriteLine("    Handlers:         " + _engine.GetHandlersList().Count.ToString());

                Dictionary<string, object> state = _engine.GetStateSnapshot();
                Console.WriteLine("    State Variables:  " + state.Count.ToString());

                Console.WriteLine("    Plugins:          " + _engine.GetLoadedPlugins().Count.ToString());

                CacheStats cs = _engine.GetCacheStats();
                Console.WriteLine("    Cache Entries:    " + cs.TotalEntries.ToString());
                Console.WriteLine("    Cache Hits:       " + cs.TotalHits.ToString());
            }
            Console.WriteLine();
        }

        #endregion

        #region Event Handlers

        private static void OnEngineLog(string level, string message)
        {
            // Logs are handled by DevToolsServer via /logs endpoint
        }

        private static void OnHotReload(string path)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  [" + DateTime.Now.ToString("HH:mm:ss") + "] Hot reload: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Path.GetFileName(path));
            Console.ResetColor();
        }

        private static void OnControlClick(ClickEvent click)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("  Click: " + click.ControlName + " (" + click.ControlType + ")");
            Console.ResetColor();
        }

        private static void OnServerRequest(string request)
        {
            _requestCount++;
        }

        private static void OnServerResponse(string response)
        {
            // Silent by default
        }

        private static void OnWsClientConnected(string clientId)
        {
            _wsClientCount++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [DevTools] Client connected: " + clientId);
            Console.ResetColor();
        }

        private static void OnWsClientDisconnected(string clientId)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [DevTools] Client disconnected: " + clientId);
            Console.ResetColor();
        }

        #endregion

        #region Helpers

        private static string GetXmlPath(string[] args, int index)
        {
            if (args.Length <= index)
            {
                string[] xmlFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.xml");

                foreach (string f in xmlFiles)
                {
                    if (Path.GetFileName(f).ToLower() == "app.xml")
                        return Path.GetFullPath(f);
                }

                if (xmlFiles.Length > 0)
                    return Path.GetFullPath(xmlFiles[0]);

                string configPath = Path.Combine(Environment.CurrentDirectory, "uvel.json");
                if (File.Exists(configPath))
                {
                    string configContent = File.ReadAllText(configPath);
                    int idx = configContent.IndexOf("\"entry\"");
                    if (idx > 0)
                    {
                        int start = configContent.IndexOf("\"", idx + 8) + 1;
                        int end = configContent.IndexOf("\"", start);
                        if (start > 0 && end > start)
                        {
                            string entry = configContent.Substring(start, end - start);
                            if (File.Exists(entry))
                                return Path.GetFullPath(entry);
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  No XML file specified or found");
                Console.ResetColor();
                return null;
            }

            string path = args[index];
            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  File not found: " + path);
                Console.ResetColor();
                return null;
            }

            return Path.GetFullPath(path);
        }

        private static void RunApp(string xmlPath, bool devMode)
        {
            try
            {
                WpfEngine engine = new WpfEngine();
                engine.IsDevMode = devMode;
                engine.Run(xmlPath, false);
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }
        }

        private static void PrintMiniHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  UVEL FRAMEWORK v3.0");
            Console.ResetColor();
        }

        private static void PrintDevBanner(string xmlPath, int port)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  UVEL DEV SERVER v3.0");
            Console.WriteLine("  =====================");
            Console.ResetColor();
            Console.WriteLine("  File:     " + Path.GetFileName(xmlPath));
            Console.WriteLine("  DevTools: http://localhost:" + port.ToString());
            Console.WriteLine("  Reload:   ENABLED");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  =====================");
            Console.ResetColor();
        }

        private static void PrintServerInfo(int port)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  DevTools server running on port " + port.ToString());
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  REST Endpoints:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    GET  /           - DevTools UI (browser)");
            Console.WriteLine("    GET  /status      - Server status");
            Console.WriteLine("    GET  /state       - State variables");
            Console.WriteLine("    GET  /controls    - Registered controls");
            Console.WriteLine("    GET  /handlers    - Event handlers");
            Console.WriteLine("    GET  /logs        - Recent logs");
            Console.WriteLine("    GET  /clicks      - Click history");
            Console.WriteLine("    GET  /switches    - Debug switches");
            Console.WriteLine("    GET  /plugins     - Loaded plugins");
            Console.WriteLine("    GET  /cache       - Cache stats");
            Console.WriteLine("    GET  /bindings    - Data bindings");
            Console.WriteLine("    GET  /reload      - Force hot reload");
            Console.WriteLine("    POST /exec        - Execute handler");
            Console.WriteLine("    POST /set         - Set variable (name=value)");
            Console.WriteLine("    POST /cmd         - Run shell command");
            Console.ResetColor();
        }

        private static void SafeConsoleClear()
        {
            try { Console.Clear(); }
            catch (IOException) { }
            catch (InvalidOperationException) { }
        }

        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  UVEL FRAMEWORK v3.0");
            Console.WriteLine("  XML-based WPF Application Framework");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  USAGE:");
            Console.WriteLine("    uvel <command> [options]");
            Console.WriteLine();
            Console.WriteLine("  COMMANDS:");
            Console.WriteLine("    run   <file.xml>      Run application");
            Console.WriteLine("    dev   <file.xml>      Run with DevTools + hot reload");
            Console.WriteLine("    build <file.xml>      Build standalone EXE");
            Console.WriteLine("    bridge                Start local Workspace bridge");
            Console.WriteLine("    install-protocol      Register uvel:// workspace protocol");
            Console.WriteLine("    new   <name>          Create new project");
            Console.WriteLine();
            Console.WriteLine("  DEV OPTIONS:");
            Console.WriteLine("    -p, --port <n>        DevTools port (default: 9222)");
            Console.WriteLine("    --no-debug            Disable interactive console");
            Console.WriteLine();
            Console.WriteLine("  BUILD OPTIONS:");
            Console.WriteLine("    -o, --output <dir>    Output directory");
            Console.WriteLine("    -n, --name <name>     Executable name");
            Console.WriteLine("    -i, --icon <path>     Icon file (.ico)");
            Console.WriteLine("    --console             Show console window");
            Console.WriteLine();
            Console.WriteLine("  EXAMPLES:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    uvel dev App.xml");
            Console.WriteLine("    uvel dev App.xml --port 8080");
            Console.WriteLine("    uvel build App.xml -o ./dist -n MyApp");
            Console.WriteLine("    uvel bridge --port 9327");
            Console.WriteLine("    uvel install-protocol");
            Console.WriteLine("    uvel new MyProject");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void PrintInfo(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  " + msg);
            Console.ResetColor();
        }

        private static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ERROR: " + msg);
            Console.ResetColor();
        }

        private static string CreateTemplateApp(string appName, string template)
        {
            switch (template.ToLower())
            {
                case "calculator":
                    return CreateCalculatorTemplate(appName);
                case "todo":
                    return CreateTodoTemplate(appName);
                default:
                    return CreateDefaultTemplate(appName);
            }
        }

        private static string CreateDefaultTemplate(string appName)
        {
            StringBuilder t = new StringBuilder();
            t.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            t.AppendLine("<App Name=\"" + appName + "\"");
            t.AppendLine("     Width=\"800\" Height=\"600\"");
            t.AppendLine("     Theme=\"Dark\"");
            t.AppendLine("     DarkMode=\"true\"");
            t.AppendLine("     DevMode=\"true\"");
            t.AppendLine("     Version=\"1.0.0\">");
            t.AppendLine("");
            t.AppendLine("    <UI>");
            t.AppendLine("        <Grid Background=\"#1E1E1E\">");
            t.AppendLine("            <StackPanel VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\">");
            t.AppendLine("                <TextBlock Name=\"lblTitle\" Text=\"" + appName + "\"");
            t.AppendLine("                           FontSize=\"48\" FontWeight=\"Bold\"");
            t.AppendLine("                           Foreground=\"White\" HorizontalAlignment=\"Center\"/>");
            t.AppendLine("");
            t.AppendLine("                <TextBlock Text=\"Welcome to Uvel Framework\"");
            t.AppendLine("                           FontSize=\"16\" Foreground=\"#888888\"");
            t.AppendLine("                           HorizontalAlignment=\"Center\" Margin=\"0,10,0,30\"/>");
            t.AppendLine("");
            t.AppendLine("                <Button Name=\"btnHello\" Content=\"Click Me!\"");
            t.AppendLine("                        Background=\"#0078D4\" Foreground=\"White\"");
            t.AppendLine("                        Padding=\"30,15\" FontSize=\"16\"");
            t.AppendLine("                        onClick=\"OnButtonClick\"/>");
            t.AppendLine("");
            t.AppendLine("                <TextBlock Name=\"lblCounter\" Text=\"Clicks: 0\"");
            t.AppendLine("                           FontSize=\"14\" Foreground=\"#AAAAAA\"");
            t.AppendLine("                           HorizontalAlignment=\"Center\" Margin=\"0,20,0,0\"/>");
            t.AppendLine("            </StackPanel>");
            t.AppendLine("        </Grid>");
            t.AppendLine("    </UI>");
            t.AppendLine("");
            t.AppendLine("    <Logic>");
            t.AppendLine("        <Var Name=\"counter\" Value=\"0\" Type=\"int\"/>");
            t.AppendLine("");
            t.AppendLine("        <Handler Name=\"OnButtonClick\">");
            t.AppendLine("            <Increment Var=\"counter\" By=\"1\"/>");
            t.AppendLine("            <Set Target=\"lblCounter\" Property=\"Text\" Value=\"Clicks: {counter}\"/>");
            t.AppendLine("            <If Condition=\"{counter} >= 10\">");
            t.AppendLine("                <Alert Message=\"You clicked 10 times!\" Title=\"Milestone\"/>");
            t.AppendLine("                <Set Var=\"counter\" Value=\"0\"/>");
            t.AppendLine("            </If>");
            t.AppendLine("        </Handler>");
            t.AppendLine("    </Logic>");
            t.AppendLine("</App>");
            return t.ToString();
        }

        private static string CreateCalculatorTemplate(string appName)
        {
            StringBuilder t = new StringBuilder();
            t.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            t.AppendLine("<App Name=\"" + appName + "\" Width=\"300\" Height=\"400\" Theme=\"Dark\" DarkMode=\"true\">");
            t.AppendLine("    <UI>");
            t.AppendLine("        <Grid Background=\"#2D2D30\">");
            t.AppendLine("            <StackPanel Margin=\"10\">");
            t.AppendLine("                <TextBox Name=\"display\" FontSize=\"24\" TextAlignment=\"Right\" Margin=\"0,0,0,10\"/>");
            t.AppendLine("            </StackPanel>");
            t.AppendLine("        </Grid>");
            t.AppendLine("    </UI>");
            t.AppendLine("    <Logic>");
            t.AppendLine("        <Var Name=\"current\" Value=\"\" Type=\"string\"/>");
            t.AppendLine("    </Logic>");
            t.AppendLine("</App>");
            return t.ToString();
        }

        private static string CreateTodoTemplate(string appName)
        {
            StringBuilder t = new StringBuilder();
            t.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            t.AppendLine("<App Name=\"" + appName + "\" Width=\"400\" Height=\"500\" Theme=\"Dark\" DarkMode=\"true\">");
            t.AppendLine("    <UI>");
            t.AppendLine("        <Grid Background=\"#1E1E1E\">");
            t.AppendLine("            <StackPanel Margin=\"20\">");
            t.AppendLine("                <TextBlock Text=\"Todo List\" FontSize=\"24\" Foreground=\"White\" Margin=\"0,0,0,20\"/>");
            t.AppendLine("                <StackPanel Orientation=\"Horizontal\" Margin=\"0,0,0,10\">");
            t.AppendLine("                    <TextBox Name=\"txtInput\" Width=\"250\" Margin=\"0,0,10,0\"/>");
            t.AppendLine("                    <Button Content=\"Add\" onClick=\"AddTodo\"/>");
            t.AppendLine("                </StackPanel>");
            t.AppendLine("                <ListBox Name=\"todoList\" Height=\"300\"/>");
            t.AppendLine("            </StackPanel>");
            t.AppendLine("        </Grid>");
            t.AppendLine("    </UI>");
            t.AppendLine("    <Logic>");
            t.AppendLine("        <Handler Name=\"AddTodo\">");
            t.AppendLine("        </Handler>");
            t.AppendLine("    </Logic>");
            t.AppendLine("</App>");
            return t.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Uvel Build System v3.0
    /// </summary>
    public class UvelBuildSystem
    {
        public string XmlPath { get; set; }
        public string OutputDirectory { get; set; }
        public string AppName { get; set; }
        public string IconPath { get; set; }
        public bool ShowConsole { get; set; }
        public bool Compress { get; set; }

        private string _tempDir;

        public void Build()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  UVEL BUILD SYSTEM v3.0");
            Console.ResetColor();
            Console.WriteLine();

            // Program.cs - Build() metodida:
_tempDir = Path.Combine(Environment.CurrentDirectory, "build", "temp_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                PrintStep(1, 7, "Validating XML...");
                ValidateXml();
                PrintOK();

                PrintStep(2, 7, "Creating directories...");
                CreateDirectories();
                PrintOK();

                PrintStep(3, 7, "Generating program...");
                GenerateProgram();
                PrintOK();

                PrintStep(4, 7, "Copying engine files...");
                CopyEngineFiles();
                PrintOK();

                PrintStep(5, 7, "Finding C# compiler...");
                string cscPath = FindCsc();
                if (string.IsNullOrEmpty(cscPath))
                    throw new Exception("CSC compiler not found.");
                PrintOK(Path.GetFileName(Path.GetDirectoryName(cscPath)));

                PrintStep(6, 7, "Compiling...");
                Compile(cscPath);
                PrintOK();

                PrintStep(7, 7, "Cleaning up...");
                Cleanup();
                PrintOK();

                string exePath = Path.Combine(OutputDirectory, AppName + ".exe");
                FileInfo fi = new FileInfo(exePath);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  BUILD SUCCESSFUL!");
                Console.ResetColor();
                Console.WriteLine("  Output: " + exePath);
                Console.WriteLine("  Size:   " + FormatSize(fi.Length));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAILED!");
                Console.WriteLine("  BUILD FAILED: " + ex.Message);
                Console.ResetColor();

                try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); } catch { }
            }
        }

        private void ValidateXml()
{
    if (!File.Exists(XmlPath))
        throw new FileNotFoundException("XML file not found: " + XmlPath);

    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
    doc.Load(XmlPath);

    if (doc.DocumentElement == null)
        throw new Exception("Invalid XML: no root element");

    if (doc.DocumentElement.Name != "App")
        throw new Exception("Invalid Uvel XML: root must be <App>");
}

        private void CreateDirectories()
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            if (!Directory.Exists(_tempDir))
                Directory.CreateDirectory(_tempDir);
        }

        private void GenerateProgram()
        {
            string xmlContent = File.ReadAllText(XmlPath, Encoding.UTF8);
            string escapedXml = xmlContent.Replace("\"", "\"\"");

            StringBuilder code = new StringBuilder();
            code.AppendLine("// Auto-generated by Uvel Build System v3.0");
            code.AppendLine("using System;");
            code.AppendLine("using System.Windows;");
            code.AppendLine("using Uvel.WPF;");
            code.AppendLine("");
            code.AppendLine("namespace " + SanitizeName(AppName));
            code.AppendLine("{");
            code.AppendLine("    public class Program");
            code.AppendLine("    {");
            code.AppendLine("        [STAThread]");
            code.AppendLine("        public static void Main(string[] args)");
            code.AppendLine("        {");
            code.AppendLine("            try");
            code.AppendLine("            {");
            code.AppendLine("                string appXml = @\"" + escapedXml + "\";");
            code.AppendLine("                WpfEngine engine = new WpfEngine();");
            code.AppendLine("                engine.IsDevMode = false;");
            code.AppendLine("                engine.Run(appXml, true);");
            code.AppendLine("            }");
            code.AppendLine("            catch (Exception ex)");
            code.AppendLine("            {");
            code.AppendLine("                MessageBox.Show(\"Error:\\n\" + ex.Message, \"" + AppName + "\", MessageBoxButton.OK, MessageBoxImage.Error);");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine("    }");
            code.AppendLine("}");

            File.WriteAllText(Path.Combine(_tempDir, "Program.cs"), code.ToString(), Encoding.UTF8);
        }

        private void CopyEngineFiles()
{
    string[] requiredFiles = {
        "WpfEngine.cs",
        "WpfUI.cs",
        "LogicRunner.cs",
        "XmlParser.cs",
        "XamlRenderer.cs",
        "CSharpCompiler.cs",
        "DevToolsServer.cs"
    };

    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string currentDir = Environment.CurrentDirectory;
    int copiedCount = 0;

    foreach (string file in requiredFiles)
    {
        string[] searchPaths = {
            Path.Combine(currentDir, file),
            Path.Combine(baseDir, file)
        };

        foreach (string src in searchPaths)
        {
            if (File.Exists(src))
            {
                File.Copy(src, Path.Combine(_tempDir, Path.GetFileName(src)), true);
                copiedCount++;
                break;
            }
        }
    }

    if (copiedCount < 3)
    {
        throw new Exception("Required engine files not found.");
    }

    // ICON NUSXALASH OLIB TASHLANDI
}

        private void Compile(string cscPath)
{
    string outputExe = Path.Combine(OutputDirectory, AppName + ".exe");
    string targetType = ShowConsole ? "exe" : "winexe";
    string wpfPath = FindWpfPath();

    if (string.IsNullOrEmpty(wpfPath))
        throw new Exception("WPF DLLs not found. Check .NET Framework installation.");

    StringBuilder args = new StringBuilder();
    args.Append("/nologo ");
    args.Append("/target:" + targetType + " ");
    args.Append("/out:\"" + Path.GetFullPath(outputExe) + "\" ");
    args.Append("/optimize+ ");
    args.Append("/warn:0 ");
    args.Append("/platform:anycpu ");
    args.Append("/nowin32manifest ");

    args.Append("/reference:System.dll ");
    args.Append("/reference:System.Core.dll ");
    args.Append("/reference:System.Xml.dll ");
    args.Append("/reference:System.Xaml.dll ");
    args.Append("/reference:System.Net.dll ");
    args.Append("/reference:System.Net.Http.dll ");
    args.Append("/reference:Microsoft.CSharp.dll ");
    
    args.Append("/reference:\"" + Path.Combine(wpfPath, "WindowsBase.dll") + "\" ");
    args.Append("/reference:\"" + Path.Combine(wpfPath, "PresentationCore.dll") + "\" ");
    args.Append("/reference:\"" + Path.Combine(wpfPath, "PresentationFramework.dll") + "\" ");

    if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
    {
        string fullIconPath = Path.GetFullPath(IconPath);
        args.Append("/win32icon:\"" + fullIconPath + "\" ");
    }

    // MUHIM: ABSOLUTE paths ishlatamiz, relative emas!
    string[] csFiles = Directory.GetFiles(_tempDir, "*.cs");
    foreach (string file in csFiles)
    {
        // To'liq yo'l bilan, faqat fayl nomi emas
        args.Append("\"" + Path.GetFullPath(file) + "\" ");
    }

    ProcessStartInfo psi = new ProcessStartInfo();
    psi.FileName = cscPath;
    psi.Arguments = args.ToString();
    psi.UseShellExecute = false;
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardError = true;
    psi.CreateNoWindow = true;
    
    // TUZATISH: WorkingDirectory ni O'CHIRISH!
    // CSC o'zining default temp papkasini ishlatsin
    // psi.WorkingDirectory = _tempDir;  <-- BU QATORNI O'CHIRING YOKI COMMENT QILING

    using (Process p = Process.Start(psi))
    {
        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            string errorMsg = !string.IsNullOrEmpty(stderr) ? stderr : stdout;
            throw new Exception("Compilation failed:\n" + errorMsg);
        }
    }
}
        private string FindWpfPath()
    {
        string runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        
        // Check standard WPF path (v4.0+)
        string wpfPath = Path.Combine(runtimeDir, "WPF");
        if (Directory.Exists(wpfPath) && File.Exists(Path.Combine(wpfPath, "PresentationCore.dll")))
            return wpfPath;

        // Check runtime dir directly (sometimes WPF dlls are there)
        if (File.Exists(Path.Combine(runtimeDir, "PresentationCore.dll")))
            return runtimeDir;

        // Check Reference Assemblies (for build tools)
        string refPath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8";
        if (Directory.Exists(refPath)) return refPath;

        refPath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2";
        if (Directory.Exists(refPath)) return refPath;

        refPath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0";
        if (Directory.Exists(refPath)) return refPath;

        return null;
    }
        private void Cleanup()
        {
            try
            {
                if (Directory.Exists(_tempDir))
                {
                    Directory.Delete(_tempDir, true);
                }
            }
            catch { }
        }

        private string FindCsc()
        {
            string[] paths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "Microsoft.NET\\Framework64\\v4.0.30319\\csc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "Microsoft.NET\\Framework\\v4.0.30319\\csc.exe"),
                "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\Roslyn\\csc.exe",
                "C:\\Program Files\\Microsoft Visual Studio\\2022\\Professional\\MSBuild\\Current\\Bin\\Roslyn\\csc.exe",
                "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\Roslyn\\csc.exe"
            };

            foreach (string p in paths)
            {
                if (File.Exists(p)) return p;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("where", "csc.exe");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                using (Process proc = Process.Start(psi))
                {
                    string result = proc.StandardOutput.ReadLine();
                    proc.WaitForExit();
                    if (!string.IsNullOrEmpty(result) && File.Exists(result))
                        return result;
                }
            }
            catch { }

            return null;
        }

        private string SanitizeName(string name)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else if (c == ' ' || c == '-')
                    sb.Append('_');
            }

            string result = sb.ToString();
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes.ToString() + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("F2") + " MB";
        }

        private void PrintStep(int current, int total, string message)
        {
            Console.Write("  [" + current.ToString() + "/" + total.ToString() + "] " + message.PadRight(30));
        }

        private void PrintOK(string extra)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("OK");
            Console.ResetColor();
            if (!string.IsNullOrEmpty(extra))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" (" + extra + ")");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        private void PrintOK()
        {
            PrintOK("");
        }
    }
}