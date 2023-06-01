using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class FileUtils
    {
        public static void CreateDirectoryIfNotExists(string[] directories)
        {
            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        public static void CreateDirectoryIfNotExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static IEnumerable<string> GetAllFilesInDirectory(string folder, string[]? fileExtensions = null)
        {
            IEnumerable<string> files;
            if (fileExtensions != null)
            {
                files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);
                return files.Where(x => fileExtensions.Contains(Path.GetExtension(x)));
            }
            else
            {
                files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);
            }
            return files;
        }

        public static void DeleteAllFiles(string folder)
        {
            IEnumerable<string> files = GetAllFilesInDirectory(folder);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        public static void DeleteAllLogFilesOlderThanTime(TimeSpan timeSpan)
        {
            IEnumerable<string> files = GetAllFilesInDirectory(Constants.Logfolder);
            foreach(string file in files)
            {
                DateTime creationTime = File.GetCreationTimeUtc(file);
                if(DateTime.UtcNow.Subtract(creationTime) > timeSpan)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
