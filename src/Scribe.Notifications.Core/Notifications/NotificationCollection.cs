using Scribe.Notifications.Core.Contracts;

namespace Scribe.Notifications.Core.Notifications;

/// <summary>
/// Represents a thread-safe collection of notification messages.
/// Implements lazy evaluation for memory efficiency and performance optimization.
/// Supports multiple query patterns: enumeration, filtering, and direct access.
/// </summary>
public sealed class NotificationCollection : INotificationStore
{
    private readonly Lock _lock = new();
    private List<NotificationMessage> _notifications = [];
    
    /// <summary>
    /// Gets the number of notifications currently stored.
    /// </summary>
    
    //public int Count => _notifications?.Count ?? 0;
    
    /// <inheritdoc/>
    public void Add(in NotificationMessage notification)
    {
       if (string.IsNullOrWhiteSpace(notification.Id))
           throw new ArgumentException("Notification ID cannot be null, empty or whitespace", nameof(notification.Id));

       using (_lock.EnterScope())
       {
           _notifications ??= [];
           _notifications.Add(notification);
       }
    }

    /// <inheritdoc/>
    public void AddRange(ReadOnlySpan<NotificationMessage> notifications)
    {
        if (notifications.IsEmpty)
            return;

        using (_lock.EnterScope())
        {
            _notifications.AddRange(notifications.ToArray());
        }
    }

    /// <inheritdoc/>
    public ValueTask AddSync(NotificationMessage notification, CancellationToken cancellationToken = default)
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

        using (_lock.EnterScope())
        {
            if (_notifications == null || _notifications.Count == 0) 
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
        
        using (_lock.EnterScope())
        {
            if (_notifications == null || _notifications.Count == 0) return 0;
            
            var removedCount = _notifications.RemoveAll(x => x.Type == notificationType);
            return removedCount;
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        using (_lock.EnterScope())
        {
            _notifications?.Clear();
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
        using (_lock.EnterScope())
        {
            return _notifications is { Count: > 0 };
        }
    }

    /// <inheritdoc/>
    public bool HasErrors()
    {
        using (_lock.EnterScope())
        {
            return _notifications != null && _notifications.Any(n => n.Type == NotificationType.Error);
        }
    }
    
    /// <inheritdoc/>
    public bool HasWarnings()
    {
        using (_lock.EnterScope())
        {
            return _notifications != null && _notifications.Any(n => n.Type == NotificationType.Warning);
        }
    }

    /// <inheritdoc/>
    public bool HasNotificationType(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

        using (_lock.EnterScope())
        {
            return _notifications != null && _notifications.Any(n => n.Type == notificationType);
        }
    }

    /// <inheritdoc/>
    public bool Contains(string notificationId)
    {
        if (string.IsNullOrWhiteSpace(notificationId)) 
            throw new ArgumentException("Notification ID cannot be null, empty or whitespace", nameof(notificationId));

        using (_lock.EnterScope())
        {
            return _notifications != null && _notifications.Any(n => n.Id == notificationId);
        }
    }

    /// <inheritdoc/>
    public int Count()
    {
        using (_lock.EnterScope())
        {
            return _notifications?.Count ?? 0;
        }
    }

    /// <inheritdoc/>
    public int Count(NotificationType notificationType)
    {
        ArgumentNullException.ThrowIfNull(notificationType);

        using (_lock.EnterScope())
        {
            return _notifications?.Count(n  => n.Type == notificationType) ?? 0;
        }
    }

    /// <inheritdoc/>
    public int ErrorCount()
    {
        using (_lock.EnterScope())
        {
            return _notifications?.Count(n => n.Type == NotificationType.Error) ?? 0;
        }
    }

    /// <inheritdoc/>
    public int WarningCount()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IEnumerable<NotificationMessage> GetAll()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IEnumerable<NotificationMessage> GetByType(NotificationType notificationType)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<NotificationMessage> GetErrors()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<NotificationMessage> GetWarnings()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<NotificationMessage> GetInfos()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<NotificationMessage> GetSuccesses()
    {
        throw new NotImplementedException();
    }

    public NotificationMessage? GetById(string notificationId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<NotificationMessage> GetAllAsList()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<NotificationMessage> GetByTypeAsList(NotificationType notificationType)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<NotificationMessage> GetErrorsAsList()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<NotificationMessage> GetWarningsAsList()
    {
        throw new NotImplementedException();
    }

    public bool TryGetAsSpan(out ReadOnlySpan<NotificationMessage> notifications)
    {
        throw new NotImplementedException();
    }

    public int CopyToSpan(Span<NotificationMessage> destination)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> HasNotificationsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> HasErrorsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> HasWarningsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<int> CountAsync(NotificationType notificationType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}