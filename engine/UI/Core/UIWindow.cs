using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Uvel.UI.Core
{
    /// <summary>
    /// Custom window with modern styling
    /// Supports borderless, shadow, blur effects
    /// </summary>
    public class UIWindow : Form
    {
        // ═══════════════════════════════════════════
        // WIN32 API
        // ═══════════════════════════════════════════
        
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
        
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        
        // ═══════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════
        
        private bool _isBorderless;
        private bool _isResizable = true;
        private bool _hasShadow = true;
        private int _cornerRadius = 0;
        private Color _backgroundColor;
        private Color _titleBarColor;
        
        private Panel _titleBar;
        private Label _titleLabel;
        private Panel _closeButton;
        private Panel _minimizeButton;
        private Panel _maximizeButton;
        
        private bool _isDragging;
        private Point _dragStart;
        
        // ═══════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        public UIWindow()
        {
            // Enable double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            
            // Default styling
            _backgroundColor = Color.FromArgb(18, 18, 18);
            _titleBarColor = Color.FromArgb(30, 30, 30);
            
            BackColor = _backgroundColor;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10);
            StartPosition = FormStartPosition.CenterScreen;
        }
        
        // ═══════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════
        
        public bool IsBorderless
        {
            get { return _isBorderless; }
            set
            {
                _isBorderless = value;
                if (value)
                {
                    FormBorderStyle = FormBorderStyle.None;
                    CreateCustomTitleBar();
                }
                else
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                    RemoveCustomTitleBar();
                }
            }
        }
        
        public bool IsResizable
        {
            get { return _isResizable; }
            set
            {
                _isResizable = value;
                if (!value && !_isBorderless)
                {
                    FormBorderStyle = FormBorderStyle.FixedSingle;
                    MaximizeBox = false;
                }
            }
        }
        
        public bool HasShadow
        {
            get { return _hasShadow; }
            set { _hasShadow = value; }
        }
        
        public int WindowCornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                if (value > 0)
                {
                    Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, value, value));
                }
            }
        }
        
        public new Color BackColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                base.BackColor = value;
            }
        }
        
        public Color TitleBarColor
        {
            get { return _titleBarColor; }
            set
            {
                _titleBarColor = value;
                if (_titleBar != null)
                {
                    _titleBar.BackColor = value;
                }
            }
        }
        
        // ═══════════════════════════════════════════
        // CUSTOM TITLE BAR
        // ═══════════════════════════════════════════
        
        private void CreateCustomTitleBar()
        {
            _titleBar = new Panel();
            _titleBar.Dock = DockStyle.Top;
            _titleBar.Height = 32;
            _titleBar.BackColor = _titleBarColor;
            _titleBar.Cursor = Cursors.Default;
            
            // Title label
            _titleLabel = new Label();
            _titleLabel.Text = Text;
            _titleLabel.ForeColor = Color.White;
            _titleLabel.Font = new Font("Segoe UI", 10);
            _titleLabel.AutoSize = true;
            _titleLabel.Location = new Point(12, 7);
            _titleLabel.BackColor = Color.Transparent;
            
            // Close button
            _closeButton = CreateTitleBarButton("×", Color.FromArgb(232, 17, 35));
            _closeButton.Location = new Point(Width - 46, 0);
            _closeButton.Click += delegate { Close(); };
            
            // Maximize button
            _maximizeButton = CreateTitleBarButton("□", Color.FromArgb(60, 60, 60));
            _maximizeButton.Location = new Point(Width - 92, 0);
            _maximizeButton.Click += delegate
            {
                if (WindowState == FormWindowState.Maximized)
                    WindowState = FormWindowState.Normal;
                else
                    WindowState = FormWindowState.Maximized;
            };
            
            // Minimize button
            _minimizeButton = CreateTitleBarButton("−", Color.FromArgb(60, 60, 60));
            _minimizeButton.Location = new Point(Width - 138, 0);
            _minimizeButton.Click += delegate { WindowState = FormWindowState.Minimized; };
            
            // Dragging
            _titleBar.MouseDown += delegate(object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isDragging = true;
                    _dragStart = e.Location;
                }
            };
            _titleBar.MouseMove += delegate(object s, MouseEventArgs e)
            {
                if (_isDragging)
                {
                    Point diff = new Point(e.X - _dragStart.X, e.Y - _dragStart.Y);
                    Location = new Point(Location.X + diff.X, Location.Y + diff.Y);
                }
            };
            _titleBar.MouseUp += delegate { _isDragging = false; };
            
            // Same for title label
            _titleLabel.MouseDown += delegate(object s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isDragging = true;
                    _dragStart = e.Location;
                }
            };
            _titleLabel.MouseMove += delegate(object s, MouseEventArgs e)
            {
                if (_isDragging)
                {
                    Point diff = new Point(e.X - _dragStart.X, e.Y - _dragStart.Y);
                    Location = new Point(Location.X + diff.X, Location.Y + diff.Y);
                }
            };
            _titleLabel.MouseUp += delegate { _isDragging = false; };
            
            _titleBar.Controls.Add(_titleLabel);
            _titleBar.Controls.Add(_closeButton);
            _titleBar.Controls.Add(_maximizeButton);
            _titleBar.Controls.Add(_minimizeButton);
            
            Controls.Add(_titleBar);
            _titleBar.BringToFront();
            
            // Anchor buttons
            _closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _maximizeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _minimizeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
        
        private Panel CreateTitleBarButton(string text, Color hoverColor)
        {
            Panel btn = new Panel();
            btn.Size = new Size(46, 32);
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Hand;
            
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = Color.White;
            lbl.Font = new Font("Segoe UI", 12);
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Dock = DockStyle.Fill;
            lbl.BackColor = Color.Transparent;
            lbl.Cursor = Cursors.Hand;
            
            Color normalColor = Color.Transparent;
            
            btn.MouseEnter += delegate { btn.BackColor = hoverColor; };
            btn.MouseLeave += delegate { btn.BackColor = normalColor; };
            lbl.MouseEnter += delegate { btn.BackColor = hoverColor; };
            lbl.MouseLeave += delegate { btn.BackColor = normalColor; };
            lbl.Click += delegate { btn.InvokeOnClick(btn, EventArgs.Empty); };
            
            btn.Controls.Add(lbl);
            return btn;
        }
        
        private void RemoveCustomTitleBar()
        {
            if (_titleBar != null)
            {
                Controls.Remove(_titleBar);
                _titleBar.Dispose();
                _titleBar = null;
            }
        }
        
        // ═══════════════════════════════════════════
        // RESIZE HANDLING
        // ═══════════════════════════════════════════
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (_cornerRadius > 0)
            {
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _cornerRadius, _cornerRadius));
            }
        }
        
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            
            if (_titleLabel != null)
            {
                _titleLabel.Text = Text;
            }
        }
        
        // ═══════════════════════════════════════════
        // SHADOW (for non-borderless windows)
        // ═══════════════════════════════════════════
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                
                if (_hasShadow)
                {
                    // Drop shadow
                    cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
                }
                
                return cp;
            }
        }
        
        // ═══════════════════════════════════════════
        // BORDERLESS RESIZE
        // ═══════════════════════════════════════════
        
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int BORDER_WIDTH = 6;
        
        protected override void WndProc(ref Message m)
        {
            if (_isBorderless && _isResizable && m.Msg == 0x0084) // WM_NCHITTEST
            {
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                
                if (pos.X <= BORDER_WIDTH && pos.Y <= BORDER_WIDTH)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (pos.X >= Width - BORDER_WIDTH && pos.Y <= BORDER_WIDTH)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (pos.X <= BORDER_WIDTH && pos.Y >= Height - BORDER_WIDTH)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (pos.X >= Width - BORDER_WIDTH && pos.Y >= Height - BORDER_WIDTH)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (pos.X <= BORDER_WIDTH)
                    m.Result = (IntPtr)HTLEFT;
                else if (pos.X >= Width - BORDER_WIDTH)
                    m.Result = (IntPtr)HTRIGHT;
                else if (pos.Y <= BORDER_WIDTH)
                    m.Result = (IntPtr)HTTOP;
                else if (pos.Y >= Height - BORDER_WIDTH)
                    m.Result = (IntPtr)HTBOTTOM;
                else
                    base.WndProc(ref m);
                
                return;
            }
            
            base.WndProc(ref m);
        }
    }
}
