namespace JobTracker.Api.Domain;

public class Interview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public JobApplication? Application { get; set; }

    public InterviewStage Stage { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public string? InterviewerName { get; set; }
    public InterviewOutcome Outcome { get; set; } = InterviewOutcome.Pending;
    public string? Notes { get; set; }
}
