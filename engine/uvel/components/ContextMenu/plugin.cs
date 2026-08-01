using System;
namespace Uvel.Library.Components.ContextMenu
{
    public static class UvelContextMenuPlugin
    {
        public static string Name { get { return "uvel.contextmenu.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
