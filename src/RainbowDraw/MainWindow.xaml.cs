using FontAwesome.WPF;
using Microsoft.Win32.SafeHandles;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using RainbowDraw.DATA;
using RainbowDraw.LOGIC;
using RainbowDraw.VIEW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace RainbowDraw
{

    public enum DRAW_MODE : int
    {
        DRAW = 1,
        DRAW_RECTANGLE = 10,
        DRAW_LINE = 11,
        DRAW_ARROW = 12,
        DRAW_HIGHLIGHTER = 13,
        RECTANGLE = 2,
        RECTANGLE_FULLFILL = 21,
        RECTANGLE_ROUND = 22,
        RECTANGLE_ROUND_FULLFILL = 23,
        LINE = 3,
        ARROW = 4,
        TEXT = 5,
        HIGHLIGHTER = 6,
        HIGHLIGHTER_RECTANGLE = 60,
        HIGHLIGHTER_LINE = 61,
        HIGHLIGHTER_ARROW = 62,
        HIGHLIGHTER_DRAW = 63,
        ERASER = 9,
        LOCK = 99,
        NULL = -1,
    }

    public partial class MainWindow : Window
    {
        private KeyboardHookListener keyboardHookListener;

        public static MainWindow _instance = new MainWindow();
        new bool IsLoaded = false;

        private MainWindow()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            SetMouseFlg();
        }

        public static MainWindow GetInstance()
        {
            return _instance;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainCanvas.Children.Clear();
            curColor = Rainbow(0);
            Task.Run(() =>
            {
                while (true)
                {
                    if (Common.rainbowFlg && Common.IsRainbowColor)
                    {
                        NextRainbow();
                    }
                    Thread.Sleep(Common.colorChangeTime);
                }
            });
            Task.Run(() =>
            {
                List<string> removeList = new List<string>();
                while (true)
                {
                    try
                    {
                        if (App.Setting.RemoveFlg)
                        {
                            removeList.Clear();
                            foreach (var item in Common.CanvasDic.Values)
                            {
                                // 追加されてなかったものは20秒後に追加する
                                if (item.RemainStartFlg == false)
                                {
                                    item.NotStartMs += 250;
                                    if (item.NotStartMs > 20000)
                                    {
                                        item.RemainStartFlg = true;
                                    }
                                    continue;
                                }

                                if (item.RemainMs > 0)
                                {
                                    item.RemainMs -= 250;
                                }
                                if (item.RemainMs <= 0)
                                {
                                    string id = item.ID;
                                    Common.Invoke(() =>
                                    {
                                        try
                                        {
                                            var can = (Canvas)mainCanvas.FindName(id);
                                            if (item.FadeStartFlg && can.Opacity == 0)
                                            {
                                                if (mainCanvas.Children.IndexOf(can) != -1)
                                                {
                                                    mainCanvas.Children.Remove(can);
                                                }
                                                removeList.Add(id);
                                            }
                                            else if (!item.FadeStartFlg)
                                            {
                                                item.FadeStartFlg = true;
                                                can.BeginAnimation(OpacityProperty, AnimationManager.DoubleAni(can.Opacity, 0, 2));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex.Message);
                                        }
                                    });
                                }
                            }
                            foreach (var id in removeList)
                            {
                                Common.CanvasDic.TryRemove(id, out _);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    Thread.Sleep(250);
                }
            });
            handle = new WindowInteropHelper(this).Handle;
            //_hook = new KeyboardHook();
            //_hook.KeyDown += new KeyboardHook.HookEventHandler(OnHookKeyDown);
            //_hook.KeyUp += new KeyboardHook.HookEventHandler(OnHookKeyUp);

            keyboardHookListener = new KeyboardHookListener(new GlobalHooker());
            keyboardHookListener.Enabled = true;
            keyboardHookListener.KeyDown += OnHookKeyDown;
            keyboardHookListener.KeyUp += OnHookKeyUp;


            style = GetWindowLong(handle, GWL_EXSTYLE);

            Common.curLineSize = App.Setting.Size;
            Common.colorChangeTime = App.Setting.ColorChangeTime;
            SetSize(Common.curLineSize);
            IsLoaded = true;
            SetDrawMode(DRAW_MODE.DRAW);
            // Lock
            Common.IsOn = false;
            SetMouseFlg();

            ToolBar.GetInstance().Show();
            new MouseHook();
        }

        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLoaded && e.ChangedButton == MouseButton.Left && !textMoveFlg)
            {
                if (RightMenu.GetInstance().IsVisible)
                {
                    RightMenu.Fold();
                    return;
                }
                IInputElement focusedControl = System.Windows.Input.Keyboard.FocusedElement;
                if (focusedControl != null && focusedControl.GetType().Name == "TextBox")
                {
                    System.Windows.Input.Keyboard.ClearFocus();
                    ToolBar.SetTopMost();
                    return;
                }
                StartDraw(Common.nowMode);
            }
            if (IsLoaded && e.ChangedButton == MouseButton.Right && !textMoveFlg)
            {
                var position = Mouse.GetPosition(this);
                RightMenu.Open(position);
            }
        }

        public void StartDraw(DRAW_MODE mode)
        {
            try
            {
                Keyboard.ClearFocus();
                nowCanvas = new Canvas();
                string id = "Can_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                nowCanvas.Tag = id;
                mainCanvas.RegisterName(id, nowCanvas);
                mainCanvas.Children.Add(nowCanvas);
                nowCanvas.MouseDown += NowCanvas_MouseDown;

                if (Common.nowMode != DRAW_MODE.TEXT)
                {
                    ItemData item = new ItemData()
                    {
                        ID = id,
                        RemainMs = App.Setting.RemainMs,
                        RemainStartFlg = false
                    };
                    Common.CanvasDic.TryAdd(id, item);
                }

                Common.CanvasIdStack.Push(id);
                startPosition = Mouse.GetPosition(this);
                startX = startPosition.X;
                startY = startPosition.Y;
                EllipseMinLeft = 2000;
                EllipseMinTop = 2000;

                switch (Common.nowMode)
                {
                    case DRAW_MODE.DRAW:
                    case DRAW_MODE.HIGHLIGHTER_DRAW:
                        Common.rainbowFlg = true;
                        elliNum = 1;
                        elliList = new Dictionary<string, List<Point>>();
                        InsertElli(startPosition, curColor.Clone());
                        break;
                    case DRAW_MODE.RECTANGLE:
                    case DRAW_MODE.DRAW_RECTANGLE:
                    case DRAW_MODE.RECTANGLE_FULLFILL:
                    case DRAW_MODE.RECTANGLE_ROUND:
                    case DRAW_MODE.RECTANGLE_ROUND_FULLFILL:
                        CreateRect();
                        break;
                    case DRAW_MODE.HIGHLIGHTER_RECTANGLE:
                        nowCanvas.Opacity = 0.23;
                        CreateRect();
                        break;
                    case DRAW_MODE.LINE:
                    case DRAW_MODE.DRAW_LINE:
                        CreateLine();
                        break;
                    case DRAW_MODE.HIGHLIGHTER_LINE:
                        nowCanvas.Opacity = 0.23;
                        CreateLine();
                        break;
                    case DRAW_MODE.ARROW:
                    case DRAW_MODE.DRAW_ARROW:
                        CreateArrow();
                        break;
                    case DRAW_MODE.HIGHLIGHTER_ARROW:
                        nowCanvas.Opacity = 0.23;
                        CreateArrow();
                        break;
                    case DRAW_MODE.TEXT:
                        CreateTextbox();
                        break;
                    case DRAW_MODE.HIGHLIGHTER:
                    case DRAW_MODE.DRAW_HIGHLIGHTER:
                        nowCanvas.Opacity = 0.23;
                        Common.rainbowFlg = true;
                        InsertElli(startPosition, curColor.Clone());
                        break;
                    case DRAW_MODE.ERASER:
                        break;
                    default:
                        break;
                }
                Common.drawFlg = true;
            }
            catch (Exception)
            {
            }
        }

        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //EmphasizeWindow.Move();
            if (IsLoaded && Common.drawFlg && e.LeftButton == MouseButtonState.Pressed && !textMoveFlg)
            {
                var position = Mouse.GetPosition(this);
                Point curPoint = new Point(position.X, position.Y);

                switch (Common.nowMode)
                {
                    case DRAW_MODE.DRAW:
                    case DRAW_MODE.HIGHLIGHTER_DRAW:
                        ++elliNum;
                        InsertElli(curPoint, curColor.Clone());
                        break;
                    case DRAW_MODE.RECTANGLE:
                    case DRAW_MODE.DRAW_RECTANGLE:
                    case DRAW_MODE.HIGHLIGHTER_RECTANGLE:
                    case DRAW_MODE.RECTANGLE_FULLFILL:
                    case DRAW_MODE.RECTANGLE_ROUND:
                    case DRAW_MODE.RECTANGLE_ROUND_FULLFILL:
                        RePaintRect(position);
                        break;
                    case DRAW_MODE.LINE:
                    case DRAW_MODE.DRAW_LINE:
                    case DRAW_MODE.HIGHLIGHTER_LINE:
                        RePaintLine(position);
                        break;
                    case DRAW_MODE.ARROW:
                    case DRAW_MODE.DRAW_ARROW:
                    case DRAW_MODE.HIGHLIGHTER_ARROW:
                        RePaintArrow(position);
                        break;
                    case DRAW_MODE.HIGHLIGHTER:
                    case DRAW_MODE.DRAW_HIGHLIGHTER:
                        InsertElli(curPoint, curColor.Clone());
                        break;
                    case DRAW_MODE.ERASER:
                        break;
                    default:
                        break;
                }

            }
        }

        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLoaded && Common.drawFlg && e.ChangedButton == MouseButton.Left && !textMoveFlg)
            {
                switch (Common.nowMode)
                {
                    case DRAW_MODE.DRAW:
                    case DRAW_MODE.HIGHLIGHTER_DRAW:
                        Common.rainbowFlg = false;
                        //Image img = new Image();
                        //Image img3 = new Image();
                        //img.Source = RenderVisualService.RenderToPNGImageSource(nowCanvas);
                        //img3.Source = RenderVisualService.RenderToPNGImageSource(nowCanvas);
                        //Canvas.SetLeft(img, EllipseMinLeft);
                        //Canvas.SetTop(img, EllipseMinTop);
                        //Canvas.SetLeft(img3, EllipseMinLeft);
                        //Canvas.SetTop(img3, EllipseMinTop);
                        //nowCanvas.Children.Clear();
                        //nowCanvas.Children.Add(img);
                        //nowCanvas.Children.Add(img3);
                        //nowCanvas.BeginAnimation(OpacityProperty, AnimationManager.DoubleAni(1, 0, 2));
                        break;
                    case DRAW_MODE.RECTANGLE:
                    case DRAW_MODE.DRAW_RECTANGLE:
                    case DRAW_MODE.HIGHLIGHTER_RECTANGLE:
                    case DRAW_MODE.RECTANGLE_FULLFILL:
                    case DRAW_MODE.RECTANGLE_ROUND:
                    case DRAW_MODE.RECTANGLE_ROUND_FULLFILL:
                        if (rectBd.Tag != null)
                        {
                            NextRainbow(float.Parse(rectBd.Tag.ToString()));
                        }
                        rectBd = null;
                        break;
                    case DRAW_MODE.LINE:
                    case DRAW_MODE.DRAW_LINE:
                    case DRAW_MODE.HIGHLIGHTER_LINE:
                        if (line.Tag != null)
                        {
                            NextRainbow(float.Parse(line.Tag.ToString()));
                        }
                        line = null;
                        break;
                    case DRAW_MODE.ARROW:
                    case DRAW_MODE.DRAW_ARROW:
                    case DRAW_MODE.HIGHLIGHTER_ARROW:
                        if (line.Tag != null)
                        {
                            NextRainbow(float.Parse(line.Tag.ToString()));
                        }
                        line = null;
                        break;
                    case DRAW_MODE.HIGHLIGHTER:
                    case DRAW_MODE.DRAW_HIGHLIGHTER:
                        Common.rainbowFlg = false;
                        Image img2 = new Image();
                        img2.Source = RenderVisualService.RenderToPNGImageSource(nowCanvas);
                        Canvas.SetLeft(img2, EllipseMinLeft);
                        Canvas.SetTop(img2, EllipseMinTop);
                        nowCanvas.Children.Clear();
                        nowCanvas.Children.Add(img2);
                        nowCanvas.Opacity = 1;
                        break;
                    case DRAW_MODE.ERASER:
                        break;
                    default:
                        break;
                }
                if (Common.nowMode != DRAW_MODE.TEXT)
                {
                    if (Common.CanvasDic.ContainsKey(nowCanvas.Tag.ToString()))
                    {
                        Common.CanvasDic[nowCanvas.Tag.ToString()].RemainStartFlg = true;
                    }
                    //ItemData item = new ItemData()
                    //{
                    //    ID = nowCanvas.Tag.ToString(),
                    //    RemainMs = App.Setting.RemainMs
                    //};
                    //Common.CanvasDic.TryAdd(nowCanvas.Tag.ToString(), item);
                }
                Common.drawFlg = false;
                mainCanvas.InvalidateVisual();
                ToolBar.SetTopMost();

            }
        }

        private void NowCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas cv = (Canvas)sender;
            Debug.WriteLine(cv.Tag);
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void ToolbarOpenBt_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.GetInstance().Open();
        }

        private void HelpBt_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(App.Setting.HelpUrl);

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (Common.nowMode == DRAW_MODE.DRAW)
                {
                    SetDrawMode(DRAW_MODE.DRAW_HIGHLIGHTER);
                    return;
                }
            }


        }

        private void mainCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (Common.nowMode == DRAW_MODE.DRAW)
                {
                    SetDrawMode(DRAW_MODE.DRAW_HIGHLIGHTER);
                    return;
                }
            }

            //if (Keyboard.Modifiers == ModifierKeys.Shift)
            //{
            //    if (Common.nowMode == DRAW_MODE.LINE)
            //    {
            //        Debug.WriteLine("mainCanvas_KeyDown 45");
            //        MainWindow.SetLineRad(45);
            //        return;
            //    }
            //}

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //if (Keyboard.Modifiers == ModifierKeys.Alt)
            //{
            //    if (prevMode == DRAW_MODE.DRAW && nowMode == DRAW_MODE.DRAW_HIGHLIGHTER)
            //    {
            //        SetDrawMode(prevMode);
            //        return;
            //    }
            //}
        }

        private void mainCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if ((Common.prevMode == DRAW_MODE.DRAW && Common.nowMode == DRAW_MODE.DRAW_HIGHLIGHTER)
                    || (Common.prevMode == DRAW_MODE.HIGHLIGHTER && Common.nowMode == DRAW_MODE.HIGHLIGHTER_DRAW))
                {
                    SetDrawMode(Common.prevMode);
                    return;
                }
            }
        }

        public void InsertTool(string name)
        {
            try
            {
                if (name == "Timer")
                {

                }
                else
                {
                    Keyboard.ClearFocus();
                    nowCanvas = new Canvas();
                    string id = "Can_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                    nowCanvas.Tag = id;
                    mainCanvas.RegisterName(id, nowCanvas);
                    mainCanvas.Children.Add(nowCanvas);
                    nowCanvas.MouseDown += NowCanvas_MouseDown;
                    //Common.CanvasDic.Add(id, nowCanvas);
                    Common.CanvasIdStack.Push(id);
                    CreateTools(name);
                }
            }
            catch (Exception)
            {
            }
        }



    }
}
