using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace SoundSwitcher.Helpers;

public enum TaskbarPosition
{
    Left = 0,
    Top = 1,
    Right = 2,
    Bottom = 3,
    Unknown = 4
}

public static partial class TaskbarHelper
{
    private const int ABM_GETTASKBARPOS = 0x00000005;

    [LibraryImport("shell32.dll", SetLastError = true)]
    private static partial IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public static TaskbarPosition GetTaskbarPosition()
    {
        APPBARDATA data = new APPBARDATA();
        data.cbSize = Marshal.SizeOf<APPBARDATA>();

        IntPtr result = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

        if (result != IntPtr.Zero)
        {
            return (TaskbarPosition)data.uEdge;
        }

        // Fallback
        if (SystemParameters.WorkArea.Top > 0) return TaskbarPosition.Top;
        if (SystemParameters.WorkArea.Left > 0) return TaskbarPosition.Left;

        return SystemParameters.WorkArea.Right < SystemParameters.PrimaryScreenWidth
            ? TaskbarPosition.Right
            : TaskbarPosition.Bottom;
    }
}
