using Content.Server.Botany.Systems;
using Content.Shared.Botany;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Botany.Components
{
    [Access(typeof(PlantExtractorSystem)), RegisterComponent]
    public sealed partial class PlantExtractorComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public int StorageMaxEntities = 10;

        [DataField("baseStorageMaxEntities"), ViewVariables(VVAccess.ReadWrite)]
        public int BaseStorageMaxEntities = 10;

        [DataField("machinePartStorageMax", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartStorageMax = "MatterBin";

        [DataField("storagePerPartRating")]
        public int StoragePerPartRating = 10;

        [DataField("workTime"), ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan WorkTime = TimeSpan.FromSeconds(3.5); // Should match extract sound duration.

        [ViewVariables(VVAccess.ReadWrite)]
        public float WorkTimeMultiplier = 1;

        [DataField("machinePartWorkTime", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartWorkTime = "Manipulator";

        [DataField("partRatingWorkTimeMultiplier")]
        public float PartRatingWorkTimerMulitplier = 0.6f;

        [DataField("mode"), ViewVariables(VVAccess.ReadWrite)]
        public PlantExtractorMode Mode = PlantExtractorMode.Transfer;

        [DataField("clickSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ClickSound { get; set; } = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        [DataField("extractSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ExtractSound { get; set; } = new SoundPathSpecifier("/Audio/Machines/blender.ogg");

        public IPlayingAudioStream? AudioStream;
    }

    [Access(typeof(PlantExtractorSystem)), RegisterComponent]
    public sealed partial class ActivePlantExtractorComponent : Component
    {
        /// <summary>
        /// Remaining time until the plant extractor finishes extracting.
        /// </summary>
        [ViewVariables]
        public TimeSpan EndTime;
    }
}
