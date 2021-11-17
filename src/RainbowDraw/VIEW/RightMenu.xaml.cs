using RainbowDraw.LOGIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RainbowDraw.VIEW
{
    /// <summary>
    /// Interaction logic for RightMenu.xaml
    /// </summary>
    public partial class RightMenu : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        public static RightMenu _instance = new RightMenu();

        private RightMenu()
        {
            InitializeComponent();
        }

        public static void Open(Point p)
        {
            _instance.Left = p.X;
            _instance.Top = p.Y;
            if (!_instance.IsVisible)
            {
                _instance.Show();
            }
            _instance.Topmost = false;
            _instance.Topmost = true;
        }

        public static void Fold()
        {
            if (_instance.IsVisible)
            {
                _instance.Hide();
                ToolBar.SetTopMost();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SizeTb.Text = Common.curLineSize.ToString();
            ShowInTaskbar = false;
            //var windowHelper = new WindowInteropHelper(this);
            //SetWindowRgn(windowHelper.Handle, CreateRoundRectRgn(0, 0, 100, 252, 15, 15), false);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EnableBlur(this);
        }

        public static RightMenu GetInstance()
        {
            _instance.Topmost = false;
            _instance.Topmost = true;
            return _instance;
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        //[DllImport("user32.dll")]
        //static extern Int32 SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        //[DllImport("gdi32.dll")]
        //static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        internal static void EnableBlur(Window win)
        {
            var windowHelper = new WindowInteropHelper(win);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.AccentFlags = 2;
            //↓の色はAABBGGRRの順番で設定する
            accent.GradientColor = 0x00000000;  // 60%の透明度が基本

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);

        }

        //SolidColorBrush SelectBrush = new SolidColorBrush(Color.FromArgb(204, 0, 191, 139));

        LinearGradientBrush SelectBrush = new LinearGradientBrush(Color.FromRgb(255, 93, 0), Color.FromRgb(93, 255, 0), new Point(0, 0), new Point(1, 1));
        LinearGradientBrush EnterBrush = new LinearGradientBrush(Color.FromArgb(255, 160, 60, 0), Color.FromArgb(255, 60, 160, 0), new Point(0, 0), new Point(1, 1));
        SolidColorBrush UnSelectBrush = new SolidColorBrush(Color.FromArgb(204, 51, 51, 51));
        SolidColorBrush UnSelectBrush2 = new SolidColorBrush(Color.FromArgb(1, 51, 51, 51));
        public void RemoveMode(DRAW_MODE mode = DRAW_MODE.NULL)
        {
            switch (mode)
            {
                case DRAW_MODE.DRAW:
                    DrawBt.Background = UnSelectBrush;
                    DrawBt.Tag = "";
                    break;
                case DRAW_MODE.RECTANGLE:
                    RectangleBt.Background = UnSelectBrush;
                    RectangleBt.Tag = "";
                    break;
                case DRAW_MODE.LINE:
                    LineBt.Background = UnSelectBrush;
                    LineBt.Tag = "";
                    break;
                case DRAW_MODE.ARROW:
                    ArrowBt.Background = UnSelectBrush;
                    ArrowBt.Tag = "";
                    break;
                case DRAW_MODE.TEXT:
                    TextBt.Background = UnSelectBrush;
                    TextBt.Tag = "";
                    break;
                case DRAW_MODE.HIGHLIGHTER:
                    HighlighterBt.Background = UnSelectBrush;
                    HighlighterBt.Tag = "";
                    break;
                case DRAW_MODE.ERASER:
                    break;
                case DRAW_MODE.NULL:
                    DrawBt.Background = UnSelectBrush;
                    RectangleBt.Background = UnSelectBrush;
                    LineBt.Background = UnSelectBrush;
                    ArrowBt.Background = UnSelectBrush;
                    TextBt.Background = UnSelectBrush;
                    HighlighterBt.Background = UnSelectBrush;
                    LockBt.Background = UnSelectBrush;
                    DrawBt.Tag = "";
                    RectangleBt.Tag = "";
                    LineBt.Tag = "";
                    ArrowBt.Tag = "";
                    TextBt.Tag = "";
                    HighlighterBt.Tag = "";
                    LockBt.Tag = "";
                    break;
                default:
                    break;
            }
        }

        public void SetMode(DRAW_MODE mode, DRAW_MODE prev = DRAW_MODE.NULL)
        {
            RemoveMode(prev);
            switch (mode)
            {
                case DRAW_MODE.DRAW:
                case DRAW_MODE.DRAW_ARROW:
                case DRAW_MODE.DRAW_LINE:
                case DRAW_MODE.DRAW_RECTANGLE:
                case DRAW_MODE.DRAW_HIGHLIGHTER:
                    DrawBt.Background = SelectBrush;
                    DrawBt.Tag = "Selected";
                    break;
                case DRAW_MODE.RECTANGLE:
                case DRAW_MODE.RECTANGLE_FULLFILL:
                case DRAW_MODE.RECTANGLE_ROUND:
                    RectangleBt.Background = SelectBrush;
                    RectangleBt.Tag = "Selected";
                    break;
                case DRAW_MODE.LINE:
                    LineBt.Background = SelectBrush;
                    LineBt.Tag = "Selected";
                    break;
                case DRAW_MODE.ARROW:
                    ArrowBt.Background = SelectBrush;
                    ArrowBt.Tag = "Selected";
                    break;
                case DRAW_MODE.TEXT:
                    TextBt.Background = SelectBrush;
                    TextBt.Tag = "Selected";
                    break;
                case DRAW_MODE.HIGHLIGHTER:
                case DRAW_MODE.HIGHLIGHTER_ARROW:
                case DRAW_MODE.HIGHLIGHTER_LINE:
                case DRAW_MODE.HIGHLIGHTER_RECTANGLE:
                case DRAW_MODE.HIGHLIGHTER_DRAW:
                    HighlighterBt.Background = SelectBrush;
                    HighlighterBt.Tag = "Selected";
                    break;
                case DRAW_MODE.LOCK:
                    LockBt.Background = SelectBrush;
                    LockBt.Tag = "Selected";
                    break;
                case DRAW_MODE.ERASER:
                    break;
                default:
                    break;
            }
        }

        private void DrawBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.DRAW);
            Fold();
        }

        private void RectangleBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.RECTANGLE);
            Fold();
        }

        private void LineBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.LINE);
            Fold();
        }

        private void ArrowBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.ARROW);
            Fold();
        }

        private void TextBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.TEXT);
            Fold();
        }

        private void HighlighterBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.HIGHLIGHTER);
            Fold();
        }

        private void CleanBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetInstance().mainCanvas.Children.Clear();
            Fold();
        }


        private void LockBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Common.IsOn = Common.IsOn ? false : true;
            MainWindow.GetInstance().SetMouseFlg();
            Fold();
        }

        private void EscBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetInstance().mainCanvas.Children.Clear();
            Common.IsOn = false;
            MainWindow.GetInstance().SetMouseFlg();
            Fold();
        }

        private void SizeUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetInstance().SizeUp();
        }

        private void SizeDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetInstance().SizeDown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
        }

        private void SubMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Grid).Background = EnterBrush;
        }

        private void SubMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Grid).Background = UnSelectBrush;
        }

        private void MenuBt_MouseEnter(object sender, MouseEventArgs e)
        {
            Border bd = sender as Border;
            if (bd != null && (bd.Tag == null || bd.Tag.ToString() == ""))
            {
                bd.Background = EnterBrush;
            }
        }

        private void MenuBt_MouseLeave(object sender, MouseEventArgs e)
        {
            Border bd = sender as Border;
            if (bd != null && (bd.Tag == null || bd.Tag.ToString() == ""))
            {
                bd.Background = UnSelectBrush;
            }
        }

        private void MenuBt2_MouseLeave(object sender, MouseEventArgs e)
        {
            Border bd = sender as Border;
            if (bd != null && (bd.Tag == null || bd.Tag.ToString() == ""))
            {
                bd.Background = UnSelectBrush2;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception)
                {
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //EmphasizeWindow.Move();
        }
    }
}
