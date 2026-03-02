---
name: dotnet-performances
description: Performance optimization patterns. Covers memory management, and optimization strategies.
---

# Performance

## Overview

A performant desktop application starts quickly, responds immediately to user input, and uses resources efficiently. This skill covers performance patterns for WPF applications.

## Definition of Done (DoD)

- [ ] Memory usage stays bounded during normal use
- [ ] No memory leaks after extended use
- [ ] Performance-critical paths are profiled and optimized

## Memory Management

### Memory Monitoring

```csharp
public class MemoryMonitor
{
    public static MemoryMonitor Instance { get; } = new();
    
    private readonly Dictionary<string, long> _snapshots = new();
    
    public void TakeSnapshot(string name)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var memory = GC.GetTotalMemory(forceFullCollection: false);
        _snapshots[name] = memory;
        
        Log.Debug("Memory snapshot {Name}: {MemoryMB:F2}MB", 
            name, memory / 1024.0 / 1024.0);
    }
    
    public void CheckForLeaks()
    {
        if (_snapshots.TryGetValue("AppStarted", out var startMemory) &&
            _snapshots.TryGetValue("AppShutdown", out var endMemory))
        {
            var growthMb = (endMemory - startMemory) / 1024.0 / 1024.0;
            
            if (growthMb > 100)  // More than 100MB growth
            {
                Log.Warning(
                    "Potential memory leak: Memory grew by {GrowthMB:F2}MB during session",
                    growthMb);
            }
        }
    }
    
    public MemoryInfo GetCurrentInfo()
    {
        var process = Process.GetCurrentProcess();
        
        return new MemoryInfo
        {
            ManagedMemoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0,
            WorkingSetMB = process.WorkingSet64 / 1024.0 / 1024.0,
            PrivateMemoryMB = process.PrivateMemorySize64 / 1024.0 / 1024.0,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2)
        };
    }
}

public record MemoryInfo
{
    public double ManagedMemoryMB { get; init; }
    public double WorkingSetMB { get; init; }
    public double PrivateMemoryMB { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
}
```

### Avoiding Memory Leaks

```csharp
// ❌ BAD - Event handler leak
public class WidgetViewModel
{
    public WidgetViewModel(IEventAggregator events)
    {
        events.ThemeChanged += OnThemeChanged;  // Never unsubscribed!
    }
}

// ✅ GOOD - Weak event or explicit unsubscription
public class WidgetViewModel : IDisposable
{
    private readonly IEventAggregator _events;
    
    public WidgetViewModel(IEventAggregator events)
    {
        _events = events;
        _events.ThemeChanged += OnThemeChanged;
    }
    
    public void Dispose()
    {
        _events.ThemeChanged -= OnThemeChanged;
    }
}

// ✅ BETTER - Use WeakEventManager
public class WidgetViewModel
{
    public WidgetViewModel(IEventAggregator events)
    {
        WeakEventManager<IEventAggregator, EventArgs>.AddHandler(
            events, nameof(events.ThemeChanged), OnThemeChanged);
    }
}
```

### Dispose Pattern

```csharp
public class WidgetWindow : Window, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly HttpClient _httpClient;
    private bool _disposed;
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            _timer.Stop();
            _httpClient.Dispose();
        }
        
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }
}
```

## Anti-Patterns

| Anti-Pattern | Impact | Solution |
|--------------|--------|----------|
| Sync over async | UI freeze | Use async/await properly |
| Large object allocations | GC pressure | Pool or reuse objects |
| Unbounded collections | Memory growth | Use pagination/windowing |

## References

- [.NET Performance Tips](https://docs.microsoft.com/en-us/dotnet/framework/performance/)
- [Memory Management](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)
