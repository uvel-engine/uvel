using System;

namespace Uvel.Library.Components.Button
{
    public static class UvelButtonLogging
    {
        public static string Name { get { return "uvel.button.logging"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
