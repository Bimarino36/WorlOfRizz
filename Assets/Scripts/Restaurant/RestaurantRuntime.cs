using System;
using System.Collections.Generic;
using System.Linq;
using IdleRestaurant.Localization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantRuntime : MonoBehaviour
    {
        public struct OfflineIncomeReport
        {
            public int RawIncome;
            public int FinalIncome;
            public int SoftCap;
            public int Stage;
            public int ElapsedSeconds;
            public bool WasCapped;
        }

        private const string SaveKey = "IdleRestaurant.RuntimeProgress";

        [Serializable]
        private struct ProgressData
        {
            public int totalMoney;
            public int totalTips;
            public int lifetimeOrderRevenue;
            public int servedGuests;
            public int walkedOutGuests;
            public int queueWalkedOutGuests;
            public int loyaltyScore;
            public int tableRevenueLevel;
            public int waiterSpeedLevel;
            public int waiterTakeOrderSpeedLevel;
            public int waiterSubmitOrderSpeedLevel;
            public int waiterPickupSpeedLevel;
            public int waiterCharismaLevel;
            public int kitchenSpeedLevel;
            public int barSpeedLevel;
            public int waiterPriorityMode;
            public long lastSavedUnixSeconds;
        }

        [Header("Scene")]
        [SerializeField] private WaiterAgent waiter;
        [SerializeField] private List<RestaurantTable> tables = new List<RestaurantTable>();
        [SerializeField] private List<RestaurantPoint> servicePoints = new List<RestaurantPoint>();

        [Header("Guests")]
        [SerializeField] private RestaurantGuest guestPrefab;
        [SerializeField, Min(1)] private int maxActiveGuests = 6;
        [SerializeField] private Vector2 spawnDelayRange = new Vector2(3.5f, 6f);
        [SerializeField] private Vector3 fallbackGuestScale = new Vector3(0.35f, 0.5f, 0.35f);

        [Header("Order Timings")]
        [SerializeField] private Vector2 kitchenReadyDelayRange = new Vector2(4f, 7f);
        [SerializeField] private Vector2 barReadyDelayRange = new Vector2(2f, 5f);
        [SerializeField] private Vector2 eatingDurationRange = new Vector2(5f, 9f);

        [Header("Queue")]
        [SerializeField] private bool enableGuestQueue = true;
        [SerializeField, Min(1)] private int queueCapacity = 4;
        [SerializeField, Min(0.5f)] private float queueSpacing = 1.2f;
        [SerializeField] private Vector3 queueDirection = new Vector3(0f, 0f, -1f);
        [SerializeField, Min(1f)] private float queueWaitLimit = 16f;
        [SerializeField, Min(0f)] private float queueWaitWarningThreshold = 0.7f;

        [Header("Guest Patience")]
        [SerializeField, Min(1f)] private float orderWaitLimit = 12f;
        [SerializeField, Min(1f)] private float deliveryWaitLimit = 18f;
        [SerializeField, Min(1f)] private float billWaitLimit = 10f;
        [SerializeField, Range(0f, 0.95f)] private float tipDecayStartNormalized = 0.45f;
        [SerializeField] private bool allowGuestWalkoutBeforeOrder = true;

        [Header("Waiter AI")]
        [SerializeField] private WaiterPriorityMode waiterPriorityMode = WaiterPriorityMode.Balanced;
        [SerializeField, Range(0.5f, 1f)] private float urgencyOverrideThreshold = 0.82f;

        [Header("Economy")]
        [SerializeField] private Vector2Int orderPriceRange = new Vector2Int(25, 60);
        [SerializeField] private Vector2Int tipRange = new Vector2Int(2, 12);
        [SerializeField] private int totalMoney;
        [SerializeField] private int totalTips;
        [SerializeField] private int lifetimeOrderRevenue;
        [SerializeField] private int servedGuests;
        [SerializeField] private int walkedOutGuests;
        [SerializeField] private int queueWalkedOutGuests;
        [SerializeField] private int loyaltyScore;

        [Header("Upgrades")]
        [SerializeField, Min(0)] private int tableRevenueLevel;
        [SerializeField, Min(0)] private int waiterSpeedLevel;
        [SerializeField, Min(0)] private int waiterTakeOrderSpeedLevel;
        [SerializeField, Min(0)] private int waiterSubmitOrderSpeedLevel;
        [SerializeField, Min(0)] private int waiterPickupSpeedLevel;
        [SerializeField, Min(0)] private int waiterCharismaLevel;
        [SerializeField, Min(1)] private int baseTableRevenueUpgradeCost = 120;
        [SerializeField, Min(1)] private int baseWaiterSpeedUpgradeCost = 90;
        [SerializeField, Min(1)] private int baseWaiterTakeOrderSpeedUpgradeCost = 80;
        [SerializeField, Min(1)] private int baseWaiterSubmitOrderSpeedUpgradeCost = 85;
        [SerializeField, Min(1)] private int baseWaiterPickupSpeedUpgradeCost = 95;
        [SerializeField, Min(1)] private int baseWaiterCharismaUpgradeCost = 110;
        [SerializeField, Min(1)] private int baseKitchenSpeedUpgradeCost = 105;
        [SerializeField, Min(1)] private int baseBarSpeedUpgradeCost = 95;
        [SerializeField, Min(1f)] private float tableRevenueUpgradeCostGrowth = 1.55f;
        [SerializeField, Min(1f)] private float waiterSpeedUpgradeCostGrowth = 1.6f;
        [SerializeField, Min(1f)] private float waiterTakeOrderSpeedUpgradeCostGrowth = 1.52f;
        [SerializeField, Min(1f)] private float waiterSubmitOrderSpeedUpgradeCostGrowth = 1.54f;
        [SerializeField, Min(1f)] private float waiterPickupSpeedUpgradeCostGrowth = 1.56f;
        [SerializeField, Min(1f)] private float waiterCharismaUpgradeCostGrowth = 1.62f;
        [SerializeField, Min(1f)] private float kitchenSpeedUpgradeCostGrowth = 1.58f;
        [SerializeField, Min(1f)] private float barSpeedUpgradeCostGrowth = 1.56f;
        [SerializeField, Min(0f)] private float tableRevenueBonusPerLevel = 0.2f;
        [SerializeField, Min(0f)] private float tableTipBonusPerLevel = 0.12f;
        [SerializeField, Min(0f)] private float waiterSpeedBonusPerLevel = 0.18f;
        [SerializeField, Min(0f)] private float waiterTakeOrderSpeedBonusPerLevel = 0.16f;
        [SerializeField, Min(0f)] private float waiterSubmitOrderSpeedBonusPerLevel = 0.15f;
        [SerializeField, Min(0f)] private float waiterPickupSpeedBonusPerLevel = 0.14f;
        [SerializeField, Min(0f)] private float waiterCharismaTipBonusPerLevel = 0.12f;
        [SerializeField, Min(0f)] private float waiterCharismaLoyaltyBonusPerLevel = 0.5f;
        [SerializeField, Min(0f)] private float kitchenSpeedBonusPerLevel = 0.16f;
        [SerializeField, Min(0f)] private float barSpeedBonusPerLevel = 0.14f;
        [SerializeField, Min(0)] private int kitchenSpeedLevel;
        [SerializeField, Min(0)] private int barSpeedLevel;

        [Header("Offline Income")]
        [SerializeField] private bool enableOfflineIncome = true;
        [SerializeField, Range(0f, 0.2f)] private float offlineIncomeFactor = 0.01f;
        [SerializeField, Min(0f)] private float maxOfflineHours = 4f;
        [SerializeField] private bool enableOfflineSoftCap = true;
        [SerializeField, Min(0)] private int offlineBaseSoftCap = 250;
        [SerializeField, Min(0)] private int offlineSoftCapPerStage = 180;
        [SerializeField, Range(0f, 1f)] private float offlineOvercapEfficiency = 0.25f;

        [Header("Options")]
        [SerializeField] private bool autoFindReferences = true;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool verboseLogs = true;

        private readonly List<RestaurantGuest> activeGuests = new List<RestaurantGuest>();
        private readonly List<RestaurantGuest> queuedGuests = new List<RestaurantGuest>();
        private readonly Dictionary<RestaurantGuest, float> queuedGuestWaitDurations = new Dictionary<RestaurantGuest, float>();
        private readonly HashSet<RestaurantGuest> queuedGuestWarned = new HashSet<RestaurantGuest>();
        private float nextSpawnAt;
        private bool isInitialized;
        private bool hasPendingOfflineIncomeReport;
        private OfflineIncomeReport pendingOfflineIncomeReport;

        public event Action<string, RestaurantNotificationType> NotificationRaised;
        public event Action StateChanged;
        public event Action<RestaurantRuntimeSignalType, RestaurantSeat> SignalRaised;

        public int TotalMoney => totalMoney;

        public int TotalTips => totalTips;

        public int LifetimeOrderRevenue => lifetimeOrderRevenue;

        public int ServedGuests => servedGuests;

        public int WalkedOutGuests => walkedOutGuests;

        public int QueueWalkedOutGuests => queueWalkedOutGuests;

        public int LoyaltyScore => loyaltyScore;

        public int ActiveGuestCount => activeGuests.Count;

        public int QueueGuestCount => queuedGuests.Count;

        public int ConfiguredTableCount => tables.Count;

        public WaiterAgent Waiter => waiter;

        public IReadOnlyList<RestaurantTable> Tables => tables;

        public IReadOnlyList<RestaurantPoint> ServicePoints => servicePoints;

        public int SessionIncome => lifetimeOrderRevenue + totalTips;

        public float AverageCheck => servedGuests > 0 ? lifetimeOrderRevenue / (float)servedGuests : 0f;

        public float AverageTip => servedGuests > 0 ? totalTips / (float)servedGuests : 0f;

        public int TableRevenueLevel => tableRevenueLevel;

        public int WaiterSpeedLevel => waiterSpeedLevel;

        public int WaiterTakeOrderSpeedLevel => waiterTakeOrderSpeedLevel;

        public int WaiterSubmitOrderSpeedLevel => waiterSubmitOrderSpeedLevel;

        public int WaiterPickupSpeedLevel => waiterPickupSpeedLevel;

        public int WaiterCharismaLevel => waiterCharismaLevel;

        public int KitchenSpeedLevel => kitchenSpeedLevel;

        public int BarSpeedLevel => barSpeedLevel;

        public float TableRevenueMultiplier => 1f + tableRevenueLevel * tableRevenueBonusPerLevel;

        public float TableTipMultiplier => 1f + tableRevenueLevel * tableTipBonusPerLevel;

        public float WaiterSpeedMultiplier => 1f + waiterSpeedLevel * waiterSpeedBonusPerLevel;

        public float WaiterTakeOrderSpeedMultiplier => 1f + waiterTakeOrderSpeedLevel * waiterTakeOrderSpeedBonusPerLevel;

        public float WaiterSubmitOrderSpeedMultiplier => 1f + waiterSubmitOrderSpeedLevel * waiterSubmitOrderSpeedBonusPerLevel;

        public float WaiterPickupSpeedMultiplier => 1f + waiterPickupSpeedLevel * waiterPickupSpeedBonusPerLevel;

        public float WaiterCharismaTipMultiplier => 1f + waiterCharismaLevel * waiterCharismaTipBonusPerLevel;

        public int WaiterCharismaLoyaltyBonus => waiterCharismaLevel > 0
            ? Mathf.Max(1, Mathf.CeilToInt(waiterCharismaLevel * waiterCharismaLoyaltyBonusPerLevel))
            : 0;

        public float KitchenSpeedMultiplier => 1f + kitchenSpeedLevel * kitchenSpeedBonusPerLevel;

        public float BarSpeedMultiplier => 1f + barSpeedLevel * barSpeedBonusPerLevel;

        public int NextTableRevenueUpgradeCost => EvaluateUpgradeCost(baseTableRevenueUpgradeCost, tableRevenueUpgradeCostGrowth, tableRevenueLevel);

        public int NextWaiterSpeedUpgradeCost => EvaluateUpgradeCost(baseWaiterSpeedUpgradeCost, waiterSpeedUpgradeCostGrowth, waiterSpeedLevel);

        public int NextWaiterTakeOrderSpeedUpgradeCost => EvaluateUpgradeCost(baseWaiterTakeOrderSpeedUpgradeCost, waiterTakeOrderSpeedUpgradeCostGrowth, waiterTakeOrderSpeedLevel);

        public int NextWaiterSubmitOrderSpeedUpgradeCost => EvaluateUpgradeCost(baseWaiterSubmitOrderSpeedUpgradeCost, waiterSubmitOrderSpeedUpgradeCostGrowth, waiterSubmitOrderSpeedLevel);

        public int NextWaiterPickupSpeedUpgradeCost => EvaluateUpgradeCost(baseWaiterPickupSpeedUpgradeCost, waiterPickupSpeedUpgradeCostGrowth, waiterPickupSpeedLevel);

        public int NextWaiterCharismaUpgradeCost => EvaluateUpgradeCost(baseWaiterCharismaUpgradeCost, waiterCharismaUpgradeCostGrowth, waiterCharismaLevel);

        public int NextKitchenSpeedUpgradeCost => EvaluateUpgradeCost(baseKitchenSpeedUpgradeCost, kitchenSpeedUpgradeCostGrowth, kitchenSpeedLevel);

        public int NextBarSpeedUpgradeCost => EvaluateUpgradeCost(baseBarSpeedUpgradeCost, barSpeedUpgradeCostGrowth, barSpeedLevel);

        public bool IsInitialized => isInitialized;

        public WaiterPriorityMode WaiterPriority => waiterPriorityMode;

        public string WaiterPriorityLabel => GetWaiterPriorityLabel(waiterPriorityMode);

        private void Awake()
        {
            if (autoFindReferences)
            {
                CollectSceneReferences();
            }
        }

        private void Start()
        {
            if (!autoStart)
            {
                return;
            }

            Initialize();
        }

        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            ProcessQueue();

            if (Time.time < nextSpawnAt)
            {
                return;
            }

            bool hasFreeSeat = HasFreeSeat();
            if (!TrySpawnGuest())
            {
                if (!hasFreeSeat)
                {
                    TryEnqueueGuest();
                }

                nextSpawnAt = Time.time + 1f;
                return;
            }

            nextSpawnAt = Time.time + Random.Range(spawnDelayRange.x, spawnDelayRange.y);
        }

        [ContextMenu("Collect Scene References")]
        public void CollectSceneReferences()
        {
            waiter = waiter != null ? waiter : FindAnyObjectByType<WaiterAgent>();

            tables = FindObjectsByType<RestaurantTable>(FindObjectsSortMode.None)
                .OrderBy(table => table.name)
                .ToList();

            for (int index = 0; index < tables.Count; index++)
            {
                if (tables[index] != null)
                {
                    tables[index].RefreshSeats();
                }
            }

            servicePoints = FindObjectsByType<RestaurantPoint>(FindObjectsSortMode.None)
                .OrderBy(point => point.PointType)
                .ThenBy(point => point.name)
                .ToList();
        }

        public void Initialize()
        {
            if (autoFindReferences)
            {
                CollectSceneReferences();
            }

            isInitialized = ValidateSetup();
            if (!isInitialized)
            {
                return;
            }

            RestaurantNavigationBootstrap.EnsureBuilt(this);

            activeGuests.Clear();
            queuedGuests.Clear();
            queuedGuestWaitDurations.Clear();
            queuedGuestWarned.Clear();
            hasPendingOfflineIncomeReport = false;
            pendingOfflineIncomeReport = default;
            LoadProgress();
            TryApplyOfflineIncome();
            nextSpawnAt = Time.time + 0.5f;
            waiter.BindRuntime(this);
            ApplyWaiterUpgrades();
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.Initialized, null);
        }

        public WaiterPriorityMode CycleWaiterPriorityMode()
        {
            switch (waiterPriorityMode)
            {
                case WaiterPriorityMode.Balanced:
                    waiterPriorityMode = WaiterPriorityMode.Speed;
                    break;
                case WaiterPriorityMode.Speed:
                    waiterPriorityMode = WaiterPriorityMode.TipFocus;
                    break;
                default:
                    waiterPriorityMode = WaiterPriorityMode.Balanced;
                    break;
            }

            SaveProgress();
            RaiseNotification(
                LocalizationService.Format("rest.notify.waiter_mode", GetWaiterPriorityLabel(waiterPriorityMode)),
                RestaurantNotificationType.Info);
            return waiterPriorityMode;
        }

        public bool TryConsumeOfflineIncomeReport(out OfflineIncomeReport report)
        {
            if (!hasPendingOfflineIncomeReport)
            {
                report = default;
                return false;
            }

            report = pendingOfflineIncomeReport;
            hasPendingOfflineIncomeReport = false;
            pendingOfflineIncomeReport = default;
            return true;
        }

        public void ShowPlayerNotification(string message, RestaurantNotificationType notificationType = RestaurantNotificationType.Info)
        {
            RaiseNotification(message, notificationType);
        }

        public void ApplyExternalRestaurantReward(int coins, int loyalty, string sourceLabel = null)
        {
            int normalizedCoins = Mathf.Max(0, coins);
            if (normalizedCoins <= 0 && loyalty == 0)
            {
                return;
            }

            totalMoney += normalizedCoins;
            loyaltyScore += loyalty;
            SaveProgress();
            StateChanged?.Invoke();

            string rewardSource = string.IsNullOrWhiteSpace(sourceLabel)
                ? LocalizationService.Get("rest.notify.meta_reward")
                : sourceLabel;
            string rewardMessage = rewardSource;
            if (normalizedCoins > 0)
            {
                rewardMessage += " +$" + normalizedCoins;
            }

            if (loyalty != 0)
            {
                if (normalizedCoins > 0)
                {
                    rewardMessage += "  ";
                }

                rewardMessage += loyalty > 0
                    ? LocalizationService.Format("rest.notify.loyalty_positive", loyalty)
                    : LocalizationService.Format("rest.notify.loyalty_negative", loyalty);
            }

            RaiseNotification(rewardMessage, RestaurantNotificationType.Success);
            Log(
                rewardSource +
                " applied. Coins: +" + normalizedCoins +
                ", loyalty: " + loyalty + ".");
        }

        public bool TryGetPoint(RestaurantPointType pointType, out RestaurantPoint point)
        {
            for (int index = 0; index < servicePoints.Count; index++)
            {
                if (servicePoints[index] != null && servicePoints[index].PointType == pointType)
                {
                    point = servicePoints[index];
                    return true;
                }
            }

            point = null;
            return false;
        }

        public int GetSeatCountByStatus(RestaurantSeatStatus status)
        {
            int count = 0;
            List<RestaurantSeat> seats = GetAllSeats();
            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index] != null && seats[index].Status == status)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetTableCountByDisplayStatus(RestaurantSeatStatus status)
        {
            int count = 0;
            for (int index = 0; index < tables.Count; index++)
            {
                if (tables[index] != null && tables[index].GetDisplayStatus() == status)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetAwaitingOrderSubmissionCount()
        {
            return GetSeatCountByStatus(RestaurantSeatStatus.AwaitingOrderSubmission);
        }

        public int GetWaitingBillCount()
        {
            return GetSeatCountByStatus(RestaurantSeatStatus.WaitingBill);
        }

        public int GetCleanupTableCount()
        {
            return GetTableCountByDisplayStatus(RestaurantSeatStatus.NeedsCleanup);
        }

        public int GetKitchenPreparingCount()
        {
            return CountOrders(order => order.RequiresKitchen && order.IsSubmitted && !order.KitchenPickedUp && !order.CanPickupKitchen(Time.time));
        }

        public int GetKitchenReadyOrderCount()
        {
            return CountOrders(order => order.RequiresKitchen && order.IsSubmitted && !order.KitchenPickedUp && order.CanPickupKitchen(Time.time));
        }

        public int GetBarPreparingCount()
        {
            return CountOrders(order => order.RequiresBar && order.IsSubmitted && !order.BarPickedUp && !order.CanPickupBar(Time.time));
        }

        public int GetBarReadyOrderCount()
        {
            return CountOrders(order => order.RequiresBar && order.IsSubmitted && !order.BarPickedUp && order.CanPickupBar(Time.time));
        }

        public bool TryGetNextTask(out WaiterTask task)
        {
            List<RestaurantSeat> seats = GetAllSeats();

            if (TryGetUrgencyOverrideTask(seats, out task))
            {
                return true;
            }

            switch (waiterPriorityMode)
            {
                case WaiterPriorityMode.Speed:
                    return TryGetSpeedTask(seats, out task);
                case WaiterPriorityMode.TipFocus:
                    return TryGetTipFocusedTask(seats, out task);
                default:
                    return TryGetBalancedTask(seats, out task);
            }
        }

        private bool TryGetBalancedTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            if (TryGetDeliverTask(seats, out task))
            {
                return true;
            }

            if (TryGetCleanupTask(seats, out task))
            {
                return true;
            }

            if (TryGetProcessBillTask(seats, out task))
            {
                return true;
            }

            if (TryGetSubmitOrderTask(seats, out task))
            {
                return true;
            }

            if (TryGetPickupTask(seats, out task))
            {
                return true;
            }

            if (TryGetTakeOrderTask(seats, out task))
            {
                return true;
            }

            task = default;
            return false;
        }

        private bool TryGetSpeedTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            if (TryGetProcessBillTask(seats, out task))
            {
                return true;
            }

            if (TryGetCleanupTask(seats, out task))
            {
                return true;
            }

            if (TryGetDeliverTask(seats, out task))
            {
                return true;
            }

            if (TryGetSubmitOrderTask(seats, out task))
            {
                return true;
            }

            if (TryGetPickupTask(seats, out task))
            {
                return true;
            }

            if (TryGetTakeOrderTask(seats, out task))
            {
                return true;
            }

            task = default;
            return false;
        }

        private bool TryGetTipFocusedTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            if (TryGetTakeOrderTask(seats, out task))
            {
                return true;
            }

            if (TryGetSubmitOrderTask(seats, out task))
            {
                return true;
            }

            if (TryGetPickupTask(seats, out task))
            {
                return true;
            }

            if (TryGetDeliverTask(seats, out task))
            {
                return true;
            }

            if (TryGetProcessBillTask(seats, out task))
            {
                return true;
            }

            if (TryGetCleanupTask(seats, out task))
            {
                return true;
            }

            task = default;
            return false;
        }

        public void HandleOrderTaken(RestaurantSeat seat)
        {
            if (seat == null || seat.Status != RestaurantSeatStatus.WaitingForOrder)
            {
                return;
            }

            OrderTicket order = CreateOrderTicket();
            seat.SetOrder(order);

            if (seat.CurrentGuest != null)
            {
                seat.CurrentGuest.BeginDeliveryWait(deliveryWaitLimit, tipDecayStartNormalized);
            }

            RaiseRuntimeSignal(RestaurantRuntimeSignalType.OrderTaken, seat);
            Log("Order taken at " + GetSeatLabel(seat) + " (kitchen: " + order.RequiresKitchen + ", bar: " + order.RequiresBar + ").");
        }

        public void HandleOrderSubmitted(RestaurantSeat seat)
        {
            if (seat == null || seat.ActiveOrder == null)
            {
                return;
            }

            OrderTicket order = seat.ActiveOrder;
            float kitchenDelay = Random.Range(kitchenReadyDelayRange.x, kitchenReadyDelayRange.y) / Mathf.Max(0.01f, KitchenSpeedMultiplier);
            float barDelay = Random.Range(barReadyDelayRange.x, barReadyDelayRange.y) / Mathf.Max(0.01f, BarSpeedMultiplier);
            order.Submit(
                Time.time,
                Mathf.Max(0.25f, kitchenDelay),
                Mathf.Max(0.25f, barDelay));
            seat.MarkAwaitingPickup();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.OrderSubmitted, seat);
            Log("Order submitted for " + GetSeatLabel(seat) + ".");
        }

        public void HandleKitchenPickedUp(RestaurantSeat seat)
        {
            if (seat == null || seat.ActiveOrder == null)
            {
                return;
            }

            seat.ActiveOrder.MarkKitchenPickedUp();
            seat.RefreshDeliveryState();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.KitchenPickedUp, seat);
            Log("Kitchen order picked up for " + GetSeatLabel(seat) + ".");
        }

        public void HandleBarPickedUp(RestaurantSeat seat)
        {
            if (seat == null || seat.ActiveOrder == null)
            {
                return;
            }

            seat.ActiveOrder.MarkBarPickedUp();
            seat.RefreshDeliveryState();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.BarPickedUp, seat);
            Log("Bar order picked up for " + GetSeatLabel(seat) + ".");
        }

        public void HandleOrderDelivered(RestaurantSeat seat)
        {
            if (seat == null || seat.ActiveOrder == null)
            {
                return;
            }

            seat.ActiveOrder.MarkDelivered();
            seat.MarkEating();

            if (seat.CurrentGuest != null)
            {
                seat.CurrentGuest.BeginEating(Random.Range(eatingDurationRange.x, eatingDurationRange.y));
            }

            RaiseRuntimeSignal(RestaurantRuntimeSignalType.OrderDelivered, seat);
            Log("Order delivered to " + GetSeatLabel(seat) + ".");
        }

        public void HandleCleanupCompleted(RestaurantSeat seat)
        {
            if (seat == null)
            {
                return;
            }

            seat.MarkWaitingBill();

            if (seat.CurrentGuest != null)
            {
                seat.CurrentGuest.BeginBillWait(billWaitLimit, tipDecayStartNormalized);
            }

            RaiseRuntimeSignal(RestaurantRuntimeSignalType.CleanupCompleted, seat);
            Log("Cleanup complete for " + GetSeatLabel(seat) + ".");
        }

        public void HandleBillDelivered(RestaurantSeat seat)
        {
            if (seat == null || seat.ActiveOrder == null)
            {
                return;
            }

            float serviceQuality = 1f;
            if (seat.CurrentGuest != null)
            {
                seat.CurrentGuest.CompleteActiveWaitPhase();
                serviceQuality = seat.CurrentGuest.ServiceQuality;
            }

            float tipMultiplier = ServiceQualityModel.EvaluateTipMultiplier(serviceQuality) * WaiterCharismaTipMultiplier;
            seat.ActiveOrder.ApplyTipMultiplier(tipMultiplier);
            int loyaltyDelta = ServiceQualityModel.EvaluateLoyaltyDelta(serviceQuality, false) + WaiterCharismaLoyaltyBonus;
            loyaltyScore += loyaltyDelta;

            int payout = seat.ActiveOrder.TotalPrice + seat.ActiveOrder.TipAmount;
            totalMoney += payout;
            totalTips += seat.ActiveOrder.TipAmount;
            lifetimeOrderRevenue += seat.ActiveOrder.TotalPrice;
            servedGuests++;
            SaveProgress();

            seat.MarkGuestLeaving();

            if (seat.CurrentGuest != null)
            {
                seat.CurrentGuest.BeginLeaving();
            }

            Log(ServiceQualityModel.BuildSettlementLog(
                GetSeatLabel(seat),
                serviceQuality,
                tipMultiplier,
                seat.ActiveOrder.BaseTipAmount,
                seat.ActiveOrder.TipAmount,
                loyaltyDelta));
            Log("Bill paid at " + GetSeatLabel(seat) + ". Earned: " + seat.ActiveOrder.TotalPrice + ", tips: " + seat.ActiveOrder.TipAmount + ".");
            RaiseNotification(
                LocalizationService.Format("rest.notify.income_tips", payout, seat.ActiveOrder.TipAmount),
                RestaurantNotificationType.Success);
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.BillDelivered, seat);
        }

        public void NotifyGuestFinishedEating(RestaurantGuest guest)
        {
            if (guest == null || guest.CurrentSeat == null)
            {
                return;
            }

            guest.CurrentSeat.MarkNeedsCleanup();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.CleanupNeeded, guest.CurrentSeat);
            Log("Guest finished eating at " + GetSeatLabel(guest.CurrentSeat) + ".");
        }

        public void NotifyGuestSeated(RestaurantGuest guest)
        {
            if (guest == null || guest.CurrentSeat == null)
            {
                return;
            }

            guest.BeginOrderWait(orderWaitLimit, tipDecayStartNormalized, allowGuestWalkoutBeforeOrder);
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.GuestSeated, guest.CurrentSeat);
            Log("Guest is waiting to order at " + GetSeatLabel(guest.CurrentSeat) + ".");
        }

        public void NotifyGuestWalkedOutBeforeOrdering(RestaurantGuest guest)
        {
            if (guest == null || guest.CurrentSeat == null)
            {
                return;
            }

            RestaurantSeat seat = guest.CurrentSeat;
            if (seat.ActiveOrder != null || seat.Status != RestaurantSeatStatus.WaitingForOrder)
            {
                return;
            }

            walkedOutGuests++;
            loyaltyScore += ServiceQualityModel.EvaluateLoyaltyDelta(0f, true);
            SaveProgress();
            seat.MarkGuestLeaving();
            guest.BeginLeaving();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.GuestWalkedOut, seat);
            Log("Guest walked out before ordering at " + GetSeatLabel(seat) + ".");
            RaiseNotification(LocalizationService.Get("rest.notify.guest_walked_out"), RestaurantNotificationType.Warning);
        }

        public void NotifyGuestExited(RestaurantGuest guest)
        {
            if (guest == null)
            {
                return;
            }

            RestaurantSeat seat = guest.CurrentSeat;
            if (seat != null)
            {
                seat.ClearSeat();
            }

            activeGuests.Remove(guest);
            queuedGuests.Remove(guest);
            queuedGuestWaitDurations.Remove(guest);
            queuedGuestWarned.Remove(guest);
            RefreshQueuePositions();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.GuestExited, seat);
            Log("Guest left the restaurant.");
        }

        private void ProcessQueue()
        {
            if (!enableGuestQueue)
            {
                if (queuedGuests.Count > 0)
                {
                    for (int index = 0; index < queuedGuests.Count; index++)
                    {
                        if (queuedGuests[index] != null)
                        {
                            queuedGuests[index].BeginLeaving();
                        }
                    }

                    queuedGuests.Clear();
                    queuedGuestWaitDurations.Clear();
                    queuedGuestWarned.Clear();
                    RaiseRuntimeSignal(RestaurantRuntimeSignalType.QueueChanged, null);
                }

                return;
            }

            RemoveInvalidQueueGuests();
            UpdateQueuePatience();
            if (queuedGuests.Count == 0)
            {
                return;
            }

            while (queuedGuests.Count > 0)
            {
                RestaurantSeat seat = GetRandomFreeSeat();
                if (seat == null || activeGuests.Count >= maxActiveGuests)
                {
                    break;
                }

                RestaurantGuest queuedGuest = queuedGuests[0];
                queuedGuests.RemoveAt(0);
                if (queuedGuest == null)
                {
                    continue;
                }

                seat.Reserve(queuedGuest);
                queuedGuest.AssignSeat(seat);
                activeGuests.Add(queuedGuest);
                queuedGuestWaitDurations.Remove(queuedGuest);
                queuedGuestWarned.Remove(queuedGuest);
                RaiseRuntimeSignal(RestaurantRuntimeSignalType.GuestAssignedSeat, seat);
                Log("Queued guest moved to " + GetSeatLabel(seat) + ".");
            }

            RefreshQueuePositions();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.QueueChanged, null);
        }

        private void RemoveInvalidQueueGuests()
        {
            for (int index = queuedGuests.Count - 1; index >= 0; index--)
            {
                RestaurantGuest guest = queuedGuests[index];
                if (guest != null)
                {
                    continue;
                }

                queuedGuests.RemoveAt(index);
            }

            List<RestaurantGuest> knownGuests = new List<RestaurantGuest>(queuedGuestWaitDurations.Keys);
            for (int index = 0; index < knownGuests.Count; index++)
            {
                RestaurantGuest guest = knownGuests[index];
                if (guest == null || !queuedGuests.Contains(guest))
                {
                    queuedGuestWaitDurations.Remove(guest);
                    queuedGuestWarned.Remove(guest);
                }
            }
        }

        private void UpdateQueuePatience()
        {
            if (queueWaitLimit <= 0.01f)
            {
                return;
            }

            bool anyWalkout = false;
            float warningThreshold = Mathf.Clamp01(queueWaitWarningThreshold);

            for (int index = queuedGuests.Count - 1; index >= 0; index--)
            {
                RestaurantGuest guest = queuedGuests[index];
                if (guest == null)
                {
                    continue;
                }

                float elapsed;
                queuedGuestWaitDurations.TryGetValue(guest, out elapsed);
                elapsed += Time.deltaTime;
                queuedGuestWaitDurations[guest] = elapsed;

                float normalized = Mathf.Clamp01(elapsed / queueWaitLimit);
                if (normalized >= warningThreshold && !queuedGuestWarned.Contains(guest))
                {
                    queuedGuestWarned.Add(guest);
                    Log("Queue guest is getting impatient (" + Mathf.RoundToInt(normalized * 100f) + "%).");
                }

                if (elapsed < queueWaitLimit)
                {
                    continue;
                }

                queueWalkedOutGuests++;
                walkedOutGuests++;
                loyaltyScore += ServiceQualityModel.EvaluateLoyaltyDelta(0f, true);
                queuedGuests.RemoveAt(index);
                queuedGuestWaitDurations.Remove(guest);
                queuedGuestWarned.Remove(guest);
                guest.BeginLeaving();
                anyWalkout = true;

                Log("Queue guest walked out after waiting " + Mathf.RoundToInt(elapsed) + " sec.");
                RaiseNotification(LocalizationService.Get("rest.notify.queue_guest_left"), RestaurantNotificationType.Warning);
            }

            if (anyWalkout)
            {
                SaveProgress();
                RefreshQueuePositions();
                RaiseRuntimeSignal(RestaurantRuntimeSignalType.QueueChanged, null);
            }
        }

        private bool TryEnqueueGuest()
        {
            if (!enableGuestQueue || queuedGuests.Count >= queueCapacity)
            {
                return false;
            }

            RestaurantPoint spawnPoint;
            if (!TryGetPoint(RestaurantPointType.GuestSpawn, out spawnPoint) || spawnPoint == null)
            {
                return false;
            }

            RestaurantPoint exitPoint = spawnPoint;
            RestaurantPoint dedicatedExitPoint;
            if (TryGetPoint(RestaurantPointType.GuestExit, out dedicatedExitPoint) && dedicatedExitPoint != null)
            {
                exitPoint = dedicatedExitPoint;
            }

            RestaurantGuest guest = CreateGuest(spawnPoint.WorldPosition);
            guest.Initialize(this, null, exitPoint);
            queuedGuests.Add(guest);
            queuedGuestWaitDurations[guest] = 0f;
            queuedGuestWarned.Remove(guest);
            guest.BeginQueueing(GetQueuePosition(spawnPoint, queuedGuests.Count - 1));
            RefreshQueuePositions();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.QueueChanged, null);
            Log("Guest queued at entrance. Queue: " + queuedGuests.Count + "/" + queueCapacity + ".");
            return true;
        }

        private void RefreshQueuePositions()
        {
            RestaurantPoint spawnPoint;
            if (!TryGetPoint(RestaurantPointType.GuestSpawn, out spawnPoint) || spawnPoint == null)
            {
                return;
            }

            for (int index = 0; index < queuedGuests.Count; index++)
            {
                if (queuedGuests[index] == null)
                {
                    continue;
                }

                queuedGuests[index].RefreshQueuePosition(GetQueuePosition(spawnPoint, index));
            }
        }

        private Vector3 GetQueuePosition(RestaurantPoint spawnPoint, int queueIndex)
        {
            Vector3 rawDirection = queueDirection;
            rawDirection.y = 0f;

            if (rawDirection.sqrMagnitude < 0.0001f)
            {
                rawDirection = -spawnPoint.transform.forward;
                rawDirection.y = 0f;
            }

            Vector3 direction = rawDirection.sqrMagnitude > 0.0001f ? rawDirection.normalized : Vector3.back;
            return spawnPoint.WorldPosition + direction * Mathf.Max(1, queueIndex + 1) * queueSpacing;
        }

        private bool TrySpawnGuest()
        {
            if (activeGuests.Count >= maxActiveGuests)
            {
                return false;
            }

            RestaurantSeat seat = GetRandomFreeSeat();
            if (seat == null)
            {
                return false;
            }

            RestaurantPoint spawnPoint;
            if (!TryGetPoint(RestaurantPointType.GuestSpawn, out spawnPoint) || spawnPoint == null)
            {
                return false;
            }

            RestaurantPoint exitPoint = spawnPoint;
            RestaurantPoint dedicatedExitPoint;
            if (TryGetPoint(RestaurantPointType.GuestExit, out dedicatedExitPoint) && dedicatedExitPoint != null)
            {
                exitPoint = dedicatedExitPoint;
            }

            RestaurantGuest guest = CreateGuest(spawnPoint.WorldPosition);
            seat.Reserve(guest);
            guest.Initialize(this, seat, exitPoint);
            activeGuests.Add(guest);

            RaiseRuntimeSignal(RestaurantRuntimeSignalType.GuestAssignedSeat, seat);
            Log("Guest spawned for " + GetSeatLabel(seat) + ".");
            return true;
        }

        private RestaurantSeat GetRandomFreeSeat()
        {
            List<RestaurantSeat> freeSeats = new List<RestaurantSeat>();
            for (int tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                if (tables[tableIndex] == null)
                {
                    continue;
                }

                // Shared seating is temporarily disabled.
                RestaurantSeat seat = tables[tableIndex].GetAvailableSeat();
                if (seat != null)
                {
                    freeSeats.Add(seat);
                }
            }

            if (freeSeats.Count == 0)
            {
                return null;
            }

            int index = Random.Range(0, freeSeats.Count);
            return freeSeats[index];
        }

        private bool HasFreeSeat()
        {
            for (int tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                if (tables[tableIndex] == null)
                {
                    continue;
                }

                if (tables[tableIndex].GetAvailableSeat() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private RestaurantGuest CreateGuest(Vector3 spawnPosition)
        {
            RestaurantGuest guest;
            Vector3 guestScale = ResolveGuestScale();
            if (guestPrefab != null)
            {
                guest = Instantiate(guestPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                GameObject guestObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                guestObject.name = "Guest";
                guestObject.transform.localScale = guestScale;
                guestObject.transform.position = new Vector3(spawnPosition.x, guestScale.y, spawnPosition.z);

                Renderer renderer = guestObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(Random.Range(0.35f, 0.95f), Random.Range(0.35f, 0.95f), Random.Range(0.35f, 0.95f));
                }

                guest = guestObject.AddComponent<RestaurantGuest>();
            }

            guest.name = "Guest_" + (servedGuests + activeGuests.Count + 1).ToString("00");
            return guest;
        }

        private OrderTicket CreateOrderTicket()
        {
            bool requiresKitchen = Random.value > 0.2f;
            bool requiresBar = Random.value > 0.35f;

            if (!requiresKitchen && !requiresBar)
            {
                requiresKitchen = true;
            }

            int price = Random.Range(orderPriceRange.x, orderPriceRange.y + 1);
            int tip = Random.Range(tipRange.x, tipRange.y + 1);
            price = Mathf.Max(1, Mathf.RoundToInt(price * TableRevenueMultiplier));
            tip = Mathf.Max(0, Mathf.RoundToInt(tip * TableTipMultiplier));
            return new OrderTicket(requiresKitchen, requiresBar, price, tip);
        }

        public bool TryPurchaseTableRevenueUpgrade()
        {
            int cost = NextTableRevenueUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            tableRevenueLevel++;
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased table revenue upgrade. Level: " + tableRevenueLevel + ".");
            RaiseNotification(
                LocalizationService.Format("rest.notify.tables_upgraded", tableRevenueLevel, TableRevenueMultiplier.ToString("0.00")),
                RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseWaiterSpeedUpgrade()
        {
            int cost = NextWaiterSpeedUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            waiterSpeedLevel++;
            ApplyWaiterUpgrades();
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased waiter speed upgrade. Level: " + waiterSpeedLevel + ".");
            RaiseNotification(BuildWaiterUpgradeNotification(GetWaiterMoveUpgradeTitle(), waiterSpeedLevel, WaiterSpeedMultiplier), RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseWaiterTakeOrderSpeedUpgrade()
        {
            int cost = NextWaiterTakeOrderSpeedUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            waiterTakeOrderSpeedLevel++;
            ApplyWaiterUpgrades();
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased waiter order-taking speed upgrade. Level: " + waiterTakeOrderSpeedLevel + ".");
            RaiseNotification(BuildWaiterUpgradeNotification(GetWaiterTakeOrderUpgradeTitle(), waiterTakeOrderSpeedLevel, WaiterTakeOrderSpeedMultiplier), RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseWaiterSubmitOrderSpeedUpgrade()
        {
            int cost = NextWaiterSubmitOrderSpeedUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            waiterSubmitOrderSpeedLevel++;
            ApplyWaiterUpgrades();
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased waiter order-submitting speed upgrade. Level: " + waiterSubmitOrderSpeedLevel + ".");
            RaiseNotification(BuildWaiterUpgradeNotification(GetWaiterSubmitUpgradeTitle(), waiterSubmitOrderSpeedLevel, WaiterSubmitOrderSpeedMultiplier), RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseWaiterPickupSpeedUpgrade()
        {
            int cost = NextWaiterPickupSpeedUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            waiterPickupSpeedLevel++;
            ApplyWaiterUpgrades();
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased waiter pickup speed upgrade. Level: " + waiterPickupSpeedLevel + ".");
            RaiseNotification(BuildWaiterUpgradeNotification(GetWaiterPickupUpgradeTitle(), waiterPickupSpeedLevel, WaiterPickupSpeedMultiplier), RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseWaiterCharismaUpgrade()
        {
            int cost = NextWaiterCharismaUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            waiterCharismaLevel++;
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased waiter charisma upgrade. Level: " + waiterCharismaLevel + ".");
            RaiseNotification(BuildWaiterCharismaUpgradeNotification(), RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseKitchenSpeedUpgrade()
        {
            int cost = NextKitchenSpeedUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            kitchenSpeedLevel++;
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased kitchen speed upgrade. Level: " + kitchenSpeedLevel + ".");
            RaiseNotification(
                LocalizationService.Format("rest.notify.kitchen_upgraded", kitchenSpeedLevel, KitchenSpeedMultiplier.ToString("0.00")),
                RestaurantNotificationType.Success);
            return true;
        }

        public bool TryPurchaseBarSpeedUpgrade()
        {
            int cost = NextBarSpeedUpgradeCost;
            if (totalMoney < cost)
            {
                return false;
            }

            totalMoney -= cost;
            barSpeedLevel++;
            SaveProgress();
            RaiseRuntimeSignal(RestaurantRuntimeSignalType.UpgradeChanged, null);
            Log("Purchased bar speed upgrade. Level: " + barSpeedLevel + ".");
            RaiseNotification(
                LocalizationService.Format("rest.notify.bar_upgraded", barSpeedLevel, BarSpeedMultiplier.ToString("0.00")),
                RestaurantNotificationType.Success);
            return true;
        }

        public bool CanAffordTableRevenueUpgrade()
        {
            return totalMoney >= NextTableRevenueUpgradeCost;
        }

        public bool CanAffordWaiterSpeedUpgrade()
        {
            return totalMoney >= NextWaiterSpeedUpgradeCost;
        }

        public bool CanAffordWaiterTakeOrderSpeedUpgrade()
        {
            return totalMoney >= NextWaiterTakeOrderSpeedUpgradeCost;
        }

        public bool CanAffordWaiterSubmitOrderSpeedUpgrade()
        {
            return totalMoney >= NextWaiterSubmitOrderSpeedUpgradeCost;
        }

        public bool CanAffordWaiterPickupSpeedUpgrade()
        {
            return totalMoney >= NextWaiterPickupSpeedUpgradeCost;
        }

        public bool CanAffordWaiterCharismaUpgrade()
        {
            return totalMoney >= NextWaiterCharismaUpgradeCost;
        }

        public bool CanAffordKitchenSpeedUpgrade()
        {
            return totalMoney >= NextKitchenSpeedUpgradeCost;
        }

        public bool CanAffordBarSpeedUpgrade()
        {
            return totalMoney >= NextBarSpeedUpgradeCost;
        }

        private Vector3 ResolveGuestScale()
        {
            if (waiter != null)
            {
                Vector3 waiterScale = waiter.transform.localScale;
                if (waiterScale.x > 0f && waiterScale.y > 0f && waiterScale.z > 0f)
                {
                    return waiterScale;
                }
            }

            return fallbackGuestScale;
        }

        private List<RestaurantSeat> GetAllSeats()
        {
            List<RestaurantSeat> result = new List<RestaurantSeat>();

            for (int tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                if (tables[tableIndex] == null)
                {
                    continue;
                }

                IReadOnlyList<RestaurantSeat> tableSeats = tables[tableIndex].Seats;
                for (int seatIndex = 0; seatIndex < tableSeats.Count; seatIndex++)
                {
                    if (tableSeats[seatIndex] != null)
                    {
                        result.Add(tableSeats[seatIndex]);
                    }
                }
            }

            return result;
        }

        private bool TryGetUrgencyOverrideTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            float threshold = Mathf.Clamp01(urgencyOverrideThreshold);
            float bestUrgency = threshold;
            WaiterTaskType bestType = WaiterTaskType.None;
            RestaurantSeat bestSeat = null;

            for (int index = 0; index < seats.Count; index++)
            {
                RestaurantSeat seat = seats[index];
                if (seat == null)
                {
                    continue;
                }

                WaiterTaskType type;
                if (!TryMapSeatToUrgentTask(seat, out type))
                {
                    continue;
                }

                float urgency = Mathf.Clamp01(seat.GuestUrgencyNormalized);
                if (urgency < bestUrgency)
                {
                    continue;
                }

                if (bestSeat == null || urgency > bestUrgency || ShouldPreferUrgentTask(type, bestType))
                {
                    bestSeat = seat;
                    bestType = type;
                    bestUrgency = urgency;
                }
            }

            if (bestSeat == null || bestType == WaiterTaskType.None)
            {
                task = default;
                return false;
            }

            task = new WaiterTask(bestType, bestSeat);
            return true;
        }

        private static bool ShouldPreferUrgentTask(WaiterTaskType candidateType, WaiterTaskType currentType)
        {
            return GetUrgentTaskRank(candidateType) < GetUrgentTaskRank(currentType);
        }

        private static int GetUrgentTaskRank(WaiterTaskType type)
        {
            switch (type)
            {
                case WaiterTaskType.TakeOrder:
                    return 0;
                case WaiterTaskType.DeliverOrder:
                    return 1;
                case WaiterTaskType.ProcessBill:
                    return 2;
                case WaiterTaskType.SubmitOrder:
                    return 3;
                case WaiterTaskType.PickupKitchen:
                case WaiterTaskType.PickupBar:
                    return 4;
                default:
                    return 10;
            }
        }

        private bool TryMapSeatToUrgentTask(RestaurantSeat seat, out WaiterTaskType type)
        {
            switch (seat.Status)
            {
                case RestaurantSeatStatus.WaitingForOrder:
                    type = WaiterTaskType.TakeOrder;
                    return true;

                case RestaurantSeatStatus.AwaitingOrderSubmission:
                    type = WaiterTaskType.SubmitOrder;
                    return true;

                case RestaurantSeatStatus.AwaitingDelivery:
                    if (seat.ActiveOrder != null && seat.ActiveOrder.IsReadyToServe)
                    {
                        type = WaiterTaskType.DeliverOrder;
                        return true;
                    }

                    break;

                case RestaurantSeatStatus.WaitingBill:
                    type = WaiterTaskType.ProcessBill;
                    return true;

                case RestaurantSeatStatus.AwaitingPickup:
                    if (seat.ActiveOrder != null)
                    {
                        if (seat.ActiveOrder.CanPickupKitchen(Time.time))
                        {
                            type = WaiterTaskType.PickupKitchen;
                            return true;
                        }

                        if (seat.ActiveOrder.CanPickupBar(Time.time))
                        {
                            type = WaiterTaskType.PickupBar;
                            return true;
                        }
                    }

                    break;
            }

            type = WaiterTaskType.None;
            return false;
        }

        private bool TryGetDeliverTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            return TryGetTaskBySeatPredicate(
                seats,
                seat => seat.Status == RestaurantSeatStatus.AwaitingDelivery &&
                        seat.ActiveOrder != null &&
                        seat.ActiveOrder.IsReadyToServe,
                WaiterTaskType.DeliverOrder,
                out task);
        }

        private bool TryGetCleanupTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            return TryGetTaskBySeatPredicate(
                seats,
                seat => seat.Status == RestaurantSeatStatus.NeedsCleanup,
                WaiterTaskType.Cleanup,
                out task);
        }

        private bool TryGetProcessBillTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            return TryGetTaskBySeatPredicate(
                seats,
                seat => seat.Status == RestaurantSeatStatus.WaitingBill,
                WaiterTaskType.ProcessBill,
                out task);
        }

        private bool TryGetSubmitOrderTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            return TryGetTaskBySeatPredicate(
                seats,
                seat => seat.Status == RestaurantSeatStatus.AwaitingOrderSubmission,
                WaiterTaskType.SubmitOrder,
                out task);
        }

        private bool TryGetTakeOrderTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            return TryGetTaskBySeatPredicate(
                seats,
                seat => seat.Status == RestaurantSeatStatus.WaitingForOrder,
                WaiterTaskType.TakeOrder,
                out task);
        }

        private bool TryGetPickupTask(List<RestaurantSeat> seats, out WaiterTask task)
        {
            RestaurantSeat bestSeat = null;
            WaiterTaskType bestType = WaiterTaskType.None;
            float bestUrgency = -1f;

            for (int index = 0; index < seats.Count; index++)
            {
                RestaurantSeat seat = seats[index];
                if (seat == null || seat.Status != RestaurantSeatStatus.AwaitingPickup || seat.ActiveOrder == null)
                {
                    continue;
                }

                bool canPickupKitchen = seat.ActiveOrder.CanPickupKitchen(Time.time);
                bool canPickupBar = seat.ActiveOrder.CanPickupBar(Time.time);
                if (!canPickupKitchen && !canPickupBar)
                {
                    continue;
                }

                float urgency = seat.GuestUrgencyNormalized;
                if (bestSeat != null && urgency < bestUrgency)
                {
                    continue;
                }

                bestSeat = seat;
                bestUrgency = urgency;
                bestType = canPickupKitchen ? WaiterTaskType.PickupKitchen : WaiterTaskType.PickupBar;
            }

            if (bestSeat == null || bestType == WaiterTaskType.None)
            {
                task = default;
                return false;
            }

            task = new WaiterTask(bestType, bestSeat);
            return true;
        }

        private static bool TryGetTaskBySeatPredicate(
            List<RestaurantSeat> seats,
            Predicate<RestaurantSeat> predicate,
            WaiterTaskType taskType,
            out WaiterTask task)
        {
            RestaurantSeat bestSeat = null;
            float bestUrgency = -1f;
            for (int index = 0; index < seats.Count; index++)
            {
                RestaurantSeat seat = seats[index];
                if (seat == null || !predicate(seat))
                {
                    continue;
                }

                float urgency = seat.GuestUrgencyNormalized;
                if (bestSeat != null && urgency < bestUrgency)
                {
                    continue;
                }

                bestSeat = seat;
                bestUrgency = urgency;
            }

            if (bestSeat == null)
            {
                task = default;
                return false;
            }

            task = new WaiterTask(taskType, bestSeat);
            return true;
        }

        private void ApplyWaiterUpgrades()
        {
            if (waiter == null)
            {
                return;
            }

            waiter.ApplyServiceSpeedMultipliers(
                WaiterSpeedMultiplier,
                WaiterTakeOrderSpeedMultiplier,
                WaiterSubmitOrderSpeedMultiplier,
                WaiterPickupSpeedMultiplier);
        }

        private string GetWaiterDisplayName()
        {
            if (waiter != null && !string.IsNullOrWhiteSpace(waiter.DisplayName))
            {
                return waiter.DisplayName;
            }

            return LocalizationService.IsRussian ? "Анатолий" : "Anatoly";
        }

        private static string GetWaiterMoveUpgradeTitle()
        {
            return LocalizationService.IsRussian ? "скорость передвижения" : "move speed";
        }

        private static string GetWaiterTakeOrderUpgradeTitle()
        {
            return LocalizationService.IsRussian ? "скорость принятия заказа" : "order taking";
        }

        private static string GetWaiterSubmitUpgradeTitle()
        {
            return LocalizationService.IsRussian ? "скорость пробития заказа" : "order input";
        }

        private static string GetWaiterPickupUpgradeTitle()
        {
            return LocalizationService.IsRussian ? "скорость забора заказа" : "order pickup";
        }

        private static string GetWaiterCharismaUpgradeTitle()
        {
            return LocalizationService.IsRussian ? "обаятельность" : "charisma";
        }

        private string BuildWaiterUpgradeNotification(string upgradeTitle, int level, float multiplier)
        {
            string waiterName = GetWaiterDisplayName();
            if (LocalizationService.IsRussian)
            {
                return waiterName + ": " + upgradeTitle + " ур." + level + "  x" + multiplier.ToString("0.00");
            }

            return waiterName + ": " + upgradeTitle + " Lv." + level + "  x" + multiplier.ToString("0.00");
        }

        private string BuildWaiterCharismaUpgradeNotification()
        {
            string waiterName = GetWaiterDisplayName();
            if (LocalizationService.IsRussian)
            {
                return waiterName +
                    ": " +
                    GetWaiterCharismaUpgradeTitle() +
                    " ур." +
                    waiterCharismaLevel +
                    "  чаевые x" +
                    WaiterCharismaTipMultiplier.ToString("0.00") +
                    "  лояльность +" +
                    WaiterCharismaLoyaltyBonus;
            }

            return waiterName +
                ": " +
                GetWaiterCharismaUpgradeTitle() +
                " Lv." +
                waiterCharismaLevel +
                "  tips x" +
                WaiterCharismaTipMultiplier.ToString("0.00") +
                "  loyalty +" +
                WaiterCharismaLoyaltyBonus;
        }

        private void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            ProgressData data = JsonUtility.FromJson<ProgressData>(json);
            totalMoney = Mathf.Max(0, data.totalMoney);
            totalTips = Mathf.Max(0, data.totalTips);
            lifetimeOrderRevenue = Mathf.Max(0, data.lifetimeOrderRevenue);
            servedGuests = Mathf.Max(0, data.servedGuests);
            walkedOutGuests = Mathf.Max(0, data.walkedOutGuests);
            queueWalkedOutGuests = Mathf.Max(0, data.queueWalkedOutGuests);
            loyaltyScore = data.loyaltyScore;
            tableRevenueLevel = Mathf.Max(0, data.tableRevenueLevel);
            waiterSpeedLevel = Mathf.Max(0, data.waiterSpeedLevel);
            waiterTakeOrderSpeedLevel = Mathf.Max(0, data.waiterTakeOrderSpeedLevel);
            waiterSubmitOrderSpeedLevel = Mathf.Max(0, data.waiterSubmitOrderSpeedLevel);
            waiterPickupSpeedLevel = Mathf.Max(0, data.waiterPickupSpeedLevel);
            waiterCharismaLevel = Mathf.Max(0, data.waiterCharismaLevel);
            kitchenSpeedLevel = Mathf.Max(0, data.kitchenSpeedLevel);
            barSpeedLevel = Mathf.Max(0, data.barSpeedLevel);
            waiterPriorityMode = IsValidPriorityMode(data.waiterPriorityMode)
                ? (WaiterPriorityMode)data.waiterPriorityMode
                : WaiterPriorityMode.Balanced;
        }

        private void SaveProgress()
        {
            ProgressData data = new ProgressData
            {
                totalMoney = totalMoney,
                totalTips = totalTips,
                lifetimeOrderRevenue = lifetimeOrderRevenue,
                servedGuests = servedGuests,
                walkedOutGuests = walkedOutGuests,
                queueWalkedOutGuests = queueWalkedOutGuests,
                loyaltyScore = loyaltyScore,
                tableRevenueLevel = tableRevenueLevel,
                waiterSpeedLevel = waiterSpeedLevel,
                waiterTakeOrderSpeedLevel = waiterTakeOrderSpeedLevel,
                waiterSubmitOrderSpeedLevel = waiterSubmitOrderSpeedLevel,
                waiterPickupSpeedLevel = waiterPickupSpeedLevel,
                waiterCharismaLevel = waiterCharismaLevel,
                kitchenSpeedLevel = kitchenSpeedLevel,
                barSpeedLevel = barSpeedLevel,
                waiterPriorityMode = (int)waiterPriorityMode,
                lastSavedUnixSeconds = GetCurrentUnixSeconds()
            };

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void TryApplyOfflineIncome()
        {
            if (!enableOfflineIncome || !PlayerPrefs.HasKey(SaveKey))
            {
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            ProgressData data = JsonUtility.FromJson<ProgressData>(json);
            if (data.lastSavedUnixSeconds <= 0)
            {
                return;
            }

            long nowUnix = GetCurrentUnixSeconds();
            long elapsedSeconds = Math.Max(0L, nowUnix - data.lastSavedUnixSeconds);
            if (elapsedSeconds <= 0)
            {
                return;
            }

            float cappedSeconds = Mathf.Min((float)elapsedSeconds, maxOfflineHours * 3600f);
            if (cappedSeconds <= 0f)
            {
                return;
            }

            int rawOfflineIncome = Mathf.Max(0, Mathf.RoundToInt(EstimateIncomePerSecond() * cappedSeconds * offlineIncomeFactor));
            int offlineIncome = ApplyOfflineSoftCap(rawOfflineIncome);
            if (offlineIncome <= 0)
            {
                return;
            }

            int stage = GetEconomyStage();
            int softCap = EvaluateOfflineSoftCap();
            bool wasCapped = offlineIncome < rawOfflineIncome;

            totalMoney += offlineIncome;
            lifetimeOrderRevenue += offlineIncome;

            if (wasCapped)
            {
                Log(
                    "Offline income soft cap applied. Raw: " +
                    rawOfflineIncome +
                    ", final: " +
                    offlineIncome +
                    ", stage: " +
                    stage +
                    ", cap: " +
                    softCap +
                    ".");
            }

            hasPendingOfflineIncomeReport = true;
            pendingOfflineIncomeReport = new OfflineIncomeReport
            {
                RawIncome = rawOfflineIncome,
                FinalIncome = offlineIncome,
                SoftCap = softCap,
                Stage = stage,
                ElapsedSeconds = Mathf.RoundToInt(cappedSeconds),
                WasCapped = wasCapped
            };

            RaiseRuntimeSignal(RestaurantRuntimeSignalType.OfflineIncomeApplied, null);
            Log("Offline income applied: +" + offlineIncome + " for " + Mathf.RoundToInt(cappedSeconds) + " sec.");
        }

        private float EstimateIncomePerSecond()
        {
            float avgSpawnDelay = Mathf.Max(0.1f, (spawnDelayRange.x + spawnDelayRange.y) * 0.5f);
            float guestRate = 1f / avgSpawnDelay;

            float historicalAvgPayout = servedGuests > 0
                ? (lifetimeOrderRevenue + totalTips) / (float)servedGuests
                : 0f;

            if (historicalAvgPayout <= 0.01f)
            {
                float basePrice = (orderPriceRange.x + orderPriceRange.y) * 0.5f * TableRevenueMultiplier;
                float baseTip = (tipRange.x + tipRange.y) * 0.5f * TableTipMultiplier;
                historicalAvgPayout = basePrice + baseTip;
            }

            return Mathf.Max(0f, historicalAvgPayout * guestRate);
        }

        private int ApplyOfflineSoftCap(int rawOfflineIncome)
        {
            if (rawOfflineIncome <= 0 || !enableOfflineSoftCap)
            {
                return rawOfflineIncome;
            }

            int softCap = EvaluateOfflineSoftCap();
            if (softCap <= 0 || rawOfflineIncome <= softCap)
            {
                return rawOfflineIncome;
            }

            int overCapIncome = rawOfflineIncome - softCap;
            float overCapEfficiency = Mathf.Clamp01(offlineOvercapEfficiency);
            int softenedIncome = softCap + Mathf.RoundToInt(overCapIncome * overCapEfficiency);
            return Mathf.Max(softCap, softenedIncome);
        }

        private int EvaluateOfflineSoftCap()
        {
            return Mathf.Max(0, offlineBaseSoftCap + GetEconomyStage() * offlineSoftCapPerStage);
        }

        private int GetEconomyStage()
        {
            return Mathf.Max(
                0,
                tableRevenueLevel +
                waiterSpeedLevel +
                waiterTakeOrderSpeedLevel +
                waiterSubmitOrderSpeedLevel +
                waiterPickupSpeedLevel +
                waiterCharismaLevel +
                kitchenSpeedLevel +
                barSpeedLevel);
        }

        private static bool IsValidPriorityMode(int value)
        {
            return value >= (int)WaiterPriorityMode.Balanced && value <= (int)WaiterPriorityMode.TipFocus;
        }

        private static string GetWaiterPriorityLabel(WaiterPriorityMode mode)
        {
            switch (mode)
            {
                case WaiterPriorityMode.Speed:
                    return LocalizationService.Get("rest.waiter.priority.speed");
                case WaiterPriorityMode.TipFocus:
                    return LocalizationService.Get("rest.waiter.priority.tip");
                default:
                    return LocalizationService.Get("rest.waiter.priority.balanced");
            }
        }

        private static long GetCurrentUnixSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void RaiseNotification(string message, RestaurantNotificationType notificationType)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            NotificationRaised?.Invoke(message, notificationType);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (Application.isEditor)
            {
                return;
            }

            if (pauseStatus)
            {
                SaveProgress();
            }
        }

        private void OnApplicationQuit()
        {
            if (Application.isEditor)
            {
                return;
            }

            SaveProgress();
        }

        [ContextMenu("Reset Saved Progress")]
        public void ResetSavedProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            totalMoney = 0;
            totalTips = 0;
            lifetimeOrderRevenue = 0;
            servedGuests = 0;
            walkedOutGuests = 0;
            queueWalkedOutGuests = 0;
            loyaltyScore = 0;
            tableRevenueLevel = 0;
            waiterSpeedLevel = 0;
            waiterTakeOrderSpeedLevel = 0;
            waiterSubmitOrderSpeedLevel = 0;
            waiterPickupSpeedLevel = 0;
            waiterCharismaLevel = 0;
            kitchenSpeedLevel = 0;
            barSpeedLevel = 0;
            waiterPriorityMode = WaiterPriorityMode.Balanced;
            queuedGuests.Clear();
            queuedGuestWaitDurations.Clear();
            queuedGuestWarned.Clear();
            hasPendingOfflineIncomeReport = false;
            pendingOfflineIncomeReport = default;
            ApplyWaiterUpgrades();
            SaveProgress();
        }

        private static int EvaluateUpgradeCost(int baseCost, float growth, int level)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * Mathf.Pow(growth, level)));
        }

        private bool ValidateSetup()
        {
            if (waiter == null)
            {
                Debug.LogWarning("RestaurantRuntime: WaiterAgent is missing.");
                return false;
            }

            if (tables.Count == 0 || GetAllSeats().Count == 0)
            {
                Debug.LogWarning("RestaurantRuntime: no configured restaurant seats were found.");
                return false;
            }

            RestaurantPoint point;
            RestaurantPointType[] requiredPoints =
            {
                RestaurantPointType.GuestSpawn,
                RestaurantPointType.WaiterOrderSubmit,
                RestaurantPointType.BarPickup,
                RestaurantPointType.KitchenPickup,
                RestaurantPointType.Garbage,
                RestaurantPointType.Cashier
            };

            for (int index = 0; index < requiredPoints.Length; index++)
            {
                if (!TryGetPoint(requiredPoints[index], out point) || point == null)
                {
                    Debug.LogWarning("RestaurantRuntime: missing point " + requiredPoints[index] + ".");
                    return false;
                }
            }

            return true;
        }

        private static string GetSeatLabel(RestaurantSeat seat)
        {
            if (seat == null)
            {
                return "<null seat>";
            }

            if (seat.Table != null)
            {
                return seat.Table.name + "/" + seat.name;
            }

            return seat.name;
        }

        private int CountOrders(Predicate<OrderTicket> predicate)
        {
            if (predicate == null)
            {
                return 0;
            }

            int count = 0;
            List<RestaurantSeat> seats = GetAllSeats();
            for (int index = 0; index < seats.Count; index++)
            {
                OrderTicket order = seats[index] != null ? seats[index].ActiveOrder : null;
                if (order != null && predicate(order))
                {
                    count++;
                }
            }

            return count;
        }

        private void RaiseRuntimeSignal(RestaurantRuntimeSignalType signalType, RestaurantSeat seat)
        {
            SignalRaised?.Invoke(signalType, seat);
            StateChanged?.Invoke();
        }

        private void Log(string message)
        {
            if (!verboseLogs)
            {
                return;
            }

            Debug.Log("[RestaurantRuntime] " + message);
        }
    }
}
