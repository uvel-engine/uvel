using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Uvel.UI.Core;
using Uvel.UI.Styles;

namespace Uvel.UI.Components
{
    /// <summary>
    /// Material Design Label with typography support
    /// </summary>
    public class UILabel : UIElement
    {
        // ═══════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════
        
        private string _text = "";
        private int _fontSize = 14;
        private bool _isBold;
        private bool _isSecondary;
        private bool _isDisabled;
        private StringAlignment _horizontalAlign = StringAlignment.Near;
        private StringAlignment _verticalAlign = StringAlignment.Near;
        private bool _autoSize = true;
        
        // ═══════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        public UILabel()
        {
            // Defaults
            _backgroundColor = Color.Transparent;
            _foregroundColor = Theme.TextPrimary;
            _cornerRadius = 0;
            _elevation = 0;
            
            Size = new Size(100, 20);
        }
        
        // ═══════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════
        
        public new string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_autoSize)
                {
                    AutoSizeControl();
                }
                Invalidate();
            }
        }
        
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                if (_autoSize)
                {
                    AutoSizeControl();
                }
                Invalidate();
            }
        }
        
        public bool IsBold
        {
            get { return _isBold; }
            set
            {
                _isBold = value;
                if (_autoSize)
                {
                    AutoSizeControl();
                }
                Invalidate();
            }
        }
        
        public bool IsSecondary
        {
            get { return _isSecondary; }
            set
            {
                _isSecondary = value;
                _foregroundColor = value ? Theme.TextSecondary : Theme.TextPrimary;
                Invalidate();
            }
        }
        
        public bool IsDisabled
        {
            get { return _isDisabled; }
            set
            {
                _isDisabled = value;
                _foregroundColor = value ? Theme.TextDisabled : Theme.TextPrimary;
                Invalidate();
            }
        }
        
        public new bool AutoSize
        {
            get { return _autoSize; }
            set
            {
                _autoSize = value;
                if (value)
                {
                    AutoSizeControl();
                }
            }
        }
        
        public StringAlignment HorizontalAlignment
        {
            get { return _horizontalAlign; }
            set { _horizontalAlign = value; Invalidate(); }
        }
        
        public StringAlignment VerticalAlignment
        {
            get { return _verticalAlign; }
            set { _verticalAlign = value; Invalidate(); }
        }
        
        // ═══════════════════════════════════════════
        // TYPOGRAPHY PRESETS
        // ═══════════════════════════════════════════
        
        public void SetDisplayLarge()
        {
            _fontSize = 57;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetDisplayMedium()
        {
            _fontSize = 45;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetDisplaySmall()
        {
            _fontSize = 36;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetHeadlineLarge()
        {
            _fontSize = 32;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetHeadlineMedium()
        {
            _fontSize = 28;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetHeadlineSmall()
        {
            _fontSize = 24;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetTitleLarge()
        {
            _fontSize = 22;
            _isBold = true;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetTitleMedium()
        {
            _fontSize = 16;
            _isBold = true;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetTitleSmall()
        {
            _fontSize = 14;
            _isBold = true;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetBodyLarge()
        {
            _fontSize = 16;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetBodyMedium()
        {
            _fontSize = 14;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetBodySmall()
        {
            _fontSize = 12;
            _isBold = false;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetLabelLarge()
        {
            _fontSize = 14;
            _isBold = true;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetLabelMedium()
        {
            _fontSize = 12;
            _isBold = true;
            AutoSizeControl();
            Invalidate();
        }
        
        public void SetLabelSmall()
        {
            _fontSize = 11;
            _isBold = true;
            AutoSizeControl();
            Invalidate();
        }
        
        // ═══════════════════════════════════════════
        // AUTO SIZE
        // ═══════════════════════════════════════════
        
        private void AutoSizeControl()
        {
            if (string.IsNullOrEmpty(_text)) return;
            
            using (Graphics g = CreateGraphics())
            {
                FontStyle style = _isBold ? FontStyle.Bold : FontStyle.Regular;
                using (Font font = new Font(Theme.FontFamily, _fontSize, style))
                {
                    SizeF textSize = g.MeasureString(_text, font);
                    Width = (int)Math.Ceiling(textSize.Width) + 4;
                    Height = (int)Math.Ceiling(textSize.Height) + 2;
                }
            }
        }
        
        // ═══════════════════════════════════════════
        // RENDERING
        // ═══════════════════════════════════════════
        
        protected override void RenderElement(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            Rectangle bounds = new Rectangle(0, 0, Width, Height);
            
            // Draw background if not transparent
            if (_backgroundColor != Color.Transparent)
            {
                if (_cornerRadius > 0)
                {
                    using (GraphicsPath path = CreateRoundedPath(bounds, _cornerRadius))
                    {
                        using (SolidBrush brush = new SolidBrush(_backgroundColor))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(_backgroundColor))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                }
            }
            
            // Draw text
            if (!string.IsNullOrEmpty(_text))
            {
                FontStyle style = _isBold ? FontStyle.Bold : FontStyle.Regular;
                using (Font font = new Font(Theme.FontFamily, _fontSize, style))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = _horizontalAlign;
                    sf.LineAlignment = _verticalAlign;
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    
                    using (SolidBrush textBrush = new SolidBrush(_foregroundColor))
                    {
                        g.DrawString(_text, font, textBrush, bounds, sf);
                    }
                }
            }
        }
    }
}
