using JobTracker.Api.Data;
using JobTracker.Api.Domain;
using JobTracker.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Produces("application/json")]
public class ApplicationsController(JobTrackerDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationSummaryResponse>>> GetAll(
        [FromQuery] ApplicationStatus? status,
        [FromQuery] int? companyId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Applications.AsNoTracking();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (companyId.HasValue) query = query.Where(a => a.CompanyId == companyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a => a.RoleTitle.Contains(term) || a.Company!.Name.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApplicationSummaryResponse(
                a.Id, a.CompanyId, a.Company!.Name, a.RoleTitle, a.Location, a.Source,
                a.SalaryMin, a.SalaryMax, a.Status, a.AppliedDate,
                a.Interviews.Count, a.UpdatedAt))
            .ToListAsync(ct);

        return Ok(new PagedResult<ApplicationSummaryResponse>(items, page, pageSize, total));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationDetailResponse>> GetById(int id, CancellationToken ct)
    {
        var app = await db.Applications
            .AsNoTracking()
            .Include(a => a.Company)
            .Include(a => a.Interviews)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return app is null ? NotFound() : Ok(ToDetail(app));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplicationDetailResponse>> Create(ApplicationRequest request, CancellationToken ct)
    {
        if (!await db.Companies.AnyAsync(c => c.Id == request.CompanyId, ct))
        {
            ModelState.AddModelError(nameof(request.CompanyId), $"Company {request.CompanyId} does not exist.");
            return ValidationProblem(ModelState);
        }

        if (request.SalaryMin.HasValue && request.SalaryMax.HasValue && request.SalaryMin > request.SalaryMax)
        {
            ModelState.AddModelError(nameof(request.SalaryMin), "SalaryMin cannot exceed SalaryMax.");
            return ValidationProblem(ModelState);
        }

        var app = new JobApplication
        {
            CompanyId = request.CompanyId,
            RoleTitle = request.RoleTitle,
            JobUrl = request.JobUrl,
            Source = request.Source,
            Location = request.Location,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Notes = request.Notes,
            AppliedDate = request.AppliedDate,
            Status = request.AppliedDate.HasValue ? ApplicationStatus.Applied : ApplicationStatus.Saved
        };

        app.StatusHistory.Add(new StatusEvent
        {
            FromStatus = null,
            ToStatus = app.Status,
            Note = "Application created."
        });

        db.Applications.Add(app);
        await db.SaveChangesAsync(ct);

        await db.Entry(app).Reference(a => a.Company).LoadAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = app.Id }, ToDetail(app));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ApplicationRequest request, CancellationToken ct)
    {
        var app = await db.Applications.FindAsync([id], ct);
        if (app is null) return NotFound();

        if (!await db.Companies.AnyAsync(c => c.Id == request.CompanyId, ct))
        {
            ModelState.AddModelError(nameof(request.CompanyId), $"Company {request.CompanyId} does not exist.");
            return ValidationProblem(ModelState);
        }

        app.CompanyId = request.CompanyId;
        app.RoleTitle = request.RoleTitle;
        app.JobUrl = request.JobUrl;
        app.Source = request.Source;
        app.Location = request.Location;
        app.SalaryMin = request.SalaryMin;
        app.SalaryMax = request.SalaryMax;
        app.Notes = request.Notes;
        app.AppliedDate = request.AppliedDate;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var app = await db.Applications.FindAsync([id], ct);
        if (app is null) return NotFound();

        db.Applications.Remove(app);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Moves an application to a new status, rejecting transitions the workflow does not allow
    /// and recording an audit entry for those it does.
    /// </summary>
    [HttpPost("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationDetailResponse>> ChangeStatus(
        int id, StatusChangeRequest request, CancellationToken ct)
    {
        var app = await db.Applications
            .Include(a => a.Company)
            .Include(a => a.Interviews)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (app is null) return NotFound();

        if (!StatusTransitions.CanTransition(app.Status, request.ToStatus))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status transition",
                Detail = StatusTransitions.IsTerminal(app.Status)
                    ? $"'{app.Status}' is a terminal state and cannot be changed."
                    : $"Cannot move from '{app.Status}' to '{request.ToStatus}'. Allowed: {string.Join(", ", StatusTransitions.NextStates(app.Status))}.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var previous = app.Status;
        app.Status = request.ToStatus;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.ToStatus == ApplicationStatus.Applied && !app.AppliedDate.HasValue)
        {
            app.AppliedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        app.StatusHistory.Add(new StatusEvent
        {
            FromStatus = previous,
            ToStatus = request.ToStatus,
            Note = request.Note
        });

        await db.SaveChangesAsync(ct);
        return Ok(ToDetail(app));
    }

    [HttpGet("{id:int}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StatusEventResponse>>> GetHistory(int id, CancellationToken ct)
    {
        if (!await db.Applications.AnyAsync(a => a.Id == id, ct)) return NotFound();

        var history = await db.StatusEvents
            .AsNoTracking()
            .Where(s => s.ApplicationId == id)
            .OrderByDescending(s => s.ChangedAt)
            .Select(s => new StatusEventResponse(s.Id, s.FromStatus, s.ToStatus, s.ChangedAt, s.Note))
            .ToListAsync(ct);

        return Ok(history);
    }

    [HttpPost("{id:int}/interviews")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InterviewResponse>> AddInterview(
        int id, InterviewRequest request, CancellationToken ct)
    {
        if (!await db.Applications.AnyAsync(a => a.Id == id, ct)) return NotFound();

        var interview = new Interview
        {
            ApplicationId = id,
            Stage = request.Stage,
            ScheduledAt = request.ScheduledAt,
            InterviewerName = request.InterviewerName,
            Outcome = request.Outcome,
            Notes = request.Notes
        };

        db.Interviews.Add(interview);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id }, ToInterview(interview));
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineStatsResponse>> GetStats(CancellationToken ct)
    {
        var counts = await db.Applications
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var byStatus = counts.ToDictionary(c => c.Status.ToString(), c => c.Count);
        var total = counts.Sum(c => c.Count);

        var active = counts
            .Where(c => !StatusTransitions.IsTerminal(c.Status))
            .Sum(c => c.Count);

        var responded = counts
            .Where(c => c.Status is not (ApplicationStatus.Saved or ApplicationStatus.Applied))
            .Sum(c => c.Count);

        var now = DateTimeOffset.UtcNow;
        var upcoming = await db.Interviews
            .AsNoTracking()
            .CountAsync(i => i.ScheduledAt > now && i.Outcome == InterviewOutcome.Pending, ct);

        var responseRate = total == 0 ? 0 : Math.Round(responded / (double)total * 100, 1);

        return Ok(new PipelineStatsResponse(total, active, upcoming, responseRate, byStatus));
    }

    private static ApplicationDetailResponse ToDetail(JobApplication a) => new(
        a.Id,
        a.CompanyId,
        a.Company?.Name ?? string.Empty,
        a.RoleTitle,
        a.JobUrl,
        a.Source,
        a.Location,
        a.SalaryMin,
        a.SalaryMax,
        a.Notes,
        a.Status,
        StatusTransitions.NextStates(a.Status),
        a.AppliedDate,
        a.CreatedAt,
        a.UpdatedAt,
        [.. a.Interviews.OrderBy(i => i.ScheduledAt).Select(ToInterview)],
        [.. a.StatusHistory.OrderByDescending(s => s.ChangedAt)
            .Select(s => new StatusEventResponse(s.Id, s.FromStatus, s.ToStatus, s.ChangedAt, s.Note))]);

    private static InterviewResponse ToInterview(Interview i) => new(
        i.Id, i.ApplicationId, i.Stage, i.ScheduledAt, i.InterviewerName, i.Outcome, i.Notes);
}
