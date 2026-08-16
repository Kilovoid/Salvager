using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Salvager.ViewModels;

namespace Salvager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var focused = FocusManager.GetFocusedElement();
        if (focused is TextBox)
        {
            if(e.Key == Key.LeftAlt)
            {
                e.Handled = true;
            }
        }
    }
    
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.FullScreen;
    }
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}