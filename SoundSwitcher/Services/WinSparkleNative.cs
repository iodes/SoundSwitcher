using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SoundSwitcher.Services
{
    internal static partial class WinSparkleNative
    {
        private const string DllName = "WinSparkle.dll";

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void win_sparkle_init();

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void win_sparkle_cleanup();

        [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void win_sparkle_set_appcast_url(string url);

        [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void win_sparkle_set_app_details(
            string company_name, string app_name, string app_version);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void win_sparkle_check_update_with_ui();

        [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        internal static partial void win_sparkle_set_eddsa_public_key(string pub_key);
    }
}
