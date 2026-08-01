using System;
namespace Uvel.Library.Components.Scroll
{
    public static class UvelScrollLogging
    {
        public static string Name { get { return "uvel.scroll.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
