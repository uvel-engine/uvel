using System;
namespace Uvel.Library.Components.List
{
    public static class UvelListLogging
    {
        public static string Name { get { return "uvel.list.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
