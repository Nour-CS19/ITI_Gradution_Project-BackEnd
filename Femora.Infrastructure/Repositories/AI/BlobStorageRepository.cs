using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class BlobStorageRepository : IBlobStorageRepository
{
    private readonly BlobContainerClient _containerClient;
    private readonly AzureBlobStorageOptions _options;
    private readonly IMemoryCache _cache;

    public BlobStorageRepository(IOptions<AzureBlobStorageOptions> options, ILogger<BlobStorageRepository> logger, IMemoryCache cache)
    {
        _options = options.Value;
        _cache = cache;
        var serviceClient = new BlobServiceClient(_options.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(_options.ContainerName);

        try
        {
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
        {
            // The storage account has "Allow Blob public access" disabled at the account
            // level (Azure's secure-by-default setting). Public access can't be granted
            // per-container in that case, so fall back to a private container instead of
            // crashing the app/seeder on every startup. Uploads still work, but the plain
            // (non-SAS) ImageUrl this repository returns will 403 for anonymous viewers
            // until either the account setting is enabled or callers switch to GetSasUrl().
            logger.LogWarning(
                "Storage account '{Container}' disallows public blob access at the account level. " +
                "Falling back to a private container — enable 'Allow Blob public access' on the storage " +
                "account (or serve images via GetSasUrl) so uploaded images are publicly viewable.",
                _options.ContainerName);
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        // If the container already existed (e.g. created previously with no
        // public access), make sure it's set to allow anonymous blob reads.
        // Otherwise every ImageUrl saved in the DB (a plain, non-SAS URL)
        // returns 403 and images never render on the client.
        try
        {
            var accessPolicy = _containerClient.GetAccessPolicy();
            if (accessPolicy.Value.BlobPublicAccess != PublicAccessType.Blob)
            {
                _containerClient.SetAccessPolicy(PublicAccessType.Blob);
            }
        }
        catch
        {
            // Best-effort: some storage accounts disable "allow blob public access"
            // at the account level, in which case this call will fail and we fall
            // back to whatever access level already exists.
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default)
    {
        var blobName = BuildBlobName(fileName, folder);
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } }, cancellationToken);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
            throw new FileNotFoundException($"Blob '{blobName}' not found.");
        var download = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return download.Value.Content;
    }

    public async Task<bool> DeleteFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    public async Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public string GetFileUrl(string blobName)
        => _containerClient.GetBlobClient(blobName).Uri.ToString();

    public string GetSasUrl(string blobName, TimeSpan expiry)
    {
        // Lesson/course content is requested repeatedly for the same blob (every time a
        // trainee opens a lesson, every module list load, etc.), and generating a SAS URI
        // is pure CPU/crypto work with no network call - but it's still wasted work if we
        // redo it every single time. Cache the SAS URL itself for a bit less than its own
        // expiry, so repeat requests for the same blob within that window are free, while
        // guaranteeing we never hand out a URL that's about to expire.
        var cacheKey = $"sas:{blobName}:{expiry.Ticks}";
        if (_cache.TryGetValue(cacheKey, out string? cachedUrl) && cachedUrl is not null)
            return cachedUrl;

        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("Cannot generate SAS URL with current credentials.");
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        var sasUrl = blobClient.GenerateSasUri(sasBuilder).ToString();

        // Cache for 80% of the SAS lifetime (a safety margin before the URL actually
        // expires), capped so we never cache for more than ~a day even for long expiries.
        var cacheDuration = TimeSpan.FromTicks((long)(expiry.Ticks * 0.8));
        if (cacheDuration > TimeSpan.FromHours(24))
            cacheDuration = TimeSpan.FromHours(24);

        _cache.Set(cacheKey, sasUrl, cacheDuration);
        return sasUrl;
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string? folder = null, CancellationToken cancellationToken = default)
    {
        var prefix = string.IsNullOrWhiteSpace(folder) ? null : $"{folder.Trim('/')}/";
        var blobNames = new List<string>();
        await foreach (var blobItem in _containerClient.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, prefix, cancellationToken))
            blobNames.Add(blobItem.Name);
        return blobNames;
    }

    private static string BuildBlobName(string fileName, string? folder)
    {
        var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
        return string.IsNullOrWhiteSpace(folder) ? uniqueName : $"{folder.Trim('/')}/{uniqueName}";
    }
}
