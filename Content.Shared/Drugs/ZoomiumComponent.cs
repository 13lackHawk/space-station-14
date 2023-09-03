namespace Content.Shared.Drugs;

[RegisterComponent]
public sealed partial class ZoomiumComponent : Component
{
    [ViewVariables] public TimeSpan NextUpdate = TimeSpan.Zero;
    public TimeSpan UpdateDelay = TimeSpan.FromMilliseconds(900);
    public TimeSpan NextSmallUpdate = TimeSpan.Zero;
    public TimeSpan SmallUpdateDelay = TimeSpan.FromMilliseconds(100);
    public float NextZoomLevel = 1f;
    public float CurrentZoomLevel = 1f;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("severity")]
    public float Severity = 1f;
}
