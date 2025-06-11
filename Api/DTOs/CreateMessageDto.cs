using System;

namespace Api.DTOs;

public class CreateMessageDto
{
    public required string RecipientUsername { get; set; }
    public required string Content { get; set; }
}
