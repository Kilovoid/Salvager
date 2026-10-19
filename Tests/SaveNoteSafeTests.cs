using Moq;
using Salvager.Models;
using Salvager.Services;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tests
{
    public class SaveNoteSafeTests
    {
        private readonly DateTime _defaultSavedAt = new DateTime(2026, 12, 12, 12, 12, 12);
        private readonly NoteService _service;
        private readonly Mock<ILogger> _logger;
        private readonly string _testRoot = Path.Combine(
            Path.GetTempPath(), "SalvagerTests");
        private readonly Dictionary<string, string> _pathAndContent;
        private readonly Mock<IFileSystem> _mockFileSystem;

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

            _logger = new Mock<ILogger>();
            _service = new NoteService(
                _mockFileSystem.Object, _logger.Object, _testRoot);
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
            string oldPath = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[oldPath] = oldFileContent;

            var newNote = new Note(id, "OldTitle", "New Content", savedAt, savedAt);

            _mockFileSystem.Setup(fs => fs
                .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()))
                    .Callback<string, string, Encoding>((path, content, encoding) =>
                    {
                        _pathAndContent[path] = "WRITE THIS";
                        throw new IOException("Error writing");
                    });

            Assert.Throws<IOException>(() => _service.SaveNote(newNote));

            Assert.True(_pathAndContent.ContainsKey(oldPath));
            Assert.Equal(oldFileContent, _pathAndContent[oldPath]);
        }

        [Fact]
        public void SaveNote_SavesAfterTemp()
        {
            Guid id = Guid.NewGuid();
            DateTime savedAt = _defaultSavedAt;
            string oldTitle = "OldTitle";
            string oldPath = NotePath(oldTitle);
            string oldFileContent = BuildNoteContent(id, oldTitle, "Test Content");

            _pathAndContent[oldPath] = oldFileContent;

            var newNote = new Note(id, oldTitle, "New Content", savedAt, savedAt);

            _service.SaveNote(newNote);

            Assert.True(_pathAndContent.ContainsKey(oldPath));
        }

        [Fact]
        public void SaveNote_CreatesTempFile()
        {

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
