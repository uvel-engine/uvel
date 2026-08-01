using System;
namespace Uvel.Library.Components.Select
{
    public static class UvelSelectFrontend
    {
        public static string Name { get { return "uvel.select.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
