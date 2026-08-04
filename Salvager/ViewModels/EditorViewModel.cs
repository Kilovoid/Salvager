using Salvager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private Note _currentPage;

        public EditorViewModel(Note page)
        {
            CurrentPage = page;
        }
    }
}
