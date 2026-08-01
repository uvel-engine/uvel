using System;
namespace Uvel.Library.Components.List
{
    public static class UvelListFrontend
    {
        public static string Name { get { return "uvel.list.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
