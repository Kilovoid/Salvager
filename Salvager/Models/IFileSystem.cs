using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Models
{
    internal interface IFileSystem
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        string[] GetFiles(string path, string pattern);
        void WriteAllText(string path, string contents, Encoding encoding);
        string ReadAllText(string path);
        void CreateDirectory(string path);
        void DeleteFile(string path);
    }
}
