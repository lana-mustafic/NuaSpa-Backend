namespace NuaSpa.Application.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime Expiration { get; set; }
}