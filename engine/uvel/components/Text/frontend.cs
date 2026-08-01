using System;
namespace Uvel.Library.Components.Text
{
    public static class UvelTextFrontend
    {
        public static string Name { get { return "uvel.text.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
