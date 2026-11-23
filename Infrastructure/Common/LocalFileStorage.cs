using Application.Contracts.Services.Common;

namespace Infrastructure.Common
{
    /// <summary>Stores files on the local disk under a root folder served as static files.</summary>
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _rootPath;
        private readonly string _requestPath;

        /// <param name="rootPath">Physical folder, e.g. {wwwroot}/uploads</param>
        /// <param name="requestPath">Public URL prefix mapped to that folder, e.g. /uploads</param>
        public LocalFileStorage(string rootPath, string requestPath)
        {
            _rootPath = Path.GetFullPath(rootPath);
            _requestPath = "/" + requestPath.Trim('/');
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken ct = default)
        {
            var safeFolder = Path.GetFileName(folder);               // no path separators allowed
            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            var directory = Path.Combine(_rootPath, safeFolder);
            Directory.CreateDirectory(directory);

            await using var target = new FileStream(
                Path.Combine(directory, fileName), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(target, ct);

            return $"{_requestPath}/{safeFolder}/{fileName}";
        }

        public Task DeleteAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(_requestPath + "/", StringComparison.Ordinal))
                return Task.CompletedTask;   // not one of our files (e.g. legacy external URL)

            var relative = url[(_requestPath.Length + 1)..].Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relative));

            // Guard against path traversal
            if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            try { if (File.Exists(fullPath)) File.Delete(fullPath); }
            catch (IOException) { /* best effort */ }

            return Task.CompletedTask;
        }
    }
}
