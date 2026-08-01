using System;
namespace Uvel.Library.Components.ListView
{
    public static class UvelListViewFrontend
    {
        public static string Name { get { return "uvel.listview.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
