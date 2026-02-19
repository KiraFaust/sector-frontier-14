using Content.Shared.BluespacePallet;
using Content.Shared.DeviceLinking.Events;
using Content.Server.Chat.Systems;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.BluespacePallet;

public sealed class BluespacePalletTeleporterSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;



    public override void Initialize()
    {
        SubscribeLocalEvent<BluespacePalletTeleporterComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<BluespacePalletTeleporterComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, BluespacePalletTeleporterComponent component, ComponentInit args)
    {
        // если нужно — можно обеспечить порт, но это опционально
    }

    private void OnSignalReceived(
        EntityUid uid,
        BluespacePalletTeleporterComponent comp,
        ref SignalReceivedEvent args)
    {
        TryTeleport(uid, comp);
    }

private void TryTeleport(EntityUid pallet, BluespacePalletTeleporterComponent comp)
{
    var targets = new List<EntityUid>();

    foreach (var entity in EntityManager.GetEntities())
    {
        if (Prototype(entity)?.ID == comp.TargetPalletPrototype)
            targets.Add(entity);
    }

    if (targets.Count == 0)
    {
        Logger.WarningS("bluespace",
            $"Логистическая цепь не настроена для паллеты {pallet}");
        return;
    }

    var target = _random.Pick(targets);
    var targetCoords = Transform(target).Coordinates;

    foreach (var entity in _lookup.GetEntitiesIntersecting(pallet))
    {
        if (entity == pallet)
            continue;

        if (!PassesWhitelist(entity, comp))
            continue;

        SpawnEffectAndTeleport(entity, targetCoords, comp);
        break;
    }
}

private bool PassesWhitelist(EntityUid entity, BluespacePalletTeleporterComponent comp)
{
    if (comp.WhitelistComponents.Count == 0)
        return true;

    foreach (var compName in comp.WhitelistComponents)
    {
        if (!_compFactory.TryGetRegistration(compName, out var registration))
            continue;

        if (_entManager.HasComponent(entity, registration.Type))
            return true;
    }

    return false;
}

private void SpawnEffectAndTeleport(
    EntityUid entity,
    EntityCoordinates targetCoords,
    BluespacePalletTeleporterComponent comp)
{
    if (comp.EffectPrototype == null)
    {
        _transform.SetCoordinates(entity, targetCoords);
        return;
    }

    var effect = Spawn(comp.EffectPrototype, targetCoords);

    if (!comp.WaitForEffect)
    {
        _transform.SetCoordinates(entity, targetCoords);
        return;
    }

    if (!TryComp<TimedDespawnComponent>(effect, out var timed))
    {
        _transform.SetCoordinates(entity, targetCoords);
        return;
    }

    Timer.Spawn(TimeSpan.FromSeconds(timed.Lifetime), () =>
    {
        if (!Deleted(entity))
            _transform.SetCoordinates(entity, targetCoords);
    });
}

private void SpawnTimer(EntityUid entity, EntityCoordinates coords, TimeSpan endTime)
{
    _timing.RunAt(endTime, () =>
    {
        if (Deleted(entity))
            return;

        _transform.SetCoordinates(entity, coords);
    });
}
}
