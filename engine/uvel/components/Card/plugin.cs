using System;

namespace Uvel.Library.Components.Card
{
    public static class UvelCardPlugin
    {
        public static string Name { get { return "uvel.card.plugin"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
