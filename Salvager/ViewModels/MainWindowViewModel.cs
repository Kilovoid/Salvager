using Salvager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using System.Xml.Serialization;
using Salvager.Services;
using System.Linq;


namespace Salvager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly INoteService _noteService;
        public ObservableCollection<Note> Notes { get; set; } = new();

        [ObservableProperty]
        private Note? _selectedNote;

        [ObservableProperty]
        private EditorViewModel? _selectedNoteViewModel;

        public MainWindowViewModel(INoteService noteService)
        {
            _noteService = noteService;
            LoadNotesFromDisk();
        }

        private void LoadNotesFromDisk()
        {
            var loadedNotes = _noteService.LoadAll();
            Notes.Clear();
            foreach (var note in loadedNotes)
            {
                Notes.Add(note);
            }
            SelectedNote = Notes.FirstOrDefault();
            SelectedNoteViewModel = SelectedNote != null
                ? new EditorViewModel(SelectedNote)
                : null;
        }

        [RelayCommand]
        private void CreateNewNote()
        {
            var newNote = new Note("New Note", "");
            CreateNewNote(newNote);
        }
        private void CreateNewNote(Note note)
        {
            Notes.Add(note);
            _noteService.SaveNote(note);
            SelectedNote = note;
            SelectedNoteViewModel = new EditorViewModel(note);
        }
        [RelayCommand]
        private void SaveNote()
        {
            if (SelectedNote == null) return;
            _noteService.SaveNote(SelectedNote);
        }
        [RelayCommand]
        private void DeleteNote()
        {
            if (SelectedNote == null) return;
            _noteService.DeleteNote(SelectedNote.Id);
            Notes.Remove(SelectedNote);
            SelectedNote = Notes.FirstOrDefault();
            SelectedNoteViewModel = SelectedNote != null
                ? new EditorViewModel(SelectedNote)
                : null;
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
