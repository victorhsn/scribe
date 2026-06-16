using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Scribe.Notifications.Core.Notifications;

BenchmarkRunner.Run<NotificationCollectionBenchmarks>();

[MemoryDiagnoser]
[ShortRunJob]
public class NotificationCollectionBenchmarks
{
    private NotificationCollection _notificationCollection = null;
    private NotificationCollection _emptyCollection = null;

    [GlobalSetup]
    public void Setup()
    {
        _notificationCollection = new NotificationCollection();
        for (var i = 0; i < 1000; i++)
        {
            _notificationCollection.Add(new NotificationMessage($"ID_{i:D4}", NotificationType.Error, $"Error {i}"));
            _notificationCollection.Add(new NotificationMessage($"WARN_{i:D4}", NotificationType.Error, $"Warning {i}"));
        }
        _emptyCollection = new NotificationCollection();
    }

    [Benchmark(Description = "Add with explicit ID")]
    public void Add_WithExplicitId()
    {
        var c = new NotificationCollection();
        c.Add(new NotificationMessage("BENCH_01", NotificationType.Error, "Benchmark error"));
    }

    [Benchmark(Description = "Add without ID (auto-generated)")]
    public void Add_WithoutId()
    {
        var c = new NotificationCollection();
        c.Add(NotificationType.Error, "Benchmark error");
    }

    [Benchmark(Description = "HasErrors on 2000 items")]
    public bool HasErrors_2000Items()
    {
        return _notificationCollection.HasErrors();
    }

    [Benchmark(Description = "HasErrors on empty collection")]
    public bool HasErrors_Empty()
    {
        return _emptyCollection.HasErrors();
    }

    [Benchmark(Description = "GetErrors lazy (1000 errors)")]
    public int GetErrors_Lazy()
    {
        var count = 0;
        foreach (var _ in _notificationCollection.GetErrors())
            count++;
        return count;
    }

    [Benchmark(Description = "GetErrorsAsList materialized (1000 errors)")]
    public int GetErrors_Materialized()
    {
        var list = _notificationCollection.GetErrorsAsList();
        return list.Count;
    }

    [Benchmark(Description = "TryGetAsSpan zero-copy")]
    public int TryGetAsSpan_ZeroCopy()
    {
        if (_notificationCollection.TryGetAsSpan(out var span))
            return span.Length;
        return 0;
    }

    [Benchmark(Description = "AddRange 100 items")]
    public void AddRange_100Items()
    {
        var c = new NotificationCollection();
        var items = new NotificationMessage[100];
        for (var i = 0; i < 100; i++)
            items[i] = new NotificationMessage(NotificationType.Error, $"Error {i}");
        c.AddRange(items);
    }
}