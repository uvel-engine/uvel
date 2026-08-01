using System;

namespace Uvel.Library.Components.Button
{
    public static class UvelButtonAnimation
    {
        public static string Name { get { return "uvel.button.animation"; } }
        public static void Log(string message) { Console.WriteLine("[" + Name + "] " + message); }
    }
}
