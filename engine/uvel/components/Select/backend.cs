using System;
namespace Uvel.Library.Components.Select
{
    public static class UvelSelectBackend
    {
        public static string Name { get { return "uvel.select.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
