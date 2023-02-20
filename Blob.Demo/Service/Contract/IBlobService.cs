using Blob.Demo.Model;
using Microsoft.AspNetCore.Http;

namespace Blob.Demo.Service.Contract;

public interface IBlobService
{
    Task<BlobResponseDto> UploadAsync(IFormFile file);

    Task<BlobDto> DownloadAsync(string blobFilename);

    Task<BlobResponseDto> DeleteAsync(string blobFilename);

    Task<List<BlobDto>> ListAsync();
}
