using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Models
{
    public partial class Note : ObservableObject
    {

        [ObservableProperty]
        private string _title = "New Note";
        [ObservableProperty]
        public string _content = "";
        [ObservableProperty]
        public DateTime _createdAt = DateTime.Now;

    }
}
