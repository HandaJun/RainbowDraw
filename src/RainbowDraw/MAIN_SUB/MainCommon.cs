using Microsoft.Win32.SafeHandles;
using RainbowDraw.LOGIC;
using RainbowDraw.VIEW;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        public static Brush curColor = Brushes.White;
        IntPtr handle = new IntPtr(0);
        Canvas nowCanvas = null;
        Border rectBd = null;
        Line line = null;
        Polygon poly = null;
        TextBox textBox = null;
        Point startPosition;
        double startX = 0;
        double startY = 0;
        int style = 0;

        public void SetDrawMode(DRAW_MODE mode)
        {
            Common.prevMode = Common.nowMode;
            Common.nowMode = mode;
            switch (mode)
            {
                case DRAW_MODE.DRAW:
                case DRAW_MODE.HIGHLIGHTER_DRAW:
                    SetCursor("Draw");
                    break;
                case DRAW_MODE.RECTANGLE:
                case DRAW_MODE.DRAW_RECTANGLE:
                case DRAW_MODE.HIGHLIGHTER_RECTANGLE:
                case DRAW_MODE.RECTANGLE_FULLFILL:
                case DRAW_MODE.RECTANGLE_ROUND:
                case DRAW_MODE.RECTANGLE_ROUND_FULLFILL:
                    SetCursor("Rectangle");
                    break;
                case DRAW_MODE.LINE:
                case DRAW_MODE.DRAW_LINE:
                case DRAW_MODE.HIGHLIGHTER_LINE:
                    SetCursor("Line");
                    break;
                case DRAW_MODE.ARROW:
                case DRAW_MODE.DRAW_ARROW:
                case DRAW_MODE.HIGHLIGHTER_ARROW:
                    SetCursor("Arrow");
                    break;
                case DRAW_MODE.TEXT:
                    SetCursor("Text");
                    break;
                case DRAW_MODE.HIGHLIGHTER:
                case DRAW_MODE.DRAW_HIGHLIGHTER:
                    SetCursor("Highlighter");
                    break;
                default:
                    this.Cursor = Cursors.Pen;
                    break;
            }
            Common.rainbowFlg = false;
            Common.drawFlg = false;
            mainCanvas.InvalidateVisual();

            ToolBar.GetInstance().SetMode(mode);
            RightMenu.GetInstance().SetMode(mode);
            //NullTb.Focus();
            mainCanvas.Focus();
            ToolBar.SetTopMost();
            //if (Common.IsEmphasize)
            //{
            //    EmphasizeWindow.Open();
            //}
        }

        public void SetCursor(string mode)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\IMG\" + mode + ".png");
            _ = System.Drawing.Graphics.FromImage(bitmap);
            IntPtr handle = bitmap.GetHicon();
            SafeFileHandle panHandle = new SafeFileHandle(handle, false);
            this.Cursor = CursorInteropHelper.Create(panHandle);
        }

        public void SetMouseFlg()
        {
            if (Common.IsOn)
            {
                SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_ACCEPTFILES);
                this.Topmost = true;
                this.Focus();
                ToolBar.GetInstance().SetMode(Common.nowMode);
                RightMenu.GetInstance().SetMode(Common.nowMode);
            }
            else
            {
                ToolBar.GetInstance().SetMode(DRAW_MODE.LOCK, DRAW_MODE.NULL);
                RightMenu.GetInstance().SetMode(DRAW_MODE.LOCK, DRAW_MODE.NULL);
                RightMenu.Fold();
                //EmphasizeWindow.Fold(Common.IsEmphasize);
                SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            }
        }

        public Brush NextRainbow(float ascending = 0.001f)
        {
            if (!Common.IsRainbowColor)
            {
                return curColor;
            }
            Brush rtn = Brushes.Transparent;
            Common.rainbowProgress += ascending;
            if (Common.rainbowProgress > 1f)
            {
                Common.rainbowProgress -= 1f;
            }
            this.Dispatcher.Invoke(() =>
            {
                rtn = Rainbow(Common.rainbowProgress);
                curColor = rtn;
            });
            return rtn;
        }

        public Brush Rainbow(float progress)
        {
            if (!Common.IsRainbowColor)
            {
                return curColor;
            }

            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;
            Color c = Colors.White;
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
#if DEBUG
            //Debug.WriteLine(c.R.ToString() + " " + c.G.ToString() + " " + c.B.ToString());
            //Debug.WriteLine(GetProgress(new SolidColorBrush(c)));
#endif
            return new SolidColorBrush(c);
        }

        public float GetProgress(Brush br)
        {
            float div = 0f;
            byte r = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).R;
            byte g = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).G;
            byte b = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).B;
            float ascending;
            if (r == 255 && b == 0)
            {
                div += 0;
                ascending = (float)g / 255f;
                div += ascending;
            }
            else if (g == 255 && b == 0)
            {
                div += 1;
                ascending = (float)(255 - r) / 255f;
                div += ascending;
            }
            else if (r == 0 && g == 255)
            {
                div += 2;
                ascending = (float)b / 255f;
                div += ascending;
            }
            else if (r == 0 && b == 255)
            {
                div += 3;
                ascending = (float)(255 - g) / 255f;
                div += ascending;
            }
            else if (g == 0 && b == 255)
            {
                div += 4;
                ascending = (float)r / 255f;
                div += ascending;
            }
            else if (r == 255 && g == 0)
            {
                div += 5;
                ascending = (float)(255 - b) / 255f;
                div += ascending;
            }
            else
            {
                div += 6;
                ascending = (float)(255 - b) / 255f;
                div += ascending;
            }

            return div / 6f;
        }

        public void SizeUp()
        {
            if (Common.curLineSize < Common.MaxLineSize)
            {
                Common.curLineSize += 1;
                ToolBar.GetInstance().SizeTb.Text = Common.curLineSize.ToString();
                RightMenu.GetInstance().SizeTb.Text = Common.curLineSize.ToString();
                App.Setting.Size = Common.curLineSize;
                App.Setting.Save();
            }
        }

        public void SizeDown()
        {
            if (Common.curLineSize > Common.MinLineSize)
            {
                Common.curLineSize -= 1;
                ToolBar.GetInstance().SizeTb.Text = Common.curLineSize.ToString();
                RightMenu.GetInstance().SizeTb.Text = Common.curLineSize.ToString();
                App.Setting.Size = Common.curLineSize;
                App.Setting.Save();
            }
        }

        public void SetSize(int size)
        {
            Common.curLineSize = size;
            ToolBar.GetInstance().SizeTb.Text = Common.curLineSize.ToString();
            RightMenu.GetInstance().SizeTb.Text = Common.curLineSize.ToString();
            App.Setting.Size = Common.curLineSize;
            App.Setting.Save();
        }

        public void RemoveLast()
        {
            while (true)
            {
                if (Common.CanvasIdStack.Count > 0)
                {
                    var item = (System.Windows.Controls.Canvas)mainCanvas.FindName(Common.CanvasIdStack.Pop());
                    if (mainCanvas.Children.IndexOf(item) != -1)
                    {
                        mainCanvas.Children.Remove(item);
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public static ZoomWindow zw = new ZoomWindow();
        private static readonly IntPtr intPtr = new IntPtr(0);
        private const int SW_MINIMIZE = 6;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void MinimizeWindow(IntPtr hwnd)
        {
            //IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, "NotePad");
            ShowWindow(hwnd, SW_MINIMIZE);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetBackgroundWindow(IntPtr hWnd);

        public void SetZoom()
        {
            if (Common.IsZoom)
            {
                zw = new ZoomWindow();
                zw.Show();
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    zw.Exit();
                });
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_ACCEPTFILES = 0x10;

        public IntPtr ToolBarHandle { get; } = intPtr;
    }
}
