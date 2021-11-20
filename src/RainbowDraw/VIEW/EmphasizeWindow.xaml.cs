using RainbowDraw.LOGIC;
using System.Windows;

namespace RainbowDraw.VIEW
{
    /// <summary>
    /// Interaction logic for EmphasizeWindow.xaml
    /// </summary>
    public partial class EmphasizeWindow : Window
    {

        public static EmphasizeWindow _instance = new EmphasizeWindow();
        private EmphasizeWindow()
        {
            InitializeComponent();
        }

        public static EmphasizeWindow GetInstance()
        {
            return _instance;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = false;
        }

        public static void Move()
        {
            var p = MouseHook.GetCurrentMousePosition();
            _instance.Left = p.X - (_instance.Width / 2);
            _instance.Top = p.Y - (_instance.Height / 2);
        }

        public static void Open()
        {
            if (!_instance.IsVisible)
            {
                Move();
                _instance.Topmost = true;
                _instance.Show();
                Common.IsEmphasize = true;
                //MainWindow.GetInstance().mainCanvas.Focus();
                //ToolBar.SetTopMost();
                ToolBar.GetInstance().EmphasizeCb.IsChecked = true;
                MouseHook.EmphasizeMoveTimer.Start();
            }
        }

        public static void Fold(bool flg = false)
        {
            MouseHook.EmphasizeMoveTimer.Stop();
            _instance.Hide();
            Common.IsEmphasize = flg;
            ToolBar.GetInstance().EmphasizeCb.IsChecked = false;
        }

        public static void Toggle()
        {
            if (_instance.IsVisible)
            {
                Fold();
            }
            else
            {
                Open();
            }
        }


    }
}
