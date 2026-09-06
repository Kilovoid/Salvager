using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Salvager.Services
{
    public interface IDialogueService
    {
        Task<ButtonResult> ShowWarningAsync(string title, string message, ButtonEnum buttons = ButtonEnum.Ok);
        Task ShowErrorAsync(string title, string message);
    }
}
