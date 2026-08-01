using System;
namespace Uvel.Library.Components.Select
{
    public static class UvelSelectLogging
    {
        public static string Name { get { return "uvel.select.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
