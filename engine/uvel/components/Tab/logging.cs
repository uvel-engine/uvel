using System;
namespace Uvel.Library.Components.Tab
{
    public static class UvelTabLogging
    {
        public static string Name { get { return "uvel.tab.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
