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

        private string _originalTitle = "New Note";
        private string _originalContent = "";

        public EditorViewModel(Note page)
        {
            CurrentPage = page;
            InitializeSnapshot();
            if (CurrentPage != null)
            {
                CurrentPage.PropertyChanged += OnNotePropertyChanged;
            }
        }

        private void InitializeSnapshot()
        {
            if (CurrentPage != null)
            {
                _originalTitle = CurrentPage.Title;
                _originalContent = CurrentPage.Content;
                CurrentPage.IsDirty = false;
            }
        }

        private void OnNotePropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (CurrentPage == null) return;

            if (args.PropertyName == nameof(Note.Title) || args.PropertyName == nameof(Note.Content))
            {
                bool isDirty = CurrentPage.Title != _originalTitle || CurrentPage.Content != _originalContent;
                CurrentPage.IsDirty = isDirty;
            }
        }

        public void ResetSnapshot()
        {
            if (CurrentPage != null)
            {
                _originalTitle = CurrentPage.Title;
                _originalContent = CurrentPage.Content;
                CurrentPage.IsDirty = false;
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
                InitializeSnapshot();
            }
        }
    }
}
