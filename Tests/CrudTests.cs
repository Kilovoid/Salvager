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
            string sameName = "SameName";

            var oldNote = new Note(Guid.NewGuid(), sameName, "", DateTime.Now, DateTime.Now);
            var newNote = new Note(Guid.NewGuid(), sameName, "", DateTime.Now, DateTime.Now);

            string oldPath = _mockFileSystem.Object.CombinePath(_testRoot, $"{sameName}.md");
            string newPath = _mockFileSystem.Object.CombinePath(_testRoot, $"{sameName} (1).md");

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());

            _service.SaveNote(oldNote);

            string oldFileContent = $"---\nid: {oldNote.Id}\ntitle: {oldNote.Title}" +
                $"\n---\n\n";

            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns([oldPath]);

            _mockFileSystem.Setup(fileSys => fileSys
            .ReadAllText(oldPath)).Returns(oldFileContent);

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(oldPath)).Returns(true);

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(newPath)).Returns(false);

            _mockFileSystem.Setup(fileSys => fileSys
            .GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns<string>(Path.GetFileNameWithoutExtension);

            _service.SaveNote(newNote);

            _mockFileSystem.Verify(fileSys => fileSys
            .DeleteFile(oldPath), Times.Never);

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(
                It.Is<string>(path => path == oldPath),
                It.IsAny<string>(),
                It.IsAny<Encoding>()), Times.Once);

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(
                It.Is<string>(path => path.Contains("(1)")),
                It.IsAny<string>(),
                It.IsAny<Encoding>()), Times.Once);

            Assert.Equal($"{sameName} (1)", newNote.Title);
        }

        [Fact]
        public void SaveNote_CorrectMetadata()
        {
            Guid testGuid = Guid.NewGuid();
            var currentNote = new Note(testGuid, $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());

            string capturedContent = null!;

            _mockFileSystem.Setup(fileSys => fileSys
            .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()))
                .Callback<string, string, Encoding>((path, content, encoding) =>
                {
                    capturedContent = content;
                });

            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(It.Is<string>(path => path.Contains(currentNote.Title)),
            It.IsAny<string>(),
            It.IsAny<Encoding>()), Times.Once);

            Assert.Contains("---", capturedContent);
            Assert.Contains($"id: {testGuid}", capturedContent);
            Assert.Contains($"title: {currentNote.Title}", capturedContent);
        }

        [Fact]
        public void SaveNote_DoesNotDuplicate()
        {
            Guid testGuid = Guid.NewGuid();
            var currentNote = new Note(testGuid, $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());


            string capturedContent = null!;
            string capturedPath = null!;

            _mockFileSystem.Setup(fileSys => fileSys
            .WriteAllText(It.Is<string>(path => path.Contains(currentNote.Title)),
                It.IsAny<string>(),
                It.IsAny<Encoding>()))
                    .Callback<string, string, Encoding>((path, content, encoding) =>
                    {
                        capturedContent = content;
                        capturedPath = path;
                    });

            _service.SaveNote(currentNote);

            _mockFileSystem.Setup(fileSys => fileSys
            .FileExists(It.Is<string>(path => path == capturedPath))).Returns(true);
            _mockFileSystem.Setup(fileSys => fileSys
            .GetFiles(_testRoot, "*.md")).Returns([ capturedPath ]);

            string fileContent = $"---\nid: {currentNote.Id}\ntitle: {currentNote.Title}" +
                $"\n---\n\n";

            _mockFileSystem.Setup(fileSys => fileSys
            .ReadAllText(capturedPath)).Returns(fileContent);

            currentNote.Content = " test ";
            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fileSys => fileSys
            .DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(It.Is<string>(path => path == capturedPath), It.IsAny<string>(), It.IsAny<Encoding>()), Times.Exactly(2));

            _mockFileSystem.Verify(fileSys => fileSys
            .WriteAllText(It.IsAny<string>(),
            It.Is<string>(content => content.Contains(" test ")),
            It.IsAny<Encoding>()), Times.Once);
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
