using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Tests.Notifications;

public class NotificationCollectionTests
{

    [Fact]
    public void Add_With_ValidNotification_Should_Store_It()
    {
        var collection = new NotificationCollection();
        var notification = new NotificationMessage("ID_001", NotificationType.Error, "Error message");

        collection.Add(notification);

        Assert.Equal(1, collection.Count());
    }

    [Fact]
    public void Add_With_EmptyId_Should_Throw_ArgumentException()
    {
        var collection = new NotificationCollection();

        Assert.Throws<ArgumentException>(() =>
            collection.Add(new NotificationMessage("  ", NotificationType.Error, "Error")));
    }

    [Fact]
    public void Add_Without_Id_Should_Generate_Id_Automatically()
    {
        var collection = new NotificationCollection();

        collection.Add(NotificationType.Error, "Email inválido");

        Assert.Equal(1, collection.Count());
        var all = collection.GetAllAsList();
        Assert.NotNull(all[0].Id);
        Assert.NotEmpty(all[0].Id);
    }

    [Fact]
    public void Add_Without_Id_Multiple_Should_Generate_Unique_Ids()
    {
        var collection = new NotificationCollection();

        collection.Add(NotificationType.Error, "Error 1");
        collection.Add(NotificationType.Error, "Error 2");

        var all = collection.GetAllAsList();
        Assert.NotEqual(all[0].Id, all[1].Id);
    }

    [Fact]
    public void AddRange_With_ValidNotifications_Should_Store_All()
    {
        var collection = new NotificationCollection();
        ReadOnlySpan<NotificationMessage> batch =
        [
            new("ID_001", NotificationType.Error, "Error 1"),
            new("ID_002", NotificationType.Warning, "Warning 1"),
            new("ID_003", NotificationType.Info, "Info 1")
        ];

        collection.AddRange(batch);

        Assert.Equal(3, collection.Count());
    }

    [Fact]
    public void AddRange_With_EmptySpan_Should_Not_Add_Anything()
    {
        var collection = new NotificationCollection();

        collection.AddRange(ReadOnlySpan<NotificationMessage>.Empty);

        Assert.Equal(0, collection.Count());
    }

    [Fact]
    public async Task AddAsync_Should_Store_Notification()
    {
        var collection = new NotificationCollection();
        var notification = new NotificationMessage("ID_001", NotificationType.Error, "Error");

        await collection.AddAsync(notification);

        Assert.Equal(1, await collection.CountAsync());
    }

    [Fact]
    public async Task AddAsync_Without_Id_Should_Generate_Id_Automatically()
    {
        var collection = new NotificationCollection();

        await collection.AddAsync(NotificationType.Warning, "Warning");

        Assert.Equal(1, await collection.CountAsync());
    }

    [Fact]
    public async Task AddRangeAsync_Should_Store_All_Notifications()
    {
        var collection = new NotificationCollection();
        NotificationMessage[] batch =
        [
            new("ID_001", NotificationType.Error, "Error 1"),
            new("ID_002", NotificationType.Warning, "Warning 1")
        ];

        await collection.AddRangeAsync(batch);

        Assert.Equal(2, await collection.CountAsync());
    }

