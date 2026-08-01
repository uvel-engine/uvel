using System;
namespace Uvel.Library.Components.List
{
    public static class UvelListProtocol
    {
        public static string Name { get { return "uvel.list.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
