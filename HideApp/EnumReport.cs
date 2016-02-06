using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HideApp
{
    
    public delegate bool EnumWindowsCallBackPtr(IntPtr hwnd, int lParam);

    public class EnumReport
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsCallBackPtr callPtr, int lPar);
        [DllImport("user32.dll")]
        public static extern bool EnumChildWindows(IntPtr hwnd, EnumWindowsCallBackPtr func, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool CloseWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        public enum Msg: UInt32
        {

        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        public static bool Report(int hwnd, int lParam)
        {
            Console.WriteLine("Window handle is " + hwnd);
            return true;
        }

        public static List<int> IntPtrToIntList(List<IntPtr> p)
        {
            return p.ConvertAll(x => x.ToInt32());
        }
        public static IntPtr GetTopPtr(IEnumerable<IntPtr> p)
        {
            var pl = p.Select(x => x.ToInt32()).ToList();
            pl.Sort();
            return new IntPtr(pl[0]);
        }
    }

}
