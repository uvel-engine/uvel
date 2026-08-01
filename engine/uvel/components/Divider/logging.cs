using System;
namespace Uvel.Library.Components.Divider
{
    public static class UvelDividerLogging
    {
        public static string Name { get { return "uvel.divider.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
