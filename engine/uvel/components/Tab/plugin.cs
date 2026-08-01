using System;
namespace Uvel.Library.Components.Tab
{
    public static class UvelTabPlugin
    {
        public static string Name { get { return "uvel.tab.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
