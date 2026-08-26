# Unity Architecture v1

Версия: 1.0  
Основание: `Production GDD v1.md`  
Цель: определить границы систем и контракты до написания игровой реализации.

## 1. Архитектурные принципы

1. Состояние симуляции не хранится в UI, анимациях или отдельных копиях в NPC.
2. `MonoBehaviour` связывает сцену и доменную логику, но не содержит баланс.
3. Баланс хранится в версионируемых `ScriptableObject`-конфигах.
4. Межсистемное общение идёт через узкие интерфейсы и типизированные события.
5. Порядок смены фаз централизован. NPC не заводят собственные часы суток.
6. AI работает событийно и с ограниченной частотой принятия решений, а не выполняет полный поиск каждый кадр.
7. Временные NPC, снаряды и предметы используют пулы.
8. Сохранение работает с простыми DTO и идентификаторами, а не сериализует сценовые ссылки.
9. Любая подписка имеет явного владельца и симметричную отписку.
10. Вертикальный срез остаётся однопользовательским; сетевые абстракции не добавляются заранее.

## 2. Рекомендуемая структура Assets

```text
Assets/Game/
  Art/
    Animations/
    Materials/
    Models/
    Prefabs/
    Sprites/
    VFX/
  Audio/
    Music/
    SFX/
  Config/
    Balance/
    Content/
  Scenes/
    Bootstrap.unity
    MainMenu.unity
    VerticalSlice.unity
  Scripts/
    Runtime/
      Application/
      Common/
      Time/
      Economy/
      Territory/
      Actors/
      AI/
      Combat/
      Buildings/
      Settlement/
      Raids/
      Crown/
      Save/
      UI/
      Audio/
    Editor/
    Tests/
      EditMode/
      PlayMode/
  UI/
    Prefabs/
    Fonts/
    Icons/
```

Assembly definitions:

- `Game.Runtime` — вся runtime-реализация;
- `Game.Editor` — инструменты редактора, ссылается на Runtime;
- `Game.Tests.EditMode` — быстрые тесты правил;
- `Game.Tests.PlayMode` — сцена, физика и жизненный цикл.

На первой версии не дробить Runtime на множество assembly definition: это замедлит разработку без практической пользы.

## 3. Сцены и запуск

### Bootstrap

Минимальная сцена, которая создаёт постоянный `GameRoot`, загружает настройки и открывает нужную сцену. Содержит только глобальные сервисы:

- `SceneFlowService`;
- `SaveService`;
- `SettingsService`;
- `AudioService`;
- `InputService`;
- `ObjectPoolRegistry`.

### MainMenu

Новая игра, продолжить, настройки, авторы, выход. Не содержит игровую симуляцию.

### VerticalSlice

Содержит уровень, камеры, точки размещения, сценовые представления и `WorldCompositionRoot`. Он получает глобальные сервисы от `GameRoot`, создаёт сервисы конкретного прохождения и явно уничтожает их при выходе.

Не использовать автоматический поиск всех зависимостей через `FindObjectsOfType`/`GetComponentInChildren` в runtime. Ссылки уровня задаются сериализованным `SceneReferences` и валидируются в `Awake`.

## 4. Слои

### Config

Неизменяемые во время сессии настройки: цены, HP, скорости, длительности, таблицы поселений и рейдов.

### Model

Чистое текущее состояние: фаза, валюта, границы заражения, население, поселение, стены, короны.

### Services

Правила, меняющие Model: время, экономика, территория, формирование рейда, победа, поражение, сохранение.

### Views

`MonoBehaviour`, Animator, SpriteRenderer, VFX, AudioSource и физические коллайдеры. View показывает состояние и передаёт намерения, но не решает экономические или победные правила.

### Presentation

HUD, подсказки, меню и форматирование. Получает read-only snapshots и события.

## 5. Композиция систем

```text
GameSession
 ├─ PhaseService
 ├─ EconomyService
 ├─ TerritoryService
 ├─ PopulationService
 ├─ JobService
 ├─ BuildingService
 ├─ SettlementService
 ├─ RaidService
 ├─ CombatService
 ├─ CrownService
 ├─ VictoryService
 ├─ SaveCoordinator
 └─ SessionTelemetry
```

