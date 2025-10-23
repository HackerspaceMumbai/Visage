# API Contracts: Event Service Integration

**Feature**: 002-blazor-frontend-redesign  
**Service**: Visage.Services.Eventing  
**Base URL**: \https://eventing\ (Aspire service discovery)

---

## Endpoint Specifications

### 1. Get Upcoming Events

**GET** \/events/upcoming\

**Description**: Retrieves all events with status 'Upcoming' and future dates, ordered by date ascending.

**Request**:
- **Method**: GET
- **Headers**:
  - \Accept: application/json\
- **Query Parameters**: None

**Response** (200 OK):
\\\json
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "name": "Introduction to Open Source",
    "date": "2025-10-27T18:30:00Z",
    "time": "18:30:00",
    "location": "Hackerspace Mumbai",
    "coverImageUrl": "https://res.cloudinary.com/hackmum/image/upload/v1/events/oss-intro.jpg",
    "description": "Learn the basics of contributing to open source projects.",
    "rsvpLink": "https://meetup.com/hackmum/events/12345",
    "attendeeCount": 45,
    "status": "Upcoming",
    "createdAt": "2025-10-01T10:00:00Z",
    "updatedAt": "2025-10-15T14:30:00Z"
  }
]
\\\

**Response** (204 No Content):
- Returned when no upcoming events exist

**Error Responses**:
- **500 Internal Server Error**: Database connection failure or unhandled exception

**Performance**: < 50ms (in-memory query, indexed by date and status)

**Cache Strategy**: Frontend caches response for 5 minutes

---

### 2. Get Past Events (Paginated)

**GET** \/events/past?page={page}&pageSize={pageSize}\

**Description**: Retrieves completed events ordered by date descending (most recent first), with pagination support.

**Request**:
- **Method**: GET
- **Headers**:
  - \Accept: application/json\
- **Query Parameters**:
  - \page\ (integer, required): Page number starting from 1
  - \pageSize\ (integer, required): Number of items per page (default: 20, max: 100)

**Response** (200 OK):
\\\json
{
  "items": [
    {
      "id": "22222222-2222-2222-2222-222222222222",
      "name": "Docker Workshop",
      "date": "2025-09-20T10:00:00Z",
      "time": "10:00:00",
      "location": "Online",
      "coverImageUrl": null,
      "description": "Hands-on workshop on containerization with Docker.",
      "rsvpLink": null,
      "attendeeCount": 120,
      "status": "Completed",
      "createdAt": "2025-08-15T09:00:00Z",
      "updatedAt": "2025-09-21T11:00:00Z"
    }
  ],
  "totalCount": 150,
  "currentPage": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
\\\

**Error Responses**:
- **400 Bad Request**: Invalid page or pageSize parameters
  \\\json
  {
    "error": "InvalidPagination",
    "message": "Page must be >= 1 and pageSize must be between 1 and 100"
  }
  \\\
- **500 Internal Server Error**: Database failure

**Performance**: < 100ms (paginated query with indexes)

**Cache Strategy**: Frontend caches each page for 1 hour (past events rarely change)

---

### 3. Get Event by ID

**GET** \/events/{id}\

**Description**: Retrieves detailed information for a specific event by its unique identifier.

**Request**:
- **Method**: GET
- **Headers**:
  - \Accept: application/json\
- **Path Parameters**:
  - \id\ (GUID, required): Unique event identifier

**Response** (200 OK):
\\\json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "Introduction to Open Source",
  "date": "2025-10-27T18:30:00Z",
  "time": "18:30:00",
  "location": "Hackerspace Mumbai",
  "coverImageUrl": "https://res.cloudinary.com/hackmum/image/upload/v1/events/oss-intro.jpg",
  "description": "Learn the basics of contributing to open source projects. This beginner-friendly session covers Git basics, finding projects to contribute to, and making your first pull request.",
  "rsvpLink": "https://meetup.com/hackmum/events/12345",
  "attendeeCount": 45,
  "status": "Upcoming",
  "createdAt": "2025-10-01T10:00:00Z",
  "updatedAt": "2025-10-15T14:30:00Z"
}
\\\

