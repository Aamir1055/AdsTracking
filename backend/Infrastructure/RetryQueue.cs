using System.Threading.Channels;

namespace AdsTracking.Api.Infrastructure;

public enum RetryItemType
{
    DownloadEvent
}

public class RetryItem
{
    public RetryItemType Type { get; set; }
    public object Payload { get; set; } = null!;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(20);
}

public class RetryQueue
{
    private readonly Channel<RetryItem> _channel = Channel.CreateUnbounded<RetryItem>();

    public ChannelReader<RetryItem> Reader => _channel.Reader;

    public void Enqueue(RetryItem item)
    {
        _channel.Writer.TryWrite(item);
    }
}
