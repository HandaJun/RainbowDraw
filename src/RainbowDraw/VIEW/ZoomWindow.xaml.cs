using RainbowDraw.LOGIC;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace RainbowDraw.VIEW
{
    /// <summary>
    /// Interaction logic for ZoomWindow.xaml
    /// </summary>
    public partial class ZoomWindow : Window
    {
        private IntPtr MainHwnd;
        private IntPtr hwndMag;
        private float _Magnification;
        private new bool Initialized;
        private readonly int screenIndex = 1;
        private Rect screenRect;
        private RECT magWindowRect;
        private RECT captureRect;
        private System.Drawing.Size MyFrmSize;
        private int CaptureWidth;
        private int CaptureHeight;
        private int MarginW;
        private int MarginH;
        private bool IsZoomStop;
        private bool IsProcessing;
        private const int GWL_EXSTYLE = -20;

        public ZoomWindow()
        {
            Loaded += new RoutedEventHandler(ZoomWindow_Loaded);
            SizeChanged += new SizeChangedEventHandler(ZoomWindow_SizeChanged);
            Closing += new CancelEventHandler(ZoomWindow_Closing);
            hwndMag = IntPtr.Zero;
            magWindowRect = new RECT();
            captureRect = new RECT(0, 0);
            ScrRatio = 0.15;
            IsZoomStop = false;
            IsProcessing = false;
            MinMagify = 1.2;
            MaxMagify = 3.0;
            InitializeComponent();
        }

        private void ZoomWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int screenIndex = this.screenIndex;
                InitMagify = App.Setting.Magify;
                screenRect = new Rect(new System.Windows.Point((double)Screen.AllScreens[screenIndex].Bounds.Left, (double)Screen.AllScreens[screenIndex].Bounds.Top), new System.Windows.Size((double)Screen.AllScreens[screenIndex].Bounds.Width, (double)Screen.AllScreens[screenIndex].Bounds.Height));
                BeginMagify();
                WindowState = WindowState.Maximized;
            }
            catch { }
        }

        public void Init()
        {

        }

        public double MinMagify { get; set; }

        public double MaxMagify { get; set; }

        public double InitMagify { get; set; }

        public float MagnificationVal
        {
            get => _Magnification;
            set
            {
                try
                {
                    if ((double)value < MinMagify)
                        value = (float)MinMagify;
                    else if ((double)value > MaxMagify)
                        value = (float)MaxMagify;
                    if ((double)_Magnification == (double)value)
                        return;
                    _Magnification = value;
                    App.Setting.Magify = value;
                    App.Setting.Save();
                    Transformation pTransform = new Transformation(_Magnification);
                    NativeMethods.MagSetWindowTransform(hwndMag, ref pTransform);
                    SetInitSize();
                }
                catch { }
            }
        }

        public double ScrRatio { get; }

        private void BeginMagify()
        {
            try
            {
                MainHwnd = new WindowInteropHelper(this).Handle;
                MagnificationVal = (float)InitMagify;
                Initialized = NativeMethods.MagInitialize();
                if (!Initialized)
                    return;
                SetupMagnifier();
                NativeMethods.SetWindowLong(MainHwnd, GWL_EXSTYLE, (int)NativeMethods.GetWindowLong(MainHwnd, GWL_EXSTYLE) | 32);
                NativeMethods.SetWindowPos(MainHwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);
                Task.Run(() =>
                {
                    try
                    {
                        IsProcessing = true;
                        IsZoomStop = false;
                        while (!IsZoomStop)
                        {
                            captureRect = UpdateMaginifier();
                            if (NativeMethods.MagSetWindowSource(hwndMag, captureRect))
                            {
                                //NativeMethods.SetWindowPos(MainHwnd, HWND_TOPMOST, 0, 0, 0, 0, 19);
                                NativeMethods.SetWindowPos(MainHwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);
                                //NativeMethods.InvalidateRect(hwndMag, IntPtr.Zero, false);
                                NativeMethods.InvalidateRect(hwndMag, IntPtr.Zero, true);
                            }
                            Thread.Sleep(NativeMethods.USER_TIMER_MINIMUM);
                        }
                        IsProcessing = false;
                    }
                    catch
                    {
                        IsProcessing = false;
                    }
                });
            }
            catch { }
        }

        private void SetupMagnifier()
        {
            if (!Initialized)
            {
                return;
            }
            IntPtr hInst;
            hInst = NativeMethods.GetModuleHandle(null);
            hwndMag = NativeMethods.CreateWindow(512, NativeMethods.WC_MAGNIFIER, "ZoomWindow",
                (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_VISIBLE | (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR,
                magWindowRect.left, magWindowRect.top, magWindowRect.right, magWindowRect.bottom, MainHwnd,
                IntPtr.Zero, hInst, IntPtr.Zero);
            if (hwndMag == IntPtr.Zero)
                return;
            Transformation matrix = new Transformation(MagnificationVal);
            NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
        }

        private void SetInitSize()
        {
            try
            {
                NativeMethods.GetClientRect(MainHwnd, ref magWindowRect);
                MyFrmSize = new System.Drawing.Size(checked(magWindowRect.right - magWindowRect.left), checked(magWindowRect.bottom - magWindowRect.top));
                CaptureWidth = checked((int)Math.Round(unchecked((double)MyFrmSize.Width / (double)MagnificationVal)));
                CaptureHeight = checked((int)Math.Round(unchecked((double)MyFrmSize.Height / (double)MagnificationVal)));
                MarginW = checked((int)Math.Round(unchecked((double)CaptureWidth * ScrRatio / (double)MagnificationVal)));
                MarginH = checked((int)Math.Round(unchecked((double)CaptureHeight * ScrRatio / (double)MagnificationVal)));
            }
            catch { }
        }

        private void ResizeMagnifier()
        {
            if (!Initialized || !(hwndMag != IntPtr.Zero))
                return;
            SetInitSize();
            NativeMethods.SetWindowPos(hwndMag, IntPtr.Zero, magWindowRect.left, magWindowRect.top, magWindowRect.right, magWindowRect.bottom, 0);
        }

        private RECT UpdateMaginifier()
        {
            RECT rect1;
            if (!Initialized || hwndMag == IntPtr.Zero)
            {
                rect1 = new RECT(0, 0);
            }
            else
            {
                POINT pt = new POINT();
                NativeMethods.GetCursorPos(ref pt);
                POINT rect3 = pt;
                NativeMethods.ScreenToClient(MainHwnd, ref rect3);
                RECT captureRect = this.captureRect;

                if ((double)pt.x < screenRect.X || (double)pt.x > screenRect.X + screenRect.Width || ((double)pt.y < screenRect.Y || (double)pt.y > screenRect.Y + screenRect.Height))
                {
                    NativeMethods.MagShowSystemCursor(true);
                }
                else
                {
                    NativeMethods.MagShowSystemCursor(false);
                }

                if (captureRect.left == 0 && captureRect.right == 0)
                {
                    captureRect.left = checked((int)Math.Round(unchecked((double)pt.x - (double)CaptureWidth / 2.0)));
                    captureRect.top = checked((int)Math.Round(unchecked((double)pt.y - (double)CaptureHeight / 2.0)));
                }
                else
                {
                    if (pt.x < checked(captureRect.left + MarginW))
                    {
                        captureRect.left = checked(pt.x - MarginW);
                    }
                    else if (checked(pt.x + MarginW) > captureRect.right)
                    {
                        captureRect.left = checked(captureRect.left + pt.x - captureRect.right + MarginW);

                        //int&local;
                        //int num = checked(^(local = ref captureRect.left) + pt.x - captureRect.right + MarginW);
                        //local = num;
                    }

                    if (pt.y < checked(captureRect.top + MarginH))
                    {
                        captureRect.top = checked(pt.y - MarginH);
                    }
                    else if (checked(pt.y + MarginH) > captureRect.bottom)
                    {
                        captureRect.top = checked(captureRect.top + pt.y - captureRect.bottom + MarginH);
                        //int&local;
                        //int num = checked(^(local = ref captureRect.top) + pt.y - captureRect.bottom + MarginH);
                        //local = num;
                    }
                }
                if ((double)captureRect.left < screenRect.X)
                {
                    captureRect.left = checked((int)Math.Round(screenRect.X));

                }
                else if ((double)checked(captureRect.left + CaptureWidth) > screenRect.X + screenRect.Width)
                {
                    captureRect.left = checked((int)Math.Round(unchecked(screenRect.X + screenRect.Width - (double)CaptureWidth)));
                }

                captureRect.right = checked(captureRect.left + CaptureWidth);

                if ((double)captureRect.top < screenRect.Y)
                {
                    captureRect.top = checked((int)Math.Round(screenRect.Y));
                }
                else if ((double)checked(captureRect.top + CaptureHeight) > screenRect.Y + screenRect.Height)
                {
                    captureRect.top = checked((int)Math.Round(unchecked(screenRect.Y + screenRect.Height - (double)CaptureHeight)));
                }
                captureRect.bottom = checked(captureRect.top + CaptureHeight);
                rect1 = captureRect;
            }
            return rect1;
        }

        public void Magnify() => MagnificationVal += 0.2f;

        public void Reduce() => MagnificationVal -= 0.2f;

        public void Exit()
        {
            try
            {
                IsZoomStop = true;
                NativeMethods.MagShowSystemCursor(true);
                if (Initialized)
                    NativeMethods.MagUninitialize();
                while (IsProcessing)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(100);
                }
            }
            catch { }
            Close();
        }

        private void ZoomWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                ResizeMagnifier();
            }
            catch { }
        }

        private void ZoomWindow_Closing(object sender, CancelEventArgs e)
        {
            NativeMethods.SetWindowLong(MainHwnd, GWL_EXSTYLE, (int)NativeMethods.GetWindowLong(MainHwnd, GWL_EXSTYLE) & -33);
        }

    }
}
