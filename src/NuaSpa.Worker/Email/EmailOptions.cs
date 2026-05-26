namespace NuaSpa.Worker.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string OutboxDirectory { get; set; } = string.Empty;
}
