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

    public string? Type { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public string? Location { get; set; }


    // Deprecated: Use CoverPicture for the Cloudinary image URL
    [Obsolete("Use CoverPicture property instead.")]
    public string? CoverPictureLocation { get; set; }

    [Obsolete("Use CoverPicture property instead.")]
    public string? CoverPictureFileName { get; set; }

    public int? AttendeesPercentage { get; set; }

    // New properties
    public string? Hashtag { get; set; }

    public string? Theme { get; set; }

    // New property for cover picture URL
    public string? CoverPicture { get; set; }
}
