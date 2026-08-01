using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace Uvel.Build
{
    /// <summary>
    /// Uvel Build System - Full build with icon, manifest, resources
    /// </summary>
    public class UvelBuildSystem
    {
        #region Properties
        
        public string XmlPath { get; set; }
        public string OutputDirectory { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string AppAuthor { get; set; }
        public string AppDescription { get; set; }
        public string IconPath { get; set; }
        public bool SingleFile { get; set; }
        public bool ShowConsole { get; set; }
        public bool IncludeDebugInfo { get; set; }
        public List<string> AdditionalFiles { get; set; }
        public List<string> References { get; set; }
        
        private XmlDocument _appDoc;
        
        #endregion
        
        #region Constructor
        
        public UvelBuildSystem()
        {
            OutputDirectory = "./build";
            AppVersion = "1.0.0";
            AppAuthor = "";
            AppDescription = "";
            SingleFile = false;
            ShowConsole = false;
            IncludeDebugInfo = false;
            AdditionalFiles = new List<string>();
            References = new List<string>
            {
                "System.dll",
                "System.Core.dll",
                "System.Xml.dll",
                "System.Xaml.dll",
                "System.Windows.Forms.dll",
                "System.Drawing.dll",
                "PresentationCore.dll",
                "PresentationFramework.dll",
                "WindowsBase.dll"
            };
        }
        
        #endregion
        
        #region Build Methods
        
        /// <summary>
        /// Full build process
        /// </summary>
        public BuildResult Build()
        {
            BuildResult result = new BuildResult();
            result.StartTime = DateTime.Now;
            
            try
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════════════════════╗");
                Console.WriteLine("║           🔨 UVEL BUILD SYSTEM                     ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                
                // Step 1: Validate
                Console.Write("  [1/7] Validating XML...           ");
                ValidateXml();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                // Step 2: Parse app info
                Console.Write("  [2/7] Parsing application...      ");
                ParseAppInfo();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                // Step 3: Create output directory
                Console.Write("  [3/7] Creating directories...     ");
                CreateDirectories();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                // Step 4: Generate code
                Console.Write("  [4/7] Generating code...          ");
                string generatedCode = GenerateCode();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                // Step 5: Process resources
                Console.Write("  [5/7] Processing resources...     ");
                ProcessResources();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                // Step 6: Compile
                Console.Write("  [6/7] Compiling...                ");
                Compile(generatedCode);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                // Step 7: Package
                Console.Write("  [7/7] Packaging...                ");
                Package();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓");
                Console.ResetColor();
                
                result.Success = true;
                result.OutputPath = Path.Combine(OutputDirectory, AppName + ".exe");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗");
                Console.WriteLine();
                Console.WriteLine("  ❌ Build failed: " + ex.Message);
                Console.ResetColor();
                
                result.Success = false;
                result.Error = ex.Message;
            }
            
            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            
            // Print summary
            PrintBuildSummary(result);
            
            return result;
        }
        
        #endregion
        
        #region Step 1: Validate
        
        private void ValidateXml()
        {
            if (!File.Exists(XmlPath))
            {
                throw new FileNotFoundException("XML file not found: " + XmlPath);
            }
            
            _appDoc = new XmlDocument();
            _appDoc.Load(XmlPath);
            
            XmlNode root = _appDoc.DocumentElement;
            if (root == null)
            {
                throw new Exception("Invalid XML: No root element");
            }
            
            XmlNode uiNode = root.SelectSingleNode("UI");
            if (uiNode == null)
            {
                throw new Exception("Invalid Uvel XML: No <UI> section found");
            }
        }
        
        #endregion
        
        #region Step 2: Parse App Info
        
        private void ParseAppInfo()
        {
            XmlNode root = _appDoc.DocumentElement;
            
            if (string.IsNullOrEmpty(AppName))
            {
                AppName = GetAttribute(root, "Name", Path.GetFileNameWithoutExtension(XmlPath));
            }
            
            if (string.IsNullOrEmpty(AppVersion))
            {
                AppVersion = GetAttribute(root, "Version", "1.0.0");
            }
            
            if (string.IsNullOrEmpty(AppAuthor))
            {
                AppAuthor = GetAttribute(root, "Author", "");
            }
            
            if (string.IsNullOrEmpty(AppDescription))
            {
                AppDescription = GetAttribute(root, "Description", "Built with Uvel Framework");
            }
            
            // Check for Icon attribute in XML
            if (string.IsNullOrEmpty(IconPath))
            {
                string xmlIcon = GetAttribute(root, "Icon", "");
                if (!string.IsNullOrEmpty(xmlIcon))
                {
                    string xmlDir = Path.GetDirectoryName(XmlPath);
                    string iconFullPath = Path.Combine(xmlDir, xmlIcon);
                    if (File.Exists(iconFullPath))
                    {
                        IconPath = iconFullPath;
                    }
                }
            }
        }
        
        #endregion
        
        #region Step 3: Create Directories
        
        private void CreateDirectories()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            
            string libDir = Path.Combine(OutputDirectory, "lib");
            if (!Directory.Exists(libDir))
            {
                Directory.CreateDirectory(libDir);
            }
            
            string assetsDir = Path.Combine(OutputDirectory, "assets");
            if (!Directory.Exists(assetsDir))
            {
                Directory.CreateDirectory(assetsDir);
            }
        }
        
        #endregion
        
        #region Step 4: Generate Code
        
        private string GenerateCode()
        {
            string xmlContent = File.ReadAllText(XmlPath);
            string escapedXml = xmlContent.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
            
            StringBuilder code = new StringBuilder();
            
            code.AppendLine("// Generated by Uvel Build System");
            code.AppendLine("// " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            code.AppendLine();
            code.AppendLine("using System;");
            code.AppendLine("using System.IO;");
            code.AppendLine("using System.Windows;");
            code.AppendLine("using System.Reflection;");
            code.AppendLine();
            code.AppendLine("namespace " + SanitizeName(AppName));
            code.AppendLine("{");
            code.AppendLine("    public class Program");
            code.AppendLine("    {");
            code.AppendLine("        [STAThread]");
            code.AppendLine("        public static void Main(string[] args)");
            code.AppendLine("        {");
            code.AppendLine("            try");
            code.AppendLine("            {");
            code.AppendLine("                // Get embedded XML or external file");
            code.AppendLine("                string xmlPath = \"app.xml\";");
            code.AppendLine("                string xmlContent = null;");
            code.AppendLine();
            code.AppendLine("                if (File.Exists(xmlPath))");
            code.AppendLine("                {");
            code.AppendLine("                    xmlContent = File.ReadAllText(xmlPath);");
            code.AppendLine("                }");
            code.AppendLine("                else");
            code.AppendLine("                {");
            code.AppendLine("                    // Use embedded XML");
            code.AppendLine("                    xmlContent = \"" + escapedXml + "\";");
            code.AppendLine("                    File.WriteAllText(xmlPath, xmlContent);");
            code.AppendLine("                }");
            code.AppendLine();
            code.AppendLine("                // Run engine");
            code.AppendLine("                Uvel.WPF.WpfEngine engine = new Uvel.WPF.WpfEngine();");
            code.AppendLine("                engine.Run(xmlPath);");
            code.AppendLine("            }");
            code.AppendLine("            catch (Exception ex)");
            code.AppendLine("            {");
            code.AppendLine("                MessageBox.Show(\"Error: \" + ex.Message + \"\\n\\n\" + ex.StackTrace,");
            code.AppendLine("                    \"" + AppName + " Error\", MessageBoxButton.OK, MessageBoxImage.Error);");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine("    }");
            code.AppendLine("}");
            
            string codePath = Path.Combine(OutputDirectory, "Program.cs");
            File.WriteAllText(codePath, code.ToString());
            
            return code.ToString();
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
        
        #endregion
        
        #region Step 5: Process Resources
        
        private void ProcessResources()
        {
            string xmlDir = Path.GetDirectoryName(XmlPath);
            
            // Copy XML file
            string destXml = Path.Combine(OutputDirectory, "app.xml");
            File.Copy(XmlPath, destXml, true);
            
            // Copy icon if exists
            if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
            {
                string destIcon = Path.Combine(OutputDirectory, Path.GetFileName(IconPath));
                File.Copy(IconPath, destIcon, true);
            }
            
            // Copy assets folder if exists
            string assetsSource = Path.Combine(xmlDir, "assets");
            if (Directory.Exists(assetsSource))
            {
                string assetsDest = Path.Combine(OutputDirectory, "assets");
                CopyDirectory(assetsSource, assetsDest);
            }
            
            // Copy additional files
            foreach (string file in AdditionalFiles)
            {
                if (File.Exists(file))
                {
                    string destFile = Path.Combine(OutputDirectory, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
            }
            
            // Generate app manifest
            GenerateManifest();
            
            // Generate app.config
            GenerateAppConfig();
        }
        
        private void CopyDirectory(string source, string dest)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
            
            foreach (string file in Directory.GetFiles(source))
            {
                string destFile = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            
            foreach (string dir in Directory.GetDirectories(source))
            {
                string destDir = Path.Combine(dest, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }
        
        private void GenerateManifest()
        {
            string manifest = string.Format(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0"">
  <assemblyIdentity
    version=""{0}.0""
    processorArchitecture=""*""
    name=""{1}""
    type=""win32""/>
  <description>{2}</description>
  <dependency>
    <dependentAssembly>
      <assemblyIdentity
        type=""win32""
        name=""Microsoft.Windows.Common-Controls""
        version=""6.0.0.0""
        processorArchitecture=""*""
        publicKeyToken=""6595b64144ccf1df""
        language=""*""/>
    </dependentAssembly>
  </dependency>
  <trustInfo xmlns=""urn:schemas-microsoft-com:asm.v3"">
    <security>
      <requestedPrivileges>
        <requestedExecutionLevel level=""asInvoker"" uiAccess=""false""/>
      </requestedPrivileges>
    </security>
  </trustInfo>
  <compatibility xmlns=""urn:schemas-microsoft-com:compatibility.v1"">
    <application>
      <supportedOS Id=""{{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}}""/>
      <supportedOS Id=""{{1f676c76-80e1-4239-95bb-83d0f6d0da78}}""/>
      <supportedOS Id=""{{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}}""/>
      <supportedOS Id=""{{35138b9a-5d96-4fbd-8e2d-a2440225f93a}}""/>
    </application>
  </compatibility>
  <application xmlns=""urn:schemas-microsoft-com:asm.v3"">
    <windowsSettings>
      <dpiAware xmlns=""http://schemas.microsoft.com/SMI/2005/WindowsSettings"">true/pm</dpiAware>
      <dpiAwareness xmlns=""http://schemas.microsoft.com/SMI/2016/WindowsSettings"">permonitorv2,permonitor</dpiAwareness>
    </windowsSettings>
  </application>
</assembly>", AppVersion, SanitizeName(AppName), AppDescription);
            
            string manifestPath = Path.Combine(OutputDirectory, AppName + ".exe.manifest");
            File.WriteAllText(manifestPath, manifest);
        }
        
        private void GenerateAppConfig()
        {
            string config = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <startup>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.0""/>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.5""/>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.8""/>
  </startup>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <probing privatePath=""lib""/>
    </assemblyBinding>
  </runtime>
</configuration>");
            
            string configPath = Path.Combine(OutputDirectory, AppName + ".exe.config");
            File.WriteAllText(configPath, config);
        }
        
        #endregion
        
        #region Step 6: Compile
        
        private void Compile(string code)
        {
            // Find csc.exe
            string cscPath = FindCsc();
            
            if (string.IsNullOrEmpty(cscPath))
            {
                throw new Exception("C# compiler (csc.exe) not found. Make sure .NET Framework is installed.");
            }
            
            string outputExe = Path.Combine(OutputDirectory, AppName + ".exe");
            string sourceFile = Path.Combine(OutputDirectory, "Program.cs");
            string targetType = ShowConsole ? "exe" : "winexe";
            
            // Build compiler arguments
            StringBuilder args = new StringBuilder();
            args.Append("/target:" + targetType);
            args.Append(" /out:\"" + outputExe + "\"");
            args.Append(" /optimize+");
            
            if (!IncludeDebugInfo)
            {
                args.Append(" /debug-");
            }
            else
            {
                args.Append(" /debug+");
            }
            
            // Add references
            foreach (string reference in References)
            {
                args.Append(" /reference:" + reference);
            }
            
            // Add icon
            if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
            {
                args.Append(" /win32icon:\"" + IconPath + "\"");
            }
            
            // Add manifest
            string manifestPath = Path.Combine(OutputDirectory, AppName + ".exe.manifest");
            if (File.Exists(manifestPath))
            {
                args.Append(" /win32manifest:\"" + manifestPath + "\"");
            }
            
            // Add source files
            args.Append(" \"" + sourceFile + "\"");
            
            // Add engine files
            string engineDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] engineFiles = new string[]
            {
                "WpfEngine.cs",
                "WpfUI.cs",
                "LogicRunner.cs",
                "LogicRunnerAdvanced.cs",
                "XmlParser.cs",
                "XamlRenderer.cs"
            };
            
            foreach (string engineFile in engineFiles)
            {
                string fullPath = Path.Combine(engineDir, engineFile);
                if (File.Exists(fullPath))
                {
                    string destPath = Path.Combine(OutputDirectory, engineFile);
                    File.Copy(fullPath, destPath, true);
                    args.Append(" \"" + destPath + "\"");
                }
            }
            
            // Run compiler
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
        
        private string FindCsc()
        {
            // Try .NET Framework paths
            string[] frameworkPaths = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\Framework\v4.0.30319\csc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\Framework64\v3.5\csc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    @"Microsoft.NET\Framework\v3.5\csc.exe")
            };
            
            foreach (string path in frameworkPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Step 7: Package
        
        private void Package()
        {
            // Clean up temporary files
            string[] tempFiles = new string[]
            {
                Path.Combine(OutputDirectory, "Program.cs"),
                Path.Combine(OutputDirectory, "WpfEngine.cs"),
                Path.Combine(OutputDirectory, "WpfUI.cs"),
                Path.Combine(OutputDirectory, "LogicRunner.cs"),
                Path.Combine(OutputDirectory, "LogicRunnerAdvanced.cs"),
                Path.Combine(OutputDirectory, "XmlParser.cs"),
                Path.Combine(OutputDirectory, "XamlRenderer.cs")
            };
            
            foreach (string file in tempFiles)
            {
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { }
                }
            }
            
            // Create ZIP archive if single file requested
            if (SingleFile)
            {
                string zipPath = Path.Combine(Path.GetDirectoryName(OutputDirectory), AppName + ".zip");
                
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                ZipFile.CreateFromDirectory(OutputDirectory, zipPath);
            }
        }
        
        #endregion
        
        #region Summary
        
        private void PrintBuildSummary(BuildResult result)
        {
            Console.WriteLine();
            
            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("╔══════════════════════════════════════════════════════╗");
                Console.WriteLine("║             ✅ BUILD SUCCESSFUL                      ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  📦 Output: " + result.OutputPath);
                
                if (File.Exists(result.OutputPath))
                {
                    FileInfo fi = new FileInfo(result.OutputPath);
                    Console.WriteLine("  📊 Size:   " + FormatFileSize(fi.Length));
                }
                
                Console.WriteLine("  ⏱️  Time:   " + result.Duration.TotalSeconds.ToString("F2") + "s");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("╔══════════════════════════════════════════════════════╗");
                Console.WriteLine("║             ❌ BUILD FAILED                          ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  Error: " + result.Error);
            }
            
            Console.WriteLine();
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
        
        #endregion
        
        #region Helpers
        
        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node == null || node.Attributes == null) return defaultValue;
            XmlAttribute attr = node.Attributes[name];
            return attr != null ? attr.Value : defaultValue;
        }
        
        #endregion
    }
    
    #region Build Result
    
    public class BuildResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public string Error { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
    
    #endregion
}
