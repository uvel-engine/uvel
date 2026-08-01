using System;
namespace Uvel.Library.Components.Badge
{
    public static class UvelBadgeProtocol
    {
        public static string Name { get { return "uvel.badge.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
