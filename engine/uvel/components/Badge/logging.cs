using System;
namespace Uvel.Library.Components.Badge
{
    public static class UvelBadgeLogging
    {
        public static string Name { get { return "uvel.badge.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
