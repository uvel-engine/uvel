using System;

namespace Uvel.Library.Components.TopBar
{
    public static class UvelTopBarFrontend
    {
        public static string Name { get { return "uvel.topbar.frontend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
