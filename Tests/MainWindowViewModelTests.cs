using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using MsBox.Avalonia.Enums;
using Salvager.Models;
using Salvager.Services;
using Salvager.ViewModels;

namespace Tests
{
    public class MainWindowViewModelTests
    {
        private readonly Mock<INoteService> _mockService;

        private readonly MainWindowViewModel _viewModel;

        private readonly Mock<IDialogueService> _mockDialogueService;

        public MainWindowViewModelTests()
        {
            _mockService = new Mock<INoteService>();
            _mockDialogueService = new Mock<IDialogueService>();
            _mockService.Setup(s => s
            .LoadAll()).Returns(new List<Note>());
            _viewModel = new MainWindowViewModel(_mockService.Object, _mockDialogueService.Object);
        }

        [Fact]
        public void Constructor_LoadsNotesFromDisk()
        {
            var note1 = new Note(Guid.NewGuid(), "Note 1", "", DateTime.Now, DateTime.Now);
            var note2 = new Note(Guid.NewGuid(), "Note 2", "", DateTime.Now, DateTime.Now);
            var expectedNoteList = new List<Note> { note1, note2 };

            _mockService.Setup(s => s
            .LoadAll()).Returns(expectedNoteList);
            _mockService.Invocations.Clear();

            var viewModel = new MainWindowViewModel(_mockService.Object, _mockDialogueService.Object);

            Assert.Equal(2, viewModel.Notes.Count);
            Assert.Equal(note1.Id, viewModel.Notes[0].Id);
            Assert.Equal(note1.Title, viewModel.Notes[0].Title);
            Assert.Equal(note1.Id, viewModel.SelectedNote.Id);
            Assert.NotNull(viewModel.SelectedNoteViewModel);
            Assert.Equal(note1.Id, viewModel.SelectedNoteViewModel.CurrentPage.Id);

            _mockService.Verify(s => s
            .LoadAll(), Times.Once);
        }

        [Fact]
        public void CreateNewNote_Works()
        {
            _viewModel.CreateNewNoteCommand.Execute(null);

            Assert.Single (_viewModel.Notes);
            Assert.Equal("New Note", _viewModel.Notes[0].Title);
            Assert.Equal(_viewModel.Notes[0], _viewModel.SelectedNote);
            Assert.NotNull(_viewModel.SelectedNoteViewModel);
            _mockService.Verify(s => s
            .SaveNote(It.IsAny<Note>()), Times.Once);
        }

        [Fact]
        public async Task SaveNote_SelectedNoteIsNull_Returns()
        {
            _mockService.Setup(s => s
            .LoadAll()).Returns(new List<Note>());

            _viewModel.SelectedNote = null;

            await _viewModel.SaveNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .SaveNote(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public async Task SaveNote_Works()
        {
            var note = new Note(Guid.NewGuid(), "Test Note", "", DateTime.Now, DateTime.Now);

            _mockService.Setup(s => s
            .LoadAll()).Returns([note]);

            _viewModel.SelectedNote = note;

            await _viewModel.SaveNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .SaveNote(It.IsAny<Note>()), Times.Once);
        }

        [Theory]
        [InlineData(typeof(ArgumentNullException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(IOException))]
        [InlineData(typeof(Exception))]
        public async Task SaveNote_Error_CallsMessageBox(Type exception)
        {
            var note = new Note(Guid.NewGuid(), "Test Note", "", DateTime.Now, DateTime.Now);

            //_mockService.Setup(s => s
            //.LoadAll()).Returns([note]);
            _viewModel.Notes.Add(note);
            _viewModel.SelectedNote = note;
            var expectedException = (Exception)Activator.CreateInstance(exception);
            _mockService.Setup(s => s
            .SaveNote(note)).Throws(expectedException);
            await _viewModel.SaveNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .SaveNote(It.IsAny<Note>()), Times.Once);
            _mockDialogueService.Verify(dial => dial
            .ShowErrorAsync(It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteNote_SelectedNoteNull_Returns()
        {
            _mockService.Setup(s => s
            .LoadAll()).Returns(new List<Note>());

            _viewModel.SelectedNote = null;

            await _viewModel.DeleteNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .DeleteNote(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteNote_OnCancelPressed_Returns()
        {
            var note = new Note(Guid.NewGuid(), "Test Note", "", DateTime.Now, DateTime.Now);
            _viewModel.Notes.Add(note);
            _viewModel.SelectedNote = note;

            _mockDialogueService.Setup(dial => dial
            .ShowWarningAsync("Warning",
            "Are you sure you want to delete this note?",
            ButtonEnum.OkCancel)).ReturnsAsync(ButtonResult.Cancel);

            await _viewModel.DeleteNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .DeleteNote(It.IsAny<Guid>()), Times.Never);
            Assert.NotEmpty(_viewModel.Notes);
            Assert.NotNull(_viewModel.SelectedNote);
        }

        [Fact]
        public async Task DeleteNote_OnOkPressed_Works()
        {
            var note = new Note(Guid.NewGuid(), "Test Note", "", DateTime.Now, DateTime.Now);
            _viewModel.Notes.Add(note);
            _viewModel.SelectedNote = note;

            _mockDialogueService.Setup(dial => dial
            .ShowWarningAsync("Warning",
            "Are you sure you want to delete this note?",
            ButtonEnum.OkCancel)).ReturnsAsync(ButtonResult.Ok);

            await _viewModel.DeleteNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .DeleteNote(It.IsAny<Guid>()), Times.Once);
            Assert.Empty(_viewModel.Notes);
            Assert.Null(_viewModel.SelectedNote);
            Assert.Null(_viewModel.SelectedNoteViewModel);
        }

        [Theory]
        [InlineData(typeof(ArgumentNullException))]
        [InlineData(typeof(ArgumentException))]
        public async Task DeleteNote_Error_CallsMessageBox(Type exception)
        {
            Guid testGuid = Guid.NewGuid();
            var note = new Note(testGuid, "Test Note", "", DateTime.Now, DateTime.Now);

            //_mockService.Setup(s => s
            //.LoadAll()).Returns([note]);
            _viewModel.Notes.Add(note);
            _viewModel.SelectedNote = note;
            var expectedException = (Exception)Activator.CreateInstance(exception);
            _mockService.Setup(s => s
            .DeleteNote(testGuid)).Throws(expectedException);
            await _viewModel.DeleteNoteCommand.ExecuteAsync(null);

            _mockService.Verify(s => s
            .DeleteNote(It.IsAny<Guid>()), Times.Once);
            _mockDialogueService.Verify(dial => dial
            .ShowErrorAsync(It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
            Assert.NotNull(_viewModel.SelectedNote);
            Assert.NotEmpty(_viewModel.Notes);
        }
    }
}
