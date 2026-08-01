using System;

namespace Uvel.Library.Components.Database
{
    public static class UvelDatabaseFrontend
    {
        public static string Name { get { return "uvel.database.frontend"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
