using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Blob.Demo.Model;
using Blob.Demo.Service.Contract;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata;
using static System.Reflection.Metadata.BlobBuilder;

namespace Blob.Demo.Service;

public class BlobService : IBlobService
{
    private readonly BlobContainerClient _blobConteinerClient;

    public BlobService(BlobServiceConfig blobConfig)
    {
        _blobConteinerClient =new BlobContainerClient(blobConfig.ConnectionString, blobConfig.ContainerName);
    }

    public async Task<BlobResponseDto> DeleteAsync(string blobFilename)
    {
        BlobClient file = _blobConteinerClient.GetBlobClient(blobFilename);

        try
        {
            await file.DeleteAsync();
        }
        catch (RequestFailedException ex) 
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            return new BlobResponseDto { Error = true, Status = $"File with name {blobFilename} not found." };
        }

        return new BlobResponseDto { Error = false, Status = $"File: {blobFilename} has been successfully deleted." };
    }

    public async Task<BlobDto> DownloadAsync(string blobFilename)
    {
        try
        {
            BlobClient file = _blobConteinerClient.GetBlobClient(blobFilename);

            if (await file.ExistsAsync())
            {
                Stream blobContent = await file.OpenReadAsync();
                var content = await file.DownloadContentAsync();

                return new BlobDto
                {
                    Content = blobContent,
                    Name= blobFilename,
                    ContentType= content.Value.Details.ContentType
                };
            }

        }
        catch (Exception)
        {

            throw;
        }

        return null;
    }

    public async Task<List<BlobDto>> ListAsync()
    {
        var files = new List<BlobDto>();

        await foreach (BlobItem file in _blobConteinerClient.GetBlobsAsync())
        {
            files.Add(new BlobDto
            {
                Uri = $"{_blobConteinerClient.Uri}/{file.Name}",
                Name= file.Name,
                ContentType = file.Properties.ContentType
            });
        }

        return files;
    }

    public async Task<BlobResponseDto> UploadAsync(IFormFile file)
    {
        BlobResponseDto response = new();
        await _blobConteinerClient.CreateIfNotExistsAsync();

        try
        {
            BlobClient client = _blobConteinerClient.GetBlobClient(file.FileName);

            await using (Stream? data = file.OpenReadStream())
            {
                await client.UploadAsync(data);
            }

            response.Status = $"File {file.FileName} Uploaded Successfully";
            response.Error = false;
            response.Blob.Uri = client.Uri.AbsoluteUri;
            response.Blob.Name = client.Name;
        }
        catch (RequestFailedException ex)
            when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
        {
            response.Status = $"File with name {file.FileName} already exists. Please use another name to store your file.";
            response.Error = true;
            return response;
        }
        catch (RequestFailedException ex)
        {
            response.Status = $"Unexpected error: {ex.Message}.";
            response.Error = true;
            return response;
        }

        return response;
    }
}
