using Salvager.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Salvager.Services
{
    public interface INoteService
    {
        Note CreateNote(string title); 
        void DeleteNote(Guid noteId); 
        Note LoadNote(Guid noteId); 
        List<Note> LoadAll(); 
        void SaveNote(Note currentNote); 
        
    }
}
