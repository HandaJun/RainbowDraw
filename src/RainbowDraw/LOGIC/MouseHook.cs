using RainbowDraw.VIEW;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;


namespace RainbowDraw.LOGIC
{
    public class MouseHook : DependencyObject
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref NativePoint pt);

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativePoint
        {
            public int X;
            public int Y;
        };

        public enum VirtualKey : UInt16
        {
            VK_LBUTTON = 0x01,
            VK_RBUTTON = 0x02,
        }

        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public static Point GetCurrentMousePosition()
        {
            NativePoint nativePoint = new NativePoint();
            GetCursorPos(ref nativePoint);
            return new Point(nativePoint.X, nativePoint.Y);
        }

        private Dispatcher dispatcher;

        public static Timer timer = new Timer(500);
        public static Timer EmphasizeMoveTimer = new Timer(10);

        public MouseHook()
        {
            dispatcher = Application.Current.MainWindow.Dispatcher;
            //timer.Elapsed += Timer_Elapsed;
            EmphasizeMoveTimer.Elapsed += EmphasizeMoveTimer_Elapsed;
            //EmphasizeMoveTimer.Start();
            //timer.Start();
        }

        void EmphasizeMoveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Point current = GetCurrentMousePosition();
            Common.Invoke(() => { 
                var w = EmphasizeWindow.GetInstance();
                if (w.IsVisible)
                {
                    w.Left = current.X - (w.Width / 2);
                    w.Top = current.Y - (w.Height / 2);
                }
            });
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Point current = GetCurrentMousePosition();

            //if (current.X > stickyWindowMaxWidth)
            //{
            //    try
            //    {
            //        Common.Invoke(() => {
            //            if (StickyWindow.GetInstance().getWidth() != 0)
            //            {
            //                if (GetAsyncKeyState((UInt16)VirtualKey.VK_LBUTTON) == 0)
            //                {
            //                    StickyWindow.GetInstance().Fold();
            //                }
            //            }
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //}
            //else if (current.X <= stickyWindowMaxWidth && current.X > 3 && current.Y > 300)
            //{
            //    try
            //    {
            //        Common.Invoke(() => {
            //            if (StickyWindow.GetInstance().getWidth() != 0)
            //            {
            //                if (StickyManager.GetMaxHeight() < current.Y)
            //                {
            //                    if (GetAsyncKeyState((UInt16)VirtualKey.VK_LBUTTON) == 0)
            //                    {
            //                        StickyWindow.GetInstance().Fold();
            //                    }
            //                }
            //            }
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //}
            //else if (current.X <= stickyWindowMaxWidth && current.X > 3 && current.Y < 300)
            //{

            //}
            //else if (current.X < 0)
            //{
            //    try
            //    {
            //        Common.Invoke(() => {
            //            if (StickyWindow.GetInstance().getWidth() != 0)
            //            {
            //                if (GetAsyncKeyState((UInt16)VirtualKey.VK_LBUTTON) == 0)
            //                {
            //                    StickyWindow.GetInstance().Fold();
            //                }
            //            }
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //}
            //this.CurrentPosition = current;
        }

        public Point CurrentPosition
        {
            get { return (Point)GetValue(CurrentPositionProperty); }

            set
            {
                dispatcher.Invoke((Action)(() =>
                  SetValue(CurrentPositionProperty, value)));
            }
        }

        public static readonly DependencyProperty CurrentPositionProperty
          = DependencyProperty.Register(
            "CurrentPosition", typeof(Point), typeof(MouseHook));
    }
}
