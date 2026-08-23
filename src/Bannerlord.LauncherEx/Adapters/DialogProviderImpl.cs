using Bannerlord.LauncherEx.Helpers.Input;
using Bannerlord.LauncherManager.External.UI;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class DialogProviderImpl : IDialogProvider
{
    public static readonly DialogProviderImpl Instance = new();

    public Task<string> SendDialogAsync(DialogType type, string title, string message, IReadOnlyList<DialogFileFilter> filters)
    {
        switch (type)
        {
            case DialogType.Warning:
            {
                var split = message.Split(new[] { "--CONTENT-SPLIT--" }, StringSplitOptions.RemoveEmptyEntries);
                var result = MessageBoxDialog.Show(
                    string.Join("\n", split),
                    new BUTRTextObject(title).ToString(),
                    MessageBoxButtons.OkCancel,
                    MessageBoxIcon.Warning,
                    0,
                    0
                );
                return Task.FromResult(result == MessageBoxResult.Ok ? "true" : "false");
            }
            case DialogType.FileOpen:
            {
                var filter = string.Join("|", filters.Select(x => $"{x.Name} {string.Join((string) ", ", x.Extensions)}|{string.Join((string) ", ", x.Extensions)}"));
                var dialog = new OpenFileDialog
                {
                    Title = title,
                    Filter = filter,

                    CheckFileExists = true,
                    CheckPathExists = true,
                    ReadOnlyChecked = true,
                    Multiselect = false,
                    ValidateNames = true,
                };
                return Task.FromResult(dialog.ShowDialog() ? dialog.FileName ?? string.Empty : string.Empty);
            }
            case DialogType.FileSave:
            {
                var fileName = message;
                var filter = string.Join("|", filters.Select(x => $"{x.Name} {string.Join(", ", x.Extensions)}|{string.Join(", ", x.Extensions)}"));
                var dialog = new SaveFileDialog
                {
                    Title = title,
                    Filter = filter,
                    FileName = fileName,

                    CheckPathExists = false,

                    ValidateNames = true,
                };
                return Task.FromResult(dialog.ShowDialog() ? dialog.FileName : string.Empty);
            }
        }
        return Task.FromResult(string.Empty);
    }
}
