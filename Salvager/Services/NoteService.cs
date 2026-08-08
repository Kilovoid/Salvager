using Salvager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Salvager.Services
{
    internal class NoteService : INoteService
    {
        public Note CreateNote(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("Title cannot be null", nameof(title));
            }
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                title = title.Replace(c, '_');
            }
            Note newNote = new Note ///Возможные ошибки - пустой заголовок, еще невалидные знаки! | СДЕЛАНО
            {
                Title = title,
                Content = ""
            };
            return newNote;
        }

        public void DeleteNote(Guid noteId)
        {
            ///Здесь тоже нужно проверять Guid. Что такое Guid по сути - в Java не было такого типа

        }

        public List<Note> LoadAll()
        {
            throw new NotImplementedException();
        }

        public Note LoadNote()
        {
            throw new NotImplementedException();
        }

        public void SaveNote()
        {

        }
    }
}
