using System;
namespace Uvel.Library.Components.ContextMenu
{
    public static class UvelContextMenuAnimation
    {
        public static string Name { get { return "uvel.contextmenu.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
