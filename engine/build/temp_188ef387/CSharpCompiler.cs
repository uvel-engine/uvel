using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.CSharp;

namespace Uvel.WPF
{
    /// <summary>
    /// Advanced, highly robust, and .NET 4.0 native C# Compiler for Uvel Engine.
    /// Fully compatible with Windows 8 and older C# compilers.
    /// Does not use modern C# 6.0+ features like string interpolation ($) or expression bodies (=>).
    /// </summary>
    public class CSharpCompiler
    {
        private WpfEngine _engine;
        private Dictionary<string, Assembly> _compiledAssemblies;
        private Dictionary<string, Dictionary<string, MethodInfo>> _methodCache;

        public CSharpCompiler(WpfEngine engine)
        {
            _engine = engine;
            _compiledAssemblies = new Dictionary<string, Assembly>();
            _methodCache = new Dictionary<string, Dictionary<string, MethodInfo>>();
        }

        /// <summary>
        /// Console Logger with colors for easy debugging. Strictly compatible with .NET 4.0.
        /// </summary>
        private void PrintLog(string level, string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            
            if (level == "ERROR" || level == "FATAL") 
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (level == "WARN") 
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (level == "SUCCESS") 
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (level == "DEBUG") 
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else 
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            string formattedMessage = string.Format("[{0}] [{1}] {2}", timeStamp, level, message);
            
            Console.WriteLine(formattedMessage);
            Console.ForegroundColor = originalColor;
            
            if (_engine != null)
            {
                _engine.Log(level, formattedMessage);
            }
        }

        /// <summary>
        /// Compiles the raw XML node containing C# code.
        /// </summary>
        public bool Compile(XmlNode manualCNode)
        {
            string id = GetAttribute(manualCNode, "id", "");
            if (string.IsNullOrEmpty(id)) id = GetAttribute(manualCNode, "Id", "");
            if (string.IsNullOrEmpty(id)) id = GetAttribute(manualCNode, "ID", "");

            if (string.IsNullOrEmpty(id))
            {
                PrintLog("ERROR", "ManualC: Missing 'id' attribute in XML node.");
                return false;
            }

            string code = ExtractCodeFromNode(manualCNode);

            if (string.IsNullOrEmpty(code))
            {
                PrintLog("WARN", string.Format("ManualC [{0}]: Code block is completely empty.", id));
                return false;
            }

            // XML decode safety (Classic approach)
            code = code.Replace("&lt;", "<");
            code = code.Replace("&gt;", ">");
            code = code.Replace("&amp;", "&");
            code = code.Replace("&quot;", "\"");
            code = code.Replace("&apos;", "'");

            string refs = GetAttribute(manualCNode, "references", "");

            // Auto-wrap code into a class if the user just wrote raw methods
            if (!code.Contains("class "))
            {
                code = WrapCode(id, code);
            }

            PrintLog("DEBUG", string.Format("ManualC: Compiling module '{0}'...", id));

            try
            {
                Assembly assembly = CompileCodeToAssembly(id, code, refs);
                if (assembly != null)
                {
                    _compiledAssemblies[id] = assembly;
                    CacheMethods(id, assembly);
                    PrintLog("SUCCESS", string.Format("ManualC '{0}': Successfully compiled and cached.", id));
                    return true;
                }
            }
            catch (Exception ex)
            {
                PrintLog("ERROR", string.Format("ManualC '{0}' Compilation Crash: {1}", id, ex.Message));
            }

            return false;
        }

        /// <summary>
        /// Safely extracts code from CDATA or InnerText.
        /// </summary>
        private string ExtractCodeFromNode(XmlNode node)
        {
            if (node == null || node.ChildNodes == null) return string.Empty;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.CDATA)
                {
                    if (child.Value != null)
                    {
                        return child.Value.Trim();
                    }
                }
            }
            
            if (node.InnerText != null)
            {
                return node.InnerText.Trim();
            }
            
