using System;
namespace Uvel.Library.Components.View
{
    public static class UvelViewBackend
    {
        public static string Name { get { return "uvel.view.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
