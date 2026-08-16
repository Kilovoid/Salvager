using Salvager.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Salvager.Services
{
    public interface INoteService
    {
        Note CreateNote(string title); ///Точно ли title - хочу разобраться - можно ли как-то сюда поставить какое-то
        ///default значение? Или просто проще передавать всегда то, что в виджете создания (я склоняюсь к этому)
        void DeleteNote(Guid noteId); ///По идее к каждому объекту в левом столбце привязана Note - полноценный объект, обращусь по нему
        Note LoadNote(Guid noteId); ///Или LoadNote(). В любом случае тут сложнее - а по чему обращаться? См. выше
        List<Note> LoadAll(); ///Может деприцировать этот метод? Он как будто не для GUI. Заменить на SaveAll()?
        void SaveNote(Note currentNote); ///Сохраняется выбранная по нажатию кнопки Save
        
    }
}
