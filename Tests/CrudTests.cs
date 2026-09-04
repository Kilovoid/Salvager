using Moq;
using Salvager.Models;
using Salvager.Services;
using System.Text;

namespace Tests
{
    public class CrudTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;

        private readonly Mock<ILogger> _mockLogger;
        private readonly NoteService _service;
        private readonly string _testRoot = @"C:\test\notes";

        public CrudTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();

            _mockLogger = new Mock<ILogger>();

            _mockFileSystem.Setup(fs => fs
            .CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((a, b) => Path.Combine(a, b));

            _mockFileSystem.Setup(fs => fs
            .DirectoryExists(It.IsAny<string>()))
                .Returns(true);

            _mockFileSystem.Setup(fs => fs
            .GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Array.Empty<string>());

            _mockFileSystem.Setup(fs => fs
            .GetInvalidFileNameChars())
                .Returns(Path.GetInvalidFileNameChars());

            _service = new NoteService(_mockFileSystem.Object, _mockLogger.Object, _testRoot);
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
            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            var exception = Record.Exception(() => _service.SaveNote(currentNote));

            Assert.Null(exception);
            _mockFileSystem.Verify(fs => fs
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

            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fs => fs
            .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()),
            Times.Once);
        }

        [Fact]
        public void SaveNote_GeneratesId_OnNull()
        {
            var currentNote = new Note(Guid.Empty, $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);
            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>()))
                .Returns(false);

            _service.SaveNote(currentNote);
            Assert.NotEqual(Guid.Empty, currentNote.Id);
        }

        [Fact]
        public void SaveNote_UpdatesTime()
        {
            var currentNote = new Note(Guid.NewGuid(), $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);

            _mockFileSystem.Setup(fs => fs
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

            _mockFileSystem.Setup(fs => fs
            .FileExists(oldPath)).Returns(false);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());

            _service.SaveNote(currentNote);

            currentNote.Title = newName;

            _mockFileSystem.Setup(fs => fs
            .FileExists(oldPath)).Returns(true);
            _mockFileSystem.Setup(fs => fs
            .FileExists(newPath)).Returns(false);

            string fileContent = $"---\nid: {currentNote.Id}\ntitle: {currentNote.Title}" +
                $"\n---\n\n";
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md"))
                .Returns([oldPath]);
            _mockFileSystem.Setup(fs => fs
            .ReadAllText(oldPath))
                .Returns(fileContent);

            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fs => fs
            .DeleteFile(oldPath), Times.Once);

            _mockFileSystem.Verify(fs => fs
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

            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());

            _service.SaveNote(oldNote);

            string oldFileContent = $"---\nid: {oldNote.Id}\ntitle: {oldNote.Title}" +
                $"\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([oldPath]);

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(oldPath)).Returns(oldFileContent);

            _mockFileSystem.Setup(fs => fs
            .FileExists(oldPath)).Returns(true);

            _mockFileSystem.Setup(fs => fs
            .FileExists(newPath)).Returns(false);

            _mockFileSystem.Setup(fs => fs
            .GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns<string>(Path.GetFileNameWithoutExtension);

            _service.SaveNote(newNote);

            _mockFileSystem.Verify(fs => fs
            .DeleteFile(oldPath), Times.Never);

            _mockFileSystem.Verify(fs => fs
            .WriteAllText(
                It.Is<string>(path => path == oldPath),
                It.IsAny<string>(),
                It.IsAny<Encoding>()), Times.Once);

            _mockFileSystem.Verify(fs => fs
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

            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());

            string capturedContent = null!;

            _mockFileSystem.Setup(fs => fs
            .WriteAllText(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Encoding>()))
                .Callback<string, string, Encoding>((path, content, encoding) =>
                {
                    capturedContent = content;
                });

            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fs => fs
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

            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(false);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns(Array.Empty<string>());


            string capturedContent = null!;
            string capturedPath = null!;

            _mockFileSystem.Setup(fs => fs
            .WriteAllText(It.Is<string>(path => path.Contains(currentNote.Title)),
                It.IsAny<string>(),
                It.IsAny<Encoding>()))
                    .Callback<string, string, Encoding>((path, content, encoding) =>
                    {
                        capturedContent = content;
                        capturedPath = path;
                    });

            _service.SaveNote(currentNote);

            _mockFileSystem.Setup(fs => fs
            .FileExists(It.Is<string>(path => path == capturedPath))).Returns(true);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([ capturedPath ]);

            string fileContent = $"---\nid: {currentNote.Id}\ntitle: {currentNote.Title}" +
                $"\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(capturedPath)).Returns(fileContent);

            currentNote.Content = " test ";
            _service.SaveNote(currentNote);

            _mockFileSystem.Verify(fs => fs
            .DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fs => fs
            .WriteAllText(It.Is<string>(path => path == capturedPath), It.IsAny<string>(), It.IsAny<Encoding>()), Times.Exactly(2));

            _mockFileSystem.Verify(fs => fs
            .WriteAllText(It.IsAny<string>(),
            It.Is<string>(content => content.Contains(" test ")),
            It.IsAny<Encoding>()), Times.Once);
        }

        //!!DeleteNote Tests!!

        [Fact]
        public void DeleteNote_Works()
        {
            string noteName = Guid.NewGuid().ToString();
            Guid noteGuid = Guid.NewGuid();
            var note = new Note(noteGuid, noteName, "", DateTime.Now, DateTime.Now);

            string path = Path.Combine(_testRoot, $"{noteName}.md");

            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([path]);
            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(true);

            string fileContent = $"---\nid: {note.Id}\ntitle: {note.Title}\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(path)).Returns(fileContent);

            _service.DeleteNote(note.Id);

            _mockFileSystem.Verify(fs => fs
            .DeleteFile(It.IsAny<string>()), Times.Once);
            
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
        public void DeleteNote_DirectoryDoesNotExist_ThrowsException()
        {
            Guid anyGuid = Guid.NewGuid();
            _mockFileSystem.Setup(fs => fs
            .DirectoryExists(It.IsAny<string>())).Returns(true);

            var service = new NoteService(_mockFileSystem.Object, _testRoot);

            _mockFileSystem.Setup(fs => fs
            .DirectoryExists(It.IsAny<string>())).Returns(false);

            Assert.Throws<DirectoryNotFoundException>(() => service.DeleteNote(anyGuid));
        }

        [Fact]
        public void DeleteNote_DirectoryHasBadFile_StillDeletesValidFile()
        {
            var badNote = new Note(Guid.NewGuid(), "Bad Note", "", DateTime.Now, DateTime.Now);
            var goodNote = new Note(Guid.NewGuid(), "Good Note", "", DateTime.Now, DateTime.Now);

            string badPath = Path.Combine(_testRoot, $"{badNote.Title}.md");
            string goodPath = Path.Combine(_testRoot, $"{goodNote.Title}.md");

            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([badPath, goodPath]);
            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(true);

            string goodFileContent = $"---\nid: {goodNote.Id}\ntitle: {goodNote.Title}\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(badPath)).Throws<IOException>();
            _mockFileSystem.Setup(fs => fs
            .ReadAllText(goodPath)).Returns(goodFileContent);

            _service.DeleteNote(goodNote.Id);

            _mockFileSystem.Verify(fs => fs
            .ReadAllText(It.IsAny<string>()), Times.Exactly(2));
            _mockFileSystem.Verify(fs => fs
            .DeleteFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void DeleteNote_InvalidData_StillDeletesValidFile()
        {
            var badNote = new Note(Guid.NewGuid(), "Bad Note", "", DateTime.Now, DateTime.Now);
            var goodNote = new Note(Guid.NewGuid(), "Good Note", "", DateTime.Now, DateTime.Now);

            string badPath = Path.Combine(_testRoot, $"{badNote.Title}.md");
            string goodPath = Path.Combine(_testRoot, $"{goodNote.Title}.md");

            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([badPath, goodPath]);
            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(true);

            string goodFileContent = $"---\nid: {goodNote.Id}\ntitle: {goodNote.Title}\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(badPath)).Throws<InvalidDataException>();
            _mockFileSystem.Setup(fs => fs
            .ReadAllText(goodPath)).Returns(goodFileContent);

            _service.DeleteNote(goodNote.Id);

            _mockFileSystem.Verify(fs => fs
            .ReadAllText(It.IsAny<string>()), Times.Exactly(2));
            _mockFileSystem.Verify(fs => fs
            .DeleteFile(It.IsAny<string>()), Times.Once);
            _mockLogger.Verify(log => log
            .Log(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData(typeof(IOException))]
        [InlineData(typeof(UnauthorizedAccessException))]
        public void DeleteNote_MarkedFileCantBeDeleted_IsNotDeleted(Type exception)
        {
            Guid noteGuid = Guid.NewGuid();
            string noteName = Guid.NewGuid().ToString();
            var note = new Note(noteGuid, noteName, "", DateTime.Now, DateTime.Now);

            string path = Path.Combine(_testRoot, $"{noteName}.md");

            _mockFileSystem.Setup(fs => fs
            .FileExists(path)).Returns(true);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([path]);
            
            string fileContent = $"---\nid: {note.Id}\ntitle: {note.Title}\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(path)).Returns(fileContent);

            var expectedException = (Exception)Activator.CreateInstance(exception);
            _mockFileSystem.Setup(fs => fs
            .DeleteFile(path)).Throws(expectedException);

            Assert.Throws(exception, () => _service.DeleteNote(note.Id));

            _mockLogger.Verify(log => log
            .Log(It.IsAny<string>()), Times.Once);
        }

        //!!LoadAll Tests!!

        [Fact]
        public void LoadAll_Works()
        {
            var note1 = new Note(Guid.NewGuid(), "note1", "", DateTime.Now, DateTime.Now);
            var note2 = new Note(Guid.NewGuid(), "note2", "", DateTime.Now, DateTime.Now);

            string path1 = Path.Combine(_testRoot, $"{note1.Title}.md");
            string path2 = Path.Combine(_testRoot, $"{note2.Title}.md");

            string file1Content = $"---\nid: {note1.Id}\ntitle: {note1.Title}\n---\n\n";
            string file2Content = $"---\nid: {note2.Id}\ntitle: {note2.Title}\n---\n\n";

            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([path1, path2]);
            _mockFileSystem.Setup(fs => fs
            .ReadAllText(path1)).Returns(file1Content);
            _mockFileSystem.Setup(fs => fs
            .ReadAllText(path2)).Returns(file2Content);

            List<Note> testNotes = _service.LoadAll();

            _mockFileSystem.Verify(fs => fs
            .GetFiles(_testRoot, "*.md"), Times.Exactly(2)); //MigrateFiles тоже вызывает GetFiles
            _mockLogger.Verify(log => log
            .Log(It.IsAny<string>()), Times.Never);
            Assert.Equal(2, testNotes.Count);
        }

        [Fact]
        public void LoadAll_CreatesNewDirectory_IfDoesNotExist()
        {
            _mockFileSystem.Setup(fs => fs
            .DirectoryExists(It.IsAny<string>())).Returns(true);

            var service = new NoteService(_mockFileSystem.Object, _testRoot);

            _mockFileSystem.Setup(fs => fs
            .DirectoryExists(It.IsAny<string>())).Returns(false);

            service.LoadAll();

            _mockFileSystem.Verify(fs => fs
            .CreateDirectory(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData(typeof(UnauthorizedAccessException))]
        [InlineData(typeof(FileNotFoundException))]
        [InlineData(typeof(Exception))]
        public void LoadAll_StillReadsAfterInvalidFiles_Logs(Type exception)
        {
            var badNote = new Note(Guid.NewGuid(), "Bad Note", "", DateTime.Now, DateTime.Now);
            var goodNote = new Note(Guid.NewGuid(), "Good Note", "", DateTime.Now, DateTime.Now);

            string goodPath = Path.Combine(_testRoot, $"{goodNote.Title}.md");
            string badPath = Path.Combine(_testRoot, $"{badNote.Title}.md");

            _mockFileSystem.Setup(fs => fs
            .GetFiles(_testRoot, "*.md")).Returns([badPath, goodPath]);
            _mockFileSystem.Setup(fs => fs
            .FileExists(It.IsAny<string>())).Returns(true);

            string goodFileContent = $"---\nid: {goodNote.Id}\ntitle: {goodNote.Title}\n---\n\n";
            var expectedException = (Exception)Activator.CreateInstance(exception);

            _mockFileSystem.Setup(fs => fs
            .ReadAllText(badPath)).Throws(expectedException);
            _mockFileSystem.Setup(fs => fs
            .ReadAllText(goodPath)).Returns(goodFileContent);

            _service.LoadAll();

            _mockFileSystem.Verify(fs => fs
            .ReadAllText(It.IsAny<string>()), Times.Exactly(2));
            _mockLogger.Verify(log => log
            .Log(It.IsAny<string>()), Times.Once);
        }

        //!LoadNote Tests!!

    }
}
