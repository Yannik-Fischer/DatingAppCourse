using System;

namespace Api.Helpers;

public class MessageParams : PaginationParams
{
    public string? Username { get; set; }
    public required string Container { get; set; } = "Unread";
}