`GameSession` управляет запуском, паузой и завершением. Он не реализует правила подсистем самостоятельно.

## 6. Общие типы контрактов

Рекомендуемые value types:

```csharp
public readonly record struct ActorId(int Value);
public readonly record struct BuildingId(int Value);
public readonly record struct ItemId(int Value);
public readonly record struct CycleNumber(int Value);
public readonly record struct GreedAmount(int Value);
public readonly record struct GoldAmount(int Value);

public enum DayPhase { Night, Dawn, Day, Dusk }
public enum GreedRole { Formless, Fighter, Gatherer, Builder }
public enum SettlementTier { Camp, Village, FortifiedVillage, City, Castle }
public enum CrownOwner { Greed, Ground, HumanCarrier, GreedCarrier, Heart, Settlement }
public enum SessionResult { None, Victory, GreedCrownLost, HeartDestroyed }
```

На границах конфигов и сохранений обязательно проверять, что количества неотрицательны, идентификаторы существуют, enum имеет допустимое значение, а версия схемы поддерживается.

## 7. Контракт времени

```csharp
public interface IGameClock
{
    DayPhase CurrentPhase { get; }
    CycleNumber CurrentCycle { get; }
    float PhaseTimeRemaining { get; }
    float PhaseProgress01 { get; }
    bool IsSimulationPaused { get; }

    event Action<PhaseChanged> PhaseChanged;
}

public readonly record struct PhaseChanged(
    DayPhase Previous,
    DayPhase Current,
    CycleNumber Cycle);
```

Только `PhaseService` переводит фазу. Он отправляет одно событие на границе, затем обновляет таймер. Порядок обработчиков фиксируется координатором:

1. завершить старые задания;
2. применить производство/рейд;
3. изменить правила света;
4. уведомить AI;
5. обновить представление и звук;
6. запросить контрольное сохранение.

Тесты времени используют внедряемый источник delta time и не зависят от реального ожидания.

## 8. Контракт экономики

```csharp
public interface IGreedWallet
{
    int Carried { get; }
    int Reserve { get; }
    int Capacity { get; }
    bool CanSpend(int amount);
    SpendResult TrySpend(int amount, SpendReason reason);
    DepositResult DepositGold(int amount, ActorId source);
    int DropFromHit(int requestedAmount, Vector2 position);

    event Action<WalletChanged> Changed;
}

public enum SpendReason
{
    Recruit, ProfessionTool, InfectionNode, GreedWall,
    EyeTower, CommandNode, Assault
}
```

Только `IGreedWallet` меняет Жадность. Постройки не вычитают валюту сами. Любое списание возвращает результат с фактически списанной суммой и причиной отказа.

```csharp
public interface IHumanTreasury
{
    int StoredGold { get; }
    int Capacity { get; }
    GoldTakeResult TryReserveGold(ActorId gatherer, int requested);
    void ConfirmTaken(ActorId gatherer);
    void CancelReservation(ActorId gatherer);
    SettlementUpgradeResult ProduceAndTryUpgrade();
}
```

Кража использует резервирование: пока Сборщик проигрывает анимацию, золото зарезервировано, но списывается только при успешном завершении. Смерть или отмена возвращает резерв.

## 9. Контракт территории

```csharp
public interface IInfectionTerritory
{
    float LeftBoundary { get; }
    float RightBoundary { get; }
    bool Contains(float worldX);
    bool CanPlace(BuildingType type, float worldX, out PlacementFailure reason);
    InfectionExpansionResult CompleteNode(BuildingId node, Side side);
    InfectionRetractionResult DestroyNode(BuildingId node);

    event Action<TerritoryChanged> Changed;
}
```

Все проверки солнца и строительства обращаются к одному `IInfectionTerritory`. View заражения подписывается на `Changed`, но не хранит собственную игровую границу.

Узлы образуют упорядоченную цепочку для каждой стороны. Удаление узла не должно оставлять разорванный активный участок.

## 10. Контракт актёра и боя

```csharp
public interface IDamageable
{
    ActorId Id { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }
    DamageResult ApplyDamage(in DamageRequest request);
    event Action<DamageApplied> Damaged;
    event Action<ActorId> Died;
}

public readonly record struct DamageRequest(
    ActorId Source,
    int Amount,
    DamageKind Kind,
    Vector2 HitPoint);
```

