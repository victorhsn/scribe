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

    [Fact]
    public void Constructor_With_CustomTimpestamp_Should_Store_Timestamp()
    {
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, object> { { "field", "email" } };

        var notification = new NotificationMessage("USER_001",  NotificationType.Error, "Invalid Email", timestamp,  metadata);
        
        Assert.Equal(timestamp, notification.CreatedAt);
        Assert.NotNull(notification.Metadata);
        Assert.Equal("email", notification.Metadata["field"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_With_NullOrEmptyMessage_Should_Throw_ArgumentNullException(string invalidMessage)
    {
        Assert.Throws<ArgumentException>(() => new NotificationMessage("USER_001", NotificationType.Error, invalidMessage));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_With_NullOrEmptyMessage_Should_Throw_ArgumentException(string invalidMessage)
    {
        Assert.Throws<ArgumentException>(() =>
            new NotificationMessage("ID_001", NotificationType.Error, invalidMessage));
    }
    
    [Fact]
    public void Constructor_Should_Intern_Id()
    {
        var id1 = new string(new[] { 'U', 'S', 'E', 'R', '_', '0', '0', '1' });
        var id2 = new string(new[] { 'U', 'S', 'E', 'R', '_', '0', '0', '1' });

        var notification1 = new NotificationMessage(id1, NotificationType.Error, "Message");
        var notification2 = new NotificationMessage(id2, NotificationType.Error, "Message");

        Assert.Same(notification1.Id, notification2.Id);
    }
    
    [Fact]
    public void WithId_Should_Create_New_NotificationMessage_With_Different_Id()
    {
        var original = new NotificationMessage("ID_001", NotificationType.Error, "Message");
        
        var modified = original.WithId("ID_002");
        
        Assert.Equal("ID_002", modified.Id);
        Assert.Equal(NotificationType.Error, modified.Type);
        Assert.Equal("Message", modified.Message);
        Assert.Equal(original.CreatedAt, modified.CreatedAt);
        Assert.NotEqual(original.Id, modified.Id);
    }
    
    [Fact]
    public void WithType_Should_Create_New_NotificationMessage_With_Different_Type()
    {
 
        var original = new NotificationMessage("ID_001", NotificationType.Error, "Message");
        
        var modified = original.WithType(NotificationType.Warning);
        
        Assert.Equal("ID_001", modified.Id);
        Assert.Equal(NotificationType.Warning, modified.Type);
        Assert.Equal("Message", modified.Message);
        Assert.Equal(original.CreatedAt, modified.CreatedAt);
        Assert.NotEqual(original.Type, modified.Type);
    }
    
    [Fact]
    public void WithMessage_Should_Create_New_NotificationMessage_With_Different_Message()
    {
        var original = new NotificationMessage("ID_001", NotificationType.Error, "Original Message");
        
        var modified = original.WithMessage("New Message");
        
        Assert.Equal("ID_001", modified.Id);
        Assert.Equal(NotificationType.Error, modified.Type);
        Assert.Equal("New Message", modified.Message);
        Assert.Equal(original.CreatedAt, modified.CreatedAt);
        Assert.NotEqual(original.Message, modified.Message);
    }
    
    [Fact]
    public void WithMetadata_Should_Create_New_NotificationMessage_With_Metadata()
    {
        var original = new NotificationMessage("ID_001", NotificationType.Error, "Message");
        var metadata = new Dictionary<string, object> { { "field", "email" } };
        
        var modified = original.WithMetadata(metadata);
        
        Assert.Equal("ID_001", modified.Id);
        Assert.Equal(NotificationType.Error, modified.Type);
        Assert.Equal("Message", modified.Message);
        Assert.NotNull(modified.Metadata);
        Assert.Equal("email", modified.Metadata["field"]);
        Assert.Null(original.Metadata);
    }
    
    [Fact]
    public void With_Methods_Should_Preserve_Immutability()
    {
        var original = new NotificationMessage("ID_001", NotificationType.Error, "Message");
        
        var modified = original.WithId("ID_002");
        
        Assert.Equal("ID_001", original.Id);
        Assert.Equal("ID_002", modified.Id);
    }
    
   [Fact]
    public void ToString_Should_Return_Formatted_String()
    {
        var notification = new NotificationMessage("USER_VALIDATION_001", NotificationType.Error, "Invalid email");
        
        var result = notification.ToString();
        
        Assert.Contains("USER_VALIDATION_001", result);
        Assert.Contains("Error", result);
        Assert.Contains("Invalid email", result);
        Assert.StartsWith("[", result);
    }

    [Fact]
    public void ToString_Should_Include_All_Key_Information()
    {
        var notification = new NotificationMessage("ID_001", NotificationType.Warning, "Test message");
        
        var result = notification.ToString();
        
        Assert.Equal("[ID_001] Warning: Test message", result);
    }
    
    [Fact]
    public void Struct_Should_Be_StackAllocated()
    {
        var notification = new NotificationMessage("ID_001", NotificationType.Error, "Message");
        
        Assert.True(typeof(NotificationMessage).IsValueType);
    }

    [Fact]
    public void Struct_Assignment_Should_Copy_Value()
    {
        var notification1 = new NotificationMessage("ID_001", NotificationType.Error, "Message");

        var notification2 = notification1;
        var notification3 = notification1.WithId("ID_002");
        
        Assert.Equal(notification1.Id, notification2.Id); // Copy of notification1
        Assert.NotEqual(notification1.Id, notification3.Id); // New instance
    }

    [Fact]
    public void Default_Struct_Should_Have_Default_Values()
    {
        var notification = default(NotificationMessage);
        
        Assert.Null(notification.Id);
        Assert.Null(notification.Type);
        Assert.Null(notification.Message);
        Assert.Equal(default(DateTime), notification.CreatedAt);
        Assert.Null(notification.Metadata);
    }
    
    [Fact]
    public void CreatedAt_Should_Be_UtcNow_By_Default()
    {
        var beforeCreation = DateTime.UtcNow;
        
        var notification = new NotificationMessage("ID_001", NotificationType.Error, "Message");

        var afterCreation = DateTime.UtcNow;
        
        Assert.True(notification.CreatedAt >= beforeCreation);
        Assert.True(notification.CreatedAt <= afterCreation);
    }

    [Fact]
    public void CreatedAt_Should_Be_Preserved_In_With_Methods()
    {
        var timestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var notification = new NotificationMessage("ID_001", NotificationType.Error, "Message", timestamp, null);
        
        var modified = notification.WithId("ID_002");
        
        Assert.Equal(timestamp, modified.CreatedAt);
    }
}