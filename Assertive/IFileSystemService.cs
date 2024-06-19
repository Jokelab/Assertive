namespace Assertive
{
    public interface IFileSystemService
    {
        string CalculateRelativePath(string currentPath, string relativePath);
        string GetFileContent(string path);
        Stream GetFileStream(string path);

        bool FileExists(string path);
    }
}