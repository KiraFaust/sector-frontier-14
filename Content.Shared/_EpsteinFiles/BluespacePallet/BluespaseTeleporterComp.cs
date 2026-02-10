namespace Content.Shared.BluespacePallet;

[RegisterComponent]
public sealed partial class BluespacePalletTeleporterComponent : Component
{
    [DataField]
    public string TargetPalletPrototype = "CargoPalletBuy";
}
