using System;
namespace Uvel.Library.Components.Text
{
    public static class UvelTextBackend
    {
        public static string Name { get { return "uvel.text.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
