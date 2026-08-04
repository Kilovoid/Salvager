using Salvager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Salvager.Services
{
    internal class NoteService : INoteService
    {
        private readonly string _notesDirectory;

        public NoteService()
        {
            _notesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Notes");
            if (!Directory.Exists(_notesDirectory))
            {
                Directory.CreateDirectory(_notesDirectory);
            }
        }
        public void DeleteNote(Note note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            if (string.IsNullOrEmpty(note.Title)) throw new ArgumentException("Title cannot be null!");
            string fileName = note.Title + ".md";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            string filePath = Path.Combine(_notesDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public List<Note> LoadAllNotes()
        {
            var notes = new List<Note>();
            var files = Directory.GetFiles(_notesDirectory, ".md");
            foreach (var file in files)
            {
                try
                {
                    string content = File.ReadAllText(file);
                    string title = Path.GetFileNameWithoutExtension(file);
                    var note = new Note
                    {
                        Title = title,
                        Content = content,
                        CreatedAt = File.GetCreationTime(file)
                    };
                    notes.Add(note);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading file {file} : {ex.Message}");
                }
            }
            return notes;
        }

        public void SaveAll(List<Note> notes)
        {
            foreach (var note in notes)
            {
                SaveNote(note);
            }
        }

        public void SaveNote(Note note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            if (string.IsNullOrEmpty(note.Title)) throw new ArgumentException("Title can't be null");
            string fileName = note.Title + ".md";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            string filePath = Path.Combine(_notesDirectory, fileName);
            File.WriteAllText(filePath, note.Content);
        }
    }
}
