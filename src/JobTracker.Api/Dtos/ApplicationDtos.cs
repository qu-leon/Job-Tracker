using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Domain;

namespace JobTracker.Api.Dtos;

public record ApplicationSummaryResponse(
    int Id,
    int CompanyId,
    string CompanyName,
    string RoleTitle,
    string? Location,
    string? Source,
    decimal? SalaryMin,
    decimal? SalaryMax,
    ApplicationStatus Status,
    DateOnly? AppliedDate,
    int InterviewCount,
    DateTimeOffset UpdatedAt);

public record ApplicationDetailResponse(
    int Id,
    int CompanyId,
    string CompanyName,
    string RoleTitle,
    string? JobUrl,
    string? Source,
    string? Location,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? Notes,
    ApplicationStatus Status,
    IReadOnlyList<ApplicationStatus> AllowedNextStates,
    DateOnly? AppliedDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<InterviewResponse> Interviews,
    IReadOnlyList<StatusEventResponse> StatusHistory);

public record ApplicationRequest(
    [property: Required] int CompanyId,
    [property: Required, MaxLength(200)] string RoleTitle,
    [property: MaxLength(1000)] string? JobUrl,
    [property: MaxLength(100)] string? Source,
    [property: MaxLength(200)] string? Location,
    [property: Range(0, 10_000_000)] decimal? SalaryMin,
    [property: Range(0, 10_000_000)] decimal? SalaryMax,
    [property: MaxLength(4000)] string? Notes,
    DateOnly? AppliedDate);

public record StatusChangeRequest(
    [property: Required] ApplicationStatus ToStatus,
    [property: MaxLength(1000)] string? Note);

public record StatusEventResponse(
    int Id,
    ApplicationStatus? FromStatus,
    ApplicationStatus ToStatus,
    DateTimeOffset ChangedAt,
    string? Note);

public record InterviewResponse(
    int Id,
    int ApplicationId,
    InterviewStage Stage,
    DateTimeOffset ScheduledAt,
    string? InterviewerName,
    InterviewOutcome Outcome,
    string? Notes);

public record InterviewRequest(
    [property: Required] InterviewStage Stage,
    [property: Required] DateTimeOffset ScheduledAt,
    [property: MaxLength(200)] string? InterviewerName,
    InterviewOutcome Outcome,
    [property: MaxLength(4000)] string? Notes);

public record PipelineStatsResponse(
    int TotalApplications,
    int ActiveApplications,
    int UpcomingInterviews,
    double ResponseRate,
    IReadOnlyDictionary<string, int> ByStatus);
