using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Core.Contracts;

/// <summary>
/// Defines the contract for storing and retrieving notifications
/// Provides operations for adding, removing, and querying.
/// </summary>
public interface INotificationStore
{
    /// <summary>
    /// Add a notification to the store.
    /// </summary>
    /// <param name="notification">The notification message to add.</param>
    void Add(in NotificationMessage notification);

    /// <summary>
    /// Add multiple notifications to the store in a single operation.
    /// </summary>
    /// <param name="notifications">A span of notifications to add.</param>
    void AddRange(ReadOnlySpan<NotificationMessage> notifications);

    /// <summary>
    /// Asynchronously adds a notification to the store.
    /// Useful for operations that may involve I/O or long-running tasks.
    /// </summary>
    /// <param name="notification">The notification message to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    ValueTask AddSync(NotificationMessage notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds multiple to the store.
    /// </summary>
    /// <param name="notifications">A span of notifications to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    ValueTask AddRangeAsync(ReadOnlySpan<NotificationMessage> notifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific notification from the store by its ID.
    /// </summary>
    /// <param name="notificationId">The unique identifier of the notification to remove.</param>
    /// <returns>True if the notification was removed; otherwise, false.</returns>
    bool Remove(string notificationId);

    /// <summary>
    /// Remove all notifications of a specific type.
    /// </summary>
    /// <param name="notificationType">The type of notifications to remove.</param>
    /// <returns>The number of notifications removed.</returns>
    int RemoveByType(NotificationType notificationType);

    /// <summary>
    /// Clears all notifications from the store.
    /// </summary>
    void Clear();

    /// <summary>
    /// Asynchronously removes a specific notification by its ID.
    /// </summary>
    /// <param name="notificationId">The unique identifier of the notification to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the notification was removed; otherwise, false.</returns>
    ValueTask<bool> RemoveAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes all notifications of a specific type.
    /// </summary>
    /// <param name="notificationType">The type of notifications to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of notifications removed.</returns>
    ValueTask<int> RemoveByTypeAsync(NotificationType notificationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously clears all notifications from the store.
    /// <param name="cancellationToken">The cancellation token.</param>
    /// </summary>
    ValueTask ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the store contains any notifications
    /// </summary>
    /// <returns>True if the store has notifications; otherwise, false.</returns>
    bool HasNotifications();

    /// <summary>
    /// Determines whether the store contains any notifications of type Error.
    /// </summary>
    /// <returns>True if the store has error notifications; otherwise, false</returns>
    bool HasErrors();

    /// <summary>
    /// Determines whether the store contains any notifications of a specific type.
    /// </summary>
    /// <param name="notificationType">The type to check for.</param>
    /// <returns>True if notifications of the specified type exist; otherwise, false</returns>
    bool HasNotificationType(NotificationType notificationType);

    /// <summary>
    /// Determines whether a notification with the specified ID exists in the store.
    /// </summary>
    /// <param name="notificationId">The ID to search for.</param>
    /// <returns>True if a notification with the ID exits; otherwise, false.</returns>
    bool Contains(string notificationId);

    /// <summary>
    /// Gets the total number of notifications in the store.
    /// </summary>
    /// <returns>The count of notifications.</returns>
    int Count();

    /// <summary>
    /// Gets the number of notifications of a specific type.
    /// </summary>
    /// <param name="notificationType">The type of notifications to count.</param>
    /// <returns>The count of notifications of the specified type.</returns>
    int Count(NotificationType notificationType);

    /// <summary>
    /// Gets the number of error notifications.
    /// </summary>
    /// <returns>The count of error notifications.</returns>
    int ErrorCount();

    /// <summary>
    /// Gets the number of warning notifications.
    /// </summary>
    /// <returns>The count of warning notifications.</returns>
    int WarningCount();

    /// <summary>
    /// Gets all notifications from the store using lazy evaluation.
    /// </summary>
    /// <returns>An enumerable of notifications without immediate allocation.</returns>
    IEnumerable<NotificationMessage> GetAll();

    /// <summary>
    /// Gets all notifications of a specific type using lazy evaluation.
    /// </summary>
    /// <param name="notificationType">The type of notifications to retrieve.</param>
    /// <returns>An enumerable of notifications of the specified type.</returns>
    IEnumerable<NotificationMessage> GetByType(NotificationType notificationType);

    /// <summary>
    /// Gets all error notifications using lazy evaluation.
    /// </summary>
    /// <returns>An enumerable of error notifications</returns>
    IEnumerable<NotificationMessage> GetErrors();

    /// <summary>
    /// Gets all warning notifications using lazy evaluation.
    /// </summary>
    /// <returns>An enumerable of warning notifications</returns>
    IEnumerable<NotificationMessage> GetWarnings();

    /// <summary>
    /// Gets all info notifications using lazy evaluation.
    /// </summary>
    /// <returns>An enumerable of info notifications.</returns>
    IEnumerable<NotificationMessage> GetInfos();

    /// <summary>
    /// Gets all notifications using lazy evaluation.
    /// </summary>
    /// <returns>An enumerable of success notifications.</returns>
    IEnumerable<NotificationMessage> GetSuccesses();

    /// <summary>
    /// Gets a specific notification by its ID.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to retrieve.</param>
    /// <returns>The notification if found; otherwise, null.</returns>
    NotificationMessage? GetById(string notificationId);

    /// <summary>
    /// Gets a read-only collection of notifications of a specific type.
    /// Materializes the collection immediately.
    /// </summary>
    /// <returns>A read-only list of notifications.</returns>
    IReadOnlyList<NotificationMessage> GetAllAsList();

    /// <summary>
    /// Gets a read-only collection of notifications of a specific type.
    /// Materializes the collection immediately.
    /// </summary>
    /// <param name="notificationType">The type of notifications to retrieve.</param>
    /// <returns>A read-only list of notifications of the specified type.</returns>
    IReadOnlyList<NotificationMessage> GetByTypeAsList(NotificationType notificationType);

    /// <summary>
    /// Gets a read-only collection of error notifications.
    /// </summary>
    /// <returns>A read-only list of error notifications.</returns>
    IReadOnlyList<NotificationMessage> GetErrorsAsList();

    /// <summary>
    /// Gets a read-only collection of warning notifications.
    /// </summary>
    /// <returns>A read-only list of warning notifications.</returns>
    IReadOnlyList<NotificationMessage> GetWarningsAsList();

    /// <summary>
    /// Tries to get all notifications as a span for zero-copy access and high performance.
    /// This is useful for performance-critical scenarios.
    /// </summary>
    /// <param name="notifications">When this method returns, contains a span of notifications if successful.</param>
    /// <returns>True if the span was retrieved; otherwise, false.</returns>
    bool TryGetAsSpan(out ReadOnlySpan<NotificationMessage> notifications);

    /// <summary>
    /// Copies all notifications to a provided span.
    /// </summary>
    /// <param name="destination">The desination span.</param>
    /// <returns>The number of notifications copied.</returns>
    /// <exception cref="ArgumentException">Thrown when the destination span is too small.</exception>
    int CopyToSpan(Span<NotificationMessage> destination);

    /// <summary>
    /// Asynchronously determines whether the store contains any notifications.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask<bool> HasNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether the store contains any error notifications.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask<bool> HasErrorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether the store contains any warning notifications.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    ValueTask<bool> HasWarningsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether the store contains any notifications.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously gets the count of notifications of a specific type.
    /// </summary>
    /// <param name="notificationType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<int> CountAsync(NotificationType notificationType, CancellationToken cancellationToken = default);
}