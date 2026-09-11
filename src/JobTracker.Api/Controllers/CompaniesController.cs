using JobTracker.Api.Data;
using JobTracker.Api.Domain;
using JobTracker.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Produces("application/json")]
public class CompaniesController(JobTrackerDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CompanyResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Companies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term) || c.Industry!.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyResponse(
                c.Id, c.Name, c.Industry, c.Website, c.Location, c.Notes,
                c.Applications.Count, c.CreatedAt))
            .ToListAsync(ct);

        return Ok(new PagedResult<CompanyResponse>(items, page, pageSize, total));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(int id, CancellationToken ct)
    {
        var company = await db.Companies
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CompanyResponse(
                c.Id, c.Name, c.Industry, c.Website, c.Location, c.Notes,
                c.Applications.Count, c.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyResponse>> Create(CompanyRequest request, CancellationToken ct)
    {
        if (await db.Companies.AnyAsync(c => c.Name == request.Name, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Duplicate company",
                Detail = $"A company named '{request.Name}' already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var company = new Company
        {
            Name = request.Name,
            Industry = request.Industry,
            Website = request.Website,
            Location = request.Location,
            Notes = request.Notes
        };

        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        var response = new CompanyResponse(
            company.Id, company.Name, company.Industry, company.Website,
            company.Location, company.Notes, 0, company.CreatedAt);

        return CreatedAtAction(nameof(GetById), new { id = company.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, CompanyRequest request, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([id], ct);
        if (company is null) return NotFound();

        company.Name = request.Name;
        company.Industry = request.Industry;
        company.Website = request.Website;
        company.Location = request.Location;
        company.Notes = request.Notes;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([id], ct);
        if (company is null) return NotFound();

        db.Companies.Remove(company);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
