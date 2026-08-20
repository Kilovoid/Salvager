using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Salvager.ViewModels;
using Salvager.Views;
using Salvager.Services;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using System.IO;
using Avalonia.Threading;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace Salvager;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<INoteService, NoteService>();

        var provider = services.BuildServiceProvider();

        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = provider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log($"AppDomain Unhandled: {ex?.Message}\n{ex?.StackTrace}");
    }

    //private async Task OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    //{
    //    Log($"Dispatcher Unhandled: {e.Exception.Message}\n{e.Exception.StackTrace}");
    //    e.Handled = true;

    //    try
    //    {
    //        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
    //            && desktop.MainWindow != null)
    //        {
    //            var box = MessageBoxManager.GetMessageBoxStandard(
    //                "Error", "Unknown error occurred. Please restart the application", ButtonEnum.Ok);
    //            await box.ShowAsync();
    //        }
    //    }
    //    catch { }
    //}

    private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var ex = e.Exception;
        Log($"UI Exception: {ex.Message}\n{ex.StackTrace}");
        e.Handled = true;
        ShowError(ex.Message, ex.StackTrace).ConfigureAwait(false); 
    }

    private async Task ShowError(string message, string stackTrace)
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var details = $"Message: {message} \n\n Stack Trace: {stackTrace}";
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Critical error",
                    "Unknown error, please restart the program",
                    ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }
        catch { }
    }  

    public static void Log(string message)
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Salvager",
                "Logs"
                );
            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(logDir, $"error_{DateTime.Now:yyyy-MM-dd}.log");
            var entry = $"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(logFile, entry, Encoding.UTF8);
        }
        catch { }
    }
}