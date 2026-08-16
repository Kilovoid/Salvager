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
}