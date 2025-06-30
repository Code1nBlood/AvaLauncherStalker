using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Interactivity;

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
    }

    private void RenderList()
    {
        RenderComboBox.ItemsSource = new List<string>
        {
            "DirectX 9",
            "DirectX 11",
            "DirectX 12"
        };
        RenderComboBox.SelectedIndex = 0;
    }
    
    private void ShadowMapList()
    {
        ShadowMapComboBox.ItemsSource = new List<string>
        {
            "2048",
            "4096",
            "8192"
        };
        ShadowMapComboBox.SelectedIndex = 0;
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


    
}