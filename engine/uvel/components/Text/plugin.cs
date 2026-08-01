using System;
namespace Uvel.Library.Components.Text
{
    public static class UvelTextPlugin
    {
        public static string Name { get { return "uvel.text.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
