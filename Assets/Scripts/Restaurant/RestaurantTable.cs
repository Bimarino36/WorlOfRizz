using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantTable : MonoBehaviour
    {
        [SerializeField] private bool allowSharedSeating;
        [SerializeField, Min(0.2f)] private float servicePointOffset = 0.65f;
        [SerializeField] private Transform waiterServicePoint;
        [SerializeField] private List<RestaurantSeat> seats = new List<RestaurantSeat>();

        public IReadOnlyList<RestaurantSeat> Seats => seats;

        public Transform WaiterServicePoint
        {
            get
            {
                if (waiterServicePoint != null)
                {
                    return waiterServicePoint;
                }

                return ResolveClosestSeatServicePoint();
            }
        }

        public RestaurantSeatStatus GetDisplayStatus()
        {
            if (HasSeatWithStatus(RestaurantSeatStatus.NeedsCleanup))
            {
                return RestaurantSeatStatus.NeedsCleanup;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.WaitingBill))
            {
                return RestaurantSeatStatus.WaitingBill;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.AwaitingDelivery))
            {
                return RestaurantSeatStatus.AwaitingDelivery;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.AwaitingPickup) ||
                HasSeatWithStatus(RestaurantSeatStatus.AwaitingOrderSubmission))
            {
                return RestaurantSeatStatus.AwaitingPickup;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.WaitingForOrder))
            {
                return RestaurantSeatStatus.WaitingForOrder;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.Eating))
            {
                return RestaurantSeatStatus.Eating;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.Reserved))
            {
                return RestaurantSeatStatus.Reserved;
            }

            if (HasSeatWithStatus(RestaurantSeatStatus.GuestLeaving))
            {
                return RestaurantSeatStatus.GuestLeaving;
            }

            return RestaurantSeatStatus.Available;
        }

        public bool IsOccupied
        {
            get
            {
                for (int index = 0; index < seats.Count; index++)
                {
                    if (seats[index] != null && seats[index].HasGuest)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public float GetUrgencyNormalized()
        {
            float highestUrgency = 0f;

            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index] == null)
                {
                    continue;
                }

                highestUrgency = Mathf.Max(highestUrgency, seats[index].GuestUrgencyNormalized);
            }

            return highestUrgency;
        }

        public void RefreshSeats()
        {
            seats = GetComponentsInChildren<RestaurantSeat>(true)
                .OrderBy(seat => seat.name)
                .ToList();

            waiterServicePoint = transform.Find("TableWaiterPoint");

            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index] != null)
                {
                    seats[index].Bind(this);
                }
            }
        }

        public RestaurantSeat GetAvailableSeat()
        {
            if (!allowSharedSeating && IsOccupied)
            {
                return null;
            }

            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index] != null && seats[index].IsAvailable)
                {
                    return seats[index];
                }
            }

            return null;
        }

        private void Awake()
        {
            RefreshSeats();
        }

#if UNITY_EDITOR
        [ContextMenu("Rebuild Seats From Child Chairs")]
        public void RebuildSeatsFromChildChairs()
        {
            Transform seatsRoot = transform.Find("SeatSlots");
            if (seatsRoot == null)
            {
                GameObject seatsRootObject = new GameObject("SeatSlots");
                Undo.RegisterCreatedObjectUndo(seatsRootObject, "Create SeatSlots");
                seatsRoot = seatsRootObject.transform;
                seatsRoot.SetParent(transform, false);
            }

            for (int index = seatsRoot.childCount - 1; index >= 0; index--)
            {
                Undo.DestroyObjectImmediate(seatsRoot.GetChild(index).gameObject);
            }

            List<Transform> chairs = GetComponentsInChildren<Transform>(true)
                .Where(child => child != transform)
                .Where(child => child.parent != seatsRoot)
                .Where(child => !child.IsChildOf(seatsRoot))
                .Where(child => child.name.ToLowerInvariant().Contains("chair"))
                .ToList();

            Vector3 tableCenter = transform.position;
            tableCenter.y = 0f;

            int seatCounter = 1;
            for (int index = 0; index < chairs.Count; index++)
            {
                Transform chair = chairs[index];

                GameObject seatRootObject = new GameObject("Seat_" + seatCounter.ToString("00"));
                Undo.RegisterCreatedObjectUndo(seatRootObject, "Create restaurant seat");
                seatRootObject.transform.SetParent(seatsRoot, false);

                Vector3 seatPosition = chair.position;
                seatPosition.y = 0f;

                Vector3 outward = chair.position - tableCenter;
                outward.y = 0f;
                if (outward.sqrMagnitude < 0.001f)
                {
                    outward = transform.right;
                    outward.y = 0f;
                }

                outward.Normalize();
                Vector3 servicePosition = seatPosition + outward * servicePointOffset;

                GameObject seatPointObject = new GameObject("SeatPoint");
                Undo.RegisterCreatedObjectUndo(seatPointObject, "Create seat point");
                seatPointObject.transform.SetParent(seatRootObject.transform, false);
                seatPointObject.transform.position = seatPosition;

                GameObject servicePointObject = new GameObject("ServicePoint");
                Undo.RegisterCreatedObjectUndo(servicePointObject, "Create service point");
                servicePointObject.transform.SetParent(seatRootObject.transform, false);
                servicePointObject.transform.position = servicePosition;

                RestaurantSeat seat = seatRootObject.AddComponent<RestaurantSeat>();
                seat.SetPoints(seatPointObject.transform, servicePointObject.transform, chair);
                seat.Bind(this);

                seatCounter++;
            }

            Transform tableWaiterPoint = transform.Find("TableWaiterPoint");
            if (tableWaiterPoint == null)
            {
                GameObject tableWaiterPointObject = new GameObject("TableWaiterPoint");
                Undo.RegisterCreatedObjectUndo(tableWaiterPointObject, "Create table waiter point");
                tableWaiterPoint = tableWaiterPointObject.transform;
                tableWaiterPoint.SetParent(transform, false);
            }

            Transform closestServicePoint = ResolveClosestSeatServicePointFromScene();
            if (closestServicePoint != null)
            {
                tableWaiterPoint.position = closestServicePoint.position;
            }

            waiterServicePoint = tableWaiterPoint;

            RefreshSeats();
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        private Transform ResolveClosestSeatServicePoint()
        {
            Vector3 referencePoint = GetRestaurantCenterPoint();
            Transform bestPoint = null;
            float bestDistance = float.MaxValue;

            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index] == null || seats[index].ServicePoint == null)
                {
                    continue;
                }

                Vector3 candidate = seats[index].ServicePoint.position;
                candidate.y = 0f;
                float distance = (candidate - referencePoint).sqrMagnitude;
                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestPoint = seats[index].ServicePoint;
            }

            return bestPoint;
        }

        private bool HasSeatWithStatus(RestaurantSeatStatus targetStatus)
        {
            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index] != null && seats[index].Status == targetStatus)
                {
                    return true;
                }
            }

            return false;
        }

        private Transform ResolveClosestSeatServicePointFromScene()
        {
            RefreshSeats();
            return ResolveClosestSeatServicePoint();
        }

        private Vector3 GetRestaurantCenterPoint()
        {
            Transform referenceRoot = transform.parent != null ? transform.parent : transform;
            Renderer[] renderers = referenceRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int index = 1; index < renderers.Length; index++)
                {
                    bounds.Encapsulate(renderers[index].bounds);
                }

                Vector3 center = bounds.center;
                center.y = 0f;
                return center;
            }

            Vector3 fallback = referenceRoot.position;
            fallback.y = 0f;
            return fallback;
        }
    }
}
