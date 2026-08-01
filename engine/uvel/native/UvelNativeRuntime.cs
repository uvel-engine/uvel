using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Uvel.Native2D
{
    public class UvelNativeRuntime
    {
        public void Run(string xmlPath)
        {
            if (string.IsNullOrEmpty(xmlPath) || !File.Exists(xmlPath))
                throw new FileNotFoundException("XML file not found", xmlPath);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UvelNativeWindow(xmlPath));
        }
    }

    internal class UvelNativeWindow : Form
    {
        private readonly string _xmlPath;
        private UvelDocument _doc;
        private FileSystemWatcher _watcher;
        private DateTime _lastReload = DateTime.MinValue;
        private readonly UvelBackendRegistry _backend = new UvelBackendRegistry();
        private UvelElement _focusedInput;
        private UvelElement _pressed;
        private UvelElement _hover;
        private readonly System.Windows.Forms.Timer _timer;
        private float _pulse;

        private readonly Font _font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Font _fontSmall = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Font _fontBold = new Font("Segoe UI", 12f, FontStyle.Bold, GraphicsUnit.Point);
        private readonly Font _fontTitle = new Font("Segoe UI", 21f, FontStyle.Bold, GraphicsUnit.Point);

        public UvelNativeWindow(string xmlPath)
        {
            _xmlPath = Path.GetFullPath(xmlPath);
            DoubleBuffered = true;
            KeyPreview = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(420, 280);
            BackColor = Color.FromArgb(11, 15, 25);
            LoadXml();
            SetupWatcher();
            NativeApi.EnableImmersiveDarkMode(Handle);
            NativeApi.EnableDwmComposition(Handle);
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 16;
            _timer.Tick += delegate { _pulse += 0.035f; Invalidate(); };
            _timer.Start();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeApi.EnableImmersiveDarkMode(Handle);
            NativeApi.EnableDwmComposition(Handle);
        }

        private void SetupWatcher()
        {
            try
            {
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(_xmlPath), "*.xml");
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
            try { if (IsHandleCreated) BeginInvoke(new Action(delegate { LoadXml(); Invalidate(); })); } catch { }
        }

        private void LoadXml()
        {
            _doc = UvelDocument.Load(_xmlPath);
            Text = _doc.Title;
            ClientSize = new Size(Math.Max(420, _doc.Width), Math.Max(280, _doc.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush bg = new SolidBrush(_doc.Background)) g.FillRectangle(bg, ClientRectangle);
            DrawAmbient(g);
            if (_doc.Root != null) { Layout(_doc.Root, ClientRectangle); DrawElement(g, _doc.Root); }
        }

        private void DrawAmbient(Graphics g)
        {
            DrawGlow(g, new PointF(ClientSize.Width * .16f, ClientSize.Height * .14f), 260, Color.FromArgb(24, 52, 199, 89));
            DrawGlow(g, new PointF(ClientSize.Width * .86f, ClientSize.Height * .12f), 300, Color.FromArgb(20, 37, 99, 235));
            DrawGlow(g, new PointF(ClientSize.Width * .60f, ClientSize.Height * .90f), 340, Color.FromArgb(16, 168, 85, 247));
        }

        private void DrawGlow(Graphics g, PointF center, int radius, Color color)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                using (PathGradientBrush b = new PathGradientBrush(path))
                {
                    b.CenterColor = color;
                    b.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                    g.FillPath(b, path);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            UvelElement h = HitTest(_doc.Root, e.Location);
            if (!object.ReferenceEquals(h, _hover)) { _hover = h; Cursor = h != null && (h.Type == "Button" || h.Type == "Input" || h.Type == "ListItem" || h.Type == "Tab") ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _pressed = HitTest(_doc.Root, e.Location);
            if (_pressed != null && _pressed.Type == "Input") { _focusedInput = _pressed; _pressed = null; Invalidate(); return; }
            if (_pressed != null) Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            UvelElement up = HitTest(_doc.Root, e.Location);
            UvelElement was = _pressed;
            _pressed = null;
            if (was != null && object.ReferenceEquals(was, up) && !string.IsNullOrEmpty(was.OnClick)) ExecuteHandler(was.OnClick);
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (_focusedInput == null) return;
            if (!char.IsControl(e.KeyChar)) { _focusedInput.Text = (_focusedInput.Text ?? "") + e.KeyChar; Invalidate(); }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_focusedInput == null) return;
            if (e.KeyCode == Keys.Back && !string.IsNullOrEmpty(_focusedInput.Text)) { _focusedInput.Text = _focusedInput.Text.Substring(0, _focusedInput.Text.Length - 1); Invalidate(); }
            if (e.KeyCode == Keys.Escape) { _focusedInput = null; Invalidate(); }
        }

        private UvelElement HitTest(UvelElement el, Point p)
        {
            if (el == null) return null;
            for (int i = el.Children.Count - 1; i >= 0; i--) { UvelElement h = HitTest(el.Children[i], p); if (h != null) return h; }
            if ((el.Type == "Button" || el.Type == "Input" || el.Type == "ListItem" || el.Type == "Tab") && el.Bounds.Contains(p)) return el;
            return null;
        }

        private void ExecuteHandler(string handler)
        {
            UvelNativeContext ctx = new UvelNativeContext(SetText);
            if (_backend.Execute(handler, ctx)) return;
            if (!_doc.Handlers.ContainsKey(handler)) return;
            foreach (XmlNode cmd in _doc.Handlers[handler]) ExecuteCommand(cmd);
        }

        private void ExecuteCommand(XmlNode cmd)
        {
            string name = cmd.Name;
            if (name == "Set") SetText(Attr(cmd, "Target", "status"), ResolveValue(Attr(cmd, "Value", "")));
            else if (name == "Call") ExecuteHandler(Attr(cmd, "Handler", Attr(cmd, "Name", cmd.InnerText.Trim())));
            else if (name == "Toast") SetText("status", ResolveValue(Attr(cmd, "Message", "Done")));
            else if (name == "DbSet" || name == "DatabaseSet") UvelDatabase.Set(Attr(cmd, "Key", "value"), ResolveValue(Attr(cmd, "Value", "")));
            else if (name == "DbGet" || name == "DatabaseGet") SetText(Attr(cmd, "Target", "status"), UvelDatabase.Get(Attr(cmd, "Key", "value"), Attr(cmd, "Default", "")));
            else if (name == "AddMessage") AddMessage(cmd);
            else if (name == "ClearText") SetText(Attr(cmd, "Control", Attr(cmd, "Target", "")), "");
        }

        private void AddMessage(XmlNode cmd)
        {
            string container = Attr(cmd, "Container", "messages");
            string text = ResolveValue(Attr(cmd, "Text", ""));
            string sender = Attr(cmd, "Sender", "me");
            UvelElement target = FindByName(_doc.Root, container);
            if (target == null || string.IsNullOrWhiteSpace(text)) return;
            UvelElement item = new UvelElement("ListItem");
            item.Text = text;
            item.Align = sender == "me" ? "right" : "left";
            item.Background = sender == "me" ? Color.FromArgb(255, 52, 199, 89) : Color.FromArgb(36, 255, 255, 255);
            item.Foreground = Color.White;
            target.Children.Add(item);
        }

        private string ResolveValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string result = value;
            int guard = 0;
            while (result.Contains("{") && result.Contains("}") && guard++ < 30)
            {
                int a = result.IndexOf('{'); int b = result.IndexOf('}', a + 1); if (a < 0 || b <= a) break;
                string token = result.Substring(a + 1, b - a - 1);
                string replacement = "";
                if (token.EndsWith(".Text")) { UvelElement el = FindByName(_doc.Root, token.Substring(0, token.Length - 5)); replacement = el == null ? "" : (el.Text ?? ""); }
                else { UvelElement el = FindByName(_doc.Root, token); replacement = el == null ? UvelDatabase.Get(token, "") : (el.Text ?? ""); }
                result = result.Substring(0, a) + replacement + result.Substring(b + 1);
            }
            return result;
        }

        private void SetText(string target, string value) { UvelElement el = FindByName(_doc.Root, target); if (el != null) el.Text = value; }
        private UvelElement FindByName(UvelElement el, string name) { if (el == null || string.IsNullOrEmpty(name)) return null; if (el.Name == name) return el; foreach (UvelElement c in el.Children) { UvelElement f = FindByName(c, name); if (f != null) return f; } return null; }
        private string Attr(XmlNode n, string name, string def) { XmlAttribute a = n == null || n.Attributes == null ? null : n.Attributes[name]; return a == null ? def : a.Value; }

        private void Layout(UvelElement root, Rectangle area) { LayoutElement(root, area, 0); }
        private void LayoutElement(UvelElement el, Rectangle area, int depth)
        {
            if (el.HasFrame) el.Bounds = new Rectangle(area.X + Math.Max(0, el.X), area.Y + Math.Max(0, el.Y), Math.Max(8, el.W <= 0 ? area.Width : el.W), Math.Max(8, el.H <= 0 ? area.Height : el.H));
            else if (depth == 0) el.Bounds = area;
            else if (el.Bounds.Width <= 0) el.Bounds = area;
            if (el.Type == "Text" || el.Type == "Button" || el.Type == "Input" || el.Type == "Icon" || el.Type == "Divider" || el.Type == "Badge") return;
            Rectangle inner = new Rectangle(el.Bounds.X + el.Padding, el.Bounds.Y + el.Padding, Math.Max(0, el.Bounds.Width - el.Padding * 2), Math.Max(0, el.Bounds.Height - el.Padding * 2));
            int y = inner.Y;
            foreach (UvelElement child in el.Children)
            {
                if (child.HasFrame) { LayoutElement(child, el.Bounds, depth + 1); continue; }
                int h = child.Type == "Button" || child.Type == "Input" ? 44 : child.Type == "Text" ? Math.Max(24, child.FontSize + 12) : child.Type == "Divider" ? 12 : child.Type == "Badge" ? 26 : child.Type == "ListItem" ? 44 : 68;
                child.Bounds = new Rectangle(inner.X, y, inner.Width, h); y += h + el.Gap; LayoutElement(child, child.Bounds, depth + 1);
            }
        }

        private void DrawElement(Graphics g, UvelElement el)
        {
            if (el.Type == "Card" || el.Type == "View" || el.Type == "Scroll" || el.Type == "List" || el.Type == "ListView" || el.Type == "Tabs")
            {
                if (el.Background.A > 0) { using (SolidBrush b = new SolidBrush(el.Background)) FillRound(g, b, el.Bounds, el.Radius); using (Pen p = new Pen(el.Border)) DrawRound(g, p, el.Bounds, el.Radius); }
            }
            else if (el.Type == "Button")
            {
                Rectangle r = el.Bounds; if (object.ReferenceEquals(_pressed, el)) { r.Offset(0, 2); }
                Color bg = object.ReferenceEquals(_pressed, el) ? Blend(el.Background, Color.Black, .18f) : object.ReferenceEquals(_hover, el) ? Blend(el.Background, Color.White, .12f) : el.Background;
                using (SolidBrush b = new SolidBrush(bg)) FillRound(g, b, r, el.Radius); using (Pen p = new Pen(Blend(bg, Color.White, .18f))) DrawRound(g, p, r, el.Radius); DrawText(g, el.Text, _font, el.Foreground, r, "center");
            }
            else if (el.Type == "Input")
            {
                Color border = object.ReferenceEquals(_focusedInput, el) ? Color.FromArgb(255, 52, 199, 89) : Color.FromArgb(54,255,255,255);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(24,255,255,255))) FillRound(g, b, el.Bounds, 14); using (Pen p = new Pen(border)) DrawRound(g, p, el.Bounds, 14);
                string txt = string.IsNullOrEmpty(el.Text) ? el.Placeholder : el.Text; if (object.ReferenceEquals(_focusedInput, el) && ((int)(_pulse*2)%2)==0) txt += "|"; DrawText(g, txt, _font, Color.FromArgb(210,255,255,255), el.Bounds, "left");
            }
            else if (el.Type == "Text") DrawText(g, el.Text, el.FontSize >= 22 ? _fontTitle : _font, el.Foreground, el.Bounds, el.Align);
            else if (el.Type == "ListItem") { Rectangle b = BubbleBounds(el); using (SolidBrush br = new SolidBrush(el.Background)) FillRound(g, br, b, 16); DrawText(g, el.Text, _font, el.Foreground, b, el.Align == "right" ? "right" : "left"); }
            else if (el.Type == "Icon") DrawText(g, UvelIconRegistry.IconText(el.Icon), _fontBold, el.Foreground, el.Bounds, "center");
            else if (el.Type == "Badge") { using (SolidBrush b = new SolidBrush(Color.FromArgb(42,52,199,89))) FillRound(g,b,el.Bounds,13); DrawText(g,el.Text,_fontSmall,Color.FromArgb(52,199,89),el.Bounds,"center"); }
            else if (el.Type == "Divider") { using (Pen p = new Pen(Color.FromArgb(34,255,255,255))) g.DrawLine(p, el.Bounds.Left, el.Bounds.Top + el.Bounds.Height/2, el.Bounds.Right, el.Bounds.Top + el.Bounds.Height/2); }
            foreach (UvelElement c in el.Children) DrawElement(g, c);
        }

        private Rectangle BubbleBounds(UvelElement el) { Rectangle b = el.Bounds; int w = Math.Min(el.Bounds.Width - 18, Math.Max(160, TextRenderer.MeasureText(el.Text ?? "", _font).Width + 34)); b.Width = w; if (el.Align == "right") b.X = el.Bounds.Right - w; return b; }
        private Color Blend(Color a, Color b, float t) { return Color.FromArgb(a.A, (int)(a.R+(b.R-a.R)*t), (int)(a.G+(b.G-a.G)*t), (int)(a.B+(b.B-a.B)*t)); }
        private void DrawText(Graphics g, string text, Font font, Color color, Rectangle bounds, string align) { using (StringFormat sf = new StringFormat()) using (SolidBrush b = new SolidBrush(color)) { sf.LineAlignment = StringAlignment.Center; sf.Alignment = align == "right" ? StringAlignment.Far : align == "left" ? StringAlignment.Near : StringAlignment.Center; Rectangle r = bounds; r.Inflate(-12,0); g.DrawString(text ?? "", font, b, r, sf); } }
        private void FillRound(Graphics g, Brush b, Rectangle r, int radius) { using (GraphicsPath p = RoundPath(r, radius)) g.FillPath(b, p); }
        private void DrawRound(Graphics g, Pen pen, Rectangle r, int radius) { using (GraphicsPath p = RoundPath(r, radius)) g.DrawPath(pen, p); }
        private GraphicsPath RoundPath(Rectangle r, int radius) { int d = Math.Max(2, radius*2); GraphicsPath p = new GraphicsPath(); p.AddArc(r.X,r.Y,d,d,180,90); p.AddArc(r.Right-d,r.Y,d,d,270,90); p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); p.AddArc(r.X,r.Bottom-d,d,d,90,90); p.CloseFigure(); return p; }
    }

    internal class UvelDocument
    {
        public string Title = "Uvel App"; public int Width = 920; public int Height = 620; public Color Background = Color.FromArgb(11,15,25); public UvelElement Root; public Dictionary<string,List<XmlNode>> Handlers = new Dictionary<string,List<XmlNode>>();
        public static UvelDocument Load(string file)
        {
            XmlDocument xml = new XmlDocument(); xml.Load(file); UvelDocument doc = new UvelDocument(); XmlElement app = xml.DocumentElement;
            if (app != null) { doc.Title = Attr(app,"Name","Uvel App"); int.TryParse(Attr(app,"Width","920"), out doc.Width); int.TryParse(Attr(app,"Height","620"), out doc.Height); }
            XmlNode logic = app == null ? null : app.SelectSingleNode("Logic");
            if (logic != null) foreach (XmlNode h in logic.ChildNodes) if (h.NodeType == XmlNodeType.Element && h.Name == "Handler") { string n=Attr(h,"Name",""); if(n!="") { List<XmlNode> list=new List<XmlNode>(); foreach(XmlNode c in h.ChildNodes) if(c.NodeType==XmlNodeType.Element) list.Add(c); doc.Handlers[n]=list; } }
            XmlNode ui = app == null ? null : app.SelectSingleNode("UI"); doc.Root = ParseElement(FirstElement(ui)); if (doc.Root == null) doc.Root = new UvelElement("Card"); return doc;
        }
        private static XmlNode FirstElement(XmlNode n) { if(n==null)return null; foreach(XmlNode c in n.ChildNodes) if(c.NodeType==XmlNodeType.Element && !c.Name.Contains(".")) return c; return null; }
        private static UvelElement ParseElement(XmlNode node)
        {
            if(node==null)return null; if(node.Name=="Import"||node.Name=="Imports"||node.Name=="Logic"||node.Name=="Styles"||node.Name.Contains(".")) return null;
            UvelElement el = new UvelElement(UvelComponentRegistry.Resolve(Normalize(node.Name)));
            el.Name=Attr(node,"Name",Attr(node,"x:Name","")); el.Text=Attr(node,"Text",Attr(node,"Content",node.InnerText.Trim())); el.Placeholder=Attr(node,"Placeholder",""); el.OnClick=Attr(node,"onClick",""); el.Icon=Attr(node,"Icon",el.Text); el.Align=Attr(node,"Align",Attr(node,"HorizontalAlignment","center")).ToLower();
            el.Background=ParseColor(Attr(node,"Background",el.Type=="Button"?"#34C759":el.Type=="View"?"#00000000":"#FFFFFF14")); el.Border=ParseColor(Attr(node,"BorderBrush",Attr(node,"BorderColor","#FFFFFF24"))); el.Foreground=ParseColor(Attr(node,"Foreground","#FFFFFF"));
            int.TryParse(Attr(node,"CornerRadius",el.Type=="Button"?"18":"24"),out el.Radius); int.TryParse(Attr(node,"Padding","20").Split(',')[0],out el.Padding); if(el.Padding<0)el.Padding=0; int.TryParse(Attr(node,"Gap","10"),out el.Gap); int.TryParse(Attr(node,"FontSize","14"),out el.FontSize);
            int x,y,w,h; bool has=false; if(int.TryParse(Attr(node,"X","-1"),out x)){el.X=x;has=true;} if(int.TryParse(Attr(node,"Y","-1"),out y)){el.Y=y;has=true;} if(int.TryParse(Attr(node,"Width","0"),out w)){el.W=w;if(w>0)has=true;} if(int.TryParse(Attr(node,"Height","0"),out h)){el.H=h;if(h>0)has=true;} el.HasFrame=has;
            foreach(XmlNode c in node.ChildNodes){ UvelElement ch=ParseElement(c); if(ch!=null) el.Children.Add(ch); }
            return el;
        }
        private static string Normalize(string name) { string n=(name??"").ToLower(); if(n=="grid")return"View"; if(n=="border"||n=="card"||n=="uvelcard"||n=="glasscard")return"Card"; if(n=="button"||n=="uvelbutton")return"Button"; if(n=="input"||n=="uvelinput"||n=="textbox")return"Input"; if(n=="textblock"||n=="text"||n=="label")return"Text"; return UvelComponentRegistry.Resolve(name); }
        private static Color ParseColor(string v) { try { if(string.IsNullOrEmpty(v))return Color.White; if(v.StartsWith("#")){string h=v.Substring(1); if(h.Length==8){int r=Convert.ToInt32(h.Substring(0,2),16);int g=Convert.ToInt32(h.Substring(2,2),16);int b=Convert.ToInt32(h.Substring(4,2),16);int a=Convert.ToInt32(h.Substring(6,2),16);return Color.FromArgb(a,r,g,b);} if(h.Length==6)return Color.FromArgb(Convert.ToInt32(h.Substring(0,2),16),Convert.ToInt32(h.Substring(2,2),16),Convert.ToInt32(h.Substring(4,2),16));} return ColorTranslator.FromHtml(v);} catch{return Color.White;} }
        private static string Attr(XmlNode n,string name,string def){XmlAttribute a=n==null||n.Attributes==null?null:n.Attributes[name];return a==null?def:a.Value;}
    }

    internal class UvelElement { public string Type,Name,Text,Placeholder,OnClick,Icon,Align; public Rectangle Bounds; public int X=-1,Y=-1,W=0,H=0,Radius=24,Padding=20,Gap=10,FontSize=14; public bool HasFrame; public Color Background=Color.FromArgb(32,255,255,255),Border=Color.FromArgb(42,255,255,255),Foreground=Color.White; public List<UvelElement> Children=new List<UvelElement>(); public UvelElement(string t){Type=t;Align="center";} }
    internal static class NativeApi
    {
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd); [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); [DllImport("user32.dll")] public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject); [DllImport("gdi32.dll")] public static extern IntPtr CreateSolidBrush(int colorRef);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize); [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("d2d1.dll", EntryPoint="D2D1CreateFactory")] public static extern int D2D1CreateFactory(uint factoryType, ref Guid riid, IntPtr options, out IntPtr factory); [DllImport("dwrite.dll", EntryPoint="DWriteCreateFactory")] public static extern int DWriteCreateFactory(uint factoryType, ref Guid iid, out IntPtr factory); [DllImport("dcomp.dll", EntryPoint="DCompositionCreateDevice")] public static extern int DCompositionCreateDevice(IntPtr dxgiDevice, ref Guid iid, out IntPtr dcompDevice);
        [StructLayout(LayoutKind.Sequential)] private struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }
        public static void EnableImmersiveDarkMode(IntPtr h){try{int v=1;DwmSetWindowAttribute(h,20,ref v,4);}catch{}} public static void EnableDwmComposition(IntPtr h){try{MARGINS m=new MARGINS();DwmExtendFrameIntoClientArea(h,ref m);}catch{}}
    }
}
