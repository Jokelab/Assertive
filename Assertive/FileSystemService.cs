namespace Assertive
{
    internal class FileSystemService : IFileSystemService
    {
        public string GetFileContent(string path)
        {
            return File.ReadAllText(path);
        }

        public Stream GetFileStream(string path)
        {
            return File.OpenRead(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string CalculateRelativePath(string currentPath, string relativePath)
        {
            // Convert the current path to a full path
            string fullPath = Path.GetFullPath(currentPath);

            // Check if the current path is a file or directory
            if (File.Exists(fullPath) || (!Directory.Exists(fullPath) && Path.HasExtension(fullPath)))
            {
                // If it's a file, get its directory
                fullPath = Path.GetDirectoryName(fullPath)!;
            }

            // Create a Uri object for the base path
            Uri baseUri = new Uri(fullPath + Path.DirectorySeparatorChar);

            // Create a Uri object for the relative path
            Uri relativeUri = new Uri(relativePath, UriKind.RelativeOrAbsolute);

            // If the relativeUri is actually an absolute Uri, return its LocalPath
            if (relativeUri.IsAbsoluteUri)
            {
                return relativeUri.LocalPath;
            }

            // Combine the base Uri with the relative Uri
            Uri resultUri = new Uri(baseUri, relativeUri);

            // Return the combined path as a string
            return resultUri.LocalPath;
        }

       
    }
}
