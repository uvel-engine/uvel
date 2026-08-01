using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Uvel.Native2D
{
    /// <summary>
    /// Native Uvel runtime: XML -> Uvel element tree -> Win32 window -> GDI/GDI+ drawing.
    /// No WPF/XAML, no NuGet, no external DLL. Windows 8+ friendly.
    ///
    /// The file intentionally contains P/Invoke declarations for the Windows
    /// rendering stack Uvel will grow into: user32, gdi32, d2d1, dwrite,
    /// dwmapi and dcomp. The current stable drawing path uses GDI/GDI+ because
    /// it is available through .NET Framework 4.0 and requires no COM wrapper.
    /// Direct2D/DirectWrite/DirectComposition hooks are present for the next
    /// renderer stage without introducing dependencies.
    /// </summary>
    public class UvelNativeRuntime
    {
        private NativeWindowHost _window;

        public void Run(string xmlPath)
        {
            if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
                throw new FileNotFoundException("XML file not found", xmlPath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _window = new NativeWindowHost(xmlPath);
            _window.Show();
            Application.Run(_window);
        }
    }

    internal class NativeWindowHost : Form
    {
        private string _xmlPath;
        private UvelDocument _doc;
        private FileSystemWatcher _watcher;
        private DateTime _lastReload = DateTime.MinValue;
        private Font _font;
        private Font _fontBold;

        public NativeWindowHost(string xmlPath)
        {
            _xmlPath = Path.GetFullPath(xmlPath);
            _font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            _fontBold = new Font("Segoe UI", 18f, FontStyle.Bold, GraphicsUnit.Point);
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(360, 240);
            this.BackColor = Color.FromArgb(11, 15, 25);
            LoadXml();
            SetupWatcher();
            NativeApi.EnableImmersiveDarkMode(this.Handle);
            NativeApi.EnableDwmComposition(this.Handle);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeApi.EnableImmersiveDarkMode(this.Handle);
            NativeApi.EnableDwmComposition(this.Handle);
        }

        private void SetupWatcher()
        {
            try
            {
                string dir = Path.GetDirectoryName(_xmlPath);
                _watcher = new FileSystemWatcher(dir, "*.xml");
                _watcher.IncludeSubdirectories = true;
                _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
                _watcher.Changed += delegate { HotReload(); };
                _watcher.Created += delegate { HotReload(); };
                _watcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        private void HotReload()
        {
            if ((DateTime.Now - _lastReload).TotalMilliseconds < 350) return;
            _lastReload = DateTime.Now;
            Thread.Sleep(80);
            try
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(delegate
                    {
                        LoadXml();
                        Invalidate();
                    }));
                }
            }
            catch { }
        }

        private void LoadXml()
        {
            _doc = UvelDocument.Load(_xmlPath);
            this.Text = _doc.Title;
            this.ClientSize = new Size(Math.Max(320, _doc.Width), Math.Max(220, _doc.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush bg = new SolidBrush(_doc.Background)) e.Graphics.FillRectangle(bg, ClientRectangle);
            if (_doc.Root != null)
            {
                Layout(_doc.Root, ClientRectangle);
                DrawElement(e.Graphics, _doc.Root);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            UvelElement hit = HitTest(_doc.Root, e.Location);
            if (hit != null && !string.IsNullOrEmpty(hit.OnClick))
            {
                ExecuteHandler(hit.OnClick);
                Invalidate();
            }
        }

        private UvelElement HitTest(UvelElement el, Point p)
        {
            if (el == null) return null;
            for (int i = el.Children.Count - 1; i >= 0; i--)
            {
                UvelElement hit = HitTest(el.Children[i], p);
                if (hit != null) return hit;
            }
            if ((el.Type == "Button" || el.Type == "UvelButton") && el.Bounds.Contains(p)) return el;
            return null;
        }

        private void ExecuteHandler(string handler)
        {
            if (handler == "uvel.backend.ping")
            {
                SetText("status", "Backend ready");
                return;
            }
            if (!_doc.Handlers.ContainsKey(handler)) return;
            List<XmlNode> commands = _doc.Handlers[handler];
            foreach (XmlNode cmd in commands)
            {
                if (cmd.Name == "Set")
                {
                    string target = Attr(cmd, "Target", "");
                    string prop = Attr(cmd, "Property", "Text");
                    string value = Attr(cmd, "Value", "");
                    if (prop.ToLower() == "text") SetText(target, value);
                }
                else if (cmd.Name == "Call")
                {
                    ExecuteHandler(Attr(cmd, "Handler", Attr(cmd, "Name", cmd.InnerText.Trim())));
                }
                else if (cmd.Name == "Toast")
                {
                    SetText("status", Attr(cmd, "Message", "Done"));
                }
            }
        }

        private void SetText(string name, string value)
        {
            UvelElement el = FindByName(_doc.Root, name);
            if (el != null) el.Text = value;
        }

        private UvelElement FindByName(UvelElement el, string name)
        {
            if (el == null || string.IsNullOrEmpty(name)) return null;
            if (el.Name == name) return el;
            foreach (UvelElement child in el.Children)
            {
                UvelElement found = FindByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void Layout(UvelElement root, Rectangle area)
        {
            int cardW = Math.Min(560, Math.Max(260, area.Width - 80));
            int cardH = Math.Min(230, Math.Max(140, area.Height - 80));
            root.Bounds = new Rectangle((area.Width - cardW) / 2, (area.Height - cardH) / 2, cardW, cardH);
            LayoutChildren(root);
        }

        private void LayoutChildren(UvelElement parent)
        {
            int y = parent.Bounds.Top + parent.Padding;
            int innerW = parent.Bounds.Width - parent.Padding * 2;
            foreach (UvelElement child in parent.Children)
            {
                int h = child.Type == "Button" || child.Type == "UvelButton" ? 46 : child.Type == "TextBlock" ? 38 : 30;
                child.Bounds = new Rectangle(parent.Bounds.Left + parent.Padding, y, innerW, h);
                y += h + 12;
                LayoutChildren(child);
            }
        }

        private void DrawElement(Graphics g, UvelElement el)
        {
            if (el.Type == "Card" || el.Type == "GlassCard" || el.Type == "UvelCard" || el.Type == "StackPanel")
            {
                if (el.Type != "StackPanel")
                {
                    using (SolidBrush b = new SolidBrush(el.Background)) FillRound(g, b, el.Bounds, el.Radius);
                    using (Pen p = new Pen(el.Border)) DrawRound(g, p, el.Bounds, el.Radius);
                }
            }
            else if (el.Type == "Button" || el.Type == "UvelButton")
            {
                using (SolidBrush b = new SolidBrush(el.Background)) FillRound(g, b, el.Bounds, el.Radius);
                using (Pen p = new Pen(el.Border)) DrawRound(g, p, el.Bounds, el.Radius);
                DrawCenteredText(g, el.Text, _font, el.Foreground, el.Bounds);
            }
            else if (el.Type == "TextBlock")
            {
                Font f = el.FontSize >= 24 ? _fontBold : _font;
                DrawCenteredText(g, el.Text, f, el.Foreground, el.Bounds);
            }

            foreach (UvelElement child in el.Children) DrawElement(g, child);
        }

        private void DrawCenteredText(Graphics g, string text, Font font, Color color, Rectangle bounds)
        {
            using (StringFormat sf = new StringFormat())
            using (SolidBrush b = new SolidBrush(color))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(text ?? "", font, b, bounds, sf);
            }
        }

        private void FillRound(Graphics g, Brush b, Rectangle r, int radius)
        {
            using (System.Drawing.Drawing2D.GraphicsPath path = RoundPath(r, radius)) g.FillPath(b, path);
        }

        private void DrawRound(Graphics g, Pen p, Rectangle r, int radius)
        {
            using (System.Drawing.Drawing2D.GraphicsPath path = RoundPath(r, radius)) g.DrawPath(p, path);
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            System.Drawing.Drawing2D.GraphicsPath p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private string Attr(XmlNode n, string name, string def)
        {
            XmlAttribute a = n.Attributes == null ? null : n.Attributes[name];
            return a == null ? def : a.Value;
        }
    }

    internal class UvelDocument
    {
        public string Title = "Uvel App";
        public int Width = 860;
        public int Height = 540;
        public Color Background = Color.FromArgb(11, 15, 25);
        public UvelElement Root;
        public Dictionary<string, List<XmlNode>> Handlers = new Dictionary<string, List<XmlNode>>();

        public static UvelDocument Load(string file)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(file);
            UvelDocument doc = new UvelDocument();
            XmlElement app = xml.DocumentElement;
            if (app != null)
            {
                doc.Title = Attr(app, "Name", "Uvel App");
                int.TryParse(Attr(app, "Width", "860"), out doc.Width);
                int.TryParse(Attr(app, "Height", "540"), out doc.Height);
            }

            XmlNode logic = app == null ? null : app.SelectSingleNode("Logic");
            if (logic != null)
            {
                foreach (XmlNode h in logic.ChildNodes)
                {
                    if (h.NodeType == XmlNodeType.Element && h.Name == "Handler")
                    {
                        string name = Attr(h, "Name", "");
                        if (!string.IsNullOrEmpty(name))
                        {
                            List<XmlNode> list = new List<XmlNode>();
                            foreach (XmlNode c in h.ChildNodes) if (c.NodeType == XmlNodeType.Element) list.Add(c);
                            doc.Handlers[name] = list;
                        }
                    }
                }
            }

            XmlNode ui = app == null ? null : app.SelectSingleNode("UI");
            XmlNode first = FirstElement(ui);
            doc.Root = ParseElement(first);
            if (doc.Root == null) doc.Root = new UvelElement("Card");
            return doc;
        }

        private static XmlNode FirstElement(XmlNode node)
        {
            if (node == null) return null;
            foreach (XmlNode c in node.ChildNodes) if (c.NodeType == XmlNodeType.Element) return c;
            return null;
        }

        private static UvelElement ParseElement(XmlNode node)
        {
            if (node == null) return null;
            string type = Normalize(node.Name);
            if (type == "Grid")
            {
                XmlNode first = FirstUsefulChild(node);
                return ParseElement(first);
            }
            UvelElement el = new UvelElement(type);
            el.Name = Attr(node, "Name", Attr(node, "x:Name", ""));
            el.Text = Attr(node, "Text", Attr(node, "Content", node.InnerText.Trim()));
            el.OnClick = Attr(node, "onClick", "");
            el.Background = ParseColor(Attr(node, "Background", type == "Button" ? "#34C759" : "#FFFFFF14"));
            el.Border = ParseColor(Attr(node, "BorderBrush", Attr(node, "BorderColor", "#FFFFFF24")));
            el.Foreground = ParseColor(Attr(node, "Foreground", "#FFFFFF"));
            int.TryParse(Attr(node, "CornerRadius", type == "Button" ? "18" : "28"), out el.Radius);
            int.TryParse(Attr(node, "Padding", "28").Split(',')[0], out el.Padding);
            if (el.Padding <= 0) el.Padding = 20;
            int fs;
            if (int.TryParse(Attr(node, "FontSize", "14"), out fs)) el.FontSize = fs;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                if (child.Name == "Import" || child.Name == "Logic") continue;
                UvelElement parsed = ParseElement(child);
                if (parsed != null) el.Children.Add(parsed);
            }
            return el;
        }

        private static XmlNode FirstUsefulChild(XmlNode node)
        {
            foreach (XmlNode c in node.ChildNodes)
            {
                if (c.NodeType != XmlNodeType.Element) continue;
                if (c.Name.Contains(".")) continue;
                return c;
            }
            return null;
        }

        private static string Normalize(string name)
        {
            string n = (name ?? "").ToLower();
            if (n == "card" || n == "uvelcard" || n == "glasscard" || n == "border") return "Card";
            if (n == "button" || n == "uvelbutton") return "Button";
            if (n == "input" || n == "uvelinput" || n == "textbox") return "Input";
            if (n == "stackpanel" || n == "panel") return "StackPanel";
            if (n == "textblock" || n == "text" || n == "label") return "TextBlock";
            return name;
        }

        private static Color ParseColor(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return Color.White;
                if (value.StartsWith("#"))
                {
                    string h = value.Substring(1);
                    if (h.Length == 8)
                    {
                        int a = Convert.ToInt32(h.Substring(0, 2), 16);
                        int r = Convert.ToInt32(h.Substring(2, 2), 16);
                        int g = Convert.ToInt32(h.Substring(4, 2), 16);
                        int b = Convert.ToInt32(h.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                    if (h.Length == 6)
                    {
                        int r = Convert.ToInt32(h.Substring(0, 2), 16);
                        int g = Convert.ToInt32(h.Substring(2, 2), 16);
                        int b = Convert.ToInt32(h.Substring(4, 2), 16);
                        return Color.FromArgb(r, g, b);
                    }
                }
                return ColorTranslator.FromHtml(value);
            }
            catch { return Color.White; }
        }

        private static string Attr(XmlNode n, string name, string def)
        {
            XmlAttribute a = n == null || n.Attributes == null ? null : n.Attributes[name];
            return a == null ? def : a.Value;
        }
    }

    internal class UvelElement
    {
        public string Type;
        public string Name;
        public string Text;
        public string OnClick;
        public Rectangle Bounds;
        public Color Background = Color.FromArgb(32, 255, 255, 255);
        public Color Border = Color.FromArgb(42, 255, 255, 255);
        public Color Foreground = Color.White;
        public int Radius = 24;
        public int Padding = 20;
        public int FontSize = 14;
        public List<UvelElement> Children = new List<UvelElement>();
        public UvelElement(string type) { Type = type; }
    }

    internal static class NativeApi
    {
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")] public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateSolidBrush(int colorRef);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("d2d1.dll", EntryPoint = "D2D1CreateFactory")] public static extern int D2D1CreateFactory(uint factoryType, ref Guid riid, IntPtr options, out IntPtr factory);
        [DllImport("dwrite.dll", EntryPoint = "DWriteCreateFactory")] public static extern int DWriteCreateFactory(uint factoryType, ref Guid iid, out IntPtr factory);
        [DllImport("dcomp.dll", EntryPoint = "DCompositionCreateDevice")] public static extern int DCompositionCreateDevice(IntPtr dxgiDevice, ref Guid iid, out IntPtr dcompDevice);

        [StructLayout(LayoutKind.Sequential)] private struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }

        public static void EnableImmersiveDarkMode(IntPtr handle)
        {
            try { int value = 1; DwmSetWindowAttribute(handle, 20, ref value, 4); } catch { }
        }

        public static void EnableDwmComposition(IntPtr handle)
        {
            try { MARGINS m = new MARGINS(); m.cxLeftWidth = 0; m.cxRightWidth = 0; m.cyTopHeight = 0; m.cyBottomHeight = 0; DwmExtendFrameIntoClientArea(handle, ref m); } catch { }
        }
    }
}
