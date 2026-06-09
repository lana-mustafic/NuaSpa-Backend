namespace NuaSpa.Application.DTOs;

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;

    /// <summary>Development-only copy of the reset link for local testing without e-mail.</summary>
    public string? DevResetUrl { get; set; }
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }
}
