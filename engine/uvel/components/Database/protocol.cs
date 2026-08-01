using System;

namespace Uvel.Library.Components.Database
{
    public static class UvelDatabaseProtocol
    {
        public static string Name { get { return "uvel.database.protocol"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
