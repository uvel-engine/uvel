using System;

namespace Uvel.Library.Components.Button
{
    public static class UvelButtonFrontend
    {
        public static string Name { get { return "uvel.button.frontend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
