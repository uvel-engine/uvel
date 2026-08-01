using System;
namespace Uvel.Library.Components.View
{
    public static class UvelViewFrontend
    {
        public static string Name { get { return "uvel.view.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