Урон применяется единожды через `IDamageable`. Анимация не вызывает второе списание HP. Для ближнего боя Animation Event только открывает подтверждённое конфигом окно попадания, а реальная цель проверяется физикой и принадлежностью к фракции.

Каждая атака имеет уникальный `AttackId`, чтобы один и тот же swing не повредил одну цель дважды.

## 11. Контракт населения и профессий

```csharp
public interface IPopulationService
{
    PopulationSnapshot Snapshot { get; }
    RecruitResult TryRecruit(ActorId formless);
    ToolOrderResult TryOrderTool(GreedRole role);
    bool TryAssignTool(ItemId tool, ActorId formless);
    void RegisterDeath(ActorId actor);
    event Action<PopulationChanged> Changed;
}
```

Профессия является единственным полем состояния Greed. Нельзя одновременно учитывать одного NPC как Бойца и Сборщика.

Инструмент имеет состояния `Available`, `Reserved`, `Carried`, `Dropped`, `Expired`. Любой переход выполняет `JobService`; NPC не меняет владение инструментом напрямую.

## 12. Контракт заданий AI

```csharp
public interface IJob
{
    JobId Id { get; }
    JobKind Kind { get; }
    int Priority { get; }
    bool IsValid(in WorldReadModel world);
    JobTickResult Tick(IAgentContext agent, float deltaTime);
    void Cancel(JobCancelReason reason);
}

public interface IJobBoard
{
    JobClaimResult TryClaim(ActorId actor, GreedRole role);
    void Release(JobId job, ActorId actor, JobReleaseReason reason);
}
```

AI делится на:

- редкое принятие решения: 4–10 раз в секунду в зависимости от роли;
- движение и физику в `FixedUpdate`;
- анимацию в View;
- реакцию на события фазы, урона и разрушения цели.

Не создавать отдельный бесконечный coroutine для каждого решения каждого NPC. Диспетчер обновляет агентов пакетами с распределением по кадрам.

## 13. Контракт строительства

```csharp
public interface IBuildingService
{
    PlacementResult TryCreateOrder(BuildingType type, BuildSocketId socket);
    BuildWorkResult ApplyWork(BuildingId building, ActorId builder, float amount);
    RepairResult Repair(BuildingId building, ActorId builder, float amount);
    IReadOnlyList<BuildJobView> GetAvailableJobs(GreedRole role);
    event Action<BuildingStateChanged> StateChanged;
}
```

Состояния постройки:

```text
Locked → Available → Funded → Constructing → Active → Damaged → Destroyed
                                    ↑             ↓
                                    └── Repairing ┘
```

Цена списывается при переходе `Available → Funded`. Повторная оплата невозможна. Уничтожение уже оплаченного, но недостроенного объекта не возвращает стоимость в v1.

Размещение выполняется через заранее созданные `BuildSocket`, а не свободное позиционирование мышью. Это соответствует управлению влево/вправо и делает уровень контролируемым.

## 14. Контракт поселения и рейдов

```csharp
public interface ISettlementService
{
    SettlementSnapshot Snapshot { get; }
    SettlementCycleResult ProcessNightStart(CycleNumber cycle);
    bool AreOuterDefensesDestroyed { get; }
    event Action<SettlementTierChanged> TierChanged;
}

public interface IRaidPlanner
{
    RaidPlan CreatePlan(SettlementSnapshot settlement, CycleNumber cycle);
}

public interface IRaidService
{
    RaidState State { get; }
    RaidStartResult Start(in RaidPlan plan);
    void BeginRetreat();
    event Action<RaidCompleted> Completed;
}
```

`IRaidPlanner` является чистой функцией: одинаковые snapshot и seed дают одинаковый план. Это позволяет тестировать состав рейда без сцены.

`RaidService` соблюдает глобальный лимит NPC, создаёт следующую группу только при наличии мест и никогда не теряет юнита из бюджета из-за временно заполненного пула.

## 15. Контракт наступления Greed

```csharp
public interface IAssaultService
{
    AssaultAvailability GetAvailability(float nightTimeRemaining);
    AssaultStartResult TryStart();
    void RequestRetreat(RetreatReason reason);
    AssaultSnapshot Snapshot { get; }
    event Action<AssaultStateChanged> StateChanged;
}
```

