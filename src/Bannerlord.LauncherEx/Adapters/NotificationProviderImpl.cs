using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherEx.Helpers.Input;
using Bannerlord.LauncherManager.External.UI;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class NotificationProviderImpl : INotificationProvider
{
    public static readonly NotificationProviderImpl Instance = new();

    private readonly ConcurrentDictionary<string, object?> _ids = new();

    public Task SendNotificationAsync(string id, NotificationType type, string message, uint displayMs)
    {
        if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();

        // Prevents message spam
        if (!_ids.TryAdd(id, null)) return Task.CompletedTask;
        using var cts = new CancellationTokenSource();
        _ = Task.Delay(TimeSpan.FromMilliseconds(displayMs), cts.Token).ContinueWith(x => _ids.TryRemove(id, out _), CancellationToken.None);

        var translatedMessage = new BUTRTextObject(message).ToString();
        switch (type)
        {
            case NotificationType.Hint:
            {
                HintManager.ShowHint(translatedMessage);
                break;
            }
            case NotificationType.Info:
            {
                // TODO:
                HintManager.ShowHint(translatedMessage);
                break;
            }
            default:
                MessageBoxDialog.Show(translatedMessage, "Information", MessageBoxButtons.Ok);
                break;
        }

        cts.Cancel();
        return Task.CompletedTask;
    }
}