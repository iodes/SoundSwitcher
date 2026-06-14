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

            Windows.Win32.DestroyIconSafeHandle hIconLarge;
            Windows.Win32.DestroyIconSafeHandle hIconSmall;

            // nIconSize combines width (LOWORD) and height (HIWORD)
            // 32 for large icon, 16 for small icon
            uint nIconSize = (16u << 16) | 32u;
            
            int hresult = Windows.Win32.PInvoke.SHDefExtractIcon(filePath, iconIndex, 0, out hIconLarge, out hIconSmall, nIconSize);
            
            if (hresult != 0)
                return null;

            ImageSource? resultSource = null;
            if (hIconLarge != null && !hIconLarge.IsInvalid)
            {
                resultSource = Imaging.CreateBitmapSourceFromHIcon(
                    hIconLarge.DangerousGetHandle(),
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                
                resultSource.Freeze(); // Make it cross-thread accessible
                hIconLarge.Dispose();
            }

            if (hIconSmall != null && !hIconSmall.IsInvalid)
            {
                hIconSmall.Dispose();
            }

            return resultSource;
        }
        catch
        {
            return null;
        }
    }
}
