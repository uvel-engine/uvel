using System;
namespace Uvel.Library.Components.ContextMenu
{
    public static class UvelContextMenuBackend
    {
        public static string Name { get { return "uvel.contextmenu.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
