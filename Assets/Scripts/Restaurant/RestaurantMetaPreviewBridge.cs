using System;
using System.Collections.Generic;
using System.Text;
using IdleRestaurant.Localization;
using IdleRestaurant.Meta;
using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantMetaPreviewBridge : MonoBehaviour
    {
        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField, Min(0)] private int rareResourceCount;
        [SerializeField, Min(0)] private int seedCount;
        [SerializeField, Min(0)] private int ingredientCount;
        [SerializeField, Min(0)] private int pendingRestaurantCoins;
        [SerializeField] private bool autoConsumePendingRestaurantCoinsOnEnter = true;
        [SerializeField] private List<SpecialOrderStatus> specialOrders = new List<SpecialOrderStatus>();

        private SpecialOrderStatus[] cachedStatuses = Array.Empty<SpecialOrderStatus>();
        private bool startupPendingRewardsApplied;

        public void Configure(RestaurantRuntime configuredRuntime)
        {
            runtime = configuredRuntime;
            EnsureDefaults();
        }

        public int SpecialOrderCount
        {
            get
            {
                EnsureDefaults();
                return cachedStatuses.Length;
            }
        }

        public MetaResourceSnapshot GetResourceSnapshot()
        {
            MetaResourceSnapshot metaSnapshot = MetaProgressService.GetResourceSnapshot();
            if (Application.isPlaying)
            {
                return metaSnapshot;
            }

            return new MetaResourceSnapshot(
                rareResourceCount + metaSnapshot.RareResources,
                seedCount + metaSnapshot.Seeds,
                ingredientCount + metaSnapshot.Ingredients,
                pendingRestaurantCoins + metaSnapshot.PendingRestaurantCoins);
        }

        public PendingRestaurantRewards GetPendingRewards()
        {
            MetaResourceSnapshot snapshot = GetResourceSnapshot();
            return new PendingRestaurantRewards(
                snapshot.PendingRestaurantCoins,
                snapshot.RareResources,
                snapshot.Seeds,
                snapshot.Ingredients);
        }

        public IReadOnlyList<SpecialOrderStatus> GetSpecialOrderStatuses()
        {
            EnsureDefaults();
            RefreshCachedStatuses();
            return cachedStatuses;
        }

        public bool CanClaimPendingRestaurantCoins()
        {
            return GetResourceSnapshot().PendingRestaurantCoins > 0;
        }

        public string GetClaimButtonLabel()
        {
            int pendingCoins = Mathf.Max(0, GetResourceSnapshot().PendingRestaurantCoins);
            return pendingCoins > 0
                ? LocalizationService.Format("common.claim_amount", pendingCoins)
                : LocalizationService.Get("common.claim_none");
        }

        public bool CanCompleteSpecialOrder(int orderIndex)
        {
            IReadOnlyList<SpecialOrderStatus> statuses = GetSpecialOrderStatuses();
            if (orderIndex < 0 || orderIndex >= statuses.Count)
            {
                return false;
            }

            SpecialOrderStatus status = statuses[orderIndex];
            return !status.IsComplete && status.IsUnlocked;
        }

        public string GetSpecialOrderActionLabel(int orderIndex)
        {
            IReadOnlyList<SpecialOrderStatus> statuses = GetSpecialOrderStatuses();
            if (orderIndex < 0 || orderIndex >= statuses.Count)
            {
                return LocalizationService.Get("common.status.missing");
            }

            SpecialOrderStatus status = statuses[orderIndex];
            if (status.IsComplete)
            {
                return status.Title + "\n" + LocalizationService.Get("rest.contract.action.done");
            }

            if (status.IsUnlocked)
            {
                return status.Title + "\n" + LocalizationService.Get("rest.contract.action.serve");
            }

            return status.Title + "\n" + LocalizationService.Get("rest.contract.action.locked");
        }

        public void TryClaimPendingRestaurantCoins()
        {
            ResolveRuntime();

            int claimedCoins = 0;
            if (Application.isPlaying)
            {
                claimedCoins = MetaProgressService.ConsumePendingRestaurantCoins();
            }
            else if (pendingRestaurantCoins > 0)
            {
                claimedCoins = pendingRestaurantCoins;
                pendingRestaurantCoins = 0;
            }

            if (claimedCoins <= 0)
            {
                NotifyPlayer(LocalizationService.Get("rest.notify.pending_none"), RestaurantNotificationType.Info);
                return;
            }

            if (runtime != null)
            {
                runtime.ApplyExternalRestaurantReward(claimedCoins, 0, LocalizationService.Get("rest.notify.expedition_payout"));
            }
            else
            {
                Debug.Log(LocalizationService.Get("rest.notify.expedition_payout") + " +$" + claimedCoins, this);
            }

            RefreshCachedStatuses();
        }

        public void TryCompleteSpecialOrder(int orderIndex)
        {
            ResolveRuntime();
            EnsureDefaults();
            RefreshCachedStatuses();

            if (orderIndex < 0 || orderIndex >= specialOrders.Count)
            {
                NotifyPlayer(LocalizationService.Get("rest.notify.special_order_missing"), RestaurantNotificationType.Warning);
                return;
            }

            SpecialOrderStatus template = specialOrders[orderIndex];
            SpecialOrderStatus liveStatus = cachedStatuses[orderIndex];

            if (liveStatus.IsComplete)
            {
                NotifyPlayer(LocalizationService.Format("rest.notify.special_order_complete", template.Title), RestaurantNotificationType.Info);
                return;
            }

            if (!liveStatus.IsUnlocked)
            {
                string missingLabel = string.IsNullOrWhiteSpace(liveStatus.LockReason)
                    ? LocalizationService.Get("common.resource_missing")
                    : liveStatus.LockReason;
                NotifyPlayer(missingLabel, RestaurantNotificationType.Warning);
                return;
            }

            if (Application.isPlaying && !MetaProgressService.TryCompleteRestaurantSpecialOrder(template.Id, template.Requirements))
            {
                NotifyPlayer(LocalizationService.Get("rest.notify.special_order_failed"), RestaurantNotificationType.Warning);
                return;
            }

            if (runtime != null)
            {
                runtime.ApplyExternalRestaurantReward(
                    template.RestaurantCoinReward,
                    template.LoyaltyReward,
                    template.Title);
            }
            else
            {
                Debug.Log(template.Title + " " + LocalizationService.Get("common.status.complete"), this);
            }

            RefreshCachedStatuses();
        }

        public void TryOpenAdventurePortal()
        {
            TryOpenPortal(MetaPortalId.Adventure);
        }

        public void TryOpenFarmPortal()
        {
            TryOpenPortal(MetaPortalId.Farm);
        }

        public void TryOpenPortal(MetaPortalId portalId)
        {
            ResolveRuntime();

            if (!MetaProgressService.IsPortalUnlocked(portalId))
            {
                NotifyPlayer(LocalizationService.Get("common.status.locked"), RestaurantNotificationType.Info);
                return;
            }

            if (SceneTransitionService.TryLoadPortal(portalId))
            {
                return;
            }

            string message = portalId == MetaPortalId.Adventure
                ? LocalizationService.Get("rest.portal.adventure_missing")
                : LocalizationService.Get("rest.portal.farm_missing");
            NotifyPlayer(message, RestaurantNotificationType.Info);
        }

        private void Awake()
        {
            ResolveRuntime();
            EnsureDefaults();
            RefreshCachedStatuses();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || startupPendingRewardsApplied || !autoConsumePendingRestaurantCoinsOnEnter)
            {
                return;
            }

            ResolveRuntime();
            if (runtime == null || !runtime.IsInitialized)
            {
                return;
            }

            startupPendingRewardsApplied = true;
            int claimedCoins = MetaProgressService.ConsumePendingRestaurantCoins();
            if (claimedCoins > 0)
            {
                runtime.ApplyExternalRestaurantReward(
                    claimedCoins,
                    0,
                    LocalizationService.Get("rest.notify.expedition_payout"));
            }

            RefreshCachedStatuses();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            rareResourceCount = Mathf.Max(0, rareResourceCount);
            seedCount = Mathf.Max(0, seedCount);
            ingredientCount = Mathf.Max(0, ingredientCount);
            pendingRestaurantCoins = Mathf.Max(0, pendingRestaurantCoins);
            ResolveRuntime();
            EnsureDefaults();
            RefreshCachedStatuses();
        }
