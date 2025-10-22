namespace Scribe.Notifications.Core.Notifications;

public readonly struct NotificationMessage : IEquatable<NotificationMessage>
{
    /// <summary>
    /// Gets the unique identifier of the notification.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the type of the notification (Error, Warning, Info, Success, etc).
    /// </summary>
    public NotificationType Type { get; }

    /// <summary>
    /// Gets the message content of the notification.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the timestamp when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Gets optional metadata associated with the notification.
    /// This is useful for storing additional context without modifying the core structure.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationMessage"/> struct.
    /// </summary>
    /// <param name="id">The unique identifier for the notification.</param>
    /// <param name="type">The type of the notification.</param>
    /// <param name="message">The message content.</param>
    /// <exception cref="ArgumentNullException">Thrown when id or message is null.</exception>
    /// <exception cref="ArgumentException">Thrown when id or message is empty or whitespace.</exception>
    public NotificationMessage(string id, NotificationType type, string message)
        : this(id, type, message, DateTime.UtcNow, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationMessage"/> struct with metadata.
    /// </summary>
    /// <param name="id">The unique identifier for the notification.</param>
    /// <param name="type">The type of the notification.</param>
    /// <param name="message">The message content.</param>
    /// <param name="metadata">Optional metadata dictionary.</param>
    /// <exception cref="ArgumentNullException">Thrown when id or message is null.</exception>
    /// <exception cref="ArgumentException">Thrown when id or message is empty or whitespace.</exception>
    public NotificationMessage(string id, NotificationType type, string message, IReadOnlyDictionary<string, object>? metadata)
        : this(id, type, message, DateTime.UtcNow, metadata)
    {
    }

    /// Initializes a new instance of the <see cref="NotificationMessage"/> struct with a specific creation timestamp.
    /// </summary>
    /// <param name="id">The unique identifier for the notification.</param>
    /// <param name="type">The type of the notification.</param>
    /// <param name="message">The message content.</param>
    /// <param name="createdAt">The timestamp when the notification was created.</param>
    /// <param name="metadata">Optional metadata dictionary.</param>
    /// <exception cref="ArgumentNullException">Thrown when id or message is null.</exception>
    /// <exception cref="ArgumentException">Thrown when id or message is empty or whitespace.</exception>
    public NotificationMessage(string id, NotificationType type, string message, DateTime createdAt,
        IReadOnlyDictionary<string, object>? metadata)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Notification ID cannot be null, empty, or whitespace.", nameof(id));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message cannot be null, empty, or whitespace.", nameof(message));

        Id = string.Intern(id);
        Type = type;
        Message = message;
        Metadata = metadata;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Creates a new <see cref="NotificationMessage"/> with a different ID.
    /// Useful for creating derived notifications while maintaining immutability.
    /// </summary>
    /// <param name="id">The new notification ID.</param>
    /// <returns>A new notification message with the specified ID.</returns>
    public NotificationMessage WithId(string id) => new NotificationMessage(id, Type, Message, CreatedAt, Metadata);

    /// <summary>
    /// Creates a new <see cref="NotificationMessage"/> with a different type.
    /// </summary>
    /// <param name="type">The new notification type.</param>
    /// <returns>A new notification message with the specified type.</returns>
    public NotificationMessage WithType(NotificationType type) => new(Id, type, Message, CreatedAt, Metadata);

    /// <summary>
    /// Creates a new <see cref="NotificationMessage"/> with a different message.
    /// </summary>
    /// <param name="message">The new message content.</param>
    /// <returns>A new notification message with the specified message.</returns>
    public NotificationMessage WithMessage(string message) => new(Id, Type, message, CreatedAt, Metadata);

    /// <summary>
    /// Creates a new <see cref="NotificationMessage"/> with additional or updated metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata dictionary.</param>
    /// <returns>A new notification message with the specified metadata.</returns>
    public NotificationMessage WithMetadata(IReadOnlyDictionary<string, object?> metadata) => new(Id, Type, Message, CreatedAt, metadata);
    /// <summary>
    /// Determines whether the specified <see cref="NotificationMessage"/> is equal to the current notification.
    /// </summary>
    /// <param name="other">The notification to compare with the current one.</param>
    /// <returns>True if the notifications are equal; otherwise, false.</returns>
    public bool Equals(NotificationMessage other) =>
        Id == other.Id &&
        Type == other.Type &&
        Message == other.Message &&
        CreatedAt == other.CreatedAt;

    /// <summary>
    /// Determines whether the specified object is equal to the current notification.
    /// </summary>
    /// <param name="obj">The object to compare with the current notification.</param>
    /// <returns>True if the object is equal to the current notification; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is NotificationMessage other && Equals(other);

    /// <summary>
    /// Gets the hash code for the current notification.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(Id, Type, Message, CreatedAt);

    /// <summary>
    /// Returns a string representation of the notification.
    /// Format: [ID] Type: Message
    /// </summary>
    /// <returns>A string representation of the notification.</returns>
    public override string ToString() =>
        $"[{Id}] {Type.Name}: {Message}";

    /// <summary>
    /// Determines whether two specified <see cref="NotificationMessage"/> instances are equal.
    /// </summary>
    /// <param name="left">The first notification to compare.</param>
    /// <param name="right">The second notification to compare.</param>
    /// <returns>True if the notifications are equal; otherwise, false.</returns>
    public static bool operator ==(NotificationMessage left, NotificationMessage right) =>
        left.Equals(right);

    /// <summary>
    /// Determines whether two specified <see cref="NotificationMessage"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first notification to compare.</param>
    /// <param name="right">The second notification to compare.</param>
    /// <returns>True if the notifications are not equal; otherwise, false.</returns>
    public static bool operator !=(NotificationMessage left, NotificationMessage right) =>
        !left.Equals(right);
}