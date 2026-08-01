using System;
namespace Uvel.Library.Components.Checkbox
{
    public static class UvelCheckboxFrontend
    {
        public static string Name { get { return "uvel.checkbox.frontend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
