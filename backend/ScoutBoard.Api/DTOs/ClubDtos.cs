using System.ComponentModel.DataAnnotations;
using ScoutBoard.Api.Models;

namespace ScoutBoard.Api.DTOs;

public record ClubListItemDto(
    int Id,
    string Name,
    City City,
    int? FoundedYear,
    string? LogoUrl,
    string? StadiumName,
    ApprovalStatus ApprovalStatus,
    int PlayerCount);

public record ClubDetailDto(
    int Id,
    string Name,
    City City,
    int? FoundedYear,
    string Description,
    string? StadiumName,
    string? Address,
    string? LogoUrl,
    ApprovalStatus ApprovalStatus,
    string? AdminNote,
    string OwnerName,
    int PlayerCount);

public class SaveClubRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public City City { get; set; }

    [Range(1900, 2100)]
    public int? FoundedYear { get; set; }

    [Required, MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? StadiumName { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(500), Url]
    public string? LogoUrl { get; set; }
}
