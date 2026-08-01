using System;

namespace Uvel.Library.Components.TopBar
{
    public static class UvelTopBarAnimation
    {
        public static string Name { get { return "uvel.topbar.animation"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
