<p align="center">
  <img src="docs/images/scribe-logo-v1.png" width="220" alt="Scribe.Notifications" />
</p>
<p align="center">
  High-performance Notification Pattern for .NET.
</p>
<p align="center">
  <strong>Record, don't throw.</strong>
</p>
<p align="center">
  Collect validation failures, warnings and domain notifications without relying on exceptions.
</p>
<p align="center">
  <a href="https://www.nuget.org/packages/Scribe.Notifications">
    <img src="https://img.shields.io/nuget/v/Scribe.Notifications.svg" />
  </a>
  <a href="https://www.nuget.org/packages/Scribe.Notifications">
    <img src="https://img.shields.io/nuget/dt/Scribe.Notifications.svg" />
  </a>
  <img src="https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-purple.svg" />
  <img src="https://img.shields.io/badge/License-MIT-yellow.svg" />
</p>

---

## What is Scribe?

Scribe.Notifications is a high-performance implementation of the Notification Pattern for .NET.

Instead of throwing exceptions for expected validation failures, Scribe collects notifications and allows callers to inspect them at the end of an operation.

This approach produces:

* Cleaner domain code
* Better performance
* Richer validation feedback
* Improved observability
* Better separation between validation and control flow

---

## Why Scribe?

Using exceptions for expected validation scenarios has drawbacks:

* Exceptions are expensive
* Only the first failure is returned
* Aggregating failures becomes difficult
* Validation logic leaks into control flow

Instead of this:

```csharp
throw new ValidationException("Email is invalid.");
```

Use this:

```csharp
notifications.Add(
    new NotificationMessage(
        "USER_EMAIL_INVALID",
        NotificationType.Error,
        "Email is invalid."
    )
);
```

Scribe enables you to:

* Collect all validation failures
* Return rich feedback
* Attach metadata
* Improve diagnostics
* Avoid exception-driven validation flows

---

## Features

* High-performance Notification Pattern implementation
* Thread-safe notification storage
* Immutable readonly struct notifications
* Lazy evaluation APIs
* Span\<T\> support
* Async APIs powered by ValueTask
* Custom notification types
* Rich metadata support
* String interning optimizations
* FrozenDictionary-backed type registry
* Zero external dependencies

---

## Installation

```bash
dotnet add package Scribe.Notifications
```

---

## Quick Start

```csharp
using Scribe.Notifications.Core.Notifications;

var notifications = new NotificationCollection();

notifications.Add(
    NotificationType.Error,
    "Email is invalid."
);

notifications.Add(
    NotificationType.Warning,
    "Name is too short."
);

if (notifications.HasErrors())
{
    foreach (var error in notifications.GetErrors())
    {
        Console.WriteLine(error);
    }
}
```

---

## Real World Example

```csharp
public NotificationCollection ValidateUser(
    string email,
    int age)
{
    var notifications = new NotificationCollection();

    if (string.IsNullOrWhiteSpace(email))
    {
        notifications.Add(
            new NotificationMessage(
                "USER_EMAIL_REQUIRED",
                NotificationType.Error,
                "Email is required."
            )
        );
    }

    if (age < 18)
    {
        notifications.Add(
            new NotificationMessage(
                "USER_AGE_INVALID",
                NotificationType.Error,
                "User must be at least 18 years old."
            )
        );
    }

    return notifications;
}
```

---

## Performance

Scribe was designed for high-throughput applications.

Performance optimizations include:

* readonly struct notifications
* String interning
* Flyweight caching
* FrozenDictionary lookups
* Lazy evaluation
* Span\<T\> support
* ValueTask APIs

### Benchmarks

| Scenario                        | Mean           | Allocated |
|---------------------------------| -------------- | --------- |
| Add with explicit ID            | 91.06 ns       | 264 B     |
| Add without ID (auto-generated) | 1,278.79 ns    | 368 B     |
| HasErrors 2,000 items           | 12.96 ns       | —         |
| HasErrors empty collection      | 12.88 ns       | —         |
| GetErrors lazy 1,000 errors     | 48,103.83 ns   | 78.4 KB   |
| GetErrorsAsList materialized    | 19,797.52 ns   | 78.3 KB   |
| TryGetAsSpan zero-copy          | 14.83 ns       | —         |
| AddRange 100 items              | 129,835.13 ns  | 25.9 KB   |

Environment:

* BenchmarkDotNet v0.14.0
* .NET 10.0.2 Arm64
* Apple M1 Pro, 8 cores
---

## Core Concepts

### NotificationMessage

Immutable readonly struct representing a single notification.

```csharp
var notification = new NotificationMessage(
    "ORDER_AMOUNT_INVALID",
    NotificationType.Error,
    "Order amount must be greater than zero."
);
```

### NotificationType

Represents notification categories.

Built-in types:

| Type    | Severity | IsFailure |
| ------- | -------- | --------- |
| Error   | 100      | true      |
| Warning | 60       | false     |
| Info    | 20       | false     |
| Success | 0        | false     |

Custom types:

```csharp
var critical = NotificationType.GetOrCreate(
    "critical",
    "Critical Error",
    95,
    true
);
```

### NotificationCollection

Thread-safe notification store.

```csharp
var notifications = new NotificationCollection();

notifications.Add(
    NotificationType.Error,
    "Something went wrong."
);

bool hasErrors = notifications.HasErrors();
```

---

## ASP.NET Minimal API Example

```csharp
app.MapPost("/users", (CreateUserRequest request) =>
{
    var notifications = Validate(request);

    if (notifications.HasErrors())
    {
        return Results.BadRequest(
            notifications.GetErrorsAsList()
        );
    }

    return Results.Ok();
});
```

---

## Custom Notification Types

```csharp
var blocked = NotificationType.GetOrCreate(
    name: "blocked",
    displayName: "Blocked",
    severityLevel: 80,
    isFailure: true
);

notifications.Add(
    new NotificationMessage(
        "ACCOUNT_BLOCKED",
        blocked,
        "Account is blocked."
    )
);
```

---

## Implementing INotificationStore

You can build your own store by implementing `INotificationStore`, useful for decorating with logging, telemetry, or custom routing.

```csharp
public class LoggingNotificationStore : INotificationStore
{
    private readonly INotificationStore _inner;
    private readonly ILogger _logger;

    public LoggingNotificationStore(INotificationStore inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void Add(in NotificationMessage notification)
    {
        _logger.LogInformation("Notification added: {Id}", notification.Id);
        _inner.Add(notification);
    }

    // Implement remaining INotificationStore members by delegating to _inner
}
```

---

## Roadmap

### v1.1
* Checks

### v1.2
* Grouping and aggregation APIs
* Severity ordering
* Filtering extensions

### v1.3
* Localization
* .resx integration

### v2.0
* OpenTelemetry integration
* Metrics and diagnostics

---

## Contributing

Contributions are welcome.

If you have suggestions, bug reports, or feature requests, please open an issue first so we can discuss the proposal.

---

## License

MIT License.

See [LICENSE](LICENSE) for details.
