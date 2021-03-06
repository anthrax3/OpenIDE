using System;
using System.IO;
using OpenIDE.Core.Logging;

namespace OpenIDE.Core.FileSystem
{
	public class FS : IFS
	{
        public static string GetTempPath()
        {
            if (OS.IsOSX) {
                return "/tmp";
            }
            return Path.GetTempPath();
        }

        public static string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

		public string[] GetFiles(string path, string searchPattern)
        {
            return GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public string[] GetFiles(string path, string searchPattern, SearchOption option)
        {
            return Directory.GetFiles(path, searchPattern, option);
        }

		public string[] ReadLines(string path)
		{
			return File.ReadAllLines(path);
		}
		
        public string ReadFileAsText(string path)
        {
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
		
		public bool FileExists(string file)
		{
			return File.Exists(file);
		}
		
		public void WriteAllText(string file, string text)
		{
			File.WriteAllText(file, text);
		}
		
		public void DeleteFile(string file)
		{
			File.Delete(file);
		}
	}
}

