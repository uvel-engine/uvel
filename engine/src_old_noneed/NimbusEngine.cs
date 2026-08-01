using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Uvel
{
    /// <summary>
    /// Uvel Engine - Core Engine Class (WinForms version)
    /// Manages windows, state, and application lifecycle
    /// Compatible with .NET Framework 4.0+
    /// </summary>
    public class UvelEngine
    {
        #region Properties
        
        public Dictionary<string, Form> Windows { get; private set; }
        public Dictionary<string, Control> Controls { get; private set; }
        public Dictionary<string, object> State { get; private set; }
        public Dictionary<string, XmlNode> EventHandlers { get; private set; }
        public Dictionary<string, Delegate> CompiledMethods { get; private set; }
        public Dictionary<string, object> Variables { get; private set; }
        
        private XmlParser _xmlParser;
        private FormRenderer _formRenderer;
        private LogicRunner _logicRunner;
        private CSharpCompiler _csharpCompiler;
        
        private XmlDocument _appDoc;
        private Timer _systemTimer;
        private FileSystemWatcher _fileWatcher;
        private string _currentXmlPath;
        private Form _mainForm;
        
        public bool IsDevMode { get; set; }
        
        #endregion
        
        #region Constructor
        
        public UvelEngine()
        {
            Windows = new Dictionary<string, Form>();
            Controls = new Dictionary<string, Control>();
            State = new Dictionary<string, object>();
            EventHandlers = new Dictionary<string, XmlNode>();
            CompiledMethods = new Dictionary<string, Delegate>();
            Variables = new Dictionary<string, object>();
            
            _xmlParser = new XmlParser(this);
            _formRenderer = new FormRenderer(this);
            _logicRunner = new LogicRunner(this);
            _csharpCompiler = new CSharpCompiler(this);
            
            // Setup system timer (1 second interval)
            _systemTimer = new Timer();
            _systemTimer.Interval = 1000;
            _systemTimer.Tick += OnSystemTimerTick;
            
            // Initialize system state
            InitializeSystemState();
        }
        
        private void InitializeSystemState()
        {
            State["_currentTime"] = DateTime.Now.ToString("HH:mm:ss");
            State["_currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            State["_osVersion"] = Environment.OSVersion.VersionString;
            State["_machineName"] = Environment.MachineName;
            State["_userName"] = Environment.UserName;
        }
        
        #endregion
        
        #region System Timer
        
        private void OnSystemTimerTick(object sender, EventArgs e)
        {
            // Update time
            State["_currentTime"] = DateTime.Now.ToString("HH:mm:ss");
            State["_currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            
            // Update clock label if exists
            if (Controls.ContainsKey("clockLabel"))
            {
                Control clockLabel = Controls["clockLabel"];
                if (clockLabel is Label)
                {
                    ((Label)clockLabel).Text = DateTime.Now.ToString("HH:mm:ss");
                }
            }
        }
        
        #endregion
        
        #region Validate
        
        public void ValidateOnly(string xmlPath)
        {
            _appDoc = new XmlDocument();
            _appDoc.Load(xmlPath);
            
            XmlNode root = _appDoc.DocumentElement;
            if (root == null)
            {
                throw new Exception("XML root element not found");
            }
            
            ParseAppMetadata(root);
            
            // Validate ManualC sections
            XmlNodeList manualCNodes = root.SelectNodes("//ManualC");
            if (manualCNodes != null)
            {
                foreach (XmlNode mc in manualCNodes)
                {
                    _csharpCompiler.Compile(mc);
                }
            }
            
            Console.WriteLine("✅ Validation successful!");
        }
        
        #endregion
        
        #region Run Application
        
        public void Run(string xmlPath)
        {
            _currentXmlPath = xmlPath;
            
            _appDoc = new XmlDocument();
            _appDoc.Load(xmlPath);
            
            XmlNode root = _appDoc.DocumentElement;
            
            if (root == null)
            {
                throw new Exception("XML root element not found");
            }
            
            // Parse app metadata
            ParseAppMetadata(root);
            
            // Check dev mode
            string devMode = _xmlParser.GetAttribute(root, "DevMode", "false");
            IsDevMode = devMode.ToLower() == "true";
            
            // Compile ManualC sections first
            XmlNodeList manualCNodes = root.SelectNodes("//ManualC");
            if (manualCNodes != null)
            {
                foreach (XmlNode mc in manualCNodes)
                {
                    _csharpCompiler.Compile(mc);
                }
            }
            
            // Parse Logic section
            XmlNode logicNode = root.SelectSingleNode("Logic");
            if (logicNode != null)
            {
                _xmlParser.ParseLogic(logicNode);
            }
            
            // Parse and create windows
            XmlNodeList windowNodes = root.SelectNodes("Window");
            if (windowNodes != null && windowNodes.Count > 0)
            {
                foreach (XmlNode windowNode in windowNodes)
                {
                    Form form = _formRenderer.CreateWindow(windowNode);
                    string id = _xmlParser.GetAttribute(windowNode, "id", "main");
                    Windows[id] = form;
                }
            }
            else
            {
                XmlNode uiNode = root.SelectSingleNode("UI");
                if (uiNode != null)
                {
                    Form form = _formRenderer.CreateWindowFromUI(root, uiNode);
                    Windows["main"] = form;
                }
            }
            
            // Setup auto-refresh if dev mode
            if (IsDevMode)
            {
                SetupAutoRefresh(xmlPath);
            }
            
            // Show all windows and run
            Form mainForm = null;
            foreach (KeyValuePair<string, Form> kvp in Windows)
            {
                if (mainForm == null)
                {
                    mainForm = kvp.Value;
                    _mainForm = mainForm;
                }
                else
                {
                    kvp.Value.Show();
                }
            }
            
            if (mainForm != null)
            {
                Program.SetMainForm(mainForm);
                _systemTimer.Start();
                Application.Run(mainForm);
                _systemTimer.Stop();
            }
        }
        
        #endregion
        
        #region Auto-Refresh
        
        private void SetupAutoRefresh(string xmlPath)
        {
            string directory = Path.GetDirectoryName(xmlPath);
            string fileName = Path.GetFileName(xmlPath);
            
            _fileWatcher = new FileSystemWatcher();
            _fileWatcher.Path = directory;
            _fileWatcher.Filter = fileName;
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
            
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║       🚀 Uvel Dev Server Running         ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║  Auto-refresh: ENABLED                     ║");
            Console.WriteLine("║  File: " + fileName.PadRight(35) + "║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
        }
        
        private DateTime _lastReload = DateTime.MinValue;
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if ((DateTime.Now - _lastReload).TotalMilliseconds < 500)
                return;
            _lastReload = DateTime.Now;
            
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] 🔄 Reloading...");
            
            if (_mainForm != null)
            {
                _mainForm.Invoke(new Action(delegate
                {
                    try
                    {
                        ReloadApplication();
                        Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] ✅ Success!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] ❌ Error: " + ex.Message);
                    }
                }));
            }
        }
        
        private void ReloadApplication()
        {
            Controls.Clear();
            EventHandlers.Clear();
            
            _appDoc = new XmlDocument();
            _appDoc.Load(_currentXmlPath);
            
            XmlNode root = _appDoc.DocumentElement;
            
            XmlNode logicNode = root.SelectSingleNode("Logic");
            if (logicNode != null)
            {
                _xmlParser.ParseLogic(logicNode);
            }
            
            XmlNode uiNode = root.SelectSingleNode("UI");
            if (uiNode != null)
            {
                _mainForm.Controls.Clear();
                _formRenderer.PopulateForm(_mainForm, uiNode);
            }
        }
        
        #endregion
        
        #region Metadata
        
        private void ParseAppMetadata(XmlNode root)
        {
            string appName = _xmlParser.GetAttribute(root, "name", "Uvel App");
            State["_appName"] = appName;
            
            string theme = _xmlParser.GetAttribute(root, "theme", "dark");
            State["_theme"] = theme;
            BaseUI.CurrentTheme = theme;
            
            string version = _xmlParser.GetAttribute(root, "version", "1.0.0");
            State["_version"] = version;
        }
        
        #endregion
        
        #region Handler Execution
        
        public void ExecuteHandler(string handlerName)
        {
            ExecuteHandler(handlerName, null);
        }
        
        public void ExecuteHandler(string handlerName, object sender)
        {
            if (EventHandlers.ContainsKey(handlerName))
            {
                _logicRunner.Execute(EventHandlers[handlerName], sender);
            }
            else if (handlerName.StartsWith("ManualC:"))
            {
                string methodId = handlerName.Substring(8);
                ExecuteManualC(methodId);
            }
        }
        
        public object ExecuteManualC(string methodId)
        {
            return ExecuteManualC(methodId, new object[0]);
        }
        
        public object ExecuteManualC(string methodId, object[] args)
        {
            if (CompiledMethods.ContainsKey(methodId))
            {
                try
                {
                    return CompiledMethods[methodId].DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show("ManualC error [" + methodId + "]:\n" + errorMsg);
                }
            }
            return null;
        }
        
        #endregion
        
        #region Control Management
        
        public Control GetControl(string id)
        {
            if (Controls.ContainsKey(id))
            {
                return Controls[id];
            }
            return null;
        }
        
        public void SetControlProperty(string controlId, string property, object value)
        {
            Control control = GetControl(controlId);
            if (control == null) return;
            
            System.Reflection.PropertyInfo prop = control.GetType().GetProperty(property);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    object convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(control, convertedValue, null);
                }
                catch
                {
                    try { prop.SetValue(control, value, null); }
                    catch { }
                }
            }
        }
        
        public object GetControlProperty(string controlId, string property)
        {
            Control control = GetControl(controlId);
            if (control == null) return null;
            
            System.Reflection.PropertyInfo prop = control.GetType().GetProperty(property);
            if (prop != null)
            {
                return prop.GetValue(control, null);
            }
            return null;
        }
        
        #endregion
        
        #region State Management
        
        public void SetState(string key, object value)
        {
            State[key] = value;
        }
        
        public object GetState(string key)
        {
            if (State.ContainsKey(key))
            {
                return State[key];
            }
            return null;
        }
        
        #endregion
        
        #region Variable Operations (+=, -=, *=, /=)
        
        public void IncrementVariable(string name, object value)
        {
            if (!State.ContainsKey(name)) State[name] = 0;
            
            double current, addVal;
            double.TryParse(State[name].ToString(), out current);
            double.TryParse(value.ToString(), out addVal);
            State[name] = current + addVal;
        }
        
        public void DecrementVariable(string name, object value)
        {
            if (!State.ContainsKey(name)) State[name] = 0;
            
            double current, subVal;
            double.TryParse(State[name].ToString(), out current);
            double.TryParse(value.ToString(), out subVal);
            State[name] = current - subVal;
        }
        
        public void MultiplyVariable(string name, object value)
        {
            if (!State.ContainsKey(name)) State[name] = 0;
            
            double current, mulVal;
            double.TryParse(State[name].ToString(), out current);
            double.TryParse(value.ToString(), out mulVal);
            State[name] = current * mulVal;
        }
        
        public void DivideVariable(string name, object value)
        {
            if (!State.ContainsKey(name)) State[name] = 0;
            
            double current, divVal;
            double.TryParse(State[name].ToString(), out current);
            double.TryParse(value.ToString(), out divVal);
            if (divVal != 0) State[name] = current / divVal;
        }
        
        #endregion
    }
    
    #region Placeholder Classes
    
    public class XmlParser
    {
        private UvelEngine _engine;
        
        public XmlParser(UvelEngine engine)
        {
            _engine = engine;
        }
        
        public string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
        
        public void ParseLogic(XmlNode logicNode)
        {
            foreach (XmlNode child in logicNode.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                if (child.Name == "Handler")
                {
                    string name = GetAttribute(child, "Name", "");
                    if (!string.IsNullOrEmpty(name))
                    {
                        _engine.EventHandlers[name] = child;
                    }
                }
            }
        }
    }
    
    public class FormRenderer
    {
        private UvelEngine _engine;
        
        public FormRenderer(UvelEngine engine)
        {
            _engine = engine;
        }
        
        public Form CreateWindow(XmlNode windowNode)
        {
            return new Form();
        }
        
        public Form CreateWindowFromUI(XmlNode root, XmlNode uiNode)
        {
            return new Form();
        }
        
        public void PopulateForm(Form form, XmlNode uiNode)
        {
        }
    }
    
    public class LogicRunner
    {
        private UvelEngine _engine;
        
        public LogicRunner(UvelEngine engine)
        {
            _engine = engine;
        }
        
        public void Execute(XmlNode node, object sender)
        {
        }
    }
    
    public class CSharpCompiler
    {
        private UvelEngine _engine;
        
        public CSharpCompiler(UvelEngine engine)
        {
            _engine = engine;
        }
        
        public void Compile(XmlNode node)
        {
        }
    }
    
    public static class BaseUI
    {
        public static string CurrentTheme { get; set; }
    }
    
    public static class Program
    {
        public static void SetMainForm(Form form) { }
    }
    
    #endregion
}
