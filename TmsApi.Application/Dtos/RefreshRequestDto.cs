using System.ComponentModel.DataAnnotations;

namespace TmsApi.Api.DTOs;
public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
