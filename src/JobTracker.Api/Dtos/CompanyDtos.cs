using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos;

public record CompanyResponse(
    int Id,
    string Name,
    string? Industry,
    string? Website,
    string? Location,
    string? Notes,
    int ApplicationCount,
    DateTimeOffset CreatedAt);

public record CompanyRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: MaxLength(100)] string? Industry,
    [property: MaxLength(500), Url] string? Website,
    [property: MaxLength(200)] string? Location,
    [property: MaxLength(2000)] string? Notes);
