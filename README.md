# Scribe.Notifications

[![NuGet](https://img.shields.io/nuget/v/Scribe.Notifications.svg)](https://www.nuget.org/packages/Scribe.Notifications)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Scribe.Notifications.svg)](https://www.nuget.org/packages/Scribe.Notifications)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-purple.svg)](https://dotnet.microsoft.com)

A high-performance **Notification Pattern** library for .NET. Simple, focused, and built for demanding workloads.

---

## What is Scribe.Notifications?

Scribe.Notifications is a library that implements the **Notification Pattern**. Instead of throwing exceptions for expected validation failures, you collect notifications and inspect them at the end of an operation. This results in cleaner code, better performance, and richer feedback to the caller.

The library is built around three core principles:

- **Performance first**: lazy evaluation, Flyweight caching, string interning, and `Span<T>` support minimize allocations and maximize throughput
- **Focused**: does one thing well, with no external dependencies
- **Thread-safe**: safe for concurrent workloads out of the box

---
## Architecture
```
┌─────────────────────────────────────────────┐
│              INotificationStore              │  ← contract
└─────────────────────┬───────────────────────┘
                      │ implements
                      ▼
┌─────────────────────────────────────────────┐
│           NotificationCollection            │  ← thread-safe, lazy
│                                             │
│  Add / AddRange / AddAsync                  │
│  Remove / RemoveByType / Clear              │
│  GetErrors()      → IEnumerable  (lazy)     │
│  GetErrorsAsList() → IReadOnlyList          │
│  TryGetAsSpan()   → ReadOnlySpan (zero-copy)│
└─────────────────────┬───────────────────────┘
                      │ contains
             ┌────────┴────────┐
             ▼                 ▼
┌────────────────────┐ ┌──────────────────────┐
│ NotificationMessage│ │   NotificationType   │
│  (readonly struct) │ │     (singleton)      │
│                    │ │                      │
│  Id (optional)     │ │  Error   (100, fail) │
│  Type              │ │  Warning  (60)       │
│  Message           │ │  Info     (20)       │
│  CreatedAt         │ │  Success   (0)       │
│  Metadata          │ │  + custom types      │
└────────────────────┘ └──────────────────────┘
```
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

// Quick add, ID is generated automatically
notifications.Add(NotificationType.Error, "Email is invalid.");
notifications.Add(NotificationType.Warning, "Name is too short.");

// With explicit ID, recommended when observability matters
notifications.Add(new NotificationMessage("USER_EMAIL_INVALID", NotificationType.Error, "Email is invalid."));

// Check and react
if (notifications.HasErrors())
{
    foreach (var error in notifications.GetErrors())
        Console.WriteLine(error); // [USER_EMAIL_INVALID] Error: Email is invalid.
}
```

---

## Core Concepts

### NotificationMessage

An immutable `readonly struct` that represents a single notification. Because it is a struct, instances are stack-allocated with no heap pressure for the common case.

```csharp
// Without explicit ID: a unique identifier is generated automatically
var notification = new NotificationMessage(NotificationType.Error, "Order amount must be greater than zero.");

// With explicit ID: recommended for tracing and observability
var notification = new NotificationMessage(
    id: "ORDER_AMOUNT_INVALID",
    type: NotificationType.Error,
    message: "Order amount must be greater than zero."
);

// Attach contextual metadata without breaking immutability
var withContext = notification.WithMetadata(
    new Dictionary<string, object?> { { "field", "amount" }, { "value", -5 } }
);
```

**Key characteristics:**
- Immutable: all properties are set at construction
- Optional ID: omit it for quick usage; a unique identifier is generated automatically
- `With*` methods return new instances, never mutate
- IDs are automatically interned: 10,000 notifications with the same ID use one string in memory
- Implements `IEquatable<T>` with value semantics

---

### NotificationType

A singleton type that represents a category of notification. Predefined types are stored in a `FrozenDictionary` for allocation-free, thread-safe lookups. Custom types are created on demand and cached. The same configuration always returns the same instance.

**Predefined types:**

| Type    | Severity | IsFailure |
|---------|----------|-----------|
| Error   | 100      | true      |
| Warning | 60       | false     |
| Info    | 20       | false     |
| Success | 0        | false     |

```csharp
// Predefined singletons
var error = NotificationType.Error;
var warning = NotificationType.Warning;

// Custom types, cached by composite key (name + displayName + severity + isFailure)
var critical = NotificationType.GetOrCreate(
    name: "critical",
    displayName: "Critical Error",
    severityLevel: 95,
    isFailure: true
);

// Same parameters always return the same instance
var same = NotificationType.GetOrCreate("critical", "Critical Error", 95, true);
Assert.Same(critical, same); // true

// Look up a predefined type by name
if (NotificationType.TryGetPredefinedType("error", out var type))
    Console.WriteLine(type.SeverityLevel); // 100
```

---

### NotificationCollection

A thread-safe collection that implements `INotificationStore`. Designed for both high-concurrency and high-throughput scenarios.

**Adding notifications:**

```csharp
var notifications = new NotificationCollection();

// Quick add. ID generated automatically
notifications.Add(NotificationType.Error, "Something went wrong.");

// Explicit ID. recommended for tracing and observability
notifications.Add(new NotificationMessage("ID_001", NotificationType.Error, "Something went wrong."));

// Batch add
ReadOnlySpan<NotificationMessage> batch = stackalloc NotificationMessage[]
{
    new("ID_002", NotificationType.Warning, "Low disk space."),
    new("ID_003", NotificationType.Info, "Cache refreshed.")
};
notifications.AddRange(batch);

// Async add
await notifications.AddAsync(NotificationType.Warning, "Low disk space.");
await notifications.AddAsync(new NotificationMessage("ID_004", NotificationType.Error, "Timeout."));
```

**Querying with lazy evaluation:**

Lazy methods use `yield return` and do not allocate a new list. Prefer them when iterating once.

```csharp
// Check presence
bool hasErrors = notifications.HasErrors();
bool hasWarnings = notifications.HasWarnings();
int total = notifications.Count();

// Lazy enumeration (no intermediate list allocation)
foreach (var error in notifications.GetErrors())
    Console.WriteLine(error.Message);

foreach (var item in notifications.GetByType(NotificationType.Warning))
    Process(item);
```

**Querying materialized lists:**

Use these when you need to pass the result to another method, serialize it, or access it multiple times.

```csharp
IReadOnlyList<NotificationMessage> errors = notifications.GetErrorsAsList();
IReadOnlyList<NotificationMessage> all = notifications.GetAllAsList();
```

**High-performance zero-copy access:**

For performance-critical paths where you need direct access to the underlying data without any allocation:

```csharp
if (notifications.TryGetAsSpan(out ReadOnlySpan<NotificationMessage> span))
{
    // Direct access to the backing array, zero allocation.
    for (int i = 0; i < span.Length; i++)
        Process(span[i]);
}
```

> ⚠️ The span is valid only as long as the collection is not modified. In concurrent scenarios, use `GetAllAsList()` instead.

**Removing notifications:**

```csharp
// Remove by ID
bool removed = notifications.Remove("ID_001");

// Remove all of a type
int count = notifications.RemoveByType(NotificationType.Warning);

// Clear everything
notifications.Clear();
```

---

## Async Support

All mutating and querying operations have async counterparts returning `ValueTask`, zero allocation in the synchronous fast path.

```csharp
// Quick async add
await notifications.AddAsync(NotificationType.Error, "Something went wrong.");

// Explicit ID async add
await notifications.AddAsync(new NotificationMessage("ID_001", NotificationType.Error, "Timeout."));
await notifications.AddRangeAsync(batch);

bool hasErrors = await notifications.HasErrorsAsync();
int count = await notifications.CountAsync();

bool removed = await notifications.RemoveAsync("ID_001");
await notifications.ClearAsync();
```

---

## Custom Notification Types

```csharp
// Register a domain-specific type
var blocked = NotificationType.GetOrCreate(
    name: "blocked",
    displayName: "Blocked",
    severityLevel: 80,
    isFailure: true
);

notifications.Add(new NotificationMessage("ACC_BLOCKED", blocked, "Account is blocked."));

// Query by custom type
foreach (var n in notifications.GetByType(blocked))
    Console.WriteLine(n);
```

---

## Implementing INotificationStore

You can build your own store by implementing `INotificationStore`, useful for decorating with persistence, telemetry, or custom routing.

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

    // ... delegate remaining members to _inner
}
```

---

## Roadmap

- **v1.1** `NotificationRules`: reusable, registrable validation rules with a fluent builder
- **v1.2** Grouping & Aggregation: `GroupBy`, `OrderBySeverity`, and filtering extensions
- **v1.3** Localization: multi-language message support with `.resx` integration
- **v2.0** Observability: built-in OpenTelemetry integration with automatic metrics per operation

---

## Contributing

Contributions are welcome. Please open an issue first to discuss what you'd like to change.

---

## License

MIT. See [LICENSE](LICENSE) for details.
