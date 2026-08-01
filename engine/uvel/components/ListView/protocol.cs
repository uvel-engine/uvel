using System;
namespace Uvel.Library.Components.ListView
{
    public static class UvelListViewProtocol
    {
        public static string Name { get { return "uvel.listview.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
