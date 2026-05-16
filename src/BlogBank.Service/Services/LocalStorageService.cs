using BlogBank.Service.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlogBank.Service.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration config)
    {
        _basePath = config["Storage:Local:BasePath"] ?? "wwwroot/exports";
        _baseUrl  = config["Storage:Local:BaseUrl"]  ?? "http://localhost:5000/exports";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(string localFilePath, string fileName = null)
    {
        fileName ??= Path.GetFileName(localFilePath);
        var destPath = Path.Combine(_basePath, fileName);

        File.Copy(localFilePath, destPath, overwrite: true);

        return $"{_baseUrl}/{fileName}";
    }

    public async Task<string> UploadAsync(Stream stream, string fileName)
    {
        var destPath = Path.Combine(_basePath, fileName);

        await using var fs = File.Create(destPath);
        await stream.CopyToAsync(fs);

        return $"{_baseUrl}/{fileName}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
        var filePath = Path.Combine(_basePath, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task<string> GeneratePresignedUrlAsync(string fileKey, int expireMinutes = 30)
        => Task.FromResult($"{_baseUrl}/{fileKey}"); // 本地不需要签名
}