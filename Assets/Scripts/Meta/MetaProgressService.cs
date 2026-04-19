using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleRestaurant.Meta
{
    public enum FarmPlotState
    {
        Empty = 0,
        Growing = 1,
        Ready = 2
    }

    [Serializable]
    public struct FarmPlotData
    {
        public FarmPlotState State;
        public long ReadyAtUnixSeconds;
        public int IngredientYield;

        public bool IsEmpty => State == FarmPlotState.Empty;
    }

    [Serializable]
    public struct AdventureRewardResult
    {
        public bool Victory;
        public bool BossDefeated;
        public int WavesCleared;
        public int RareResources;
        public int Seeds;
        public int PendingRestaurantCoins;

        public AdventureRewardResult(
            bool victory,
            bool bossDefeated,
            int wavesCleared,
            int rareResources,
            int seeds,
            int pendingRestaurantCoins)
        {
            Victory = victory;
            BossDefeated = bossDefeated;
            WavesCleared = wavesCleared;
            RareResources = rareResources;
            Seeds = seeds;
            PendingRestaurantCoins = pendingRestaurantCoins;
        }

        public bool HasAnyReward => RareResources > 0 || Seeds > 0 || PendingRestaurantCoins > 0;
    }

    [Serializable]
    public struct MetaProgressData
    {
        public int DataVersion;
        public bool RestaurantSceneUnlocked;
        public bool AdventureSceneUnlocked;
        public bool FarmSceneUnlocked;
        public int RareResources;
        public int Seeds;
        public int Ingredients;
        public int PendingRestaurantCoins;
        public int CompletedAdventureRuns;
        public int TotalAdventureRuns;
        public int CompletedFarmOrders;
        public int TotalHarvestedIngredients;
        public AdventureRewardResult LastAdventureReward;
        public string[] CompletedRestaurantSpecialOrders;
        public FarmPlotData[] FarmPlots;
    }

    public static class GameSceneCatalog
    {
        public const string RestaurantMain = "Restaurant_Main";
        public const string AdventureWorld = "Adventure_World";
        public const string FarmGarden = "Farm_Garden";

        public const string RestaurantMainPath = "Assets/Scenes/Restaurant_Main.unity";
        public const string AdventureWorldPath = "Assets/Scenes/Adventure_World.unity";
        public const string FarmGardenPath = "Assets/Scenes/Farm_Garden.unity";
    }

    public static class MetaProgressService
    {
        private const int CurrentDataVersion = 1;
        private const string SaveKey = "IdleRestaurant.MetaProgress";

        public static MetaProgressData GetData()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                MetaProgressData emptyData = default;
                NormalizeData(ref emptyData);
                return emptyData;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                MetaProgressData emptyData = default;
                NormalizeData(ref emptyData);
                return emptyData;
            }

            MetaProgressData data = JsonUtility.FromJson<MetaProgressData>(json);
            if (NormalizeData(ref data))
            {
                Save(data);
            }

            return data;
        }

        public static MetaResourceSnapshot GetResourceSnapshot()
        {
            MetaProgressData data = GetData();
            return new MetaResourceSnapshot(
                data.RareResources,
                data.Seeds,
                data.Ingredients,
                data.PendingRestaurantCoins);
        }

        public static void AddAdventureRewards(AdventureRewardResult result)
        {
            if (!result.HasAnyReward && !result.Victory && result.WavesCleared <= 0)
            {
                return;
            }

            MetaProgressData data = GetData();
            data.RareResources += Mathf.Max(0, result.RareResources);
            data.Seeds += Mathf.Max(0, result.Seeds);
            data.PendingRestaurantCoins += Mathf.Max(0, result.PendingRestaurantCoins);
            data.TotalAdventureRuns += 1;
            if (result.Victory)
            {
                data.CompletedAdventureRuns += 1;
            }

            data.LastAdventureReward = result;
            Save(data);
        }

        public static FarmPlotData[] GetFarmPlots(int plotCount)
        {
            MetaProgressData data = GetData();
            bool resized = EnsureFarmPlots(ref data, plotCount);
            bool normalized = NormalizeFarmPlots(ref data);
            if (resized || normalized)
            {
                Save(data);
            }

            FarmPlotData[] result = new FarmPlotData[data.FarmPlots.Length];
            Array.Copy(data.FarmPlots, result, data.FarmPlots.Length);
            return result;
        }

        public static bool TryPlantSeed(int plotIndex, int growthDurationSeconds, int ingredientYield)
        {
            MetaProgressData data = GetData();
            EnsureFarmPlots(ref data, plotIndex + 1);
            NormalizeFarmPlots(ref data);

            if (!IsValidPlotIndex(data.FarmPlots, plotIndex))
            {
                return false;
            }

            if (data.Seeds <= 0 || data.FarmPlots[plotIndex].State != FarmPlotState.Empty)
            {
                return false;
            }

            data.Seeds -= 1;
            data.FarmPlots[plotIndex] = new FarmPlotData
            {
                State = FarmPlotState.Growing,
                ReadyAtUnixSeconds = GetCurrentUnixSeconds() + Mathf.Max(1, growthDurationSeconds),
                IngredientYield = Mathf.Max(1, ingredientYield)
            };
            Save(data);
            return true;
        }

        public static bool TryHarvestPlot(int plotIndex, out int ingredientYield)
        {
            MetaProgressData data = GetData();
            EnsureFarmPlots(ref data, plotIndex + 1);
            NormalizeFarmPlots(ref data);

            ingredientYield = 0;
            if (!IsValidPlotIndex(data.FarmPlots, plotIndex))
            {
                return false;
            }

            FarmPlotData plot = data.FarmPlots[plotIndex];
            if (plot.State != FarmPlotState.Ready)
            {
                return false;
            }

            ingredientYield = Mathf.Max(1, plot.IngredientYield);
            data.Ingredients += ingredientYield;
            data.TotalHarvestedIngredients += ingredientYield;
            data.FarmPlots[plotIndex] = default;
            Save(data);
            return true;
        }

        public static bool TrySpendIngredients(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            MetaProgressData data = GetData();
            if (data.Ingredients < amount)
            {
                return false;
            }

            data.Ingredients -= amount;
            Save(data);
            return true;
        }

        public static void AddIngredients(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            MetaProgressData data = GetData();
            data.Ingredients += amount;
            Save(data);
        }

        public static void AddSeeds(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            MetaProgressData data = GetData();
            data.Seeds += amount;
            Save(data);
        }

        public static void AddRareResources(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            MetaProgressData data = GetData();
            data.RareResources += amount;
            Save(data);
        }

        public static void AddPendingRestaurantCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            MetaProgressData data = GetData();
            data.PendingRestaurantCoins += amount;
            Save(data);
        }

        public static int ConsumePendingRestaurantCoins()
        {
            MetaProgressData data = GetData();
            int pendingCoins = Mathf.Max(0, data.PendingRestaurantCoins);
            if (pendingCoins <= 0)
            {
                return 0;
            }

            data.PendingRestaurantCoins = 0;
            Save(data);
            return pendingCoins;
        }

        public static bool IsPortalUnlocked(MetaPortalId portalId)
        {
            MetaProgressData data = GetData();
            return IsPortalUnlocked(data, portalId);
        }

        public static void SetPortalUnlocked(MetaPortalId portalId, bool unlocked)
        {
            MetaProgressData data = GetData();
            if (!SetPortalUnlocked(ref data, portalId, unlocked))
            {
                return;
            }

            Save(data);
        }

        public static bool AreRequirementsMet(IReadOnlyList<SpecialOrderRequirement> requirements)
        {
            MetaProgressData data = GetData();
            return AreRequirementsMet(data, requirements);
        }

        public static bool IsRestaurantSpecialOrderCompleted(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return false;
            }

            MetaProgressData data = GetData();
            return HasCompletedRestaurantOrder(data.CompletedRestaurantSpecialOrders, orderId);
        }

        public static bool TryCompleteRestaurantSpecialOrder(string orderId, IReadOnlyList<SpecialOrderRequirement> requirements)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return false;
            }

            MetaProgressData data = GetData();
            if (HasCompletedRestaurantOrder(data.CompletedRestaurantSpecialOrders, orderId))
            {
                return false;
            }

            if (!AreRequirementsMet(data, requirements))
            {
                return false;
            }

            ConsumeRequirements(ref data, requirements);
            AddCompletedRestaurantOrder(ref data, orderId);
            Save(data);
            return true;
        }

        public static void RegisterFarmOrderCompletion()
        {
            MetaProgressData data = GetData();
            data.CompletedFarmOrders += 1;
            Save(data);
        }

        public static void Save(MetaProgressData data)
        {
            NormalizeData(ref data);
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private static bool NormalizeData(ref MetaProgressData data)
        {
            bool changed = false;
            int previousVersion = data.DataVersion;

            if (previousVersion < 1)
            {
                data.RestaurantSceneUnlocked = true;
                data.AdventureSceneUnlocked = true;
                data.FarmSceneUnlocked = true;
                changed = true;
            }

            if (!data.RestaurantSceneUnlocked)
            {
                data.RestaurantSceneUnlocked = true;
                changed = true;
            }

            if (data.DataVersion != CurrentDataVersion)
            {
                data.DataVersion = CurrentDataVersion;
                changed = true;
            }

            changed |= NormalizeFarmPlots(ref data);
            return changed;
        }

        private static bool EnsureFarmPlots(ref MetaProgressData data, int plotCount)
        {
            int normalizedCount = Mathf.Max(0, plotCount);
            if (data.FarmPlots != null && data.FarmPlots.Length == normalizedCount)
            {
                return false;
            }

            FarmPlotData[] resized = new FarmPlotData[normalizedCount];
            if (data.FarmPlots != null)
            {
                int copyCount = Mathf.Min(data.FarmPlots.Length, resized.Length);
                Array.Copy(data.FarmPlots, resized, copyCount);
            }

            data.FarmPlots = resized;
            return true;
        }

        private static bool NormalizeFarmPlots(ref MetaProgressData data)
        {
            if (data.FarmPlots == null || data.FarmPlots.Length == 0)
            {
                return false;
            }

            bool changed = false;
            long now = GetCurrentUnixSeconds();
            for (int index = 0; index < data.FarmPlots.Length; index++)
            {
                if (data.FarmPlots[index].State == FarmPlotState.Growing &&
                    data.FarmPlots[index].ReadyAtUnixSeconds > 0 &&
                    now >= data.FarmPlots[index].ReadyAtUnixSeconds)
                {
                    FarmPlotData plot = data.FarmPlots[index];
                    plot.State = FarmPlotState.Ready;
                    data.FarmPlots[index] = plot;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool IsValidPlotIndex(FarmPlotData[] plots, int plotIndex)
        {
            return plots != null && plotIndex >= 0 && plotIndex < plots.Length;
        }

        private static bool IsPortalUnlocked(MetaProgressData data, MetaPortalId portalId)
        {
            switch (portalId)
            {
                case MetaPortalId.Adventure:
                    return data.AdventureSceneUnlocked;
                case MetaPortalId.Farm:
                    return data.FarmSceneUnlocked;
                default:
                    return false;
            }
        }

        private static bool SetPortalUnlocked(ref MetaProgressData data, MetaPortalId portalId, bool unlocked)
        {
            switch (portalId)
            {
                case MetaPortalId.Adventure:
                    if (data.AdventureSceneUnlocked == unlocked)
                    {
                        return false;
                    }

                    data.AdventureSceneUnlocked = unlocked;
                    return true;
                case MetaPortalId.Farm:
                    if (data.FarmSceneUnlocked == unlocked)
                    {
                        return false;
                    }

                    data.FarmSceneUnlocked = unlocked;
                    return true;
                default:
                    return false;
            }
        }

        private static bool AreRequirementsMet(MetaProgressData data, IReadOnlyList<SpecialOrderRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0)
            {
                return true;
            }

            for (int index = 0; index < requirements.Count; index++)
            {
                int requiredAmount = Mathf.Max(0, requirements[index].Amount);
                if (requiredAmount <= 0)
                {
                    continue;
                }

                if (GetResourceAmount(data, requirements[index].ResourceKind) < requiredAmount)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ConsumeRequirements(ref MetaProgressData data, IReadOnlyList<SpecialOrderRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0)
            {
                return;
            }

            for (int index = 0; index < requirements.Count; index++)
            {
                int requiredAmount = Mathf.Max(0, requirements[index].Amount);
                if (requiredAmount <= 0)
                {
                    continue;
                }

                switch (requirements[index].ResourceKind)
                {
                    case MetaResourceKind.RareResource:
                        data.RareResources = Mathf.Max(0, data.RareResources - requiredAmount);
                        break;
                    case MetaResourceKind.Seeds:
                        data.Seeds = Mathf.Max(0, data.Seeds - requiredAmount);
                        break;
                    case MetaResourceKind.Ingredients:
                        data.Ingredients = Mathf.Max(0, data.Ingredients - requiredAmount);
                        break;
                }
            }
        }

        private static int GetResourceAmount(MetaProgressData data, MetaResourceKind resourceKind)
        {
            switch (resourceKind)
            {
                case MetaResourceKind.RareResource:
                    return Mathf.Max(0, data.RareResources);
                case MetaResourceKind.Seeds:
                    return Mathf.Max(0, data.Seeds);
                case MetaResourceKind.Ingredients:
                    return Mathf.Max(0, data.Ingredients);
                default:
                    return 0;
            }
        }

        private static bool HasCompletedRestaurantOrder(string[] completedOrderIds, string orderId)
        {
            if (completedOrderIds == null || completedOrderIds.Length == 0 || string.IsNullOrWhiteSpace(orderId))
            {
                return false;
            }

            for (int index = 0; index < completedOrderIds.Length; index++)
            {
                if (string.Equals(completedOrderIds[index], orderId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddCompletedRestaurantOrder(ref MetaProgressData data, string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId) || HasCompletedRestaurantOrder(data.CompletedRestaurantSpecialOrders, orderId))
            {
                return;
            }

            int currentCount = data.CompletedRestaurantSpecialOrders != null ? data.CompletedRestaurantSpecialOrders.Length : 0;
            string[] completedOrderIds = new string[currentCount + 1];
            if (currentCount > 0)
            {
                Array.Copy(data.CompletedRestaurantSpecialOrders, completedOrderIds, currentCount);
            }

            completedOrderIds[currentCount] = orderId;
            data.CompletedRestaurantSpecialOrders = completedOrderIds;
        }

        private static long GetCurrentUnixSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
