using Salvager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Salvager.Services
{
    internal class NoteService : INoteService
    {
        private readonly string _notesDirectory;

        public NoteService()
        {
            _notesDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Data", "Notes"
                );
        }
        public Note CreateNote(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("Title cannot be null", nameof(title));
            }
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                title = title.Replace(c, '_');
            }
            Note newNote = new Note ///Возможные ошибки - пустой заголовок, еще невалидные знаки! | СДЕЛАНО
            {
                Title = title,
                Content = ""
            };
            return newNote;
        }

        public void DeleteNote(Guid noteId)
        {
            if (string.IsNullOrEmpty(noteId.ToString()))
            {
                throw new ArgumentNullException($"Id ({noteId}) cannot be null!");
            }
            string fileName = (noteId + ".md");
            string filePath = Path.Combine(_notesDirectory, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filePath} is not found!");
            }
            File.Delete(filePath);
        }

        public List<Note> LoadAll()
        {
            List<Note> notesToLoad = new List<Note>();
            if (!Directory.Exists(_notesDirectory))
            {
                Directory.CreateDirectory(_notesDirectory);
                return new List<Note>();
            }

            string[] mdFiles = Directory.GetFiles(_notesDirectory, "*.md");
            foreach (string mdFile in mdFiles)
            {
                using var reader = new StreamReader(mdFile);
                string title = reader.ReadLine() ?? "Untitled";
                string content = reader.ReadToEnd();
                Note loadedNote = new Note(title, content);
                notesToLoad.Add(loadedNote);
            }
            return notesToLoad;
        }

        public Note LoadNote()
        {
            throw new NotImplementedException();
        }

        public void SaveNote()
        {

        }
    }
}
