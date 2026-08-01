using System;
namespace Uvel.Library.Components.Checkbox
{
    public static class UvelCheckboxBackend
    {
        public static string Name { get { return "uvel.checkbox.backend"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
