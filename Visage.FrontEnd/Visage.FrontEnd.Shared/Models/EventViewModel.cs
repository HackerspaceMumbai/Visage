using StrictId;
using SharedEvent = Visage.Shared.Models.Event;

namespace Visage.FrontEnd.Shared.Models;

public record EventViewModel
{
    // Strict typed Event id (used for calling EventService and checkin flows)
    public required Id<SharedEvent> EventId { get; init; }

    // Legacy Guid id kept for places that expect a Guid
    public required Guid Id { get; init; }

    public required string Name { get; init; }
    public required DateTime Date { get; init; }
    public required string Location { get; init; }
    public string? CoverImageUrl { get; init; }
    public required string Description { get; init; }
    public string? RsvpLink { get; init; }
    public int AttendeeCount { get; init; }
    public EventStatus Status { get; init; }

    /// <summary>
    /// Optimized image URL with CDN transformations applied during mapping.
    /// Assigned by the mapper using IImageUrlTransformer.
    /// </summary>
    public string? OptimizedImageUrl { get; init; }

    // Computed Properties
    public string FormattedDate => Date.ToString("MMM dd, yyyy");
    public string FormattedTime => Date.ToString("h:mm tt");
    public string FormattedDateTime => $"{FormattedDate} at {FormattedTime}";
    public bool HasImage => !string.IsNullOrEmpty(OptimizedImageUrl);

    // Time-dependent methods (accept currentTime for testability)
    public bool IsUpcoming(DateTime currentTime) => Status == EventStatus.Upcoming && Date > currentTime;
    public bool IsPast(DateTime currentTime) => Status == EventStatus.Completed || Date < currentTime;
}
