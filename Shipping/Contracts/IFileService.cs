namespace Shipping.Contracts;

public interface IFileService
{
    /// <summary>
    /// Saves file to disk at the specified path relative to the assets directory
    /// </summary>
    /// <param name="file"></param>
    /// <param name="absolutePath">Relative to assets directory</param>
    /// <returns>
    /// Physical path to the file
    /// </returns>
    Task<string> SaveFileAsync(IFormFile file, string absolutePath);

    string GetPublicUrl(ReadOnlySpan<char> filePath);
    string GetPhysicalPath(ReadOnlySpan<char> base64EncodedFilePath);
    void DeleteFile(string filePath);
}