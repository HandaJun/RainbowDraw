using RainbowDraw.LOGIC;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        public void CreateRect()
        {
            double thickness = Common.curLineSize;
            if (Common.nowMode == DRAW_MODE.HIGHLIGHTER_RECTANGLE)
            {
                thickness = thickness * 2 + 3;
            }
            rectBd = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(5),
                Width = 0,
                Height = 0
            };
            double minus = thickness / 2;
            Canvas.SetLeft(rectBd, startX - minus);
            Canvas.SetTop(rectBd, startY - minus);
            LinearGradientBrush lg = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            lg.GradientStops.Add(new GradientStop(((SolidColorBrush)curColor).Color, 0));
            lg.GradientStops.Add(new GradientStop(((SolidColorBrush)curColor).Color, 1));
            rectBd.BorderBrush = lg;
            if (Common.nowMode == DRAW_MODE.RECTANGLE_FULLFILL || Common.nowMode == DRAW_MODE.RECTANGLE_ROUND_FULLFILL)
            {
                rectBd.Background = lg.Clone();
                rectBd.BorderThickness = new Thickness(0);
            }
            else
            {
                rectBd.BorderThickness = new Thickness(thickness);
            }
            nowCanvas.Children.Add(rectBd);
        }

        public void RePaintRect(Point p)
        {
            double thickness = Common.curLineSize;
            if (Common.nowMode == DRAW_MODE.HIGHLIGHTER_RECTANGLE)
            {
                thickness = thickness * 2 + 3;
            }
            double minus = thickness / 2;
            rectBd.Width = Math.Abs(p.X - startX + minus);
            rectBd.Height = Math.Abs(p.Y - startY + minus);
            if (Common.nowMode == DRAW_MODE.RECTANGLE_ROUND || Common.nowMode == DRAW_MODE.RECTANGLE_ROUND_FULLFILL)
            {
                rectBd.CornerRadius = new CornerRadius(Math.Max(rectBd.Width, rectBd.Height));
            }
            Canvas.SetLeft(rectBd, Math.Min(p.X, startX - minus));
            Canvas.SetTop(rectBd, Math.Min(p.Y, startY - minus));
            double addProg = Math.Min((rectBd.Width + rectBd.Height) / 10000d, 0.3);
            addProg = Math.Max(addProg, 0.06f);
            LinearGradientBrush lg = rectBd.BorderBrush as LinearGradientBrush;
            foreach (var gs in lg.GradientStops)
            {
                if (gs.Offset == 1)
                {
                    gs.Color = ((SolidColorBrush)Rainbow(Common.rainbowProgress + (float)addProg)).Color;
                }
            }
            lg.StartPoint = new Point(p.X >= startX ? 0 : 1, p.Y >= startY ? 0 : 1);
            lg.EndPoint = new Point(p.X >= startX ? 1 : 0, p.Y >= startY ? 1 : 0);

            if (Common.nowMode == DRAW_MODE.RECTANGLE_FULLFILL || Common.nowMode == DRAW_MODE.RECTANGLE_ROUND_FULLFILL)
            {
                LinearGradientBrush lg2 = rectBd.Background as LinearGradientBrush;
                foreach (var gs in lg2.GradientStops)
                {
                    if (gs.Offset == 1)
                    {
                        gs.Color = ((SolidColorBrush)Rainbow(Common.rainbowProgress + (float)addProg)).Color;
                    }
                }
                lg2.StartPoint = new Point(p.X >= startX ? 0 : 1, p.Y >= startY ? 0 : 1);
                lg2.EndPoint = new Point(p.X >= startX ? 1 : 0, p.Y >= startY ? 1 : 0);
            }

            rectBd.Tag = addProg.ToString();
        }
    }
}
