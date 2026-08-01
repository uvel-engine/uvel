using System;

namespace Uvel.Library.Components.Button
{
    public static class UvelButtonPlugin
    {
        public static string Name { get { return "uvel.button.plugin"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
