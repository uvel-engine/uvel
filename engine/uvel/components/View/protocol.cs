using System;
namespace Uvel.Library.Components.View
{
    public static class UvelViewProtocol
    {
        public static string Name { get { return "uvel.view.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
