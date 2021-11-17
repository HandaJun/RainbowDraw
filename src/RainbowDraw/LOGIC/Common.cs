using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using RainbowDraw.DATA;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace RainbowDraw.LOGIC
{
    public class Common
    {
        public static bool IsEmphasize = false;

        public static int curLineSize = 5;
        public static int colorChangeTime = 250;
        public static bool rainbowFlg = false;
        public static bool drawFlg = false;
        public static float rainbowProgress = 0;

        public static ConcurrentDictionary<string, ItemData> CanvasDic = new ConcurrentDictionary<string, ItemData>();
        public static Stack<string> CanvasIdStack = new Stack<string>();

        public static DRAW_MODE nowMode = DRAW_MODE.DRAW;
        public static DRAW_MODE prevMode = DRAW_MODE.DRAW;

        public static bool IsOn = true;

        public static double MaxLineSize = 30;
        public static double MinLineSize = 2;

        public static bool IsZoom = false;
        public static bool IsRecord = false;
        public static bool IsRainbowColor = true;
        public static System.Windows.Media.Brush lastRainbowColor = System.Windows.Media.Brushes.Red;
        public static float lastRainbowProgress = 0;

        public static void Invoke(Action act)
        {
            MainWindow.GetInstance().Dispatcher.Invoke(() => {
                try
                {
                    act();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        public static Rect[] ScreenRectInfo;
        public static double MainScreenRatio;

        public static void GetScreenDpi()
        {
            double m11 = PresentationSource.FromVisual((Visual)System.Windows.Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;
            Common.MainScreenRatio = m11;
            if (((IEnumerable<Screen>)Screen.AllScreens).Count<Screen>() == 1)
            {
                Common.ScreenRectInfo = new Rect[1];
                Screen allScreen = Screen.AllScreens[0];
                Rect[] screenRectInfo = Common.ScreenRectInfo;
                double x = (double)allScreen.Bounds.X / m11;
                double y = (double)allScreen.Bounds.Y / m11;
                Rectangle bounds = allScreen.Bounds;
                double width = (double)bounds.Width / m11;
                bounds = allScreen.Bounds;
                double height = (double)bounds.Height / m11;
                Rect rect = new Rect(x, y, width, height);
                screenRectInfo[0] = rect;
            }
            else
            {
                Common.ScreenRectInfo = new Rect[checked(((IEnumerable<Screen>)Screen.AllScreens).Count<Screen>() - 1 + 1)];
                Screen primaryScreen = Screen.PrimaryScreen;
                Rect rect = new Rect();
                ref Rect local = ref rect;
                Rectangle bounds1 = primaryScreen.Bounds;
                double x = (double)bounds1.X / m11;
                bounds1 = primaryScreen.Bounds;
                double y = (double)bounds1.Y / m11;
                bounds1 = primaryScreen.Bounds;
                double width = (double)bounds1.Width / m11;
                bounds1 = primaryScreen.Bounds;
                double height = (double)bounds1.Height / m11;
                local = new Rect(x, y, width, height);
                int num = checked(((IEnumerable<Screen>)Screen.AllScreens).Count<Screen>() - 1);
                int index = 0;
                while (index <= num)
                {
                    Rectangle bounds2 = Screen.AllScreens[index].Bounds;
                    if (Screen.AllScreens[index] == Screen.PrimaryScreen)
                    {
                        Common.ScreenRectInfo[index] = rect;
                    }
                    else
                    {
                        //bounds2.X = (double)bounds2.X >= Common.ScreenRectInfo[index].X ? checked((int)Math.Round(unchecked((double)bounds2.X / m11))) : checked((int)Math.Round(unchecked((double)checked(-bounds2.Width) / m11)));
                        //bounds2.Y = (double)bounds2.Y >= Common.ScreenRectInfo[index].Y ? checked((int)Math.Round(unchecked((double)bounds2.Y / m11))) : checked((int)Math.Round(unchecked((double)checked(-bounds2.Height) / m11)));
                        //bounds2.Width = checked((int)Math.Round(unchecked((double)bounds2.Width / m11)));
                        //bounds2.Height = checked((int)Math.Round(unchecked((double)bounds2.Height / m11)));
                        Common.ScreenRectInfo[index] = new Rect((double)bounds2.X, (double)bounds2.Y, (double)bounds2.Width, (double)bounds2.Height);
                    }
                    checked { ++index; }
                }
            }
        }

        public static BitmapImage BitmapToBitmpaImage(ref Bitmap fBit)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                fBit.Save((Stream)memoryStream, ImageFormat.Png);
                MemoryStream fMem = memoryStream;
                return MemStreamToBitmapImage(ref fMem);
            }
        }

        public static BitmapImage MemStreamToBitmapImage(ref MemoryStream fMem)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = (Stream)new MemoryStream();
            fMem.WriteTo(bitmapImage.StreamSource);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static System.Windows.Input.Cursor MakeCursor(RenderTargetBitmap rtb, double w)
        {
            using (MemoryStream memoryStream1 = new MemoryStream())
            {
                PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)rtb));
                pngBitmapEncoder.Save((Stream)memoryStream1);
                byte[] array = memoryStream1.ToArray();
                int length = array.GetLength(0);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    memoryStream2.Write(BitConverter.GetBytes((short)0), 0, 2);
                    memoryStream2.Write(BitConverter.GetBytes((short)2), 0, 2);
                    memoryStream2.Write(BitConverter.GetBytes((short)1), 0, 2);
                    memoryStream2.WriteByte((byte)32);
                    memoryStream2.WriteByte((byte)32);
                    memoryStream2.WriteByte((byte)0);
                    memoryStream2.WriteByte((byte)0);
                    memoryStream2.Write(BitConverter.GetBytes(checked((short)Math.Round(unchecked(w / 2.0)))), 0, 2);
                    memoryStream2.Write(BitConverter.GetBytes(checked((short)Math.Round(unchecked(w / 2.0)))), 0, 2);
                    memoryStream2.Write(BitConverter.GetBytes(length), 0, 4);
                    memoryStream2.Write(BitConverter.GetBytes(22), 0, 4);
                    memoryStream2.Write(array, 0, length);
                    memoryStream2.Seek(0L, SeekOrigin.Begin);
                    return new System.Windows.Input.Cursor((Stream)memoryStream2);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool DownLoad_File(string sourceURL, string savePath)
        {
            bool flag;
            try
            {
                WebClient webClient = new WebClient();
                if (Operators.CompareString(Microsoft.VisualBasic.FileSystem.Dir(savePath, FileAttribute.Archive), "", false) != 0)
                    Microsoft.VisualBasic.FileSystem.Kill(savePath);
                string address = sourceURL;
                string fileName = savePath;
                webClient.DownloadFile(address, fileName);
                flag = true;
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                flag = false;
                ProjectData.ClearProjectError();
            }
            return flag;
        }

        public static string Get_Webpage_Source(string fUrl)
        {
            WebClient webClient = new WebClient();
            string str1;
            try
            {
                webClient.Encoding = Encoding.GetEncoding("utf-8");
                string str2 = webClient.DownloadString(fUrl);
                webClient.Dispose();
                str1 = str2;
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                Exception exception = ex;
                webClient.Dispose();
                str1 = "\b" + exception.Message;
                ProjectData.ClearProjectError();
            }
            return str1;
        }

        public static string GetFileHash256(string fName)
        {
            string Expression = "";
            try
            {
                using (FileStream fileStream = new FileStream(fName, FileMode.Open, FileAccess.Read))
                {
                    Expression = BitConverter.ToString(SHA256.Create().ComputeHash((Stream)fileStream));
                    Expression = Strings.Replace(Expression, "-", "");
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                ProjectData.ClearProjectError();
            }
            return Expression;
        }

        public static string ColorToHexString(System.Windows.Media.Color color)
        {
            return new System.Windows.Media.ColorConverter().ConvertToString(color);
        }

        public static string ColorToString(System.Windows.Media.Color srcColor) => ColorTranslator.ToHtml(System.Drawing.Color.FromArgb((int)srcColor.A, (int)srcColor.R, (int)srcColor.G, (int)srcColor.B));

        public static System.Drawing.Color StringToColor(string colorStr) => ColorTranslator.FromHtml(colorStr);

        public static System.Windows.Media.Color StringToMediaColor(string colorStr)
        {
            System.Drawing.Color color = StringToColor(colorStr);
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static double getAlgleFromPoint(System.Windows.Point pos1, System.Windows.Point pos2)
        {
            double num1;
            if (pos1 == pos2)
            {
                num1 = 0.0;
            }
            else
            {
                int num2 = checked((int)Math.Round(unchecked(pos2.X - pos1.X)));
                int num3 = checked((int)Math.Round(unchecked(pos2.Y - pos1.Y)));
                double num4 = Math.Atan((double)num2 / (double)num3) * 180.0 / Math.PI;
                if (num3 > 0)
                    num4 = 180.0 - Math.Abs(num4);
                double d = num2 >= 0 ? Math.Abs(num4) : -Math.Abs(num4);
                if (double.IsNaN(d))
                    d = 0.0;
                num1 = d;
            }
            return num1;
        }

        public static System.Windows.Point getPointFromAngle(System.Windows.Point pos1, System.Windows.Point pos2, double Angle)
        {
            pos2.X -= pos1.X;
            pos2.Y -= pos1.Y;
            double num1 = Angle * (Math.PI / 180.0);
            double num2 = Math.Cos(num1) * pos2.X - Math.Sin(num1) * pos2.Y;
            double num3 = Math.Sin(num1) * pos2.X + Math.Cos(num1) * pos2.Y;
            return new System.Windows.Point(num2 + pos1.X, num3 + pos1.Y);
        }


        public static double GetDisplayRatio(Visual win)
        {
            double num;
            try
            {
                num = PresentationSource.FromVisual(win).CompositionTarget.TransformToDevice.M11;
            }
            catch (Exception)
            {
                num = 1.0;
            }
            return num;
        }

        public static object ReadRegKey(string HKEY, string KeyString, string KeyName)
        {
            object obj;
            try
            {
                string Left = HKEY;
                if (Operators.CompareString(Left, "LM", false) != 0)
                {
                    if (Operators.CompareString(Left, "CU", false) != 0)
                    {
                        if (Operators.CompareString(Left, "CR", false) == 0)
                        {
                            obj = RuntimeHelpers.GetObjectValue(Registry.ClassesRoot.CreateSubKey(KeyString).GetValue(KeyName)) ?? (object)"";
                            Registry.ClassesRoot.Close();
                        }
                        else
                            obj = (object)"";
                    }
                    else
                    {
                        obj = RuntimeHelpers.GetObjectValue(Registry.CurrentUser.CreateSubKey(KeyString).GetValue(KeyName)) ?? (object)"";
                        Registry.CurrentUser.Close();
                    }
                }
                else
                {
                    obj = RuntimeHelpers.GetObjectValue(Registry.LocalMachine.CreateSubKey(KeyString).GetValue(KeyName)) ?? (object)"";
                    Registry.LocalMachine.Close();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                obj = (object)"";
                ProjectData.ClearProjectError();
            }
            return obj;
        }

        public static bool WriteRegKey(string HKEY, string KeyString, string KeyName, object KeyValue)
        {
            bool flag;
            try
            {
                flag = true;
                string Left = HKEY;
                if (Operators.CompareString(Left, "LM", false) != 0)
                {
                    if (Operators.CompareString(Left, "CU", false) != 0)
                    {
                        if (Operators.CompareString(Left, "CR", false) == 0)
                        {
                            Registry.ClassesRoot.CreateSubKey(KeyString).SetValue(KeyName, RuntimeHelpers.GetObjectValue(KeyValue));
                            Registry.ClassesRoot.Close();
                        }
                    }
                    else
                    {
                        Registry.CurrentUser.CreateSubKey(KeyString).SetValue(KeyName, RuntimeHelpers.GetObjectValue(KeyValue));
                        Registry.CurrentUser.Close();
                    }
                }
                else
                {
                    Registry.LocalMachine.CreateSubKey(KeyString).SetValue(KeyName, RuntimeHelpers.GetObjectValue(KeyValue));
                    Registry.LocalMachine.Close();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                flag = false;
                ProjectData.ClearProjectError();
            }
            return flag;
        }

        public static bool DeleteRegKey(string HKEY, string KeyString, string KeyName)
        {
            bool flag;
            try
            {
                flag = true;
                string Left = HKEY;
                if (Operators.CompareString(Left, "LM", false) != 0)
                {
                    if (Operators.CompareString(Left, "CU", false) == 0)
                    {
                        Registry.CurrentUser.OpenSubKey(KeyString, true).DeleteValue(KeyName);
                        Registry.CurrentUser.Close();
                    }
                }
                else
                {
                    Registry.LocalMachine.OpenSubKey(KeyString, true).DeleteValue(KeyName);
                    Registry.LocalMachine.Close();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                flag = false;
                ProjectData.ClearProjectError();
            }
            return flag;
        }

        public static bool GetRegValueBoolean(string HKEY, string KeyString, string KeyName)
        {
            string Left = Conversions.ToString(ReadRegKey(HKEY, KeyString, KeyName));
            return Operators.CompareString(Left, "", false) != 0 && (Versioned.IsNumeric((object)Left) && Conversions.ToInteger(Left) == -1);
        }

        public static void MakeStoryBoard(
            string targetCtlName,
            bool isIn,
            double sValue,
            double eValue,
            double runSec,
            PropertyPath Attr,
            FrameworkElement ele)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            if (isIn)
            {
                doubleAnimation.From = new double?(sValue);
                doubleAnimation.To = new double?(eValue);
                doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(runSec));
                Storyboard.SetTargetName((DependencyObject)doubleAnimation, targetCtlName);
                Storyboard.SetTargetProperty((DependencyObject)doubleAnimation, Attr);
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add((Timeline)doubleAnimation);
                storyboard.Begin(ele);
            }
            if (isIn)
                return;
            doubleAnimation.From = new double?(sValue);
            doubleAnimation.To = new double?(eValue);
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(runSec));
            Storyboard.SetTargetName((DependencyObject)doubleAnimation, targetCtlName);
            Storyboard.SetTargetProperty((DependencyObject)doubleAnimation, Attr);
            Storyboard storyboard1 = new Storyboard();
            storyboard1.Children.Add((Timeline)doubleAnimation);
            storyboard1.Begin(ele);
        }

        public struct Font_Info
        {
            public System.Windows.Media.FontFamily FontFamily;
            public double FontSize;
            public SolidColorBrush Foreground;
            public System.Windows.FontStyle FontStyle;
            public FontWeight FontWeight;
        }


    }
}
