using Bannerlord.LauncherEx.Extensions;

using System;
using System.Collections.Generic;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Bannerlord.LauncherEx.Widgets;

internal sealed class LauncherToggleButtonWidget : ImageWidget
{
    private enum ButtonClickState
    {
        None,
        HandlingClick,
        HandlingAlternateClick
    }

    //[Editor]
    public Widget? ToggleIndicator
    {
        get => _toggleIndicator;
        set
        {
            if (_toggleIndicator != value)
            {
                _toggleIndicator = value;
                Refresh();
            }
        }
    }
    private Widget? _toggleIndicator;

    //[Editor]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                Refresh();
                RefreshState();
                OnPropertyChanged(value);
            }
        }
    }
    private bool _isSelected;

    //[Editor]
    public bool DominantSelectedState { get => _dominantSelectedState; set => this.SetField(ref _dominantSelectedState, value); }
    private bool _dominantSelectedState = true;

    //[Editor]
    public bool ManualToggle { get => _manualToggle; set => this.SetField(ref _manualToggle, value); }
    private bool _manualToggle;

    private const float _maxDoubleClickDeltaTimeInSeconds = 0.5f;
    private float _lastClickTime;

    private ButtonClickState _clickState;

    public List<Action<Widget>> ClickEventHandlers = new();

    public LauncherToggleButtonWidget(UIContext context) : base(context)
    {
        FrictionEnabled = true;
    }

    protected override bool OnPreviewMousePressed()
    {
        base.OnPreviewMousePressed();
        return true;
    }

    protected override void RefreshState()
    {
        base.RefreshState();
        if (!OverrideDefaultStateSwitchingEnabled)
        {
            if (IsDisabled)
            {
                SetState("Disabled");
            }
            else if (IsSelected && DominantSelectedState)
            {
                SetState("Selected");
            }
            else if (IsPressed)
            {
                SetState("Pressed");
            }
            else if (IsHovered)
            {
                SetState("Hovered");
            }
            else if (IsSelected && !DominantSelectedState)
            {
                SetState("Selected");
            }
            else
            {
                SetState("Default");
            }
        }

        if (UpdateChildrenStates)
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChild(i);
                if (child is not ImageWidget { OverrideDefaultStateSwitchingEnabled: true })
                {
                    child.SetState(CurrentState);
                }
            }
        }
    }

    private void Refresh() => ShowHideToggle();

    private void ShowHideToggle()
    {
        if (ToggleIndicator == null) return;

        if (_isSelected)
        {
            ToggleIndicator.Show();
            return;
        }

        ToggleIndicator.Hide();
    }

    protected override void OnMousePressed()
    {
        if (_clickState != ButtonClickState.None) return;

        _clickState = ButtonClickState.HandlingClick;
        this.SetIsPressed(true);
        if (!DoNotPassEventsToChildren)
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChild(i);
                child?.SetIsPressed(true);
            }
        }
    }

    protected override void OnMouseReleased()
    {
        if (_clickState != ButtonClickState.HandlingClick) return;

        _clickState = ButtonClickState.None;
        this.SetIsPressed(false);
        if (!DoNotPassEventsToChildren)
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChild(i);
                child?.SetIsPressed(false);
            }
        }

        if (IsPointInsideMeasuredAreaAndCheckIfVisible())
        {
            HandleClick();
        }
    }

    private bool IsPointInsideMeasuredAreaAndCheckIfVisible() => this.IsPointInsideMeasuredArea() && IsRecursivelyVisible();

    protected override void OnMouseAlternatePressed()
    {
        if (_clickState != ButtonClickState.None) return;

        _clickState = ButtonClickState.HandlingAlternateClick;
        this.SetIsPressed(true);
        if (!DoNotPassEventsToChildren)
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChild(i);
                child?.SetIsPressed(true);
            }
        }
    }

    protected override void OnMouseAlternateReleased()
    {
        if (_clickState != ButtonClickState.HandlingAlternateClick) return;

        _clickState = ButtonClickState.None;
        this.SetIsPressed(false);
        if (!DoNotPassEventsToChildren)
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChild(i);
                child?.SetIsPressed(false);
            }
        }

        if (IsPointInsideMeasuredAreaAndCheckIfVisible())
        {
            HandleAlternateClick();
        }
    }

    private void HandleClick()
    {
        foreach (var action in ClickEventHandlers)
        {
            action(this);
        }

        if (!ManualToggle)
        {
            IsSelected = !IsSelected;
        }

        OnClick();
        EventFired("Click", Array.Empty<object>());
        if (Context.EventManager.Time - _lastClickTime < _maxDoubleClickDeltaTimeInSeconds)
        {
            EventFired("DoubleClick", Array.Empty<object>());
            return;
        }

        _lastClickTime = Context.EventManager.Time;
    }

    private void HandleAlternateClick()
    {
        OnAlternateClick();
        EventFired("AlternateClick", Array.Empty<object>());
    }

    private void OnClick() { }
    private void OnAlternateClick() { }
}