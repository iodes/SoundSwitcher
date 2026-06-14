using System.IO;
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

            if (cleanPath.StartsWith('@'))
                cleanPath = cleanPath[1..];

            int commaIndex = cleanPath.LastIndexOf(',');

            if (commaIndex == -1)
                return null;

            string filePath = cleanPath[..commaIndex];
            string indexStr = cleanPath[(commaIndex + 1)..];

            filePath = Environment.ExpandEnvironmentVariables(filePath);

            if (!File.Exists(filePath))
                return null;

            if (!int.TryParse(indexStr, out int iconIndex))
                return null;

            // nIconSize combines width (LOWORD) and height (HIWORD)
            // 32 for large icon, 16 for small icon
            const uint nIconSize = (16u << 16) | 32u;

            int hresult = Windows.Win32.PInvoke.SHDefExtractIcon(filePath, iconIndex, 0, out var hIconLarge, out var hIconSmall, nIconSize);

            if (hresult != 0)
                return null;

            ImageSource? resultSource = null;

            if (hIconLarge is { IsInvalid: false })
            {
                resultSource = Imaging.CreateBitmapSourceFromHIcon(
                    hIconLarge.DangerousGetHandle(),
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                resultSource.Freeze(); // Make it cross-thread accessible
                hIconLarge.Dispose();
            }

            if (hIconSmall is { IsInvalid: false })
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
