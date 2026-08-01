using System;
namespace Uvel.Library.Components.Divider
{
    public static class UvelDividerPlugin
    {
        public static string Name { get { return "uvel.divider.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
