namespace NuaSpa.Application.DTOs;

public class ChangePasswordResponseDto
{
    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public DateTime RefreshExpiration { get; set; }
}
