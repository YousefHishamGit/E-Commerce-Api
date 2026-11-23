using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Addresses
{
    public class CreateAddressDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string City { get; set; } = null!;
        [Required, StringLength(200, MinimumLength = 3)]
        public string Street { get; set; } = null!;
        [Url, StringLength(500)]
        public string? LocationUrl { get; set; }
    }
}
