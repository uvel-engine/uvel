using System;
namespace Uvel.Library.Components.Scroll
{
    public static class UvelScrollFrontend
    {
        public static string Name { get { return "uvel.scroll.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
