using System;

namespace Uvel.Library.Components.TopBar
{
    public static class UvelTopBarPlugin
    {
        public static string Name { get { return "uvel.topbar.plugin"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
