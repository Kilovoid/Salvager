using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Models
{
    public partial class Note : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;
        [ObservableProperty]
        private string _title = "New Note";
        [ObservableProperty]
        public string _content = "";
        [ObservableProperty]
        public DateTime _createdAt = DateTime.Now;
        [ObservableProperty]
        public DateTime _updatedAt = DateTime.Now;

        public Note(string title, string content)
        {
            _title = title;
            _content = content;
            _id = Guid.Empty;
            _createdAt = DateTime.Now;
            _updatedAt = DateTime.Now;
        }

        public Note(Guid id, string title, string content, DateTime createdAt, DateTime updatedAt)
        {
            _id = id;
            _title = title;
            _content = content;
            _createdAt = createdAt;
            _updatedAt = UpdatedAt;
        }
    }
}
