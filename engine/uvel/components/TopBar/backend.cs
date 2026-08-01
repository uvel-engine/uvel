using System;

namespace Uvel.Library.Components.TopBar
{
    public static class UvelTopBarBackend
    {
        public static string Name { get { return "uvel.topbar.backend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
