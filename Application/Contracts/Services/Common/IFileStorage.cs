namespace Application.Contracts.Services.Common
{
    public interface IFileStorage
    {
        /// <summary>Saves the stream under the given folder and returns its public relative URL.</summary>
        Task<string> SaveAsync(Stream content, string extension, string folder, CancellationToken ct = default);

        /// <summary>Deletes a previously saved file by its public URL. Missing files are ignored.</summary>
        Task DeleteAsync(string url);
    }
}
