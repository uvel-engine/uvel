using System;
namespace Uvel.Library.Components.ContextMenu
{
    public static class UvelContextMenuFrontend
    {
        public static string Name { get { return "uvel.contextmenu.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
