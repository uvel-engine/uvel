using System;
namespace Uvel.Library.Components.Select
{
    public static class UvelSelectPlugin
    {
        public static string Name { get { return "uvel.select.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
