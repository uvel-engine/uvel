using System;
namespace Uvel.Library.Components.Checkbox
{
    public static class UvelCheckboxAnimation
    {
        public static string Name { get { return "uvel.checkbox.animation"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
