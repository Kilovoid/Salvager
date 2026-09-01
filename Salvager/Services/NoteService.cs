using Avalonia.Controls;
using Avalonia.Platform;
using Salvager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Salvager.Services
{
    public class NoteService : INoteService
    {
        private readonly string _notesDirectory;

        private readonly IFileSystem _fileSystem;

        private static readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        private static readonly ISerializer _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        public NoteService(IFileSystem fileSystem, string? customDirectory = null)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _notesDirectory = customDirectory ??
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Salvager",
                "Notes"
                );
            _fileSystem.CreateDirectory(_notesDirectory);
            MigrateOldFiles();
        }

        public NoteService() : this(new RealFileSystem(), null) { }
        public NoteService(string? customDirectory) : this(new RealFileSystem(), customDirectory) { }

        private string BuildFileContent(Note note)
        {
            var metadata = new NoteMetadata
            {
                Id = note.Id,
                Title = note.Title,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
            string frontmatter = _serializer.Serialize(metadata);
            return $"---\n{frontmatter}---\n\n{note.Content}";
        }
        private (Note, string contentBody) ParseFileContent(string fileContent)
        {
            int start = fileContent.IndexOf("---");
            if (start == -1) throw new InvalidDataException("No frontmatter found");

            int end = fileContent.IndexOf("---", start + 3);
            if (end == -1) throw new InvalidDataException("Unclosed frontmatter");

            string yaml = fileContent.Substring(start + 3, end - start - 3).Trim();
            string body = fileContent.Substring(end + 3).TrimStart('\n');

            var metadata = _deserializer.Deserialize<NoteMetadata>(yaml);
            var note = new Note(metadata.Id,
                metadata.Title, body, metadata.CreatedAt, metadata.UpdatedAt);

            return (note, body);
        }

        public Note CreateNote(string title)
        {

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("Title cannot be null", nameof(title));
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException("Title cannot be blank", nameof(title));
            }
            char[] invalidChars = _fileSystem.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                title = title.Replace(c, '_');
            }
            Note newNote = new Note(title, "");
            return newNote;
        }

        public void SaveNote(Note currentNote)
        {
            if (currentNote == null)
            {
                throw new ArgumentNullException(nameof(currentNote));
            }
            if (string.IsNullOrWhiteSpace(currentNote.Title))
            {
                throw new ArgumentException("Title cannot be blank!", nameof(currentNote));
            }
            if (string.IsNullOrEmpty(currentNote.Title))
            {
                throw new ArgumentException("Title cannot be empty!", nameof(currentNote));
            }
            if (currentNote.Id == Guid.Empty)
            {
                currentNote.Id = Guid.NewGuid();
            }
            currentNote.UpdatedAt = DateTime.Now;
            string? oldFilePath = FindNoteFileById(currentNote.Id);
            string sanitized = SanitizeName(currentNote.Title);
            string newFileName = sanitized + ".md";
            string newFilePath = _fileSystem.CombinePath(_notesDirectory, newFileName);

            if (oldFilePath != null && oldFilePath != newFilePath)
            {
                _fileSystem.DeleteFile(oldFilePath);
            }

            if (_fileSystem.FileExists(newFilePath) && oldFilePath != newFilePath)
            {
                newFileName = GetUniqueFileName(sanitized);
                newFilePath = _fileSystem.CombinePath(_notesDirectory, newFileName);
                currentNote.Title = _fileSystem.GetFileNameWithoutExtension(newFilePath);
            }

            string fileContent = BuildFileContent(currentNote);
            _fileSystem.WriteAllText(newFilePath, fileContent, Encoding.UTF8);
        }

        public void DeleteNote(Guid noteId)
        {
            if (noteId == Guid.Empty)
            {
                throw new ArgumentNullException($"Id ({noteId}) cannot be null!");
            }
            if (!_fileSystem.DirectoryExists(_notesDirectory))
            {
                throw new DirectoryNotFoundException($"Directory {_notesDirectory} does not exist");
            }
            string? targetFile = null;
            foreach (string filePath in _fileSystem.GetFiles(_notesDirectory, "*.md"))
            {
                try
                {
                    string content = _fileSystem.ReadAllText(filePath);
                    var (note, _) = ParseFileContent(content);
                    if (note.Id == noteId)
                    {
                        targetFile = filePath;
                        break;
                    }
                }
                catch (IOException ex)
                {
                    App.Log($"Cannot read file {filePath}: {ex.Message}");
                    throw;
                }
                catch (InvalidDataException ex)
                {
                    App.Log($"Invalid format in file {filePath}: {ex.Message}");
                    throw;
                }
            }
            if (targetFile == null)
            {
                throw new FileNotFoundException($"Note with ID {noteId} is not found");
            }
            
            try
            {
                _fileSystem.DeleteFile(targetFile);
            }
            catch (IOException ex)
            {
                App.Log($"Failed to delete file {targetFile} : {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                App.Log($"Access denied to file {targetFile} : {ex.Message}");
                throw;
            }
        }

        public List<Note> LoadAll()
        {
            List<Note> notesToLoad = new List<Note>();
            if (!_fileSystem.DirectoryExists(_notesDirectory))
            {
                _fileSystem.CreateDirectory(_notesDirectory);
                return new List<Note>();
            }

            string[] mdFiles = _fileSystem.GetFiles(_notesDirectory, "*.md");
            foreach (string mdFile in mdFiles)
            {
                try
                {
                    string content = _fileSystem.ReadAllText(mdFile);
                    var (note, _) = ParseFileContent(content);
                    notesToLoad.Add(note);
                }
                catch (UnauthorizedAccessException ex)
                {
                    App.Log($"Can't access file {mdFile} : {ex.Message}");
                } 
                catch (FileNotFoundException ex)
                {
                    App.Log($"File {mdFile} has not been found {ex.Message}");
                }
                catch (Exception ex)
                {
                    App.Log($"Error loading {mdFile} : {ex.Message}");
                }
            }
            return notesToLoad;
        }

        public Note LoadNote(Guid noteId)
        {
            if (noteId == Guid.Empty)
            {
                throw new ArgumentNullException("Guid cannot be null", nameof(noteId)); 
            }
            if (!_fileSystem.DirectoryExists(_notesDirectory))
            {
                throw new DirectoryNotFoundException($"Directory {_notesDirectory} does not exist");
            }
            foreach(string filePath in _fileSystem.GetFiles(_notesDirectory, "*.md"))
            {
                try
                {
                    string content = _fileSystem.ReadAllText(filePath);
                    var (note, _) = ParseFileContent(content);
                    if (note.Id == noteId)
                    {
                        return note;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    App.Log($"Can't access the file {filePath} : {ex.Message}");
                }
                catch (Exception ex)
                {
                    App.Log($"Unable to load note {filePath} : {ex.Message}");
                }
            }
            throw new FileNotFoundException($"Note with ID {noteId} does not exist");
        }

        private string SanitizeName(string title)
        {
            foreach (char c in _fileSystem.GetInvalidFileNameChars())
            {
                title = title.Replace(c, '_');
            }
            return title;
        }

        private string? FindNoteFileById(Guid noteId)
        {
            foreach (string filePath in _fileSystem.GetFiles(_notesDirectory, "*.md"))
            {
                try
                {
                    string content = _fileSystem.ReadAllText(filePath);
                    var (note, _) = ParseFileContent(content);
                    if (note.Id == noteId)
                    {
                        return filePath;
                    }
                }
                catch { }
            }
            return null;
        }

        private Note? LoadNoteByFilePath(string filePath)
        {
            try
            {
                string content = _fileSystem.ReadAllText(filePath);
                var (note, _) = ParseFileContent(content);
                return note;
            }
            catch { return null; }
        }

        private void MigrateOldFiles()
        {
            foreach (string filePath in _fileSystem.GetFiles(_notesDirectory, "*.md"))
            {
                string content = _fileSystem.ReadAllText(filePath);
                if (content.TrimStart().StartsWith("---"))
                {
                    continue;
                }

                using var reader = new StringReader(content);
                string title = reader.ReadLine() ?? "Untitled";
                string body = reader.ReadToEnd();
                var note = new Note(Guid.NewGuid(), title, body, DateTime.Now, DateTime.Now);
                string newContent = BuildFileContent(note);

                _fileSystem.WriteAllText(filePath, newContent, Encoding.UTF8);
            }
        }

        private string GetUniqueFileName(string baseName)
        {
            int counter = 1;
            string candidate = baseName;
            while (_fileSystem.FileExists(_fileSystem.CombinePath(_notesDirectory, candidate+".md")))
            {
                candidate = $"{baseName} ({counter++})";
            }
            return candidate + ".md";
        }
    }
}
