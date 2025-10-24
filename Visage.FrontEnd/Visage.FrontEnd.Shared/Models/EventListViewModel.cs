namespace Visage.FrontEnd.Shared.Models;

public record EventListViewModel
{
    public required string Title { get; init; }
    public required List<EventViewModel> Events { get; init; }
    public int TotalCount => Events.Count;
    public bool HasEvents => Events.Any();
    public string EmptyMessage { get; init; } = "No events available.";
    
    // Pagination (for past events)
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}
