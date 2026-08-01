using System;
namespace Uvel.Library.Components.Scroll
{
    public static class UvelScrollBackend
    {
        public static string Name { get { return "uvel.scroll.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
