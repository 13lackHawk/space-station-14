using Content.Shared.Damage;

namespace Content.Shared.Implants.Components;

[RegisterComponent]
public sealed partial class LoyaltyImplantComponent : Component
{
    [DataField("damage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = default!;

    [DataField("heal", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Heal = default!;

    [DataField("durability")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Durability = 5;

    [ViewVariables] public TimeSpan NextImpact = TimeSpan.Zero;
    public TimeSpan ImpactDelay = TimeSpan.FromSeconds(3);
}
