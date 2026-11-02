namespace Application.DTOs.Addresses
{
    public class CreateAddressDto
    {
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? LocationUrl { get; set; }
    }
}
