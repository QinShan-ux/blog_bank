namespace BlogBank.Service.Interfaces;

public interface IStorageService
{
    /// <summary>上传文件，返回下载地址</summary>
    Task<string> UploadAsync(string localFilePath, string fileName = null);

    /// <summary>上传流</summary>
    Task<string> UploadAsync(Stream stream, string fileName);

    /// <summary>删除文件</summary>
    Task DeleteAsync(string fileUrl);

    /// <summary>生成临时下载链接（有过期时间）</summary>
    Task<string> GeneratePresignedUrlAsync(string fileKey, int expireMinutes = 30);
}