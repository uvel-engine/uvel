using System;
namespace Uvel.Library.Components.ListView
{
    public static class UvelListViewLogging
    {
        public static string Name { get { return "uvel.listview.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
