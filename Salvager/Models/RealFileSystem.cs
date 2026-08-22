using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Salvager.Models
{
    internal class RealFileSystem : IFileSystem
    {
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public void DeleteFile(string path) => File.Delete(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);

        public string[] GetFiles(string path, string pattern) => Directory.GetFiles(path, pattern);
        public string ReadAllText(string path) => File.ReadAllText(path);

        public void WriteAllText(string path, string contents, Encoding encoding) =>
            File.WriteAllText(path, contents, encoding);
    }
}
