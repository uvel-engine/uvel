using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Uvel.UI.Core
{
    /// <summary>
    /// Base class for all UI elements
    /// Provides common properties and rendering foundation
    /// </summary>
    public abstract class UIElement : Panel
    {
        // ═══════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════
        
        protected string _id;
        protected bool _isHovered;
        protected bool _isPressed;
        protected bool _isFocused;
        protected bool _isEnabled = true;
        protected bool _isVisible = true;
        
        // Layout
        protected int _marginLeft;
        protected int _marginTop;
        protected int _marginRight;
        protected int _marginBottom;
        protected int _paddingLeft;
        protected int _paddingTop;
        protected int _paddingRight;
        protected int _paddingBottom;
        
        // Styling
        protected Color _backgroundColor;
        protected Color _foregroundColor;
        protected Color _borderColor;
        protected int _borderWidth;
        protected int _cornerRadius;
        protected int _elevation;
        
        // Animation state
        protected float _animationProgress;
        protected Color _currentBackColor;
        protected Color _targetBackColor;
        
        // ═══════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        public UIElement()
        {
            // Enable double buffering for smooth rendering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            
            // Default values
            _backgroundColor = Color.Transparent;
            _foregroundColor = Color.White;
            _borderColor = Color.Transparent;
            _borderWidth = 0;
            _cornerRadius = 0;
            _elevation = 0;
            
            _currentBackColor = _backgroundColor;
            _targetBackColor = _backgroundColor;
            
            // Event handlers
            MouseEnter += OnMouseEnterHandler;
            MouseLeave += OnMouseLeaveHandler;
            MouseDown += OnMouseDownHandler;
            MouseUp += OnMouseUpHandler;
            GotFocus += OnGotFocusHandler;
            LostFocus += OnLostFocusHandler;
        }
        
        // ═══════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════
        
        public string ElementId
        {
            get { return _id; }
            set { _id = value; Name = value; }
        }
        
        public bool IsHovered
        {
            get { return _isHovered; }
        }
        
        public bool IsPressed
        {
            get { return _isPressed; }
        }
        
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; Invalidate(); }
        }
        
        public int Elevation
        {
            get { return _elevation; }
            set { _elevation = value; Invalidate(); }
        }
        
        public new Color BackColor
        {
            get { return _backgroundColor; }
            set 
            { 
                _backgroundColor = value;
                _currentBackColor = value;
                _targetBackColor = value;
                Invalidate(); 
            }
        }
        
        public new Color ForeColor
        {
            get { return _foregroundColor; }
            set { _foregroundColor = value; Invalidate(); }
        }
        
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }
        
        public int BorderWidth
        {
            get { return _borderWidth; }
            set { _borderWidth = value; Invalidate(); }
        }
        
        // ═══════════════════════════════════════════
        // MARGIN & PADDING
        // ═══════════════════════════════════════════
        
        public void SetMargin(int all)
        {
            _marginLeft = _marginTop = _marginRight = _marginBottom = all;
            Margin = new Padding(all);
        }
        
        public void SetMargin(int horizontal, int vertical)
        {
            _marginLeft = _marginRight = horizontal;
            _marginTop = _marginBottom = vertical;
            Margin = new Padding(horizontal, vertical, horizontal, vertical);
        }
        
        public void SetMargin(int left, int top, int right, int bottom)
        {
            _marginLeft = left;
            _marginTop = top;
            _marginRight = right;
            _marginBottom = bottom;
            Margin = new Padding(left, top, right, bottom);
        }
        
        public void SetPadding(int all)
        {
            _paddingLeft = _paddingTop = _paddingRight = _paddingBottom = all;
            Padding = new Padding(all);
        }
        
        public void SetPadding(int horizontal, int vertical)
        {
            _paddingLeft = _paddingRight = horizontal;
            _paddingTop = _paddingBottom = vertical;
            Padding = new Padding(horizontal, vertical, horizontal, vertical);
        }
        
        // ═══════════════════════════════════════════
        // GRAPHICS HELPERS
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Create rounded rectangle path
        /// </summary>
        protected GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
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
        
        /// <summary>
        /// Blend two colors
        /// </summary>
        protected Color BlendColors(Color from, Color to, float amount)
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
        protected Color LightenColor(Color color, float amount)
        {
            return BlendColors(color, Color.White, amount);
        }
        
        /// <summary>
        /// Darken a color
        /// </summary>
        protected Color DarkenColor(Color color, float amount)
        {
            return BlendColors(color, Color.Black, amount);
        }
        
        // ═══════════════════════════════════════════
        // EVENT HANDLERS
        // ═══════════════════════════════════════════
        
        protected virtual void OnMouseEnterHandler(object sender, EventArgs e)
        {
            _isHovered = true;
            OnHoverStateChanged();
            Invalidate();
        }
        
        protected virtual void OnMouseLeaveHandler(object sender, EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            OnHoverStateChanged();
            Invalidate();
        }
        
        protected virtual void OnMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                OnPressStateChanged();
                Invalidate();
            }
        }
        
        protected virtual void OnMouseUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = false;
                OnPressStateChanged();
                Invalidate();
            }
        }
        
        protected virtual void OnGotFocusHandler(object sender, EventArgs e)
        {
            _isFocused = true;
            Invalidate();
        }
        
        protected virtual void OnLostFocusHandler(object sender, EventArgs e)
        {
            _isFocused = false;
            Invalidate();
        }
        
        // ═══════════════════════════════════════════
        // VIRTUAL METHODS
        // ═══════════════════════════════════════════
        
        protected virtual void OnHoverStateChanged() { }
        protected virtual void OnPressStateChanged() { }
        
        /// <summary>
        /// Get current background color based on state
        /// </summary>
        protected virtual Color GetCurrentBackColor()
        {
            if (_isPressed)
                return LightenColor(_backgroundColor, 0.2f);
            if (_isHovered)
                return LightenColor(_backgroundColor, 0.1f);
            return _backgroundColor;
        }
        
        // ═══════════════════════════════════════════
        // RENDERING
        // ═══════════════════════════════════════════
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Let derived classes render
            RenderElement(g);
        }
        
        /// <summary>
        /// Override in derived classes to render the element
        /// </summary>
        protected virtual void RenderElement(Graphics g)
        {
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            
            using (GraphicsPath path = CreateRoundedPath(rect, _cornerRadius))
            {
                // Background
                using (SolidBrush brush = new SolidBrush(GetCurrentBackColor()))
                {
                    g.FillPath(brush, path);
                }
                
                // Border
                if (_borderWidth > 0)
                {
                    using (Pen pen = new Pen(_borderColor, _borderWidth))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}
