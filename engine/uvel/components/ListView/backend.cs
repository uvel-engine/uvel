using System;
namespace Uvel.Library.Components.ListView
{
    public static class UvelListViewBackend
    {
        public static string Name { get { return "uvel.listview.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
