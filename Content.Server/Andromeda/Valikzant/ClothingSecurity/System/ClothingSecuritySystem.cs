using Content.Shared.Inventory.Events;
using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NPC.Components;
using Content.Server.Popups;
using Content.Shared.Popups;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.ClothingSecurity
{
    public sealed class ClothingSecuritySystem : EntitySystem
    {
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        public override void Initialize()
        {
            SubscribeLocalEvent<ClothingSecurityComponent, GotEquippedEvent>(OnGotEquipped);
            SubscribeLocalEvent<ClothingSecurityComponent, GotUnequippedEvent>(OnGotUnequipped);
        }
        /// <summary>
        ///     Считает время перед активацией сценария.
        /// </summary>
        private async Task CountTime(ClothingSecurityComponent component, EntityUid playerUid, EntityUid clothingUid, CancellationToken cancellationToken)
        {
            try
            { // Мой работающий говнокод-таймер
                var delayBetween = component.Delays > TimeSpan.FromMilliseconds(250) ? component.Delays : TimeSpan.FromMilliseconds(250);
                await Task.Delay(delayBetween, cancellationToken);
                _popup.PopupEntity(component.ClothingPersonality > 0 ? Loc.GetString($"security-clothing-trigger-{component.ClothingPersonality}") : string.Empty, playerUid, playerUid, PopupType.MediumCaution);
                await Task.Delay(delayBetween, cancellationToken);
                _popup.PopupEntity(component.ClothingPersonality > 0 ? Loc.GetString($"security-clothing-warning-{component.ClothingPersonality}") : string.Empty, playerUid, playerUid, PopupType.LargeCaution);
                await Task.Delay(delayBetween, cancellationToken);
                ScenarioStart(playerUid, clothingUid, component.Scenario, component.Destroy); // Таймер прошел...
            }
            catch (TaskCanceledException)
            {
                // Остановка таймера перед сценарием и завершение задачи типа.
            }
        }
        /// <summary>
        ///     Вызывается, когда игрок снял вещь, у которой присутствует компонент.
        /// </summary>
        private void OnGotUnequipped(EntityUid uid, ClothingSecurityComponent component, GotUnequippedEvent args)
        {
            component.CancelScenario(); // Отменяем сценарий, если игрок снял предмет
            if (component.ClothingOwnerUid == EntityUid.Invalid && _entManager.EntityExists(args.Equipee))
                component.ClothingOwnerUid = args.Equipee; // Присваиваем владельца при снятии предмета.
        }
        /// <summary>
        ///     Вызывается, когда игрок надел вещь, у которой присутствует компонент.
        /// </summary>
        private async void OnGotEquipped(EntityUid uid, ClothingSecurityComponent component, GotEquippedEvent args)
        {
            // Поиск, если у сущности есть компонент фракции и в компоненте указана проверка фракции
            if (TryComp<NpcFactionMemberComponent>(args.Equipee, out var faction) && !string.IsNullOrWhiteSpace(component.CheckFaction))
            {
                var readedFactions = faction.Factions; // Читаем фракции из компонента.
                if (readedFactions.Contains(component.CheckFaction))
                { // Если фракция из компонента совпадает с фракцией сущности, то отменяем вызов таймера с сценарием.
                    return;
                }
            }
            // Запускаем сценарий, если владелец (если он уже существует) не совпадает с игроком
            if (component.ClothingOwnerUid != args.Equipee && component.ClothingOwnerUid != EntityUid.Invalid && _entManager.EntityExists(args.Equipee))
            {
                var cts = new CancellationTokenSource(); // Сохраняем в компонент токен, с помощью которого
                component.SetCancellationTokenSource(cts); // потом отменять сценарий, если игрок снимет вещь
                await CountTime(component, args.Equipee, args.Equipment, cts.Token);
            }
        }
        /// <summary>
        ///     Запускает определенный сценарий
        /// </summary>
        private async void ScenarioStart(EntityUid playerUid, EntityUid clothingUid, string scenario, bool destroy)
        {
            if (scenario == null || !_entManager.EntityExists(playerUid))
                return;

            if (scenario == "Explosion")
            {
                _explosionSystem.QueueExplosion(playerUid, "Default", 4, 1, 2, maxTileBreak: 0);
            }

            if (scenario == "LungsBreak")
                return; //TODO

            if (scenario == "BonesBreak")
                return; //TODO

            if (scenario == "FireInTheHole")
                return; //TODO

            if ((scenario == "Gib" || scenario == "Explosion") && _entManager.EntityExists(playerUid))
                _bodySystem.GibBody(playerUid);

            if (_entManager.EntityExists(clothingUid) && destroy)
                EntityManager.DeleteEntity(clothingUid);
        }
    }
}
