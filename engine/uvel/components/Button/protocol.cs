using System;

namespace Uvel.Library.Components.Button
{
    public static class UvelButtonProtocol
    {
        public static string Name { get { return "uvel.button.protocol"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
