using Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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

        public static void CreateZipFileSafely()
        {
            using (FileStream zipToOpen = new FileStream(Constants.LogZipfile, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.GetFiles(Constants.Logfolder))
                {
                    var entryName = Path.GetFileName(file);
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    entry.LastWriteTime = File.GetLastWriteTime(file);
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var stream = entry.Open())
                    {
                        fs.CopyTo(stream);
                    }
                }
            }
        }
    }
}
