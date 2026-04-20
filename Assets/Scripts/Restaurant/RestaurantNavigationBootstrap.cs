using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IdleRestaurant.Gameplay
{
    public static class RestaurantNavigationBootstrap
    {
        private static readonly string[] IgnoredNameTokens =
        {
            "waiter",
            "guest",
            "runtimehud",
            "eventsystem",
            "label"
        };

        private const float BoundsPadding = 2.5f;
        private const float AgentRadius = 0.16f;
        private const float AgentHeight = 0.65f;
        private const float AgentClimb = 0.04f;
        private const float AgentSlope = 50f;

        private static NavMeshDataInstance currentNavMeshInstance;
        private static int builtSceneHandle = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildAfterSceneLoad()
        {
            RestaurantRuntime runtime = Object.FindAnyObjectByType<RestaurantRuntime>();
            if (runtime != null)
            {
                EnsureBuilt(runtime);
            }
        }

        public static bool EnsureBuilt(RestaurantRuntime runtime)
        {
            if (runtime == null)
            {
                return false;
            }

            int sceneHandle = runtime.gameObject.scene.handle;
            if (sceneHandle == builtSceneHandle && currentNavMeshInstance.valid)
            {
                return true;
            }

            RemoveCurrentNavMesh();

            List<Transform> sourceRoots = ResolveSourceRoots(runtime);
            if (sourceRoots.Count == 0)
            {
                Debug.LogWarning("RestaurantNavigationBootstrap: navigation roots were not found.");
                return false;
            }

            List<NavMeshBuildMarkup> markups = CollectIgnoredMarkups(sourceRoots);
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>(256);
            for (int index = 0; index < sourceRoots.Count; index++)
            {
                if (sourceRoots[index] == null)
                {
                    continue;
                }

                NavMeshBuilder.CollectSources(
                    sourceRoots[index],
                    ~0,
                    NavMeshCollectGeometry.PhysicsColliders,
                    0,
                    markups,
                    sources);
            }

            if (sources.Count == 0)
            {
                Debug.LogWarning("RestaurantNavigationBootstrap: no physics colliders were collected for navigation.");
                return false;
            }

            Bounds buildBounds;
            if (!TryCalculateBounds(sourceRoots, out buildBounds))
            {
                buildBounds = new Bounds(runtime.transform.position, Vector3.one * 48f);
            }

            NavMeshBuildSettings settings = ResolveBuildSettings();
            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(settings, sources, buildBounds, Vector3.zero, Quaternion.identity);
            if (navMeshData == null)
            {
                Debug.LogWarning("RestaurantNavigationBootstrap: failed to build navigation data.");
                return false;
            }

            currentNavMeshInstance = NavMesh.AddNavMeshData(navMeshData);
            builtSceneHandle = sceneHandle;
            return currentNavMeshInstance.valid;
        }

        private static List<Transform> ResolveSourceRoots(RestaurantRuntime runtime)
        {
            List<Transform> roots = new List<Transform>(2);
            if (runtime != null)
            {
                roots.Add(runtime.transform);
            }

            GameObject[] sceneRoots = runtime.gameObject.scene.GetRootGameObjects();
            for (int index = 0; index < sceneRoots.Length; index++)
            {
                GameObject rootObject = sceneRoots[index];
                if (rootObject == null || rootObject.name != "Environment")
                {
                    continue;
                }

                Transform environmentRoot = rootObject.transform.Find("RestaurantInterior");
                roots.Add(environmentRoot != null ? environmentRoot : rootObject.transform);
                break;
            }

            return roots;
        }

        private static List<NavMeshBuildMarkup> CollectIgnoredMarkups(List<Transform> roots)
        {
            List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>(96);
            for (int rootIndex = 0; rootIndex < roots.Count; rootIndex++)
            {
                Transform root = roots[rootIndex];
                if (root == null)
                {
                    continue;
                }

                Transform[] children = root.GetComponentsInChildren<Transform>(true);
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                {
                    Transform child = children[childIndex];
                    if (child == null || !ShouldIgnoreTransform(child))
                    {
                        continue;
                    }

                    markups.Add(new NavMeshBuildMarkup
                    {
                        root = child,
                        applyToChildren = true,
                        overrideIgnore = true,
                        ignoreFromBuild = true
                    });
                }
            }

            return markups;
        }

        private static bool TryCalculateBounds(List<Transform> roots, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            for (int rootIndex = 0; rootIndex < roots.Count; rootIndex++)
            {
                Transform root = roots[rootIndex];
                if (root == null)
                {
                    continue;
                }

                Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    Collider collider = colliders[colliderIndex];
                    if (collider == null || !collider.enabled || ShouldIgnoreTransform(collider.transform))
                    {
                        continue;
                    }

                    if (!hasBounds)
                    {
                        bounds = collider.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            bounds.Expand(Vector3.one * BoundsPadding);
            return true;
        }

        private static NavMeshBuildSettings ResolveBuildSettings()
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsCount() > 0
                ? NavMesh.GetSettingsByIndex(0)
                : NavMesh.CreateSettings();

            settings.agentRadius = AgentRadius;
            settings.agentHeight = AgentHeight;
            settings.agentClimb = AgentClimb;
            settings.agentSlope = AgentSlope;
            settings.minRegionArea = 0.01f;
            settings.overrideVoxelSize = false;
            settings.overrideTileSize = false;
            return settings;
        }

        private static bool ShouldIgnoreTransform(Transform target)
        {
            if (target == null)
            {
                return true;
            }

            if (target.GetComponent<RestaurantRuntime>() != null
                || target.GetComponent<RestaurantHud>() != null
                || target.GetComponent<RestaurantOperationsHud>() != null
                || target.GetComponent<RestaurantPoint>() != null
                || target.GetComponent<RestaurantSeat>() != null
                || target.GetComponent<RestaurantGuest>() != null
                || target.GetComponent<WaiterAgent>() != null
                || target.GetComponent<WorldBillboardLabel>() != null
                || target.GetComponent<Canvas>() != null)
            {
                return true;
            }

            string name = target.name.ToLowerInvariant();
            for (int index = 0; index < IgnoredNameTokens.Length; index++)
            {
                if (name.Contains(IgnoredNameTokens[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveCurrentNavMesh()
        {
            if (currentNavMeshInstance.valid)
            {
                NavMesh.RemoveNavMeshData(currentNavMeshInstance);
            }

            currentNavMeshInstance = default;
            builtSceneHandle = -1;
        }
    }
}
