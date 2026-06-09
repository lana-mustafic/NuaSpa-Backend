namespace NuaSpa.Application.Configuration;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    /// <summary>Deep link or web URL prefix, e.g. nuaspa://reset-password or https://app.nuaspa.ba/reset-password</summary>
    public string BaseUrl { get; set; } = "nuaspa://reset-password";

    public int TokenLifespanHours { get; set; } = 24;
}