Операция `TryStart` атомарна: проверяет время, Бойцов и валюту, затем резервирует Бойцов и списывает цену. Частично сформированного отряда быть не может.

Состояния: `Idle → Forming → Advancing → Attacking → Retreating → Completed`. Смена фазы на Рассвет переводит любое активное наступательное состояние в `Retreating`.

## 16. Контракт Корон и завершения сессии

```csharp
public interface ICrownService
{
    CrownSnapshot GreedCrown { get; }
    CrownSnapshot HumanCrown { get; }
    CrownPickupResult TryPickup(CrownKind crown, ActorId actor);
    CrownDropResult Drop(CrownKind crown, ActorId carrier, Vector2 position);
    CrownDeliveryResult TryDeliver(CrownKind crown, CrownDestination destination);
    event Action<CrownStateChanged> Changed;
}

public interface IVictoryService
{
    SessionResult Result { get; }
    bool TryResolve(SessionEndCause cause);
    event Action<SessionEnded> SessionEnded;
}
```

`IVictoryService` допускает только первый терминальный результат. После него экономика, AI и бой останавливаются, а UI получает неизменяемый итог.

Корона не является дочерним объектом переносчика как источником истины. `CrownService` хранит владельца, а View только визуально следует за указанным носителем.

## 17. Контракт сохранения

```csharp
public interface ISaveService
{
    Task<SaveResult> SaveAsync(GameSaveData data, CancellationToken token);
    Task<LoadResult<GameSaveData>> LoadLatestAsync(CancellationToken token);
    bool HasCompatibleSave { get; }
}

public interface ISaveContributor
{
    string SectionKey { get; }
    object CaptureState();
    RestoreResult RestoreState(object section, int schemaVersion);
}
```

Правила:

- `schemaVersion` обязателен;
- путь записи выбирает сервис, а не игровая система;
- запись выполняется вне игрового кадра, но snapshot собирается на главном потоке;
- используется `save.tmp → проверка → save.json`, предыдущий файл становится `save.bak`;
- отмена выхода ждёт завершения уже начатой атомарной замены;
- отсутствие необязательной секции использует безопасное значение по умолчанию;
- неизвестная более новая версия не загружается молча.

## 18. Контракт UI

```csharp
public interface IGameHudView
{
    void Render(in HudViewModel model);
    void ShowInteraction(in InteractionPrompt prompt);
    void HideInteraction();
    void ShowSessionResult(SessionResult result);
}

public interface IGameHudPresenter : IDisposable
{
    void Initialize();
}
```

Presenter подписывается на агрегированные изменения и создаёт `HudViewModel`. View не обращается к миру через поиск объектов.

Текст игрока хранится через стабильные ключи, даже до подключения Unity Localization. Нельзя использовать отображаемую русскую строку как идентификатор логики.

## 19. Конфиги ScriptableObject

Минимальный набор:

- `GameBalanceConfig` — ссылки на остальные конфиги и версия баланса;
- `PhaseConfig` — длительности фаз и кривая солнечного урона;
- `ActorConfig` — HP, скорости, атаки и интервалы решений;
- `BuildingConfig` — цена, HP, work amount и prefab;
- `SettlementProgressionConfig` — уровни, цены, доход и вместимость;
- `RaidCompositionConfig` — бюджеты, обязательные составы и лимиты;
- `MapConfig` — координаты, границы и build sockets;
- `SaveConfig` — версия схемы и имена файлов;
- `AudioConfig` — события и клипы без игровой логики.

Каждый конфиг валидируется в `OnValidate` и отдельным EditMode-тестом:

- неотрицательные цены и HP;
- последовательное возрастание уровней;
- уникальные идентификаторы;
- существующие prefab-ссылки;
- длительность полного цикла равна сумме фаз;
- обязательный состав рейда не нарушает абсолютный лимит NPC.

## 20. Навигация и производительность

Поскольку мир одномерный, не использовать NavMesh для базового движения.

