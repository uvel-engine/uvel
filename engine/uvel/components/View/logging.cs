using System;
namespace Uvel.Library.Components.View
{
    public static class UvelViewLogging
    {
        public static string Name { get { return "uvel.view.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
