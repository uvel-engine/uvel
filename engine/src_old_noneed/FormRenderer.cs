using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;

namespace Uvel
{
    /// <summary>
    /// Form Renderer - Creates WinForms from XML definitions
    /// Full custom rendering with smooth animations
    /// Compatible with .NET Framework 4.0
    /// </summary>
    public class FormRenderer
    {
        private UvelEngine _engine;
        private XmlParser _parser;
        
        // Theme colors - iOS/macOS style
        private static readonly Color BgDark = Color.FromArgb(0, 0, 0);
        private static readonly Color SurfaceDark = Color.FromArgb(28, 28, 30);
        private static readonly Color SurfaceLight = Color.FromArgb(44, 44, 46);
        private static readonly Color AccentOrange = Color.FromArgb(255, 149, 0);
        private static readonly Color AccentBlue = Color.FromArgb(10, 132, 255);
        private static readonly Color TextWhite = Color.FromArgb(255, 255, 255);
        private static readonly Color TextGray = Color.FromArgb(142, 142, 147);
        
        public FormRenderer(UvelEngine engine)
        {
            _engine = engine;
            _parser = new XmlParser(engine);
        }
        
        public Form CreateWindow(XmlNode windowNode)
        {
            Form form = new Form();
            SetupForm(form);
            
            form.Text = _parser.GetAttribute(windowNode, "title", "Uvel Window");
            form.Width = _parser.GetIntAttribute(windowNode, "width", 400);
            form.Height = _parser.GetIntAttribute(windowNode, "height", 300);
            
            bool borderless = _parser.GetBoolAttribute(windowNode, "borderless", false);
            if (borderless)
            {
                form.FormBorderStyle = FormBorderStyle.None;
            }
            
            bool topmost = _parser.GetBoolAttribute(windowNode, "topmost", false);
            form.TopMost = topmost;
            
            bool resizable = _parser.GetBoolAttribute(windowNode, "resizable", true);
            if (!resizable)
            {
                form.FormBorderStyle = FormBorderStyle.FixedSingle;
                form.MaximizeBox = false;
            }
            
            // Parse UI
            XmlNode uiNode = windowNode.SelectSingleNode("UI");
            if (uiNode != null)
            {
                ParseUIChildren(form, uiNode, windowNode);
            }
            else
            {
                ParseUIChildren(form, windowNode, windowNode);
            }
            
            string id = _parser.GetAttribute(windowNode, "id", "window_" + _engine.Windows.Count);
            _engine.Controls[id] = form;
            
            // Resize event for responsive layout
            form.Resize += delegate(object s, EventArgs e)
            {
                ApplyResponsiveLayout(form);
            };
            
            return form;
        }
        
        public Form CreateWindowFromUI(XmlNode root, XmlNode uiNode)
        {
            Form form = new Form();
            SetupForm(form);
            
            form.Text = _parser.GetAttribute(root, "name", "Uvel App");
            form.Width = _parser.GetIntAttribute(root, "width", 400);
            form.Height = _parser.GetIntAttribute(root, "height", 300);
            
            bool borderless = _parser.GetBoolAttribute(root, "borderless", false);
            if (borderless)
            {
                form.FormBorderStyle = FormBorderStyle.None;
            }
            
            ParseUIChildren(form, uiNode, root);
            
            // Resize event for responsive layout
            form.Resize += delegate(object s, EventArgs e)
            {
                ApplyResponsiveLayout(form);
            };
            
            return form;
        }
        
        private void SetupForm(Form form)
        {
            form.BackColor = BgDark;
            form.ForeColor = TextWhite;
            form.Font = new Font("Segoe UI", 10);
            form.StartPosition = FormStartPosition.CenterScreen;
            
            // Enable double buffering via reflection
            SetDoubleBuffered(form);
        }
        
        private void SetDoubleBuffered(Control control)
        {
            try
            {
                typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                    .SetValue(control, true, null);
            }
            catch { }
        }
        
        // ═══════════════════════════════════════════
        // RESPONSIVE LAYOUT
        // ═══════════════════════════════════════════
        
