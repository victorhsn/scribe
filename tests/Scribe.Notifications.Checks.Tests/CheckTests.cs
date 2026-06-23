using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Checks.Tests;

public class CheckTests
{
    [Fact]
    public void Pass_Should_Return_Valid_Check()
    {
        var check = Check.Pass();
        
        Assert.True(check.IsValid);
        Assert.Empty(check.NotificationMessages);
    }

    [Fact]
    public void Fail_Should_Return_Invalid_Check()
    {
        var check = Check.Fail("ID_0001", NotificationType.Error, "Failed");

        Assert.False(check.IsValid);
    }

    [Fact]
    public void Fail_Should_Contain_Single_Notification()
    {
        var check = Check.Fail("ID_0001", NotificationType.Error, "Failed");
        
        Assert.Single(check.NotificationMessages);
    }

    [Fact]
    public void Fail_Should_Contain_Correct_Notification()
    {
        var check = Check.Fail("ID_0001", NotificationType.Error, "Failed");
        var notification = check.NotificationMessages[0];
        Assert.Equal(NotificationType.Error, notification.Type);
        Assert.Equal("Failed", notification.Message);
    }

    [Fact]
    public void From_With_Empty_Collection_Should_Return_Valid_Check()
    {
        var check = Check.From([]);
        Assert.True(check.IsValid);
        Assert.Empty(check.NotificationMessages);
    }

    [Fact]
    public void From_With_Notifications_Should_Return_Invalid_Check()
    {
        var notifications = new List<NotificationMessage>()
        {
            new("ID_0001", NotificationType.Error, "Failed"),
            new("ID_0002", NotificationType.Warning, "Warning"),
        };
        
        var check = Check.From(notifications);
        Assert.False(check.IsValid);
        Assert.Equal(2, check.NotificationMessages.Count);
    }

    [Fact]
    public void From_With_Null_Should_Throw_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Check.From(null!));
    }
    
    [Fact]
    public void From_Should_Preserve_All_Notifications()
    {
        var notifications = new List<NotificationMessage>
        {
            new("ID_001", NotificationType.Error, "Error 1."),
            new("ID_002", NotificationType.Error, "Error 2."),
            new("ID_003", NotificationType.Warning, "Warning 1.")
        };

        var check = Check.From(notifications);

        Assert.Equal(3, check.NotificationMessages.Count);
        Assert.Equal("ID_001", check.NotificationMessages[0].Id);
        Assert.Equal("ID_002", check.NotificationMessages[1].Id);
        Assert.Equal("ID_003", check.NotificationMessages[2].Id);
    }
    
    [Fact]
    public void NotificationMessages_Should_Be_Immutable()
    {
        var check = Check.Fail("ID_001", NotificationType.Error, "Error.");

        Assert.IsAssignableFrom<IReadOnlyList<NotificationMessage>>(check.NotificationMessages);
    }
    
    [Fact]
    public void From_Should_Not_Be_Affected_By_Source_Collection_Changes()
    {
        var notifications = new List<NotificationMessage>
        {
            new("ID_001", NotificationType.Error, "Error 1.")
        };

        var check = Check.From(notifications);
        notifications.Add(new("ID_002", NotificationType.Error, "Error 2."));

        Assert.Single(check.NotificationMessages);
    }
}
