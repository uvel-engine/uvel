using System;
namespace Uvel.Library.Components.Scroll
{
    public static class UvelScrollPlugin
    {
        public static string Name { get { return "uvel.scroll.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
