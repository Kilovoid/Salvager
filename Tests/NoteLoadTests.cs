using Moq;
using Salvager.Abstractions;
using Salvager.Models;
using Salvager.Services;
using System;
using System.Text;

namespace Tests
{
    public class NoteLoadTests
    {
        private readonly NoteService _service;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IClock> _mockClock;
        private readonly string _testRoot = Path.Combine(
            Path.GetTempPath(), "SalvagerTests");

        public NoteLoadTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger>();
            _mockClock = new Mock<IClock>();

            _mockFileSystem
                .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                .Returns(true);

            _mockFileSystem
                .Setup(fs => fs.GetFiles(It.IsAny<string>(), "*.md"))
                .Returns(Array.Empty<string>());

            _service = new NoteService(
                _mockFileSystem.Object,
                _mockLogger.Object,
                _mockClock.Object,
                _testRoot);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void LoadNote_WithoutBlankLineAfterFrontmatter_KeepsFirstContentLine(
            string newline)
        {
            Guid id = Guid.NewGuid();
            string path = Path.Combine(_testRoot, "Test.md");

            string expectedContent =
                $"First line{newline}Second line";

            string fileContent =
                BuildFrontmatter(id, newline) + expectedContent;

            SetupFile(path, fileContent);

            Note loadedNote = _service.LoadNote(id);

            Assert.Equal(id, loadedNote.Id);
            Assert.Equal(expectedContent, loadedNote.Content);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void LoadNote_WithLeadingBlankLines_PreservesContent(
            string newline)
        {
            Guid id = Guid.NewGuid();
            string path = Path.Combine(_testRoot, "Test.md");

            string expectedContent =
                $"{newline}{newline}First line{newline}Second line{newline}";

            string fileContent =
                BuildFrontmatter(id, newline)
                + newline
                + expectedContent;

            SetupFile(path, fileContent);

            Note loadedNote = _service.LoadNote(id);

            Assert.Equal(expectedContent, loadedNote.Content);
        }

        private void SetupFile(string path, string content)
        {
            _mockFileSystem
                .Setup(fs => fs.GetFiles(_testRoot, "*.md"))
                .Returns(new[] { path });

            _mockFileSystem
                .Setup(fs => fs.ReadAllText(path))
                .Returns(content);

            _mockFileSystem
                .Setup(fs => fs.FileExists(path))
                .Returns(true);
        }

        private static string BuildFrontmatter(Guid id, string newline)
        {
            return string.Join(newline, new[]
            {
                "---",
                $"id: {id}",
                "title: Test",
                "createdAt: 2026-01-01T12:00:00",
                "updatedAt: 2026-01-02T12:00:00",
                "---"
            }) + newline;
        }
    }
}
