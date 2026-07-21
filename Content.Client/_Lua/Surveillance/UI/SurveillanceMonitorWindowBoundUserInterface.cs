// LuaCorp - This file is licensed under AGPLv3
// Copyright (c) 2026 LuaCorp Contributors
// See AGPLv3.txt for details.

using Robust.Client.UserInterface;
using Content.Shared._Lua.Surveillance.UI;
using Content.Shared._Lua.Surveillance.Events;

namespace Content.Client._Lua.Surveillance.UI;

public sealed class SurveillanceMonitorWindowBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SurveillanceMonitorWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SurveillanceMonitorWindow>();

        _window.CameraSelected += OnCameraSelected;
        _window.GridSelected += OnGridSelected;
        _window.UpdateGridList += OnGridRefresh;
    }

    private void OnCameraSelected(NetEntity? camera)
    {
        SendMessage(new SurveillanceMonitorCameraSelectMessage(camera));
    }

    private void OnGridSelected(NetEntity? grid)
    {
        SendMessage(new SurveillanceMonitorGridSelectMessage(grid));
    }

    private void OnGridRefresh()
    {
        SendMessage(new SurveillanceMonitorUpdateGridListMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_window == null || state is not SurveillanceMonitorWindowInterfaceState cast)
        {
            return;
        }

        _window.UpdateState(cast);
    }
}
