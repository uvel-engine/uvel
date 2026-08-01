using System;

namespace Uvel.Library.Components.Database
{
    public static class UvelDatabaseBackend
    {
        public static string Name { get { return "uvel.database.backend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
