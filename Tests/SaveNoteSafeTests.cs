using Moq;
using Salvager.Abstractions;
using Salvager.Models;
using Salvager.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tests
{
    public class SaveNoteSafeTests
    {
        private readonly DateTime _defaultSavedAt = new DateTime(2026, 12, 12, 12, 12, 12);
        private readonly DateTime _defaultUpdatedAt = new DateTime(2027, 12, 12, 12, 12, 12);
        private readonly NoteService _service;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IClock> _mockClock;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly string _testRoot = Path.Combine(
            Path.GetTempPath(), "SalvagerTests");
        private readonly Dictionary<string, string> _pathAndContent;

        public SaveNoteSafeTests()
        {
            _pathAndContent = new Dictionary<string, string>();
            _mockFileSystem = new Mock<IFileSystem>();

            _mockFileSystem.Setup(fs => fs
                .ReadAllText(It.IsAny<string>())).Returns<string>(path => _pathAndContent[path]); // Вернуть содержимое типа string по ключу path
            _mockFileSystem.Setup(fs => fs
                .FileExists(It.IsAny<string>())).Returns<string>(path => _pathAndContent.ContainsKey(path)); // Вернуть true если в Dict есть path
            _mockFileSystem.Setup(fs => fs
                .GetFiles(It.IsAny<string>(), "*.md")).Returns(() => _pathAndContent.Keys
                    .Where(path => Path.GetExtension(path) == ".md")
                    .ToArray()); // не возвращает tmp например
            _mockFileSystem.Setup(fs => fs
                .DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs
                .CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>(
                (directory, file) => Path.Combine(directory, file));
            _mockFileSystem.Setup(fs => fs
                .GetInvalidFileNameChars()).Returns(Path.GetInvalidFileNameChars());
            _mockFileSystem.Setup(fs => fs
                .DeleteFile(It.IsAny<string>())).Callback<string>(path => _pathAndContent.Remove(path));
            _mockFileSystem.Setup(fs => fs
                .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()))
                    .Callback<string, string, Encoding>((path, content, encoding) => _pathAndContent[path] = content); // записываем у ключу какой-то контент
            _mockFileSystem.Setup(fs => fs
                .ReplaceFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((source, destination) =>
                {
                    if(source == destination)
                    {
                        throw new IOException("Source and destination paths must be different");
                    }
                    if(!_pathAndContent.ContainsKey(source))
                    {
                        throw new FileNotFoundException(null, source);
                    }
                    if(!_pathAndContent.ContainsKey(destination))
                    {
                        throw new FileNotFoundException(null, destination);
                    }
                    _pathAndContent[destination] = _pathAndContent[source];
                    _pathAndContent.Remove(source);
                });
            _mockFileSystem.Setup(fs => fs
                .MoveFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((source, destination) =>
                {
                    if (!_pathAndContent.ContainsKey(source))
                    {
                        throw new FileNotFoundException(null, source);
                    }
                    if (_pathAndContent.ContainsKey(destination))
                    {
                        throw new IOException("Destination already exists");
                    }
                    _pathAndContent[destination] = _pathAndContent[source];
                    _pathAndContent.Remove(source);
                });

            _mockLogger = new Mock<ILogger>();
            _mockClock = new Mock<IClock>();

            _mockClock.Setup(cl => cl
            .GetCurrentTime()).Returns(_defaultUpdatedAt);

            _service = new NoteService(
                _mockFileSystem.Object, _mockLogger.Object, _mockClock.Object, _testRoot);
        }
        [Fact]
        public void SaveNote_WhenWriteFailsWithoutRename_KeepsOldContent()
        {
            //Arrange: подготовить файл и объект заметки, изменить содержимое, создать ошибку записи
            //Act: вызвать SaveNote и поймать ошибку IOException
            //Assert: проверить, что DeleteNote не вызвался, старый файл все еще на месте, содержимое такое же

            //Для Arrange понадобится вызвать настоящий NoteService, фальшивый FileSystem

            //Чтобы проверять содержимое есть смысл создать некоторый словарь, который хранит как ключ путь, а содержимое как переменную.
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            string oldTitle = "OldTitle";
            string path = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[path] = oldFileContent;

            var newNote = new Note(id, "OldTitle", "New Content", savedAt, savedAt);

            _mockFileSystem.Setup(fs => fs
                .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()))
                    .Callback<string, string, Encoding>((path, content, encoding) =>
                    {
                        _pathAndContent[path] = "WRITE THIS";
                        throw new IOException("Error writing");
                    });

            Assert.Throws<IOException>(() => _service.SaveNote(newNote));

            Assert.True(_pathAndContent.ContainsKey(path));
            Assert.Equal(oldFileContent, _pathAndContent[path]);
        }

        [Fact]
        public void SaveNote_SavesAfterTemp()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            string oldTitle = "OldTitle";
            string path = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[path] = oldFileContent;

            var newNote = new Note(id, oldTitle, "New Content", savedAt, savedAt);

            _service.SaveNote(newNote);

            Assert.True(_pathAndContent.ContainsKey(path));
            Assert.EndsWith("New Content", _pathAndContent[path]);

            Assert.Single(_pathAndContent);
        }

        [Fact]
        public void SaveNote_WhenTitleChanges_SavesNewRemovesOld()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            string oldTitle = "OldTitle";
            string newTitle = "NewTitle";
            string oldPath = NotePath(oldTitle);
            string newPath = NotePath(newTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[oldPath] = oldFileContent;

            var newNote = new Note(id, newTitle, "New Content", savedAt, savedAt);

            _service.SaveNote(newNote);

            Assert.True(_pathAndContent.ContainsKey(newPath));
            Assert.EndsWith("New Content", _pathAndContent[newPath]);
            Assert.False(_pathAndContent.ContainsKey(oldPath));
            Assert.Single(_pathAndContent);
        }

        [Fact]
        public void SaveNote_ReplaceFails_EverythingStays()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            string oldTitle = "OldTitle";
            string path = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[path] = oldFileContent;

            var newNote = new Note(id, oldTitle, "New Content", savedAt, savedAt);

            var expectedError = new IOException("Replace failed");

            _mockFileSystem.Setup(fs => fs
                .ReplaceFile(It.IsAny<string>(), It.IsAny<string>())).Throws(expectedError);

            var actualError = Assert.Throws<IOException>(() => _service.SaveNote(newNote));

            Assert.Same(expectedError, actualError);
            Assert.True(_pathAndContent.ContainsKey(path));
            Assert.Equal(oldFileContent, _pathAndContent[path]);

            _mockFileSystem.Verify(fs => fs
                .ReplaceFile(It.IsAny<string>(), path), Times.Once);
        }

        [Fact]
        public void SaveNote_MoveFileFails_EverythingStays()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            string oldTitle = "OldTitle";
            string newTitle = "New Title";
            string oldPath = NotePath(oldTitle);
            string newPath = NotePath(newTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[oldPath] = oldFileContent;

            var newNote = new Note(id, newTitle, "New Content", savedAt, savedAt);

            var expectedError = new IOException("Move failed");

            _mockFileSystem.Setup(fs => fs
                .MoveFile(It.IsAny<string>(), It.IsAny<string>())).Throws(expectedError);

            var actualError = Assert.Throws<IOException>(() => _service.SaveNote(newNote));

            Assert.Same(expectedError, actualError);
            Assert.True(_pathAndContent.ContainsKey(oldPath));
            Assert.Equal(oldFileContent, _pathAndContent[oldPath]);
            Assert.False(_pathAndContent.ContainsKey(newPath));

            _mockFileSystem.Verify(fs => fs
                .MoveFile(It.IsAny<string>(), newPath), Times.Once);
        }

        [Fact]
        public void SaveNote_NewNote_CreatesFileAndGivesId()
        {
            Guid id = Guid.Empty;
            string title = "New Note";
            string path = NotePath(title);

            var note = new Note(id, title, "Content", _defaultSavedAt, _defaultSavedAt);

            _service.SaveNote(note);

            Assert.NotEqual(Guid.Empty, note.Id);
            Assert.True(_pathAndContent.ContainsKey(path));
            Assert.Single(_pathAndContent);
            Assert.EndsWith("Content", _pathAndContent[path]);
        }

        [Fact]
        public void SaveNote_NewNoteSaveFails_KeepsEmptyGuid()
        {
            Guid id = Guid.Empty;
            string title = "New Note";
            string path = NotePath(title);

            var note = new Note(id, title, "Content", _defaultSavedAt, _defaultSavedAt);

            var expectedError = new IOException("Error saving file");

            _mockFileSystem.Setup(fs => fs
                .MoveFile(It.IsAny<string>(), It.IsAny<string>())).Throws(expectedError);

            var actualError = Assert.Throws<IOException>(() => _service.SaveNote(note));

            Assert.Same(expectedError, actualError);
            Assert.Equal(Guid.Empty, note.Id);
            Assert.False(_pathAndContent.ContainsKey(path));

            _mockFileSystem.Verify(fs => fs
                .MoveFile(It.IsAny<string>(), path), Times.Once);
        }
        [Fact]
        public void SaveNote_WhenSavingFails_KeepsOldUpdateTime()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            DateTime updatedAt = _defaultUpdatedAt;
            string oldTitle = "OldTitle";
            string path = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[path] = oldFileContent;

            var newNote = new Note(id, "OldTitle", "New Content", savedAt, updatedAt);

            var expectedError = new IOException("Save error");

            _mockFileSystem.Setup(fs => fs
                .ReplaceFile(It.IsAny<string>(), It.IsAny<string>())).Throws(expectedError);

            var actualError = Assert.Throws<IOException>(() => _service.SaveNote(newNote));

            Assert.Same(expectedError, actualError);
            Assert.Equal(_defaultUpdatedAt, newNote.UpdatedAt);
        }
        [Fact]
        public void SaveNote_WhenSaved_UpdatesTime()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            DateTime updatedAt = _defaultUpdatedAt;
            string oldTitle = "OldTitle";
            string path = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[path] = oldFileContent;

            var newNote = new Note(id, "OldTitle", "New Content", savedAt, savedAt);

            _service.SaveNote(newNote);

            Assert.Equal(updatedAt, newNote.UpdatedAt);
            Assert.NotEqual(savedAt, newNote.UpdatedAt);
            Assert.Contains(_pathAndContent[path], updatedAt.ToString());

        }
        //Вспомогательные методы

        private string NotePath(string noteTitle)
        {
            return Path.Combine(_testRoot, noteTitle + ".md");
        }

        private string BuildNoteContent(Guid id, string title, string content)
        {
            var metadata = new NoteMetadata
            {
                Id = id,
                Title = title,
                CreatedAt = _defaultSavedAt,
                UpdatedAt = _defaultSavedAt
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yamlPart = serializer.Serialize(metadata);

            return "---\n" + yamlPart + "---\n\n" + content;
        }


    }
}
