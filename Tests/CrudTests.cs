using Salvager.Models;
using Salvager.Services;
using SkiaSharp;

namespace Tests
{
    public class CrudTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly NoteService _service;

        public CrudTests()
        {
            _testRoot = Path.Combine(
                Path.GetTempPath(), "Salvager Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testRoot);
            _service = new NoteService(_testRoot);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testRoot))
                {
                    Directory.Delete(_testRoot, true);
                    Console.WriteLine($"DELETED!!!! {_testRoot}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED TO DELETE!!!! {_testRoot} : {ex.Message}");
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "dispose_error.txt"), ex.ToString());
            }
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
            var exception = Record.Exception(() => _service.SaveNote(currentNote));

            Assert.Null(exception);
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
            _service.SaveNote(currentNote);
            string fileName = currentNote.Title + ".md";
            string path = Path.Combine(_testRoot, fileName);
            Assert.True(File.Exists(path), "File was not saved");
        }

        [Fact]
        public void SaveNote_GeneratesId_OnNull()
        {
            var currentNote = new Note(Guid.Empty, $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);
            _service.SaveNote(currentNote);

            Assert.NotEqual(Guid.Empty, currentNote.Id);
        }

        [Fact]
        public void SaveNote_UpdatesTime()
        {
            var currentNote = new Note(Guid.NewGuid(), $"{Guid.NewGuid()}", "", DateTime.Now, DateTime.Now);
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
            _service.SaveNote(currentNote);
            string fileName = currentNote.Title + ".md";
            string oldPath = Path.Combine(_testRoot, fileName);
            Assert.True(File.Exists(oldPath));

            currentNote.Title = "TESTFORTITLECHANGE";
            _service.SaveNote(currentNote);

            Assert.False(File.Exists(oldPath));

            string newFileName = currentNote.Title + ".md";
            string newPath = Path.Combine(_testRoot, newFileName);
            Assert.True(File.Exists(newPath));
        }

        [Fact]
        public void SaveNote_AddsSuffix()
        {
            string sameName = Guid.NewGuid().ToString();
            var note1 = new Note(Guid.NewGuid(), sameName, "", DateTime.Now, DateTime.Now);
            var note2 = new Note(Guid.NewGuid(), sameName, "", DateTime.Now, DateTime.Now);

            _service.SaveNote(note1);
            _service.SaveNote(note2);

            string path1 = Path.Combine(_testRoot, $"{sameName}.md");
            string path2 = Path.Combine(_testRoot, $"{sameName} (1).md");

            Assert.True(File.Exists(path1));
            Assert.True(File.Exists(path2));

            Assert.Equal($"{sameName} (1)", note2.Title);
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
            string noteName = Guid.NewGuid().ToString();
            var note = new Note(noteName, "");

            _service.SaveNote(note);
            var filePath = Path.Combine(_testRoot, $"{noteName}.md");
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
