namespace IdleRestaurant.Gameplay
{
    public enum WaiterTaskType
    {
        None = 0,
        TakeOrder = 1,
        SubmitOrder = 2,
        PickupKitchen = 3,
        PickupBar = 4,
        DeliverOrder = 5,
        Cleanup = 6,
        ProcessBill = 7
    }

    public struct WaiterTask
    {
        public WaiterTask(WaiterTaskType type, RestaurantSeat seat)
        {
            Type = type;
            Seat = seat;
        }

        public WaiterTaskType Type;
        public RestaurantSeat Seat;

        public bool IsValid => Type != WaiterTaskType.None && Seat != null;
    }
}
