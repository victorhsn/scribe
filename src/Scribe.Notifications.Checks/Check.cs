using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Checks;

/// <summary>
/// Represents the observable result of a domain check.
/// A check carries zero or more notifications produced during validation
/// When no notifications are present, the check passed.
/// </summary>
public sealed class Check
{
    private readonly NotificationMessage[] _notificationMessages;
    /// <summary>
    /// Gets whether the check passed (no notifications were produced)
    /// </summary>
    public bool IsValid => _notificationMessages.Length == 0;
    /// <summary>
    /// Gets the notifications produced b this check.
    /// Empty when the check passed.
    /// </summary>
    public IReadOnlyList<NotificationMessage> NotificationMessages => _notificationMessages;

    private Check(NotificationMessage[] notificationMessages)
    {
        _notificationMessages = notificationMessages ??  throw new ArgumentNullException(nameof(notificationMessages));
    }

    /// <summary>
    /// Creates a check that passed, no notifications produced.
    /// </summary>
    public static Check Pass() => new([]);

    /// <summary>
    /// Creates a check that failed with a single notification.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="notificationType">The notification type.</param>
    /// <param name="message">The notification message.</param>
    public static Check Fail(string id, NotificationType notificationType, string message) =>
        new([new NotificationMessage(id, notificationType, message)]);

    /// <summary>
    /// Creates a check from an existing set of notifications.
    /// The check is considered valid when the collection is empty.
    /// </summary>
    /// <param name="notificationMessages">The notifications produced during validation</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static Check From(IEnumerable<NotificationMessage> notificationMessages)
    {
        ArgumentNullException.ThrowIfNull(notificationMessages);
        return new Check([.. notificationMessages]);
    }
}