using System;

namespace Uvel.Library.Components.Database
{
    public static class UvelDatabaseAnimation
    {
        public static string Name { get { return "uvel.database.animation"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
