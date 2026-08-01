using System;
namespace Uvel.Library.Components.Tab
{
    public static class UvelTabFrontend
    {
        public static string Name { get { return "uvel.tab.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
