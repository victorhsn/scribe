using Scribe.Notifications.Core.Contracts;

namespace Scribe.Notifications.Checks.Extensions;

/// <summary>
/// Extension methods that integrate <see cref="Check"/> with <see cref="INotificationStore"/>
/// </summary>
public static class NotificationStoreExtensions
{
    /// <summary>
    /// Applies a check to the notification store.
    /// If the check failed, all its notifications are added to the store
    /// </summary>
    /// <param name="store">The notification store to add failures to.</param>
    /// <param name="check">The check result to apply</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Apply(this INotificationStore store, Check check)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(check);

        if (check.IsValid)
            return;
        
        foreach (var notification in check.NotificationMessages)
            store.Add(notification);
    }

    /// <summary>
    /// Applies multiple checks to the notification store.
    /// All checks are evaluated regardless of previous failure.
    /// </summary>
    /// <param name="store">The notification store to add failures to.</param>
    /// <param name="checks">The check results to apply</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ApplyAll(this INotificationStore store, IEnumerable<Check> checks)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(checks);
        
        foreach (var check in checks)
            store.Apply(check);
    }

    /// <summary>
    /// Applies a check asynchronously to the notification store.
    /// If the check failed, all its notifications are added to the store.
    /// </summary>
    /// <param name="store">The notification store to add failures to.</param>
    /// <param name="checkTask">The async check result to apply.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async ValueTask ApplyAsync(this INotificationStore store, 
        ValueTask<Check> checkTask,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ct.ThrowIfCancellationRequested();

        var check = await checkTask;
        store.Apply(check);
    }
}