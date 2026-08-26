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
    }
}
