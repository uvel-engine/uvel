using System;
using System.IO;
using System.Text;
using System.Windows;
using Uvel.WPF;

namespace calculator {
    public class Program {
        [STAThread]
        public static void Main(string[] args) {
            try {
                Console.OutputEncoding = Encoding.UTF8;
                string xmlPath = "App.xml";
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDir, xmlPath);
                if (!File.Exists(fullPath)) {
                    fullPath = Path.Combine(Environment.CurrentDirectory, xmlPath);
                }
                if (!File.Exists(fullPath)) {
                    MessageBox.Show("App.xml not found!", "Error");
                    return;
                }
                WpfEngine engine = new WpfEngine();
                engine.Run(fullPath);
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message + "\n\n" + ex.StackTrace, "calculator Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
