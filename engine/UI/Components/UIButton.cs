using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Uvel.UI.Core;
using Uvel.UI.Styles;
using Uvel.UI.Effects;
using Uvel.UI.Animation;

namespace Uvel.UI.Components
{
    /// <summary>
    /// Material Design Button with ripple, shadow, hover effects
    /// </summary>
    public class UIButton : UIElement
    {
        // ═══════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════
        
        private string _text = "";
        private Font _font;
        private bool _isPrimary;
        private bool _isOutlined;
        private bool _isText;
        private RippleEffect _ripple;
        
        private Color _normalColor;
        private Color _hoverColor;
        private Color _pressedColor;
        private Color _textColor;
        
        // ═══════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        public UIButton()
        {
            // Defaults
            _cornerRadius = Theme.CornerRadiusMedium;
            _elevation = Theme.ElevationLow;
            _font = Theme.LabelLarge;
            
            SetNormalStyle();
            
            Size = new Size(120, 40);
            Cursor = Cursors.Hand;
            
            // Enable ripple
            _ripple = new RippleEffect(this);
        }
        
        // ═══════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════
        
        public new string Text
        {
            get { return _text; }
            set { _text = value; Invalidate(); }
        }
        
        public bool IsPrimary
        {
            get { return _isPrimary; }
            set
            {
                _isPrimary = value;
                if (value)
                {
                    SetPrimaryStyle();
                }
                else
                {
                    SetNormalStyle();
                }
                Invalidate();
            }
        }
        
        public bool IsOutlined
        {
            get { return _isOutlined; }
            set
            {
                _isOutlined = value;
                if (value)
                {
                    SetOutlinedStyle();
                }
                Invalidate();
            }
        }
        
        public bool IsTextButton
        {
            get { return _isText; }
            set
            {
                _isText = value;
                if (value)
                {
                    SetTextStyle();
                }
                Invalidate();
            }
        }
        
        public new Color ForeColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }
        
        // ═══════════════════════════════════════════
        // STYLES
        // ═══════════════════════════════════════════
        
        private void SetNormalStyle()
        {
            _normalColor = Theme.SurfaceVariant;
            _hoverColor = Theme.Lighten(Theme.SurfaceVariant, 0.1f);
            _pressedColor = Theme.Lighten(Theme.SurfaceVariant, 0.2f);
            _textColor = Theme.TextPrimary;
            _backgroundColor = _normalColor;
            _borderWidth = 0;
        }
        
        private void SetPrimaryStyle()
        {
            _normalColor = Theme.Primary;
            _hoverColor = Theme.Lighten(Theme.Primary, 0.1f);
            _pressedColor = Theme.Darken(Theme.Primary, 0.1f);
            _textColor = Theme.CurrentTheme == "dark" ? Color.FromArgb(30, 0, 50) : Color.White;
            _backgroundColor = _normalColor;
            _elevation = Theme.ElevationMedium;
            _borderWidth = 0;
            
            if (_ripple != null)
            {
                _ripple.RippleColor = Theme.WithAlpha(Color.White, 60);
            }
        }
        
        private void SetOutlinedStyle()
        {
            _normalColor = Color.Transparent;
            _hoverColor = Theme.WithAlpha(Theme.Primary, 20);
            _pressedColor = Theme.WithAlpha(Theme.Primary, 40);
            _textColor = Theme.Primary;
            _backgroundColor = _normalColor;
            _borderWidth = 2;
            _borderColor = Theme.Primary;
            _elevation = 0;
        }
        
        private void SetTextStyle()
        {
            _normalColor = Color.Transparent;
            _hoverColor = Theme.WithAlpha(Theme.Primary, 15);
            _pressedColor = Theme.WithAlpha(Theme.Primary, 30);
            _textColor = Theme.Primary;
            _backgroundColor = _normalColor;
            _elevation = 0;
            _borderWidth = 0;
        }
        
        // ═══════════════════════════════════════════
        // STATE HANDLERS
        // ═══════════════════════════════════════════
        
        protected override void OnHoverStateChanged()
        {
            Color target = _isHovered ? _hoverColor : _normalColor;
            AnimationManager.Instance.AnimateColor(this, _backgroundColor, target, 150);
            _backgroundColor = target;
        }
        
        protected override void OnPressStateChanged()
        {
            if (_isPressed)
            {
                _backgroundColor = _pressedColor;
            }
            else
            {
                _backgroundColor = _isHovered ? _hoverColor : _normalColor;
            }
        }
        
        protected override Color GetCurrentBackColor()
        {
            if (_isPressed)
                return _pressedColor;
            if (_isHovered)
                return _hoverColor;
            return _normalColor;
        }
        
        // ═══════════════════════════════════════════
        // RENDERING
        // ═══════════════════════════════════════════
        
        protected override void RenderElement(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            
            // Draw shadow
            if (_elevation > 0 && !_isText && !_isOutlined)
            {
                Rectangle shadowBounds = new Rectangle(
                    _elevation, 
                    _elevation, 
                    Width - _elevation * 2, 
                    Height - _elevation
                );
                ShadowRenderer.DrawMaterialShadow(g, shadowBounds, _elevation, _cornerRadius);
            }
            
            // Draw background
            using (GraphicsPath path = CreateRoundedPath(bounds, _cornerRadius))
            {
                using (SolidBrush brush = new SolidBrush(GetCurrentBackColor()))
                {
                    g.FillPath(brush, path);
                }
                
                // Draw border for outlined style
                if (_borderWidth > 0)
                {
                    using (Pen pen = new Pen(_borderColor, _borderWidth))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
            
            // Draw text
            if (!string.IsNullOrEmpty(_text))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                
                using (SolidBrush textBrush = new SolidBrush(_textColor))
                {
                    g.DrawString(_text, _font, textBrush, bounds, sf);
                }
            }
        }
        
        // ═══════════════════════════════════════════
        // CLEANUP
        // ═══════════════════════════════════════════
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_ripple != null)
                {
                    _ripple.Dispose();
                    _ripple = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
