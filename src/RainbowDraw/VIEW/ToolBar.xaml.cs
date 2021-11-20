using FontAwesome.WPF;
using RainbowDraw.LOGIC;
using RainbowDraw.VIEW;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace RainbowDraw
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
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    public partial class ToolBar : Window
    {
        public static ToolBar _instance = new ToolBar();
        bool IsStartup = false;

        private ToolBar()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SizeTb.Text = Common.curLineSize.ToString();
            IsStartup = CheckStartUp();
            if (IsStartup)
            {
                StartupIcon.Visibility = Visibility.Visible;
            }
            else
            {
                StartupIcon.Visibility = Visibility.Hidden;
            }
            Debug.WriteLine(App.Setting.RemoveFlg);
            FadeCb.IsChecked = App.Setting.RemoveFlg;
            foreach (var color in App.Setting.ColorList)
            {
                AddColorGrid(Common.StringToMediaColor(color), true);
            }
            ColorChange(App.Setting.LastColor);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EnableBlur(this);
        }

        public static ToolBar GetInstance()
        {
            //_instance.Topmost = false;
            //_instance.Topmost = true;
            return _instance;
        }

        public static void SetTopMost(bool flg = true)
        {
            if (flg)
            {
                _instance.Topmost = false;
                _instance.Topmost = true;
                if (Common.IsZoom)
                {
                    NativeMethods.SetWindowPos(new WindowInteropHelper(MainWindow.zw).Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);
                }
            }
            else
            {
                _instance.Topmost = false;
            }
        }

        public void Open()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        internal static void EnableBlur(Window win)
        {
            var windowHelper = new WindowInteropHelper(win);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.AccentFlags = 2;
            //↓の色はAABBGGRRの順番で設定する
            accent.GradientColor = 0x99000000;  // 60%の透明度が基本

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        //SolidColorBrush SelectBrush = new SolidColorBrush(Color.FromArgb(204, 0, 191, 139));

        readonly LinearGradientBrush SelectBrush = new LinearGradientBrush(Color.FromRgb(255, 93, 0), Color.FromRgb(93, 255, 0), new Point(0, 0), new Point(1, 1));
        readonly LinearGradientBrush EnterBrush = new LinearGradientBrush(Color.FromArgb(255, 160, 60, 0), Color.FromArgb(255, 60, 160, 0), new Point(0, 0), new Point(1, 1));
        readonly SolidColorBrush UnSelectBrush = new SolidColorBrush(Color.FromArgb(204, 51, 51, 51));
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
                case DRAW_MODE.RECTANGLE_ROUND_FULLFILL:
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

        private void Head_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left)
            //{
            //    this.DragMove();
            //}
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DrawBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.DRAW);
        }

        private void RectangleBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.RECTANGLE);
        }

        private void LineBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.LINE);
        }

        private void ArrowBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.ARROW);
        }

        private void TextBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.TEXT);
        }

        private void HighlighterBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Common.IsOn)
            {
                Common.IsOn = true;
                MainWindow.GetInstance().SetMouseFlg();
            }
            MainWindow.GetInstance().SetDrawMode(DRAW_MODE.HIGHLIGHTER);
        }

        private void CleanBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetInstance().mainCanvas.Children.Clear();
        }


        private void LockBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Common.IsOn = Common.IsOn ? false : true;
            MainWindow.GetInstance().SetMouseFlg();
        }

        private void EscBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GetInstance().mainCanvas.Children.Clear();
            Common.IsOn = false;
            MainWindow.GetInstance().SetMouseFlg();
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
            WindowState = WindowState.Minimized;
            e.Cancel = true;
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
                    MainWindow.GetInstance().SetDrawMode(DRAW_MODE.DRAW_HIGHLIGHTER);
                    return;
                }
                if (Common.nowMode == DRAW_MODE.HIGHLIGHTER)
                {
                    MainWindow.GetInstance().SetDrawMode(DRAW_MODE.HIGHLIGHTER_DRAW);
                    return;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void Shutdown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(App.Setting.HelpUrl);
            Popup.IsOpen = false;
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
            if (sender is Border bd && (bd.Tag == null || bd.Tag.ToString() == ""))
            {
                bd.Background = EnterBrush;
            }
        }

        private void MenuBt_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border bd && (bd.Tag == null || bd.Tag.ToString() == ""))
            {
                bd.Background = UnSelectBrush;
            }
        }


        public bool CheckStartUp()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                var data = key.GetValue(curAssembly.GetName().Name, null);
                if (data == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public void InstallMeOnStartUp()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }

        public void InstallMeOffStartUp()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.DeleteValue(curAssembly.GetName().Name);
            }
            catch { }
        }

        private void Startup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsStartup)
            {
                IsStartup = false;
                InstallMeOffStartUp();
                StartupIcon.Visibility = Visibility.Hidden;
            }
            else
            {
                IsStartup = true;
                InstallMeOnStartUp();
                StartupIcon.Visibility = Visibility.Visible;
            }
            Popup.IsOpen = false;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
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

        private void EmphasizeToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EmphasizeWindow.Toggle();
        }

        private void MagnificationBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Common.IsZoom = !Common.IsZoom;
            if (Common.IsZoom)
            {
                MagnificationBt.Background = SelectBrush;
            }
            else
            {
                MagnificationBt.Background = UnSelectBrush;
            }
            MainWindow.GetInstance().SetZoom();
        }

        private void EraserBt_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ToolsBt_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void FadeToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.Setting.RemoveFlg = !App.Setting.RemoveFlg;
            App.Setting.Save();
            FadeCb.IsChecked = App.Setting.RemoveFlg;
        }

        readonly LinearGradientBrush RainbowBrush = new LinearGradientBrush(
            new GradientStopCollection() {
                new GradientStop(Color.FromArgb(255, 255, 0, 0), 0.15),
                new GradientStop(Color.FromArgb(255, 255, 255, 0), 0.3),
                new GradientStop(Color.FromArgb(255, 0, 255, 0), 0.45),
                new GradientStop(Color.FromArgb(255, 0, 255, 255), 0.55),
                new GradientStop(Color.FromArgb(255, 0, 120, 255), 0.7),
                new GradientStop(Color.FromArgb(255, 255, 0, 255), 0.85),
            }, new Point(0, 0), new Point(1, 1));


        private void ColorChange_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid gd = (Grid)sender;
            if (gd.Tag != null)
            {
                ColorChange(gd.Tag.ToString());
            }
            ColorPickerPopup.IsOpen = false;
        }

        public void ColorChange(string colorStr)
        {
            if (colorStr == "Rainbow")
            {
                Common.IsRainbowColor = true;
                ColorBt.Background = RainbowBrush;
                MainWindow.curColor = Common.lastRainbowColor;
                Common.rainbowProgress = Common.lastRainbowProgress;
            }
            else
            {
                if (Common.IsRainbowColor)
                {
                    Common.lastRainbowColor = MainWindow.curColor;
                    Common.lastRainbowProgress = Common.rainbowProgress;
                }
                Common.IsRainbowColor = false;
                Color color = Common.StringToMediaColor(colorStr);
                ColorBt.Background = new SolidColorBrush(color);
                MainWindow.curColor = new SolidColorBrush(color);
                Common.rainbowProgress = MainWindow.GetInstance().GetProgress(MainWindow.curColor);
            }
            App.Setting.LastColor = colorStr;
            App.Setting.Save();
        }

        private void AddColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private static readonly Color white = Colors.White;
        private Color lastChangeColor = white;

        public Color LastChangeColor { get => lastChangeColor; set => lastChangeColor = value; }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            LastChangeColor = (Color)e.NewValue;
        }

        private void AddColorBt_Click(object sender, RoutedEventArgs e)
        {
            AddColorPopup.IsOpen = false;
            AddColorGrid((Color)AddColorCp.SelectedColor);
            //AddColorCp.GetValue
        }

        private void ColorRemove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border bd = (Border)sender;
            Grid gd = VisualTreeHelper.GetParent(bd) as Grid;
            App.Setting.ColorList.Remove(bd.Tag.ToString());
            App.Setting.Save();
            AddColorPickerMenuPn.Children.Remove(gd);
        }

        public void AddColorGrid(Color color, bool OverWrite = false)
        {
            string colorStr = Common.ColorToHexString(color);
            if (!OverWrite && App.Setting.ColorList.Contains(colorStr))
            {
                //MessageBox.Show("");
                return;
            }

            Grid gd = new Grid
            {
                Cursor = Cursors.Hand
            };

            Grid mainGd = new Grid
            {
                Width = 30,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0),
                Tag = colorStr
            };
            mainGd.MouseDown += ColorChange_MouseDown;

            Border mainBd = new Border
            {
                Margin = new Thickness(2),
                CornerRadius = new CornerRadius(30),
                Background = new SolidColorBrush(color)
            };
            mainGd.Children.Add(mainBd);

            Border closeBd = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 12,
                Height = 12,
                CornerRadius = new CornerRadius(15),
                Cursor = Cursors.Hand,
                Tag = colorStr
            };
            closeBd.MouseDown += ColorRemove_MouseDown;

            ImageAwesome closeIa = new ImageAwesome
            {
                Icon = FontAwesomeIcon.Close,
                Margin = new Thickness(3),
                Foreground = Brushes.White
            };
            closeBd.Child = closeIa;

            gd.Children.Add(mainGd);
            gd.Children.Add(closeBd);
            AddColorPickerMenuPn.Children.Add(gd);
            if (!App.Setting.ColorList.Contains(colorStr))
            {
                App.Setting.ColorList.Add(colorStr);
                App.Setting.Save();
            }
        }

        private void RecordStartBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Common.IsRecord = !Common.IsRecord;
            if (Common.IsRecord)
            {
            }
            else
            {
            }
        }

        private void InsertTool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid gd = (Grid)sender;
            if (gd.Tag != null)
            {
                MainWindow.GetInstance().InsertTool(gd.Tag.ToString());
            }
            ToolsPopup.IsOpen = false;
        }

        private void AddTools_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("開発中です。");
        }
    }
}
