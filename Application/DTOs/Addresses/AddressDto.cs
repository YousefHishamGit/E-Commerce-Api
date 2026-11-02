namespace Application.DTOs.Addresses
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? LocationUrl { get; set; }
    }
}
