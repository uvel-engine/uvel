using System;
namespace Uvel.Library.Components.List
{
    public static class UvelListPlugin
    {
        public static string Name { get { return "uvel.list.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
