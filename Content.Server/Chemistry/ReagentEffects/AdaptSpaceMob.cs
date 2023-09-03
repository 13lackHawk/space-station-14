// RIP Homilin 20.06.23?
using Content.Server.Ghost.Roles.Components;
using Content.Server.Salvage;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects;

[UsedImplicitly]
public sealed partial class AdaptSpaceMob : ReagentEffect
{
    // cringe, fix later
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-drunk", ("chance", Probability));

    public override void Effect(ReagentEffectArgs args)
    {
        if (args.EntityManager.HasComponent<SalvageMobRestrictionsComponent>(args.SolutionEntity))
        {
            args.EntityManager.RemoveComponent<SalvageMobRestrictionsComponent>(args.SolutionEntity);
            args.EntityManager.RemoveComponent<GhostTakeoverAvailableComponent>(args.SolutionEntity);
        }
    }
}
