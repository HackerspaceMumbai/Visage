namespace Visage.FrontEnd.Shared.Models;

public record EventViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateTime Date { get; init; }
    public required string Location { get; init; }
    public string? CoverImageUrl { get; init; }
    public required string Description { get; init; }
    public string? RsvpLink { get; init; }
    public int AttendeeCount { get; init; }
    public EventStatus Status { get; init; }

    // Computed Properties
    public string FormattedDate => Date.ToString("MMM dd, yyyy");
    public string FormattedTime => Date.ToString("h:mm tt");
    public string FormattedDateTime => $"{FormattedDate} at {FormattedTime}";
    public bool IsUpcoming => Status == EventStatus.Upcoming && Date > DateTime.Now;
    public bool IsPast => Status == EventStatus.Completed || Date < DateTime.Now;
    public bool HasImage => !string.IsNullOrEmpty(CoverImageUrl);
    public string OptimizedImageUrl => HasImage
        ? CoverImageUrl!.Replace("/upload/", "/upload/f_auto,q_auto,w_800/")
        : string.Empty;
}
