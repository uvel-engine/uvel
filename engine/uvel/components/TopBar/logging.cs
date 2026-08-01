using System;

namespace Uvel.Library.Components.TopBar
{
    public static class UvelTopBarLogging
    {
        public static string Name { get { return "uvel.topbar.logging"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
