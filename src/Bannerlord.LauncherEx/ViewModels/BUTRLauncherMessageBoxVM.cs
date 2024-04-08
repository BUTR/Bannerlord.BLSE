using Bannerlord.LauncherEx.Helpers;
using Bannerlord.LauncherManager.Localization;

using System;

namespace Bannerlord.LauncherEx.ViewModels;

internal class BUTRLauncherMessageBoxVM : BUTRViewModel
{
    [BUTRDataSourceProperty]
    public string Title { get => _title; set => SetField(ref _title, value); }
    private string _title = string.Empty;

    [BUTRDataSourceProperty]
    public string Description { get => _description; set => SetField(ref _description, value); }
    private string _description = string.Empty;

    [BUTRDataSourceProperty]
    public bool IsEnabled { get => _isEnabled; set => SetField(ref _isEnabled, value); }
    private bool _isEnabled;

    [BUTRDataSourceProperty]
    public string CancelText => new BUTRTextObject("{=DzJmcvsP}Cancel").ToString();
    [BUTRDataSourceProperty]
    public string ConfirmText => new BUTRTextObject("{=epTxGUqT}Confirm").ToString();


    private Action? _onConfirm;
    private Action? _onCancel;

    public void Show(string title, string description, Action? onConfirm, Action? onCancel)
    {
            if (IsEnabled) return;

            Title = title;
            Description = description;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            IsEnabled = true;
        }

    private void ExecuteConfirm()
    {
            _onConfirm?.Invoke();
            IsEnabled = false;
        }

    private void ExecuteCancel()
    {
            _onCancel?.Invoke();
            IsEnabled = false;
        }
}