using Robust.Shared.GameStates;

namespace Content.Shared.Andromeda.Fatigue;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FatigueComponent : Component
{
    [DataField("uid"), AutoNetworkedField]
    public EntityUid Uid;

    [DataField("currentFatigue"), ViewVariables(VVAccess.ReadWrite)]
    public int CurrentFatigue { get; set; }

    [DataField("maxFatigue"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxFatigue { get; set; } = 100;

    [DataField("decreaseIntervalMinutes"), ViewVariables(VVAccess.ReadWrite)]
    public float DecreaseIntervalMinutes = 2f;

    [DataField("recoverIntervalSeconds"), ViewVariables(VVAccess.ReadWrite)]
    public float RecoverIntervalSeconds = 1f;

    [DataField("fatiguePopupIntervalMinutes"), ViewVariables(VVAccess.ReadWrite)]
    public float FatiguePopupIntervalMinutes = 10f;

    [DataField("lastDecreaseTime")]
    public TimeSpan LastDecreaseTime { get; set; }

    [DataField("lastRecoverTime")]
    public TimeSpan LastRecoverTime { get; set; }

    [DataField("lastPopupTime")]
    public TimeSpan LastPopupTime { get; set; }

    [DataField("originalWalkSpeed")]
    public float OriginalWalkSpeed { get; set; }

    [DataField("originalSprintSpeed")]
    public float OriginalSprintSpeed { get; set; }

    [DataField("speedReduced")]
    public bool SpeedReduced { get; set; } = false;

    [DataField("isSleeping")]
    public bool IsSleeping { get; set; } = false;

    [DataField("hasNearsightedComponent")]
    public bool HasNearsightedComponent { get; set; } = false;

    [DataField("nearsightedAddedBySystem")]
    public bool NearsightedAddedBySystem { get; set; } = false;

    [DataField("hasPermanentBlindnessComponent")]
    public bool HasPermanentBlindnessComponent { get; set; } = false;

    [DataField("temporaryBlindnessAddedBySystem")]
    public bool TemporaryBlindnessAddedBySystem { get; set; } = false;

    [DataField("isAntag")]
    public bool IsAntag { get; set; } = false;

    [DataField("fatigueMessagesTypeOne")]
    public List<string> FatigueMessagesTypeOne { get; set; } = new()
    {
        "Вам хочется поспать...",
        "Вы чувствуете себя усталым...",
        "Вы чувствуете себя сонным...",
        "Вам кажется, что даже воздух говорит вам: пора спать...",
        "Вам кажется, что сон как магнит притягивает вас...",
        "Вы представляете себе, как было бы замечательно просто прикрыв глаза, уснуть..."
    };

    [DataField("fatigueMessagesTypeTwo")]
    public List<string> FatigueMessagesTypeTwo { get; set; } = new()
    {
        "Вы мечтаете о теплой постели и мягком одеяле...",
        "У вас прямо руки опускаются от сонливости...",
        "Вы чувствуете, что мозг уже медленно работает от усталости...",
        "Вы понимаете, что каждым мгновением все труднее оставаться на ногах...",
        "Вы ощущаете, как каждая клетка вашего тела просит отдыха и сна...",
        "Вам хочется просто лечь и заснуть..."
    };

    [DataField("fatigueMessagesTypeThree")]
    public List<string> FatigueMessagesTypeThree { get; set; } = new()
    {
        "Каждый шаг даётся с огромным трудом...",
        "Вы уже чувствуете как засыпаете...",
        "Вы чувствуете как ваши веки стали тяжёлыми...",
        "Вы понимаете что долго так не может продолжаться...",
        "Ваша голова стала заметно дольше думать...",
        "Вы уже начинаете путать слова и неправильно говорить их...",
        "Ваше тело вас не слушается..."
    };
}
