using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoundSwitcher.Helpers;

public static class NativeIconHelper
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHDefExtractIcon(
        string pszIconFile,
        int iIndex,
        uint uFlags,
        ref IntPtr phiconLarge,
        ref IntPtr phiconSmall,
        uint nIconSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    /// <summary>
    /// Parses a string like "@%windir%\system32\mmres.dll,-3012" and extracts the icon as an ImageSource.
    /// </summary>
    public static ImageSource? ExtractDeviceIcon(string? iconPathString)
    {
        if (string.IsNullOrWhiteSpace(iconPathString))
            return null;

        try
        {
            string cleanPath = iconPathString;
            if (cleanPath.StartsWith("@"))
                cleanPath = cleanPath.Substring(1);

            int commaIndex = cleanPath.LastIndexOf(',');
            if (commaIndex == -1)
                return null;

            string filePath = cleanPath.Substring(0, commaIndex);
            string indexStr = cleanPath.Substring(commaIndex + 1);

            filePath = Environment.ExpandEnvironmentVariables(filePath);
            if (!File.Exists(filePath))
                return null;

            if (!int.TryParse(indexStr, out int iconIndex))
                return null;

            IntPtr hIconLarge = IntPtr.Zero;
            IntPtr hIconSmall = IntPtr.Zero;

            // nIconSize combines width (LOWORD) and height (HIWORD)
            // 32 for large icon, 16 for small icon
            uint nIconSize = (16u << 16) | 32u;
            
            int hresult = SHDefExtractIcon(filePath, iconIndex, 0, ref hIconLarge, ref hIconSmall, nIconSize);
            
            if (hresult != 0)
                return null;

            ImageSource? resultSource = null;
            if (hIconLarge != IntPtr.Zero)
            {
                resultSource = Imaging.CreateBitmapSourceFromHIcon(
                    hIconLarge,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                
                resultSource.Freeze(); // Make it cross-thread accessible
                DestroyIcon(hIconLarge);
            }

            if (hIconSmall != IntPtr.Zero)
            {
                DestroyIcon(hIconSmall);
            }

            return resultSource;
        }
        catch
        {
            return null;
        }
    }
}
