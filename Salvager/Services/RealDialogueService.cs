using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Salvager.Services
{
    internal class RealDialogueService : IDialogueService
    {
        public async Task ShowErrorAsync(string title, string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok);
            await box.ShowAsync();
        }

        public async Task<ButtonResult> ShowWarningAsync(string title, string message, ButtonEnum buttons = ButtonEnum.Ok)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, buttons);
            return await box.ShowAsync();
        }
    }
}
