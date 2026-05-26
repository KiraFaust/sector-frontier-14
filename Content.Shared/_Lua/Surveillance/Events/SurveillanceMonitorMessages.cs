// LuaCorp - This file is licensed under AGPLv3
// Copyright (c) 2026 LuaCorp Contributors
// See AGPLv3.txt for details.

using Robust.Shared.Serialization;

namespace Content.Shared._Lua.Surveillance.Events;

[Serializable, NetSerializable]
public sealed class SurveillanceMonitorUpdateGridListMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class SurveillanceMonitorGridSelectMessage(NetEntity? grid) : BoundUserInterfaceMessage
{
    public NetEntity? Grid = grid;
}

[Serializable, NetSerializable]
public sealed class SurveillanceMonitorCameraSelectMessage(NetEntity? camera) : BoundUserInterfaceMessage
{
    public NetEntity? Camera = camera;
}