    [Fact]
    public async Task AddAsync_With_CancelledToken_Should_Throw()
    {
        var collection = new NotificationCollection();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            collection.AddAsync(new NotificationMessage("ID_001", NotificationType.Error, "Error"), cts.Token).AsTask());
    }

    [Fact]
    public void Remove_With_ExistingId_Should_Return_True_And_Remove()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        var result = collection.Remove("ID_001");

        Assert.True(result);
        Assert.Equal(0, collection.Count());
    }

    [Fact]
    public void Remove_With_NonExistingId_Should_Return_False()
    {
        var collection = new NotificationCollection();

        var result = collection.Remove("NON_EXISTING");

        Assert.False(result);
    }

    [Fact]
    public void Remove_With_EmptyCollection_Should_Return_False()
    {
        var collection = new NotificationCollection();

        var result = collection.Remove("ID_001");

        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Remove_With_InvalidId_Should_Throw_ArgumentException(string? invalidId)
    {
        var collection = new NotificationCollection();

        Assert.Throws<ArgumentException>(() => collection.Remove(invalidId!));
    }

    [Fact]
    public void RemoveByType_Should_Remove_All_Of_Type_And_Return_Count()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error 1"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error 2"));
        collection.Add(new NotificationMessage("ID_003", NotificationType.Warning, "Warning"));

        var removed = collection.RemoveByType(NotificationType.Error);

        Assert.Equal(2, removed);
        Assert.Equal(1, collection.Count());
        Assert.False(collection.HasErrors());
    }

    [Fact]
    public void RemoveByType_With_NoMatchingType_Should_Return_Zero()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning"));

        var removed = collection.RemoveByType(NotificationType.Error);

        Assert.Equal(0, removed);
        Assert.Equal(1, collection.Count());
    }

    [Fact]
    public void RemoveByType_With_NullType_Should_Throw_ArgumentNullException()
    {
        var collection = new NotificationCollection();

        Assert.Throws<ArgumentNullException>(() => collection.RemoveByType(null!));
    }

    [Fact]
    public void Clear_Should_Remove_All_Notifications()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error 1"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning 1"));

        collection.Clear();

        Assert.Equal(0, collection.Count());
        Assert.False(collection.HasNotifications());
    }

    [Fact]
    public void Clear_On_EmptyCollection_Should_Not_Throw()
    {
        var collection = new NotificationCollection();

        var exception = Record.Exception(() => collection.Clear());

        Assert.Null(exception);
    }

    [Fact]
    public void HasNotifications_With_Empty_Collection_Should_Return_False()
    {
        var collection = new NotificationCollection();

        Assert.False(collection.HasNotifications());
    }

    [Fact]
    public void HasNotifications_With_Any_Notification_Should_Return_True()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Info, "Info"));

        Assert.True(collection.HasNotifications());
    }

    [Fact]
    public void HasErrors_With_Errors_Should_Return_True()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        Assert.True(collection.HasErrors());
    }

    [Fact]
    public void HasErrors_Without_Errors_Should_Return_False()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning"));

        Assert.False(collection.HasErrors());
    }

    [Fact]
    public void HasWarnings_With_Warnings_Should_Return_True()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning"));

        Assert.True(collection.HasWarnings());
    }

    [Fact]
    public void HasNotificationType_With_MatchingType_Should_Return_True()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Info, "Info"));

        Assert.True(collection.HasNotificationType(NotificationType.Info));
    }

    [Fact]
    public void HasNotificationType_With_NonMatchingType_Should_Return_False()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning"));

        Assert.False(collection.HasNotificationType(NotificationType.Error));
    }

    [Fact]
    public void Contains_With_ExistingId_Should_Return_True()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        Assert.True(collection.Contains("ID_001"));
    }

    [Fact]
    public void Contains_With_NonExistingId_Should_Return_False()
    {
        var collection = new NotificationCollection();

        Assert.False(collection.Contains("ID_999"));
    }

    [Fact]
    public void Count_Should_Return_Total_Number_Of_Notifications()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));
        collection.Add(new NotificationMessage("ID_003", NotificationType.Info, "Info"));

        Assert.Equal(3, collection.Count());
    }

    [Fact]
    public void Count_With_Empty_Collection_Should_Return_Zero()
    {
        var collection = new NotificationCollection();

        Assert.Equal(0, collection.Count());
    }

    [Fact]
    public void Count_By_Type_Should_Return_Correct_Number()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error 1"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error 2"));
        collection.Add(new NotificationMessage("ID_003", NotificationType.Warning, "Warning"));

        Assert.Equal(2, collection.Count(NotificationType.Error));
        Assert.Equal(1, collection.Count(NotificationType.Warning));
        Assert.Equal(0, collection.Count(NotificationType.Info));
    }

    [Fact]
    public void ErrorCount_Should_Return_Number_Of_Errors()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error 1"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error 2"));
        collection.Add(new NotificationMessage("ID_003", NotificationType.Warning, "Warning"));

        Assert.Equal(2, collection.ErrorCount());
    }

    [Fact]
    public void WarningCount_Should_Return_Number_Of_Warnings()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning 1"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error"));

        Assert.Equal(1, collection.WarningCount());
    }

    [Fact]
    public void GetAll_Should_Return_All_Notifications_Lazily()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        var all = collection.GetAll().ToList();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetAll_On_Empty_Collection_Should_Return_Empty()
    {
        var collection = new NotificationCollection();

        var all = collection.GetAll().ToList();

        Assert.Empty(all);
    }

    [Fact]
    public void GetByType_Should_Return_Only_Matching_Type()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));
        collection.Add(new NotificationMessage("ID_003", NotificationType.Error, "Error 2"));

        var errors = collection.GetByType(NotificationType.Error).ToList();

        Assert.Equal(2, errors.Count);
        Assert.All(errors, n => Assert.Equal(NotificationType.Error, n.Type));
    }

    [Fact]
    public void GetErrors_Should_Return_Only_Errors()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        var errors = collection.GetErrors().ToList();

        Assert.Single(errors);
        Assert.Equal("ID_001", errors[0].Id);
    }

    [Fact]
    public void GetWarnings_Should_Return_Only_Warnings()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        var warnings = collection.GetWarnings().ToList();

        Assert.Single(warnings);
        Assert.Equal("ID_002", warnings[0].Id);
    }

    [Fact]
    public void GetInfos_Should_Return_Only_Infos()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Info, "Info"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error"));

        var infos = collection.GetInfos().ToList();

        Assert.Single(infos);
        Assert.Equal("ID_001", infos[0].Id);
    }

    [Fact]
    public void GetSuccesses_Should_Return_Only_Successes()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Success, "Success"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error"));

        var successes = collection.GetSuccesses().ToList();

        Assert.Single(successes);
        Assert.Equal("ID_001", successes[0].Id);
    }

    [Fact]
    public void GetAllAsList_Should_Return_All_As_ReadOnlyList()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        var all = collection.GetAllAsList();

        Assert.IsAssignableFrom<IReadOnlyList<NotificationMessage>>(all);
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetAllAsList_On_Empty_Should_Return_Empty_List()
    {
        var collection = new NotificationCollection();

        var all = collection.GetAllAsList();

        Assert.Empty(all);
    }

    [Fact]
    public void GetAllAsList_Should_Return_Copy_Not_Internal_Reference()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        var snapshot = collection.GetAllAsList();
        collection.Clear();

        Assert.Single(snapshot);
        Assert.Equal(0, collection.Count());
    }

    [Fact]
    public void GetErrorsAsList_Should_Return_Only_Errors()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        var errors = collection.GetErrorsAsList();

        Assert.Single(errors);
        Assert.Equal(NotificationType.Error, errors[0].Type);
    }

    [Fact]
    public void GetWarningsAsList_Should_Return_Only_Warnings()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Info, "Info"));

        var warnings = collection.GetWarningsAsList();

        Assert.Single(warnings);
        Assert.Equal(NotificationType.Warning, warnings[0].Type);
    }

    [Fact]
    public void GetById_With_ExistingId_Should_Return_Notification()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        var result = collection.GetById("ID_001");

        Assert.NotNull(result);
        Assert.Equal("ID_001", result.Value.Id);
    }

    [Fact]
    public void GetById_With_NonExistingId_Should_Return_Null()
    {
        var collection = new NotificationCollection();

        var result = collection.GetById("NON_EXISTING");

        Assert.Null(result);
    }

    [Fact]
    public void GetById_On_Empty_Collection_Should_Return_Null()
    {
        var collection = new NotificationCollection();

        var result = collection.GetById("ID_001");

        Assert.Null(result);
    }

    [Fact]
    public void TryGetAsSpan_With_Notifications_Should_Return_True_And_Span()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        var result = collection.TryGetAsSpan(out var span);

        Assert.True(result);
        Assert.Equal(1, span.Length);
    }

    [Fact]
    public void TryGetAsSpan_On_Empty_Collection_Should_Return_False()
    {
        var collection = new NotificationCollection();

        var result = collection.TryGetAsSpan(out var span);

        Assert.False(result);
        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void CopyToSpan_Should_Copy_All_Notifications()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        Span<NotificationMessage> destination = new NotificationMessage[2];
        var count = collection.CopyToSpan(destination);

        Assert.Equal(2, count);
        Assert.Equal("ID_001", destination[0].Id);
        Assert.Equal("ID_002", destination[1].Id);
    }

    [Fact]
    public void CopyToSpan_With_Empty_Collection_Should_Return_Zero()
    {
        var collection = new NotificationCollection();
        Span<NotificationMessage> destination = new NotificationMessage[5];

        var count = collection.CopyToSpan(destination);

        Assert.Equal(0, count);
    }

    [Fact]
    public void CopyToSpan_With_TooSmall_Destination_Should_Throw()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Error, "Error 2"));

        var tooSmall = new NotificationMessage[1];

        Assert.Throws<ArgumentException>(() => collection.CopyToSpan(tooSmall));
    }

    [Fact]
    public async Task HasErrorsAsync_Should_Return_True_When_Errors_Exist()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        Assert.True(await collection.HasErrorsAsync());
    }

    [Fact]
    public async Task HasWarningsAsync_Should_Return_True_When_Warnings_Exist()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Warning, "Warning"));

        Assert.True(await collection.HasWarningsAsync());
    }

    [Fact]
    public async Task CountAsync_Should_Return_Total()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));
        collection.Add(new NotificationMessage("ID_002", NotificationType.Warning, "Warning"));

        Assert.Equal(2, await collection.CountAsync());
    }

    [Fact]
    public async Task RemoveAsync_Should_Remove_And_Return_True()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        var result = await collection.RemoveAsync("ID_001");

        Assert.True(result);
        Assert.Equal(0, await collection.CountAsync());
    }

    [Fact]
    public async Task ClearAsync_Should_Remove_All()
    {
        var collection = new NotificationCollection();
        collection.Add(new NotificationMessage("ID_001", NotificationType.Error, "Error"));

        await collection.ClearAsync();

        Assert.Equal(0, await collection.CountAsync());
    }

    [Fact]
    public async Task Concurrent_Adds_Should_All_Be_Stored()
    {
        const int threadCount = 10;
        const int itemsPerThread = 100;
        var collection = new NotificationCollection();
        var tasks = new Task[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            tasks[t] = Task.Run(() =>
            {
                for (var i = 0; i < itemsPerThread; i++)
                    collection.Add(new NotificationMessage(
                        $"ID_{threadIndex}_{i}",
                        NotificationType.Error,
                        "Error"));
            });
        }

        await Task.WhenAll(tasks);

        Assert.Equal(threadCount * itemsPerThread, collection.Count());
    }

    [Fact]
    public async Task Concurrent_Reads_And_Writes_Should_Not_Throw()
    {
        var collection = new NotificationCollection();
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        var writer = Task.Run(() =>
        {
            for (var i = 0; i < 200; i++)
                collection.Add(new NotificationMessage($"ID_{i}", NotificationType.Error, "Error"));
        });

        var reader = Task.Run(() =>
        {
            for (var i = 0; i < 200; i++)
            {
                try { _ = collection.Count(); }
                catch (Exception ex) { exceptions.Add(ex); }
            }
        });

        await Task.WhenAll(writer, reader);

        Assert.Empty(exceptions);
    }
}