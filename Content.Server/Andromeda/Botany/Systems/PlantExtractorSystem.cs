using Content.Server.Andromeda.Botany.Components;
using Content.Shared.Andromeda.Botany;
using Content.Server.Botany.Components;
using Content.Server.Construction;
using Content.Shared.Chemistry;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Content.Shared.Containers.ItemSlots;
using Content.Server.Popups;
using Robust.Server.GameObjects;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using System.Linq;
using Content.Shared.Kitchen.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Server.Power.EntitySystems;
using Content.Shared.Random.Helpers;
using Robust.Shared.Timing;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Random;
using Robust.Server.Audio;
using Content.Server.Chemistry.Containers.EntitySystems;

namespace Content.Server.Andromeda.Botany.Systems
{
    /// <summary>
    /// Contains all the server-side logic for PlantExtractors.
    /// <seealso cref="PlantExtractorComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class PlantExtractorSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly RandomHelperSystem _randomHelper = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlantExtractorComponent, ComponentStartup>((uid, _, _) => UpdateUiState(uid));
            SubscribeLocalEvent<PlantExtractorComponent, SolutionContainerChangedEvent>((uid, _, _) => UpdateUiState(uid));
            SubscribeLocalEvent<PlantExtractorComponent, BoundUIOpenedEvent>((uid, _, _) => UpdateUiState(uid));
            SubscribeLocalEvent<PlantExtractorComponent, EntInsertedIntoContainerMessage>((uid, _, _) => UpdateUiState(uid));
            SubscribeLocalEvent<PlantExtractorComponent, EntRemovedFromContainerMessage>((uid, _, _) => UpdateUiState(uid));

            SubscribeLocalEvent<PlantExtractorComponent, RefreshPartsEvent>(OnRefreshParts);
            SubscribeLocalEvent<PlantExtractorComponent, UpgradeExamineEvent>(OnUpgradeExamine);
            SubscribeLocalEvent<PlantExtractorComponent, InteractUsingEvent>(OnInteractUsing);

            SubscribeLocalEvent<PlantExtractorComponent, PlantExtractorSetModeMessage>(OnSetModeMessage);
            SubscribeLocalEvent<PlantExtractorComponent, PlantExtractorReagentAmountButtonMessage>(OnReagentButtonMessage);

            SubscribeLocalEvent<PlantExtractorComponent, ContainerIsRemovingAttemptEvent>(OnEntRemoveAttempt);
            SubscribeLocalEvent<PlantExtractorComponent, PlantExtractorStartMessage>(OnStartMessage);
            SubscribeLocalEvent<PlantExtractorComponent, PlantExtractorEjectChamberAllMessage>(OnEjectChamberAllMessage);
            SubscribeLocalEvent<PlantExtractorComponent, PlantExtractorEjectChamberContentMessage>(OnEjectChamberContentMessage);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<ActivePlantExtractorComponent, PlantExtractorComponent>();
            while (query.MoveNext(out var uid, out var activeComponent, out var component))
            //foreach (var (activeComponent, component) in EntityQuery<ActivePlantExtractorComponent, PlantExtractorComponent>())
            {
                //var uid = component.Owner;

                if (activeComponent.EndTime > _timing.CurTime)
                    continue;

                component.AudioStream = _audioSystem.Stop(component.AudioStream);
                RemCompDeferred<ActivePlantExtractorComponent>(uid);

                var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPlantExtractor.InputContainerId);

                if (!_solutionContainerSystem.TryGetSolution(uid, SharedPlantExtractor.BufferId, out _, out var bufferSolution))
                    continue;

                foreach (var item in inputContainer.ContainedEntities.ToList())
                {
                    var solution = GetExtractSolution(item);

                    if (solution is null)
                        continue;

                    if (TryComp<StackComponent>(item, out var stack))
                    {
                        solution.ScaleSolution(stack.Count);
                    }
                    QueueDel(item);

                    // _solutionContainerSystem.TryAddSolution(uid, bufferSolution, solution);
                    bufferSolution.AddSolution(solution, _prototypeManager);
                }

