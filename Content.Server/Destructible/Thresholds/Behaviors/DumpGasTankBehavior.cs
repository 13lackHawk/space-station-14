using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Destructible.Thresholds.Behaviors;

[Serializable]
[DataDefinition]
public sealed partial class DumpGasTankBehavior : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        system.EntityManager.EntitySysManager.GetEntitySystem<GasTankSystem>().PurgeContents(owner);
    }
}
