using System;
using System.Collections.Generic;
using System.Text;
using Salvager.Models;

namespace Salvager.ViewModels
{
    public class EditorViewModelFactory : IEditorViewModelFactory
    {
        public IEditorViewModel Create(Note note) => new EditorViewModel(note);
    }

    public interface IEditorViewModelFactory
    {
        IEditorViewModel Create(Note note);
    }
}
