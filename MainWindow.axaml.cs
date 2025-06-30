using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        if (File.Exists("news.json"))
        {
            string json = File.ReadAllText("news.json");
            NewsList = JsonSerializer.Deserialize<ObservableCollection<News>>(json);
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