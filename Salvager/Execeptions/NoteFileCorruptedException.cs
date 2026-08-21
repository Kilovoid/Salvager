using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Execeptions
{
    public class NoteFileCorruptedException : Exception
    {
        public string FilePath { get; }
        public Guid? NoteId { get; }

        public NoteFileCorruptedException(string filePath, string message)
            : base(message)
        {
            FilePath = filePath;
        }
        public NoteFileCorruptedException(string filePath, string message, Exception innerException)
            : base(message, innerException)
        {
            FilePath = filePath;
        }
        public NoteFileCorruptedException(string filePath, Guid noteId, string message)
            : base(message)
        {
            FilePath = filePath;
            NoteId = noteId;
        }
    }
}