- движение идёт по оси X к числовой цели;
- препятствия представлены воротами/стенами и точками ожидания;
- lane/ground height задаётся уровнем;
- локальные столкновения решаются Physics2D;
- поиск ближайшей цели использует зарегистрированные коллекции и пространственные интервалы, а не `FindObjectsOfType`;
- дальние NPC обновляют решение реже;
- максимум в вертикальном срезе: 40 Greed, 20 людей, 30 переносимых предметов и 32 снаряда одновременно.

Пулы обязательны для людей рейда, кристаллов, монет, инструментов, снарядов, hit VFX и floating text. Постоянные здания не требуют пула.

## 21. Жизненный цикл и события

Для подписок использовать один из двух шаблонов:

1. `OnEnable` подписывает, `OnDisable` отписывает — только для View с одинаковым временем жизни.
2. Явный `Initialize` возвращает/создаёт `CompositeDisposable`, а `Dispose` освобождает — для сервисов и Presenter.

Запрещено:

- анонимно подписываться на глобальное событие без сохранения делегата;
- оставлять `CancellationTokenSource` без `Cancel` и `Dispose`;
- запускать бесконечный coroutine без владельца;
- хранить уничтоженный `UnityEngine.Object` в реестре;
- использовать статические mutable-события как глобальную шину.

## 22. Логирование

Категории: `Session`, `Save`, `Economy`, `AI`, `Combat`, `Settlement`.

- ошибки инвариантов — `Error` с ID сущности и состоянием;
- невозможная операция пользователя — результат API и UI-подсказка, не ошибка в Output;
- переход уровня, победа, поражение и ошибка сохранения — единичные диагностические записи;
- движение, обычные атаки и кадровые обновления не логируются.

## 23. Минимальный набор автоматических тестов

EditMode:

1. Цикл проходит фазы в правильном порядке и длительности.
2. Конвертация золота сохраняет сумму при заполненном переносимом запасе.
3. Частичная оплата не создаёт готовую постройку.
4. Территория расширяется и откатывается по цепочке Узлов.
5. Солнечный урон отсутствует внутри заражения.
6. Поселение списывает цену повышения и не понижает уровень после кражи.
7. RaidPlanner соблюдает обязательный состав и лимит.
8. AssaultService не списывает валюту при отсутствии четырёх Бойцов.
9. Одна атака не наносит цели повторный урон.
10. Корона имеет ровно одного владельца.
11. Первый результат сессии остаётся терминальным.
12. Save DTO проходит serialize/deserialize без потери значений.

PlayMode:

1. Сборщик проходит полный маршрут, крадёт и сдаёт золото.
2. Рассвет отменяет поход и запускает возврат.
3. Greed получает солнечный урон только за границей.
4. Строитель завершает оплаченный заказ и ремонтирует стену.
5. Рейд разрушает стену и переключает цель.
6. Обе Короны можно выронить, подобрать и доставить.
7. После возврата в пул NPC не остаётся подписанным на фазу.
8. Загрузка контрольной точки восстанавливает эквивалентное состояние.

## 24. Порядок реализации классов

1. Общие ID, результаты операций и конфиги.
2. `PhaseService` и `IGameClock`.
3. `InfectionTerritory` без визуала.
4. `GreedWallet` и `HumanTreasury`.
5. Базовые Actor, Health, Damage и Faction.
6. Главная Жадность и взаимодействие оплатой.
7. Population, инструменты и профессии.
8. JobBoard и Строитель.
9. Сборщик и резервирование золота.
10. Боец и ближний бой.
11. BuildingService, Стена и Глазница.
12. SettlementService и RaidPlanner.
13. RaidService и человеческие классы.
14. AssaultService и Командир.
15. CrownService, победа и поражение.
16. SaveService.
17. HUD, звук, обучение и полировка.

Каждый пункт завершается тестом контракта до перехода к следующему.

## 25. Definition of Done для любой системы

Система готова, если:

- правило из Production GDD реализовано без дублирующего источника истины;
- публичный контракт имеет описанные входы, результаты и ошибки;
- конфиг не содержит недействительных значений;
- подписки и временные объекты освобождаются;
- существуют EditMode-тесты чистых правил;
- при наличии сценового поведения существует PlayMode-тест или ручной чек-лист;
- UI показывает причины отказа игроку;
- отсутствуют новые ошибки и исключения в Console;
- документация изменена, если контракт поменялся.
