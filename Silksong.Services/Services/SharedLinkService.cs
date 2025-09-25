using System;
using System.Diagnostics;
namespace Silksong.Services;

public class SharedLinkService : ISharedLinkService
{
    private string? _pendingLink;
    public event Action<string>? LinkReceived;

    public void ReceiveSharedLink(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        Debug.WriteLine($"[SharedLinkService] ReceiveSharedLink called: {url}");

        if (LinkReceived != null)
        {
            LinkReceived.Invoke(url);
        }
        else
        {
            _pendingLink = url;
            Debug.WriteLine($"[SharedLinkService] Link stored in _pendingLink: {url}");
        }
    }
    public string? ConsumePendingLink()
    {
        var link = _pendingLink;
        _pendingLink = null;
        return link;
    }
    public void Subscribe(Action<string> callback)
    {
        LinkReceived += callback;
        if (!string.IsNullOrWhiteSpace(_pendingLink))
        {
            callback.Invoke(_pendingLink!);
            _pendingLink = null;
        }
    }
}

