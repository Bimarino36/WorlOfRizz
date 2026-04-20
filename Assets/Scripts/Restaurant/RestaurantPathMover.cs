using System;
using UnityEngine;
using UnityEngine.AI;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantPathMover : MonoBehaviour
    {
        private static readonly Vector3 InvalidVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        [SerializeField, Min(0.25f)] private float navMeshSampleDistance = 1.2f;
        [SerializeField, Min(0.02f)] private float waypointReachDistance = 0.08f;
        [SerializeField, Min(0.02f)] private float destinationChangeThreshold = 0.18f;

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
            transform.position = Vector3.MoveTowards(currentPosition, flattenedWaypoint, moveSpeed * Time.deltaTime);
            RotateTowards(flattenedWaypoint - currentPosition, turnSpeed);

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

            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
            RotateTowards(delta, turnSpeed);
            return false;
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
