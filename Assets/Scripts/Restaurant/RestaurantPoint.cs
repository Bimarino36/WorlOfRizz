using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantPoint : MonoBehaviour
    {
        [SerializeField] private RestaurantPointType pointType;
        [SerializeField, Min(0.05f)] private float arrivalRadius = 0.35f;

        public RestaurantPointType PointType => pointType;

        public float ArrivalRadius => arrivalRadius;

        public Vector3 WorldPosition => transform.position;

        public void Configure(RestaurantPointType newPointType, float newArrivalRadius = -1f)
        {
            pointType = newPointType;
            if (newArrivalRadius > 0f)
            {
                arrivalRadius = newArrivalRadius;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = GetColor(pointType);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, arrivalRadius);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.7f);
        }

        private static Color GetColor(RestaurantPointType type)
        {
            switch (type)
            {
                case RestaurantPointType.GuestSpawn:
                    return new Color(0.25f, 0.85f, 0.35f);
                case RestaurantPointType.WaiterOrderSubmit:
                    return new Color(0.2f, 0.65f, 1f);
                case RestaurantPointType.BarPickup:
                    return new Color(0.95f, 0.65f, 0.2f);
                case RestaurantPointType.KitchenPickup:
                    return new Color(0.95f, 0.35f, 0.2f);
                case RestaurantPointType.Garbage:
                    return new Color(0.45f, 0.45f, 0.45f);
                case RestaurantPointType.Cashier:
                    return new Color(0.9f, 0.85f, 0.2f);
                case RestaurantPointType.GuestExit:
                    return new Color(1f, 0.35f, 0.7f);
                default:
                    return Color.white;
            }
        }
    }
}
