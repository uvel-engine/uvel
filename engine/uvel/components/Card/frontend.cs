using System;

namespace Uvel.Library.Components.Card
{
    public static class UvelCardFrontend
    {
        public static string Name { get { return "uvel.card.frontend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
