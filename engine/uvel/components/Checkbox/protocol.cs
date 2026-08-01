using System;
namespace Uvel.Library.Components.Checkbox
{
    public static class UvelCheckboxProtocol
    {
        public static string Name { get { return "uvel.checkbox.protocol"; } }
        public static void Touch() { Console.WriteLine("[" + Name + "] ready"); }
    }
}
