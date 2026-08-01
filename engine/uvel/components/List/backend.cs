using System;
namespace Uvel.Library.Components.List
{
    public static class UvelListBackend
    {
        public static string Name { get { return "uvel.list.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
