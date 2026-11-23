using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Addresses
{
    public class UpdateAddressDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? City { get; set; }
        [StringLength(200, MinimumLength = 3)]
        public string? Street { get; set; }
        [Url, StringLength(500)]
        public string? LocationUrl { get; set; }
    }
}
