using System;
using System.Collections.Generic;
using IdleRestaurant.Localization;
using IdleRestaurant.Meta;
using UnityEngine;

namespace IdleRestaurant.Farm
{
    public sealed class FarmRuntime : MonoBehaviour
    {
        [Serializable]
        private sealed class FarmOrderDefinition
        {
            public string Title;
            public int IngredientCost;
            public int RareReward;
            public int PendingCoinReward;
        }

        [Header("Scene Flow")]
        [SerializeField] private string restaurantSceneName = GameSceneCatalog.RestaurantMain;
        [SerializeField, Min(1)] private int plotCount = 4;
        [SerializeField] private Transform plotsRoot;
        [SerializeField] private Vector3 firstPlotPosition = new Vector3(-3.6f, 0f, -1.4f);
        [SerializeField] private Vector3 plotSpacing = new Vector3(2.4f, 0f, 2.2f);

        [Header("Crop")]
        [SerializeField, Min(5)] private int growthDurationSeconds = 45;
        [SerializeField, Min(1)] private int ingredientYieldPerHarvest = 2;

        [Header("Orders")]
        [SerializeField] private List<FarmOrderDefinition> orders = new List<FarmOrderDefinition>();

        private readonly List<FarmPlotView> plotViews = new List<FarmPlotView>();
        private FarmPlotData[] farmPlots = Array.Empty<FarmPlotData>();
        private MetaResourceSnapshot resourceSnapshot;
        private string statusKey = "farm.status.start";
        private object[] statusArgs = Array.Empty<object>();
        private float nextRefreshAt;

        public int PlotCount => plotCount;

        public string StatusMessage => LocalizationService.Format(statusKey, statusArgs);

        public MetaResourceSnapshot ResourceSnapshot => resourceSnapshot;

        public int OrderCount => orders.Count;

        private void Awake()
        {
            EnsureDefaultOrders();
            EnsurePlotsRoot();
            EnsurePlotViews();
        }

        private void Start()
        {
            SetStatus("farm.status.start");
            RefreshState(true);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshAt)
            {
                return;
            }

            RefreshState(false);
            nextRefreshAt = Time.unscaledTime + 0.35f;
        }

        public string GetPlotButtonLabel(int plotIndex)
        {
            if (!IsValidPlotIndex(plotIndex))
            {
                return LocalizationService.Get("farm.plot.default");
            }

            FarmPlotData plot = farmPlots[plotIndex];
            switch (plot.State)
            {
                case FarmPlotState.Growing:
                    return LocalizationService.Format("farm.plot.grow", plotIndex + 1, Mathf.CeilToInt(GetRemainingSeconds(plot)));
                case FarmPlotState.Ready:
                    return LocalizationService.Format("farm.plot.harvest", plotIndex + 1, Mathf.Max(1, plot.IngredientYield));
                default:
                    return LocalizationService.Format("farm.plot.plant", plotIndex + 1);
            }
        }

        public bool CanInteractWithPlot(int plotIndex)
        {
            if (!IsValidPlotIndex(plotIndex))
            {
                return false;
            }

            FarmPlotData plot = farmPlots[plotIndex];
            return plot.State == FarmPlotState.Ready || resourceSnapshot.Seeds > 0;
        }

        public void InteractWithPlot(int plotIndex)
        {
            if (!IsValidPlotIndex(plotIndex))
            {
                return;
            }

            FarmPlotData plot = farmPlots[plotIndex];
            if (plot.State == FarmPlotState.Ready)
            {
                if (MetaProgressService.TryHarvestPlot(plotIndex, out int harvestedIngredients))
                {
                    SetStatus("farm.status.harvested", harvestedIngredients);
                }
                else
                {
                    SetStatus("farm.status.not_ready");
                }
            }
            else if (plot.State == FarmPlotState.Empty)
            {
                if (MetaProgressService.TryPlantSeed(plotIndex, growthDurationSeconds, ingredientYieldPerHarvest))
                {
                    SetStatus("farm.status.planted", plotIndex + 1);
                }
                else
                {
                    SetStatus("farm.status.need_seeds");
                }
            }
            else
            {
                SetStatus("farm.status.growing");
            }

            RefreshState(true);
        }

        public string GetOrderButtonLabel(int orderIndex)
        {
            if (!IsValidOrderIndex(orderIndex))
            {
                return LocalizationService.Get("farm.order.default");
            }

            FarmOrderDefinition order = orders[orderIndex];
            string title = GetOrderTitle(orderIndex);
            string rewardLabel = order.RareReward > 0
                ? LocalizationService.Format("farm.order.reward_rare", order.RareReward, order.PendingCoinReward)
                : LocalizationService.Format("farm.order.reward_coins", order.PendingCoinReward);
            return LocalizationService.Format("farm.order.label", title, order.IngredientCost, rewardLabel);
        }

        public bool CanCompleteOrder(int orderIndex)
        {
            return IsValidOrderIndex(orderIndex) && resourceSnapshot.Ingredients >= orders[orderIndex].IngredientCost;
        }

