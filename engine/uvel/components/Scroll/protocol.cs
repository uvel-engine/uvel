using System;
namespace Uvel.Library.Components.Scroll
{
    public static class UvelScrollProtocol
    {
        public static string Name { get { return "uvel.scroll.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
