using System;
namespace Uvel.Library.Components.Divider
{
    public static class UvelDividerAnimation
    {
        public static string Name { get { return "uvel.divider.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
