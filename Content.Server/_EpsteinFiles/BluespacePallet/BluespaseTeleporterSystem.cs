using Content.Shared.BluespacePallet;
using Content.Shared.DeviceLinking.Events;
using Content.Server.Chat.Systems;

using Robust.Shared.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.BluespacePallet;

public sealed class BluespacePalletTeleporterSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

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
        Logger.WarningS("bluespace", "Логистическая цепь не настроена для паллеты {0}", pallet);
        return;
    }



    var target = _random.Pick(targets);
    var targetCoords = Transform(target).Coordinates;

    foreach (var entity in _lookup.GetEntitiesIntersecting(pallet))
    {
        if (entity == pallet)
            continue;

        Transform(entity).Coordinates = targetCoords;
        break;
    }
}
}
