using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using Uvel.UI.Styles;
using Uvel.UI.Layout;
using Uvel.UI.Components;
using Uvel.UI.Effects;

namespace Uvel.UI.Core
{
    /// <summary>
    /// Main UI Renderer - creates controls from XML definitions
    /// </summary>
    public class UIRenderer
    {
        private UvelEngine _engine;
        private Dictionary<string, Control> _controls;
        
        public UIRenderer(UvelEngine engine)
        {
            _engine = engine;
            _controls = new Dictionary<string, Control>();
        }
        
        // ═══════════════════════════════════════════
        // WINDOW CREATION
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Create window from XML node
        /// </summary>
        public Form CreateWindow(XmlNode windowNode, XmlNode rootNode)
        {
            UIWindow window = new UIWindow();
            
            // Get attributes
            string title = GetAttr(windowNode, "title", GetAttr(rootNode, "name", "Uvel App"));
            int width = GetIntAttr(windowNode, "width", GetIntAttr(rootNode, "width", 400));
            int height = GetIntAttr(windowNode, "height", GetIntAttr(rootNode, "height", 300));
            bool borderless = GetBoolAttr(windowNode, "borderless", GetBoolAttr(rootNode, "borderless", false));
            bool topmost = GetBoolAttr(windowNode, "topmost", false);
            bool resizable = GetBoolAttr(windowNode, "resizable", true);
            int cornerRadius = GetIntAttr(windowNode, "cornerRadius", 0);
            
            // Apply properties
            window.Text = title;
            window.Width = width;
            window.Height = height;
            window.TopMost = topmost;
            window.IsResizable = resizable;
            window.IsBorderless = borderless;
            window.WindowCornerRadius = cornerRadius;
            
            // Theme
            string theme = GetAttr(rootNode, "theme", "dark");
            Theme.CurrentTheme = theme;
            window.BackColor = Theme.Background;
            window.ForeColor = Theme.TextPrimary;
            
            // Parse UI children
            XmlNode uiNode = windowNode.SelectSingleNode("UI");
            if (uiNode == null)
            {
                uiNode = windowNode;
            }
            
            ParseUIChildren(window, uiNode);
            
            return window;
        }
        
        /// <summary>
        /// Create window from root node with UI section
        /// </summary>
        public Form CreateWindowFromUI(XmlNode rootNode, XmlNode uiNode)
        {
            UIWindow window = new UIWindow();
            
            string title = GetAttr(rootNode, "name", "Uvel App");
            int width = GetIntAttr(rootNode, "width", 400);
            int height = GetIntAttr(rootNode, "height", 300);
            bool borderless = GetBoolAttr(rootNode, "borderless", false);
            
            window.Text = title;
            window.Width = width;
            window.Height = height;
            window.IsBorderless = borderless;
            
            string theme = GetAttr(rootNode, "theme", "dark");
            Theme.CurrentTheme = theme;
            window.BackColor = Theme.Background;
            window.ForeColor = Theme.TextPrimary;
            
            ParseUIChildren(window, uiNode);
            
            return window;
        }
        
        // ═══════════════════════════════════════════
        // UI PARSING
        // ═══════════════════════════════════════════
        
        private void ParseUIChildren(Control parent, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                
                Control control = CreateControl(child);
                if (control != null)
                {
                    parent.Controls.Add(control);
                    
                    string id = GetAttr(child, "id", "");
                    if (!string.IsNullOrEmpty(id))
                    {
                        _controls[id] = control;
                        control.Name = id;
                        _engine.Controls[id] = control;
                    }
                    
                    // Recursive for containers
                    if (IsContainer(child.Name) && child.HasChildNodes)
                    {
                        ParseUIChildren(control, child);
                    }
                }
            }
        }
        
        private bool IsContainer(string type)
        {
            string t = type.ToLower();
            return t == "panel" || t == "card" || t == "div" || t == "container" || t == "grid";
        }
        
        // ═══════════════════════════════════════════
        // CONTROL CREATION
        // ═══════════════════════════════════════════
        
        private Control CreateControl(XmlNode node)
        {
            string type = node.Name.ToLower();
            Control control = null;
            
            switch (type)
            {
                case "button":
                    control = CreateButton(node);
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
                case "container":
                    control = CreatePanel(node);
                    break;
                    
                case "card":
                    control = CreateCard(node);
                    break;
                    
                case "grid":
                    control = CreateGrid(node);
                    break;
                    
                case "checkbox":
                    control = CreateCheckBox(node);
                    break;
                    
                case "combobox":
                case "dropdown":
                    control = CreateComboBox(node);
                    break;
                    
                case "image":
                case "picture":
                    control = CreateImage(node);
                    break;
            }
            
            if (control != null)
            {
                ApplyCommonProperties(control, node);
            }
            
            return control;
        }
        
        // ═══════════════════════════════════════════
        // BUTTON
        // ═══════════════════════════════════════════
        
