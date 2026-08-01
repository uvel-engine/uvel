using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Uvel.UI.Animation
{
    /// <summary>
    /// Animation types
    /// </summary>
    public enum AnimationType
    {
        Color,
        Size,
        Position,
        Opacity,
        Scale,
        Custom
    }
    
    /// <summary>
    /// Single animation definition
    /// </summary>
    public class UIAnimation
    {
        public Control Target { get; set; }
        public AnimationType Type { get; set; }
        public object FromValue { get; set; }
        public object ToValue { get; set; }
        public int Duration { get; set; }
        public int ElapsedTime { get; set; }
        public Func<float, float> EasingFunction { get; set; }
        public Action<Control, float> UpdateAction { get; set; }
        public Action OnComplete { get; set; }
        public bool IsCompleted { get; set; }
        
        public UIAnimation()
        {
            Duration = 200;
            ElapsedTime = 0;
            EasingFunction = Easing.EaseOutCubic;
            IsCompleted = false;
        }
    }
    
    /// <summary>
    /// Animation Manager - handles all UI animations
    /// </summary>
    public class AnimationManager : IDisposable
    {
        private static AnimationManager _instance;
        private Timer _timer;
        private List<UIAnimation> _animations;
        private const int FRAME_RATE = 16; // ~60fps
        
        // ═══════════════════════════════════════════
        // SINGLETON
        // ═══════════════════════════════════════════
        
        public static AnimationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AnimationManager();
                }
                return _instance;
            }
        }
        
        // ═══════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════
        
        private AnimationManager()
        {
            _animations = new List<UIAnimation>();
            _timer = new Timer();
            _timer.Interval = FRAME_RATE;
            _timer.Tick += OnTimerTick;
        }
        
        // ═══════════════════════════════════════════
        // TIMER
        // ═══════════════════════════════════════════
        
        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_animations.Count == 0)
            {
                _timer.Stop();
                return;
            }
            
            List<UIAnimation> completed = new List<UIAnimation>();
            
            foreach (UIAnimation anim in _animations)
            {
                anim.ElapsedTime += FRAME_RATE;
                
                float progress = (float)anim.ElapsedTime / anim.Duration;
                if (progress >= 1.0f)
                {
                    progress = 1.0f;
                    anim.IsCompleted = true;
                    completed.Add(anim);
                }
                
                float easedProgress = anim.EasingFunction(progress);
                
                if (anim.UpdateAction != null && anim.Target != null)
                {
                    try
                    {
                        anim.UpdateAction(anim.Target, easedProgress);
                    }
                    catch { }
                }
            }
            
            foreach (UIAnimation anim in completed)
            {
                _animations.Remove(anim);
                if (anim.OnComplete != null)
                {
                    anim.OnComplete();
                }
            }
        }
        
        // ═══════════════════════════════════════════
        // START/STOP
        // ═══════════════════════════════════════════
        
        public void StartAnimation(UIAnimation animation)
        {
            // Remove existing animation for same target and type
            _animations.RemoveAll(a => a.Target == animation.Target && a.Type == animation.Type);
            
            _animations.Add(animation);
            
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }
        
        public void StopAnimation(Control target)
        {
            _animations.RemoveAll(a => a.Target == target);
        }
        
        public void StopAll()
        {
            _animations.Clear();
            _timer.Stop();
        }
        
        // ═══════════════════════════════════════════
        // COLOR ANIMATION
        // ═══════════════════════════════════════════
        
        public void AnimateColor(Control target, Color fromColor, Color toColor, int duration, Action onComplete)
        {
            UIAnimation anim = new UIAnimation();
            anim.Target = target;
            anim.Type = AnimationType.Color;
            anim.FromValue = fromColor;
            anim.ToValue = toColor;
            anim.Duration = duration;
            anim.OnComplete = onComplete;
            
            anim.UpdateAction = delegate(Control ctrl, float progress)
            {
                Color from = (Color)anim.FromValue;
                Color to = (Color)anim.ToValue;
                
                int r = (int)(from.R + (to.R - from.R) * progress);
                int g = (int)(from.G + (to.G - from.G) * progress);
                int b = (int)(from.B + (to.B - from.B) * progress);
                int a = (int)(from.A + (to.A - from.A) * progress);
                
                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));
                a = Math.Max(0, Math.Min(255, a));
                
                ctrl.BackColor = Color.FromArgb(a, r, g, b);
            };
            
            StartAnimation(anim);
        }
        
        public void AnimateColor(Control target, Color fromColor, Color toColor, int duration)
        {
            AnimateColor(target, fromColor, toColor, duration, null);
        }
        
        // ═══════════════════════════════════════════
        // SIZE ANIMATION
        // ═══════════════════════════════════════════
        
        public void AnimateSize(Control target, Size fromSize, Size toSize, int duration, Action onComplete)
        {
            UIAnimation anim = new UIAnimation();
            anim.Target = target;
            anim.Type = AnimationType.Size;
            anim.FromValue = fromSize;
            anim.ToValue = toSize;
            anim.Duration = duration;
            anim.OnComplete = onComplete;
            
            anim.UpdateAction = delegate(Control ctrl, float progress)
            {
                Size from = (Size)anim.FromValue;
                Size to = (Size)anim.ToValue;
                
                int w = (int)(from.Width + (to.Width - from.Width) * progress);
                int h = (int)(from.Height + (to.Height - from.Height) * progress);
                
                ctrl.Size = new Size(w, h);
            };
            
            StartAnimation(anim);
        }
        
        public void AnimateSize(Control target, Size fromSize, Size toSize, int duration)
        {
            AnimateSize(target, fromSize, toSize, duration, null);
        }
        
        // ═══════════════════════════════════════════
        // POSITION ANIMATION
        // ═══════════════════════════════════════════
        
        public void AnimatePosition(Control target, Point fromPos, Point toPos, int duration, Action onComplete)
        {
            UIAnimation anim = new UIAnimation();
            anim.Target = target;
            anim.Type = AnimationType.Position;
            anim.FromValue = fromPos;
            anim.ToValue = toPos;
            anim.Duration = duration;
            anim.OnComplete = onComplete;
            
            anim.UpdateAction = delegate(Control ctrl, float progress)
            {
                Point from = (Point)anim.FromValue;
                Point to = (Point)anim.ToValue;
                
                int x = (int)(from.X + (to.X - from.X) * progress);
                int y = (int)(from.Y + (to.Y - from.Y) * progress);
                
                ctrl.Location = new Point(x, y);
            };
            
            StartAnimation(anim);
        }
        
        public void AnimatePosition(Control target, Point fromPos, Point toPos, int duration)
        {
            AnimatePosition(target, fromPos, toPos, duration, null);
        }
        
        // ═══════════════════════════════════════════
        // CUSTOM ANIMATION
        // ═══════════════════════════════════════════
        
        public void AnimateCustom(Control target, Action<Control, float> updateAction, int duration, Action onComplete)
        {
            UIAnimation anim = new UIAnimation();
            anim.Target = target;
            anim.Type = AnimationType.Custom;
            anim.Duration = duration;
            anim.UpdateAction = updateAction;
            anim.OnComplete = onComplete;
            
            StartAnimation(anim);
        }
        
        public void AnimateCustom(Control target, Action<Control, float> updateAction, int duration)
        {
            AnimateCustom(target, updateAction, duration, null);
        }
        
        // ═══════════════════════════════════════════
        // FADE ANIMATION (using opacity simulation)
        // ═══════════════════════════════════════════
        
        public void FadeIn(Control target, int duration, Action onComplete)
        {
            Color baseColor = target.BackColor;
            Color transparent = Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B);
            AnimateColor(target, transparent, baseColor, duration, onComplete);
        }
        
        public void FadeIn(Control target, int duration)
        {
            FadeIn(target, duration, null);
        }
        
        public void FadeOut(Control target, int duration, Action onComplete)
        {
            Color baseColor = target.BackColor;
            Color transparent = Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B);
            AnimateColor(target, baseColor, transparent, duration, onComplete);
        }
        
        public void FadeOut(Control target, int duration)
        {
            FadeOut(target, duration, null);
        }
        
        // ═══════════════════════════════════════════
        // SCALE ANIMATION
        // ═══════════════════════════════════════════
        
        public void AnimateScale(Control target, float fromScale, float toScale, int duration, Action onComplete)
        {
            Size originalSize = target.Size;
            
            UIAnimation anim = new UIAnimation();
            anim.Target = target;
            anim.Type = AnimationType.Scale;
            anim.FromValue = fromScale;
            anim.ToValue = toScale;
            anim.Duration = duration;
            anim.OnComplete = onComplete;
            
            anim.UpdateAction = delegate(Control ctrl, float progress)
            {
                float from = (float)anim.FromValue;
                float to = (float)anim.ToValue;
                float scale = from + (to - from) * progress;
                
                int w = (int)(originalSize.Width * scale);
                int h = (int)(originalSize.Height * scale);
                
                ctrl.Size = new Size(w, h);
            };
            
            StartAnimation(anim);
        }
        
        public void AnimateScale(Control target, float fromScale, float toScale, int duration)
        {
            AnimateScale(target, fromScale, toScale, duration, null);
        }
        
        // ═══════════════════════════════════════════
        // PULSE ANIMATION
        // ═══════════════════════════════════════════
        
        public void Pulse(Control target, int duration)
        {
            Color baseColor = target.BackColor;
            Color highlight = Styles.Theme.Lighten(baseColor, 0.3f);
            
            AnimateColor(target, baseColor, highlight, duration / 2, delegate
            {
                AnimateColor(target, highlight, baseColor, duration / 2);
            });
        }
        
        // ═══════════════════════════════════════════
        // DISPOSE
        // ═══════════════════════════════════════════
        
        public void Dispose()
        {
            StopAll();
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
