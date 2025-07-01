using System.Globalization;
using System.Resources;
using ReactiveUI;

namespace AvaLauncherStalker;

public class LocalizationBoss :ReactiveObject
{
    private static readonly ResourceManager resourceManager = new ResourceManager("AvaLauncherStalker.Localization", typeof(LocalizationBoss).Assembly);
    private CultureInfo _currentCulture = new CultureInfo("ru-RU");
    public static LocalizationBoss Instance { get; } = new LocalizationBoss();
    public string this[string key] => resourceManager.GetString(key, _currentCulture) ?? key;

    public void SetLanguage(string languageCode)
    {
        _currentCulture = new CultureInfo(languageCode);
        this.RaisePropertyChanged(string.Empty);
        var settings = MainWindow.SettingsApp.Load();
        settings.LanguageCode = languageCode;
        MainWindow.SettingsApp.Save(settings);
    }
}