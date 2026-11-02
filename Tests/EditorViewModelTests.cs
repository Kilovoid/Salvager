using Moq;
using Salvager.Models;
using Salvager.Services;
using Salvager.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class EditorViewModelTests
    {
        private readonly DateTime _defaultSavedAt = new DateTime(2026, 12, 12, 12, 12, 12);
        private readonly DateTime _defaultUpdatedAt = new DateTime(2027, 12, 12, 12, 12, 12);
        private readonly Mock<INoteService> _mockService;
        private readonly Mock<IDialogueService> _mockDialogueService;
        private readonly Mock<IEditorViewModelFactory> _mockEditorViewModelFactory;
        private readonly Mock<IEditorViewModel> _mockEditor;


        public EditorViewModelTests()
        {
            _mockService = new Mock<INoteService>();
            _mockDialogueService = new Mock<IDialogueService>();
            _mockEditorViewModelFactory = new Mock<IEditorViewModelFactory>();
            _mockEditor = new Mock<IEditorViewModel>();
        }

        [Fact]
        public void ContentChanged_MarksNoteDirty()
        {
            Guid id = Guid.NewGuid();
            string title = "TestTitle";
            string content = "TestContent";

            var note = new Note(id, title, content, _defaultSavedAt, _defaultSavedAt);

            var testEditorViewModel = new EditorViewModel(note);

            note.Content = "Changed content";

            Assert.True(note.IsDirty);
        }

        [Fact]
        public void ContentRestored_UnmarksNoteDirty()
        {
            Guid id = Guid.NewGuid();
            string title = "TestTitle";
            string content = "TestContent";

            var note = new Note(id, title, content, _defaultSavedAt, _defaultSavedAt);

            var testEditorViewModel = new EditorViewModel(note);

            note.Content = "Changed content";
            Assert.True(note.IsDirty);
            note.Content = "TestContent";
            Assert.False(note.IsDirty);
        }

        [Fact]
        public void TitleChanged_MarksNoteDirty()
        {
            Guid id = Guid.NewGuid();
            string title = "TestTitle";
            string content = "TestContent";

            var note = new Note(id, title, content, _defaultSavedAt, _defaultSavedAt);

            var testEditorViewModel = new EditorViewModel(note);

            note.Title = "NEW TITLE";

            Assert.True(note.IsDirty);
        }

        [Fact]
        public void ResetSnapshot_Works()
        {
            Guid id = Guid.NewGuid();
            string title = "TestTitle";
            string content = "TestContent";

            var note = new Note(id, title, content, _defaultSavedAt, _defaultSavedAt);

            var testEditorViewModel = new EditorViewModel(note);

            note.Content = "askdn";

            Assert.True(note.IsDirty);

            testEditorViewModel.ResetSnapshot();

            Assert.False(note.IsDirty);

            note.Content = "TestContent";

            Assert.True(note.IsDirty);
        }
        [Fact]
        public async Task SaveNote_WhenSuccsess_ResetsSnapshot()
        {
            Guid id = Guid.NewGuid();
            string title = "TestTitle";
            string content = "TestContent";

            var note = new Note(id, title, content, _defaultSavedAt, _defaultSavedAt);

            _mockService.Setup(serv => serv
            .LoadAll()).Returns(new List<Note>());

            _mockEditorViewModelFactory.Setup(fc => fc
            .Create(note)).Returns(_mockEditor.Object);

            var viewModel = new MainWindowViewModel(_mockService.Object,
                _mockDialogueService.Object,
                _mockEditorViewModelFactory.Object);

            viewModel.Notes.Add(note);
            viewModel.SelectedNote = note;

            await viewModel.SaveNoteCommand.ExecuteAsync(null);

            _mockService.Verify(serv => serv
            .SaveNote(It.IsAny<Note>()), Times.Once);
            _mockEditor.Verify(ed => ed
            .ResetSnapshot(), Times.Once);
        }
    }
}
