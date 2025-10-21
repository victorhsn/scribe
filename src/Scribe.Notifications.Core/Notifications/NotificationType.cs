using System.Collections.Frozen;

namespace Scribe.Notifications.Core.Notifications;

public sealed class NotificationType : IEquatable<NotificationType>
{
    /// <summary>
    /// Gets the unique name/code of the notification type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display name or description of the notification type.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the severity level of the notification (0-100, where 100 is most severe).
    /// </summary>
    public int SeverityLevel { get; }

    /// <summary>
    /// Gets a value indicating whether this notification type is considered a failure/error state.
    /// </summary>
    public bool IsFailure { get; }

    private NotificationType(string name, string displayName, int severityLevel, bool isFailure)
    {
        Name = string.Intern(name);
        DisplayName = string.Intern(displayName);
        SeverityLevel = severityLevel;
        IsFailure = isFailure;
        UniqueId = GenerateUniqueId(name, displayName, severityLevel, isFailure);
    }

    /// <summary>
    /// Gets the Error notification type (severity: 100).
    /// Represents a critical failure that must be addressed.
    /// </summary>
    public static NotificationType Error { get; } = new("error", "Error", 100, true);

    /// <summary>
    /// Gets the Warning notification type (severity: 60).
    /// Represents a non-critical issue that should be reviewed.
    /// </summary>
    public static NotificationType Warning { get; } = new("warning", "Warning", 60, false);

    /// <summary>
    /// Gets the Info notification type (severity: 20).
    /// Represents informational messages.
    /// </summary>
    public static NotificationType Info { get; } = new("info", "Info", 20, false);

    /// <summary>
    /// Gets the Success notification type (severity: 0).
    /// Represents a successful operation.
    /// </summary>
    public static NotificationType Success { get; } = new("success", "Success", 0, false);

    /// <summary>
    /// Gets the unique identifier for this notification type instance.
    /// Used internally for cache management to ensure absolute uniqueness.
    /// </summary>
    private string UniqueId { get; }
    
    /// <summary>
    /// Internal cache of all predefined types using FrozenDictionary for thread-safe, allocation-free lookups.
    /// </summary>
    private static readonly FrozenDictionary<string, NotificationType> PredefinedTypesCache =
        new Dictionary<string, NotificationType>(StringComparer.OrdinalIgnoreCase)
        {
            { Error.Name, Error },
            { Warning.Name, Warning },
            { Info.Name, Info },
            { Success.Name, Success }
        }.ToFrozenDictionary();

    /// <summary>
    /// Cache for custom notification types created at runtime.
    /// </summary>
    private static readonly Lock CustomTypesLock = new();
    private static Dictionary<string, NotificationType>? _customTypesCache;

    /// <summary>
    /// Generates a unique cache key for composite identification.
    /// This ensures that multiple configurations of the same notification type name
    /// can coexist without conflicts.
    /// </summary>
    private static string GenerateUniqueId(string name, string displayName, int severityLevel, bool isFailure)
    => $"{name}-{displayName}-{severityLevel}-{isFailure}";

    /// <summary>
    /// Gets or creates a notification type by name.
    /// If the type is predefined, returns the cached singleton.
    /// If the type is custom, returns a cached instance or creates a new one.
    /// </summary>
    /// <param name="name">The name/code of the notification type.</param>
    /// <returns>The notification type instance (singleton or cached).</returns>
    /// <exception cref="ArgumentException">Thrown when name is empty or whitespace.</exception>
    public static NotificationType GetOrCreate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Notification type name cannot be null, empty or whitespace.", nameof(name));

        if (PredefinedTypesCache.TryGetValue(name, out var predefinedType))
            return predefinedType;

        using (CustomTypesLock.EnterScope())
        {
            _customTypesCache ??= new Dictionary<string, NotificationType>(StringComparer.OrdinalIgnoreCase);

            var type = new NotificationType(name, name, 50, false);
            
            if (_customTypesCache.TryGetValue(type.UniqueId, out var customType))
                return customType;
            
            _customTypesCache[type.UniqueId] = type;
            return type;
        }
    }

    /// <summary>
    /// Gets or creates a notification type with custom settings.
    /// </summary>
    /// <param name="name">The name/code of the notification type.</param>
    /// <param name="displayName">The display name or description.</param>
    /// <param name="severityLevel">The severity level (0-100).</param>
    /// <param name="isFailure">Whether this type represents a failure state.</param>
    /// <returns>The notification type instance (singleton or cached).</returns>
    /// <exception cref="ArgumentException">Thrown when name or displayName is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when severityLevel is not between 0 and 100.</exception>
    public static NotificationType GetOrCreate(string name, string displayName, int severityLevel, bool isFailure)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Notification type name cannot be null, empty or whitespace.", nameof(name));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Notification type display name cannot be null, empty or whitespace.", nameof(displayName));

        if (severityLevel is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(severityLevel), "Severity level must be between 0 and 100.");

        if (PredefinedTypesCache.TryGetValue(name, out var predefinedType))
            return predefinedType;

        var type = new NotificationType(name, displayName, severityLevel, isFailure);
        
        using (CustomTypesLock.EnterScope())
        {
            _customTypesCache ??= new Dictionary<string, NotificationType>(StringComparer.OrdinalIgnoreCase);

            if (_customTypesCache.TryGetValue(type.UniqueId, out var customType))
                return customType;
            
            _customTypesCache[type.UniqueId] = type;
            return type;
        }
    }

    /// <summary>
    /// Tries to get a predefined notification type by name.
    /// </summary>
    /// <param name="name">The name of the notification type.</param>
    /// <param name="notificationType">When this method returns, contains the notification type if found.</param>
    /// <returns>True if the type was found; otherwise, false.</returns>
    public static bool TryGetPredefinedType(string name, out NotificationType? notificationType) =>
    PredefinedTypesCache.TryGetValue(name, out notificationType);

    /// <summary>
    /// Gets all predefined notification types.
    /// </summary>
    /// <returns>A read-only collection of predefined notification types.</returns>
    public static IReadOnlyList<NotificationType> GetPredefinedTypes() =>
        PredefinedTypesCache.Values;

    /// <summary>
    /// Determines whether the specified <see cref="NotificationType"/> is equal to the current type.
    /// Comparison is case-insensitive by name.
    /// </summary>
    /// <param name="other">The notification type to compare.</param>
    /// <returns>True if the types are equal; otherwise, false.</returns>
    public bool Equals(NotificationType? other) =>
        other != null && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified object is equal to the current notification type.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the object is equal to the current type; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is NotificationType other && Equals(other);

    /// <summary>
    /// Gets the hash code for the current notification type.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    /// <summary>
    /// Returns a string representation of the notification type.
    /// </summary>
    /// <returns>The display name of the notification type.</returns>
    public override string ToString() =>
        DisplayName;

    /// <summary>
    /// Determines whether two specified <see cref="NotificationType"/> instances are equal.
    /// </summary>
    /// <param name="left">The first type to compare.</param>
    /// <param name="right">The second type to compare.</param>
    /// <returns>True if the types are equal; otherwise, false.</returns>
    public static bool operator ==(NotificationType? left, NotificationType? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two specified <see cref="NotificationType"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first type to compare.</param>
    /// <param name="right">The second type to compare.</param>
    /// <returns>True if the types are not equal; otherwise, false.</returns>
    public static bool operator !=(NotificationType? left, NotificationType? right) =>
        !(left == right);
}