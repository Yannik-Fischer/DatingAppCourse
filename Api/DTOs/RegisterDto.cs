using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class RegisterDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
