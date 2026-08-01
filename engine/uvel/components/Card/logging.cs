using System;

namespace Uvel.Library.Components.Card
{
    public static class UvelCardLogging
    {
        public static string Name { get { return "uvel.card.logging"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
