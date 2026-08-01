using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Uvel.UI.Styles;

namespace Uvel.UI.Effects
{
    /// <summary>
    /// Shadow renderer for UI elements
    /// Supports multiple shadow levels (elevation)
    /// </summary>
    public static class ShadowRenderer
    {
        // ═══════════════════════════════════════════
        // SHADOW RENDERING
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Draw shadow behind element
        /// </summary>
        public static void DrawShadow(Graphics g, Rectangle bounds, int elevation, int cornerRadius)
        {
            if (elevation <= 0) return;
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Number of shadow layers based on elevation
            int layers = elevation * 2;
            int maxOffset = elevation * 3;
            int baseAlpha = 15;
            
            for (int i = layers; i > 0; i--)
            {
                float ratio = (float)i / layers;
                int offset = (int)(maxOffset * ratio);
                int blur = offset;
                int alpha = (int)(baseAlpha * (1 - ratio * 0.5f));
                
                Rectangle shadowRect = new Rectangle(
                    bounds.X + offset / 2,
                    bounds.Y + offset,
                    bounds.Width,
                    bounds.Height
                );
                
                // Inflate for blur effect
                shadowRect.Inflate(blur / 2, blur / 2);
                
                using (GraphicsPath path = CreateRoundedPath(shadowRect, cornerRadius + blur / 2))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draw inner shadow
        /// </summary>
        public static void DrawInnerShadow(Graphics g, Rectangle bounds, int depth, int cornerRadius)
        {
            if (depth <= 0) return;
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            using (GraphicsPath path = CreateRoundedPath(bounds, cornerRadius))
            {
                // Clip to the shape
                Region oldClip = g.Clip;
                g.SetClip(path);
                
                // Draw gradient shadows on each edge
                int alpha = 30;
                
                // Top shadow
                Rectangle topRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, depth);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    topRect,
                    Color.FromArgb(alpha, 0, 0, 0),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, topRect);
                }
                
                // Left shadow
                Rectangle leftRect = new Rectangle(bounds.X, bounds.Y, depth, bounds.Height);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    leftRect,
                    Color.FromArgb(alpha, 0, 0, 0),
                    Color.Transparent,
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, leftRect);
                }
                
                g.Clip = oldClip;
            }
        }
        
        /// <summary>
        /// Draw ambient shadow (softer, all around)
        /// </summary>
        public static void DrawAmbientShadow(Graphics g, Rectangle bounds, int elevation, int cornerRadius)
        {
            if (elevation <= 0) return;
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int blur = elevation * 2;
            int alpha = 10 + elevation * 5;
            if (alpha > 40) alpha = 40;
            
            Rectangle shadowRect = bounds;
            shadowRect.Inflate(blur, blur);
            shadowRect.Offset(0, elevation);
            
            using (GraphicsPath path = CreateRoundedPath(shadowRect, cornerRadius + blur))
            {
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(alpha, 0, 0, 0);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    brush.CenterPoint = new PointF(
                        shadowRect.X + shadowRect.Width / 2,
                        shadowRect.Y + shadowRect.Height / 2
                    );
                    
                    g.FillPath(brush, path);
                }
            }
        }
        
        /// <summary>
        /// Draw key shadow (directional, from light source)
        /// </summary>
        public static void DrawKeyShadow(Graphics g, Rectangle bounds, int elevation, int cornerRadius)
        {
            if (elevation <= 0) return;
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int offsetY = elevation * 2;
            int blur = elevation * 3;
            int alpha = 20 + elevation * 8;
            if (alpha > 60) alpha = 60;
            
            Rectangle shadowRect = bounds;
            shadowRect.Inflate(blur / 2, blur / 2);
            shadowRect.Offset(0, offsetY);
            
            for (int i = blur; i > 0; i -= 2)
            {
                Rectangle layerRect = shadowRect;
                layerRect.Inflate(-i / 2, -i / 2);
                
                int layerAlpha = (int)(alpha * (1 - (float)i / blur));
                
                using (GraphicsPath path = CreateRoundedPath(layerRect, cornerRadius))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(layerAlpha, 0, 0, 0)))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draw Material Design shadow (combination of ambient and key)
        /// </summary>
        public static void DrawMaterialShadow(Graphics g, Rectangle bounds, int elevation, int cornerRadius)
        {
            if (elevation <= 0) return;
            
            // Ambient shadow (soft, all around)
            DrawAmbientShadow(g, bounds, elevation, cornerRadius);
            
            // Key shadow (directional, below)
            DrawKeyShadow(g, bounds, elevation, cornerRadius);
        }
        
        // ═══════════════════════════════════════════
        // HELPER
        // ═══════════════════════════════════════════
        
        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
            
            // Top left
            path.AddArc(arc, 180, 90);
            
            // Top right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            // Bottom right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            
            // Bottom left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            
            path.CloseFigure();
            return path;
        }
    }
}
