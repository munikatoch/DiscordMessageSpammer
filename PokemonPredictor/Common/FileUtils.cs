using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonPredictor.Common
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

        public static async Task WriteToFile(string fileName, string content)
        {
            await File.AppendAllTextAsync(fileName, content);
        }

        public static void DeleteAllFiles(string folder)
        {
            IEnumerable<string> files = GetAllFilesInDirectory(folder);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
