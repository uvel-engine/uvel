using System;
namespace Uvel.Library.Components.List
{
    public static class UvelListAnimation
    {
        public static string Name { get { return "uvel.list.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
