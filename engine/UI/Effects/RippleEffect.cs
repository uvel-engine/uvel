using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Uvel.UI.Styles;
using Uvel.UI.Animation;

namespace Uvel.UI.Effects
{
    /// <summary>
    /// Ripple effect for Material Design touch feedback
    /// </summary>
    public class RippleEffect : IDisposable
    {
        // ═══════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════
        
        private Control _target;
        private Timer _timer;
        private Point _center;
        private float _radius;
        private float _maxRadius;
        private float _alpha;
        private bool _isExpanding;
        private bool _isFading;
        private Color _rippleColor;
        
        private const int FRAME_RATE = 16;
        private const float EXPAND_SPEED = 15f;
        private const float FADE_SPEED = 0.05f;
        
        // ═══════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        public RippleEffect(Control target)
        {
            _target = target;
            _rippleColor = Theme.Ripple;
            _alpha = 0;
            _radius = 0;
            _isExpanding = false;
            _isFading = false;
            
            _timer = new Timer();
            _timer.Interval = FRAME_RATE;
            _timer.Tick += OnTimerTick;
            
            // Hook events
            _target.MouseDown += OnMouseDown;
            _target.MouseUp += OnMouseUp;
            _target.Paint += OnPaint;
        }
        
        // ═══════════════════════════════════════════
        // RIPPLE COLOR
        // ═══════════════════════════════════════════
        
        public Color RippleColor
        {
            get { return _rippleColor; }
            set { _rippleColor = value; }
        }
        
        // ═══════════════════════════════════════════
        // EVENT HANDLERS
        // ═══════════════════════════════════════════
        
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                StartRipple(e.Location);
            }
        }
        
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                StartFade();
            }
        }
        
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (_alpha > 0 && _radius > 0)
            {
                DrawRipple(e.Graphics);
            }
        }
        
        // ═══════════════════════════════════════════
        // RIPPLE LOGIC
        // ═══════════════════════════════════════════
        
        public void StartRipple(Point center)
        {
            _center = center;
            _radius = 0;
            _alpha = 0.3f;
            _isExpanding = true;
            _isFading = false;
            
            // Calculate max radius (diagonal of control)
            float dx = Math.Max(center.X, _target.Width - center.X);
            float dy = Math.Max(center.Y, _target.Height - center.Y);
            _maxRadius = (float)Math.Sqrt(dx * dx + dy * dy);
            
            _timer.Start();
        }
        
        public void StartFade()
        {
            _isFading = true;
        }
        
        private void OnTimerTick(object sender, EventArgs e)
        {
            bool needsRepaint = false;
            
            // Expand ripple
            if (_isExpanding && _radius < _maxRadius)
            {
                _radius += EXPAND_SPEED;
                if (_radius >= _maxRadius)
                {
                    _radius = _maxRadius;
                }
                needsRepaint = true;
            }
            
            // Fade out
            if (_isFading)
            {
                _alpha -= FADE_SPEED;
                if (_alpha <= 0)
                {
                    _alpha = 0;
                    _radius = 0;
                    _isExpanding = false;
                    _isFading = false;
                    _timer.Stop();
                }
                needsRepaint = true;
            }
            
            if (needsRepaint)
            {
                _target.Invalidate();
            }
        }
        
        // ═══════════════════════════════════════════
        // DRAWING
        // ═══════════════════════════════════════════
        
        private void DrawRipple(Graphics g)
        {
            if (_radius <= 0 || _alpha <= 0) return;
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int alpha = (int)(_alpha * 255);
            Color color = Color.FromArgb(alpha, _rippleColor.R, _rippleColor.G, _rippleColor.B);
            
            float diameter = _radius * 2;
            RectangleF rect = new RectangleF(
                _center.X - _radius,
                _center.Y - _radius,
                diameter,
                diameter
            );
            
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, rect);
            }
        }
        
        // ═══════════════════════════════════════════
        // MANUAL TRIGGER
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Trigger ripple at center of control
        /// </summary>
        public void TriggerRipple()
        {
            Point center = new Point(_target.Width / 2, _target.Height / 2);
            StartRipple(center);
            
            // Auto fade after delay
            Timer fadeTimer = new Timer();
            fadeTimer.Interval = 150;
            fadeTimer.Tick += delegate
            {
                fadeTimer.Stop();
                fadeTimer.Dispose();
                StartFade();
            };
            fadeTimer.Start();
        }
        
        // ═══════════════════════════════════════════
        // DISPOSE
        // ═══════════════════════════════════════════
        
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            
            if (_target != null)
            {
                _target.MouseDown -= OnMouseDown;
                _target.MouseUp -= OnMouseUp;
                _target.Paint -= OnPaint;
            }
        }
    }
    
    /// <summary>
    /// Static helper for one-shot ripple effects
    /// </summary>
    public static class Ripple
    {
        /// <summary>
        /// Create ripple on control at specified point
        /// </summary>
        public static void At(Control control, Point location)
        {
            RippleOverlay overlay = new RippleOverlay(control, location);
            overlay.Start();
        }
        
        /// <summary>
        /// Create ripple at center of control
        /// </summary>
        public static void AtCenter(Control control)
        {
            Point center = new Point(control.Width / 2, control.Height / 2);
            At(control, center);
        }
    }
    
    /// <summary>
    /// Temporary overlay for ripple effect
    /// </summary>
    internal class RippleOverlay : IDisposable
    {
        private Control _target;
        private Point _center;
        private Timer _timer;
        private float _radius;
        private float _maxRadius;
        private float _alpha;
        private bool _isFading;
        
        public RippleOverlay(Control target, Point center)
        {
            _target = target;
            _center = center;
            _radius = 0;
            _alpha = 0.25f;
            _isFading = false;
            
            float dx = Math.Max(center.X, target.Width - center.X);
            float dy = Math.Max(center.Y, target.Height - center.Y);
            _maxRadius = (float)Math.Sqrt(dx * dx + dy * dy);
            
            _timer = new Timer();
            _timer.Interval = 16;
            _timer.Tick += OnTick;
            
            _target.Paint += OnPaint;
        }
        
        public void Start()
        {
            _timer.Start();
        }
        
        private void OnTick(object sender, EventArgs e)
        {
            if (!_isFading)
            {
                _radius += 12f;
                if (_radius >= _maxRadius)
                {
                    _isFading = true;
                }
            }
            else
            {
                _alpha -= 0.03f;
                if (_alpha <= 0)
                {
                    Dispose();
                    return;
                }
            }
            
            _target.Invalidate();
        }
        
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (_radius <= 0 || _alpha <= 0) return;
            
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            int alpha = (int)(_alpha * 255);
            Color color = Color.FromArgb(alpha, Theme.Ripple.R, Theme.Ripple.G, Theme.Ripple.B);
            
            float diameter = _radius * 2;
            RectangleF rect = new RectangleF(
                _center.X - _radius,
                _center.Y - _radius,
                diameter,
                diameter
            );
            
            using (SolidBrush brush = new SolidBrush(color))
            {
                e.Graphics.FillEllipse(brush, rect);
            }
        }
        
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            
            if (_target != null)
            {
                _target.Paint -= OnPaint;
                _target.Invalidate();
            }
        }
    }
}
