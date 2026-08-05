using Salvager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Services
{
    public interface INoteService
    {
        List<Note> LoadAllNotes();
        void SaveNote(Note note);
        void DeleteNote(Note note);
        void SaveAll(List<Note> notes);
    }
}
