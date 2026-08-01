using System;
namespace Uvel.Library.Components.ListView
{
    public static class UvelListViewAnimation
    {
        public static string Name { get { return "uvel.listview.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
