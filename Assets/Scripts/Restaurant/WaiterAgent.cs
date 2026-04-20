using System.Collections;
using IdleRestaurant.Localization;
using UnityEngine;
using UnityEngine.AI;

namespace IdleRestaurant.Gameplay
{
    public sealed class WaiterAgent : MonoBehaviour
    {
        [SerializeField] private string displayName = "Анатолий";
        [SerializeField, Min(0.5f)] private float moveSpeed = 2.4f;
        [SerializeField, Min(0.5f)] private float initialMoveSpeed = 2.4f;
        [SerializeField, Min(180f)] private float turnSpeed = 720f;
        [SerializeField, Min(0.05f)] private float baseStopDistance = 0.2f;
        [SerializeField, Min(0.05f)] private float interactionDelay = 0.45f;
        [SerializeField, Min(0.1f)] private float takeOrderDuration = 4f;
        [SerializeField, Min(0.1f)] private float submitOrderDuration = 3f;
        [SerializeField, Min(0.1f)] private float kitchenPickupDuration = 5f;
        [SerializeField, Min(0.1f)] private float barPickupDuration = 5f;
        [SerializeField, Min(0.1f)] private float processBillDuration = 2f;
        [SerializeField] private string currentTaskLabel = "Idle";
        [SerializeField] private RestaurantPathMover pathMover;
        [SerializeField] private WaiterActionProgressView actionProgressView;

        private RestaurantRuntime runtime;
        private Coroutine serviceLoop;
        private WaiterTaskType currentTaskType;
        private float baseTakeOrderDuration;
        private float baseSubmitOrderDuration;
        private float baseKitchenPickupDuration;
        private float baseBarPickupDuration;

        public string CurrentTaskLabel => GetTaskLabel(currentTaskType);

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? "Анатолий" : displayName;

        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            ResolvePathMover();
            CaptureBaseStats();
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

        private void OnDisable()
        {
            HideActionProgress();
        }

        public void ApplyMoveSpeedMultiplier(float multiplier)
        {
            CaptureBaseStats();
            moveSpeed = Mathf.Max(0.5f, initialMoveSpeed * Mathf.Max(0.1f, multiplier));
        }

        public void ApplyServiceSpeedMultipliers(
            float moveSpeedMultiplier,
            float takeOrderSpeedMultiplier,
            float submitOrderSpeedMultiplier,
            float pickupSpeedMultiplier)
        {
            CaptureBaseStats();
            ApplyMoveSpeedMultiplier(moveSpeedMultiplier);
            takeOrderDuration = EvaluateDuration(baseTakeOrderDuration, takeOrderSpeedMultiplier);
            submitOrderDuration = EvaluateDuration(baseSubmitOrderDuration, submitOrderSpeedMultiplier);
            kitchenPickupDuration = EvaluateDuration(baseKitchenPickupDuration, pickupSpeedMultiplier);
            barPickupDuration = EvaluateDuration(baseBarPickupDuration, pickupSpeedMultiplier);
        }

        private IEnumerator ServiceLoop()
        {
            while (true)
            {
                if (runtime == null)
                {
                    SetCurrentTask(WaiterTaskType.None);
                    HideActionProgress();
                    yield return null;
                    continue;
                }

                WaiterTask task;
                if (!runtime.TryGetNextTask(out task) || !task.IsValid)
                {
                    SetCurrentTask(WaiterTaskType.None);
                    HideActionProgress();
                    yield return null;
                    continue;
                }

                SetCurrentTask(task.Type);
                yield return ExecuteTask(task);
                HideActionProgress();
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
                    yield return WaitInteraction(takeOrderDuration, task.Type);
                    runtime.HandleOrderTaken(seat);
                    break;

                case WaiterTaskType.SubmitOrder:
                    yield return MoveToPoint(RestaurantPointType.WaiterOrderSubmit);
                    yield return WaitInteraction(submitOrderDuration, task.Type);
                    runtime.HandleOrderSubmitted(seat);
                    break;

                case WaiterTaskType.PickupKitchen:
                    yield return MoveToPoint(RestaurantPointType.KitchenPickup);
                    yield return WaitInteraction(kitchenPickupDuration, task.Type);
                    runtime.HandleKitchenPickedUp(seat);
                    break;

                case WaiterTaskType.PickupBar:
                    yield return MoveToPoint(RestaurantPointType.BarPickup);
                    yield return WaitInteraction(barPickupDuration, task.Type);
                    runtime.HandleBarPickedUp(seat);
                    break;

                case WaiterTaskType.DeliverOrder:
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction(interactionDelay, task.Type);
                    runtime.HandleOrderDelivered(seat);
                    break;

                case WaiterTaskType.Cleanup:
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction(interactionDelay, task.Type);
                    yield return MoveToPoint(RestaurantPointType.Garbage);
                    yield return WaitInteraction(interactionDelay, task.Type);
                    runtime.HandleCleanupCompleted(seat);
                    break;

                case WaiterTaskType.ProcessBill:
                    yield return MoveToPoint(RestaurantPointType.Cashier);
                    yield return WaitInteraction(processBillDuration, task.Type);
                    yield return MoveToTransform(GetWaiterTarget(seat), baseStopDistance);
                    yield return WaitInteraction(interactionDelay, task.Type);
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

            yield return MoveToPosition(
                ResolveNavigableDestination(point.WorldPosition),
                Mathf.Max(baseStopDistance, point.ArrivalRadius));
        }

        private IEnumerator MoveToTransform(Transform targetTransform, float stopDistance)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            yield return MoveToPosition(ResolveNavigableDestination(targetTransform.position), stopDistance);
        }