        private void ApplyResponsiveLayout(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                // Check for layout tags
                if (child.Tag != null && child.Tag is LayoutInfo)
                {
                    LayoutInfo info = (LayoutInfo)child.Tag;
                    ApplyLayoutInfo(child, parent, info);
                }
                
                // Recursive
                if (child.Controls.Count > 0)
                {
                    ApplyResponsiveLayout(child);
                }
            }
        }
        
        private void ApplyLayoutInfo(Control control, Control parent, LayoutInfo info)
        {
            int parentW = parent.ClientSize.Width;
            int parentH = parent.ClientSize.Height;
            
            // Calculate size
            int w = control.Width;
            int h = control.Height;
            
            if (info.WidthPercent > 0)
            {
                w = (int)(parentW * info.WidthPercent / 100);
            }
            if (info.HeightPercent > 0)
            {
                h = (int)(parentH * info.HeightPercent / 100);
            }
            
            control.Size = new Size(w, h);
            
            // Calculate position
            int x = control.Left;
            int y = control.Top;
            
            if (info.CenterH)
            {
                x = (parentW - w) / 2;
            }
            if (info.CenterV)
            {
                y = (parentH - h) / 2;
            }
            if (info.XPercent > 0)
            {
                x = (int)(parentW * info.XPercent / 100);
            }
            if (info.YPercent > 0)
            {
                y = (int)(parentH * info.YPercent / 100);
            }
            
            control.Location = new Point(x, y);
        }
        
