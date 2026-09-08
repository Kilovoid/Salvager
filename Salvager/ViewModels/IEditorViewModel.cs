using Salvager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Salvager.ViewModels
{
    public interface IEditorViewModel
    {
        void ResetSnapshot();
        Note CurrentPage { get; }
    }
}
