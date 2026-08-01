using System;
namespace Uvel.Library.Components.Badge
{
    public static class UvelBadgePlugin
    {
        public static string Name { get { return "uvel.badge.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
