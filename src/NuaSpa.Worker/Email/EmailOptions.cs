namespace NuaSpa.Worker.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string FromAddress { get; set; } = "noreply@nuaspa.ba";
    public string FromName { get; set; } = "NuaSpa";
    public string OutboxDirectory { get; set; } = "email-outbox";
}
