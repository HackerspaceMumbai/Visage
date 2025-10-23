# Data Model Design: Event Display Domain

**Feature**: 002-blazor-frontend-redesign  
**Purpose**: Define domain entities, view models, and validation rules for event display

---

## Domain Entities

### Event (Backend Entity)

**Location**: Visage.Shared/Models/Event.cs (existing)

\\\csharp
public class Event
{
    public EventId Id { get; set; } // StrictId<Guid>
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeOnly Time { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? RsvpLink { get; set; }
    public int AttendeeCount { get; set; }
    public EventStatus Status { get; set; } // Upcoming, InProgress, Completed, Cancelled
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum EventStatus
{
    Upcoming,
    InProgress,
    Completed,
    Cancelled
}
\\\

---

## View Models (Frontend)

### EventViewModel

**Location**: Visage.FrontEnd.Shared/Models/EventViewModel.cs (new)

**Purpose**: Presentation model for event cards and lists with computed properties for UI binding.

\\\csharp
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

    // Factory Method
    public static EventViewModel FromEvent(Event evt) => new()
    {
        Id = evt.Id.Value,
        Name = evt.Name,
        Date = evt.Date,
        Location = evt.Location,
        CoverImageUrl = evt.CoverImageUrl,
        Description = evt.Description,
        RsvpLink = evt.RsvpLink,
        AttendeeCount = evt.AttendeeCount,
        Status = evt.Status
    };
}
\\\

### EventListViewModel

**Location**: Visage.FrontEnd.Shared/Models/EventListViewModel.cs (new)

**Purpose**: Container for event collections with metadata for pagination/filtering.

\\\csharp
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
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}
\\\

---

## Validation Rules

### Event Data Validation

| Field | Rule | Error Message |
|-------|------|---------------|
| Name | Required, max 200 chars | "Event name is required and must be less than 200 characters" |
| Date | Required, valid DateTime | "Event date is required and must be a valid date" |
| Location | Required, max 500 chars | "Location is required and must be less than 500 characters" |
| Description | Required, max 2000 chars | "Description is required and must be less than 2000 characters" |
| CoverImageUrl | Optional, valid URL format | "Cover image must be a valid URL" |
| RsvpLink | Optional, valid URL format | "RSVP link must be a valid URL" |

**Note**: Validation is enforced at the backend (Visage.Services.Eventing). Frontend performs minimal client-side validation for UX (required field checks only).

---

## Data Flow Diagram

\\\
┌─────────────────────────────────────────────────────────────────┐
│ Frontend (Blazor)                                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Home.razor (InteractiveAuto)                                   │
│      │                                                           │
│      ├──> EventService.GetUpcomingEventsAsync()                 │
│      │         │                                                 │
│      │         ├──> MemoryCache (5min TTL)                      │
│      │         └──> HttpClient.GetFromJsonAsync<List<Event>>()  │
│      │                    │                                      │
│      ├──> EventViewModel.FromEvent() [mapping]                  │
│      │         │                                                 │
│      └──> EventList.razor                                       │
│                │                                                 │
│                ├──> EventGrid.razor (upcoming)                   │
│                │      └──> EventCard.razor (x10-20)             │
│                │                                                 │
│                └──> Virtualize<EventViewModel> (past)           │
│                       └──> EventCard.razor (virtualized)        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                             │
                             │ HTTPS + Service Discovery
                             ↓
┌─────────────────────────────────────────────────────────────────┐
│ Backend (Visage.Services.Eventing)                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  GET /events/upcoming                                            │
│      ↓                                                           │
│  EventRepository.GetUpcomingAsync()                             │
│      ↓                                                           │
│  EventDB (EF Core)                                              │
│      ↓                                                           │
│  SQL Server (indexed by Date, Status)                           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
\\\

---

## Performance Considerations

### Caching Strategy

| Data Type | Cache TTL | Cache Key | Invalidation Strategy |
|-----------|-----------|-----------|----------------------|
| Upcoming Events | 5 minutes | "upcoming-events" | Time-based (events rarely added/changed live) |
| Past Events (page) | 1 hour | "past-events-{page}-{pageSize}" | Time-based (historical data changes very rarely) |
| Event Details | 10 minutes | "event-{id}" | Time-based or manual on admin update |

### Database Indexes (Backend)

\\\sql
-- Visage.Services.Eventing/EventDB.cs - EF Core configuration
CREATE INDEX IX_Events_Date_Status ON Events(Date, Status);
CREATE INDEX IX_Events_Status ON Events(Status);
\\\

**Rationale**: Most queries filter by Status='Upcoming' and order by Date. Combined index optimizes this query pattern.

---

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Empty upcoming events list | Show EmptyState component with "No upcoming events scheduled" message |
| Event with missing image | Show placeholder icon (📅 emoji) with gradient background |
| Very long event names (>100 chars) | Truncate with \line-clamp-2\ utility (DaisyUI) + full name in title attribute |
| Events starting "today" | Highlight with "Today" badge on event card |
| Cancelled events | Show greyed-out card with "Cancelled" badge, no RSVP link |
| API request failure | Show error toast with retry button, fallback to cached data if available |
| Slow network (>3s) | Show skeleton loaders for event cards during fetch |

---

## Testing Data Requirements

### Seed Data for Integration Tests

\\\csharp
// Visage.Test.Aspire/Fixtures/EventTestData.cs
public static class EventTestData
{
    public static List<Event> GetSampleEvents() => new()
    {
        new Event
        {
            Id = EventId.From(Guid.Parse("11111111-1111-1111-1111-111111111111")),
            Name = "Introduction to Open Source",
            Date = DateTime.Now.AddDays(7),
            Time = new TimeOnly(18, 30),
            Location = "Hackerspace Mumbai",
            CoverImageUrl = "https://res.cloudinary.com/sample/image/upload/v1/events/oss-intro.jpg",
            Description = "Learn the basics of contributing to open source projects.",
            RsvpLink = "https://meetup.com/hackmum/events/12345",
            AttendeeCount = 45,
            Status = EventStatus.Upcoming
        },
        new Event
        {
            Id = EventId.From(Guid.Parse("22222222-2222-2222-2222-222222222222")),
            Name = "Docker Workshop",
            Date = DateTime.Now.AddDays(-30),
            Time = new TimeOnly(10, 0),
            Location = "Online",
            CoverImageUrl = null, // Test missing image scenario
            Description = "Hands-on workshop on containerization with Docker.",
            RsvpLink = null,
            AttendeeCount = 120,
            Status = EventStatus.Completed
        }
        // Add 20+ more events for pagination testing
    };
}
\\\

---

## Summary

**Data Model Status**: ✅ **Complete**

- **EventViewModel**: Record type with computed properties for UI binding
- **EventListViewModel**: Container with pagination metadata
- **Validation**: Enforced at backend; frontend performs minimal UX validation
- **Caching**: 5min for upcoming, 1hr for past events
- **Performance**: Indexed queries, virtualized rendering, lazy image loading
- **Testing**: Seed data fixtures for integration tests

**Next**: Define API contracts in contracts/ directory.
