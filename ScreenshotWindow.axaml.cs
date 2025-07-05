using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace AvaLauncherStalker;

public partial class ScreenshotWindow : Window
{
    public ObservableCollection<Bitmap> Screenshots { get; set; } = new();
    public ScreenshotWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadScreenshots();
        /*LoadDummyImages();*/
    }
    
    /*private void LoadDummyImages()
    {

        try
        {
            Screenshots.Add(new Bitmap("images/maxresdefault.jpg"));
            Screenshots.Add(new Bitmap("images/maxresdefault.jpg"));
            Screenshots.Add(new Bitmap("images/maxresdefault.jpg"));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading dummy images: {e.Message}");
        }
    }*/
    
    private void LoadScreenshots()
    {
        string launcherPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
        string folder = Path.Combine(launcherPath, "appdata", "screenshots");



        if (!Directory.Exists(folder)) return;

        var files = Directory.GetFiles(folder, "*.jpg")
            .OrderByDescending(File.GetCreationTime)
            .Take(6);

        foreach (var file in files)
        {
            using var stream = File.OpenRead(file);
            Screenshots.Add(new Bitmap(stream));
        }
    }
    
    
    private void OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        string launcherPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
        string folder = Path.Combine(launcherPath, "appdata", "screenshots");

        if (Directory.Exists(folder))
            Process.Start(new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
    }
}