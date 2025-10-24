using System;
using System.ComponentModel.DataAnnotations;
using StrictId;

namespace Visage.Shared.Models;

public class Event
{
    [Required]
    //[StringLength(50, ErrorMessage = "Event ID must be less than 50 characters.")]
    public Id<Event> Id { get; init; }

    [Required]
    [StringLength(100, ErrorMessage = "Event Name must be less than 100 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Type must be less than 50 characters")]
    public string? Type { get; set; }

    [StringLength(2000, ErrorMessage = "Description must be less than 2000 characters")]
    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [StringLength(500, ErrorMessage = "Location must be less than 500 characters")]
    public string? Location { get; set; }

    // Cover picture as Cloudinary URL
    [StringLength(500, ErrorMessage = "Cover picture URL must be less than 500 characters")]
    public string? CoverPicture { get; set; }

    public decimal? AttendeesPercentage { get; set; }

    [StringLength(100, ErrorMessage = "Hashtag must be less than 100 characters")]
    public string? Hashtag { get; set; }
    
    [StringLength(200, ErrorMessage = "Theme must be less than 200 characters")]
    public string? Theme { get; set; }
}
