using Salvager.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class NoteServiceTests : IDisposable
    {
        private readonly string _testNoteDirectory;
        private readonly INoteService noteService;

        public NoteServiceTests()
        {
            _testNoteDirectory = Path.Combine(Path.GetTempPath(), "SalvagerTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testNoteDirectory);
        }
        public void Dispose()
        {
            if (Directory.Exists(_testNoteDirectory))
            {
                Directory.Delete(_testNoteDirectory, true);
            }
        }
    }
}
