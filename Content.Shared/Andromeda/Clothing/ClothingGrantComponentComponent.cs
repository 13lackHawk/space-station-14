using Robust.Shared.Prototypes;

namespace Content.Shared.Andromeda.Clothing
{
    [RegisterComponent]
    public sealed partial class ClothingGrantComponentComponent : Component
    {
        [DataField("component", required: true)]
        [AlwaysPushInheritance]
        public Robust.Shared.Prototypes.ComponentRegistry Components { get; private set; } = new();

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive = false;
    }
}
