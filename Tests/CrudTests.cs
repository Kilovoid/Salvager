using Salvager.Models;
using Salvager.Services;

namespace Tests
{
    public class CrudTests : IDisposable
    {
        private string _testRoot;
        private NoteService _service;

        public CrudTests()
        {
            _testRoot = Path.Combine(
                Path.GetTempPath(), "Salvager Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testRoot);
            _service = new NoteService(_testRoot);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, true);
            }
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void CreateNote_Works()
        {
            var exception = Record.Exception(() => _service.CreateNote($"{Guid.NewGuid}"));
            Assert.Null(exception);
        }

        [Fact]
        public void DeleteNote_Works()
        {
            Guid testGuid = new Guid();
            var note = _service.CreateNote($"{testGuid}");
            _service.SaveNote(note);
            var exception = Record.Exception(() => _service.DeleteNote(note.Id));
            Assert.Null(exception);
        }

        [Fact]
        public void SaveNote_File_IsWritten()
        {
            Guid testGuid = new Guid();
            var note = _service.CreateNote($"{testGuid}");
            _service.SaveNote(note);
            string fileName = testGuid + ".md";
            string path = Path.Combine(_testRoot, fileName);
            Assert.True(File.Exists(path), "File was not saved");
        }

        [Fact]
        public void DeleteNote_File_IsDeleted()
        {
            Guid testGuid = new Guid();
            var note = _service.CreateNote($"{testGuid}");
            _service.SaveNote(note);
            string fileName = testGuid + ".md";
            string path = Path.Combine(_testRoot, fileName);
            _service.DeleteNote(note.Id);
            Assert.False(File.Exists(path), "File was not deleted");
        }

        //Exception throwing tests for CreateNote

        [Fact]
        public void CreateNote_Null_Title()
        {
            string? title = null;

            Assert.Throws<ArgumentNullException>(() => _service.CreateNote(title));
        }

        [Fact]
        public void CreateNote_Blank_Title()
        {
            string? title = "";

            Assert.Throws<ArgumentNullException>(() => _service.CreateNote(title));
        }

        [Fact]
        public void CreateNote_WhiteSpace_Title()
        {
            string? title = " ";

            Assert.Throws<ArgumentException>(() => _service.CreateNote(title));
        }

        // Тест на форматирование заголовка CreateNote

        [Theory]
        [InlineData("<><><><>", "________")]
        [InlineData("<><>aa<>", "____aa__")]
        public void CreateNote_IsFormatting_FileName(string sampleName, string expected)
        {
            string title = sampleName;

            var note = _service.CreateNote(title);

            Assert.Equal(note.Title, expected);
        }
    }
}
