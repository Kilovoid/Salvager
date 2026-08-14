using Salvager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Salvager.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private Note _currentPage;

        private string originalTitle = "New Note";
        private string originalContent = "";

        public EditorViewModel(Note page)
        {
            CurrentPage = page;
        }

        private void OnNotePropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Note.Title) || args.PropertyName == nameof(Note.Content))
            {
                CurrentPage.IsDirty = true;
            }
        }

        partial void OnCurrentPageChanged(Note oldVal, Note newVal)
        {
            if (oldVal != null)
            {
                oldVal.PropertyChanged -= OnNotePropertyChanged;
            }
            if (newVal != null)
            {
                newVal.PropertyChanged += OnNotePropertyChanged;
            }
        }
    }
}
