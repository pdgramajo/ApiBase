using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace api.Helpers
{
    public class CUtils
    {
        public const string File = "File";
        public static string GetFolderPathToSave(string type)
        {
            switch (type)
            {
                case File:
                    return FilePathToSave();
                default:
                    return "";
            }
        }

        private static string FilePathToSave()
        {
            var folderName = Path.Combine("Resources", "Files");
            var directoryInfo = new DirectoryInfo(folderName);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            return folderName;
        }

        public static string GetFileNameFormated(string fileName)
        {
            var regex = new Regex("[^a-zA-Z0-9]");

            var newFileName = Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"[^a-zA-Z0-9]", "");

            var result = string.Concat(
                DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                "_",
                newFileName,
                Path.GetExtension(fileName)
                );
            return result;
        }

        public static string GetRootFolderToStaticFiles()
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"Resources");
            var directoryInfo = new DirectoryInfo(folderPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            return folderPath;
        }

    }
}