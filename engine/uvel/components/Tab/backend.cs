using System;
namespace Uvel.Library.Components.Tab
{
    public static class UvelTabBackend
    {
        public static string Name { get { return "uvel.tab.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