        private IEnumerator MoveToPosition(Vector3 worldPosition, float stopDistance)
        {
            while (!AdvanceTo(worldPosition, stopDistance))
            {
                yield return null;
            }
        }

        private IEnumerator WaitInteraction(float duration, WaiterTaskType taskType)
        {
            float sanitizedDuration = Mathf.Max(0.01f, duration);
            ResolveActionProgressView();

            if (actionProgressView == null)
            {
                yield return new WaitForSeconds(sanitizedDuration);
                yield break;
            }

            Color progressColor = GetProgressColor(taskType);
            float elapsed = 0f;
            while (elapsed < sanitizedDuration)
            {
                elapsed += Time.deltaTime;
                actionProgressView.SetProgress(elapsed / sanitizedDuration, progressColor);
                yield return null;
            }

            actionProgressView.SetProgress(1f, progressColor);
            actionProgressView.Hide();
        }

        private bool AdvanceTo(Vector3 targetPosition, float stopDistance)
        {
            ResolvePathMover();
            if (pathMover != null)
            {
                return pathMover.MoveTowards(targetPosition, stopDistance, moveSpeed, turnSpeed);
            }

            return AdvanceDirectlyTo(targetPosition, stopDistance);
        }

        private bool AdvanceDirectlyTo(Vector3 targetPosition, float stopDistance)
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

        private void ResolvePathMover()
        {
            if (pathMover == null)
            {
                pathMover = GetComponent<RestaurantPathMover>();
            }

            if (pathMover == null)
            {
                pathMover = gameObject.AddComponent<RestaurantPathMover>();
            }
        }

        private void CaptureBaseStats()
        {
            if (initialMoveSpeed <= 0f)
            {
                initialMoveSpeed = moveSpeed;
            }

            if (baseTakeOrderDuration <= 0f)
            {
                baseTakeOrderDuration = takeOrderDuration;
            }

            if (baseSubmitOrderDuration <= 0f)
            {
                baseSubmitOrderDuration = submitOrderDuration;
            }

            if (baseKitchenPickupDuration <= 0f)
            {
                baseKitchenPickupDuration = kitchenPickupDuration;
            }

            if (baseBarPickupDuration <= 0f)
            {
                baseBarPickupDuration = barPickupDuration;
            }
        }

        private static float EvaluateDuration(float baseDuration, float speedMultiplier)
        {
            float sanitizedBaseDuration = Mathf.Max(0.1f, baseDuration);
            float sanitizedSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            return Mathf.Max(0.1f, sanitizedBaseDuration / sanitizedSpeedMultiplier);
        }

        private void ResolveActionProgressView()
        {
            if (actionProgressView == null)
            {
                actionProgressView = GetComponent<WaiterActionProgressView>();
            }

            if (actionProgressView == null)
            {
                actionProgressView = gameObject.AddComponent<WaiterActionProgressView>();
            }
        }

        private Vector3 ResolveNavigableDestination(Vector3 targetPosition)
        {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(targetPosition, out hit, 1.5f, NavMesh.AllAreas))
            {
                return targetPosition;
            }

            Vector3 sampledPosition = new Vector3(hit.position.x, targetPosition.y, hit.position.z);
            Vector3 planarOffset = sampledPosition - targetPosition;
            planarOffset.y = 0f;
            if (planarOffset.sqrMagnitude <= 0.0004f)
            {
                return targetPosition;
            }

            return sampledPosition;
        }

        private void HideActionProgress()
        {
            if (actionProgressView != null)
            {
                actionProgressView.HideImmediate();
            }
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

        private static Color GetProgressColor(WaiterTaskType taskType)
        {
            switch (taskType)
            {
                case WaiterTaskType.TakeOrder:
                    return new Color(0.98f, 0.77f, 0.31f, 0.98f);
                case WaiterTaskType.SubmitOrder:
                    return new Color(0.37f, 0.8f, 1f, 0.98f);
                case WaiterTaskType.PickupKitchen:
                    return new Color(1f, 0.55f, 0.31f, 0.98f);
                case WaiterTaskType.PickupBar:
                    return new Color(0.39f, 0.89f, 0.92f, 0.98f);
                case WaiterTaskType.ProcessBill:
                    return new Color(0.47f, 0.86f, 0.45f, 0.98f);
                default:
                    return new Color(0.91f, 0.92f, 0.96f, 0.96f);
            }
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
