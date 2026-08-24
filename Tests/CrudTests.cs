using Salvager.Models;
using Salvager.Services;

namespace Tests
{
    public class CrudTests
    {
        string _notesDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Salvager",
                "Notes"
                );

        [Fact]
        public void CreateNote_Works()
        {
            var service = new NoteService();
            var title = "TESTNOTEFORCREATION";

            var exception = Record.Exception(() => service.CreateNote(title));

            Assert.Null(exception);
        }

        [Fact]
        public void SaveNote_File_IsWritten()
        {
            var service = new NoteService();
            var title = "TESTNOTEFORSAVING";

            Note testNote = service.CreateNote(title);
            service.SaveNote(testNote);
            string fileName = (title + ".md");
            string testNotePath = Path.Combine(_notesDirectory, fileName);
            Assert.True(File.Exists(testNotePath), $"File was not created at {_notesDirectory}");
            if (File.Exists(testNotePath))
            {
                File.Delete(testNotePath);
            }
        }
        [Fact]
        public void DeleteNote_Works()
        {
            var service = new NoteService();
            var title = "TESTNOTEFORDELETION";

            Note testNote = service.CreateNote(title);
            service.SaveNote(testNote);
            string testNotePath = Path.Combine(_notesDirectory, (title + ".md"));

            var exception = Record.Exception(() => service.DeleteNote(testNote.Id));

            Assert.Null(exception);

            if (File.Exists(testNotePath))
            {
                File.Delete(testNotePath);
            }
        }
        [Fact]
        public void DeleteNote_File_IsDeleted()
        {
            var service = new NoteService();
            var title = "TESTNOTEFORFILEDELETIONPROCESS";

            Note testNote = service.CreateNote(title);
            service.SaveNote(testNote);
            string testNotePath = Path.Combine(_notesDirectory, (title + ".md"));

            service.DeleteNote(testNote.Id);

            Assert.True((!File.Exists(testNotePath)), $"The note file is still there in {_notesDirectory}");
        }
    }
}
