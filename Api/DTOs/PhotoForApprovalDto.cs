namespace Api.DTOs;

public class PhotoForApprovalDto
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public required string Username { get; set; }
    public bool IsApproved { get; set; }
}