            return string.Empty;
        }

        /// <summary>
        /// Caches the public static methods found in the compiled assembly.
        /// </summary>
        private void CacheMethods(string id, Assembly assembly)
        {
            Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
            
            Type[] assemblyTypes = assembly.GetTypes();
            
            foreach (Type t in assemblyTypes)
            {
                MethodInfo[] allMethods = t.GetMethods(BindingFlags.Public | BindingFlags.Static);
                
                foreach (MethodInfo mi in allMethods)
                {
                    if (mi.DeclaringType != null && mi.DeclaringType.Namespace != null && !mi.DeclaringType.Namespace.StartsWith("System"))
                    {
                        methods[mi.Name] = mi;
                        PrintLog("DEBUG", string.Format("  -> Discovered Method: {0} (Takes {1} params)", mi.Name, mi.GetParameters().Length));
                    }
                }
            }

            if (methods.Count > 0)
            {
                _methodCache[id] = methods;
            }
            else
            {
                PrintLog("WARN", string.Format("ManualC '{0}': No 'public static' methods were found to cache.", id));
            }
        }

        /// <summary>
        /// Wraps raw method strings into a complete valid C# class structure.
        /// </summary>
        private string WrapCode(string id, string code)
        {
            string safeId = SanitizeId(id);
            List<string> usingLines = new List<string>();
            List<string> bodyLines = new List<string>();

            string[] separators = new string[] { "\n", "\r\n" };
            string[] lines = code.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
                {
                    usingLines.Add(trimmed);
                }
                else
                {
                    bodyLines.Add(line);
                }
            }

            StringBuilder sb = new StringBuilder();
            
            // Standard Native .NET Usings
            string[] defaultUsings = new string[] 
            {
                "using System;", 
                "using System.IO;", 
                "using System.Text;", 
                "using System.Collections.Generic;", 
                "using System.Windows;", 
                "using System.Windows.Controls;",
                "using System.Windows.Media;", 
                "using System.Windows.Threading;"
            };

            foreach (string dUsing in defaultUsings)
            {
                if (!usingLines.Contains(dUsing)) 
                {
                    sb.AppendLine(dUsing);
                }
            }

            foreach (string u in usingLines) 
            {
                sb.AppendLine(u);
            }

            sb.AppendLine();
            sb.AppendLine("namespace Uvel.ManualC.Generated");
            sb.AppendLine("{");
            sb.AppendLine(string.Format("    public static class {0}", safeId));
            sb.AppendLine("    {");

            string body = string.Join("\n", bodyLines.ToArray());
            
            if (body.Contains("public static"))
            {
                sb.AppendLine("        " + body.Replace("\n", "\n        "));
            }
            else
            {
                sb.AppendLine("        public static object Run()");
                sb.AppendLine("        {");
                sb.AppendLine("            " + body.Replace("\n", "\n            "));
                sb.AppendLine("            return null;");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// The Core Compiler Engine. Loads required Native .NET DLLs.
        /// </summary>
        private Assembly CompileCodeToAssembly(string id, string code, string additionalRefs)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.TreatWarningsAsErrors = false;

            // Load Basic .NET assemblies
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

            // Locate correct Native runtime directory (Important for Windows 8)
            string runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            string wpfPath = Path.Combine(runtimeDir, "WPF");

            // Load WPF Native Libraries
            string[] wpfDlls = new string[] { "WindowsBase.dll", "PresentationCore.dll", "PresentationFramework.dll" };
            foreach (string dll in wpfDlls)
            {
                string path1 = Path.Combine(wpfPath, dll);
                string path2 = Path.Combine(runtimeDir, dll);
                
                if (File.Exists(path1)) 
                {
                    parameters.ReferencedAssemblies.Add(path1);
                }
                else if (File.Exists(path2)) 
                {
                    parameters.ReferencedAssemblies.Add(path2);
                }
            }

            string xamlPath = Path.Combine(runtimeDir, "System.Xaml.dll");
            if (File.Exists(xamlPath)) 
            {
                parameters.ReferencedAssemblies.Add(xamlPath);
            }

            // Reference current running assembly
            string selfPath = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(selfPath) && File.Exists(selfPath))
            {
                parameters.ReferencedAssemblies.Add(selfPath);
            }

            // Add additional user references
            if (!string.IsNullOrEmpty(additionalRefs))
            {
                string[] refArray = additionalRefs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string r in refArray)
                {
                    string trimmed = r.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        parameters.ReferencedAssemblies.Add(trimmed);
                    }
                }
            }

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder();
                errors.AppendLine(string.Format("COMPILATION FAILED for '{0}':", id));
                
                foreach (CompilerError error in results.Errors)
                {
                    if (!error.IsWarning)
                    {
                        errors.AppendLine(string.Format("  -> [Line {0}]: {1}", error.Line, error.ErrorText));
                    }
                }
                
                PrintLog("ERROR", errors.ToString());
                return null;
            }

            return results.CompiledAssembly;
        }

        /// <summary>
        /// Executes a specified method in the compiled code safely.
        /// </summary>
        public object ExecuteMethod(string id, string methodName, params object[] args)
        {
            if (!_methodCache.ContainsKey(id))
            {
                PrintLog("ERROR", string.Format("Execution Failed: ManualC module '{0}' does not exist or failed to compile.", id));
                return null;
            }

            Dictionary<string, MethodInfo> methods = _methodCache[id];
            MethodInfo method = null;

            if (!string.IsNullOrEmpty(methodName) && methods.ContainsKey(methodName))
            {
                method = methods[methodName];
            }
            else if (string.IsNullOrEmpty(methodName) && methods.ContainsKey("Run"))
            {
                method = methods["Run"];
            }

            if (method == null)
            {
                PrintLog("ERROR", string.Format("Execution Failed: Method '{0}' not found in module '{1}'.", methodName ?? "Run", id));
                return null;
            }

            ParameterInfo[] paramInfos = method.GetParameters();
            
            if (args == null)
            {
                args = new object[0];
            }

            object[] convertedArgs = new object[paramInfos.Length];

            // Ultra-robust parameter type casting (Strictly .NET 4.0 compatible)
            for (int i = 0; i < paramInfos.Length; i++)
            {
                Type targetType = paramInfos[i].ParameterType;

                if (i < args.Length && args[i] != null)
                {
                    try
                    {
                        // Special conversions
                        if (targetType == typeof(string))
                        {
                            convertedArgs[i] = args[i].ToString();
                        }
                        else if (targetType == typeof(int))
                        {
                            convertedArgs[i] = Convert.ToInt32(args[i]);
                        }
                        else if (targetType == typeof(double))
                        {
                            convertedArgs[i] = Convert.ToDouble(args[i], System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (targetType == typeof(bool))
                        {
                            string strVal = args[i].ToString().ToLower();
                            convertedArgs[i] = (strVal == "true" || strVal == "1");
                        }
                        else
                        {
                            convertedArgs[i] = Convert.ChangeType(args[i], targetType, System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintLog("WARN", string.Format("Failed to cast param '{0}' to {1}. Using default. Error: {2}", paramInfos[i].Name, targetType.Name, ex.Message));
                        
                        if (targetType.IsValueType)
                            convertedArgs[i] = Activator.CreateInstance(targetType);
                        else
                            convertedArgs[i] = null;
                    }
                }
                else
                {
                    if (paramInfos[i].IsOptional)
                    {
                        convertedArgs[i] = paramInfos[i].DefaultValue;
                    }
                    else
                    {
                        if (targetType.IsValueType)
                            convertedArgs[i] = Activator.CreateInstance(targetType);
                        else
                            convertedArgs[i] = null;
                    }
                }
            }

            // Invoke Method Safely
            try
            {
                PrintLog("DEBUG", string.Format("Invoking '{0}.{1}' with {2} arguments...", id, method.Name, args.Length));
                
                object result = method.Invoke(null, convertedArgs);
                
                string resultString = "NULL";
                if (result != null) resultString = result.ToString();
                
                PrintLog("SUCCESS", string.Format("Execution '{0}.{1}' completed. Returned: {2}", id, method.Name, resultString));
                return result;
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg = ex.InnerException.Message;
                }
                
                PrintLog("ERROR", string.Format("Runtime Crash in '{0}.{1}': {2}", id, method.Name, errorMsg));
                return null;
            }
        }

        public bool HasModule(string id)
        {
            return _methodCache.ContainsKey(id);
        }

        public bool HasMethod(string id, string methodName)
        {
            if (!_methodCache.ContainsKey(id)) return false;
            return _methodCache[id].ContainsKey(methodName);
        }

        public List<string> GetMethods(string id)
        {
            List<string> result = new List<string>();
            if (_methodCache.ContainsKey(id))
            {
                foreach (string key in _methodCache[id].Keys)
                {
                    result.Add(key);
                }
            }
            return result;
        }

        private string SanitizeId(string id)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in id.ToCharArray())
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }
            string clean = sb.ToString();
            if (clean.Length > 0 && char.IsDigit(clean[0]))
            {
                clean = "_" + clean;
            }
            return clean;
        }

        private string GetAttribute(XmlNode node, string name, string defaultValue)
        {
            if (node != null && node.Attributes != null && node.Attributes[name] != null)
            {
                return node.Attributes[name].Value;
            }
            return defaultValue;
        }
    }
}