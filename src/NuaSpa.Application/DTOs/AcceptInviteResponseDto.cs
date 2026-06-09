namespace NuaSpa.Application.DTOs;

public class AcceptInviteResponseDto
{
    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }
}
