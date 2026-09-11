namespace JobTracker.Api.Domain;

/// <summary>
/// Append-only audit record written on every accepted status change.
/// </summary>
public class StatusEvent
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public JobApplication? Application { get; set; }

    public ApplicationStatus? FromStatus { get; set; }
    public ApplicationStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Note { get; set; }
}
