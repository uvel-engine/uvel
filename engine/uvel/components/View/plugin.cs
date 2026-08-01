using System;
namespace Uvel.Library.Components.View
{
    public static class UvelViewPlugin
    {
        public static string Name { get { return "uvel.view.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
