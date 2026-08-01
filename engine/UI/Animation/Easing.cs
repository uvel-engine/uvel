using System;

namespace Uvel.UI.Animation
{
    /// <summary>
    /// Easing functions for smooth animations
    /// Based on CSS easing and Material Design motion
    /// </summary>
    public static class Easing
    {
        // ═══════════════════════════════════════════
        // STANDARD EASING
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Linear - no easing
        /// </summary>
        public static float Linear(float t)
        {
            return t;
        }
        
        // ═══════════════════════════════════════════
        // EASE IN
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Ease In Quad - slow start
        /// </summary>
        public static float EaseInQuad(float t)
        {
            return t * t;
        }
        
        /// <summary>
        /// Ease In Cubic - slower start
        /// </summary>
        public static float EaseInCubic(float t)
        {
            return t * t * t;
        }
        
        /// <summary>
        /// Ease In Quart
        /// </summary>
        public static float EaseInQuart(float t)
        {
            return t * t * t * t;
        }
        
        /// <summary>
        /// Ease In Quint
        /// </summary>
        public static float EaseInQuint(float t)
        {
            return t * t * t * t * t;
        }
        
        /// <summary>
        /// Ease In Sine
        /// </summary>
        public static float EaseInSine(float t)
        {
            return 1 - (float)Math.Cos(t * Math.PI / 2);
        }
        
        /// <summary>
        /// Ease In Expo
        /// </summary>
        public static float EaseInExpo(float t)
        {
            return t == 0 ? 0 : (float)Math.Pow(2, 10 * (t - 1));
        }
        
        /// <summary>
        /// Ease In Circ
        /// </summary>
        public static float EaseInCirc(float t)
        {
            return 1 - (float)Math.Sqrt(1 - t * t);
        }
        
        // ═══════════════════════════════════════════
        // EASE OUT
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Ease Out Quad - slow end
        /// </summary>
        public static float EaseOutQuad(float t)
        {
            return t * (2 - t);
        }
        
        /// <summary>
        /// Ease Out Cubic - slower end
        /// </summary>
        public static float EaseOutCubic(float t)
        {
            float f = t - 1;
            return f * f * f + 1;
        }
        
        /// <summary>
        /// Ease Out Quart
        /// </summary>
        public static float EaseOutQuart(float t)
        {
            float f = t - 1;
            return 1 - f * f * f * f;
        }
        
        /// <summary>
        /// Ease Out Quint
        /// </summary>
        public static float EaseOutQuint(float t)
        {
            float f = t - 1;
            return 1 + f * f * f * f * f;
        }
        
        /// <summary>
        /// Ease Out Sine
        /// </summary>
        public static float EaseOutSine(float t)
        {
            return (float)Math.Sin(t * Math.PI / 2);
        }
        
        /// <summary>
        /// Ease Out Expo
        /// </summary>
        public static float EaseOutExpo(float t)
        {
            return t == 1 ? 1 : 1 - (float)Math.Pow(2, -10 * t);
        }
        
        /// <summary>
        /// Ease Out Circ
        /// </summary>
        public static float EaseOutCirc(float t)
        {
            float f = t - 1;
            return (float)Math.Sqrt(1 - f * f);
        }
        
        // ═══════════════════════════════════════════
        // EASE IN OUT
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Ease In Out Quad - slow start and end
        /// </summary>
        public static float EaseInOutQuad(float t)
        {
            if (t < 0.5f)
                return 2 * t * t;
            return -1 + (4 - 2 * t) * t;
        }
        
        /// <summary>
        /// Ease In Out Cubic
        /// </summary>
        public static float EaseInOutCubic(float t)
        {
            if (t < 0.5f)
                return 4 * t * t * t;
            float f = 2 * t - 2;
            return 0.5f * f * f * f + 1;
        }
        
        /// <summary>
        /// Ease In Out Quart
        /// </summary>
        public static float EaseInOutQuart(float t)
        {
            if (t < 0.5f)
                return 8 * t * t * t * t;
            float f = t - 1;
            return 1 - 8 * f * f * f * f;
        }
        
