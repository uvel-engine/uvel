using System;
using System.Drawing;

namespace Uvel.UI.Styles
{
    /// <summary>
    /// Color utilities and palettes
    /// Material Design 3 color system
    /// </summary>
    public static class Colors
    {
        // ═══════════════════════════════════════════
        // MATERIAL PALETTE - PRIMARY
        // ═══════════════════════════════════════════
        
        public static class Purple
        {
            public static Color P10 = Color.FromArgb(33, 0, 93);
            public static Color P20 = Color.FromArgb(56, 30, 114);
            public static Color P30 = Color.FromArgb(79, 55, 139);
            public static Color P40 = Color.FromArgb(103, 80, 164);
            public static Color P50 = Color.FromArgb(127, 105, 189);
            public static Color P60 = Color.FromArgb(154, 130, 219);
            public static Color P70 = Color.FromArgb(177, 156, 217);
            public static Color P80 = Color.FromArgb(208, 188, 255);
            public static Color P90 = Color.FromArgb(234, 221, 255);
            public static Color P95 = Color.FromArgb(246, 237, 255);
            public static Color P99 = Color.FromArgb(255, 251, 254);
        }
        
        public static class Teal
        {
            public static Color T10 = Color.FromArgb(0, 55, 52);
            public static Color T20 = Color.FromArgb(0, 82, 79);
            public static Color T30 = Color.FromArgb(0, 109, 105);
            public static Color T40 = Color.FromArgb(0, 137, 131);
            public static Color T50 = Color.FromArgb(3, 166, 158);
            public static Color T60 = Color.FromArgb(3, 195, 186);
            public static Color T70 = Color.FromArgb(51, 218, 209);
            public static Color T80 = Color.FromArgb(102, 240, 231);
            public static Color T90 = Color.FromArgb(153, 255, 248);
            public static Color T95 = Color.FromArgb(204, 255, 252);
        }
        
        // ═══════════════════════════════════════════
        // NEUTRAL PALETTE
        // ═══════════════════════════════════════════
        
        public static class Neutral
        {
            public static Color N0 = Color.FromArgb(0, 0, 0);
            public static Color N10 = Color.FromArgb(18, 18, 18);
            public static Color N20 = Color.FromArgb(30, 30, 30);
            public static Color N30 = Color.FromArgb(45, 45, 45);
            public static Color N40 = Color.FromArgb(60, 60, 60);
            public static Color N50 = Color.FromArgb(80, 80, 80);
            public static Color N60 = Color.FromArgb(100, 100, 100);
            public static Color N70 = Color.FromArgb(130, 130, 130);
            public static Color N80 = Color.FromArgb(160, 160, 160);
            public static Color N90 = Color.FromArgb(200, 200, 200);
            public static Color N95 = Color.FromArgb(230, 230, 230);
            public static Color N99 = Color.FromArgb(250, 250, 250);
            public static Color N100 = Color.FromArgb(255, 255, 255);
        }
        
        // ═══════════════════════════════════════════
        // ERROR PALETTE
        // ═══════════════════════════════════════════
        
        public static class Error
        {
            public static Color E10 = Color.FromArgb(65, 14, 11);
            public static Color E20 = Color.FromArgb(96, 20, 16);
            public static Color E30 = Color.FromArgb(140, 29, 24);
            public static Color E40 = Color.FromArgb(179, 38, 30);
            public static Color E50 = Color.FromArgb(207, 55, 48);
            public static Color E60 = Color.FromArgb(228, 79, 75);
            public static Color E70 = Color.FromArgb(236, 117, 114);
            public static Color E80 = Color.FromArgb(242, 184, 181);
            public static Color E90 = Color.FromArgb(249, 222, 220);
            public static Color E95 = Color.FromArgb(252, 238, 238);
        }
        
        // ═══════════════════════════════════════════
        // SUCCESS PALETTE
        // ═══════════════════════════════════════════
        
