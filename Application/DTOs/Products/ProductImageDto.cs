namespace Application.DTOs.Products
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }

    /// <summary>A file received from the client (kept framework-agnostic).</summary>
    public class ImageUploadDto
    {
        public Stream Content { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
    }
}
