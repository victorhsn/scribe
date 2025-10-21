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
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetOrCreate_With_InvalidName_Should_Throw_ArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => NotificationType.GetOrCreate(invalidName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetOrCreate_With_CustomProperties_InvalidName_Should_Throw_ArgumentException(string invalidName)
    {
        Assert.Throws<ArgumentException>(() => NotificationType.GetOrCreate(invalidName, "Display", 50, false));
    }

    [Theory]
    [InlineData(null)]
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
}