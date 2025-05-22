# Project Context

Visage is an app to manage events for OSS community events which are usually houseful 3 times the venue capacity, yet we are determined to promote inclusiveness and diversity.

## Technical Architecture

1. **Hybrid Blazor Web App Front End**: A single code base for both Web and Mobile applications.
2. **.NET Aspire Orchestration**: The solution is orchestrated through the .NET Aspire workload for local developer loop.
3. **Minimal APIs**: Each service is a Minimal API with its own .http file for ad-hoc testing.
4. **Project Resources**: All services are added as Project Resources in Aspire AppHost, with service dependencies properly configured.
5. **Data Access**: The Minimal APIs connect to the backend via EF Core using Repository and Unit of Work patterns.

## Application Purpose

Visage helps manage community event registrations, attendance, and engagement, with specific focus on:

- Registration management with diversity considerations
- Venue capacity optimization
- Waitlist management and automatic rollover
- Attendance tracking
- Feedback and analytics
- Community engagement tools