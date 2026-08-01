using System;
namespace Uvel.Library.Components.Divider
{
    public static class UvelDividerProtocol
    {
        public static string Name { get { return "uvel.divider.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
