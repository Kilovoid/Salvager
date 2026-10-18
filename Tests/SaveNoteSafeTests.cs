using Moq;
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
        }
        [Fact]
        public void SaveNote_WhenWriteFailsWithoutRename_KeepsOldContent()
        {
            //Arrange: подготовить файл и объект заметки, изменить содержимое, создать ошибку записи
            //Act: вызвать SaveNote и поймать ошибку IOException
            //Assert: проверить, что DeleteNote не вызвался, старый файл все еще на месте, содержимое такое же

            //Для Arrange понадобится вызвать настоящий NoteService, фальшивый FileSystem

            //Чтобы проверять содержимое есть смысл создать некоторый словарь, который хранит как ключ путь, а содержимое как переменную.


        } 
    }
}
