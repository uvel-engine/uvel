using System;
namespace Uvel.Library.Components.Text
{
    public static class UvelTextAnimation
    {
        public static string Name { get { return "uvel.text.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
