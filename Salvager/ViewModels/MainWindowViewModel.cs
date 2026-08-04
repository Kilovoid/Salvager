using Salvager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using System.Xml.Serialization;


namespace Salvager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<Note> Notes { get; set; } = new();

        [ObservableProperty]
        private Note _selectedNote;

        [ObservableProperty]
        private EditorViewModel _selectedNoteViewModel;

        public MainWindowViewModel()
        {
            var testNote = new Note { Title = "Hello", Content = "Waow" };
            Notes.Add(testNote);
            SelectedNote = testNote;
            SelectedNoteViewModel = new EditorViewModel(testNote);
        }
        [RelayCommand]
        private void CreateNewPage()
        {
            var newNote = new Note();
            Notes.Add(newNote);
            SelectedNote = newNote;
            SelectedNoteViewModel = new EditorViewModel(newNote);
        }

        partial void OnSelectedNoteChanged(Note value)
        {
            if (value != null)
            {
                SelectedNoteViewModel = new EditorViewModel(value);
            }
        }
    }
}
