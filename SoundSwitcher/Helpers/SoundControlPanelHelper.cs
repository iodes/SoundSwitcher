using System.Diagnostics;
using System.Windows;
using SoundSwitcher.Localization;

namespace SoundSwitcher.Helpers;

public static class SoundControlPanelHelper
{
    public static bool IsSoundPropertiesWindowOpen()
    {
        try
        {
            Process[] processes = Process.GetProcessesByName("rundll32");

            foreach (var p in processes)
            {
                try
                {
                    if (p.Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Contains("mmsys.cpl", StringComparison.OrdinalIgnoreCase)))
                        return true;
                }
                catch
                {
                    // Ignore exceptions (e.g., access denied, process exited)
                }
                finally
                {
                    p.Dispose();
                }
            }
        }
        catch
        {
            // Ignore overall exceptions
        }

        return false;
    }

    public static bool OpenDeviceProperties(string deviceId)
    {
        try
        {
            if (IsSoundPropertiesWindowOpen())
            {
                return false;
            }

            // Windows allows opening specific device properties by passing the device ID to mmsys.cpl via rundll32
            string arguments = $"shell32.dll,Control_RunDLL mmsys.cpl,,{deviceId},general";

            Process.Start(new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = arguments,
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            // Fallback to opening general sound settings if it fails
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mmsys.cpl",
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                // Ignore exceptions
            }

            return false;
        }
    }
}
