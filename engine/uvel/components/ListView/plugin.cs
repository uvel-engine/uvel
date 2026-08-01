using System;
namespace Uvel.Library.Components.ListView
{
    public static class UvelListViewPlugin
    {
        public static string Name { get { return "uvel.listview.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
