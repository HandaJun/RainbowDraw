using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        public void CreateTools(string name)
        {
            Image img = new Image();
            var image = GetImage(name);
            img.Source = image;
            img.Stretch = Stretch.Fill;

            ContentControl cc = new ContentControl();
            Selector.SetIsSelected(cc, true);
            cc.Style = this.FindResource("DesignerItemStyle") as Style;

            Canvas.SetLeft(cc, MainWindow.GetInstance().ActualWidth / 2 - image.PixelWidth / 2);
            Canvas.SetTop(cc, MainWindow.GetInstance().ActualHeight / 2 - image.PixelHeight / 2);
            cc.Width = image.PixelWidth;
            cc.Height = image.PixelHeight;

            Grid wrap = new Grid
            {
                IsHitTestVisible = false,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0))
            };
            wrap.Children.Add(img);

            cc.Content = wrap;

            nowCanvas.Children.Add(cc);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public BitmapSource GetImage(string name)
        {
            BitmapImage bmp;
            try
            {
                string tempPath = @"IMG\" + name;
                if (!tempPath.Contains("."))
                {
                    tempPath += ".png";
                }
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(tempPath, UriKind.Relative);
                bmp.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
            return bmp;
        }

    }
}
