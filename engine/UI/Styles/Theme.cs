using System;
using System.Drawing;

namespace Uvel.UI.Styles
{
    /// <summary>
    /// Theme manager - colors, fonts, sizes
    /// Supports dark and light themes
    /// </summary>
    public static class Theme
    {
        // ═══════════════════════════════════════════
        // CURRENT THEME
        // ═══════════════════════════════════════════
        
        private static string _currentTheme = "dark";
        
        public static string CurrentTheme
        {
            get { return _currentTheme; }
            set
            {
                _currentTheme = value.ToLower();
                UpdateColors();
            }
        }
        
        // ═══════════════════════════════════════════
        // DYNAMIC COLORS
        // ═══════════════════════════════════════════
        
        public static Color Background { get; private set; }
        public static Color Surface { get; private set; }
        public static Color SurfaceVariant { get; private set; }
        public static Color Primary { get; private set; }
        public static Color PrimaryVariant { get; private set; }
        public static Color Secondary { get; private set; }
        public static Color TextPrimary { get; private set; }
        public static Color TextSecondary { get; private set; }
        public static Color TextDisabled { get; private set; }
        public static Color Border { get; private set; }
        public static Color Divider { get; private set; }
        public static Color Error { get; private set; }
        public static Color Success { get; private set; }
        public static Color Warning { get; private set; }
        public static Color Shadow { get; private set; }
        public static Color Ripple { get; private set; }
        public static Color Hover { get; private set; }
        public static Color Pressed { get; private set; }
        
        // ═══════════════════════════════════════════
        // STATIC CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        static Theme()
        {
            UpdateColors();
        }
        
        // ═══════════════════════════════════════════
        // UPDATE COLORS
        // ═══════════════════════════════════════════
        
        private static void UpdateColors()
        {
            if (_currentTheme == "dark")
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }
        }
        
        private static void ApplyDarkTheme()
        {
            // Material Design 3 Dark
            Background = Color.FromArgb(18, 18, 18);           // #121212
            Surface = Color.FromArgb(30, 30, 30);              // #1E1E1E
            SurfaceVariant = Color.FromArgb(45, 45, 45);       // #2D2D2D
            Primary = Color.FromArgb(187, 134, 252);           // #BB86FC (Purple)
            PrimaryVariant = Color.FromArgb(147, 94, 212);     // #935ED4
            Secondary = Color.FromArgb(3, 218, 198);           // #03DAC6 (Teal)
            TextPrimary = Color.FromArgb(255, 255, 255);       // #FFFFFF
            TextSecondary = Color.FromArgb(180, 180, 180);     // #B4B4B4
            TextDisabled = Color.FromArgb(100, 100, 100);      // #646464
            Border = Color.FromArgb(60, 60, 60);               // #3C3C3C
            Divider = Color.FromArgb(40, 40, 40);              // #282828
            Error = Color.FromArgb(207, 102, 121);             // #CF6679
            Success = Color.FromArgb(80, 200, 120);            // #50C878
            Warning = Color.FromArgb(255, 180, 50);            // #FFB432
            Shadow = Color.FromArgb(80, 0, 0, 0);              // Black 80% alpha
            Ripple = Color.FromArgb(40, 255, 255, 255);        // White 40% alpha
            Hover = Color.FromArgb(20, 255, 255, 255);         // White 20% alpha
            Pressed = Color.FromArgb(40, 255, 255, 255);       // White 40% alpha
        }
        
        private static void ApplyLightTheme()
        {
            // Material Design 3 Light
            Background = Color.FromArgb(255, 251, 254);        // #FFFBFE
            Surface = Color.FromArgb(255, 255, 255);           // #FFFFFF
            SurfaceVariant = Color.FromArgb(240, 240, 240);    // #F0F0F0
            Primary = Color.FromArgb(103, 80, 164);            // #6750A4 (Purple)
            PrimaryVariant = Color.FromArgb(79, 55, 139);      // #4F378B
            Secondary = Color.FromArgb(98, 91, 113);           // #625B71
            TextPrimary = Color.FromArgb(28, 27, 31);          // #1C1B1F
            TextSecondary = Color.FromArgb(100, 100, 100);     // #646464
            TextDisabled = Color.FromArgb(180, 180, 180);      // #B4B4B4
            Border = Color.FromArgb(200, 200, 200);            // #C8C8C8
            Divider = Color.FromArgb(220, 220, 220);           // #DCDCDC
            Error = Color.FromArgb(179, 38, 30);               // #B3261E
            Success = Color.FromArgb(56, 142, 60);             // #388E3C
            Warning = Color.FromArgb(237, 108, 2);             // #ED6C02
            Shadow = Color.FromArgb(40, 0, 0, 0);              // Black 40% alpha
            Ripple = Color.FromArgb(30, 0, 0, 0);              // Black 30% alpha
            Hover = Color.FromArgb(10, 0, 0, 0);               // Black 10% alpha
            Pressed = Color.FromArgb(20, 0, 0, 0);             // Black 20% alpha
        }
        
