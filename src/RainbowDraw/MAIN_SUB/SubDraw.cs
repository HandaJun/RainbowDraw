using RainbowDraw.LOGIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        long elliNum = 0;
        Dictionary<string, List<Point>> elliList = new Dictionary<string, List<Point>>();

        public void InsertElli(Point cp, Brush color, bool save = true)
        {
            if(cp.X <= 10 || cp.Y <= 10 || startX <= 10 || startY <= 10)
            {
                return;
            }

            if (save && Common.rainbowFlg)
            {
                List<Point> pointList = new List<Point>();
                pointList.Add(cp);

                int interval = 1;
                if (Common.curLineSize < 4)
                {
                    interval = 1;
                }

                double sabunX = Math.Abs(startX - cp.X);
                double sabunY = Math.Abs(startY - cp.Y);
                if (sabunX > interval || sabunY > interval)
                {
                    int loop = (int)Math.Max(sabunX, sabunY) / interval;
                    double tempX = startX;
                    double tempY = startY;
                    for (int i = 0; i < loop; i++)
                    {

                        if (cp.X > startX)
                        {
                            if (tempX < cp.X)
                            {
                                tempX += sabunX / loop;
                            }
                        }
                        else
                        {
                            if (tempX > cp.X)
                            {
                                tempX -= sabunX / loop;
                            }
                        }

                        if (cp.Y > startY)
                        {
                            if (tempY < cp.Y)
                            {
                                tempY += sabunY / loop;
                            }
                        }
                        else
                        {
                            if (tempY > cp.Y)
                            {
                                tempY -= sabunY / loop;
                            }
                        }
                        Point tempPoint = new Point(tempX, tempY);
                        //pointList.Add(tempPoint);
                        InsertElli(tempPoint, color, false);
                    }
                }

                //elliList.Add(elliNum.ToString(), pointList);

                startX = cp.X;
                startY = cp.Y;
            }

            CreateAnEllipse(color, cp);
        }

        double EllipseMinLeft = 2000;
        double EllipseMinTop = 2000;

        public void CreateAnEllipse(Brush b, Point p)
        {
            double thickness = Common.curLineSize;
            if (Common.nowMode == DRAW_MODE.HIGHLIGHTER || Common.nowMode == DRAW_MODE.DRAW_HIGHLIGHTER)
            {
                thickness = thickness * 2 + 3;

                Rectangle rt = new Rectangle();
                rt.Height = thickness;
                rt.Width = thickness;
                rt.StrokeThickness = thickness;
                rt.Stroke = b.Clone();
                rt.Fill = b.Clone();
                double left = p.X - (thickness / 3);
                double top = p.Y - (thickness / 2);
                Canvas.SetLeft(rt, left);
                Canvas.SetTop(rt, top);
                if (left < EllipseMinLeft)
                {
                    EllipseMinLeft = left;
                }
                if (top < EllipseMinTop)
                {
                    EllipseMinTop = top;
                }

                try
                {
                    nowCanvas.Children.Add(rt);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                Ellipse el = new Ellipse();
                el.Height = thickness;
                el.Width = thickness;
                el.StrokeThickness = thickness;
                el.Stroke = b.Clone();
                el.Fill = b.Clone();
                double left = p.X - (thickness / 2);
                double top = p.Y - (thickness / 2);
                Canvas.SetLeft(el, left);
                Canvas.SetTop(el, top);
                if (left < EllipseMinLeft)
                {
                    EllipseMinLeft = left;
                }
                if (top < EllipseMinTop)
                {
                    EllipseMinTop = top;
                }

                try
                {
                    nowCanvas.Children.Add(el);
                }
                catch (Exception)
                {
                }
            }

            
            

        }
    }
}
