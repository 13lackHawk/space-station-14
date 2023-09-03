using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Swab;

[Serializable, NetSerializable]
public sealed partial class BotanySwabDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class MedicalSwabDoAfterEvent : DoAfterEvent
{
    [DataField("solution", required: true)]
    public Solution Solution = default!;

    private MedicalSwabDoAfterEvent()
    {
    }

    public MedicalSwabDoAfterEvent(Solution solution)
    {
        Solution = solution;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
