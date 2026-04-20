using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantPathMover : MonoBehaviour
    {
        private static readonly Vector3 InvalidVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private static readonly List<RestaurantPathMover> ActiveMovers = new List<RestaurantPathMover>();

        [SerializeField, Min(0.25f)] private float navMeshSampleDistance = 1.2f;
        [SerializeField, Min(0.02f)] private float waypointReachDistance = 0.08f;
        [SerializeField, Min(0.02f)] private float destinationChangeThreshold = 0.18f;
        [SerializeField, Min(0.001f)] private float stalledWaypointDistance = 0.01f;
        [SerializeField, Min(0.05f)] private float characterAvoidanceRadius = 0.24f;
        [SerializeField, Min(0f)] private float characterAvoidanceStrength = 0.85f;
        [SerializeField, Min(0.01f)] private float characterAvoidanceMaxPush = 0.09f;

        private NavMeshPath navMeshPath;
        private Vector3[] pathCorners = Array.Empty<Vector3>();
        private Vector3 lastRequestedTarget = InvalidVector;
        private Vector3 sampledTargetPosition = InvalidVector;
        private float sampledTargetOffset;
        private int currentCornerIndex;
        private bool hasActiveNavMeshPath;

        private void Awake()
        {
            navMeshPath = new NavMeshPath();
            ClearPath();
        }

        private void OnEnable()
        {
            if (!ActiveMovers.Contains(this))
            {
                ActiveMovers.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveMovers.Remove(this);
        }

        private void OnDestroy()
        {
            ActiveMovers.Remove(this);
        }

        public void ClearPath()
        {
            hasActiveNavMeshPath = false;
            pathCorners = Array.Empty<Vector3>();
            currentCornerIndex = 0;
            lastRequestedTarget = InvalidVector;
            sampledTargetPosition = InvalidVector;
            sampledTargetOffset = 0f;
        }

        public bool MoveTowards(Vector3 worldTarget, float stopDistance, float moveSpeed, float turnSpeed)
        {
            Vector3 currentPosition = transform.position;
            Vector3 flattenedTarget = new Vector3(worldTarget.x, currentPosition.y, worldTarget.z);

            bool reached;
            if (TryAdvanceAlongNavMesh(flattenedTarget, stopDistance, moveSpeed, turnSpeed, out reached))
            {
                return reached;
            }

            return AdvanceDirect(flattenedTarget, stopDistance, moveSpeed, turnSpeed);
        }

        private bool TryAdvanceAlongNavMesh(Vector3 targetPosition, float stopDistance, float moveSpeed, float turnSpeed, out bool reached)
        {
            reached = false;
            if (!TryBuildPath(targetPosition))
            {
                return false;
            }

            Vector3 currentPosition = transform.position;
            float arrivalDistance = Mathf.Max(stopDistance, sampledTargetOffset + stopDistance + 0.05f);
            if (PlanarDistanceSqr(currentPosition, targetPosition) <= arrivalDistance * arrivalDistance)
            {
                ClearPath();
                reached = true;
                return true;
            }

            Vector3 waypoint = GetCurrentWaypoint(currentPosition, stopDistance);
            Vector3 flattenedWaypoint = new Vector3(waypoint.x, currentPosition.y, waypoint.z);
            if (PlanarDistanceSqr(currentPosition, flattenedWaypoint) <= stalledWaypointDistance * stalledWaypointDistance
                && PlanarDistanceSqr(currentPosition, targetPosition) > arrivalDistance * arrivalDistance)
            {
                ClearPath();
                return false;
            }

            float stepDistance = moveSpeed * Time.deltaTime;
            Vector3 desiredPosition = Vector3.MoveTowards(currentPosition, flattenedWaypoint, stepDistance);
            Vector3 nextPosition = ApplyCharacterAvoidance(currentPosition, desiredPosition, stepDistance);
            transform.position = nextPosition;
            RotateTowards(nextPosition - currentPosition, turnSpeed);

            Vector3 movedPosition = transform.position;
            if (PlanarDistanceSqr(movedPosition, targetPosition) <= arrivalDistance * arrivalDistance)
            {
                ClearPath();
                reached = true;
            }

            return true;
        }

        private bool TryBuildPath(Vector3 targetPosition)
        {
            if (navMeshPath == null)
            {
                navMeshPath = new NavMeshPath();
            }

            bool destinationChanged = !IsValid(lastRequestedTarget)
                || PlanarDistanceSqr(lastRequestedTarget, targetPosition) > destinationChangeThreshold * destinationChangeThreshold;

            if (!destinationChanged && hasActiveNavMeshPath && pathCorners.Length > 0 && currentCornerIndex < pathCorners.Length)
            {
                return true;
            }

            Vector3 sampledStart;
            Vector3 sampledEnd;
            if (!TrySamplePosition(transform.position, out sampledStart) || !TrySamplePosition(targetPosition, out sampledEnd))
            {
                ClearPath();
                return false;
            }

            if (!NavMesh.CalculatePath(sampledStart, sampledEnd, NavMesh.AllAreas, navMeshPath)
                || navMeshPath.status == NavMeshPathStatus.PathInvalid
                || navMeshPath.corners == null
                || navMeshPath.corners.Length == 0)
            {
                ClearPath();
                return false;
            }

            pathCorners = navMeshPath.corners;
            currentCornerIndex = pathCorners.Length > 1 ? 1 : 0;
            hasActiveNavMeshPath = true;
            lastRequestedTarget = targetPosition;
            sampledTargetPosition = new Vector3(sampledEnd.x, targetPosition.y, sampledEnd.z);
            sampledTargetOffset = Mathf.Sqrt(PlanarDistanceSqr(sampledTargetPosition, targetPosition));
            return true;
        }

        private Vector3 GetCurrentWaypoint(Vector3 currentPosition, float stopDistance)
        {
            float waypointDistance = Mathf.Max(waypointReachDistance, stopDistance + 0.02f);
            while (currentCornerIndex < pathCorners.Length - 1)
            {
                Vector3 currentCorner = new Vector3(pathCorners[currentCornerIndex].x, currentPosition.y, pathCorners[currentCornerIndex].z);
                if (PlanarDistanceSqr(currentPosition, currentCorner) > waypointDistance * waypointDistance)
                {
                    break;
                }

                currentCornerIndex++;
            }

            Vector3 waypoint = pathCorners[Mathf.Clamp(currentCornerIndex, 0, pathCorners.Length - 1)];
            return new Vector3(waypoint.x, currentPosition.y, waypoint.z);
        }

        private bool TrySamplePosition(Vector3 position, out Vector3 sampledPosition)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                sampledPosition = hit.position;
                return true;
            }

            sampledPosition = default;
            return false;
        }

        private bool AdvanceDirect(Vector3 targetPosition, float stopDistance, float moveSpeed, float turnSpeed)
        {
            Vector3 currentPosition = transform.position;
            Vector3 delta = targetPosition - currentPosition;
            delta.y = 0f;

            if (delta.sqrMagnitude <= stopDistance * stopDistance)
            {
                transform.position = targetPosition;
                ClearPath();
                return true;
            }

            float stepDistance = moveSpeed * Time.deltaTime;
            Vector3 desiredPosition = Vector3.MoveTowards(currentPosition, targetPosition, stepDistance);
            Vector3 nextPosition = ApplyCharacterAvoidance(currentPosition, desiredPosition, stepDistance);
            transform.position = nextPosition;
            RotateTowards(nextPosition - currentPosition, turnSpeed);
            return false;
        }

        private Vector3 ApplyCharacterAvoidance(Vector3 currentPosition, Vector3 desiredPosition, float maxStepDistance)
        {
            if (ActiveMovers.Count <= 1 || maxStepDistance <= 0.0001f)
            {
                return desiredPosition;
            }

            Vector3 desiredDelta = desiredPosition - currentPosition;
            desiredDelta.y = 0f;
            if (desiredDelta.sqrMagnitude <= 0.000001f)
            {
                return desiredPosition;
            }

            Vector3 accumulatedPush = Vector3.zero;
            float combinedRadiusSqrPadding = 0.0001f;
            for (int i = 0; i < ActiveMovers.Count; i++)
            {
                RestaurantPathMover other = ActiveMovers[i];
                if (other == null || other == this || !other.isActiveAndEnabled || !other.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 otherPosition = other.transform.position;
                if (Mathf.Abs(otherPosition.y - currentPosition.y) > 0.75f)
                {
                    continue;
                }

                otherPosition.y = currentPosition.y;
                float combinedRadius = Mathf.Max(0.05f, characterAvoidanceRadius + other.characterAvoidanceRadius);
                Vector3 offset = desiredPosition - otherPosition;
                offset.y = 0f;
                float distanceSqr = offset.sqrMagnitude;
                if (distanceSqr >= combinedRadius * combinedRadius - combinedRadiusSqrPadding)
                {
                    continue;
                }

                float distance = Mathf.Sqrt(Mathf.Max(distanceSqr, 0.000001f));
                float overlap = Mathf.Max(0f, combinedRadius - distance);
                Vector3 pushDirection = distance > 0.001f
                    ? offset / distance
                    : GetFallbackAvoidanceDirection(desiredDelta);
                accumulatedPush += pushDirection * overlap;
            }

            if (accumulatedPush.sqrMagnitude <= 0.000001f)
            {
                return desiredPosition;
            }

            Vector3 desiredDirection = desiredDelta.normalized;
            Vector3 forwardPush = Vector3.Project(accumulatedPush, desiredDirection);
            Vector3 lateralPush = accumulatedPush - forwardPush;
            Vector3 adjustedPush = lateralPush + (forwardPush * 0.25f);
            adjustedPush = Vector3.ClampMagnitude(
                adjustedPush * Mathf.Max(0f, characterAvoidanceStrength),
                Mathf.Min(characterAvoidanceMaxPush, maxStepDistance));

            Vector3 adjustedDelta = desiredDelta + adjustedPush;
            float forwardDistance = Vector3.Dot(adjustedDelta, desiredDirection);
            if (forwardDistance < 0f)
            {
                adjustedDelta -= desiredDirection * forwardDistance;
            }

            adjustedDelta = Vector3.ClampMagnitude(adjustedDelta, maxStepDistance);
            return new Vector3(
                currentPosition.x + adjustedDelta.x,
                currentPosition.y,
                currentPosition.z + adjustedDelta.z);
        }

        private Vector3 GetFallbackAvoidanceDirection(Vector3 desiredDelta)
        {
            Vector3 planarDesired = desiredDelta;
            planarDesired.y = 0f;
            if (planarDesired.sqrMagnitude > 0.0001f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, planarDesired.normalized);
                if (side.sqrMagnitude > 0.0001f)
                {
                    return side.normalized;
                }
            }

            Vector3 planarForward = transform.forward;
            planarForward.y = 0f;
            if (planarForward.sqrMagnitude > 0.0001f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, planarForward.normalized);
                if (side.sqrMagnitude > 0.0001f)
                {
                    return side.normalized;
                }
            }

            return Vector3.right;
        }

        private void RotateTowards(Vector3 delta, float turnSpeed)
        {
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        private static float PlanarDistanceSqr(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        private static bool IsValid(Vector3 value)
        {
            return !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z);
        }
    }
}
