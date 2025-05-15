using System.Buffers.Text;
using System.Text;

namespace Shipping.Services;

internal sealed class FileService : IFileService
{
    private readonly IHttpContextAccessor contextAccessor;
    private const string AssetsDirectory = "wwwroot";
    private static string? Host;
    private const char Slash = '/';
    private const byte SlashByte = (byte)'/';

    private const char Plus = '+';
    private const byte PlusByte = (byte)'+';

    private const char Hyphen = '-';
    private const char Underscore = '_';
    private const char Equal = '=';
    private const byte ByteEqual = (byte)'=';
    private readonly string assetsPath;
    public FileService(IHttpContextAccessor contextAccessor)
    {
        this.contextAccessor = contextAccessor;
        string projectDirectory = Directory.GetCurrentDirectory();
                
        assetsPath = Path.Combine(projectDirectory, AssetsDirectory);
        
        if (!Directory.Exists(assetsPath))
        {
            Directory.CreateDirectory(assetsPath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string absolutePath)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        
        var uploadPath = Path.Combine(assetsPath, absolutePath);

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var filePath = Path.Combine(uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream);

        return Path.Combine(absolutePath, fileName);
    }

    public string GetPublicUrl(ReadOnlySpan<char> filePath)
    {
        if (string.IsNullOrEmpty(Host))
        {
            var request = contextAccessor.HttpContext?.Request;
            Host = $"{request?.Scheme}://{request?.Host}/api";
        }

        int bytesCount = Encoding.UTF8.GetByteCount(filePath);

        Span<byte> bytes = stackalloc byte[bytesCount];
        
        var bytesWriten = Encoding.UTF8.GetBytes(filePath, bytes);

        Span<char> encodedChars = stackalloc char[Base64.GetMaxEncodedToUtf8Length(bytesWriten)];
        Convert.TryToBase64Chars(bytes, encodedChars, out _);
        var nonPaddingLength = encodedChars.Length;
        while (encodedChars[nonPaddingLength - 1] == Equal) nonPaddingLength--;
       
        for (int i = 0; i < nonPaddingLength; i++)
        {
            encodedChars[i] = encodedChars[i] switch
            {
                Plus => Hyphen,
                Slash => Underscore,
                _ => encodedChars[i]
            };
        }
        return $"{Host}/{new string(encodedChars.Slice(0, nonPaddingLength))}";
    }

    public string GetPhysicalPath(ReadOnlySpan<char> base64EncodedFilePath)
    {

        int paddingCount = (4 - (base64EncodedFilePath.Length % 4)) % 4;

        Span<byte> base64Bytes = stackalloc byte[base64EncodedFilePath.Length + paddingCount];
        
        for (int i = 0; i < base64EncodedFilePath.Length; i++)
        {
            base64Bytes[i] = base64EncodedFilePath[i] switch
            {
                Hyphen => PlusByte,
                Underscore => SlashByte,
                _ => (byte)base64EncodedFilePath[i]
            };
        }

        if (paddingCount > 0)
        {
            for (int i = 0; i < paddingCount; i++)
            {
                base64Bytes[base64EncodedFilePath.Length + i] = ByteEqual;
            }
        }

        int maxDecodedLength = Base64.GetMaxDecodedFromUtf8Length(base64Bytes.Length);
        Span<byte> decodedBytes = maxDecodedLength <= 256 ? stackalloc byte[maxDecodedLength] : new byte[maxDecodedLength];

        Base64.DecodeFromUtf8(base64Bytes, decodedBytes, out _, out int bytesWritten);

        Span<char> decodedFilePath = stackalloc char[bytesWritten];
        for(int i = 0; i < bytesWritten; i++)
        {
            decodedFilePath[i] = (char)decodedBytes[i];
        }
        return Path.Combine(assetsPath, decodedFilePath.ToString());
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
       
        var absolutePath = Path.Combine(assetsPath, filePath);
        
        if(File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }
}