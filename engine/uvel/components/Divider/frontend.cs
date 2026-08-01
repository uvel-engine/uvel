using System;
namespace Uvel.Library.Components.Divider
{
    public static class UvelDividerFrontend
    {
        public static string Name { get { return "uvel.divider.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
