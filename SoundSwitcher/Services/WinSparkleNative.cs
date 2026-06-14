using System;
using System.Runtime.InteropServices;

namespace SoundSwitcher.Services
{
    public static class WinSparkleNative
    {
        private const string DllName = "WinSparkle.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void win_sparkle_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void win_sparkle_cleanup();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void win_sparkle_set_appcast_url([MarshalAs(UnmanagedType.LPStr)] string url);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void win_sparkle_set_app_details(
            [MarshalAs(UnmanagedType.LPWStr)] string company_name, 
            [MarshalAs(UnmanagedType.LPWStr)] string app_name, 
            [MarshalAs(UnmanagedType.LPWStr)] string app_version);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void win_sparkle_check_update_with_ui();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void win_sparkle_set_eddsa_public_key([MarshalAs(UnmanagedType.LPStr)] string pub_key);
    }
}
