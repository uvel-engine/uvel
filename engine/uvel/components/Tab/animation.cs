using System;
namespace Uvel.Library.Components.Tab
{
    public static class UvelTabAnimation
    {
        public static string Name { get { return "uvel.tab.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
