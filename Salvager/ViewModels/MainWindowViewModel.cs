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
        private async Task CreateNewNote(Note note)
        {
            if (string.IsNullOrWhiteSpace(note.Title))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Warning" ,"Check the title of the new note", ButtonEnum.Ok);
                await box.ShowAsync();
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
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error", "Not to save is null!", ButtonEnum.Ok);
                await box.ShowAsync();
            }
            catch (ArgumentException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"Error in data : {ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
            }
            catch (IOException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"Unable to save the note, please check the directory and disk :: {ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"Unknown Error, please restart the app!", ButtonEnum.Ok);
                await box.ShowAsync();
            }
            _noteService.SaveNote(SelectedNote);
        }
        [RelayCommand]
        private async Task DeleteNote()
        {
            if (SelectedNote == null) return;
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Warning", "Are you sure you want to delete this note?", ButtonEnum.OkCancel);
            ButtonResult result = await box.ShowAsync();
            if (result == ButtonResult.Cancel)
            {
                return;
            }
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
