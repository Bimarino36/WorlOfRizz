namespace IdleRestaurant.Gameplay
{
    public enum RestaurantRuntimeSignalType
    {
        Initialized = 0,
        QueueChanged = 1,
        GuestAssignedSeat = 2,
        GuestSeated = 3,
        OrderTaken = 4,
        OrderSubmitted = 5,
        KitchenPickedUp = 6,
        BarPickedUp = 7,
        OrderDelivered = 8,
        CleanupNeeded = 9,
        CleanupCompleted = 10,
        BillDelivered = 11,
        GuestWalkedOut = 12,
        GuestExited = 13,
        UpgradeChanged = 14,
        OfflineIncomeApplied = 15
    }
}
