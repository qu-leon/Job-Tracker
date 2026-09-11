namespace JobTracker.Api.Domain;

public class JobApplication
{
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public string RoleTitle { get; set; } = string.Empty;
    public string? JobUrl { get; set; }
    public string? Source { get; set; }
    public string? Location { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Notes { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Saved;
    public DateOnly? AppliedDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Interview> Interviews { get; set; } = [];
    public List<StatusEvent> StatusHistory { get; set; } = [];
}
