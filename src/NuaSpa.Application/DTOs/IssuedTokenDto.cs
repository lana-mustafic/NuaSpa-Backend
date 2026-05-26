namespace NuaSpa.Application.DTOs;

public class IssuedTokenDto
{
    public string Token { get; set; } = null!;
    public string Jti { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
}
