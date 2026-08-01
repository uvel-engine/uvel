using System;
namespace Uvel.Library.Components.Select
{
    public static class UvelSelectAnimation
    {
        public static string Name { get { return "uvel.select.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
