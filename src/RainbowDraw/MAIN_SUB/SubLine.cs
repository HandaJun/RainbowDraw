﻿using RainbowDraw.LOGIC;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        public static double _rad5;

        public static void SetLineRad(int angle)
        {
            _rad5 = DegreeToRadian(angle);
        }

        public void CreateLine()
        {
            _rad5 = DegreeToRadian(2);
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
                VerticalAlignment = VerticalAlignment.Top
            };
            double thickness = Common.curLineSize;
            if (Common.nowMode == DRAW_MODE.HIGHLIGHTER_LINE)
            {
                thickness = thickness * 2 + 3;
                line.StrokeStartLineCap = PenLineCap.Flat;
                line.StrokeEndLineCap = PenLineCap.Flat;
            }
            else
            {
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;
            }
            line.StrokeThickness = thickness;
            nowCanvas.Children.Add(line);
        }

        public void RePaintLine(Point p)
        {
            double distX = Math.Abs(p.X - startX);
            double distY = Math.Abs(p.Y - startY);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                _rad5 = DegreeToRadian(45);
            }
            else
            {
                _rad5 = DegreeToRadian(2);
            }

            float angle = (float)Math.Atan2(p.Y - startY, p.X - startX);
            double step = _rad5;
            double finalAngle;
            double c = angle % _rad5;
            finalAngle = angle - c;
            if (c > step / 2)
            {
                finalAngle = (angle - c) + step;
            }
            double length = Math.Sqrt((Math.Pow(startX - p.X, 2) + Math.Pow(startY - p.Y, 2)));
            line.X2 = (int)((int)startX + Math.Cos(finalAngle) * length);
            line.Y2 = (int)((int)startY + Math.Sin(finalAngle) * length);

            double addProg = Math.Min((distX + distY) / 10000d, 0.3);
            addProg = Math.Max(addProg, 0.06f);
            LinearGradientBrush lg = line.Stroke as LinearGradientBrush;
            foreach (var gs in lg.GradientStops)
            {
                if (gs.Offset == 1)
                {
                    gs.Color = ((SolidColorBrush)Rainbow(Common.rainbowProgress + (float)addProg)).Color;
                }
            }
            lg.StartPoint = new Point(p.X >= startX ? 0 : 1, p.Y >= startY ? 0 : 1);
            lg.EndPoint = new Point(p.X >= startX ? 1 : 0, p.Y >= startY ? 1 : 0);
            line.Tag = addProg.ToString();
        }

        private static double DegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        public Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X = (int)(cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y = (int)(sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
    }
}
