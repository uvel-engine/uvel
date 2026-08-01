using System;
namespace Uvel.Library.Components.Select
{
    public static class UvelSelectProtocol
    {
        public static string Name { get { return "uvel.select.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
