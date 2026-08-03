using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Salvager.ViewModels
{
    internal class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<Page> Pages { get; set; } = new();

        [ObservableProperty]
        private Page _selected;

        public MainViewModel(){

        }
    }
}
