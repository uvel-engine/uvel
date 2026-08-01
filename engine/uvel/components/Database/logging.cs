using System;

namespace Uvel.Library.Components.Database
{
    public static class UvelDatabaseLogging
    {
        public static string Name { get { return "uvel.database.logging"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
