using Scribe.Notifications.Core.Notifications;

namespace Scribe.Notifications.Tests.Notifications;

public class NotificationTypeTests
{
    [Fact]
    public void Predefined_Error_Should_Have_Correct_Properties()
    {
        var error = NotificationType.Error;

        Assert.Equal("error", error.Name);
        Assert.Equal("Error", error.DisplayName);
        Assert.Equal(100, error.SeverityLevel);
        Assert.True(error.IsFailure);
    }

    [Fact]
    public void Predefined_Warning_Should_Have_Correct_Properties()
    {
        var warning = NotificationType.Warning;

        Assert.Equal("warning", warning.Name);
        Assert.Equal("Warning", warning.DisplayName);
        Assert.Equal(60, warning.SeverityLevel);
        Assert.False(warning.IsFailure);
    }

    [Fact]
    public void Predefined_Information_Should_Have_Correct_Properties()
    {
        var information = NotificationType.Info;

        Assert.Equal("info", information.Name);
        Assert.Equal("Info", information.DisplayName);
        Assert.Equal(20, information.SeverityLevel);
        Assert.False(information.IsFailure);
    }

    [Fact]
    public void Predefined_Success_Should_Have_Correct_Properties()
    {
        var success = NotificationType.Success;

        Assert.Equal("success", success.Name);
        Assert.Equal("Success", success.DisplayName);
        Assert.Equal(0, success.SeverityLevel);
        Assert.False(success.IsFailure);
    }

    [Fact]
    public void Predefined_Types_Should_Be_Singletons()
    {
        var error1 = NotificationType.Error;
        var error2 = NotificationType.Error;

        var warning1 = NotificationType.Warning;
        var warning2 = NotificationType.Warning;

        Assert.Same(error1, error2);
        Assert.Same(warning1, warning2);
    }

    [Fact]
    public void GetOrCreate_With_PredefinedType_Should_Return_Singleton()
    {
        var error1 = NotificationType.GetOrCreate("error");
        var error2 = NotificationType.GetOrCreate("error");
        var directError = NotificationType.Error;

        Assert.Same(error1, error2);
        Assert.Same(error1, directError);
    }

    [Fact]
    public void GetOrCreate_With_CustomType_Should_Return_Cached_Instance()
    {
        var custom1 = NotificationType.GetOrCreate("custom_type");
        var custom2 = NotificationType.GetOrCreate("custom_type");

        Assert.Same(custom1, custom2);
    }

    [Fact]
    public void GetOrCreate_With_CustomType_Should_Have_Default_Severity()
    {
        var custom = NotificationType.GetOrCreate("custom_type");

        Assert.Equal(50, custom.SeverityLevel);
        Assert.False(custom.IsFailure);
    }

