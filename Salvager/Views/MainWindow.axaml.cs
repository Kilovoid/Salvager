using Avalonia.Controls;
using Salvager.ViewModels;

namespace Salvager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnWindowClosed(object sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SaveAllNotes();
        }
    }
}