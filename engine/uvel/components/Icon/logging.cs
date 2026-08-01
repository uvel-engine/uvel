using System;

namespace Uvel.Library.Components.Icon
{
    public static class UvelIconLogging
    {
        public static string Name { get { return "uvel.icon.logging"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