    [Fact]
    public void GetOrCreate_With_CustomProperties_Should_Store_Values()
    {
        var custom = NotificationType.GetOrCreate("critical", "Critical Alert", 90, true);

        Assert.Equal("critical", custom.Name);
        Assert.Equal("Critical Alert", custom.DisplayName);
        Assert.Equal(90, custom.SeverityLevel);
        Assert.True(custom.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetOrCreate_With_InvalidName_Should_Throw_ArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => NotificationType.GetOrCreate(invalidName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetOrCreate_With_CustomProperties_InvalidName_Should_Throw_ArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => NotificationType.GetOrCreate(invalidName, "Display", 50, false));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetOrCreate_With_CustomProperties_InvalidDisplayName_Should_Throw_ArgumentException(
        string invalidDisplayName)
    {
        Assert.Throws<ArgumentException>(() =>
            NotificationType.GetOrCreate(invalidDisplayName, invalidDisplayName, 50, false));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void GetOrCreate_With_InvalidSeverityLevel_Should_Throw_ArgumentOutOfRangeException(int invalidSeverityLevel)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NotificationType.GetOrCreate("name", "Display", invalidSeverityLevel, false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void GetOrCreate_With_ValidSeverityLevel_Should_Succeed(int validSeverityLevel)
    {
        var type = NotificationType.GetOrCreate("name", "Display", validSeverityLevel, false);

        Assert.Equal(validSeverityLevel, type.SeverityLevel);
    }

    [Fact]
    public void GetOrCreate_With_Same_Properties_Should_Return_Same_Cached_Instance()
    {
        var type1 = NotificationType.GetOrCreate($"alert", "Alert", 75, false);
        var type2 = NotificationType.GetOrCreate($"alert", "Alert", 75, false);

        Assert.Same(type1, type2);
    }

    [Fact]
    public void GetOrCreate_With_Different_SeverityLevel_Should_Create_Different_Instance()
    {
        var alert50 = NotificationType.GetOrCreate("alert", "Alert", 50, false);
        var alert100 = NotificationType.GetOrCreate("alert", "Alert", 100, false);

        Assert.NotSame(alert50, alert100);
        Assert.Equal(50, alert50.SeverityLevel);
        Assert.Equal(100, alert100.SeverityLevel);
        Assert.Equal("alert", alert50.Name);
        Assert.Equal("alert", alert100.Name);
    }

    [Fact]
    public void GetOrCreate_With_Different_IsFailure_Should_Create_Different_Instance()
    {
        var alertNoFailure = NotificationType.GetOrCreate("alert", "Alert", 50, false);
        var alertFailure = NotificationType.GetOrCreate("alert", "Alert", 50, true);

        Assert.NotSame(alertNoFailure, alertFailure);
        Assert.False(alertNoFailure.IsFailure);
        Assert.True(alertFailure.IsFailure);
    }

    [Fact]
    public void GetOrCreate_Multiple_Configurations_Same_Name_Should_All_Coexist()
    {

        var alert1 = NotificationType.GetOrCreate("alert", "Alert", 50, false);
        var alert2 = NotificationType.GetOrCreate("alert", "Alert", 75, false);
        var alert3 = NotificationType.GetOrCreate("alert", "Critical Alert", 90, true);
        var alert1Again = NotificationType.GetOrCreate("alert", "Alert", 50, false);

        Assert.NotSame(alert1, alert2);
        Assert.NotSame(alert2, alert3);
        Assert.NotSame(alert1, alert3);
        Assert.Same(alert1, alert1Again);
    }

    [Fact]
    public void TryGetPredefinedType_With_ValidPredefinedType_Should_Return_True_And_Type()
    {
        var result = NotificationType.TryGetPredefinedType("error", out var type);

        Assert.True(result);
        Assert.NotNull(type);
        Assert.Same(NotificationType.Error, type);
    }

    [Fact]
    public void TryGetPredefinedType_With_CaseInsensitiveName_Should_Return_True()
    {

        var result1 = NotificationType.TryGetPredefinedType("ERROR", out var type1);
        var result2 = NotificationType.TryGetPredefinedType("Error", out var type2);
        var result3 = NotificationType.TryGetPredefinedType("error", out var type3);

        Assert.False(result1);
        Assert.False(result2);
        Assert.True(result3);
        Assert.Same(type1, type2);
        Assert.NotSame(type2, type3);
    }

    [Fact]
    public void TryGetPredefined_With_NonPredefinedType_Should_Return_False()
    {

        var result = NotificationType.TryGetPredefinedType("custom_type", out var type);

        Assert.False(result);
        Assert.Null(type);
    }

    [Fact]
    public void TryGetPredefined_All_Predefined_Types_Should_Be_Found()
    {
        Assert.True(NotificationType.TryGetPredefinedType("error", out _));
        Assert.True(NotificationType.TryGetPredefinedType("warning", out _));
        Assert.True(NotificationType.TryGetPredefinedType("info", out _));
        Assert.True(NotificationType.TryGetPredefinedType("success", out _));
    }

    [Fact]
    public void GetPredefinedTypes_Should_Return_All_Four_Predefined_Types()
    {
        var types = NotificationType.GetPredefinedTypes();

        Assert.NotEmpty(types);
        Assert.Contains(NotificationType.Error, types);
        Assert.Contains(NotificationType.Warning, types);
        Assert.Contains(NotificationType.Info, types);
        Assert.Contains(NotificationType.Success, types);
    }

    [Fact]
    public void GetPredefinedTypes_Should_Return_Exactly_Four_Types()
    {
        var types = NotificationType.GetPredefinedTypes();

        Assert.Equal(4, types.Count);
    }

    [Fact]
    public void Equals_With_Same_PredefinedType_Should_Return_True()
    {
        var error1 = NotificationType.Error;
        var error2 = NotificationType.Error;

        Assert.True(error1.Equals(error2));
        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Equals_With_Same_CustomType_Should_Return_True()
    {
        var custom1 = NotificationType.GetOrCreate("custom");
        var custom2 = NotificationType.GetOrCreate("custom");

        Assert.True(custom1.Equals(custom2));
        Assert.Equal(custom1, custom2);
    }

    [Fact]
    public void Equals_With_DifferentTypes_Should_Return_False()
    {
        var error = NotificationType.Error;
        var warning = NotificationType.Warning;

        Assert.False(error.Equals(warning));
        Assert.NotEqual(error, warning);
    }

    [Fact]
    public void Equals_With_CaseInsensitiveNames_Should_Return_True()
    {
        var custom1 = NotificationType.GetOrCreate("MyType", "My Type", 50, false);
        var custom2 = NotificationType.GetOrCreate("mytype");

        Assert.True(custom1.Equals(custom2));
    }

    [Fact]
    public void Equals_With_Null_Should_Return_False()
    {
        var type = NotificationType.Error;

        Assert.False(type.Equals(null));
    }

    [Fact]
    public void Equals_With_NonNotificationTypeObject_Should_Return_False()
    {

        var type = NotificationType.Error;
        var obj = "error";

        Assert.False(type.Equals((object)obj));
    }

    [Fact]
    public void EqualityOperator_Should_Work_Correctly()
    {

        var error1 = NotificationType.Error;
        var error2 = NotificationType.Error;
        var warning = NotificationType.Warning;

        Assert.True(error1 == error2);
        Assert.False(error1 == warning);
    }

    [Fact]
    public void InequalityOperator_Should_Work_Correctly()
    {

        var error1 = NotificationType.Error;
        var error2 = NotificationType.Error;
        var warning = NotificationType.Warning;

        Assert.False(error1 != error2);
        Assert.True(error1 != warning);
    }

    [Fact]
    public void EqualityOperator_With_Null_Values_Should_Work_Correctly()
    {

        NotificationType? type1 = NotificationType.Error;
        NotificationType? type2 = null;
        NotificationType? type3 = null;

        Assert.True(type1 == NotificationType.Error);
        Assert.False(type1 == type2);
        Assert.True(type2 == type3);
    }

    [Fact]
    public void GetHashCode_With_SameType_Should_Return_SameHash()
    {
        var error1 = NotificationType.Error;
        var error2 = NotificationType.Error;

        var hash1 = error1.GetHashCode();
        var hash2 = error2.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_Should_Allow_Use_In_HashSet()
    {
        var set = new HashSet<NotificationType>
        {
            NotificationType.Error,
            NotificationType.Warning,
            NotificationType.Info,
            NotificationType.Success
        };

        var contains = set.Contains(NotificationType.Error);

        Assert.True(contains);
        Assert.Equal(4, set.Count);
    }

    [Fact]
    public void GetHashCode_Should_Allow_Use_In_Dictionary()
    {
        var dict = new Dictionary<NotificationType, string>
        {
            { NotificationType.Error, "An error occurred" },
            { NotificationType.Warning, "A warning" }
        };

        var value = dict[NotificationType.Error];

        Assert.Equal("An error occurred", value);
    }

    [Fact]
    public void ToString_Should_Return_DisplayName()
    {
        var error = NotificationType.Error;
        var warning = NotificationType.Warning;

        Assert.Equal("Error", error.ToString());
        Assert.Equal("Warning", warning.ToString());
    }

    [Fact]
    public void ToString_With_CustomType_Should_Return_DisplayName()
    {
        var custom = NotificationType.GetOrCreate("critical", "Critical Error", 95, true);

        Assert.Equal("Critical Error", custom.ToString());
    }

    [Fact]
    public async Task Concurrent_GetOrCreate_Should_Return_Same_Instance()
    {
        const int threadCount = 10;
        const int iterations = 100;
        var results = new NotificationType[threadCount * iterations];
        var tasks = new Task[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            tasks[t] = Task.Run(() =>
            {
                for (var i = 0; i < iterations; i++)
                {
                    results[threadIndex * iterations + i] = NotificationType.GetOrCreate("concurrent_test");
                }
            });
        }

        await Task.WhenAll(tasks);

        var firstInstance = results[0];
        for (var i = 1; i < results.Length; i++)
        {
            Assert.Same(firstInstance, results[i]);
        }
    }

    [Fact]
    public async Task Concurrent_Predefined_Types_Access_Should_Always_Return_Singleton()
    {
        const int threadCount = 10;
        const int iterations = 100;
        var results = new NotificationType[threadCount * iterations];
        var tasks = new Task[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            tasks[t] = Task.Run(() =>
            {
                for (var i = 0; i < iterations; i++)
                {
                    results[threadIndex * iterations + i] = NotificationType.Error;
                }
            });
        }

        await Task.WhenAll(tasks);

        var firstInstance = results[0];
        for (var i = 1; i < results.Length; i++)
        {
            Assert.Same(firstInstance, results[i]);
        }
    }

    [Fact]
    public void Predefined_Types_Access_Should_Be_Extremely_Fast()
    {
        const int iterations = 1_000_000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            _ = NotificationType.Error;
            _ = NotificationType.Warning;
            _ = NotificationType.Info;
            _ = NotificationType.Success;
        }

        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Performance test failed: {iterations * 4} accesses took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GetOrCreate_Predefined_Type_Should_Be_Fast()
    {
        const int iterations = 100_000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            _ = NotificationType.GetOrCreate("error");
        }

        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"Performance test failed: {iterations} GetOrCreate calls took {stopwatch.ElapsedMilliseconds}ms");
    }
}