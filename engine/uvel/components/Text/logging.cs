using System;
namespace Uvel.Library.Components.Text
{
    public static class UvelTextLogging
    {
        public static string Name { get { return "uvel.text.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
