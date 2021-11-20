using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace RainbowDraw.LOGIC
{
    public static class User32Helper
    {

        public const uint MB_ICONWARNING = (uint)0x00000030L;
        public const uint MB_CANCELTRYCONTINUE = (uint)0x00000006L;
        public const uint MB_DEFBUTTON2 = (uint)0x00000100L;

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;

        public const uint GA_ROOT = 2;


        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point2
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 指定された文字列をウィンドウのタイトルとクラス名に含んでいるプロセスを閉じる
        /// </summary>
        /// <param name="windowText">ウィンドウのタイトルに含むべき文字列。
        /// nullを指定すると、classNameだけで検索する。</param>
        /// <param name="className">ウィンドウが属するクラス名に含むべき文字列。
        /// nullを指定すると、windowTextだけで検索する。</param>
        public static void KillProcessesByWindow(string windowText, string className)
        {
            try
            {
                Process[] ps = GetProcessesByWindow(windowText, className);

                foreach (var item in ps)
                {
                    //Log.Info("●" + item.ProcessName + " - Kill");
                    item.Kill();
                }

                if (ps.Length > 0)
                {
                    Thread.Sleep(1000);
                }
            }
            catch { }
        }

        /// <summary>
        /// 指定された文字列をウィンドウのタイトルとクラス名に含んでいるプロセスを
        /// すべて取得する。
        /// </summary>
        /// <param name="windowText">ウィンドウのタイトルに含むべき文字列。
        /// nullを指定すると、classNameだけで検索する。</param>
        /// <param name="className">ウィンドウが属するクラス名に含むべき文字列。
        /// nullを指定すると、windowTextだけで検索する。</param>
        /// <returns>見つかったプロセスの配列。</returns>
        public static Process[] GetProcessesByWindow(
            string windowText, string className)
        {
            //検索の準備をする
            foundProcesses = new ArrayList();
            foundProcessIds = new ArrayList();
            searchWindowText = windowText;
            searchClassName = className;

            Process[] processList = null;
            try
            {
                //ウィンドウを列挙して、対象のプロセスを探す
                EnumWindows(new EnumWindowsDelegate(EnumWindowCallBack), IntPtr.Zero);

                //プロセス取得
                processList = (Process[])foundProcesses.ToArray(typeof(Process));
            }
            catch { }

            //結果を返す
            return processList;
        }

        private static string searchWindowText = null;
        private static string searchClassName = null;
        private static ArrayList foundProcessIds = null;
        private static ArrayList foundProcesses = null;

        private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool EnumWindows(EnumWindowsDelegate lpEnumFunc,
            IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd,
            StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd,
            StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowThreadProcessId(
            IntPtr hWnd, out int lpdwProcessId);

        private static bool EnumWindowCallBack(IntPtr hWnd, IntPtr lparam)
        {
            if (searchWindowText != null)
            {
                //ウィンドウのタイトルの長さを取得する
                int textLen = GetWindowTextLength(hWnd);
                if (textLen == 0)
                {
                    //次のウィンドウを検索
                    return true;
                }
                //ウィンドウのタイトルを取得する
                StringBuilder tsb = new StringBuilder(textLen + 1);
                GetWindowText(hWnd, tsb, tsb.Capacity);
                //タイトルに指定された文字列を含むか
                if (tsb.ToString().IndexOf(searchWindowText) < 0)
                {
                    //含んでいない時は、次のウィンドウを検索
                    return true;
                }
            }

            if (searchClassName != null)
            {
                //ウィンドウのクラス名を取得する
                StringBuilder csb = new StringBuilder(256);
                GetClassName(hWnd, csb, csb.Capacity);
                //クラス名に指定された文字列を含むか
                if (csb.ToString().IndexOf(searchClassName) < 0)
                {
                    //含んでいない時は、次のウィンドウを検索
                    return true;
                }
            }

            //プロセスのIDを取得する
            GetWindowThreadProcessId(hWnd, out int processId);
            //今まで見つかったプロセスでは無いことを確認する
            if (!foundProcessIds.Contains(processId))
            {
                foundProcessIds.Add(processId);
                //プロセスIDをからProcessオブジェクトを作成する
                foundProcesses.Add(Process.GetProcessById(processId));
            }

            //次のウィンドウを検索
            return true;
        }


        /// <summary>
        /// プロセスIDで
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            try
            {
                for (var index = 0; index < processesByName.Length; index++)
                {
                    processIndexdName = index == 0 ? processName : processName + "#" + index;
                    var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                    if ((int)processId.NextValue() == pid)
                    {
                        return processIndexdName;
                    }
                }
            }
            catch { }

            return processIndexdName;
        }

        /// <summary>
        /// プロセス名で親プロセス取得
        /// </summary>
        /// <param name="indexedProcessName"></param>
        /// <returns></returns>
        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            Process proc = null;
            try
            {
                var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
                proc = Process.GetProcessById((int)parentId.NextValue());
            }
            catch { }
            return proc;
        }

        /// <summary>
        /// プロセスの親プロセスを取得
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static Process Parent(this Process process)
        {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetBackgroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        /// <summary>
        /// Windowフォームアクティブ化処理
        /// </summary>
        /// <param name="handle">フォームハンドル</param>
        /// <returns>true : 成功 / false : 失敗</returns>
        public static bool ForceActive(IntPtr handle, bool IsForeGround)
        {
            const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
            const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
            const int SPIF_SENDCHANGE = 0x2;

            IntPtr dummy = IntPtr.Zero;
            IntPtr timeout = IntPtr.Zero;


            // フォアグラウンドウィンドウを作成したスレッドのIDを取得
            int foregroundID = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            // 目的のウィンドウを作成したスレッドのIDを取得
            int targetID = GetWindowThreadProcessId(handle, out _);

            // スレッドのインプット状態を結び付ける
            AttachThreadInput(targetID, foregroundID, true);

            // 現在の設定を timeout に保存
            SystemParametersInfo(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, timeout, 0);
            // ウィンドウの切り替え時間を 0ms にする
            SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, dummy, SPIF_SENDCHANGE);


            bool isSuccess;
            // ウィンドウをフォアグラウンドに持ってくる
            if (IsForeGround)
            {
                isSuccess = SetForegroundWindow(handle);
            }
            else
            {
                isSuccess = SetBackgroundWindow(handle);
            }

            // 設定を元に戻す
            SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, timeout, SPIF_SENDCHANGE);

            // スレッドのインプット状態を切り離す
            AttachThreadInput(targetID, foregroundID, false);

            return isSuccess;
        }



        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out System.Drawing.Rectangle lpRect);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(int hWnd, String text, String caption, uint type);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hwnd, bool enabled);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd,
            int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        public static bool ShowHideWindow(IntPtr hwnd, int nCmdShow)
        {
            return ShowWindow(hwnd, nCmdShow);
        }

        public static string GetClassName(IntPtr hWnd)
        {
            StringBuilder csb = new StringBuilder(256);
            GetClassName(hWnd, csb, csb.Capacity);
            return csb.ToString();
        }

        public static bool GetWindowZOrder(IntPtr hwnd, out int zOrder)
        {
            const uint GW_HWNDPREV = 3;
            const uint GW_HWNDLAST = 1;

            var lowestHwnd = GetWindow(hwnd, GW_HWNDLAST);

            var z = 0;
            var hwndTmp = lowestHwnd;
            while (hwndTmp != IntPtr.Zero)
            {
                if (hwnd == hwndTmp)
                {
                    zOrder = z;
                    return true;
                }

                hwndTmp = GetWindow(hwndTmp, GW_HWNDPREV);
                z++;
            }

            zOrder = int.MinValue;
            return false;
        }

        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        //[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

        [System.Runtime.InteropServices.DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        public static IntPtr WindowFromPoint2(int x, int y)
        {
            return WindowFromPoint(x, y);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static string GetWindowText(IntPtr hWnd)
        {
            int textLen = GetWindowTextLength(hWnd);
            if (textLen == 0)
            {
                return "";
            }
            //ウィンドウのタイトルを取得する
            StringBuilder tsb = new StringBuilder(textLen + 1);
            GetWindowText(hWnd, tsb, tsb.Capacity);
            return tsb.ToString();
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        //[DllImport("user32.dll", EntryPoint = "GetWindowText",
        //    ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction,
            IntPtr lParam);

        //public static List<DesktopWindow> GetDesktopWindows()
        //{
        //    var collection = new List<DesktopWindow>();
        //    EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
        //    {
        //        var result = new StringBuilder(255);
        //        GetWindowText(hWnd, result, result.Capacity + 1);
        //        string title = result.ToString();

        //        var isVisible = !string.IsNullOrEmpty(title) && IsWindowVisible(hWnd);

        //        GetWindowThreadProcessId(hWnd, out int id);

        //        collection.Add(new DesktopWindow { Id = id, Handle = hWnd, Title = title, IsVisible = isVisible });

        //        return true;
        //    };

        //    EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero);
        //    return collection;
        //}

    }
}
