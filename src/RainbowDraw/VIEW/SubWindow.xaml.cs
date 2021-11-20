using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RainbowDraw
{
    /// <summary>
    /// Interaction logic for SubWindow.xaml
    /// </summary>
    public partial class SubWindow : Window
    {
        Brush curColor = Brushes.White;
        readonly int curLineSize = 5;
        bool rainbowFlg = false;
        private static readonly IntPtr intPtr = new IntPtr(0);
        float rainbowProgress = 0;
        readonly Dictionary<string, Canvas> CanvasDic = new Dictionary<string, Canvas>();
        Canvas nowCanvas = null;
        readonly Stack<string> CanvasIdStack = new Stack<string>();
        new bool IsLoaded = false;
        readonly DRAW_MODE nowMode = DRAW_MODE.DRAW;

        Border rectBd = null;
        Line line = null;
        Polygon poly = null;
        TextBox textBox = null;

        bool drawFlg = false;

        double startX = 0;
        double startY = 0;

        public SubWindow()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            MainWindow.GetInstance().SetMouseFlg();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            can.Children.Clear();
            curColor = Rainbow(0);
            Task.Run(() =>
            {
                while (true)
                {
                    if (rainbowFlg)
                    {
                        NextRainbow();
                    }
                    Thread.Sleep(100);
                }
            });
            Handle = new WindowInteropHelper(this).Handle;
            IsLoaded = true;
        }

        public Brush NextRainbow(float ascending = 0.01f)
        {
            Brush rtn = Brushes.Transparent;
            rainbowProgress += ascending;
            if (rainbowProgress > 1f)
            {
                rainbowProgress -= 1f;
            }
            this.Dispatcher.Invoke(() =>
            {
                rtn = Rainbow(rainbowProgress);
                curColor = rtn;
            });
            return rtn;
        }

        public Brush Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;
            Color c;
            switch ((int)div)
            {
                case 0:
                    c = Color.FromArgb(255, 255, (byte)ascending, 0);
                    break;
                case 1:
                    c = Color.FromArgb(255, (byte)descending, 255, 0);
                    break;
                case 2:
                    c = Color.FromArgb(255, 0, 255, (byte)ascending);
                    break;
                case 3:
                    c = Color.FromArgb(255, 0, (byte)descending, 255);
                    break;
                case 4:
                    c = Color.FromArgb(255, (byte)ascending, 0, 255);
                    break;
                default: // case 5:
                    c = Color.FromArgb(255, 255, 0, (byte)descending);
                    break;
            }
            return new SolidColorBrush(c);
        }

        private void Can_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLoaded && e.ChangedButton == MouseButton.Left)
            {
                StartDraw();
            }
        }

        public void StartDraw()
        {
            try
            {
                Keyboard.ClearFocus();
                nowCanvas = new Canvas();
                string id = "Can_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                can.RegisterName(id, nowCanvas);
                can.Children.Add(nowCanvas);
                CanvasDic.Add(id, nowCanvas);
                CanvasIdStack.Push(id);
                var position = Mouse.GetPosition(this);
                startX = position.X;
                startY = position.Y;

                switch (nowMode)
                {
                    case DRAW_MODE.DRAW:
                        rainbowFlg = true;
                        InsertElli(position, curColor.Clone());
                        break;
                    case DRAW_MODE.RECTANGLE:
                        CreateRect();
                        break;
                    case DRAW_MODE.LINE:
                        CreateLine();
                        break;
                    case DRAW_MODE.ARROW:
                        CreateArrow();
                        break;
                    case DRAW_MODE.TEXT:
                        CreateTextbox();
                        break;
                    case DRAW_MODE.ERASER:
                        break;
                    default:
                        break;
                }
                drawFlg = true;
            }
            catch (Exception)
            {
            }
        }

        private void Can_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsLoaded && drawFlg && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = Mouse.GetPosition(this);
                Point curPoint = new Point(position.X, position.Y);

                switch (nowMode)
                {
                    case DRAW_MODE.DRAW:
                        InsertElli(curPoint, curColor.Clone());
                        break;
                    case DRAW_MODE.RECTANGLE:
                        RePaintRect(position);
                        break;
                    case DRAW_MODE.LINE:
                        RePaintLine(position);
                        break;
                    case DRAW_MODE.ARROW:
                        RePaintArrow(position);
                        break;
                    case DRAW_MODE.ERASER:
                        break;
                    default:
                        break;
                }

            }
        }

        private void Can_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLoaded && drawFlg && e.ChangedButton == MouseButton.Left)
            {
                switch (nowMode)
                {
                    case DRAW_MODE.DRAW:
                        rainbowFlg = false;
                        break;
                    case DRAW_MODE.RECTANGLE:
                        if (rectBd.Tag != null)
                        {
                            NextRainbow(float.Parse(rectBd.Tag.ToString()));
                        }
                        rectBd = null;
                        break;
                    case DRAW_MODE.LINE:
                        if (line.Tag != null)
                        {
                            NextRainbow(float.Parse(line.Tag.ToString()));
                        }
                        line = null;
                        break;
                    case DRAW_MODE.ARROW:
                        if (line.Tag != null)
                        {
                            NextRainbow(float.Parse(line.Tag.ToString()));
                        }
                        line = null;
                        break;
                    case DRAW_MODE.ERASER:
                        break;
                    default:
                        break;
                }
                drawFlg = false;
                can.InvalidateVisual();
            }
        }

        public void InsertElli(Point cp, Brush color, bool save = true)
        {
            if (save && rainbowFlg)
            {
                int interval = 2;
                if (curLineSize < 4)
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
                        InsertElli(tempPoint, color, false);
                    }
                }

                startX = cp.X;
                startY = cp.Y;
            }

            CreateAnEllipse(color, cp);
        }

        public void CreateAnEllipse(Brush b, Point p)
        {
            Ellipse el = new Ellipse
            {
                Height = curLineSize,
                Width = curLineSize,
                StrokeThickness = curLineSize,
                Stroke = b.Clone(),
                Fill = b.Clone()
            };
            Canvas.SetLeft(el, p.X);
            Canvas.SetTop(el, p.Y);
            try
            {
                nowCanvas.Children.Add(el);
            }
            catch (Exception)
            {
            }
        }

        public void CreateRect()
        {
            rectBd = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(curLineSize),
                Width = 0,
                Height = 0
            };
            Canvas.SetLeft(rectBd, startX);
            Canvas.SetTop(rectBd, startY);
            LinearGradientBrush lg = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            lg.GradientStops.Add(new GradientStop(((SolidColorBrush)curColor).Color, 0));
            lg.GradientStops.Add(new GradientStop(((SolidColorBrush)curColor).Color, 1));
            rectBd.BorderBrush = lg;
            nowCanvas.Children.Add(rectBd);
        }

        public void RePaintRect(Point p)
        {
            rectBd.Width = Math.Abs(p.X - startX);
            rectBd.Height = Math.Abs(p.Y - startY);
            Canvas.SetLeft(rectBd, Math.Min(p.X, startX));
            Canvas.SetTop(rectBd, Math.Min(p.Y, startY));
            double addProg = Math.Min((rectBd.Width + rectBd.Height) / 10000d, 0.3);
            addProg = Math.Max(addProg, 0.05f);
            LinearGradientBrush lg = rectBd.BorderBrush as LinearGradientBrush;
            foreach (var gs in lg.GradientStops)
            {
                if (gs.Offset == 1)
                {
                    gs.Color = ((SolidColorBrush)Rainbow(rainbowProgress + (float)addProg)).Color;
                }
            }
            lg.StartPoint = new Point(p.X >= startX ? 0 : 1, p.Y >= startY ? 0 : 1);
            lg.EndPoint = new Point(p.X >= startX ? 1 : 0, p.Y >= startY ? 1 : 0);
            rectBd.Tag = addProg.ToString();
        }


        public void CreateLine()
        {
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
                StrokeThickness = curLineSize,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            nowCanvas.Children.Add(line);
        }

        public void RePaintLine(Point p)
        {
            double distX = Math.Abs(p.X - startX);
            double distY = Math.Abs(p.Y - startY);
            line.X2 = p.X;
            line.Y2 = p.Y;
            double addProg = Math.Min((distX + distY) / 10000d, 0.3);
            addProg = Math.Max(addProg, 0.05f);
            LinearGradientBrush lg = line.Stroke as LinearGradientBrush;
            foreach (var gs in lg.GradientStops)
            {
                if (gs.Offset == 1)
                {
                    gs.Color = ((SolidColorBrush)Rainbow(rainbowProgress + (float)addProg)).Color;
                }
            }
            lg.StartPoint = new Point(p.X >= startX ? 0 : 1, p.Y >= startY ? 0 : 1);
            lg.EndPoint = new Point(p.X >= startX ? 1 : 0, p.Y >= startY ? 1 : 0);
            line.Tag = addProg.ToString();
        }

        public void CreateArrow()
        {
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
                StrokeThickness = curLineSize,
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
            double arrowScale = 1 + ((curLineSize - 5) * 0.15d);
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
            addProg = Math.Max(addProg, 0.05f);
            Brush endBrush = Rainbow(rainbowProgress + (float)addProg);
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


        public void CreateTextbox()
        {
            textBox = new TextBox
            {
                FontSize = 50,
                MinWidth = 30,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };
            Canvas.SetLeft(textBox, startX);
            Canvas.SetTop(textBox, startY);
            textBox.KeyUp += TextBox_KeyUp;
            TextCompositionManager.AddPreviewTextInputUpdateHandler(textBox, TextBox_PreviewTextInputUpdate);
            TextCompositionManager.AddPreviewTextInputHandler(textBox, TextBox_PreviewTextInput);
            textBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            textBox.LostKeyboardFocus += TextBox_LostKeyboardFocus;

            LinearGradientBrush lg = new LinearGradientBrush
            {
                MappingMode = BrushMappingMode.Absolute,
                EndPoint = new Point(20, 0)
            };
            Color c = ((SolidColorBrush)curColor).Color;
            lg.GradientStops.Add(new GradientStop(c, 0));
            lg.GradientStops.Add(new GradientStop(c, 1));
            textBox.Foreground = lg;
            textBox.Tag = (curColor as SolidColorBrush).Color.ToString();
            nowCanvas.Children.Add(textBox);
            textBox.Focus();
        }


        private bool isImeOnConv = false;

        public IntPtr Handle { get; set; } = intPtr;

        private void TextBox_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.TextComposition.CompositionText.Length == 0)
            {
                isImeOnConv = false;
            }
            else
            {
                tb.Foreground = curColor;
                isImeOnConv = true;
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (isImeOnConv == false)
            {
                TextBox tb = sender as TextBox;
                Color c = (Color)ColorConverter.ConvertFromString(tb.Tag.ToString());

                LinearGradientBrush lg = new LinearGradientBrush
                {
                    //lg.StartPoint = new Point(0, 0);
                    MappingMode = BrushMappingMode.Absolute,
                    EndPoint = new Point(tb.ActualWidth, tb.ActualHeight)
                };
                lg.GradientStops.Add(new GradientStop(c, 0));
                Brush endBrush = Rainbow(rainbowProgress + (float)0.2);
                Color endColor = ((SolidColorBrush)endBrush).Color;
                lg.GradientStops.Add(new GradientStop(endColor, 1));
                tb.Foreground = lg;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            isImeOnConv = false;
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            }
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 150, 150, 150));
            }
        }

    }
}
