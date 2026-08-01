using System;
namespace Uvel.Library.Components.Badge
{
    public static class UvelBadgeAnimation
    {
        public static string Name { get { return "uvel.badge.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
