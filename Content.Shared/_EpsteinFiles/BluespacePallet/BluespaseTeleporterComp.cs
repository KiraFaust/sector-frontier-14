using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.BluespacePallet;

[RegisterComponent]
public sealed partial class BluespacePalletTeleporterComponent : Component
{
    [DataField]
    public string TargetPalletPrototype = "CargoPalletBuy";

    // Whitelist компонентов
    [DataField]
    public HashSet<string> WhitelistComponents = new();

    // Прототип эффекта
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? EffectPrototype = "EffectFlashBluespace";

    // Ждать ли завершения эффекта
    [DataField]
    public bool WaitForEffect = false;
}
