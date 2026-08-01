using System;
namespace Uvel.Library.Components.Scroll
{
    public static class UvelScrollAnimation
    {
        public static string Name { get { return "uvel.scroll.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
