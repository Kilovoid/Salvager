using CommunityToolkit.Mvvm.ComponentModel;

namespace Salvager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Salvager!";
}