        // ═══════════════════════════════════════════
        // FONTS
        // ═══════════════════════════════════════════
        
        public static string FontFamily = "Segoe UI";
        
        public static Font DisplayLarge { get { return new Font(FontFamily, 57, FontStyle.Regular); } }
        public static Font DisplayMedium { get { return new Font(FontFamily, 45, FontStyle.Regular); } }
        public static Font DisplaySmall { get { return new Font(FontFamily, 36, FontStyle.Regular); } }
        
        public static Font HeadlineLarge { get { return new Font(FontFamily, 32, FontStyle.Regular); } }
        public static Font HeadlineMedium { get { return new Font(FontFamily, 28, FontStyle.Regular); } }
        public static Font HeadlineSmall { get { return new Font(FontFamily, 24, FontStyle.Regular); } }
        
        public static Font TitleLarge { get { return new Font(FontFamily, 22, FontStyle.Bold); } }
        public static Font TitleMedium { get { return new Font(FontFamily, 16, FontStyle.Bold); } }
        public static Font TitleSmall { get { return new Font(FontFamily, 14, FontStyle.Bold); } }
        
        public static Font BodyLarge { get { return new Font(FontFamily, 16, FontStyle.Regular); } }
        public static Font BodyMedium { get { return new Font(FontFamily, 14, FontStyle.Regular); } }
        public static Font BodySmall { get { return new Font(FontFamily, 12, FontStyle.Regular); } }
        
        public static Font LabelLarge { get { return new Font(FontFamily, 14, FontStyle.Bold); } }
        public static Font LabelMedium { get { return new Font(FontFamily, 12, FontStyle.Bold); } }
        public static Font LabelSmall { get { return new Font(FontFamily, 11, FontStyle.Bold); } }
        
        // ═══════════════════════════════════════════
        // SIZING
        // ═══════════════════════════════════════════
        
        public static int CornerRadiusSmall = 4;
        public static int CornerRadiusMedium = 8;
        public static int CornerRadiusLarge = 12;
        public static int CornerRadiusExtraLarge = 16;
        public static int CornerRadiusFull = 9999;
        
        public static int ElevationNone = 0;
        public static int ElevationLow = 1;
        public static int ElevationMedium = 2;
        public static int ElevationHigh = 3;
        
        public static int SpacingXSmall = 4;
        public static int SpacingSmall = 8;
        public static int SpacingMedium = 16;
        public static int SpacingLarge = 24;
        public static int SpacingXLarge = 32;
        
        // ═══════════════════════════════════════════
        // COLOR HELPERS
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Get elevation shadow color
        /// </summary>
        public static Color GetElevationShadow(int level)
        {
            int alpha = 20 + (level * 15);
            if (alpha > 80) alpha = 80;
            return Color.FromArgb(alpha, 0, 0, 0);
        }
        
        /// <summary>
        /// Blend two colors
        /// </summary>
        public static Color BlendColors(Color from, Color to, float amount)
        {
            int r = (int)(from.R + (to.R - from.R) * amount);
            int g = (int)(from.G + (to.G - from.G) * amount);
            int b = (int)(from.B + (to.B - from.B) * amount);
            int a = (int)(from.A + (to.A - from.A) * amount);
            
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));
            a = Math.Max(0, Math.Min(255, a));
            
            return Color.FromArgb(a, r, g, b);
        }
        
        /// <summary>
        /// Lighten a color
        /// </summary>
        public static Color Lighten(Color color, float amount)
        {
            return BlendColors(color, Color.White, amount);
        }
        
        /// <summary>
        /// Darken a color
        /// </summary>
        public static Color Darken(Color color, float amount)
        {
            return BlendColors(color, Color.Black, amount);
        }
        
        /// <summary>
        /// Add alpha to color
        /// </summary>
        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }
    }
}