        private void ParseUIChildren(Control parent, XmlNode node, XmlNode rootNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                Control control = CreateControl(child, parent, rootNode);
                if (control != null)
                {
                    parent.Controls.Add(control);
                    
                    string id = _parser.GetAttribute(child, "id", "");
                    if (!string.IsNullOrEmpty(id))
                    {
                        _engine.Controls[id] = control;
                        control.Name = id;
                    }
                    
                    // Recursive for containers
                    if (IsContainer(child.Name) && child.HasChildNodes)
                    {
                        ParseUIChildren(control, child, rootNode);
                    }
                }
            }
        }
        
        private bool IsContainer(string type)
        {
            string t = type.ToLower();
            return t == "panel" || t == "card" || t == "div";
        }
        
        private Control CreateControl(XmlNode node, Control parent, XmlNode rootNode)
        {
            string type = node.Name.ToLower();
            Control control = null;
            
            switch (type)
            {
                case "button":
                    control = CreateButton(node, parent);
                    break;
                case "label":
                    control = CreateLabel(node);
                    break;
                case "textbox":
                case "input":
                    control = CreateTextBox(node);
                    break;
                case "panel":
                case "div":
                    control = CreatePanel(node);
                    break;
                case "card":
                    control = CreateCard(node);
                    break;
                case "grid":
                    control = CreateGrid(node, rootNode);
                    break;
                case "checkbox":
                    control = CreateCheckBox(node);
                    break;
                case "combobox":
                case "dropdown":
                    control = CreateComboBox(node);
                    break;
            }
            
            if (control != null)
            {
                ApplyCommonProperties(control, node, parent);
            }
            
            return control;
        }
        
        // ═══════════════════════════════════════════
        // iOS STYLE BUTTON - Full Custom with Ripple
        // ═══════════════════════════════════════════
        
        private Control CreateButton(XmlNode node, Control parent)
        {
            string text = _parser.GetAttribute(node, "text", node.InnerText.Trim());
            int radius = _parser.GetIntAttribute(node, "radius", 12);
            int fontSize = _parser.GetIntAttribute(node, "fontSize", 18);
            
            string bgColorStr = _parser.GetAttribute(node, "bgColor", "");
            string fgColorStr = _parser.GetAttribute(node, "color", "");
            
            Color bgColor = SurfaceLight;
            Color fgColor = TextWhite;
            
            if (!string.IsNullOrEmpty(bgColorStr))
            {
                try { bgColor = ColorTranslator.FromHtml(bgColorStr); }
                catch { }
            }
            
            if (!string.IsNullOrEmpty(fgColorStr))
            {
                try { fgColor = ColorTranslator.FromHtml(fgColorStr); }
                catch { }
            }
            
            // Create custom panel for button
            Panel btn = new Panel();
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Hand;
            SetDoubleBuffered(btn);
            
            // State
            bool isHovered = false;
            bool isPressed = false;
            
            // Ripple animation state
            float rippleRadius = 0;
            Point rippleCenter = Point.Empty;
            bool rippleActive = false;
            Timer rippleTimer = new Timer();
            rippleTimer.Interval = 16; // ~60fps
            
            // Paint event - smooth rendering with ripple
            btn.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                
                Rectangle rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                
                // Calculate current color
                Color drawColor = bgColor;
                if (isPressed)
                {
                    drawColor = DarkenColor(bgColor, 0.2f);
                }
                else if (isHovered)
                {
                    drawColor = LightenColor(bgColor, 0.1f);
                }
                
                // Draw rounded rectangle
                using (GraphicsPath path = CreateRoundedPath(rect, radius))
                {
                    // Background
                    using (SolidBrush brush = new SolidBrush(drawColor))
                    {
                        g.FillPath(brush, path);
                    }
                    
                    // Ripple effect
                    if (rippleActive && rippleRadius > 0)
                    {
                        g.SetClip(path);
                        int alpha = Math.Max(0, 60 - (int)(rippleRadius / 3));
                        using (SolidBrush rippleBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
                        {
                            g.FillEllipse(rippleBrush,
                                rippleCenter.X - rippleRadius,
                                rippleCenter.Y - rippleRadius,
                                rippleRadius * 2,
                                rippleRadius * 2);
                        }
                        g.ResetClip();
                    }
                }
                
                // Draw text centered
                if (!string.IsNullOrEmpty(text))
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        
                        using (Font font = new Font("Segoe UI", fontSize, FontStyle.Bold))
                        using (SolidBrush textBrush = new SolidBrush(fgColor))
                        {
                            g.DrawString(text, font, textBrush, rect, sf);
                        }
                    }
                }
            };
            
            // Ripple timer
            rippleTimer.Tick += delegate(object s, EventArgs e)
            {
                if (rippleActive)
                {
                    rippleRadius += 15;
                    float maxRadius = (float)Math.Sqrt(btn.Width * btn.Width + btn.Height * btn.Height);
                    if (rippleRadius > maxRadius)
                    {
                        rippleActive = false;
                        rippleRadius = 0;
                        rippleTimer.Stop();
                    }
                    btn.Invalidate();
                }
            };
            
            // Mouse events
            btn.MouseEnter += delegate(object s, EventArgs e)
            {
                isHovered = true;
                btn.Invalidate();
            };
            
            btn.MouseLeave += delegate(object s, EventArgs e)
            {
                isHovered = false;
                isPressed = false;
                btn.Invalidate();
            };
            
            btn.MouseDown += delegate(object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isPressed = true;
                    
                    // Start ripple
                    rippleCenter = e.Location;
                    rippleRadius = 0;
                    rippleActive = true;
                    rippleTimer.Start();
                    
                    btn.Invalidate();
                }
            };
            
            btn.MouseUp += delegate(object s, MouseEventArgs e)
            {
                isPressed = false;
                btn.Invalidate();
            };
            
            // Click handler
            string onClick = _parser.GetAttribute(node, "onClick", "");
            if (!string.IsNullOrEmpty(onClick))
            {
                btn.MouseClick += delegate(object s, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        _engine.ExecuteHandler(onClick, btn);
                    }
                };
            }
            
            // Set region for true rounded corners
            btn.Resize += delegate(object s, EventArgs e)
            {
                if (btn.Width > 0 && btn.Height > 0)
                {
                    using (GraphicsPath path = CreateRoundedPath(new Rectangle(0, 0, btn.Width, btn.Height), radius))
                    {
                        btn.Region = new Region(path);
                    }
                }
            };
            
            return btn;
        }
        
        // ═══════════════════════════════════════════
        // LABEL
        // ═══════════════════════════════════════════
        
        private Control CreateLabel(XmlNode node)
        {
            string text = _parser.GetAttribute(node, "text", node.InnerText.Trim());
            int fontSize = _parser.GetIntAttribute(node, "fontSize", 14);
            bool bold = _parser.GetBoolAttribute(node, "bold", false);
            bool secondary = _parser.GetBoolAttribute(node, "secondary", false);
            
            string colorStr = _parser.GetAttribute(node, "color", "");
            Color fgColor = secondary ? TextGray : TextWhite;
            
            if (!string.IsNullOrEmpty(colorStr))
            {
                try { fgColor = ColorTranslator.FromHtml(colorStr); }
                catch { }
            }
            
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = fgColor;
            lbl.BackColor = Color.Transparent;
            lbl.AutoSize = true;
            
            FontStyle style = bold ? FontStyle.Bold : FontStyle.Regular;
            lbl.Font = new Font("Segoe UI", fontSize, style);
            
            return lbl;
        }
        
        // ═══════════════════════════════════════════
        // TEXTBOX with placeholder
        // ═══════════════════════════════════════════
        
        private Control CreateTextBox(XmlNode node)
        {
            TextBox txt = new TextBox();
            txt.BackColor = SurfaceDark;
            txt.ForeColor = TextWhite;
            txt.BorderStyle = BorderStyle.None;
            txt.Font = new Font("Segoe UI", 12);
            txt.Text = _parser.GetAttribute(node, "text", "");
            
            bool multiline = _parser.GetBoolAttribute(node, "multiline", false);
            txt.Multiline = multiline;
            if (multiline)
            {
                txt.ScrollBars = ScrollBars.Vertical;
            }
            
            string placeholder = _parser.GetAttribute(node, "placeholder", "");
            if (!string.IsNullOrEmpty(placeholder) && string.IsNullOrEmpty(txt.Text))
            {
                txt.Text = placeholder;
                txt.ForeColor = TextGray;
                
                txt.GotFocus += delegate(object s, EventArgs e)
                {
                    if (txt.Text == placeholder)
                    {
                        txt.Text = "";
                        txt.ForeColor = TextWhite;
                    }
                };
                
                txt.LostFocus += delegate(object s, EventArgs e)
                {
                    if (string.IsNullOrEmpty(txt.Text))
                    {
                        txt.Text = placeholder;
                        txt.ForeColor = TextGray;
                    }
                };
            }
            
            return txt;
        }
        
        // ═══════════════════════════════════════════
        // PANEL with scroll support
        // ═══════════════════════════════════════════
        
        private Control CreatePanel(XmlNode node)
        {
            bool scrollable = _parser.GetBoolAttribute(node, "scrollable", false);
            
            Panel panel = new Panel();
            panel.BackColor = Color.Transparent;
            SetDoubleBuffered(panel);
            
            if (scrollable)
            {
                panel.AutoScroll = true;
            }
            
            string bgColorStr = _parser.GetAttribute(node, "bgColor", "");
            if (!string.IsNullOrEmpty(bgColorStr))
            {
                try { panel.BackColor = ColorTranslator.FromHtml(bgColorStr); }
                catch { }
            }
            
            return panel;
        }
        
        // ═══════════════════════════════════════════
        // CARD - With proper shadow and rounded corners
        // ═══════════════════════════════════════════
        
        private Control CreateCard(XmlNode node)
        {
            int radius = _parser.GetIntAttribute(node, "radius", 16);
            int elevation = _parser.GetIntAttribute(node, "elevation", 2);
            
            string bgColorStr = _parser.GetAttribute(node, "bgColor", "");
            Color bgColor = SurfaceDark;
            
            if (!string.IsNullOrEmpty(bgColorStr))
            {
                try { bgColor = ColorTranslator.FromHtml(bgColorStr); }
                catch { }
            }
            
            Panel card = new Panel();
            card.BackColor = Color.Transparent;
            SetDoubleBuffered(card);
            
            card.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                
                // Clear background
                if (card.Parent != null)
                {
                    g.Clear(card.Parent.BackColor);
                }
                
                int shadowOffset = elevation * 2;
                Rectangle cardRect = new Rectangle(
                    shadowOffset / 2,
                    shadowOffset / 2,
                    card.Width - shadowOffset - 1,
                    card.Height - shadowOffset - 1);
                
                // Draw shadow layers
                if (elevation > 0)
                {
                    for (int i = elevation; i > 0; i--)
                    {
                        int offset = i * 2;
                        Rectangle shadowRect = new Rectangle(
                            offset / 2 + 1,
                            offset / 2 + 2,
                            card.Width - offset - 2,
                            card.Height - offset - 2);
                        
                        int alpha = 15 + (elevation - i) * 5;
                        using (GraphicsPath shadowPath = CreateRoundedPath(shadowRect, radius))
                        using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                        {
                            g.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }
                
                // Draw card background
                using (GraphicsPath path = CreateRoundedPath(cardRect, radius))
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    g.FillPath(brush, path);
                }
            };
            
            return card;
        }
        
        // ═══════════════════════════════════════════
        // GRID
        // ═══════════════════════════════════════════
        
        private Control CreateGrid(XmlNode node, XmlNode rootNode)
        {
            int cols = _parser.GetIntAttribute(node, "columns", 1);
            int rows = _parser.GetIntAttribute(node, "rows", 1);
            int spacing = _parser.GetIntAttribute(node, "spacing", 4);
            
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.ColumnCount = cols;
            grid.RowCount = rows;
            grid.BackColor = Color.Transparent;
            grid.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            grid.Margin = new Padding(0);
            grid.Padding = new Padding(0);
            
            grid.ColumnStyles.Clear();
            grid.RowStyles.Clear();
            
            float colWidth = 100f / cols;
            float rowHeight = 100f / rows;
            
            for (int i = 0; i < cols; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, colWidth));
            }
            
            for (int i = 0; i < rows; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, rowHeight));
            }
            
            SetDoubleBuffered(grid);
            
            // Parse grid children
            int childIndex = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                
                Control control = CreateControl(child, grid, rootNode);
                if (control != null)
                {
                    int col = childIndex % cols;
                    int row = childIndex / cols;
                    
                    if (row < rows)
                    {
                        control.Dock = DockStyle.Fill;
                        control.Margin = new Padding(spacing);
                        grid.Controls.Add(control, col, row);
                        
                        string id = _parser.GetAttribute(child, "id", "");
                        if (!string.IsNullOrEmpty(id))
                        {
                            _engine.Controls[id] = control;
                            control.Name = id;
                        }
                    }
                    
                    childIndex++;
                }
            }
            
            return grid;
        }
        
        // ═══════════════════════════════════════════
        // OTHER CONTROLS
        // ═══════════════════════════════════════════
        
        private Control CreateCheckBox(XmlNode node)
        {
            CheckBox chk = new CheckBox();
            chk.Text = _parser.GetAttribute(node, "text", "");
            chk.Checked = _parser.GetBoolAttribute(node, "checked", false);
            chk.ForeColor = TextWhite;
            chk.BackColor = Color.Transparent;
            chk.Font = new Font("Segoe UI", 10);
            chk.AutoSize = true;
            
            return chk;
        }
        
        private Control CreateComboBox(XmlNode node)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.BackColor = SurfaceDark;
            combo.ForeColor = TextWhite;
            combo.Font = new Font("Segoe UI", 10);
            combo.FlatStyle = FlatStyle.Flat;
            
            string items = _parser.GetAttribute(node, "items", "");
            if (!string.IsNullOrEmpty(items))
            {
                string[] itemArray = items.Split(',');
                foreach (string item in itemArray)
                {
                    combo.Items.Add(item.Trim());
                }
                if (combo.Items.Count > 0)
                {
                    combo.SelectedIndex = 0;
                }
            }
            
            return combo;
        }
        
        // ═══════════════════════════════════════════
        // COMMON PROPERTIES with layout support
        // ═══════════════════════════════════════════
        
        private void ApplyCommonProperties(Control control, XmlNode node, Control parent)
        {
            // Layout info for responsive
            LayoutInfo layoutInfo = new LayoutInfo();
            
            // Position
            int x = _parser.GetIntAttribute(node, "x", -1);
            int y = _parser.GetIntAttribute(node, "y", -1);
            if (x >= 0 && y >= 0)
            {
                control.Location = new Point(x, y);
            }
            
            // Size
            int width = _parser.GetIntAttribute(node, "width", -1);
            int height = _parser.GetIntAttribute(node, "height", -1);
            if (width > 0) control.Width = width;
            if (height > 0) control.Height = height;
            
            // Percentage-based layout (e.g., width="50%")
            string widthStr = _parser.GetAttribute(node, "width", "");
            string heightStr = _parser.GetAttribute(node, "height", "");
            
            if (widthStr.EndsWith("%"))
            {
                float pct = 0;
                if (float.TryParse(widthStr.TrimEnd('%'), out pct))
                {
                    layoutInfo.WidthPercent = pct;
                    if (parent != null)
                    {
                        control.Width = (int)(parent.ClientSize.Width * pct / 100);
                    }
                }
            }
            
            if (heightStr.EndsWith("%"))
            {
                float pct = 0;
                if (float.TryParse(heightStr.TrimEnd('%'), out pct))
                {
                    layoutInfo.HeightPercent = pct;
                    if (parent != null)
                    {
                        control.Height = (int)(parent.ClientSize.Height * pct / 100);
                    }
                }
            }
            
            // Centering
            string align = _parser.GetAttribute(node, "align", "").ToLower();
            if (align.Contains("center"))
            {
                layoutInfo.CenterH = true;
                if (parent != null)
                {
                    control.Left = (parent.ClientSize.Width - control.Width) / 2;
                }
            }
            
            string valign = _parser.GetAttribute(node, "valign", "").ToLower();
            if (valign.Contains("center"))
            {
                layoutInfo.CenterV = true;
                if (parent != null)
                {
                    control.Top = (parent.ClientSize.Height - control.Height) / 2;
                }
            }
            
            // Store layout info
            if (layoutInfo.HasLayout())
            {
                control.Tag = layoutInfo;
            }
            
            // Dock
            string dock = _parser.GetAttribute(node, "dock", "").ToLower();
            switch (dock)
            {
                case "fill": control.Dock = DockStyle.Fill; break;
                case "top": control.Dock = DockStyle.Top; break;
                case "bottom": control.Dock = DockStyle.Bottom; break;
                case "left": control.Dock = DockStyle.Left; break;
                case "right": control.Dock = DockStyle.Right; break;
            }
            
            // Anchor
            string anchor = _parser.GetAttribute(node, "anchor", "").ToLower();
            if (!string.IsNullOrEmpty(anchor))
            {
                AnchorStyles anchors = AnchorStyles.None;
                if (anchor.Contains("top")) anchors = anchors | AnchorStyles.Top;
                if (anchor.Contains("bottom")) anchors = anchors | AnchorStyles.Bottom;
                if (anchor.Contains("left")) anchors = anchors | AnchorStyles.Left;
                if (anchor.Contains("right")) anchors = anchors | AnchorStyles.Right;
                control.Anchor = anchors;
            }
            
            // Visibility
            control.Visible = _parser.GetBoolAttribute(node, "visible", true);
            control.Enabled = _parser.GetBoolAttribute(node, "enabled", true);
            
            // Margin/Padding
            int margin = _parser.GetIntAttribute(node, "margin", -1);
            if (margin >= 0) control.Margin = new Padding(margin);
            
            int padding = _parser.GetIntAttribute(node, "padding", -1);
            if (padding >= 0) control.Padding = new Padding(padding);
        }
        
        // ═══════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════
        
        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            
            int diameter = radius * 2;
            
            // Top left
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Top right
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Bottom right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Bottom left
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }
        
        private Color LightenColor(Color color, float amount)
        {
            int r = (int)Math.Min(255, color.R + 255 * amount);
            int g = (int)Math.Min(255, color.G + 255 * amount);
            int b = (int)Math.Min(255, color.B + 255 * amount);
            return Color.FromArgb(color.A, r, g, b);
        }
        
        private Color DarkenColor(Color color, float amount)
        {
            int r = (int)Math.Max(0, color.R - 255 * amount);
            int g = (int)Math.Max(0, color.G - 255 * amount);
            int b = (int)Math.Max(0, color.B - 255 * amount);
            return Color.FromArgb(color.A, r, g, b);
        }
    }
    
    /// <summary>
    /// Layout information for responsive design
    /// </summary>
    public class LayoutInfo
    {
        public float WidthPercent { get; set; }
        public float HeightPercent { get; set; }
        public float XPercent { get; set; }
        public float YPercent { get; set; }
        public bool CenterH { get; set; }
        public bool CenterV { get; set; }
        
        public bool HasLayout()
        {
            return WidthPercent > 0 || HeightPercent > 0 || 
                   XPercent > 0 || YPercent > 0 || 
                   CenterH || CenterV;
        }
    }
}
