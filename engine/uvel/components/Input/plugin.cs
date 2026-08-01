using System;

namespace Uvel.Library.Components.Input
{
    public static class UvelInputPlugin
    {
        public static string Name { get { return "uvel.input.plugin"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
