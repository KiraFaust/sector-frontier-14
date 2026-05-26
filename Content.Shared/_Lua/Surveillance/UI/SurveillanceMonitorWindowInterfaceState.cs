// LuaCorp - This file is licensed under AGPLv3
// Copyright (c) 2026 LuaCorp Contributors
// See AGPLv3.txt for details.

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Lua.Surveillance.UI;

[Serializable, NetSerializable]
public sealed class SurveillanceMonitorWindowInterfaceState(
    List<(NetEntity, NetCoordinates)>? cameras,
    HashSet<NetEntity> grids,
    NetEntity? activeGrid,
    NetEntity? activeCamera,
    bool connecting,
    bool multiGrid
    ) : BoundUserInterfaceState
{
    public List<(NetEntity, NetCoordinates)>? Cameras = cameras;
    public HashSet<NetEntity> Grids = grids;
    public NetEntity? ActiveGrid = activeGrid;
    public NetEntity? ActiveCamera = activeCamera;
    public bool Connecting = connecting;
    public bool MultiGrid = multiGrid;
}
