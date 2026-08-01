using System;
namespace Uvel.Library.Components.Tab
{
    public static class UvelTabProtocol
    {
        public static string Name { get { return "uvel.tab.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
