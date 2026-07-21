// LuaCorp - This file is licensed under AGPLv3
// Copyright (c) 2026 LuaCorp Contributors
// See AGPLv3.txt for details.

using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Map;
using Robust.Shared.Utility;
using Robust.Server.GameObjects;
using Content.Shared._Lua.Surveillance.UI;
using Content.Shared._Lua.Surveillance.Events;
using Content.Shared._Lua.Surveillance.Components;
using Content.Server.SurveillanceCamera;
using Content.Server.Popups;

namespace Content.Server._Lua.Surveillance.Systems;

public sealed class SurveillanceMonitorSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SurveillanceCameraSystem _camera = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SurveillanceMonitorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SurveillanceMonitorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SurveillanceMonitorComponent, SurveillanceCameraDeactivateEvent>(OnCameraDeactivate);

        Subs.BuiEvents<SurveillanceMonitorComponent>(SurveillanceMonitorUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpenInterface);
            subs.Event<BoundUIClosedEvent>(OnCloseInterface);
            subs.Event<SurveillanceMonitorGridSelectMessage>(OnGridSelectMessage);
            subs.Event<SurveillanceMonitorCameraSelectMessage>(OnCameraSelectMessage);
            subs.Event<SurveillanceMonitorUpdateGridListMessage>(OnUpdateGridListMessage);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var monitorQuery = EntityQueryEnumerator<SurveillanceMonitorComponent>();

        while (monitorQuery.MoveNext(out var monitor, out var monitorComp))
        {
            if (monitorComp.ConnectingCamera == null)
            {
                continue;
            }

            if (monitorComp.ConnectingTimer > 0f)
            {
                monitorComp.ConnectingTimer -= frameTime;
                continue;
            }

            ConnectToCamera(monitor, monitorComp);
            monitorComp.ConnectingCamera = null;
            UpdateUserInterface(monitor, monitorComp);
        }
    }

    private void ConnectToCamera(EntityUid monitor, SurveillanceMonitorComponent monitorComp)
    {
        var connectingCamera = monitorComp.ConnectingCamera;

        if (!TryComp<SurveillanceCameraComponent>(connectingCamera, out var cameraComp)
            || !cameraComp.Active)
        {
            _popup.PopupEntity(Loc.GetString("surveillance-monitor-invalid-camera"), monitor);
            return;
        }

        var activeRouter = monitorComp.ActiveRouter;

        if (!TryComp<SurveillanceCameraRouterComponent>(activeRouter, out var router)
            || !router.Active)
        {
            _popup.PopupEntity(Loc.GetString("surveillance-monitor-invalid-router"), monitor);
            return;
        }

        if (_transform.GetGrid(connectingCamera.Value) != _transform.GetGrid(activeRouter.Value))
        {
            _popup.PopupEntity(Loc.GetString("surveillance-monitor-router-unauthorised"), monitor);
            return;
        }

        _camera.AddActiveViewers(connectingCamera.Value, monitorComp.Viewers, monitor);
        monitorComp.ActiveCamera = connectingCamera;
    }

    private void OnInit(EntityUid monitor, SurveillanceMonitorComponent monitorComp, ComponentInit args)
    {
        if (monitorComp.ActiveGrid == null
            && TryGetAvailableGrids(monitor, monitorComp, out var grids))
        {
            ResetActiveGrid(monitorComp, grids);
        }
    }

    private void OnShutdown(EntityUid monitor, SurveillanceMonitorComponent monitorComp, ComponentShutdown args)
    {
        RemoveActiveCamera(monitor, monitorComp);
    }

    private void OnCameraDeactivate(EntityUid monitor, SurveillanceMonitorComponent monitorComp, SurveillanceCameraDeactivateEvent args)
    {
        if (args.Camera == monitorComp.ActiveCamera)
        {
            RemoveActiveCamera(monitor, monitorComp);
        }

        UpdateUserInterface(monitor, monitorComp);
    }

    private void OnOpenInterface(EntityUid monitor, SurveillanceMonitorComponent monitorComp, BoundUIOpenedEvent args)
    {
        monitorComp.Viewers.Add(args.Actor);

        if (monitorComp.ActiveCamera != null)
        {
            _camera.AddActiveViewer(monitorComp.ActiveCamera.Value, args.Actor, monitor);
        }

        UpdateUserInterface(monitor, monitorComp);
    }

    private void OnCloseInterface(EntityUid monitor, SurveillanceMonitorComponent monitorComp, BoundUIClosedEvent args)
    {
        monitorComp.Viewers.Remove(args.Actor);

        if (monitorComp.ActiveCamera != null)
        {
            _camera.RemoveActiveViewer(monitorComp.ActiveCamera.Value, args.Actor, monitor);
        }

        if (monitorComp.Viewers.Count == 0)
        {
            monitorComp.ActiveCamera = null;
            return;
        }

        UpdateUserInterface(monitor, monitorComp);
    }

    private void OnUpdateGridListMessage(EntityUid monitor, SurveillanceMonitorComponent monitorComp, SurveillanceMonitorUpdateGridListMessage args)
    {
        UpdateUserInterface(monitor, monitorComp);
    }

    private void OnGridSelectMessage(EntityUid monitor, SurveillanceMonitorComponent monitorComp, SurveillanceMonitorGridSelectMessage args)
    {
        if (!monitorComp.MultiGrid)
        {
            UpdateUserInterface(monitor, monitorComp);
            return;
        }

        if (!TryGetEntity(args.Grid, out var newGrid))
        {
            return;
        }

        if (TryFindRouterOnGrid(newGrid.Value, out var router))
        {
            monitorComp.ActiveGrid = newGrid;
            monitorComp.ActiveRouter = router;
        }

        UpdateUserInterface(monitor, monitorComp);
    }

    private void OnCameraSelectMessage(EntityUid monitor, SurveillanceMonitorComponent monitorComp, SurveillanceMonitorCameraSelectMessage args)
    {
        if (!TryGetEntity(args.Camera, out var newCamera))
        {
            return;
        }

        RemoveActiveCamera(monitor, monitorComp);
        monitorComp.ConnectingCamera = newCamera;
        monitorComp.ConnectingTimer = monitorComp.ConnectingTime;
        UpdateUserInterface(monitor, monitorComp);
    }

    private void RemoveActiveCamera(EntityUid monitor, SurveillanceMonitorComponent monitorComp)
    {
        if (monitorComp.ActiveCamera != null)
        {
            _camera.RemoveActiveViewers(monitorComp.ActiveCamera.Value, monitorComp.Viewers, monitor);
            monitorComp.ActiveCamera = null;
        }
    }

    private void UpdateUserInterface(EntityUid monitor, SurveillanceMonitorComponent? monitorComp = null)
    {
        if (!Resolve(monitor, ref monitorComp)
            || !_userInterface.HasUi(monitor, SurveillanceMonitorUiKey.Key))
        {
            return;
        }

        if (!TryGetAvailableGrids(monitor, monitorComp, out var availableGrids))
        {
            RemoveActiveCamera(monitor, monitorComp);
            _userInterface.CloseUi(monitor, SurveillanceMonitorUiKey.Key);
            _popup.PopupEntity(Loc.GetString("surveillance-monitor-no-routers"), monitor);
            monitorComp.Viewers.Clear();
            return;
        }

        if (!TryGetNetEntity(monitorComp.ActiveGrid, out var netGrid)
            || !availableGrids.Contains(netGrid.Value))
        {
            _popup.PopupEntity(Loc.GetString("surveillance-monitor-router-malfunction"), monitor);
            ResetActiveGrid(monitorComp, availableGrids);
        }

        var state = new SurveillanceMonitorWindowInterfaceState
        (
            GetCamerasOnGrid(monitorComp.ActiveGrid),
            availableGrids,
            GetNetEntity(monitorComp.ActiveGrid),
            GetNetEntity(monitorComp.ActiveCamera),
            monitorComp.ConnectingTimer > 0f,
            monitorComp.MultiGrid
        );
        _userInterface.SetUiState(monitor, SurveillanceMonitorUiKey.Key, state);
    }

    private void ResetActiveGrid(SurveillanceMonitorComponent monitorComp, HashSet<NetEntity> availableGrids)
    {
        if (!availableGrids.TryFirstOrNull(out var first)
            || !TryGetEntity(first, out var grid)
            || !TryFindRouterOnGrid(grid.Value, out var router))
        {
            monitorComp.ActiveGrid = null;
            monitorComp.ActiveRouter = null;
            return;
        }

        monitorComp.ActiveGrid = grid;
        monitorComp.ActiveRouter = router;
    }

    private bool TryFindRouterOnGrid(EntityUid grid, out EntityUid? router)
    {
        var routersQuery = EntityQueryEnumerator<SurveillanceCameraRouterComponent, TransformComponent>();

        while (routersQuery.MoveNext(out var routerUid, out var routerComp, out var routerTransform))
        {
            if (routerTransform.GridUid == grid && routerComp.Active)
            {
                router = routerUid;
                return true;
            }
        }

        router = null;
        return false;
    }

    private bool TryGetAvailableGrids(EntityUid monitor, SurveillanceMonitorComponent monitorComp, [NotNullWhen(true)] out HashSet<NetEntity>? grids)
    {
        if (!monitorComp.MultiGrid)
        {
            var monitorGrid = _transform.GetGrid(monitor);

            if (monitorGrid == null
                || !TryGetNetEntity(monitorGrid, out var netMonitorGrid)
                || !TryFindRouterOnGrid(monitorGrid.Value, out _))
            {
                grids = null;
                return false;
            }

            grids = [netMonitorGrid.Value];
            return true;
        }

        grids = new();

        var routersQuery = EntityQueryEnumerator<SurveillanceCameraRouterComponent, TransformComponent>();

        while (routersQuery.MoveNext(out var routerComp, out var routerTransform))
        {
            var routerGrid = routerTransform.GridUid;

            if (routerGrid == null || !routerComp.Active)
            {
                continue;
            }

            if (TryGetNetEntity(routerGrid.Value, out var netRouterGrid))
            {
                grids.Add(netRouterGrid.Value);
            }
        }

        return grids.Count > 0;
    }

    private List<(NetEntity, NetCoordinates)>? GetCamerasOnGrid(EntityUid? grid)
    {
        if (!Exists(grid))
        {
            return null;
        }

        var cameras = new List<(NetEntity, NetCoordinates)>();

        var cameraQuery = EntityQueryEnumerator<SurveillanceCameraComponent, TransformComponent>();

        while (cameraQuery.MoveNext(out var cameraUid, out var cameraComp, out var cameraTransform))
        {
            if (cameraTransform.GridUid != grid || !cameraComp.Active)
            {
                continue;
            }

            var netCameraUid = GetNetEntity(cameraUid);
            var netCoordinates = GetNetCoordinates(cameraTransform.Coordinates);
            cameras.Add((netCameraUid, netCoordinates));
        }

        return cameras;
    }
}
