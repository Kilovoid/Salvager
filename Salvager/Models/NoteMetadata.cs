using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Salvager.Models
{
    public class NoteMetadata
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "New Note";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
