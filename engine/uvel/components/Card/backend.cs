using System;

namespace Uvel.Library.Components.Card
{
    public static class UvelCardBackend
    {
        public static string Name { get { return "uvel.card.backend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
