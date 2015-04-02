using System;
using System.Collections.Generic;
using System.IO;

namespace MarkupDiff
{
    /// <summary>
    /// Utility library for doing things with filesystem.
    /// </summary>
    public class FileSystemHelper
    {
        /// <summary>
        /// Gets a list of file names for files nested under a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetFilesUnder(string path)
        {
            List<string> files = new List<string>();

            GetFilesUnderInternal(path, null, null, files);

            return files.ToArray();
        }

        public static string[] GetFilesUnder(string path, IEnumerable<string> fileTypes)
        {
            List<string> files = new List<string>();

            GetFilesUnderInternal(path, fileTypes, null, files);

            return files.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Path to search in</param>
        /// <param name="fileTypes">File types to filter for. Nullable.</param>
        /// <param name="ignoreFolders">List of folders to ignore. Case-insensitive. Nullable.</param>
        /// <param name="files">Holder of files to return</param>
        private static void GetFilesUnderInternal(
            string path,
            IEnumerable<string> fileTypes,
            ICollection<string> ignoreFolders,
            ICollection<string> files
            )
        {
            try
            {
                // handles files
                if (!Directory.Exists(path))
                    return;

                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] filesInDir = null;

                if (fileTypes == null)
                    filesInDir = dir.GetFiles();
                else
                {
                    string search = "";

                    foreach (string fileType in fileTypes)
                        search += "*." + fileType;

                    filesInDir = dir.GetFiles(search);
                }

                foreach (FileInfo file in filesInDir)
                {
                    try
                    {
                        files.Add(file.FullName);
                    }
                    catch (PathTooLongException)
                    {
                        // suppress these
                    }
                }


                // handles folders
                DirectoryInfo[] dirs = dir.GetDirectories();

                foreach (DirectoryInfo child in dirs)
                    if (ignoreFolders == null || (!ignoreFolders.Contains(child.FullName.ToLower()) && !ignoreFolders.Contains(child.Name)))
                        GetFilesUnderInternal(
                            child.FullName,
                            fileTypes,
                            ignoreFolders,
                            files);
            }
            catch (UnauthorizedAccessException)
            {
                // suppress these exceptions
            }
            catch (PathTooLongException)
            {
                // suppress these, they will be hit a lot
            }
        }
    }
}
