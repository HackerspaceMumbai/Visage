using System;
using System.ComponentModel.DataAnnotations;
using StrictId;

namespace Visage.FrontEnd.Shared.Models;

public class Event
{
    [Required]
    //[StringLength(50, ErrorMessage = "Event ID must be less than 50 characters.")]
    public Id<Event> Id { get; init; }

    [Required]
    [StringLength(100, ErrorMessage = "Event Name must be less than 100 characters")]
    public string Title { get; set; } = string.Empty;

    public string? Type { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Location { get; set; }

    public string? CoverPictureLocation { get; set; }

    public string? CoverPictureFileName { get; set; }

    public int? AttendeesPercentage { get; set; }

    // New properties
    public string? Hashtag { get; set; }

    public string? Theme { get; set; }
}
