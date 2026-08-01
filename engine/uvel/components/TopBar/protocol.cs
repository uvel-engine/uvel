using System;

namespace Uvel.Library.Components.TopBar
{
    public static class UvelTopBarProtocol
    {
        public static string Name { get { return "uvel.topbar.protocol"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
