namespace IdleRestaurant.Gameplay
{
    public enum RestaurantSeatStatus
    {
        Available = 0,
        Reserved = 1,
        WaitingForOrder = 2,
        AwaitingOrderSubmission = 3,
        AwaitingPickup = 4,
        AwaitingDelivery = 5,
        Eating = 6,
        NeedsCleanup = 7,
        WaitingBill = 8,
        GuestLeaving = 9
    }
}
