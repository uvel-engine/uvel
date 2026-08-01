using System;

namespace Uvel.Library.Components.Card
{
    public static class UvelCardProtocol
    {
        public static string Name { get { return "uvel.card.protocol"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