        private Control CreateButton(XmlNode node)
        {
            string text = GetAttr(node, "text", node.InnerText.Trim());
            bool isPrimary = GetBoolAttr(node, "primary", false);
            bool isFab = GetBoolAttr(node, "fab", false);
            int radius = GetIntAttr(node, "radius", 12);
            int elevation = GetIntAttr(node, "elevation", 2);
            
            UIButton btn = new UIButton();
            btn.Text = text;
            btn.IsPrimary = isPrimary;
            btn.CornerRadius = radius;
            btn.Elevation = elevation;
            
            // Custom colors
            string bgColor = GetAttr(node, "bgColor", "");
            if (!string.IsNullOrEmpty(bgColor))
            {
                btn.BackColor = ParseColor(bgColor);
            }
            
            string fgColor = GetAttr(node, "color", "");
            if (!string.IsNullOrEmpty(fgColor))
            {
                btn.ForeColor = ParseColor(fgColor);
            }
            
            // Click event
            string onClick = GetAttr(node, "onClick", "");
            if (!string.IsNullOrEmpty(onClick))
            {
                btn.Click += delegate(object s, EventArgs e)
                {
                    _engine.ExecuteHandler(onClick, btn);
                };
            }
            
            return btn;
        }
        
        // ═══════════════════════════════════════════
        // LABEL
        // ═══════════════════════════════════════════
        
        private Control CreateLabel(XmlNode node)
        {
            string text = GetAttr(node, "text", node.InnerText.Trim());
            int fontSize = GetIntAttr(node, "fontSize", 14);
            bool bold = GetBoolAttr(node, "bold", false);
            bool secondary = GetBoolAttr(node, "secondary", false);
            
            UILabel lbl = new UILabel();
            lbl.Text = text;
            lbl.FontSize = fontSize;
            lbl.IsBold = bold;
            lbl.IsSecondary = secondary;
            
            string color = GetAttr(node, "color", "");
            if (!string.IsNullOrEmpty(color))
            {
                lbl.ForeColor = ParseColor(color);
            }
            
            return lbl;
        }
        
        // ═══════════════════════════════════════════
        // TEXTBOX
        // ═══════════════════════════════════════════
        
        private Control CreateTextBox(XmlNode node)
        {
            string text = GetAttr(node, "text", "");
            string placeholder = GetAttr(node, "placeholder", "");
            bool multiline = GetBoolAttr(node, "multiline", false);
            bool readOnly = GetBoolAttr(node, "readonly", false);
            int radius = GetIntAttr(node, "radius", 8);
            
            UITextBox txt = new UITextBox();
            txt.Text = text;
            txt.Placeholder = placeholder;
            txt.IsMultiline = multiline;
            txt.IsReadOnly = readOnly;
            txt.CornerRadius = radius;
            
            // TextChanged event
            string onTextChanged = GetAttr(node, "onTextChanged", "");
            if (!string.IsNullOrEmpty(onTextChanged))
            {
                txt.TextChanged += delegate(object s, EventArgs e)
                {
                    _engine.ExecuteHandler(onTextChanged, txt);
                };
            }
            
            return txt;
        }
        
        // ═══════════════════════════════════════════
        // PANEL
        // ═══════════════════════════════════════════
        
        private Control CreatePanel(XmlNode node)
        {
            int radius = GetIntAttr(node, "radius", 0);
            
            UIPanel panel = new UIPanel();
            panel.CornerRadius = radius;
            
            string bgColor = GetAttr(node, "bgColor", "");
            if (!string.IsNullOrEmpty(bgColor))
            {
                panel.BackColor = ParseColor(bgColor);
            }
            
            return panel;
        }
        
        // ═══════════════════════════════════════════
        // CARD
        // ═══════════════════════════════════════════
        
        private Control CreateCard(XmlNode node)
        {
            int radius = GetIntAttr(node, "radius", 16);
            int elevation = GetIntAttr(node, "elevation", 2);
            
            UICard card = new UICard();
            card.CornerRadius = radius;
            card.Elevation = elevation;
            
            string bgColor = GetAttr(node, "bgColor", "");
            if (!string.IsNullOrEmpty(bgColor))
            {
                card.BackColor = ParseColor(bgColor);
            }
            else
            {
                card.BackColor = Theme.Surface;
            }
            
            return card;
        }
        
        // ═══════════════════════════════════════════
        // GRID
        // ═══════════════════════════════════════════
        
