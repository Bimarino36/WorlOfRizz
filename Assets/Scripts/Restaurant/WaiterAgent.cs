using System.Collections;
using IdleRestaurant.Localization;
using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class WaiterAgent : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float moveSpeed = 2.4f;
        [SerializeField, Min(0.5f)] private float initialMoveSpeed = 2.4f;
        [SerializeField, Min(180f)] private float turnSpeed = 720f;
        [SerializeField, Min(0.05f)] private float baseStopDistance = 0.2f;
        [SerializeField, Min(0.05f)] private float interactionDelay = 0.45f;
        [SerializeField] private string currentTaskLabel = "Idle";

        private RestaurantRuntime runtime;
        private Coroutine serviceLoop;
        private WaiterTaskType currentTaskType;

        public string CurrentTaskLabel => GetTaskLabel(currentTaskType);

        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            if (initialMoveSpeed <= 0f)
            {
                initialMoveSpeed = moveSpeed;
            }
        }

        public void BindRuntime(RestaurantRuntime ownerRuntime)
        {
            runtime = ownerRuntime;

            if (!Application.isPlaying || serviceLoop != null)
            {
                return;
            }

            serviceLoop = StartCoroutine(ServiceLoop());
        }

        private void Start()
        {
            if (runtime != null && serviceLoop == null)
            {
                serviceLoop = StartCoroutine(ServiceLoop());
            }
        }

        public void ApplyMoveSpeedMultiplier(float multiplier)
        {
            if (initialMoveSpeed <= 0f)
            {
                initialMoveSpeed = moveSpeed;
            }

            moveSpeed = Mathf.Max(0.5f, initialMoveSpeed * Mathf.Max(0.1f, multiplier));
        }

        private IEnumerator ServiceLoop()
        {
            while (true)
            {
                if (runtime == null)
                {
                    SetCurrentTask(WaiterTaskType.None);
                    yield return null;
                    continue;
                }

                WaiterTask task;
                if (!runtime.TryGetNextTask(out task) || !task.IsValid)
                {
                    SetCurrentTask(WaiterTaskType.None);
                    yield return null;
                    continue;
                }

                SetCurrentTask(task.Type);
                yield return ExecuteTask(task);
            }
        }

        private IEnumerator ExecuteTask(WaiterTask task)
        {
            RestaurantSeat seat = task.Seat;
            if (seat == null)
            {
                yield break;
            }

            switch (task.Type)
            {
                case WaiterTaskType.TakeOrder:
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction();
                    runtime.HandleOrderTaken(seat);
                    break;

                case WaiterTaskType.SubmitOrder:
                    yield return MoveToPoint(RestaurantPointType.WaiterOrderSubmit);
                    yield return WaitInteraction();
                    runtime.HandleOrderSubmitted(seat);
                    break;

                case WaiterTaskType.PickupKitchen:
                    yield return MoveToPoint(RestaurantPointType.KitchenPickup);
                    yield return WaitInteraction();
                    runtime.HandleKitchenPickedUp(seat);
                    break;

                case WaiterTaskType.PickupBar:
                    yield return MoveToPoint(RestaurantPointType.BarPickup);
                    yield return WaitInteraction();
                    runtime.HandleBarPickedUp(seat);
                    break;

                case WaiterTaskType.DeliverOrder:
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction();
                    runtime.HandleOrderDelivered(seat);
                    break;

                case WaiterTaskType.Cleanup:
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction();
                    yield return MoveToPoint(RestaurantPointType.Garbage);
                    yield return WaitInteraction();
                    runtime.HandleCleanupCompleted(seat);
                    break;

                case WaiterTaskType.ProcessBill:
                    yield return MoveToPoint(RestaurantPointType.Cashier);
                    yield return WaitInteraction();
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction();
                    runtime.HandleBillDelivered(seat);
                    break;
            }
        }

        private IEnumerator MoveToPoint(RestaurantPointType pointType)
        {
            RestaurantPoint point;
            if (!runtime.TryGetPoint(pointType, out point) || point == null)
            {
                yield break;
            }

            yield return MoveToPosition(point.WorldPosition, Mathf.Max(baseStopDistance, point.ArrivalRadius));
        }

        private IEnumerator MoveToTransform(Transform targetTransform, float stopDistance)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            yield return MoveToPosition(targetTransform.position, stopDistance);
        }

        private IEnumerator MoveToPosition(Vector3 worldPosition, float stopDistance)
        {
            while (!AdvanceTo(worldPosition, stopDistance))
            {
                yield return null;
            }
        }

        private IEnumerator WaitInteraction()
        {
            yield return new WaitForSeconds(interactionDelay);
        }

        private bool AdvanceTo(Vector3 targetPosition, float stopDistance)
        {
            Vector3 currentPosition = transform.position;
            Vector3 flattenedTarget = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
            Vector3 delta = flattenedTarget - currentPosition;
            delta.y = 0f;

            if (delta.sqrMagnitude <= stopDistance * stopDistance)
            {
                transform.position = flattenedTarget;
                return true;
            }

            transform.position = Vector3.MoveTowards(currentPosition, flattenedTarget, moveSpeed * Time.deltaTime);

            if (delta.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            return false;
        }

        private static Transform GetWaiterTarget(RestaurantSeat seat)
        {
            if (seat == null)
            {
                return null;
            }

            if (seat.Table != null && seat.Table.WaiterServicePoint != null)
            {
                return seat.Table.WaiterServicePoint;
            }

            return seat.ServicePoint;
        }

        private void SetCurrentTask(WaiterTaskType taskType)
        {
            currentTaskType = taskType;
            currentTaskLabel = GetTaskLabel(currentTaskType);
        }

        private static string GetTaskLabel(WaiterTaskType taskType)
        {
            switch (taskType)
            {
                case WaiterTaskType.TakeOrder:
                    return LocalizationService.Get("rest.waiter.task.take_order");
                case WaiterTaskType.SubmitOrder:
                    return LocalizationService.Get("rest.waiter.task.submit_order");
                case WaiterTaskType.PickupKitchen:
                    return LocalizationService.Get("rest.waiter.task.pickup_kitchen");
                case WaiterTaskType.PickupBar:
                    return LocalizationService.Get("rest.waiter.task.pickup_bar");
                case WaiterTaskType.DeliverOrder:
                    return LocalizationService.Get("rest.waiter.task.deliver_order");
                case WaiterTaskType.Cleanup:
                    return LocalizationService.Get("rest.waiter.task.cleanup");
                case WaiterTaskType.ProcessBill:
                    return LocalizationService.Get("rest.waiter.task.process_bill");
                default:
                    return LocalizationService.Get("rest.waiter.task.idle");
            }
        }
    }
}