        public void CompleteOrder(int orderIndex)
        {
            if (!IsValidOrderIndex(orderIndex))
            {
                return;
            }

            FarmOrderDefinition order = orders[orderIndex];
            string title = GetOrderTitle(orderIndex);
            if (!MetaProgressService.TrySpendIngredients(order.IngredientCost))
            {
                SetStatus("farm.status.not_enough_ingredients");
                RefreshState(true);
                return;
            }

            MetaProgressService.AddPendingRestaurantCoins(order.PendingCoinReward);
            MetaProgressService.AddRareResources(order.RareReward);
            MetaProgressService.RegisterFarmOrderCompletion();
            SetStatus("farm.status.order_completed", title);
            RefreshState(true);
        }

        public void ReturnToRestaurant()
        {
            if (!SceneTransitionService.TryLoadScene(restaurantSceneName))
            {
                Debug.LogWarning(LocalizationService.Get("farm.warning.restaurant_scene_missing"), this);
            }
        }

        private void RefreshState(bool immediate)
        {
            farmPlots = MetaProgressService.GetFarmPlots(plotCount);
            resourceSnapshot = MetaProgressService.GetResourceSnapshot();
            ApplyPlotViews();
            if (immediate)
            {
                nextRefreshAt = Time.unscaledTime + 0.35f;
            }
        }

        private void EnsureDefaultOrders()
        {
            if (orders.Count > 0)
            {
                return;
            }

            orders.Add(new FarmOrderDefinition
            {
                Title = LocalizationService.Get("farm.order.soup_bundle"),
                IngredientCost = 3,
                PendingCoinReward = 45,
                RareReward = 0
            });

            orders.Add(new FarmOrderDefinition
            {
                Title = LocalizationService.Get("farm.order.rare_basket"),
                IngredientCost = 5,
                PendingCoinReward = 35,
                RareReward = 1
            });
        }

        private void EnsurePlotsRoot()
        {
            if (plotsRoot != null)
            {
                return;
            }

            Transform existing = transform.Find("Plots");
            if (existing != null)
            {
                plotsRoot = existing;
                return;
            }

            GameObject root = new GameObject("Plots");
            root.transform.SetParent(transform, false);
            plotsRoot = root.transform;
        }

        private void EnsurePlotViews()
        {
            plotViews.Clear();

            FarmPlotView[] existingViews = plotsRoot.GetComponentsInChildren<FarmPlotView>(true);
            for (int index = 0; index < existingViews.Length; index++)
            {
                if (existingViews[index] != null)
                {
                    plotViews.Add(existingViews[index]);
                }
            }

            while (plotViews.Count < plotCount)
            {
                int index = plotViews.Count;
                GameObject plotObject = new GameObject("Plot_" + (index + 1));
                plotObject.transform.SetParent(plotsRoot, false);
                plotObject.transform.localPosition = GetPlotLocalPosition(index);
                FarmPlotView view = plotObject.AddComponent<FarmPlotView>();
                plotViews.Add(view);
            }

            plotViews.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            for (int index = 0; index < plotViews.Count; index++)
            {
                plotViews[index].Configure(index);
            }
        }

        private void ApplyPlotViews()
        {
            for (int index = 0; index < plotViews.Count && index < farmPlots.Length; index++)
            {
                plotViews[index].Apply(farmPlots[index], GetRemainingSeconds(farmPlots[index]));
            }
        }

        private float GetRemainingSeconds(FarmPlotData plotData)
        {
            if (plotData.State != FarmPlotState.Growing || plotData.ReadyAtUnixSeconds <= 0)
            {
                return 0f;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Mathf.Max(0f, plotData.ReadyAtUnixSeconds - now);
        }

        private Vector3 GetPlotLocalPosition(int plotIndex)
        {
            int column = plotIndex % 2;
            int row = plotIndex / 2;
            return firstPlotPosition + new Vector3(plotSpacing.x * column, 0f, plotSpacing.z * row);
        }

        private bool IsValidPlotIndex(int plotIndex)
        {
            return farmPlots != null && plotIndex >= 0 && plotIndex < farmPlots.Length;
        }

        private bool IsValidOrderIndex(int orderIndex)
        {
            return orderIndex >= 0 && orderIndex < orders.Count;
        }

        private string GetOrderTitle(int orderIndex)
        {
            switch (orderIndex)
            {
                case 0:
                    return LocalizationService.Get("farm.order.soup_bundle");
                case 1:
                    return LocalizationService.Get("farm.order.rare_basket");
                default:
                    return IsValidOrderIndex(orderIndex) ? orders[orderIndex].Title : string.Empty;
            }
        }

        private void SetStatus(string key, params object[] args)
        {
            statusKey = string.IsNullOrWhiteSpace(key) ? "farm.status.start" : key;
            statusArgs = args ?? Array.Empty<object>();
        }
    }
}
