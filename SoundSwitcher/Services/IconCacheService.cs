using System.IO;

namespace SoundSwitcher.Services;

public static class IconCacheService
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SoundSwitcher");

    private static readonly string IconsFolder = Path.Combine(SettingsFolder, "icons");

    public static string CacheIcon(string sourceFilePath)
    {
        Directory.CreateDirectory(IconsFolder);
        
        string ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (ext != ".ico" && ext != ".png" && ext != ".jpg" && ext != ".jpeg")
        {
            throw new ArgumentException("Unsupported image format. Only ICO, PNG, and JPEG are supported.");
        }

        string fileName = $"{Guid.NewGuid()}{ext}";
        string destFilePath = Path.Combine(IconsFolder, fileName);

        File.Copy(sourceFilePath, destFilePath, true);

        return fileName;
    }

    public static string? GetIconFullPath(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        string safeFileName = Path.GetFileName(fileName);
        string fullPath = Path.Combine(IconsFolder, safeFileName);

        return File.Exists(fullPath) ? fullPath : null;
    }

    public static void DeleteIcon(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        string safeFileName = Path.GetFileName(fileName);
        string fullPath = Path.Combine(IconsFolder, safeFileName);

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Ignore errors if file is locked or doesn't exist
        }
    }
}
