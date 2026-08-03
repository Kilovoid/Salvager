using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Models
{
    public class Page
    {
        public string Title { get; set; } = "New Note";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