#endif

        private void ResolveRuntime()
        {
            if (runtime == null)
            {
                runtime = GetComponent<RestaurantRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<RestaurantRuntime>();
            }
        }

        private void EnsureDefaults()
        {
            if (specialOrders.Count == 0)
            {
                specialOrders.Add(new SpecialOrderStatus(
                    "garden-soup-contract",
                    LocalizationService.Get("rest.contract.garden_soup.title"),
                    LocalizationService.Get("rest.contract.garden_soup.desc"),
                    false,
                    false,
                    LocalizationService.Get("rest.contract.garden_soup.reward"),
                    LocalizationService.Get("rest.contract.garden_soup.lock"),
                    90,
                    0,
                    new SpecialOrderRequirement(MetaResourceKind.Ingredients, 3)));

                specialOrders.Add(new SpecialOrderStatus(
                    "signature-feast-contract",
                    LocalizationService.Get("rest.contract.signature.title"),
                    LocalizationService.Get("rest.contract.signature.desc"),
                    false,
                    false,
                    LocalizationService.Get("rest.contract.signature.reward"),
                    LocalizationService.Get("rest.contract.signature.lock"),
                    150,
                    6,
                    new SpecialOrderRequirement(MetaResourceKind.RareResource, 1),
                    new SpecialOrderRequirement(MetaResourceKind.Ingredients, 5)));
            }
            else
            {
                RebuildLocalizedDefaults();
            }

            if (cachedStatuses == null || cachedStatuses.Length != specialOrders.Count)
            {
                cachedStatuses = new SpecialOrderStatus[specialOrders.Count];
            }
        }

        private void RefreshCachedStatuses()
        {
            EnsureDefaults();
            MetaResourceSnapshot snapshot = GetResourceSnapshot();

            for (int index = 0; index < specialOrders.Count; index++)
            {
                SpecialOrderStatus template = specialOrders[index];
                bool isComplete = Application.isPlaying
                    ? MetaProgressService.IsRestaurantSpecialOrderCompleted(template.Id)
                    : template.IsComplete;
                bool isUnlocked = !isComplete && HasRequirements(snapshot, template.Requirements);
                string lockReason = isComplete
                    ? string.Empty
                    : BuildLockReason(snapshot, template);

                cachedStatuses[index] = new SpecialOrderStatus(
                    template.Id,
                    template.Title,
                    template.Description,
                    isUnlocked,
                    isComplete,
                    template.RewardLabel,
                    lockReason,
                    template.RestaurantCoinReward,
                    template.LoyaltyReward,
                    template.Requirements);
            }
        }

        private void NotifyPlayer(string message, RestaurantNotificationType notificationType)
        {
            if (runtime != null)
            {
                runtime.ShowPlayerNotification(message, notificationType);
                return;
            }

            Debug.Log(message, this);
        }

        private static bool HasRequirements(MetaResourceSnapshot snapshot, IReadOnlyList<SpecialOrderRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0)
            {
                return true;
            }

            for (int index = 0; index < requirements.Count; index++)
            {
                SpecialOrderRequirement requirement = requirements[index];
                if (GetResourceAmount(snapshot, requirement.ResourceKind) < Mathf.Max(0, requirement.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        private static string BuildLockReason(MetaResourceSnapshot snapshot, SpecialOrderStatus template)
        {
            StringBuilder builder = new StringBuilder(96);
            if (template.Requirements != null)
            {
                for (int index = 0; index < template.Requirements.Length; index++)
                {
                    SpecialOrderRequirement requirement = template.Requirements[index];
                    int missingAmount = Mathf.Max(0, requirement.Amount - GetResourceAmount(snapshot, requirement.ResourceKind));
                    if (missingAmount <= 0)
                    {
                        continue;
                    }

                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(GetResourceLabel(requirement.ResourceKind));
                    builder.Append(" x");
                    builder.Append(missingAmount);
                }
            }

            return builder.Length > 0
                ? LocalizationService.Format("rest.contract.lock_need", builder.ToString())
                : template.LockReason;
        }

        private static int GetResourceAmount(MetaResourceSnapshot snapshot, MetaResourceKind resourceKind)
        {
            switch (resourceKind)
            {
                case MetaResourceKind.RareResource:
                    return Mathf.Max(0, snapshot.RareResources);
                case MetaResourceKind.Seeds:
                    return Mathf.Max(0, snapshot.Seeds);
                case MetaResourceKind.Ingredients:
                    return Mathf.Max(0, snapshot.Ingredients);
                default:
                    return 0;
            }
        }

        private static string GetResourceLabel(MetaResourceKind resourceKind)
        {
            switch (resourceKind)
            {
                case MetaResourceKind.RareResource:
                    return LocalizationService.Get("common.resource.rare");
                case MetaResourceKind.Seeds:
                    return LocalizationService.Get("common.resource.seeds");
                case MetaResourceKind.Ingredients:
                    return LocalizationService.Get("common.resource.ingredients");
                default:
                    return LocalizationService.Get("common.requirements.none");
            }
        }

        private void RebuildLocalizedDefaults()
        {
            for (int index = 0; index < specialOrders.Count; index++)
            {
                SpecialOrderStatus status = specialOrders[index];
                switch (status.Id)
                {
                    case "garden-soup-contract":
                        status.Title = LocalizationService.Get("rest.contract.garden_soup.title");
                        status.Description = LocalizationService.Get("rest.contract.garden_soup.desc");
                        status.RewardLabel = LocalizationService.Get("rest.contract.garden_soup.reward");
                        status.LockReason = LocalizationService.Get("rest.contract.garden_soup.lock");
                        break;
                    case "signature-feast-contract":
                        status.Title = LocalizationService.Get("rest.contract.signature.title");
                        status.Description = LocalizationService.Get("rest.contract.signature.desc");
                        status.RewardLabel = LocalizationService.Get("rest.contract.signature.reward");
                        status.LockReason = LocalizationService.Get("rest.contract.signature.lock");
                        break;
                }

                specialOrders[index] = status;
            }
        }
    }
}
