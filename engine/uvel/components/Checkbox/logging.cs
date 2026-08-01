using System;
namespace Uvel.Library.Components.Checkbox
{
    public static class UvelCheckboxLogging
    {
        public static string Name { get { return "uvel.checkbox.logging"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
