using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Uvel.Native2D
{
    /// <summary>
    /// Experimental native Uvel UI layer. This is the beginning of the non-XAML
    /// renderer requested for Uvel: XML -> Uvel element tree -> Win32/GDI 2D.
    /// The current stable runtime still uses WPF; this layer is isolated so it
    /// can grow without breaking existing apps.
    /// </summary>
    public class UvelNativeElement
    {
        public string Type;
        public string Name;
        public Rectangle Bounds;
        public Dictionary<string, string> Props = new Dictionary<string, string>();
        public List<UvelNativeElement> Children = new List<UvelNativeElement>();
    }

    public class UvelNativeRenderer
    {
        public void Render(Graphics g, UvelNativeElement root)
        {
            if (g == null || root == null) return;
            RenderElement(g, root);
        }

        private void RenderElement(Graphics g, UvelNativeElement el)
        {
            if (el.Type == "Card" || el.Type == "UvelCard")
            {
                using (Brush b = new SolidBrush(Color.FromArgb(32, 255, 255, 255)))
                    g.FillRectangle(b, el.Bounds);
                using (Pen p = new Pen(Color.FromArgb(42, 255, 255, 255)))
                    g.DrawRectangle(p, el.Bounds);
            }
            foreach (UvelNativeElement child in el.Children) RenderElement(g, child);
        }
    }

    public static class UvelNativeProtocol
    {
        public const string Renderer = "uvel.native2d";
        public const string Backend = "uvel.native2d.backend";
        public const string Frontend = "uvel.native2d.frontend";
        public const string Animation = "uvel.native2d.animation";
    }
}
