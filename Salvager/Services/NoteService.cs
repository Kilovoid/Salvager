using Avalonia.Platform;
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
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Salvager",
                "Notes"
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
            Note newNote = new Note(title, "");
            return newNote;
        }

        public void DeleteNote(Guid noteId)
        {
            if (noteId == Guid.Empty)
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
                string fileName = Path.GetFileNameWithoutExtension(mdFile);
                if (!Guid.TryParse(fileName, out Guid noteId))
                {
                    continue;
                }
                using var reader = new StreamReader(mdFile);
                string title = reader.ReadLine() ?? "Untitled";
                string content = reader.ReadToEnd();
                Note loadedNote = new Note(noteId, title, content, File.GetCreationTime(mdFile));
                notesToLoad.Add(loadedNote);
            }
            return notesToLoad;
        }

        public Note LoadNote(Guid noteId)
        {
            string fileName = noteId + ".md";
            string[] matchingFiles = Directory.GetFiles(_notesDirectory, fileName);
            if (matchingFiles.Length == 0)
            {
                throw new FileNotFoundException($"Note with id {noteId} is not found in {_notesDirectory}");
            }
            string filePath = matchingFiles[0];
            using var reader = new StreamReader(filePath);
            string title = reader.ReadLine() ?? "Untitled";
            string content = reader.ReadToEnd();
            Note loadedNote = new Note(noteId, title, content, File.GetCreationTime(filePath));
            return loadedNote;
        }
        public void SaveNote(Note currentNote)
        {
            if (currentNote == null)
            {
                throw new ArgumentNullException(nameof(currentNote));
            }
            if (currentNote.Id == Guid.Empty)
            {
                currentNote.Id = Guid.NewGuid();
            }
            string title = currentNote.Title;
            string content = currentNote.Content;
            string noteContents = $"{title}\n{content}";

            string fileName = $"{currentNote.Id}.md";
            string filePath = Path.Combine(_notesDirectory, fileName);
            File.WriteAllText(filePath, noteContents, Encoding.UTF8);
        }
    }
}
