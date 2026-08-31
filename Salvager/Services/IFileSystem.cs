using System;
using System.Collections.Generic;
using System.Text;

namespace Salvager.Services
{
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        string[] GetFiles(string path, string pattern);
        void WriteAllText(string path, string contents, Encoding encoding);
        string ReadAllText(string path);
        void CreateDirectory(string path);
        void DeleteFile(string path);
        string CombinePath(string path1, string path2);
        string GetFileNameWithoutExtension(string path);
        char[] GetInvalidFileNameChars();
    }
}
