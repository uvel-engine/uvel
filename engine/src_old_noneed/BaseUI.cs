using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Uvel
{
    /// <summary>
    /// Base UI - Default styles and theme management
    /// All UI definitions are in C# for safety (csc.exe compatible)
    /// Compatible with .NET Framework 4.0 (no C# 6.0 features)
    /// </summary>
    public static class BaseUI
    {
        // Current theme: "dark" or "light"
        public static string CurrentTheme = "dark";
        
        // ===========================================
        // THEME COLORS
        // ===========================================
        
        public static class Dark
        {
            public static Color Background = Color.FromArgb(30, 30, 35);
            public static Color Surface = Color.FromArgb(45, 45, 50);
            public static Color SurfaceLight = Color.FromArgb(60, 60, 65);
            public static Color Primary = Color.FromArgb(100, 150, 255);
            public static Color PrimaryHover = Color.FromArgb(130, 170, 255);
            public static Color Text = Color.FromArgb(240, 240, 240);
            public static Color TextSecondary = Color.FromArgb(160, 160, 160);
            public static Color Border = Color.FromArgb(70, 70, 75);
            public static Color Success = Color.FromArgb(80, 200, 120);
            public static Color Warning = Color.FromArgb(255, 180, 50);
            public static Color Error = Color.FromArgb(255, 100, 100);
        }
        
        public static class Light
        {
            public static Color Background = Color.FromArgb(245, 245, 250);
            public static Color Surface = Color.FromArgb(255, 255, 255);
            public static Color SurfaceLight = Color.FromArgb(235, 235, 240);
            public static Color Primary = Color.FromArgb(60, 120, 220);
            public static Color PrimaryHover = Color.FromArgb(80, 140, 240);
            public static Color Text = Color.FromArgb(30, 30, 35);
            public static Color TextSecondary = Color.FromArgb(100, 100, 110);
            public static Color Border = Color.FromArgb(200, 200, 210);
            public static Color Success = Color.FromArgb(60, 180, 100);
            public static Color Warning = Color.FromArgb(230, 160, 30);
            public static Color Error = Color.FromArgb(220, 80, 80);
        }
        
        // ===========================================
        // HELPER: GET CURRENT THEME COLOR
        // ===========================================
        
        public static Color GetBackground()
        {
            if (CurrentTheme == "dark")
                return Dark.Background;
            return Light.Background;
        }
        
        public static Color GetSurface()
        {
            if (CurrentTheme == "dark")
                return Dark.Surface;
            return Light.Surface;
        }
        
        public static Color GetSurfaceLight()
        {
            if (CurrentTheme == "dark")
                return Dark.SurfaceLight;
            return Light.SurfaceLight;
        }
        
        public static Color GetPrimary()
        {
            if (CurrentTheme == "dark")
                return Dark.Primary;
            return Light.Primary;
        }
        
        public static Color GetPrimaryHover()
        {
            if (CurrentTheme == "dark")
                return Dark.PrimaryHover;
            return Light.PrimaryHover;
        }
        
        public static Color GetText()
        {
            if (CurrentTheme == "dark")
                return Dark.Text;
            return Light.Text;
        }
        
        public static Color GetTextSecondary()
        {
            if (CurrentTheme == "dark")
                return Dark.TextSecondary;
            return Light.TextSecondary;
        }
        
        public static Color GetBorder()
        {
            if (CurrentTheme == "dark")
                return Dark.Border;
            return Light.Border;
        }
        
        // ===========================================
        // FONTS
        // ===========================================
        
        public static Font FontRegular = new Font("Segoe UI", 10);
        public static Font FontMedium = new Font("Segoe UI", 11);
        public static Font FontLarge = new Font("Segoe UI", 14);
        public static Font FontTitle = new Font("Segoe UI", 18, FontStyle.Bold);
        public static Font FontMono = new Font("Consolas", 11);
        
        // ===========================================
        // FORM STYLES
        // ===========================================
        
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = GetBackground();
            form.ForeColor = GetText();
            form.Font = FontRegular;
            form.StartPosition = FormStartPosition.CenterScreen;
            
            // Enable double buffering for smooth rendering
            typeof(Form).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic)
                .SetValue(form, true, null);
        }
        
        /// <summary>
        /// Apply fake blur/acrylic effect
        /// </summary>
        public static void ApplyFakeBlur(Form form)
        {
            form.BackColor = Color.FromArgb(200, 30, 30, 40);
            form.TransparencyKey = Color.Empty;
            form.Opacity = 0.95;
            
            // Add gradient overlay
            form.Paint += delegate(object s, PaintEventArgs e)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    form.ClientRectangle,
                    Color.FromArgb(40, 100, 150, 255),
                    Color.FromArgb(20, 50, 100, 200),
                    LinearGradientMode.ForwardDiagonal))
                {
                    e.Graphics.FillRectangle(brush, form.ClientRectangle);
                }
            };
        }
        
        // ===========================================
        // CONTROL STYLES
        // ===========================================
        
        public static void ApplyButtonStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = GetSurface();
            btn.ForeColor = GetText();
            btn.Font = FontMedium;
            btn.Cursor = Cursors.Hand;
            btn.Padding = new Padding(15, 8, 15, 8);
            
            // Hover effects
            btn.MouseEnter += delegate(object s, EventArgs e)
            {
                btn.BackColor = GetSurfaceLight();
            };
            btn.MouseLeave += delegate(object s, EventArgs e)
            {
                btn.BackColor = GetSurface();
            };
        }
        
        public static void ApplyPrimaryButtonStyle(Button btn)
        {
            ApplyButtonStyle(btn);
            btn.BackColor = GetPrimary();
            btn.ForeColor = Color.White;
            
            btn.MouseEnter += delegate(object s, EventArgs e)
            {
                btn.BackColor = GetPrimaryHover();
            };
            btn.MouseLeave += delegate(object s, EventArgs e)
            {
                btn.BackColor = GetPrimary();
            };
        }
        
        public static void ApplyLabelStyle(Label lbl)
        {
            lbl.ForeColor = GetText();
            lbl.Font = FontRegular;
            lbl.BackColor = Color.Transparent;
        }
        
        public static void ApplyTextBoxStyle(TextBox txt)
        {
            txt.BackColor = GetSurface();
            txt.ForeColor = GetText();
            txt.Font = FontRegular;
            txt.BorderStyle = BorderStyle.FixedSingle;
        }
        
        public static void ApplyPanelStyle(Control panel)
        {
            panel.BackColor = GetBackground();
        }
        
        public static void ApplyCheckBoxStyle(CheckBox chk)
        {
            chk.ForeColor = GetText();
            chk.Font = FontRegular;
            chk.BackColor = Color.Transparent;
        }
        
        public static void ApplyListBoxStyle(ListBox list)
        {
            list.BackColor = GetSurface();
            list.ForeColor = GetText();
            list.Font = FontRegular;
            list.BorderStyle = BorderStyle.FixedSingle;
        }
        
        public static void ApplyComboBoxStyle(ComboBox combo)
        {
            combo.BackColor = GetSurface();
            combo.ForeColor = GetText();
            combo.Font = FontRegular;
            combo.FlatStyle = FlatStyle.Flat;
        }
        
        // ===========================================
        // CUSTOM CONTROLS
        // ===========================================
        
        /// <summary>
        /// Create rounded button
        /// </summary>
        public static Button CreateRoundedButton(string text, int radius)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            
            ApplyButtonStyle(btn);
            
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = CreateRoundedRegion(btn.Width, btn.Height, radius);
            
            btn.Resize += delegate(object s, EventArgs e)
            {
                btn.Region = CreateRoundedRegion(btn.Width, btn.Height, radius);
            };
            
            return btn;
        }
        
        public static Button CreateRoundedButton(string text)
        {
            return CreateRoundedButton(text, 8);
        }
        
        /// <summary>
        /// Create rounded region
        /// </summary>
        private static Region CreateRoundedRegion(int width, int height, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            return new Region(path);
        }
        
        // ===========================================
        // UTILITY
        // ===========================================
        
        /// <summary>
        /// Create display/result textbox (calculator style)
        /// </summary>
        public static TextBox CreateDisplayBox()
        {
            TextBox txt = new TextBox();
            txt.ReadOnly = true;
            txt.TextAlign = HorizontalAlignment.Right;
            txt.Font = new Font("Consolas", 24, FontStyle.Bold);
            txt.BackColor = GetSurface();
            txt.ForeColor = GetText();
            txt.BorderStyle = BorderStyle.None;
            txt.Text = "0";
            return txt;
        }
        
        /// <summary>
        /// Create calculator button
        /// </summary>
        public static Button CreateCalcButton(string text, bool isOperator, bool isEqual)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(2);
            
            btn.FlatAppearance.BorderSize = 0;
            
            if (isEqual)
            {
                btn.BackColor = GetPrimary();
                btn.ForeColor = Color.White;
            }
            else if (isOperator)
            {
                btn.BackColor = GetSurfaceLight();
                btn.ForeColor = GetPrimary();
            }
            else
            {
                btn.BackColor = GetSurface();
                btn.ForeColor = GetText();
            }
            
            Color originalColor = btn.BackColor;
            btn.MouseEnter += delegate(object s, EventArgs e)
            {
                btn.BackColor = ControlPaint.Light(originalColor, 0.2f);
            };
            btn.MouseLeave += delegate(object s, EventArgs e)
            {
                btn.BackColor = originalColor;
            };
            
            return btn;
        }
        
        public static Button CreateCalcButton(string text)
        {
            return CreateCalcButton(text, false, false);
        }
    }
}