        public static class Success
        {
            public static Color S10 = Color.FromArgb(16, 56, 16);
            public static Color S20 = Color.FromArgb(27, 94, 32);
            public static Color S30 = Color.FromArgb(46, 125, 50);
            public static Color S40 = Color.FromArgb(56, 142, 60);
            public static Color S50 = Color.FromArgb(76, 175, 80);
            public static Color S60 = Color.FromArgb(102, 187, 106);
            public static Color S70 = Color.FromArgb(129, 199, 132);
            public static Color S80 = Color.FromArgb(165, 214, 167);
            public static Color S90 = Color.FromArgb(200, 230, 201);
            public static Color S95 = Color.FromArgb(232, 245, 233);
        }
        
        // ═══════════════════════════════════════════
        // COLOR UTILITIES
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Parse color from string (hex, rgb, name)
        /// </summary>
        public static Color Parse(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr))
            {
                return Color.Transparent;
            }
            
            colorStr = colorStr.Trim();
            
            try
            {
                // Hex format
                if (colorStr.StartsWith("#"))
                {
                    return ColorTranslator.FromHtml(colorStr);
                }
                
                // RGB format: rgb(r,g,b)
                if (colorStr.StartsWith("rgb(") && colorStr.EndsWith(")"))
                {
                    string inner = colorStr.Substring(4, colorStr.Length - 5);
                    string[] parts = inner.Split(',');
                    if (parts.Length >= 3)
                    {
                        int r = int.Parse(parts[0].Trim());
                        int g = int.Parse(parts[1].Trim());
                        int b = int.Parse(parts[2].Trim());
                        return Color.FromArgb(r, g, b);
                    }
                }
                
                // RGBA format: rgba(r,g,b,a)
                if (colorStr.StartsWith("rgba(") && colorStr.EndsWith(")"))
                {
                    string inner = colorStr.Substring(5, colorStr.Length - 6);
                    string[] parts = inner.Split(',');
                    if (parts.Length >= 4)
                    {
                        int r = int.Parse(parts[0].Trim());
                        int g = int.Parse(parts[1].Trim());
                        int b = int.Parse(parts[2].Trim());
                        float a = float.Parse(parts[3].Trim());
                        int alpha = (int)(a * 255);
                        return Color.FromArgb(alpha, r, g, b);
                    }
                }
                
                // Named color
                return Color.FromName(colorStr);
            }
            catch
            {
                return Color.Transparent;
            }
        }
        
        /// <summary>
        /// Convert color to hex string
        /// </summary>
        public static string ToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
        
        /// <summary>
        /// Convert color to rgba string
        /// </summary>
        public static string ToRgba(Color color)
        {
            float a = color.A / 255f;
            return string.Format("rgba({0},{1},{2},{3:F2})", color.R, color.G, color.B, a);
        }
        
        /// <summary>
        /// Check if color is dark
        /// </summary>
        public static bool IsDark(Color color)
        {
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return luminance < 0.5;
        }
        
        /// <summary>
        /// Get contrasting color (black or white)
        /// </summary>
        public static Color GetContrastColor(Color color)
        {
            return IsDark(color) ? Color.White : Color.Black;
        }
        
        /// <summary>
        /// Mix two colors
        /// </summary>
        public static Color Mix(Color color1, Color color2, float amount)
        {
            return Theme.BlendColors(color1, color2, amount);
        }
        
        /// <summary>
        /// Add opacity to color
        /// </summary>
        public static Color WithOpacity(Color color, float opacity)
        {
            int alpha = (int)(255 * opacity);
            alpha = Math.Max(0, Math.Min(255, alpha));
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }
        
        /// <summary>
        /// Desaturate color
        /// </summary>
        public static Color Desaturate(Color color, float amount)
        {
            int gray = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
            Color grayColor = Color.FromArgb(gray, gray, gray);
            return Mix(color, grayColor, amount);
        }
    }
}
