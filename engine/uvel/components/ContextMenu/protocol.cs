using System;
namespace Uvel.Library.Components.ContextMenu
{
    public static class UvelContextMenuProtocol
    {
        public static string Name { get { return "uvel.contextmenu.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
