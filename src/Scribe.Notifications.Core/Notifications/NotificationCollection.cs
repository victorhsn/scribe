using System.Runtime.InteropServices;
using Scribe.Notifications.Core.Contracts;

namespace Scribe.Notifications.Core.Notifications;

/// <summary>
/// Represents a thread-safe collection of notification messages.
/// Implements lazy evaluation for memory efficiency and performance optimization.
/// Supports multiple query patterns: enumeration, filtering, and direct access.
/// </summary>
public sealed class NotificationCollection : INotificationStore
{
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif
    private readonly List<NotificationMessage> _notifications = [];

    /// <summary>
    /// Gets the number of notifications currently stored.
    /// </summary>
    /// <inheritdoc/>
    public void Add(in NotificationMessage notification)
    {
        if (string.IsNullOrWhiteSpace(notification.Id))
            throw new ArgumentException("Notification ID cannot be null, empty or whitespace", nameof(notification.Id));

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            _notifications.Add(notification);
        }
    }

    /// <inheritdoc/>
    public void AddRange(ReadOnlySpan<NotificationMessage> notifications)
    {
        if (notifications.IsEmpty)
            return;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            _notifications.AddRange(notifications.ToArray());
        }
    }

    /// <inheritdoc/>
    public ValueTask AddAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Add(notification);
        return default;
    }

    /// <inheritdoc/>
    public ValueTask AddRangeAsync(ReadOnlySpan<NotificationMessage> notifications, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AddRange(notifications);
        return default;
    }

    /// <inheritdoc/>
    public bool Remove(string notificationId)
    {
        if (string.IsNullOrWhiteSpace(notificationId))
            throw new ArgumentException("Notification ID cannot be null, empty or whitespace", nameof(notificationId));

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return false;

            var index = _notifications.FindIndex(x => x.Id == notificationId);

            if (index < 0) return false;

            _notifications.RemoveAt(index);
            return true;
        }
    }

    /// <inheritdoc/>
    public int RemoveByType(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0) return 0;

            var removedCount = _notifications.RemoveAll(x => x.Type == notificationType);
            return removedCount;
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            _notifications.Clear();
        }
    }

    /// <inheritdoc/>
    public ValueTask<bool> RemoveAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<bool>(Remove(notificationId));
    }

    /// <inheritdoc/>
    public ValueTask<int> RemoveByTypeAsync(NotificationType notificationType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<int>(RemoveByType(notificationType));
    }

    /// <inheritdoc/>
    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Clear();
        return default;
    }

    /// <inheritdoc/>
    public bool HasNotifications()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications is { Count: > 0 };
        }
    }

    /// <inheritdoc/>
    public bool HasErrors()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications.Any(n => n.Type == NotificationType.Error);
        }
    }

    /// <inheritdoc/>
    public bool HasWarnings()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications.Any(n => n.Type == NotificationType.Warning);
        }
    }

    /// <inheritdoc/>
    public bool HasNotificationType(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications.Any(n => n.Type == notificationType);
        }
    }

    /// <inheritdoc/>
    public bool Contains(string notificationId)
    {
        if (string.IsNullOrWhiteSpace(notificationId))
            throw new ArgumentException("Notification ID cannot be null, empty or whitespace", nameof(notificationId));

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications.Any(n => n.Id == notificationId);
        }
    }

    /// <inheritdoc/>
    public int Count()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications?.Count ?? 0;
        }
    }

    /// <inheritdoc/>
    public int Count(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications?.Count(n => n.Type == notificationType) ?? 0;
        }
    }

    /// <inheritdoc/>
    public int ErrorCount()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications?.Count(n => n.Type == NotificationType.Error) ?? 0;
        }
    }

    /// <inheritdoc/>
    public int WarningCount()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            return _notifications?.Count(n => n.Type == NotificationType.Warning) ?? 0;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<NotificationMessage> GetAll()
    {
        List<NotificationMessage> copy;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            copy = _notifications.Count == 0 ? [] : [.. _notifications];
        }

        foreach (var notification in copy)
            yield return notification;
    }

    /// <inheritdoc/>
    public IEnumerable<NotificationMessage> GetByType(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);
        List<NotificationMessage> copy;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                copy = [];
            else
                copy =
                [
                    .._notifications
                        .Where(n => n.Type == notificationType)
                ];

        }

        foreach (var notification in copy)
            yield return notification;
    }

    /// <inheritdoc />
    public IEnumerable<NotificationMessage> GetErrors()
    {
        List<NotificationMessage> copy;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                copy = [];
            else
                copy = [.. _notifications.Where(n => n.Type == NotificationType.Error)];
        }

        foreach (var notification in copy)
            yield return notification;
    }

    /// <inheritdoc />
    public IEnumerable<NotificationMessage> GetWarnings()
    {
        List<NotificationMessage> copy;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                copy = [];
            else
                copy = [.. _notifications.Where(n => n.Type == NotificationType.Warning)];
        }

        foreach (var notification in copy)
            yield return notification;
    }

    /// <inheritdoc />
    public IEnumerable<NotificationMessage> GetInfos()
    {
        List<NotificationMessage> copy;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                copy = [];
            else
                copy = [.. _notifications.Where(n => n.Type == NotificationType.Info)];
        }

        foreach (var notification in copy)
            yield return notification;
    }

    /// <inheritdoc />
    public IEnumerable<NotificationMessage> GetSuccesses()
    {
        List<NotificationMessage> copy;

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                copy = [];
            else
                copy = [.. _notifications.Where(n => n.Type == NotificationType.Success)];
        }

        foreach (var notification in copy)
            yield return notification;
    }

    /// <inheritdoc />
    public NotificationMessage? GetById(string notificationId)
    {
        if (string.IsNullOrWhiteSpace(notificationId))
            throw new ArgumentException("Notification ID cannot be null, empty or whitespace", nameof(notificationId));

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return null;

            return _notifications.FirstOrDefault(n => n.Id == notificationId);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<NotificationMessage> GetAllAsList()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return Array.Empty<NotificationMessage>();

            return _notifications.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<NotificationMessage> GetByTypeAsList(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return Array.Empty<NotificationMessage>();

            return _notifications.Where(n => n.Type == notificationType).ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<NotificationMessage> GetErrorsAsList()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return Array.Empty<NotificationMessage>();

            return _notifications.Where(n => n.Type == NotificationType.Error).ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<NotificationMessage> GetWarningsAsList()
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return Array.Empty<NotificationMessage>();

            return _notifications.Where(n => n.Type == NotificationType.Warning).ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Tries to get all notifications as a span for zero-copy access and high performance.
    /// </summary>
    /// <remarks>
    /// The returned span points directly to the internal backing array of the collection.
    /// It is valid only as long as the collection is not modified after this call.
    /// Any structural change (add, remove, clear) may cause the list to reallocate its
    /// backing array, leaving the span pointing to stale memory.
    /// In concurrent scenarios, prefer <see cref="GetAllAsList"/> for safe access.
    /// </remarks>
    /// <param name="notifications">When this method returns, contains a span of notifications if successful.</param>
    /// <returns>True if the span was retrieved; otherwise, false.</returns>
    public bool TryGetAsSpan(out ReadOnlySpan<NotificationMessage> notifications)
    {
#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
            {
                notifications = ReadOnlySpan<NotificationMessage>.Empty;
                return false;
            }

            notifications = CollectionsMarshal.AsSpan(_notifications);
            return true;
        }
    }

    /// <inheritdoc />
    public int CopyToSpan(Span<NotificationMessage> destination)
    {
        if (destination.IsEmpty)
            throw new ArgumentException("Destination span cannot be empty.", nameof(destination));

#if NET9_0_OR_GREATER
        using (_lock.EnterScope())
#else
        lock (_lock)
#endif
        {
            if (_notifications.Count == 0)
                return 0;

            if (destination.Length < _notifications.Count)
                throw new ArgumentException("Destination span is too small.", nameof(destination));

            _notifications.CopyTo(destination);
            return _notifications.Count;
        }
    }

    /// <inheritdoc />
    public ValueTask<bool> HasNotificationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<bool>(HasNotifications());
    }

    /// <inheritdoc />
    public ValueTask<bool> HasErrorsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<bool>(HasErrors());
    }

    /// <inheritdoc />
    public ValueTask<bool> HasWarningsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<bool>(HasWarnings());
    }

    /// <inheritdoc />
    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<int>(Count());
    }

    /// <inheritdoc />
    public ValueTask<int> CountAsync(NotificationType notificationType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<int>(Count(notificationType));
    }
}