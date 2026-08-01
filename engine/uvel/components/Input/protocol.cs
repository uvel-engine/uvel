using System;

namespace Uvel.Library.Components.Input
{
    public static class UvelInputProtocol
    {
        public static string Name { get { return "uvel.input.protocol"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
