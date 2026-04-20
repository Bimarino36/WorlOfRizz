using UnityEngine;
using UnityEngine.AI;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantGuest : MonoBehaviour
    {
        private enum WaitPhase
        {
            None = 0,
            WaitingForOrder = 1,
            WaitingForDelivery = 2,
            WaitingForBill = 3
        }

        private enum GuestMode
        {
            Idle = 0,
            WalkingToSeat = 1,
            WaitingInQueue = 2,
            Eating = 3,
            Leaving = 4
        }

        [SerializeField, Min(0.5f)] private float moveSpeed = 1.9f;
        [SerializeField, Min(180f)] private float turnSpeed = 540f;
        [SerializeField, Range(0f, 1f)] private float serviceQuality = 1f;
        [SerializeField, Range(0f, 1f)] private float currentUrgencyNormalized;
        [SerializeField] private string waitPhaseLabel = "None";
        [SerializeField] private bool hasWalkedOut;
        [SerializeField] private RestaurantPathMover pathMover;

        private RestaurantRuntime runtime;
        private RestaurantSeat currentSeat;
        private RestaurantPoint exitPoint;
        private GuestMode mode;
        private float stateTimer;
        private Vector3 queueTargetPosition;
        private WaitPhase currentWaitPhase;
        private float waitElapsed;
        private float waitLimit;
        private float waitDecayStartNormalized;
        private bool allowWalkoutDuringCurrentPhase;

        private void Awake()
        {
            ResolvePathMover();
        }

        public RestaurantSeat CurrentSeat => currentSeat;

        public float ServiceQuality => serviceQuality;

        public float CurrentUrgencyNormalized => currentUrgencyNormalized;

        public bool HasWalkedOut => hasWalkedOut;

        public void Initialize(RestaurantRuntime ownerRuntime, RestaurantSeat seat, RestaurantPoint guestExitPoint)
        {
            ResolvePathMover();
            pathMover?.ClearPath();
            runtime = ownerRuntime;
            currentSeat = seat;
            exitPoint = guestExitPoint;
            mode = GuestMode.WalkingToSeat;
            serviceQuality = 1f;
            currentUrgencyNormalized = 0f;
            hasWalkedOut = false;
            queueTargetPosition = transform.position;
            ClearWaitState();
        }

        public void BeginQueueing(Vector3 queuePosition)
        {
            ResolvePathMover();
            pathMover?.ClearPath();
            ClearWaitState();
            queueTargetPosition = queuePosition;
            mode = GuestMode.WaitingInQueue;
        }

        public void RefreshQueuePosition(Vector3 queuePosition)
        {
            queueTargetPosition = queuePosition;
        }

        public void AssignSeat(RestaurantSeat seat)
        {
            if (seat == null)
            {
                return;
            }

            ResolvePathMover();
            pathMover?.ClearPath();
            currentSeat = seat;
            mode = GuestMode.WalkingToSeat;
        }

        public void BeginEating(float duration)
        {
            pathMover?.ClearPath();
            CompleteActiveWaitPhase();
            stateTimer = Mathf.Max(0.1f, duration);
            mode = GuestMode.Eating;
        }

        public void BeginLeaving()
        {
            ResolvePathMover();
            pathMover?.ClearPath();
            ClearWaitState();
            mode = GuestMode.Leaving;
        }

        public void BeginOrderWait(float limit, float decayStartNormalized, bool allowWalkout)
        {
            StartWaitPhase(WaitPhase.WaitingForOrder, limit, decayStartNormalized, allowWalkout);
        }

        public void BeginDeliveryWait(float limit, float decayStartNormalized)
        {
            StartWaitPhase(WaitPhase.WaitingForDelivery, limit, decayStartNormalized, false);
        }

        public void BeginBillWait(float limit, float decayStartNormalized)
        {
            StartWaitPhase(WaitPhase.WaitingForBill, limit, decayStartNormalized, false);
        }

        public void CompleteActiveWaitPhase()
        {
            if (currentWaitPhase == WaitPhase.None)
            {
                return;
            }

            serviceQuality *= EvaluateCurrentPhaseQualityFactor();
            serviceQuality = Mathf.Clamp01(serviceQuality);
            ClearWaitState();
        }

        private void Update()
        {
            switch (mode)
            {
                case GuestMode.WalkingToSeat:
                    UpdateWalkToSeat();
                    break;
                case GuestMode.WaitingInQueue:
                    UpdateQueueing();
                    break;
                case GuestMode.Eating:
                    UpdateEating();
                    break;
                case GuestMode.Leaving:
                    UpdateLeaving();
                    break;
            }

            if (mode == GuestMode.Idle)
            {
                UpdateWaiting();
            }
        }

        private void UpdateWalkToSeat()
        {
            if (currentSeat == null)
            {
                mode = GuestMode.WaitingInQueue;
                return;
            }

            Vector3 approachPosition = GetSeatApproachPosition();
            if (!MoveTowards(approachPosition, 0.08f))
            {
                return;
            }

            FinishSeating();
        }

        private void UpdateQueueing()
        {
            MoveTowards(queueTargetPosition, 0.04f);
        }

        private void UpdateEating()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer > 0f)
            {
                return;
            }

            mode = GuestMode.Idle;
            if (runtime != null)
            {
                runtime.NotifyGuestFinishedEating(this);
            }
        }

        private void UpdateLeaving()
        {
            if (exitPoint == null)
            {
                if (runtime != null)
                {
                    runtime.NotifyGuestExited(this);
                }

                Destroy(gameObject);
                return;
            }

            if (!MoveTowards(exitPoint.WorldPosition, exitPoint.ArrivalRadius))
            {
                return;
            }

            if (runtime != null)
            {
                runtime.NotifyGuestExited(this);
            }

            Destroy(gameObject);
        }

        private void UpdateWaiting()
        {
            if (currentWaitPhase == WaitPhase.None)
            {
                currentUrgencyNormalized = 0f;
                return;
            }

            waitElapsed += Time.deltaTime;
            currentUrgencyNormalized = waitLimit > 0.01f ? Mathf.Clamp01(waitElapsed / waitLimit) : 1f;

            if (!allowWalkoutDuringCurrentPhase || hasWalkedOut || waitElapsed < waitLimit)
            {
                return;
            }

            hasWalkedOut = true;
            ClearWaitState();

            if (runtime != null)
            {
                runtime.NotifyGuestWalkedOutBeforeOrdering(this);
            }
        }

        private void StartWaitPhase(WaitPhase waitPhase, float limit, float decayStartNormalized, bool allowWalkout)
        {
            CompleteActiveWaitPhase();

            currentWaitPhase = waitPhase;
            waitElapsed = 0f;
            waitLimit = Mathf.Max(0.1f, limit);
            waitDecayStartNormalized = Mathf.Clamp01(decayStartNormalized);
            allowWalkoutDuringCurrentPhase = allowWalkout;
            currentUrgencyNormalized = 0f;
            waitPhaseLabel = waitPhase.ToString();
        }

        private float EvaluateCurrentPhaseQualityFactor()
        {
            if (currentWaitPhase == WaitPhase.None || waitLimit <= 0.01f)
            {
                return 1f;
            }

            return ServiceQualityModel.EvaluateWaitPhaseQualityFactor(waitElapsed, waitLimit, waitDecayStartNormalized);
        }

        private void ClearWaitState()
        {
            currentWaitPhase = WaitPhase.None;
            waitElapsed = 0f;
            waitLimit = 0f;
            waitDecayStartNormalized = 0f;
            allowWalkoutDuringCurrentPhase = false;
            currentUrgencyNormalized = 0f;
            waitPhaseLabel = WaitPhase.None.ToString();
        }

        private bool MoveTowards(Vector3 targetPosition, float stopDistance)
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

        private Vector3 GetSeatApproachPosition()
        {
            if (currentSeat == null)
            {
                return transform.position;
            }

            Transform seatPoint = currentSeat.SeatPoint;
            Transform servicePoint = currentSeat.ServicePoint;
            if (servicePoint == null)
            {
                return seatPoint.position;
            }

            if (seatPoint == null)
            {
                return servicePoint.position;
            }

            float seatOffset = EstimateNavMeshOffset(seatPoint.position);
            float serviceOffset = EstimateNavMeshOffset(servicePoint.position);

            if (serviceOffset + 0.04f < seatOffset)
            {
                return servicePoint.position;
            }

            return seatPoint.position;
        }

        private void FinishSeating()
        {
            if (currentSeat == null)
            {
                return;
            }

            Vector3 seatPosition = currentSeat.SeatPoint.position;
            transform.position = new Vector3(seatPosition.x, transform.position.y, seatPosition.z);
            pathMover?.ClearPath();
            currentSeat.MarkGuestSeated();
            mode = GuestMode.Idle;
            if (runtime != null)
            {
                runtime.NotifyGuestSeated(this);
            }
        }

        private static float EstimateNavMeshOffset(Vector3 targetPosition)
        {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
            {
                return float.PositiveInfinity;
            }

            Vector3 delta = hit.position - targetPosition;
            delta.y = 0f;
            return delta.magnitude;
        }
    }
}
