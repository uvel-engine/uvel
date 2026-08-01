using System;
namespace Uvel.Library.Components.Checkbox
{
    public static class UvelCheckboxPlugin
    {
        public static string Name { get { return "uvel.checkbox.plugin"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
