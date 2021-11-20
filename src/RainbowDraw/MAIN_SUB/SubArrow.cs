using RainbowDraw.LOGIC;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        public void CreateArrow()
        {
            double thickness = Common.curLineSize;
            if (Common.nowMode == DRAW_MODE.HIGHLIGHTER_ARROW)
            {
                thickness = thickness * 2 + 3;
            }
            LinearGradientBrush lg = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            lg.GradientStops.Add(new GradientStop(((SolidColorBrush)curColor).Color, 0));
            lg.GradientStops.Add(new GradientStop(((SolidColorBrush)curColor).Color, 1));
            line = new Line
            {
                Stroke = lg,
                X1 = startX,
                Y1 = startY,
                X2 = startX,
                Y2 = startY,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Flat
            };
            nowCanvas.Children.Add(line);

            PointCollection pc = new PointCollection
            {
                new Point(-1, -1),
                new Point(25, 10),
                new Point(10, 25)
            };
            poly = new Polygon
            {
                Points = pc,
                StrokeLineJoin = PenLineJoin.Round,
                Stroke = curColor,
                Fill = curColor,
                StrokeThickness = 5,
                Height = 30,
                Width = 30,
                RenderTransformOrigin = new Point(0, 0)
            };
            Canvas.SetLeft(poly, startX);
            Canvas.SetTop(poly, startY);

            RotateTransform rt = new RotateTransform
            {
                Angle = 45
            };
            double arrowScale = 1 + ((thickness - 5) * 0.15d);
            ScaleTransform st = new ScaleTransform
            {
                ScaleX = arrowScale,
                ScaleY = arrowScale
            };
            TransformGroup tfg = new TransformGroup();
            tfg.Children.Add(rt);
            tfg.Children.Add(st);
            poly.RenderTransform = tfg;
            nowCanvas.Children.Add(poly);
        }

        public void RePaintArrow(Point p)
        {
            double distX = Math.Abs(p.X - startX);
            double distY = Math.Abs(p.Y - startY);
            line.X2 = p.X;
            line.Y2 = p.Y;
            double addProg = Math.Min((distX + distY) / 10000d, 0.3);
            addProg = Math.Max(addProg, 0.06f);
            Brush endBrush = Rainbow(Common.rainbowProgress + (float)addProg);
            Color endColor = ((SolidColorBrush)endBrush).Color;
            LinearGradientBrush lg = line.Stroke as LinearGradientBrush;
            foreach (var gs in lg.GradientStops)
            {
                if (gs.Offset == 1)
                {
                    gs.Color = endColor;
                }
            }
            lg.StartPoint = new Point(p.X >= startX ? 0 : 1, p.Y >= startY ? 0 : 1);
            lg.EndPoint = new Point(p.X >= startX ? 1 : 0, p.Y >= startY ? 1 : 0);
            line.Tag = addProg.ToString();

            poly.Stroke = endBrush;
            poly.Fill = endBrush;
            Canvas.SetLeft(poly, p.X);
            Canvas.SetTop(poly, p.Y);
            TransformGroup tfg = poly.RenderTransform as TransformGroup;
            foreach (var item in tfg.Children)
            {
                if (item is RotateTransform)
                {
                    (item as RotateTransform).Angle = GetAngle(new Point(startX, startY), p) + 135;
                }
            }

        }

        public double GetAngle(Point start, Point end)
        {
            double dy = end.Y - start.Y;
            double dx = end.X - start.X;
            return Math.Atan2(dy, dx) * (180.0 / Math.PI);
        }
    }
}
