using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Tests.Notifications;

public class NotificationMessageTests
{
    [Fact]
    public void Constructor_With_ValidParameters_Should_Create_Notification()
    {
        var notification = new NotificationMessage("USER_001", NotificationType.Error, "Invalid Email");
        
        Assert.Equal("USER_001", notification.Id);
        Assert.Equal(NotificationType.Error, notification.Type);
        Assert.Equal("Invalid Email", notification.Message);
        Assert.NotEqual(default(DateTime), notification.CreatedAt);
        Assert.Null(notification.Metadata);
    }

    [Fact]
    public void Constructor_With_Metadata_Should_Store_Metadata()
    {
        var metadata = new Dictionary<string, object> { { "field", "email" } };
        
        var notification = new NotificationMessage("USER_0001", NotificationType.Error, "Invalid Email", metadata);
        
        Assert.NotNull(notification.Metadata);
        Assert.Equal("email", notification.Metadata["field"]);
    }
}