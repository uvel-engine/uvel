using System;
namespace Uvel.Library.Components.ContextMenu
{
    public static class UvelContextMenuLogging
    {
        public static string Name { get { return "uvel.contextmenu.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
