namespace JobTracker.Api.Domain;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<JobApplication> Applications { get; set; } = [];
}
