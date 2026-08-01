using System;

namespace Uvel.Library.Components.Icon
{
    public static class UvelIconFrontend
    {
        public static string Name { get { return "uvel.icon.frontend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
