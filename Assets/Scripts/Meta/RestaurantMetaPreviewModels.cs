using System;

namespace IdleRestaurant.Meta
{
    public enum MetaPortalId
    {
        Adventure = 0,
        Farm = 1
    }

    public enum MetaResourceKind
    {
        RareResource = 0,
        Seeds = 1,
        Ingredients = 2
    }

    [Serializable]
    public struct MetaResourceSnapshot
    {
        public int RareResources;
        public int Seeds;
        public int Ingredients;
        public int PendingRestaurantCoins;

        public MetaResourceSnapshot(int rareResources, int seeds, int ingredients, int pendingRestaurantCoins)
        {
            RareResources = rareResources;
            Seeds = seeds;
            Ingredients = ingredients;
            PendingRestaurantCoins = pendingRestaurantCoins;
        }
    }

    [Serializable]
    public struct PendingRestaurantRewards
    {
        public int Coins;
        public int RareResources;
        public int Seeds;
        public int Ingredients;

        public PendingRestaurantRewards(int coins, int rareResources, int seeds, int ingredients)
        {
            Coins = coins;
            RareResources = rareResources;
            Seeds = seeds;
            Ingredients = ingredients;
        }

        public bool HasAnyValue => Coins > 0 || RareResources > 0 || Seeds > 0 || Ingredients > 0;
    }

    [Serializable]
    public struct SpecialOrderRequirement
    {
        public MetaResourceKind ResourceKind;
        public int Amount;

        public SpecialOrderRequirement(MetaResourceKind resourceKind, int amount)
        {
            ResourceKind = resourceKind;
            Amount = amount;
        }
    }

    [Serializable]
    public struct SpecialOrderStatus
    {
        public string Id;
        public string Title;
        public string Description;
        public bool IsUnlocked;
        public bool IsComplete;
        public string RewardLabel;
        public string LockReason;
        public int RestaurantCoinReward;
        public int LoyaltyReward;
        public SpecialOrderRequirement[] Requirements;

        public SpecialOrderStatus(
            string id,
            string title,
            string description,
            bool isUnlocked,
            bool isComplete,
            string rewardLabel,
            string lockReason,
            int restaurantCoinReward,
            int loyaltyReward,
            params SpecialOrderRequirement[] requirements)
        {
            Id = id;
            Title = title;
            Description = description;
            IsUnlocked = isUnlocked;
            IsComplete = isComplete;
            RewardLabel = rewardLabel;
            LockReason = lockReason;
            RestaurantCoinReward = restaurantCoinReward;
            LoyaltyReward = loyaltyReward;
            Requirements = requirements ?? Array.Empty<SpecialOrderRequirement>();
        }
    }
}