**Error Responses**:
- **404 Not Found**: Event with specified ID does not exist
  \\\json
  {
    "error": "EventNotFound",
    "message": "Event with ID 11111111-1111-1111-1111-111111111111 was not found"
  }
  \\\
- **400 Bad Request**: Invalid GUID format
  \\\json
  {
    "error": "InvalidEventId",
    "message": "Event ID must be a valid GUID"
  }
  \\\
- **500 Internal Server Error**: Database failure

**Performance**: < 30ms (primary key lookup)

**Cache Strategy**: Frontend caches response for 10 minutes

---

## Data Types

### Event DTO

\\\	ypescript
interface Event {
  id: string;                  // GUID format
  name: string;                // Max 200 chars
  date: string;                // ISO 8601 datetime
  time: string;                // HH:mm:ss format
  location: string;            // Max 500 chars
  coverImageUrl: string | null; // Valid URL or null
  description: string;         // Max 2000 chars
  rsvpLink: string | null;     // Valid URL or null
  attendeeCount: number;       // >= 0
  status: 'Upcoming' | 'InProgress' | 'Completed' | 'Cancelled';
  createdAt: string;           // ISO 8601 datetime
  updatedAt: string;           // ISO 8601 datetime
}
\\\

### PaginatedResult<T> DTO

\\\	ypescript
interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
\\\

---

## Service Discovery Configuration

**Aspire Service Name**: \venting\

**Frontend HttpClient Configuration** (Program.cs):
\\\csharp
builder.Services.AddHttpClient<EventService>(client =>
{
    client.BaseAddress = new Uri("https://eventing");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
\\\

**Service Discovery Resolution**: Aspire resolves \https://eventing\ to the actual Visage.Services.Eventing endpoint at runtime.

---

## Error Handling Strategy

### Frontend Error Handling

\\\csharp
// EventService.cs
public async Task<List<EventViewModel>> GetUpcomingEventsAsync()
{
    try
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>("https://eventing/events/upcoming");
        return events?.Select(EventViewModel.FromEvent).ToList() ?? new List<EventViewModel>();
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
    {
        // No upcoming events - return empty list
        return new List<EventViewModel>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to fetch upcoming events from API");
        
        // Check cache for fallback data
        if (_cache.TryGetValue("upcoming-events-fallback", out List<EventViewModel>? cached))
        {
            return cached ?? new List<EventViewModel>();
        }
        
        throw; // Rethrow to trigger UI error state
    }
}
\\\

### Backend Error Responses

All error responses follow this format:
\\\json
{
  "error": "ErrorCode",
  "message": "Human-readable error description"
}
\\\

---

## Testing Contracts

### Integration Test Examples

\\\csharp
// Visage.Test.Aspire/EventApiIntegrationTests.cs
[Test]
public async Task GetUpcomingEvents_ReturnsOrderedEvents()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/events/upcoming");
    var events = await response.Content.ReadFromJsonAsync<List<Event>>();
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    events.Should().NotBeNull();
    events.Should().BeInAscendingOrder(e => e.Date);
    events.Should().OnlyContain(e => e.Status == EventStatus.Upcoming);
}

[Test]
public async Task GetPastEvents_WithPagination_ReturnsCorrectPage()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/events/past?page=2&pageSize=10");
    var result = await response.Content.ReadFromJsonAsync<PaginatedResult<Event>>();
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    result.Should().NotBeNull();
    result!.CurrentPage.Should().Be(2);
    result.PageSize.Should().Be(10);
    result.Items.Should().HaveCountLessOrEqualTo(10);
}

[Test]
public async Task GetEventById_WithInvalidId_Returns400()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/events/invalid-guid");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
\\\

---

## Summary

**Contract Status**: ✅ **Complete**

- **3 Endpoints**: GET /events/upcoming, GET /events/past, GET /events/{id}
- **DTOs**: Event, PaginatedResult<T>
- **Error Handling**: Standardized error responses with fallback strategies
- **Service Discovery**: Aspire-based resolution via \https://eventing\ service name
- **Testing**: Integration test examples provided
- **Performance**: All endpoints meet < 100ms target
- **Caching**: Frontend implements TTL-based caching (5min/1hr/10min)

**Next**: Create quickstart.md developer guide.
