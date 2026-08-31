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

        private static readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        private static readonly ISerializer _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        public NoteService(string? customDirectory)
        {
            _notesDirectory = customDirectory ??
                Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Salvager",
                "Notes"
                );
            Directory.CreateDirectory(_notesDirectory);
            MigrateOldFiles();
        }

        public NoteService() : this(null) { }

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
            char[] invalidChars = Path.GetInvalidFileNameChars();
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
            string newFilePath = Path.Combine(_notesDirectory, newFileName);

            if (oldFilePath != null && oldFilePath != newFilePath)
            {
                File.Delete(oldFilePath);
            }

            if (File.Exists(newFilePath) && oldFilePath != newFilePath)
            {
                newFileName = GetUniqueFileName(sanitized);
                newFilePath = Path.Combine(_notesDirectory, newFileName);
                currentNote.Title = Path.GetFileNameWithoutExtension(newFilePath);
            }

            string fileContent = BuildFileContent(currentNote);
            File.WriteAllText(newFilePath, fileContent, Encoding.UTF8);
        }

        public void DeleteNote(Guid noteId)
        {
            if (noteId == Guid.Empty)
            {
                throw new ArgumentNullException($"Id ({noteId}) cannot be null!");
            }
            if (!Directory.Exists(_notesDirectory))
            {
                throw new DirectoryNotFoundException($"Directory {_notesDirectory} does not exist");
            }
            string? targetFile = null;
            foreach (string filePath in Directory.GetFiles(_notesDirectory, "*.md"))
            {
                try
                {
                    string content = File.ReadAllText(filePath);
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
                File.Delete(targetFile);
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
            if (!Directory.Exists(_notesDirectory))
            {
                Directory.CreateDirectory(_notesDirectory);
                return new List<Note>();
            }

            string[] mdFiles = Directory.GetFiles(_notesDirectory, "*.md");
            foreach (string mdFile in mdFiles)
            {
                try
                {
                    string content = File.ReadAllText(mdFile);
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
            ///Выбрасываем ошибку если
            ///1.Нельзя читать файл или директорию (СДЕЛАНО)
            ///2.Пустой переданный id (СДЕЛАНО)
            ///3.Не существует директории (СДЕЛАНО)
            ///4.Не существует файла с данным id (СДЕЛАНО)
            ///5.Не получается запарсить содержимое через ParseFileContent (СДЕЛАНО)
            if (noteId == Guid.Empty)
            {
                //Такое надо пробросить до ViewModel т.к. надо вывести предупреждение
                throw new ArgumentNullException("Guid cannot be null", nameof(noteId)); 
            }
            if (!Directory.Exists(_notesDirectory))
            {
                throw new DirectoryNotFoundException($"Directory {_notesDirectory} does not exist");
            }
            foreach(string filePath in Directory.GetFiles(_notesDirectory, "*.md"))
            {
                try
                {
                    string content = File.ReadAllText(filePath);
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
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(c, '_');
            }
            return title;
        }

        private string? FindNoteFileById(Guid noteId)
        {
            foreach (string filePath in Directory.GetFiles(_notesDirectory, "*.md"))
            {
                try
                {
                    string content = File.ReadAllText(filePath);
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
                string content = File.ReadAllText(filePath);
                var (note, _) = ParseFileContent(content);
                return note;
            }
            catch { return null; }
        }

        private void MigrateOldFiles()
        {
            foreach (string filePath in Directory.GetFiles(_notesDirectory, "*.md"))
            {
                string content = File.ReadAllText(filePath);
                if (content.TrimStart().StartsWith("---"))
                {
                    continue;
                }

                using var reader = new StringReader(content);
                string title = reader.ReadLine() ?? "Untitled";
                string body = reader.ReadToEnd();
                var note = new Note(Guid.NewGuid(), title, body, DateTime.Now, DateTime.Now);
                string newContent = BuildFileContent(note);

                File.WriteAllText(filePath, newContent, Encoding.UTF8);
            }
        }

        private string GetUniqueFileName(string baseName)
        {
            int counter = 1;
            string candidate = baseName;
            while (File.Exists(Path.Combine(_notesDirectory, candidate+".md")))
            {
                candidate = $"{baseName} ({counter++})";
            }
            return candidate + ".md";
        }
    }
}
