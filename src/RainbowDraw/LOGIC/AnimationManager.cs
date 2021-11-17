using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RainbowDraw.LOGIC
{
    public class AnimationManager
    {
        public static DoubleAnimation DoubleAni(double from = 0, double to = 0, double time = 0.2, IEasingFunction easingFun = null)
        {
            QuarticEase easingFunction = new QuarticEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            DoubleAnimation ani;
            ani = new DoubleAnimation();
            ani.From = from;
            ani.To = to;
            ani.Duration = new Duration(TimeSpan.FromSeconds(time));
            if (easingFun == null)
            {
                ani.EasingFunction = easingFunction;
            }
            else
            {
                ani.EasingFunction = easingFun;
            }
            return ani;
        }

        public static ThicknessAnimation ThicknessAni(Thickness from = new Thickness(), Thickness to = new Thickness(), double time = 0.3, IEasingFunction easingFun = null)
        {
            QuarticEase easingFunction = new QuarticEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            ThicknessAnimation ani;
            ani = new ThicknessAnimation();
            if (from == new Thickness())
            {
                ani.From = new Thickness(0, 0, 0, 0);
            }
            else
            {
                ani.From = from;
            }
            if (to == new Thickness())
            {
                ani.To = new Thickness(0, 0, 0, 0);
            }
            else
            {
                ani.To = to;
            }
            ani.Duration = new Duration(TimeSpan.FromSeconds(1));
            if (easingFun == null)
            {
                ani.EasingFunction = easingFunction;
            }
            else
            {
                ani.EasingFunction = easingFun;
            }
            return ani;
        }

        public static ColorAnimation ColorAni(Color from, Color to, double time)
        {
            ColorAnimation ani;
            ani = new ColorAnimation();
            ani.From = from;
            ani.To = to;
            ani.Duration = new Duration(TimeSpan.FromSeconds(time));
            return ani;
        }
    }
}
