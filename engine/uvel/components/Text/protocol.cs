using System;
namespace Uvel.Library.Components.Text
{
    public static class UvelTextProtocol
    {
        public static string Name { get { return "uvel.text.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
