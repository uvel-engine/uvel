using System;

namespace Uvel.Library.Components.Input
{
    public static class UvelInputBackend
    {
        public static string Name { get { return "uvel.input.backend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
