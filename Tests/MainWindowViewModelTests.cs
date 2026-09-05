using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Salvager.Models;
using Salvager.Services;
using Salvager.ViewModels;

namespace Tests
{
    public class MainWindowViewModelTests
    {
        private readonly Mock<INoteService> _mockService;

        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModelTests()
        {
            _mockService = new Mock<INoteService>();
            _viewModel = new MainWindowViewModel(_mockService.Object);
        }

        [Fact]
        public void Constructor_LoadsNotesFromDisk()
        {
            var note1 = new Note(Guid.NewGuid(), "Note 1", "", DateTime.Now, DateTime.Now);
            var note2 = new Note(Guid.NewGuid(), "Note 2", "", DateTime.Now, DateTime.Now);
            var expectedNoteList = new List<Note> { note1, note2 };

            _mockService.Setup(s => s
            .LoadAll()).Returns(expectedNoteList);

            var viewModel = new MainWindowViewModel(_mockService.Object);

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

            Assert.Equal(1, _viewModel.Notes.Count);
            Assert.Equal("New Note", _viewModel.Notes[0].Title);
            Assert.Equal(_viewModel.Notes[0], _viewModel.SelectedNote);
            Assert.NotNull(_viewModel.SelectedNoteViewModel);
            _mockService.Verify(s => s
            .SaveNote(It.IsAny<Note>()), Times.Once);
        }
    }
}
