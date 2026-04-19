using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleRestaurant.Meta
{
    public static class SceneTransitionService
    {
        public static bool TryLoadRestaurant()
        {
            return TryLoadScene(GameSceneCatalog.RestaurantMain);
        }

        public static bool TryReloadActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrWhiteSpace(activeScene.name))
            {
                return false;
            }

            SceneManager.LoadScene(activeScene.name);
            return true;
        }

        public static bool TryLoadPortal(MetaPortalId portalId)
        {
            if (!MetaProgressService.IsPortalUnlocked(portalId))
            {
                return false;
            }

            return TryLoadScene(GetScenePath(portalId));
        }

        public static bool CanLoadPortal(MetaPortalId portalId)
        {
            return MetaProgressService.IsPortalUnlocked(portalId) && CanLoadScene(GetScenePath(portalId));
        }

        public static string GetSceneName(MetaPortalId portalId)
        {
            switch (portalId)
            {
                case MetaPortalId.Adventure:
                    return GameSceneCatalog.AdventureWorld;
                case MetaPortalId.Farm:
                    return GameSceneCatalog.FarmGarden;
                default:
                    return string.Empty;
            }
        }

        public static string GetScenePath(MetaPortalId portalId)
        {
            switch (portalId)
            {
                case MetaPortalId.Adventure:
                    return GameSceneCatalog.AdventureWorldPath;
                case MetaPortalId.Farm:
                    return GameSceneCatalog.FarmGardenPath;
                default:
                    return string.Empty;
            }
        }

        public static bool TryLoadScene(string sceneNameOrPath)
        {
            string resolvedScenePath = ResolveScenePath(sceneNameOrPath);
            if (!CanLoadScene(resolvedScenePath))
            {
                return false;
            }

            SceneManager.LoadScene(resolvedScenePath);
            return true;
        }

        public static bool CanLoadScene(string sceneNameOrPath)
        {
            string resolvedScenePath = ResolveScenePath(sceneNameOrPath);
            if (string.IsNullOrWhiteSpace(resolvedScenePath))
            {
                return false;
            }

            if (Application.CanStreamedLevelBeLoaded(resolvedScenePath))
            {
                return true;
            }

            return SceneUtility.GetBuildIndexByScenePath(resolvedScenePath) >= 0;
        }

        private static string ResolveScenePath(string sceneNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(sceneNameOrPath))
            {
                return string.Empty;
            }

            switch (sceneNameOrPath)
            {
                case GameSceneCatalog.RestaurantMain:
                case GameSceneCatalog.RestaurantMainPath:
                    return GameSceneCatalog.RestaurantMainPath;
                case GameSceneCatalog.AdventureWorld:
                case GameSceneCatalog.AdventureWorldPath:
                    return GameSceneCatalog.AdventureWorldPath;
                case GameSceneCatalog.FarmGarden:
                case GameSceneCatalog.FarmGardenPath:
                    return GameSceneCatalog.FarmGardenPath;
                default:
                    return sceneNameOrPath;
            }
        }
    }
}
