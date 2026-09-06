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
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.IO;
using System.Net.Http.Headers;


namespace Salvager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly INoteService _noteService;
        private readonly IDialogueService _dialogueService;

        [ObservableProperty]
        private ObservableCollection<Note> _notes = new();

        [ObservableProperty]
        private bool _sortDescending = true;

        [ObservableProperty]
        private Note? _selectedNote;

        [ObservableProperty]
        private EditorViewModel? _selectedNoteViewModel;

        public MainWindowViewModel(INoteService noteService, IDialogueService dialogueService)
        {
            _noteService = noteService;
            _dialogueService = dialogueService;
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
        private async Task CreateNewNote(Note note)
        {
            if (string.IsNullOrWhiteSpace(note.Title))
            {
                await _dialogueService.ShowWarningAsync("Warning", "Check the title - it cannot be blank");
                return;
            }
            Notes.Add(note);
            _noteService.SaveNote(note);
            SelectedNote = note;
            SelectedNoteViewModel = new EditorViewModel(note);
        }
        [RelayCommand]
        private async Task SaveNote()
        {
            if (SelectedNote == null) return;
            try
            {
                _noteService.SaveNote(SelectedNote);
                SelectedNoteViewModel?.ResetSnapshot();
            }
            catch (ArgumentNullException ex)
            {
                await _dialogueService.ShowErrorAsync("Error", "Selected note is null");
                return;
            }
            catch (ArgumentException ex)
            {
                await _dialogueService.ShowErrorAsync("Error", $"Error in data : {ex.Message}");
                return;
            }
            catch (IOException ex)
            {
                await _dialogueService.ShowErrorAsync("Error",
                    $"Unable to save the note, please check the directory and disk :: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                await _dialogueService.ShowErrorAsync("Error", $"Unknown Error, please restart the app!");
                return;
            }
        }
        [RelayCommand]
        private async Task DeleteNote()
        {
            if (SelectedNote == null) return;
            ButtonResult result = await _dialogueService.ShowWarningAsync("Warning",
                "Are you sure you want to delete this note?",
                ButtonEnum.OkCancel);

            if (result == ButtonResult.Cancel)
            {
                return;
            }
            try
            {
                _noteService.DeleteNote(SelectedNote.Id);
                Notes.Remove(SelectedNote);
                SelectedNote = Notes.FirstOrDefault();
                SelectedNoteViewModel = SelectedNote != null
                    ? new EditorViewModel(SelectedNote)
                    : null;
            }
            catch (ArgumentNullException ex)
            {
                await _dialogueService.ShowErrorAsync("Error", $"{ex.Message}");
                return;
            }
            catch (ArgumentException ex)
            {
                await _dialogueService.ShowErrorAsync("Error", $"{ex.Message}");
                return;
            }

        }

        [RelayCommand]
        private void DoSort()
        {
            var selected = SelectedNote;
            System.Diagnostics.Debug.WriteLine($"Before sort: {Notes.Count}");
            var sorted = SortDescending
                ? Notes.OrderByDescending(n => n.UpdatedAt).ToList()
                : Notes.OrderBy(n => n.UpdatedAt).ToList();
            for (int i =0; i < sorted.Count; i++)
            {
                var note = sorted[i];
                int currentIndex = Notes.IndexOf(note);
                if (currentIndex != i)
                {
                    Notes.Move(currentIndex, i);
                }
            }
            if (selected != null && Notes.Contains(selected))
            {
                SelectedNote = selected;
            }
        }

        [RelayCommand]
        private void Exit()
        {
            Environment.Exit(0);
        }

        [RelayCommand]
        private void GenerateError()
        {
            throw new InvalidOperationException("Test of the exception from app.axaml.cs");
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
