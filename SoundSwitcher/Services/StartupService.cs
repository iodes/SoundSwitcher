namespace SoundSwitcher.Services;

/// <summary>
/// Service for registering/unregistering Windows startup programs.
/// </summary>
public static class StartupService
{
    private const string AppName = "SoundSwitcher";

    public static void SetAutoStartup(bool register)
    {
        var exePath = Environment.ProcessPath ?? string.Empty;

        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (key == null)
                return;

            if (register)
            {
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // Ignore registry access failures
        }
    }
}
