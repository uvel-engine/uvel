using System;
namespace Uvel.Library.Components.View
{
    public static class UvelViewAnimation
    {
        public static string Name { get { return "uvel.view.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
