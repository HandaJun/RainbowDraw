using FontAwesome.WPF;
using RainbowDraw.LOGIC;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        public void CreateTextbox()
        {
            Grid gd = new Grid();
            gd.MouseEnter += TextMove_MouseEnter;
            gd.MouseLeave += TextMove_MouseLeave;
            Canvas.SetLeft(gd, startX);
            Canvas.SetTop(gd, startY);

            textBox = new TextBox
            {
                FontSize = Common.curLineSize * 10,
                MinWidth = 30,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                IsTabStop = true
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
            textBox.Tag = Common.rainbowProgress.ToString();
            NextRainbow(0.06f);
            //nowCanvas.Children.Add(textBox);
            gd.Children.Add(textBox);

            Border bd = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(130, 44, 44, 44)),
                Width = 20,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(-10, -10, 0, 0),
                Visibility = Visibility.Hidden
            };
            bd.MouseDown += TextMove_MouseDown;
            bd.MouseMove += TextMove_MouseMove;
            bd.MouseUp += TextMove_MouseUp;
            ImageAwesome ia = new ImageAwesome
            {
                Icon = FontAwesomeIcon.ArrowsAlt,
                Width = 15,
                Foreground = Brushes.White
            };
            bd.Child = ia;
            gd.Children.Add(bd);

            nowCanvas.Children.Add(gd);

            textBox.Focus();
        }


        private bool isImeOnConv = false;
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
            Debug.WriteLine(isImeOnConv);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            SetGradientColor(tb);

            if (e.Key == Key.Up && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                FontSizeChange(tb, true);
                e.Handled = true;
            }

            if (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                FontSizeChange(tb, false);
                e.Handled = true;
            }

        }

        public void SetGradientColor(TextBox tb)
        {
            if (isImeOnConv == false)
            {
                //Color c = (Color)ColorConverter.ConvertFromString(tb.Tag.ToString()); 
                float startProgress = 0f;
                if (tb.Tag != null)
                {
                    float.TryParse(tb.Tag.ToString(), out startProgress);
                }
                Brush startBrush = Rainbow(startProgress);
                //Color c = (Color)ColorConverter.ConvertFromString(tb.Tag.ToString()); 
                Color startColor = ((SolidColorBrush)startBrush).Color;
                LinearGradientBrush lg = new LinearGradientBrush
                {
                    MappingMode = BrushMappingMode.Absolute,
                    EndPoint = new Point(tb.ActualWidth, tb.ActualHeight)
                };
                lg.GradientStops.Add(new GradientStop(startColor, 0));
                double addProg = Math.Min((float)tb.ActualWidth / 5000f, 0.2f);
                addProg = Math.Max(addProg, 0.06f);
                Brush endBrush = Rainbow(startProgress + (float)addProg);
                Color endColor = ((SolidColorBrush)endBrush).Color;
                lg.GradientStops.Add(new GradientStop(endColor, 1));
                tb.Foreground = lg;
            }
        }

        public void FontSizeChange(TextBox tb, bool upFlg)
        {
            double fontSize = tb.FontSize;
            double changeSize;
            if (fontSize < 20)
            {
                changeSize = 2;
            }
            else if (fontSize < 40)
            {
                changeSize = 4;
            }
            else if (fontSize < 60)
            {
                changeSize = 5;
            }
            else if (fontSize < 100)
            {
                changeSize = 10;
            }
            else
            {
                changeSize = 50;
            }

            if (upFlg)
            {
                tb.FontSize = Math.Min(tb.FontSize + changeSize, 600);
            }
            else
            {
                tb.FontSize = Math.Max(tb.FontSize - changeSize, 10);
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
                if (string.IsNullOrEmpty(tb.Text))
                {
                    Grid gd = VisualTreeHelper.GetParent(tb) as Grid;
                    Canvas cv = VisualTreeHelper.GetParent(gd) as Canvas;
                    string id = cv.Tag.ToString();
                    mainCanvas.Children.Remove((System.Windows.Controls.Canvas)mainCanvas.FindName(id));
                }
                SetGradientColor(tb);
            }
        }

        bool textMoveFlg = false;
        Grid moveText = null;
        private void TextMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                textMoveFlg = true;
                //Grid gd = (sender as Border).Parent as Grid;
                moveText = VisualTreeHelper.GetParent((sender as Border)) as Grid;
                Mouse.Capture((UIElement)sender);
                var position = Mouse.GetPosition(this);
                Canvas.SetLeft(moveText, position.X);
                Canvas.SetTop(moveText, position.Y);
            }
        }

        private void TextMove_MouseMove(object sender, MouseEventArgs e)
        {
            if (textMoveFlg)
            {
                var position = Mouse.GetPosition(this);
                Canvas.SetLeft(moveText, position.X);
                Canvas.SetTop(moveText, position.Y);
            }
        }

        private void TextMove_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            textMoveFlg = false;
        }

        private void TextMove_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid gd = sender as Grid;
            foreach (var item in gd.Children)
            {
                if (item.GetType().Name == "Border")
                {
                    (item as Border).Visibility = Visibility.Visible;
                }
            }
        }

        private void TextMove_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid gd = sender as Grid;
            foreach (var item in gd.Children)
            {
                if (item.GetType().Name == "Border")
                {
                    (item as Border).Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
