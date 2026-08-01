using System;
namespace Uvel.Library.Components.Badge
{
    public static class UvelBadgeBackend
    {
        public static string Name { get { return "uvel.badge.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