        private Control CreateGrid(XmlNode node)
        {
            int cols = GetIntAttr(node, "columns", 1);
            int rows = GetIntAttr(node, "rows", 1);
            int spacing = GetIntAttr(node, "spacing", 4);
            
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.ColumnCount = cols;
            grid.RowCount = rows;
            grid.BackColor = Color.Transparent;
            grid.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            
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
            
            // Parse grid children
            int childIndex = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                
                Control control = CreateControl(child);
                if (control != null)
                {
                    int col = childIndex % cols;
                    int row = childIndex / cols;
                    
                    if (row < rows)
                    {
                        control.Dock = DockStyle.Fill;
                        control.Margin = new Padding(spacing);
                        grid.Controls.Add(control, col, row);
                    }
                    
                    string id = GetAttr(child, "id", "");
                    if (!string.IsNullOrEmpty(id))
                    {
                        _controls[id] = control;
                        control.Name = id;
                        _engine.Controls[id] = control;
                    }
                    
                    childIndex++;
                }
            }
            
            return grid;
        }
        
        // ═══════════════════════════════════════════
        // CHECKBOX
        // ═══════════════════════════════════════════
        
        private Control CreateCheckBox(XmlNode node)
        {
            CheckBox chk = new CheckBox();
            chk.Text = GetAttr(node, "text", "");
            chk.Checked = GetBoolAttr(node, "checked", false);
            chk.ForeColor = Theme.TextPrimary;
            chk.BackColor = Color.Transparent;
            chk.Font = new Font("Segoe UI", 10);
            chk.AutoSize = true;
            
            return chk;
        }
        
        // ═══════════════════════════════════════════
        // COMBOBOX
        // ═══════════════════════════════════════════
        
        private Control CreateComboBox(XmlNode node)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.BackColor = Theme.Surface;
            combo.ForeColor = Theme.TextPrimary;
            combo.Font = new Font("Segoe UI", 10);
            combo.FlatStyle = FlatStyle.Flat;
            
            string items = GetAttr(node, "items", "");
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
        // IMAGE
        // ═══════════════════════════════════════════
        
        private Control CreateImage(XmlNode node)
        {
            PictureBox pic = new PictureBox();
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.BackColor = Color.Transparent;
            
            string src = GetAttr(node, "src", "");
            if (!string.IsNullOrEmpty(src))
            {
                try
                {
                    string fullPath = System.IO.Path.Combine(Program.AppDirectory, src);
                    if (System.IO.File.Exists(fullPath))
                    {
                        pic.Image = Image.FromFile(fullPath);
                    }
                }
                catch { }
            }
            
            return pic;
        }
        
        // ═══════════════════════════════════════════
        // COMMON PROPERTIES
        // ═══════════════════════════════════════════
        
        private void ApplyCommonProperties(Control control, XmlNode node)
        {
            // Position
            int x = GetIntAttr(node, "x", -1);
            int y = GetIntAttr(node, "y", -1);
            if (x >= 0 && y >= 0)
            {
                control.Location = new Point(x, y);
            }
            
            // Size
            int width = GetIntAttr(node, "width", -1);
            int height = GetIntAttr(node, "height", -1);
            if (width > 0) control.Width = width;
            if (height > 0) control.Height = height;
            
            // Dock
            string dock = GetAttr(node, "dock", "").ToLower();
            switch (dock)
            {
                case "fill": control.Dock = DockStyle.Fill; break;
                case "top": control.Dock = DockStyle.Top; break;
                case "bottom": control.Dock = DockStyle.Bottom; break;
                case "left": control.Dock = DockStyle.Left; break;
                case "right": control.Dock = DockStyle.Right; break;
            }
            
            // Anchor
            string anchor = GetAttr(node, "anchor", "").ToLower();
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
            control.Visible = GetBoolAttr(node, "visible", true);
            control.Enabled = GetBoolAttr(node, "enabled", true);
            
            // Margin
            int margin = GetIntAttr(node, "margin", -1);
            if (margin >= 0)
            {
                control.Margin = new Padding(margin);
            }
            
            // Padding
            int padding = GetIntAttr(node, "padding", -1);
            if (padding >= 0)
            {
                control.Padding = new Padding(padding);
            }
        }
        
        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════
        
        private string GetAttr(XmlNode node, string name, string defaultValue)
        {
            if (node.Attributes != null && node.Attributes[name] != null)
            {
                return node.Attributes[name].Value;
            }
            return defaultValue;
        }
        
        private int GetIntAttr(XmlNode node, string name, int defaultValue)
        {
            string val = GetAttr(node, name, "");
            int result;
            if (int.TryParse(val, out result))
            {
                return result;
            }
            return defaultValue;
        }
        
        private bool GetBoolAttr(XmlNode node, string name, bool defaultValue)
        {
            string val = GetAttr(node, name, "").ToLower();
            if (val == "true" || val == "1" || val == "yes")
                return true;
            if (val == "false" || val == "0" || val == "no")
                return false;
            return defaultValue;
        }
        
        private Color ParseColor(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr))
            {
                return Color.Transparent;
            }
            
            try
            {
                if (colorStr.StartsWith("#"))
                {
                    return ColorTranslator.FromHtml(colorStr);
                }
                return Color.FromName(colorStr);
            }
            catch
            {
                return Color.Transparent;
            }
        }
    }
}
