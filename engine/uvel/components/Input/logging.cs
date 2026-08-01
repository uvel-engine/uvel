using System;

namespace Uvel.Library.Components.Input
{
    public static class UvelInputLogging
    {
        public static string Name { get { return "uvel.input.logging"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
