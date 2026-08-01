using System;
namespace Uvel.Library.Components.Divider
{
    public static class UvelDividerBackend
    {
        public static string Name { get { return "uvel.divider.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
