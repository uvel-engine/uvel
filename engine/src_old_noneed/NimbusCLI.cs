using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using Uvel.WPF;

namespace Uvel
{
    /// <summary>
    /// Uvel CLI - Command Line Interface
    /// Commands: run, dev, build, new, help
    /// </summary>
    public class UvelCLI
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
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
                case "new":
                    NewCommand(args);
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    // Agar .xml fayl bo'lsa, to'g'ridan-to'g'ri run qil
                    if (command.EndsWith(".xml") && File.Exists(command))
                    {
                        RunApp(command, false);
                    }
                    else
                    {
                        Console.WriteLine("Unknown command: " + command);
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
            
            RunApp(xmlPath, false);
        }
        
        private static void DevCommand(string[] args)
        {
            string xmlPath = GetXmlPath(args, 1);
            if (string.IsNullOrEmpty(xmlPath)) return;
            
            PrintDevBanner(xmlPath);
            RunApp(xmlPath, true);
        }
        
        private static void BuildCommand(string[] args)
        {
            string xmlPath = GetXmlPath(args, 1);
            if (string.IsNullOrEmpty(xmlPath)) return;
            
            // Parse build arguments
            string outputDir = "./build";
            string appName = Path.GetFileNameWithoutExtension(xmlPath);
            string iconPath = "";
            bool singleFile = false;
            bool showConsole = false;
            
            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                
                if ((arg == "-o" || arg == "--output") && i + 1 < args.Length)
                {
                    outputDir = args[++i];
                }
                else if ((arg == "-n" || arg == "--name") && i + 1 < args.Length)
                {
                    appName = args[++i];
                }
                else if ((arg == "-i" || arg == "--icon") && i + 1 < args.Length)
                {
                    iconPath = args[++i];
                }
                else if (arg == "--single-file")
                {
                    singleFile = true;
                }
                else if (arg == "--console")
                {
                    showConsole = true;
                }
            }
            
            // Build
            UvelBuildSystem builder = new UvelBuildSystem();
            builder.XmlPath = xmlPath;
            builder.OutputDirectory = outputDir;
            builder.AppName = appName;
            builder.IconPath = iconPath;
            builder.SingleFile = singleFile;
            builder.ShowConsole = showConsole;
            
