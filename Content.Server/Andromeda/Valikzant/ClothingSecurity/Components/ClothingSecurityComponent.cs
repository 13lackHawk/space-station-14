using System.Threading;

namespace Content.Server.ClothingSecurity
{
    /// <summary>
    ///     Активирует определенный сценарий, если игрок надевает костюм, который кто-то носил его до этого.
    ///     То есть, этот компонент жёстко фиксирует владельца по uid, которое присваивается костюму послего его снятия
    ///     если кто-то был заспавнен с ним. Или же присваивает после надевания, если до этого не имело uid.
    ///
    ///     При указании определенной фракции проверяет только про фракции, и владельца костюма.
    /// </summary>
    [RegisterComponent]
    public sealed partial class ClothingSecurityComponent : Component
    {
        /// <summary>
        ///     Используемый сценарий:
        ///     null - Ничего не делать
        ///     Explosion - Взрыв (По умолчанию)
        ///     LungsBreak - Ломание лёгких - TODO
        ///     BonesBreak - Ломание костей - TODO
        ///     FireInTheHole - Поджигание внутренностей костюма - TODO
        ///     Gib - Уничтожение игрока
        /// </summary>
        [DataField("scenario")]
        [ViewVariables(VVAccess.ReadWrite)]
        public string Scenario = "Explosion";

        /// <summary>
        ///     На какую фракцию не накладываются ограничения?
        /// </summary>
        [DataField("checkFaction")]
        [ViewVariables(VVAccess.ReadOnly)]
        public string CheckFaction = "";

        /// <summary>
        ///     Уничтожать ли одежду после исполнения сценария?
        /// </summary>
        [DataField("destroy")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Destroy = true;

        /// <summary>
        ///     Сообщения какого типа будет выдавать одежда перед выполнением сценария?
        ///     -1 - Отключает любые оповещения
        ///     0 (По умолчанию) - Проклятая одежда: 'Вы чувствуете что-то странное...'
        ///     1 - Технологии НаноТрейзен: 'Контрактов с НаноТрейзен не обнаружено...'
        ///     2 - Технологии Синдиката: 'Обнаружен враг!'
        ///     3 - Приветливый костюм: 'Приветс- Ошибка доступа!'
        ///     4 - Крутой костюм: 'Эй, чувак! Ты что-то попутал!'
        ///     5 - Вежливый костюм: 'Соизвольте активировать сценарии защиты...'
        ///     6 - TODO?
        /// </summary>
        [DataField("clothingPersonality")]
        [ViewVariables(VVAccess.ReadOnly)]
        public int ClothingPersonality = 0;

        /// <summary>
        ///     Задержка между фразами в секундах
        ///     (0.5 - 500 мл. секунд)
        ///     (1 - 1 секунда) и тд.
        ///     Если одежде включены фразы, то стоит ставить хотя-бы 1.5 секунды,
        ///     чтобы игрок успел прочитать текст.
        ///     Самое минимальное значение - 250 мл. секунд.
        /// </summary>
        [DataField("delays")]
        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan Delays = TimeSpan.FromMilliseconds(1000);

        [ViewVariables(VVAccess.ReadOnly)]
        private static EntityUid _clothingOwnerUid = EntityUid.Invalid;

        [ViewVariables(VVAccess.ReadWrite)]
        public EntityUid ClothingOwnerUid = _clothingOwnerUid;
        private CancellationTokenSource _cancellationTokenSource;
        public void SetCancellationTokenSource(CancellationTokenSource cts)
        {
            _cancellationTokenSource = cts;
        }

        public void CancelScenario()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
