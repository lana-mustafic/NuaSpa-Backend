namespace NuaSpa.Worker.Email;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
