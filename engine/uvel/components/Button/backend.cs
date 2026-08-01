using System;

namespace Uvel.Library.Components.Button
{
    public static class UvelButtonBackend
    {
        public static string Name { get { return "uvel.button.backend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
