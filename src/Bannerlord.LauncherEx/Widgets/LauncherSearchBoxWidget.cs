using Bannerlord.LauncherEx.Extensions;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Bannerlord.LauncherEx.Widgets;

internal sealed class LauncherSearchBoxWidget : BrushWidget
{
    public EditableTextWidget? EditableTextWidget
    {
        get => _editableTextWidget;
        set
        {
            if (this.SetField(ref _editableTextWidget, value))
            {
                if (_editableTextWidget is not null)
                    _editableTextWidget.PropertyChanged -= SearchTextOnPropertyChanged;

                if (value is not null)
                    value.PropertyChanged += SearchTextOnPropertyChanged;
            }
        }
    }

    private void SearchTextOnPropertyChanged(PropertyOwnerObject propertyOwnerObject, string propertyName, object value)
    {
        if (propertyName == nameof(TaleWorlds.GauntletUI.BaseTypes.EditableTextWidget.RealText) && value is string searchText)
            SearchText = searchText;
    }

    private EditableTextWidget? _editableTextWidget;

    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (this.SetField(ref _searchText, value) && EditableTextWidget is not null)
            {
                EditableTextWidget.RealText = value;
            }
        }
    }
    private string? _searchText;

    public LauncherSearchBoxWidget(UIContext context) : base(context) { }
}