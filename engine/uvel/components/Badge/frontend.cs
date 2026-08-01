using System;
namespace Uvel.Library.Components.Badge
{
    public static class UvelBadgeFrontend
    {
        public static string Name { get { return "uvel.badge.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
