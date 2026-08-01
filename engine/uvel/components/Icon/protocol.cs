using System;

namespace Uvel.Library.Components.Icon
{
    public static class UvelIconProtocol
    {
        public static string Name { get { return "uvel.icon.protocol"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
