using Bannerlord.LauncherEx.Helpers.Input;
using Bannerlord.LauncherManager.External.UI;
using Bannerlord.LauncherManager.Localization;
using Bannerlord.LauncherManager.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.LauncherEx.Adapters;

internal sealed class DialogProviderImpl : IDialogProvider
{
    public static readonly DialogProviderImpl Instance = new();

    public void SendDialog(DialogType type, string title, string message, IReadOnlyList<DialogFileFilter> filters, Action<string> onResult)
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
                onResult(result == MessageBoxResult.Ok ? "true" : "false");
                return;
            }
            case DialogType.FileOpen:
            {
                var filter = string.Join("|", filters.Select(x => $"{x.Name} ({string.Join((string) ", ", (IEnumerable<string>) x.Extensions)}|{string.Join((string) ", ", (IEnumerable<string>) x.Extensions)}"));
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
                onResult(dialog.ShowDialog() ? dialog.FileName ?? string.Empty : string.Empty);
                return;
            }
            case DialogType.FileSave:
            {
                var fileName = message;
                var filter = string.Join("|", filters.Select(x => $"{x.Name} ({string.Join(", ", x.Extensions)}|{string.Join(", ", x.Extensions)}"));
                var dialog = new SaveFileDialog
                {
                    Title = title,
                    Filter = filter,
                    FileName = fileName,

                    CheckPathExists = false,

                    ValidateNames = true,
                };
                onResult(dialog.ShowDialog() ? dialog.FileName : string.Empty);
                return;
            }
        }
    }
}