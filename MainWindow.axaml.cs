using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using MsBox.Avalonia;

namespace AvaLauncherStalker;

public partial class MainWindow : Window
{
    public ObservableCollection<News> NewsList { get; set; }
    public LocalizationBoss Localization => LocalizationBoss.Instance;
    public MainWindow()
    {
        /*if (File.Exists("news.json"))
        {
            string json = File.ReadAllText("news.json");
            NewsList = JsonSerializer.Deserialize<ObservableCollection<News>>(json) ?? new ObservableCollection<News>();
        }
        else
        {
            NewsList.Add(new News { Title = "Ошибка", Content = "Файл news.json не найден" });
        }*/
        string? json = null;

// 1. Loading from a disk nearby .exe
        string diskPath = Path.Combine(AppContext.BaseDirectory, "news.json");
        if (File.Exists(diskPath))
        {
            json = File.ReadAllText(diskPath);
        }
        else
        {
            // 2. Loading from an embedded resource
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("AvaLauncherStalker.news.json");
            if (stream != null)
                using (var reader = new StreamReader(stream))
                    json = reader.ReadToEnd();
        }

        if (json != null)
        {
            NewsList = JsonSerializer.Deserialize<ObservableCollection<News>>(json) ?? new ObservableCollection<News>();
        }
        else
        {
            NewsList.Add(new News { Title = "Ошибка", Content = "news.json не найден ни на диске, ни в ресурсах" });
        }
        
        InitializeComponent();
        DataContext = this;
        RenderList();
        ShadowMapList();
        ResolutionList();
    }

    private void RenderList()
    {
        RenderComboBox.ItemsSource = new List<string>
        {
            "DirectX 8",
            "DirectX 9",
            "DirectX 11"
        };
        RenderComboBox.SelectedIndex = 0;
    }
    
    private void ShadowMapList()
    {
        ShadowMapComboBox.ItemsSource = new List<string>
        {
            "1536",
            "2048",
            "2560",
            "3072",
            "4096"
        };
        ShadowMapComboBox.SelectedIndex = 0;
    }
    
    private void ResolutionList()
    {
        ResolutionComboBox.ItemsSource = WindowManager.GetSupportedResolutions();
        ResolutionComboBox.SelectedIndex = 0;
    }
    
    public class News
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
    
    private void SetRussianLanguage(object sender, RoutedEventArgs e)
    {
        Localization.SetLanguage("ru-RU");
    }

    private void SetEnglishLanguage(object sender, RoutedEventArgs e)
    {
        Localization.SetLanguage("en-US");
    }
    
    private async void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        string launcherPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string gameDirectory = launcherPath;
        string renderer = RenderComboBox.SelectedItem?.ToString();
        bool useAvx = AvxCheckBox.IsChecked == true;
        string exeName = renderer switch
        {


            "DirectX 8" => useAvx ? "AnomalyDX8AVX.exe" : "AnomalyDX8.exe",
            "DirectX 9" => useAvx ? "AnomalyDX9AVX.exe" : "AnomalyDX9.exe",
            "DirectX 10" => useAvx ? "AnomalyDX10AVX.exe" : "AnomalyDX10.exe",
            "DirectX 11" => useAvx ? "AnomalyDX11AVX.exe" : "AnomalyDX11.exe",
            _ => "AnomalyDX9.exe" // Default to DirectX 9 if no renderer is selected
        };
        string exePath = Path.Combine(gameDirectory, "bin", exeName);
        var args = new List<string>();
        string resolution = ResolutionComboBox.SelectedItem?.ToString();
        string userLtxPath = Path.Combine(gameDirectory, "appdata", "user.ltx");
        if (!string.IsNullOrWhiteSpace(resolution) && File.Exists(userLtxPath))
        {
            try
            {
                string[] lines = File.ReadAllLines(userLtxPath);
                bool vidModeFound = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("vid_mode"))
                    {
                        lines[i] = $"vid_mode {resolution}";
                        vidModeFound = true;
                        break;
                    }
                }
                if (!vidModeFound)
                {
                    Array.Resize(ref lines, lines.Length + 1);
                    lines[lines.Length - 1] = $"vid_mode {resolution}";
                }
                File.WriteAllLines(userLtxPath, lines);
            }
            catch (Exception ex)
            {
                var messageBox = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", $"Не удалось изменить user.ltx для установки разрешения: {ex.Message}");
                await messageBox.ShowAsync();
                return;
            }
        }


        string shadow = ShadowMapComboBox.SelectedItem?.ToString();
        if (!string.IsNullOrWhiteSpace(shadow))
            args.Add($"-smap{shadow}");
        
        if (DevCheckBox.IsChecked == true)
            args.Add("-dbg");
        
        if (ClearCacheCheckBox.IsChecked == true)
        {
            string shadersCachePath = Path.Combine(gameDirectory, "appdata", "shaders_cache");
            if (Directory.Exists(shadersCachePath))
            {
                try
                {
                    Directory.Delete(shadersCachePath, true); 
                    Directory.CreateDirectory(shadersCachePath); 
                }
                catch (Exception ex)
                {
                    var messageBox = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Не удалось очистить папку shaders_cache по пути: {shadersCachePath}\nОшибка: {ex.Message}");
                    await messageBox.ShowAsync();
                    return;
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(shadersCachePath);
                }
                catch (Exception ex)
                {
                    var messageBox = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Не удалось создать папку shaders_cache по пути: {shadersCachePath}\nОшибка: {ex.Message}");
                    await messageBox.ShowAsync();
                    return;
                }
            }
            args.Add("-nocache");
        }

        if (ReloadSoundsCheckBox.IsChecked == true)
            args.Add("-prefetch_sounds");
        
        if (DefaultUserLtxCheckBox.IsChecked == true)
        {
            if (File.Exists(userLtxPath))
            {
                try
                {
                    File.Delete(userLtxPath); 
                }
                catch (Exception ex)
                {
                    var messageBox = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Не удалось удалить файл user.ltx по пути: {userLtxPath}\nОшибка: {ex.Message}");
                    await messageBox.ShowAsync();
                    return;
                }
            }
        }
        
        try
        {
            var stalker = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = string.Join(" ", args),
                WorkingDirectory = gameDirectory
            };
            Process.Start(stalker);
        }
        catch (Exception ex)
        {
            var  messageBox = MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Произошла ошибка при запуске игры:\n{ex.Message}");
            var res = await messageBox.ShowAsync();

        }




            

    }


    
}