# Blazor Hybrid Guidance

## Shared Code Approach

Visage uses a Hybrid Blazor approach to maintain a single codebase for both web and mobile platforms:

- Put shared components, models, and services in the Shared project
- Use conditional compilation when platform-specific code is needed
- Leverage .NET MAUI for native mobile functionality
- Ensure components are responsive and adapt to different screen sizes

## Component Design

When designing and implementing Blazor components:

- Create small, focused components that can be composed
- Use CSS isolation for component-specific styles
- Implement proper state management
- Ensure components handle loading, error, and empty states gracefully
- Design for reusability when appropriate

## MAUI Integration

When working with MAUI-specific features in the hybrid app:

- Use conditional rendering for platform-specific UI elements
- Leverage MAUI Handlers for native controls when necessary
- Implement proper lifecycle management for mobile platforms
- Consider battery and resource usage for mobile implementations
- Test interactions on both web and mobile platforms

## Performance Considerations

- Use virtualization for large lists
- Implement proper lazy loading for components and resources
- Consider the impact of JavaScript interop on performance
- Minimize state changes that cause re-renders
- Use efficient rendering approaches (e.g., key-based rendering)