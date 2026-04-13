using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantSeat : MonoBehaviour
    {
        [SerializeField] private Transform seatPoint;
        [SerializeField] private Transform servicePoint;
        [SerializeField] private Transform chairAnchor;
        [SerializeField] private RestaurantSeatStatus status = RestaurantSeatStatus.Available;
        [SerializeField] private RestaurantGuest currentGuest;
        [SerializeField] private OrderTicket activeOrder;

        public RestaurantTable Table { get; private set; }

        public Transform SeatPoint => seatPoint != null ? seatPoint : transform;

        public Transform ServicePoint => servicePoint != null ? servicePoint : transform;

        public Transform ChairAnchor => chairAnchor;

        public RestaurantSeatStatus Status => status;

        public RestaurantGuest CurrentGuest => currentGuest;

        public OrderTicket ActiveOrder => activeOrder;

        public bool IsAvailable => currentGuest == null && status == RestaurantSeatStatus.Available;

        public bool HasGuest => currentGuest != null;

        public float GuestUrgencyNormalized => currentGuest != null ? currentGuest.CurrentUrgencyNormalized : 0f;

        public float GuestServiceQuality => currentGuest != null ? currentGuest.ServiceQuality : 1f;

        public bool HasWalkedOutGuest => currentGuest != null && currentGuest.HasWalkedOut;

        public void Bind(RestaurantTable table)
        {
            Table = table;
        }

        public void SetPoints(Transform seatPointTransform, Transform servicePointTransform, Transform chairTransform)
        {
            seatPoint = seatPointTransform;
            servicePoint = servicePointTransform;
            chairAnchor = chairTransform;
        }

        public void Reserve(RestaurantGuest guest)
        {
            currentGuest = guest;
            status = RestaurantSeatStatus.Reserved;
        }

        public void MarkGuestSeated()
        {
            if (currentGuest == null)
            {
                return;
            }

            status = RestaurantSeatStatus.WaitingForOrder;
        }

        public void SetOrder(OrderTicket ticket)
        {
            activeOrder = ticket;
            status = RestaurantSeatStatus.AwaitingOrderSubmission;
        }

        public void MarkAwaitingPickup()
        {
            status = RestaurantSeatStatus.AwaitingPickup;
        }

        public void RefreshDeliveryState()
        {
            if (activeOrder != null && activeOrder.IsReadyToServe)
            {
                status = RestaurantSeatStatus.AwaitingDelivery;
            }
        }

        public void MarkEating()
        {
            status = RestaurantSeatStatus.Eating;
        }

        public void MarkNeedsCleanup()
        {
            status = RestaurantSeatStatus.NeedsCleanup;
        }

        public void MarkWaitingBill()
        {
            status = RestaurantSeatStatus.WaitingBill;
        }

        public void MarkGuestLeaving()
        {
            status = RestaurantSeatStatus.GuestLeaving;
        }

        public void ClearSeat()
        {
            currentGuest = null;
            activeOrder = null;
            status = RestaurantSeatStatus.Available;
        }
    }
}