        /// <summary>
        /// Ease In Out Quint
        /// </summary>
        public static float EaseInOutQuint(float t)
        {
            if (t < 0.5f)
                return 16 * t * t * t * t * t;
            float f = 2 * t - 2;
            return 0.5f * f * f * f * f * f + 1;
        }
        
        /// <summary>
        /// Ease In Out Sine
        /// </summary>
        public static float EaseInOutSine(float t)
        {
            return -0.5f * ((float)Math.Cos(Math.PI * t) - 1);
        }
        
        /// <summary>
        /// Ease In Out Expo
        /// </summary>
        public static float EaseInOutExpo(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            if (t < 0.5f)
                return 0.5f * (float)Math.Pow(2, 20 * t - 10);
            return 1 - 0.5f * (float)Math.Pow(2, -20 * t + 10);
        }
        
        /// <summary>
        /// Ease In Out Circ
        /// </summary>
        public static float EaseInOutCirc(float t)
        {
            if (t < 0.5f)
                return 0.5f * (1 - (float)Math.Sqrt(1 - 4 * t * t));
            float f = 2 * t - 2;
            return 0.5f * ((float)Math.Sqrt(1 - f * f) + 1);
        }
        
        // ═══════════════════════════════════════════
        // SPECIAL EASING (Material Design)
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Material Standard - general purpose
        /// </summary>
        public static float Standard(float t)
        {
            return EaseInOutCubic(t);
        }
        
        /// <summary>
        /// Material Emphasized - for important transitions
        /// </summary>
        public static float Emphasized(float t)
        {
            return EaseInOutQuart(t);
        }
        
        /// <summary>
        /// Material Decelerate - for entering elements
        /// </summary>
        public static float Decelerate(float t)
        {
            return EaseOutCubic(t);
        }
        
        /// <summary>
        /// Material Accelerate - for exiting elements
        /// </summary>
        public static float Accelerate(float t)
        {
            return EaseInCubic(t);
        }
        
        // ═══════════════════════════════════════════
        // BOUNCE AND ELASTIC
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Bounce at end
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            if (t < 1 / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
        
        /// <summary>
        /// Elastic at end
        /// </summary>
        public static float EaseOutElastic(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;
            float p = 0.3f;
            float s = p / 4;
            return (float)(Math.Pow(2, -10 * t) * Math.Sin((t - s) * (2 * Math.PI) / p) + 1);
        }
        
        /// <summary>
        /// Back overshoot at end
        /// </summary>
        public static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;
            float f = t - 1;
            return 1 + c3 * f * f * f + c1 * f * f;
        }
        
        // ═══════════════════════════════════════════
        // HELPER
        // ═══════════════════════════════════════════
        
        /// <summary>
        /// Get easing function by name
        /// </summary>
        public static Func<float, float> GetByName(string name)
        {
            switch (name.ToLower())
            {
                case "linear": return Linear;
                case "easeinquad": return EaseInQuad;
                case "easeoutquad": return EaseOutQuad;
                case "easeinoutquad": return EaseInOutQuad;
                case "easeincubic": return EaseInCubic;
                case "easeoutcubic": return EaseOutCubic;
                case "easeinoutcubic": return EaseInOutCubic;
                case "easeinsine": return EaseInSine;
                case "easeoutsine": return EaseOutSine;
                case "easeinoutsine": return EaseInOutSine;
                case "easeinexpo": return EaseInExpo;
                case "easeoutexpo": return EaseOutExpo;
                case "easeinoutexpo": return EaseInOutExpo;
                case "standard": return Standard;
                case "emphasized": return Emphasized;
                case "decelerate": return Decelerate;
                case "accelerate": return Accelerate;
                case "easeoutbounce": return EaseOutBounce;
                case "easeoutelastic": return EaseOutElastic;
                case "easeoutback": return EaseOutBack;
                default: return EaseOutCubic;
            }
        }
    }
}
