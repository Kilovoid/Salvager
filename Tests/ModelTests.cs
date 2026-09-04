using System;
using System.Collections.Generic;
using System.Text;
using Salvager.Models;

namespace Tests
{
    public class ModelTests
    {
        //private Note _fullNoteModel;
        //private Note _noteModel;
        //public ModelTests()
        //{
        //    var _fullNoteModel = new Note(Guid.NewGuid(), "Test Note", "Note for tests", DateTime.Now, DateTime.Now);
        //    var _noteModel = new Note("Test Note 2", "Note for tests");
        //}

        [Fact]
        public void FullNote_ProperlyUpdatesFields()
        {
            Guid newGuid = Guid.NewGuid();
            string newTitle = "New Title";
            string newContent = "Test content";
            DateTime newCreationTime = new DateTime(2000, 12, 12, 12, 12, 0);
            DateTime newUpdateTime = new DateTime(2000, 10, 10, 10, 10, 0);

            var note = new Note(Guid.NewGuid(), "TESTNOTE", "", DateTime.Now, DateTime.Now);
            note.Id = newGuid;
            note.Title = newTitle;
            note.Content = newContent;
            note.CreatedAt = newCreationTime;
            note.UpdatedAt = newUpdateTime;

            Assert.Equal(newGuid, note.Id);
            Assert.Equal(newTitle, note.Title);
            Assert.Equal(newContent, note.Content);
            Assert.Equal(newCreationTime, note.CreatedAt);
            Assert.Equal(newUpdateTime, note.UpdatedAt);
        }

        [Fact]
        public void Note_ProperlyUpdatesFields()
        {
            Guid newGuid = Guid.NewGuid();
            string newTitle = "New Title";
            string newContent = "Test content";
            DateTime newCreationTime = new DateTime(2000, 12, 12, 12, 12, 0);
            DateTime newUpdateTime = new DateTime(2000, 10, 10, 10, 10, 0);

            var note = new Note("Test note", "");
            note.Id = newGuid;
            note.Title = newTitle;
            note.Content = newContent;
            note.CreatedAt = newCreationTime;
            note.UpdatedAt = newUpdateTime;

            Assert.Equal(newGuid, note.Id);
            Assert.Equal(newTitle, note.Title);
            Assert.Equal(newContent, note.Content);
            Assert.Equal(newCreationTime, note.CreatedAt);
            Assert.Equal(newUpdateTime, note.UpdatedAt);
        }
    }
}
