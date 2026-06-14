using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace SoundSwitcher.Localization
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        public static LocalizationManager Instance { get; } = new();

        private CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");

        private static readonly ResourceManager ResourceManager =
            new("SoundSwitcher.Localization.Strings", typeof(LocalizationManager).Assembly);

        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key] =>
            ResourceManager.GetString(key, _culture) ?? key;

        public void SetLanguage(string code)
        {
            _culture = code switch
            {
                "ko-KR" => CultureInfo.GetCultureInfo("ko-KR"),
                "ja-JP" => CultureInfo.GetCultureInfo("ja-JP"),
                "zh-CN" => CultureInfo.GetCultureInfo("zh-CN"),
                "zh-TW" => CultureInfo.GetCultureInfo("zh-TW"),
                "ko" => CultureInfo.GetCultureInfo("ko-KR"),
                "ja" => CultureInfo.GetCultureInfo("ja-JP"),
                "zh" => CultureInfo.GetCultureInfo("zh-CN"),
                _ => CultureInfo.GetCultureInfo("en-US")
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
        }

        public void ApplyFromSettings(string savedCode)
        {
            if (string.IsNullOrEmpty(savedCode))
            {
                savedCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                {
                    "ko" => "ko-KR",
                    "ja" => "ja-JP",
                    "zh" => CultureInfo.CurrentUICulture.Name == "zh-TW" ? "zh-TW" : "zh-CN",
                    _ => "en-US"
                };
            }
            SetLanguage(savedCode);
        }
    }
}