            builder.Build();
        }
        
        private static void NewCommand(string[] args)
        {
            string projectName = "MyUvelApp";
            if (args.Length > 1)
            {
                projectName = args[1];
            }
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Creating new Uvel project: " + projectName);
            Console.ResetColor();
            Console.WriteLine();
            
            // Create directory
            string projectDir = Path.Combine(Environment.CurrentDirectory, projectName);
            if (!Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }
            
            // Create App.xml
            string appXml = CreateTemplateApp(projectName);
            File.WriteAllText(Path.Combine(projectDir, "App.xml"), appXml, Encoding.UTF8);
            
            // Create assets folder
            string assetsDir = Path.Combine(projectDir, "assets");
            if (!Directory.Exists(assetsDir))
            {
                Directory.CreateDirectory(assetsDir);
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✅ Project created successfully!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Next steps:");
            Console.WriteLine("    cd " + projectName);
            Console.WriteLine("    uvel dev App.xml");
            Console.WriteLine();
        }
        
        #endregion
        
        #region Helpers
        
        private static string GetXmlPath(string[] args, int index)
        {
            if (args.Length <= index)
            {
                // Try to find App.xml or *.xml in current directory
                string[] xmlFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.xml");
                if (xmlFiles.Length > 0)
                {
                    // Prefer App.xml
                    foreach (string f in xmlFiles)
                    {
                        if (Path.GetFileName(f).ToLower() == "app.xml")
                        {
                            return f;
                        }
                    }
                    return xmlFiles[0];
                }
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Error: No XML file specified");
                Console.ResetColor();
                return null;
            }
            
            string path = args[index];
            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Error: File not found: " + path);
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
                engine.Run(xmlPath);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Error: " + ex.Message);
                Console.ResetColor();
                
                // Log to file
                try
                {
                    File.WriteAllText("uvel_error.log", 
                        DateTime.Now.ToString() + "\n" + ex.Message + "\n" + ex.StackTrace);
                }
                catch { }
            }
        }
        
        private static void PrintDevBanner(string xmlPath)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                           ║");
            Console.WriteLine("  ║     ███╗   ██╗██╗███╗   ███╗██████╗ ██╗   ██╗███████╗     ║");
            Console.WriteLine("  ║     ████╗  ██║██║████╗ ████║██╔══██╗██║   ██║██╔════╝     ║");
            Console.WriteLine("  ║     ██╔██╗ ██║██║██╔████╔██║██████╔╝██║   ██║███████╗     ║");
            Console.WriteLine("  ║     ██║╚██╗██║██║██║╚██╔╝██║██╔══██╗██║   ██║╚════██║     ║");
            Console.WriteLine("  ║     ██║ ╚████║██║██║ ╚═╝ ██║██████╔╝╚██████╔╝███████║     ║");
            Console.WriteLine("  ║     ╚═╝  ╚═══╝╚═╝╚═╝     ╚═╝╚═════╝  ╚═════╝ ╚══════╝     ║");
            Console.WriteLine("  ║                                                           ║");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ║              🚀 DEV SERVER RUNNING                        ║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║                                                           ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════╣");
            Console.ResetColor();
            Console.WriteLine("  ║                                                           ║");
            Console.Write("  ║  📁 File: ");
            Console.ForegroundColor = ConsoleColor.White;
            string fileName = Path.GetFileName(xmlPath);
            Console.Write(fileName.PadRight(46));
            Console.ResetColor();
            Console.WriteLine("║");
            Console.Write("  ║  📂 Path: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            string dirPath = Path.GetDirectoryName(xmlPath);
            if (dirPath.Length > 46) dirPath = "..." + dirPath.Substring(dirPath.Length - 43);
            Console.Write(dirPath.PadRight(46));
            Console.ResetColor();
            Console.WriteLine("║");
            Console.WriteLine("  ║                                                           ║");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ║  ✨ Auto-refresh: ENABLED                                 ║");
            Console.ResetColor();
            Console.WriteLine("  ║  💡 Save file to hot-reload                               ║");
            Console.WriteLine("  ║                                                           ║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
        
        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Uvel Framework v2.0");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Usage: uvel <command> [options]");
            Console.WriteLine();
            Console.WriteLine("  Commands:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("    run <file.xml>      ");
            Console.ResetColor();
            Console.WriteLine("Run application");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("    dev <file.xml>      ");
            Console.ResetColor();
            Console.WriteLine("Run with auto-refresh (dev server)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("    build <file.xml>    ");
            Console.ResetColor();
            Console.WriteLine("Build executable");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("    new <name>          ");
            Console.ResetColor();
            Console.WriteLine("Create new project");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("    help                ");
            Console.ResetColor();
            Console.WriteLine("Show this help");
            Console.WriteLine();
            Console.WriteLine("  Build options:");
            Console.WriteLine("    -o, --output <dir>    Output directory (default: ./build)");
            Console.WriteLine("    -n, --name <name>     Application name");
            Console.WriteLine("    -i, --icon <file>     Icon file (.ico)");
            Console.WriteLine("    --single-file         Create single executable");
            Console.WriteLine("    --console             Show console window");
            Console.WriteLine();
            Console.WriteLine("  Examples:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    uvel dev App.xml");
            Console.WriteLine("    uvel build App.xml -o ./dist -i icon.ico -n MyApp");
            Console.WriteLine("    uvel new MyProject");
            Console.ResetColor();
            Console.WriteLine();
        }
        
        private static string CreateTemplateApp(string appName)
        {
            return string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<App Name=""{0}"" Width=""800"" Height=""600"" Theme=""Dark"" DevMode=""true"">
    
    <UI>
        <Grid>
            <Grid.RowDefinitions>Auto, *</Grid.RowDefinitions>
            
            <!-- Header -->
            <Border Row=""0"" Background=""#0078D4"" Padding=""20"">
                <TextBlock Text=""🚀 {0}"" FontSize=""24"" FontWeight=""Bold"" 
                           Foreground=""White"" HorizontalAlignment=""Center""/>
            </Border>
            
            <!-- Content -->
            <StackPanel Row=""1"" VerticalAlignment=""Center"" HorizontalAlignment=""Center"">
                
                <TextBlock Text=""Welcome to Uvel!"" FontSize=""32"" FontWeight=""Bold"" 
                           Foreground=""White"" HorizontalAlignment=""Center""/>
                
                <TextBlock Text=""Edit App.xml to get started"" FontSize=""16"" 
                           Foreground=""#888888"" HorizontalAlignment=""Center"" Margin=""0,10,0,30""/>
                
                <TextBox Name=""txtInput"" Width=""300"" Padding=""10"" FontSize=""14""
                         Background=""#2D2D30"" Foreground=""White"" 
                         Text=""Type something...""/>
                
                <Button Name=""btnClick"" Content=""Click Me!"" 
                        Background=""#0078D4"" Foreground=""White""
                        Padding=""20,10"" Margin=""0,20,0,0""
                        HorizontalAlignment=""Center""
                        onClick=""OnButtonClick""/>
                
                <TextBlock Name=""lblResult"" Text="""" FontSize=""18"" 
                           Foreground=""#00D4FF"" HorizontalAlignment=""Center"" Margin=""0,20,0,0""/>
                
            </StackPanel>
        </Grid>
    </UI>
    
    <Logic>
        <Var Name=""clickCount"" Type=""int"" Value=""0""/>
        
        <Handler Name=""OnButtonClick"">
            <Increment State=""clickCount"" Value=""1""/>
            <Get Control=""txtInput"" Property=""Text"" ToState=""inputText""/>
            <Set Control=""lblResult"" Property=""Text"" Value=""You typed: {{inputText}} (Clicks: {{clickCount}})""/>
            <Glow Control=""lblResult"" Color=""#00D4FF"" Intensity=""15"" Animate=""true""/>
        </Handler>
    </Logic>
    
</App>", appName);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Uvel Build System
    /// </summary>
    public class UvelBuildSystem
    {
        public string XmlPath { get; set; }
        public string OutputDirectory { get; set; }
        public string AppName { get; set; }
        public string IconPath { get; set; }
        public bool SingleFile { get; set; }
        public bool ShowConsole { get; set; }
        
        public void Build()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              ���� UVEL BUILD SYSTEM                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            
            DateTime startTime = DateTime.Now;
            
            try
            {
                // Step 1: Validate
                Console.Write("  [1/6] Validating XML...           ");
                ValidateXml();
                PrintSuccess();
                
                // Step 2: Create directories
                Console.Write("  [2/6] Creating directories...     ");
                CreateDirectories();
                PrintSuccess();
                
                // Step 3: Generate code
                Console.Write("  [3/6] Generating code...          ");
                GenerateCode();
                PrintSuccess();
                
                // Step 4: Copy resources
                Console.Write("  [4/6] Copying resources...        ");
                CopyResources();
                PrintSuccess();
                
                // Step 5: Compile
                Console.Write("  [5/6] Compiling...                ");
                Compile();
                PrintSuccess();
                
                // Step 6: Cleanup
                Console.Write("  [6/6] Cleaning up...              ");
                Cleanup();
                PrintSuccess();
                
                // Done
                TimeSpan duration = DateTime.Now - startTime;
                string exePath = Path.Combine(OutputDirectory, AppName + ".exe");
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║              ✅ BUILD SUCCESSFUL                          ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.Write("  📦 Output: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(exePath);
                Console.ResetColor();
                
                if (File.Exists(exePath))
                {
                    FileInfo fi = new FileInfo(exePath);
                    Console.Write("  📊 Size:   ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(FormatFileSize(fi.Length));
                    Console.ResetColor();
                }
                
                Console.Write("  ⏱️  Time:   ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(duration.TotalSeconds.ToString("F2") + "s");
                Console.ResetColor();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗");
                Console.WriteLine();
                Console.WriteLine("  ❌ Build failed: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine();
            }
        }
        
        private void ValidateXml()
        {
            if (!File.Exists(XmlPath))
            {
                throw new FileNotFoundException("XML file not found: " + XmlPath);
            }
            
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(XmlPath);
            
            if (doc.DocumentElement == null)
            {
                throw new Exception("Invalid XML: No root element");
            }
        }
        
        private void CreateDirectories()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }
        
        private void GenerateCode()
        {
            string xmlContent = File.ReadAllText(XmlPath);
            string escapedXml = xmlContent
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", "\\n");
            
            StringBuilder code = new StringBuilder();
            code.AppendLine("// Generated by Uvel Build System");
            code.AppendLine("using System;");
            code.AppendLine("using System.IO;");
            code.AppendLine("using System.Windows;");
            code.AppendLine("");
            code.AppendLine("namespace " + SanitizeName(AppName) + " {");
            code.AppendLine("    public class Program {");
            code.AppendLine("        [STAThread]");
            code.AppendLine("        public static void Main(string[] args) {");
            code.AppendLine("            try {");
            code.AppendLine("                string xmlPath = \"App.xml\";");
            code.AppendLine("                if (!File.Exists(xmlPath)) {");
            code.AppendLine("                    string xml = \"" + escapedXml + "\";");
            code.AppendLine("                    File.WriteAllText(xmlPath, xml);");
            code.AppendLine("                }");
            code.AppendLine("                Uvel.WPF.WpfEngine engine = new Uvel.WPF.WpfEngine();");
            code.AppendLine("                engine.Run(xmlPath);");
            code.AppendLine("            } catch (Exception ex) {");
            code.AppendLine("                MessageBox.Show(ex.Message, \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine("    }");
            code.AppendLine("}");
            
            File.WriteAllText(Path.Combine(OutputDirectory, "Program.cs"), code.ToString());
        }
        
        private void CopyResources()
        {
            // Copy XML
            string destXml = Path.Combine(OutputDirectory, "App.xml");
            File.Copy(XmlPath, destXml, true);
            
            // Copy icon if exists
            if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
            {
                string destIcon = Path.Combine(OutputDirectory, Path.GetFileName(IconPath));
                File.Copy(IconPath, destIcon, true);
            }
            
            // Copy engine files
            string[] engineFiles = { "WpfEngine.cs", "wpfui.cs", "LogicRunner.cs", "XmlParser.cs", "XamlRenderer.cs" };
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            foreach (string file in engineFiles)
            {
                string srcPath = Path.Combine(baseDir, file);
                if (File.Exists(srcPath))
                {
                    File.Copy(srcPath, Path.Combine(OutputDirectory, file), true);
                }
            }
        }
        
        private void Compile()
        {
            string cscPath = FindCsc();
            if (string.IsNullOrEmpty(cscPath))
            {
                throw new Exception("C# compiler not found. Install .NET Framework SDK.");
            }
            
            string outputExe = Path.Combine(OutputDirectory, AppName + ".exe");
            string targetType = ShowConsole ? "exe" : "winexe";
            
            StringBuilder args = new StringBuilder();
            args.Append("/target:" + targetType);
            args.Append(" /out:\"" + outputExe + "\"");
            args.Append(" /optimize+");
            
            // References
            args.Append(" /reference:System.dll");
            args.Append(" /reference:System.Core.dll");
            args.Append(" /reference:System.Xml.dll");
            args.Append(" /reference:System.Xaml.dll");
            args.Append(" /reference:PresentationCore.dll");
            args.Append(" /reference:PresentationFramework.dll");
            args.Append(" /reference:WindowsBase.dll");
            
            // Icon
            if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
            {
                args.Append(" /win32icon:\"" + IconPath + "\"");
            }
            
            // Source files
            string[] sourceFiles = Directory.GetFiles(OutputDirectory, "*.cs");
            foreach (string file in sourceFiles)
            {
                args.Append(" \"" + file + "\"");
            }
            
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = cscPath;
            psi.Arguments = args.ToString();
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.WorkingDirectory = OutputDirectory;
            
            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    throw new Exception("Compilation failed:\n" + output + "\n" + errors);
                }
            }
        }
        
        private void Cleanup()
        {
            // Remove source files
            string[] csFiles = Directory.GetFiles(OutputDirectory, "*.cs");
            foreach (string file in csFiles)
            {
                try { File.Delete(file); } catch { }
            }
        }
        
        private string FindCsc()
        {
            string[] paths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\Framework\v4.0.30319\csc.exe")
            };
            
            foreach (string path in paths)
            {
                if (File.Exists(path)) return path;
            }
            
            return null;
        }
        
        private string SanitizeName(string name)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }
            if (sb.Length == 0 || char.IsDigit(sb[0]))
            {
                sb.Insert(0, '_');
            }
            return sb.ToString();
        }
        
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }
        
        private void PrintSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();
        }
    }
}
