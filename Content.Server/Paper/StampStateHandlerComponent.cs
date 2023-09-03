namespace Content.Server.Paper;

[RegisterComponent]
public sealed partial class StampStateHandlerComponent : Component
{
    [DataField("stampStateCollection")]
    public string[] StampStateCollection { get; private set; } = { "paper_stamp-generic"};

    [DataField("stampNameCollection")]
    public string[] StampNameCollection { get; private set; } = { "stamp-component-stamped-name-default" };

    public int CurrentStateIndex { get; set; } = 0;
}
