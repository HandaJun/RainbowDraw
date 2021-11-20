using RainbowDraw.LOGIC;
using RainbowDraw.VIEW;
using System.Windows;
using System.Windows.Forms;

namespace RainbowDraw
{
    public partial class MainWindow : Window
    {
        void OnHookKeyDown(object sender, KeyEventArgs e)
        {
            IInputElement focusedControl = System.Windows.Input.Keyboard.FocusedElement;
            if (focusedControl != null && focusedControl.GetType().Name == "TextBox")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    ToolBar.SetTopMost();
                    System.Windows.Input.Keyboard.ClearFocus();
                }
                return;
            }

            // Global
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Tab:
                        Common.IsOn = !Common.IsOn;
                        SetMouseFlg();
                        e.Handled = true;
                        return;
                    case Keys.F9:
                        if (ToolBar.GetInstance().WindowState == WindowState.Normal)
                        {
                            ToolBar.GetInstance().WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            ToolBar.GetInstance().WindowState = WindowState.Normal;
                        }
                        e.Handled = true;
                        return;
                    case Keys.F10:
                        EmphasizeWindow.Toggle();
                        e.Handled = true;
                        return;
                    case Keys.F11:
                        App.Setting.RemoveFlg = !App.Setting.RemoveFlg;
                        App.Setting.Save();
                        ToolBar.GetInstance().FadeCb.IsChecked = App.Setting.RemoveFlg;
                        //RenderVisualService.RenderToPNGFile(mainCanvas, "myawesomeimage.png");
                        return;
                    case Keys.F8:
                        Common.IsZoom = !Common.IsZoom;
                        SetZoom();
                        return;
                    case Keys.Add:
                        if (Common.IsZoom)
                        {
                            zw.Magnify();
                        }
                        return;
                    case Keys.Subtract:
                        if (Common.IsZoom)
                        {
                            zw.Reduce();
                        }
                        return;
                    default:
                        break;
                }
            }



            // ↓ IsOn
            if (!Common.IsOn)
            {
                return;
            }

            if (e.Control && e.KeyCode == Keys.Z)
            {
                RemoveLast();
                e.Handled = true;
                return;
            }

            if (e.Alt && e.KeyCode == Keys.F4)
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            if (Common.nowMode == DRAW_MODE.DRAW)
            {
                if (e.Alt)
                {
                    SetDrawMode(DRAW_MODE.DRAW_HIGHLIGHTER);
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                {
                    SetDrawMode(DRAW_MODE.DRAW_LINE);
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
                {
                    SetDrawMode(DRAW_MODE.DRAW_RECTANGLE);
                    //e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.Tab)
                {
                    SetDrawMode(DRAW_MODE.DRAW_ARROW);
                    e.Handled = true;
                    return;
                }

            }
            if (Common.nowMode == DRAW_MODE.HIGHLIGHTER)
            {
                if (e.Alt)
                {
                    SetDrawMode(DRAW_MODE.HIGHLIGHTER_DRAW);
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                {
                    SetDrawMode(DRAW_MODE.HIGHLIGHTER_LINE);
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
                {
                    SetDrawMode(DRAW_MODE.HIGHLIGHTER_RECTANGLE);
                    //e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.Tab)
                {
                    SetDrawMode(DRAW_MODE.HIGHLIGHTER_ARROW);
                    e.Handled = true;
                    return;
                }
            }
            if (Common.nowMode == DRAW_MODE.RECTANGLE)
            {
                if (e.Alt)
                {
                    SetDrawMode(DRAW_MODE.RECTANGLE_FULLFILL);
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                {
                    //SetDrawMode(DRAW_MODE.RECTANGLE_FULLFILL);
                    return;
                }

                if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
                {
                    SetDrawMode(DRAW_MODE.RECTANGLE_ROUND);
                    //e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.Tab)
                {
                    SetDrawMode(DRAW_MODE.RECTANGLE_ROUND_FULLFILL);
                    e.Handled = true;
                    return;
                }
            }

            //if (Common.nowMode == DRAW_MODE.LINE)
            //{
            //    //if (e.Alt)
            //    //{
            //    //    SetDrawMode(DRAW_MODE.RECTANGLE_FULLFILL);
            //    //    return;
            //    //}
            //    Task.Run(() => {
            //        Debug.WriteLine(e.KeyCode.ToString());
            //        Debug.WriteLine(e.Shift.ToString());

            //        if (e.Shift)
            //        {
            //            Debug.WriteLine("on 45");
            //            MainWindow.SetLineRad(45);
            //            return;
            //        }
            //    });

            //    //if (e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            //    //{
            //    //    SetDrawMode(DRAW_MODE.RECTANGLE_ROUND);
            //    //    return;
            //    //}

            //    //if (e.KeyCode == Keys.Tab)
            //    //{
            //    //    SetDrawMode(DRAW_MODE.RECTANGLE_ROUND_FULLFILL);
            //    //    return;
            //    //}
            //}

            if (!e.Control && !e.Alt && !e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.D:
                        SetDrawMode(DRAW_MODE.DRAW);
                        break;
                    case Keys.R:
                        SetDrawMode(DRAW_MODE.RECTANGLE);
                        break;
                    case Keys.L:
                    case Keys.E:
                        SetDrawMode(DRAW_MODE.LINE);
                        break;
                    case Keys.A:
                        SetDrawMode(DRAW_MODE.ARROW);
                        break;
                    case Keys.T:
                        SetDrawMode(DRAW_MODE.TEXT);
                        break;
                    case Keys.H:
                        SetDrawMode(DRAW_MODE.HIGHLIGHTER);
                        break;
                    case Keys.Escape:
                        mainCanvas.Children.Clear();
                        Common.IsOn = false;
                        SetMouseFlg();
                        break;
                    case Keys.C:
                    case Keys.Delete:
                        mainCanvas.Children.Clear();
                        break;
                    case Keys.Back:
                        RemoveLast();
                        break;
                    case Keys.Up:
                        SizeUp();
                        break;
                    case Keys.Down:
                        SizeDown();
                        break;

                    //case Keys.F2:
                    //    EmphasizeWindow.Toggle();
                    //    break;
                    case Keys.D2:
                    case Keys.NumPad2:
                        SetSize(2);
                        break;
                    case Keys.D3:
                    case Keys.NumPad3:
                        SetSize(3);
                        break;
                    case Keys.D4:
                    case Keys.NumPad4:
                        SetSize(4);
                        break;
                    case Keys.D5:
                    case Keys.NumPad5:
                        SetSize(5);
                        break;
                    case Keys.D6:
                    case Keys.NumPad6:
                        SetSize(6);
                        break;
                    case Keys.D7:
                    case Keys.NumPad7:
                        SetSize(7);
                        break;
                    case Keys.D8:
                    case Keys.NumPad8:
                        SetSize(8);
                        break;
                    case Keys.D9:
                    case Keys.NumPad9:
                        SetSize(9);
                        break;
                    case Keys.D0:
                    case Keys.NumPad0:
                        SetSize(10);
                        break;
                    default:
                        break;
                }
            }
        }

        void OnHookKeyUp(object sender, KeyEventArgs e)
        {
            if (!Common.IsOn)
            {
                return;
            }

            if (Common.prevMode == DRAW_MODE.DRAW)
            {
                if ((e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                    && Common.nowMode == DRAW_MODE.DRAW_LINE)
                {
                    if (line != null)
                    {
                        NextRainbow(float.Parse(line.Tag.ToString()));
                    }
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if ((e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
                    && Common.nowMode == DRAW_MODE.DRAW_RECTANGLE)
                {
                    if (rectBd != null)
                    {
                        NextRainbow(float.Parse(rectBd.Tag.ToString()));
                    }
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if ((e.KeyCode == Keys.Tab)
                    && Common.nowMode == DRAW_MODE.DRAW_ARROW)
                {
                    if (line != null)
                    {
                        NextRainbow(float.Parse(line.Tag.ToString()));
                    }
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if (e.Alt && Common.nowMode == DRAW_MODE.DRAW_HIGHLIGHTER)
                {
                    SetDrawMode(Common.prevMode);
                    return;
                }

            }

            if (Common.prevMode == DRAW_MODE.HIGHLIGHTER)
            {
                if ((e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                    && Common.nowMode == DRAW_MODE.HIGHLIGHTER_LINE)
                {
                    if (line != null)
                    {
                        NextRainbow(float.Parse(line.Tag.ToString()));
                    }
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if ((e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
                    && Common.nowMode == DRAW_MODE.HIGHLIGHTER_RECTANGLE)
                {
                    if (rectBd != null)
                    {
                        NextRainbow(float.Parse(rectBd.Tag.ToString()));
                    }
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if ((e.KeyCode == Keys.Tab)
                    && Common.nowMode == DRAW_MODE.HIGHLIGHTER_ARROW)
                {
                    if (line != null)
                    {
                        NextRainbow(float.Parse(line.Tag.ToString()));
                    }
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if (e.Alt && Common.nowMode == DRAW_MODE.HIGHLIGHTER_DRAW)
                {
                    SetDrawMode(Common.prevMode);
                    return;
                }

            }

            if (Common.prevMode == DRAW_MODE.RECTANGLE)
            {
                if ((e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                    && Common.nowMode == DRAW_MODE.RECTANGLE_FULLFILL)
                {
                    //SetDrawMode(Common.prevMode);
                    return;
                }

                if ((e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
                    && Common.nowMode == DRAW_MODE.RECTANGLE_ROUND)
                {
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if ((e.KeyCode == Keys.Tab)
                    && Common.nowMode == DRAW_MODE.RECTANGLE_ROUND_FULLFILL)
                {
                    SetDrawMode(Common.prevMode);
                    return;
                }

                if (e.Alt && Common.nowMode == DRAW_MODE.RECTANGLE_FULLFILL)
                {
                    SetDrawMode(Common.prevMode);
                    return;
                }

            }

            //if (Common.nowMode == DRAW_MODE.LINE)
            //{
            //    if (e.Shift)
            //    {
            //        Debug.WriteLine("off 45");
            //        MainWindow.SetLineRad(2);
            //        return;
            //    }
            //}



            //if (e.Control)
            //{
            //    IsOn = false;
            //    SetMouseFlg();
            //}
        }
    }
}
