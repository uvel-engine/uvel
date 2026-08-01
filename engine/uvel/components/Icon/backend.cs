using System;

namespace Uvel.Library.Components.Icon
{
    public static class UvelIconBackend
    {
        public static string Name { get { return "uvel.icon.backend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
