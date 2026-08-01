using System;

namespace Uvel.Library.Components.Input
{
    public static class UvelInputFrontend
    {
        public static string Name { get { return "uvel.input.frontend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
