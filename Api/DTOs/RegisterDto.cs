using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class RegisterDto
{
    [Required]
    public string? Gender { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string? KnownAs { get; set; }
    [Required]
    public string? DateOfBirth { get; set; }
    [Required]
    public string? City { get; set; }
    [Required]
    public string? Country { get; set; }
    [Required]
    [StringLength(8, MinimumLength = 4, ErrorMessage = "The Password must be between {2} and {1} characters.")]
    public string Password { get; set; } = string.Empty;
}