                UpdateUiState(uid);
                _userInterfaceSystem.TrySendUiMessage(uid, PlantExtractorUiKey.Key, new PlantExtractorWorkCompletedMessage());
            }
        }

        #region UI
        private void UpdateUiState(EntityUid uid)
        {
            if (!_solutionContainerSystem.TryGetSolution(uid, SharedPlantExtractor.BufferId, out _, out var bufferSolution))
                return;

            if (!TryComp<PlantExtractorComponent>(uid, out var plantExtractorComponent))
                return;

            var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPlantExtractor.InputContainerId);
            var beakerContainer = _itemSlotsSystem.GetItemOrNull(uid, SharedPlantExtractor.BeakerContainerId);

            var bufferReagents = bufferSolution.Contents;
            var bufferCurrentVolume = bufferSolution.Volume;

            var isBusy = HasComp<ActivePlantExtractorComponent>(uid);
            var canExtract = false;

            if (inputContainer.ContainedEntities.Count > 0)
                canExtract = inputContainer.ContainedEntities.All(CanExtract);

            var state = new PlantExtractorBoundUserInterfaceState(
                plantExtractorComponent.Mode,
                BuildBeakerContainerInfo(beakerContainer),
                bufferReagents,
                bufferCurrentVolume,
                isBusy,
                canExtract,
                GetNetEntityArray(inputContainer.ContainedEntities.ToArray()));

            _userInterfaceSystem.TrySetUiState(uid, PlantExtractorUiKey.Key, state);
        }
        #endregion

        #region Actions
        private void OnRefreshParts(EntityUid uid, PlantExtractorComponent component, RefreshPartsEvent args)
        {
            var ratingWorkTime = args.PartRatings[component.MachinePartWorkTime];
            var ratingStorage = args.PartRatings[component.MachinePartStorageMax];

            component.WorkTimeMultiplier = MathF.Pow(component.PartRatingWorkTimerMulitplier, ratingWorkTime - 1);
            component.StorageMaxEntities = component.BaseStorageMaxEntities + (int) (component.StoragePerPartRating * ratingStorage);
        }

        private void OnUpgradeExamine(EntityUid uid, PlantExtractorComponent component, UpgradeExamineEvent args)
        {
            args.AddPercentageUpgrade("plant-extractor-component-upgrade-work-time", component.WorkTimeMultiplier);
            args.AddNumberUpgrade("plant-extractor-component-upgrade-storage", component.StorageMaxEntities - component.BaseStorageMaxEntities);
        }

        private void OnSetModeMessage(EntityUid uid, PlantExtractorComponent component, PlantExtractorSetModeMessage message)
        {
            if (!Enum.IsDefined(typeof(PlantExtractorMode), message.PlantExtractorMode))
                return;

            component.Mode = message.PlantExtractorMode;
            UpdateUiState(uid);
            ClickSound(uid, component);
        }

        private void OnReagentButtonMessage(EntityUid uid, PlantExtractorComponent component, PlantExtractorReagentAmountButtonMessage message)
        {
            if (!Enum.IsDefined(typeof(PlantExtractorReagentAmount), message.Amount))
                return;

            switch (component.Mode)
            {
                case PlantExtractorMode.Transfer:
                    TransferReagents(component, message.ReagentId, message.Amount.GetFixedPoint(), message.FromBuffer);
                    break;
                case PlantExtractorMode.Discard:
                    DiscardReagents(component, message.ReagentId, message.Amount.GetFixedPoint(), message.FromBuffer);
                    break;
                default:
                    // Invalid mode.
                    return;
            }

            ClickSound(uid, component);
        }

        private void TransferReagents(PlantExtractorComponent component, ReagentId id, FixedPoint2 amount, bool fromBuffer)
        {
            var container = _itemSlotsSystem.GetItemOrNull(component.Owner, SharedPlantExtractor.BeakerContainerId);
            if (container is null ||
                !_solutionContainerSystem.TryGetFitsInDispenser(container.Value, out var containerSoln, out var containerSolution) ||
                !_solutionContainerSystem.TryGetSolution(component.Owner, SharedPlantExtractor.BufferId, out _, out var bufferSolution))
            {
                return;
            }

            if (fromBuffer) // Buffer to container
            {
                amount = FixedPoint2.Min(amount, containerSolution.AvailableVolume);
                amount = bufferSolution.RemoveReagent(id, amount);
                _solutionContainerSystem.TryAddReagent(containerSoln.Value, id, amount, out var _);
            }
            else // Container to buffer
            {
                amount = FixedPoint2.Min(amount, containerSolution.GetReagentQuantity(id));
                _solutionContainerSystem.RemoveReagent(containerSoln.Value, id, amount);
                bufferSolution.AddReagent(id, amount);
            }

            UpdateUiState(component.Owner);
        }

        private void DiscardReagents(PlantExtractorComponent plantExtractorComponent, ReagentId id, FixedPoint2 amount, bool fromBuffer)
        {

            if (fromBuffer)
            {
                if (_solutionContainerSystem.TryGetSolution(plantExtractorComponent.Owner, SharedPlantExtractor.BufferId, out _, out var bufferSolution))
                    bufferSolution.RemoveReagent(id, amount);
                else
                    return;
            }
            else
            {
                var container = _itemSlotsSystem.GetItemOrNull(plantExtractorComponent.Owner, SharedPlantExtractor.BeakerContainerId);
                if (container is not null &&
                    _solutionContainerSystem.TryGetFitsInDispenser(container.Value, out var containerSolution, out _))
                {
                    _solutionContainerSystem.RemoveReagent(containerSolution.Value, id, amount);
                }
                else
                    return;
            }

            UpdateUiState(plantExtractorComponent.Owner);
        }

        private void OnInteractUsing(EntityUid uid, PlantExtractorComponent component, InteractUsingEvent args)
        {
            var heldEnt = args.Used;
            var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPlantExtractor.InputContainerId);

            if (!HasComp<ExtractableComponent>(heldEnt) || !HasComp<ProduceComponent>(heldEnt))
            {
                if (!HasComp<FitsInDispenserComponent>(heldEnt))
                {
                    _popupSystem.PopupEntity(Loc.GetString("plant-extractor-component-cannot-put-entity-message"), uid, args.User);
                }

                return;
            }

            if (args.Handled)
                return;

            if (inputContainer.ContainedEntities.Count >= component.StorageMaxEntities)
                return;

            if (!_containerSystem.Insert(heldEnt, inputContainer))
                return;

            args.Handled = true;
        }

        private void OnEntRemoveAttempt(EntityUid uid, PlantExtractorComponent component, ContainerIsRemovingAttemptEvent args)
        {
            if (HasComp<ActivePlantExtractorComponent>(uid))
                args.Cancel();
        }

        private void OnStartMessage(EntityUid uid, PlantExtractorComponent component, PlantExtractorStartMessage message)
        {
            if (!this.IsPowered(uid, EntityManager) || HasComp<ActivePlantExtractorComponent>(uid))
                return;

            DoWork(uid, component);
        }

        private void OnEjectChamberAllMessage(EntityUid uid, PlantExtractorComponent component, PlantExtractorEjectChamberAllMessage message)
        {
            var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPlantExtractor.InputContainerId);

            if (HasComp<ActivePlantExtractorComponent>(uid) || inputContainer.ContainedEntities.Count <= 0)
                return;

            ClickSound(uid, component);
            foreach (var entity in inputContainer.ContainedEntities.ToList())
            {
                _containerSystem.Remove(entity, inputContainer);
                _randomHelper.RandomOffset(entity, 0.4f);
            }

            UpdateUiState(uid);
        }

        private void OnEjectChamberContentMessage(EntityUid uid, PlantExtractorComponent component, PlantExtractorEjectChamberContentMessage message)
        {
            if (HasComp<ActivePlantExtractorComponent>(uid))
                return;

            var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPlantExtractor.InputContainerId);

            var ent = GetEntity(message.EntityId);

            if (_containerSystem.Remove(ent, inputContainer))
            {
                _randomHelper.RandomOffset(ent, 0.4f);
                ClickSound(uid, component);
                UpdateUiState(uid);
            }
        }

        private void DoWork(EntityUid uid, PlantExtractorComponent component)
        {
            var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPlantExtractor.InputContainerId);

            if (inputContainer.ContainedEntities.Count <= 0)
                return;

            var active = AddComp<ActivePlantExtractorComponent>(uid);
            active.EndTime = _timing.CurTime + component.WorkTime * component.WorkTimeMultiplier;

            component.AudioStream = _audioSystem.PlayPvs(component.ExtractSound, uid, AudioParams.Default.WithPitchScale(1 / component.WorkTimeMultiplier)).Value.Entity;
            _userInterfaceSystem.TrySendUiMessage(uid, PlantExtractorUiKey.Key, new PlantExtractorWorkStartedMessage());
        }
        #endregion

        #region Helpers
        private ContainerInfo? BuildBeakerContainerInfo(EntityUid? container)
        {
            if (container is not { Valid: true })
                return null;

            if (!TryComp(container, out FitsInDispenserComponent? fits)
                || !_solutionContainerSystem.TryGetSolution(container.Value, fits.Solution, out _, out var solution))
            {
                return null;
            }

            return BuildContainerInfo(Name(container.Value), solution);
        }

        private static ContainerInfo BuildContainerInfo(string name, Solution solution)
        {
            return new ContainerInfo(name, solution.Volume, solution.MaxVolume)
            {
                Reagents = solution.Contents
            };
        }

        private bool CanExtract(EntityUid uid)
        {
            var solutionName = CompOrNull<ExtractableComponent>(uid)?.GrindableSolution;

            return solutionName is not null && _solutionContainerSystem.TryGetSolution(uid, solutionName, out _);
        }

        private Solution? GetExtractSolution(EntityUid uid)
        {
            if (TryComp<ExtractableComponent>(uid, out var extractable)
                && extractable.GrindableSolution is not null
                && _solutionContainerSystem.TryGetSolution(uid, extractable.GrindableSolution, out _, out var solution))
            {
                return solution;
            }
            else
                return null;
        }

        private void ClickSound(EntityUid uid, PlantExtractorComponent component)
        {
            _audioSystem.PlayPvs(component.ClickSound, uid, AudioParams.Default.WithVolume(-2f));
        }
        #endregion
    }
}
