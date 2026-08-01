using System;

namespace Uvel.Library.Components.Icon
{
    public static class UvelIconPlugin
    {
        public static string Name { get { return "uvel.icon.plugin"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
