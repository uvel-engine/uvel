using System;

namespace Uvel.Library.Components.Input
{
    public static class UvelInputAnimation
    {
        public static string Name { get { return "uvel.input.animation"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
