// LuaCorp - This file is licensed under AGPLv3
// Copyright (c) 2026 LuaCorp Contributors
// See AGPLv3.txt for details.

namespace Content.Shared._Lua.Surveillance.Components;

[RegisterComponent]
public sealed partial class SurveillanceMonitorComponent : Component
{
    [ViewVariables]
    public EntityUid? ActiveCamera;

    [ViewVariables]
    public EntityUid? ActiveRouter;

    [ViewVariables]
    public EntityUid? ActiveGrid;

    [ViewVariables]
    public HashSet<EntityUid> Viewers = new();

    [ViewVariables]
    public EntityUid? ConnectingCamera;

    [ViewVariables]
    public float ConnectingTimer;

    [DataField]
    public float ConnectingTime = 2.0f; // Seconds

    [DataField]
    public bool MultiGrid = false;
}
