using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface IBlobStorageRepository
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string blobName, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string blobName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default);
    string GetFileUrl(string blobName);
    string GetSasUrl(string blobName, TimeSpan expiry);
    Task<IEnumerable<string>> ListFilesAsync(string? folder = null, CancellationToken cancellationToken = default);
}
