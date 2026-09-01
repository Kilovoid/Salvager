using Moq;
using Salvager.Models;
using Salvager.Services;
using System.Text;

namespace Tests
{
    public class CrudTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly NoteService _service;
        private readonly string _testRoot = @"C:\test\notes";

        public CrudTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();

            _mockFileSystem.Setup(fileSys => fileSys
            .CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((a, b) => Path.Combine(a, b));

            _mockFileSystem.Setup(fileSys => fileSys
            .DirectoryExists(It.IsAny<string>()))
                .Returns(true);

            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Array.Empty<string>());

            _mockFileSystem.Setup(fileSys => fileSys
            .GetInvalidFileNameChars())
                .Returns(Path.GetInvalidFileNameChars());

            _service = new NoteService(_mockFileSystem.Object, _testRoot);
        }

        //!!CreateNote Tests!!

        [Fact]
        public void CreateNote_Works()
        {
            var exception = Record.Exception(() => _service.CreateNote($"{Guid.NewGuid}"));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CreateNote_InvalidNoteTitle(string? title)
        {
            Assert.Throws<ArgumentNullException>(() => _service.CreateNote(title));
        }

        [Theory]
        [InlineData("<><><><>", "________")]
        [InlineData("<><>aa<>", "____aa__")]
        public void CreateNote_IsFormatting_FileName(string sampleName, string expected)
        {
            string title = sampleName;

            var note = _service.CreateNote(title);

            Assert.Equal(note.Title, expected);
        }

        //!!SaveNoteTests!!

        [Fact]
        public void SaveNote_Works()
        {
            var currentNote = new Note($"{Guid.NewGuid()}", "");
            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            var exception = Record.Exception(() => _service.SaveNote(currentNote));

            Assert.Null(exception);
            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()), Times.Once);
        }

        [Fact]
        public void SaveNote_NullNote()
        {
            Assert.Throws<ArgumentNullException>(() => _service.SaveNote(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void SaveNote_InvalidTitle(string? title)
        {
            var currentNote = new Note(title, "");

            Assert.Throws<ArgumentException>(() => _service.SaveNote(currentNote));
        }

        [Fact]
        public void SaveNote_File_IsWritten()
        {
            var currentNote = new Note($"{Guid.NewGuid()}", "");

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()),
            Times.Once);
        }

        [Fact]
        public void SaveNote_GeneratesId_OnNull()
        {
            var currentNote = new Note(Guid.Empty, $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);
            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            _service.SaveNote(currentNote);
            Assert.NotEqual(Guid.Empty, currentNote.Id);
        }

        [Fact]
        public void SaveNote_UpdatesTime()
        {
            var currentNote = new Note(Guid.NewGuid(), $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            var oldUpdateDate = currentNote.UpdatedAt;

            Thread.Sleep(10);
            _service.SaveNote(currentNote);

            Assert.NotEqual(oldUpdateDate, currentNote.UpdatedAt);
            Assert.True(currentNote.UpdatedAt > oldUpdateDate);
        }

        [Fact]
        public void SaveNote_WhenTitleChanges_CreatesNewDeletesOld()
        {
            var currentNote = new Note(Guid.NewGuid(), $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);

            string oldName = currentNote.Title;
            string oldPath = @"C:\test\notes\" + oldName + ".md";
            string newName = "TestForNameUpdate";
            string newPath = @"C:\test\notes\" + newName + ".md";

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(oldPath)).Returns(false);
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());

            _service.SaveNote(currentNote);

            currentNote.Title = newName;

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(oldPath)).Returns(true);
            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(newPath)).Returns(false);

            string fileContent = $"---\nid: {currentNote.Id}\ntitle: {currentNote.Title}" +
                $"\n---\n\n";
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md"))
                .Returns([oldPath]);
            _mockFileSystem.Setup(fileSys => fileSys
            .ReadAllText(oldPath))
                .Returns(fileContent);

            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fileSys => fileSys
            .DeleteFile(oldPath), Times.Once);

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(It.Is<string>(p => p.Contains("TestForNameUpdate")),
            It.IsAny<string>(), It.IsAny<Encoding>()),
            Times.Once);
        }

        [Fact]
        public void SaveNote_AddsSuffix()
        {
            string sameName = Guid.NewGuid().ToString();
            var note1 = new Note(Guid.NewGuid(), sameName, "", DateTime.Now, DateTime.Now);
            var note2 = new Note(Guid.NewGuid(), sameName, "", DateTime.Now, DateTime.Now);

            //_service.SaveNote(note1);
            //_service.SaveNote(note2);

            string path1 = @"C:\test\notes\" + sameName + ".md";
            string path2 = @"C:\test\notes\" + sameName + " (1).md";

            _mockFileSystem.Setup(fs => fs.FileExists(path1)).Returns(true);   // первый файл существует
            _mockFileSystem.Setup(fs => fs.FileExists(path2)).Returns(false);  // второго ещё нет
            _mockFileSystem.Setup(fs => fs.FileExists(It.Is<string>(p => p != path1 && p != path2)))
                .Returns(false);
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns([path1]);

            string fileContent1 = $"---\nid: {note1.Id}\ntitle: {note1.Title}\n---\n\n";

            _mockFileSystem.Setup(fileSys => fileSys
            .ReadAllText(path1)).Returns(fileContent1);

            _service.SaveNote(note1);
            _service.SaveNote(note2);

            Assert.Equal($"{sameName} (1)", note2.Title);

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(
                It.Is<string>(p => p.Contains(" (1)")),
                It.IsAny<string>(),
                It.IsAny<Encoding>()), Times.Once);
        }

        [Fact]
        public void SaveNote_CorrectMetadata()
        {
            Guid testGuid = Guid.NewGuid();
            string testNameGuid = Guid.NewGuid().ToString();
            var note = new Note(testGuid, testNameGuid, "", DateTime.Now, DateTime.Now);

            _service.SaveNote(note);

            string filePath = Path.Combine(_testRoot, $"{note.Title}.md");
            string content = File.ReadAllText(filePath);

            Assert.Contains("---", content);
            Assert.Contains($"title: {testNameGuid}", content);
            Assert.Contains($"id: {testGuid.ToString()}", content);
            //Assert.Contains($"createdAt: {note.CreatedAt}", content);
            //Assert.Contains($"updatedAt: {note.UpdatedAt}", content);
        }

        [Fact]
        public void SaveNote_DoesNotDuplicate()
        {
            Guid testGuid = Guid.NewGuid();
            var note = new Note(testGuid, $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);

            _service.SaveNote(note);

            note.Content = "TEST";
            _service.SaveNote(note);

            var files = Directory.GetFiles(_testRoot, "*.md");
            Assert.Single(files);

            string content = File.ReadAllText(files[0]);
            Assert.Contains("TEST", content);
        }

        //!!DeleteNote Tests!!

        [Fact]
        public void DeleteNote_Works()
        {
            var note = new Note($"{Guid.NewGuid()}", "");
            _service.SaveNote(note);
            var exception = Record.Exception(() => _service.DeleteNote(note.Id));
            Assert.Null(exception);
        }

        [Fact]
        public void DeleteNote_File_IsDeleted()
        {
            var note = new Note($"{Guid.NewGuid()}", "");
            _service.SaveNote(note);
            string fileName = note.Title + ".md";
            string path = Path.Combine(_testRoot, fileName);
            _service.DeleteNote(note.Id);
            Assert.False(File.Exists(path), "File was not deleted");
        }

        [Fact]
        public void DeleteNote_NullGuid()
        {
            Guid noteId = Guid.Empty;

            Assert.Throws<ArgumentNullException>(() => _service.DeleteNote(noteId));
        }

        [Fact]
        public void DeleteNote_FileIsNull()
        {
            Guid noteId = Guid.NewGuid();

            Assert.Throws<FileNotFoundException>(() => _service.DeleteNote(noteId));
        }

        [Fact]
        public void DeleteNote_WhenFileIsLocked_ThrowsIOException()
        {
            var note = new Note($"{Guid.NewGuid()}", "");

            _service.SaveNote(note);
            var filePath = Path.Combine(_testRoot, $"{note.Title}.md");
            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

            Assert.Throws<IOException>(() => _service.DeleteNote(note.Id));
        }

        [Fact]
        public void DeleteNote_NoFrontmatter_ThrowsInvalidDataException()
        {
            string noteName = Guid.NewGuid().ToString();
            var note = new Note(noteName, "");

            _service.SaveNote(note);

            var filePath = Path.Combine(_testRoot, $"{noteName}.md");
            File.WriteAllText(filePath, "INVALID TEXT");

            Assert.Throws<InvalidDataException>(() => _service.DeleteNote(note.Id));
        }

        [Fact]
        public void DeleteNote_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            var testDirectory = Path.Combine(_testRoot, Guid.NewGuid().ToString());
            var testService = new NoteService(testDirectory);

            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory);
            }

            Assert.Throws<DirectoryNotFoundException>(() => testService.DeleteNote(Guid.NewGuid()));
        }
    }
}
