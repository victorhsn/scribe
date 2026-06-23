using Scribe.Notifications.Checks.Extensions;
using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Checks.Tests.Extensions;

public class NotificationStoreExtensionsTests
{
    [Fact]
    public void Apply_With_ValidCheck_Should_Not_Add_Notifications()
    {
        var store = new NotificationCollection();
        
        store.Apply(Check.Pass());
        
        Assert.Equal(0, store.Count());
    }

    [Fact]
    public void Apply_With_FailedCheck_Should_Add_Notifications()
    {
        var store = new NotificationCollection();
        
        store.Apply(Check.Fail("ID_0001", NotificationType.Error, "Failed"));
        
        Assert.Equal(1, store.Count());
    }

    [Fact]
    public void Apply_With_FailedCheck_Should_Add_Correct_Notification()
    {
        var store = new NotificationCollection();
        
        store.Apply(Check.Fail("ID_0001", NotificationType.Error, "Failed"));

        var notification = store.GetErrorsAsList()[0];
        
        Assert.Equal("ID_0001", notification.Id);
        Assert.Equal(NotificationType.Error, notification.Type);
        Assert.Equal("Failed", notification.Message);
    }

    [Fact]
    public void Apply_With_Null_Store_Should_Throw_ArgumentNullException()
    {
        NotificationCollection store = null!;
        
        Assert.Throws<ArgumentNullException>(() => store.Apply(Check.Pass()));
    }
    
    [Fact]
    public void ApplyAll_With_AllValid_Should_Not_Add_Notifications()
    {
        var store = new NotificationCollection();

        store.ApplyAll([Check.Pass(), Check.Pass(), Check.Pass()]);

        Assert.Equal(0, store.Count());
    }
    
    [Fact]
    public void ApplyAll_Should_Apply_All_Checks_Regardless_Of_Failures()
    {
        var store = new NotificationCollection();

        store.ApplyAll([
            Check.Fail("ID_001", NotificationType.Error, "Error 1."),
            Check.Pass(),
            Check.Fail("ID_002", NotificationType.Error, "Error 2.")
        ]);

        Assert.Equal(2, store.Count());
    }
    
    [Fact]
    public void ApplyAll_With_Null_Store_Should_Throw_ArgumentNullException()
    {
        NotificationCollection store = null!;

        Assert.Throws<ArgumentNullException>(() => store.ApplyAll([Check.Pass()]));
    }

    [Fact]
    public void ApplyAll_With_Null_Checks_Should_Throw_ArgumentNullException()
    {
        var store = new NotificationCollection();

        Assert.Throws<ArgumentNullException>(() => store.ApplyAll(null!));
    }

    [Fact]
    public void ApplyAll_With_MultipleNotifications_Per_Check_Should_Add_All()
    {
        var store = new NotificationCollection();
        var check = Check.From([
            new NotificationMessage("ID_001", NotificationType.Error, "Error 1."),
            new NotificationMessage("ID_002", NotificationType.Warning, "Warning 1.")
        ]);

        store.ApplyAll([check]);

        Assert.Equal(2, store.Count());
    }
    
    [Fact]
    public async Task ApplyAsync_With_ValidCheck_Should_Not_Add_Notifications()
    {
        var store = new NotificationCollection();

        await store.ApplyAsync(ValueTask.FromResult(Check.Pass()));

        Assert.Equal(0, await store.CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_With_FailedCheck_Should_Add_Notification()
    {
        var store = new NotificationCollection();

        await store.ApplyAsync(ValueTask.FromResult(
            Check.Fail("ID_001", NotificationType.Error, "Something failed.")));

        Assert.Equal(1, await store.CountAsync());
    }

    [Fact]
    public async Task ApplyAsync_With_CancelledToken_Should_Throw()
    {
        var store = new NotificationCollection();
        var ct = new CancellationToken(canceled: true);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => store.ApplyAsync(ValueTask.FromResult(Check.Pass()), ct).AsTask());
    }

    [Fact]
    public async Task ApplyAsync_With_Null_Store_Should_Throw_ArgumentNullException()
    {
        NotificationCollection store = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.ApplyAsync(ValueTask.FromResult(Check.Pass())).AsTask());
    }
}