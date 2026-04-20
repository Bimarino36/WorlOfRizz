using UnityEngine;
using IdleRestaurant.Localization;

namespace IdleRestaurant.Gameplay
{
    public enum RestaurantActorRole
    {
        Cook = 0,
        Bartender = 1,
        Manager = 2
    }

    public sealed class RestaurantActorStatusView : MonoBehaviour
    {
        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField] private RestaurantActorRole role;
        [SerializeField] private WorldBillboardLabel worldLabel;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Vector3 labelOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField, Min(0.02f)] private float labelCharacterSize = 0.06f;
        [SerializeField, Min(1)] private int labelFontSize = 64;

        private MaterialPropertyBlock propertyBlock;
        private string lastLabelText = string.Empty;
        private Color lastLabelColor = Color.clear;
        private Color lastTintColor = Color.clear;

        public void Configure(RestaurantRuntime ownerRuntime, RestaurantActorRole actorRole, Vector3 offset)
        {
            runtime = ownerRuntime;
            role = actorRole;
            labelOffset = offset;
            ResolveReferences();
            ApplyLabelLayout();
        }

        private void Awake()
        {
            ResolveReferences();
            ApplyLabelLayout();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
            ApplyLabelLayout();
        }
#endif

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveReferences();
            if (runtime == null || worldLabel == null)
            {
                return;
            }

            UpdatePresentation();
        }

        private void ResolveReferences()
        {
            if (runtime == null)
            {
                runtime = GetComponentInParent<RestaurantRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<RestaurantRuntime>();
            }

            if (worldLabel == null)
            {
                worldLabel = GetComponent<WorldBillboardLabel>();
            }

            if (worldLabel == null)
            {
                worldLabel = gameObject.AddComponent<WorldBillboardLabel>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
        }

        private void ApplyLabelLayout()
        {
            if (worldLabel == null)
            {
                return;
            }

            worldLabel.SetLocalOffset(labelOffset);
            worldLabel.SetCharacterSize(labelCharacterSize);
            worldLabel.SetFontSize(labelFontSize);
        }

        private void UpdatePresentation()
        {
            string labelText;
            Color color;

            switch (role)
            {
                case RestaurantActorRole.Cook:
                    BuildCookPresentation(out labelText, out color);
                    break;

                case RestaurantActorRole.Bartender:
                    BuildBartenderPresentation(out labelText, out color);
                    break;

                default:
                    BuildManagerPresentation(out labelText, out color);
                    break;
            }

            if (lastLabelText != labelText)
            {
                worldLabel.SetText(labelText);
                lastLabelText = labelText;
            }

            if (lastLabelColor != color)
            {
                worldLabel.SetColor(color);
                lastLabelColor = color;
            }

            ApplyRendererTint(color);
        }

        private void BuildCookPresentation(out string labelText, out Color color)
        {
            int readyCount = runtime.GetKitchenReadyOrderCount();
            int preparingCount = runtime.GetKitchenPreparingCount();
            if (readyCount > 0)
            {
                labelText = LocalizationService.Format("rest.actor.cook_ready", readyCount);
                color = new Color(0.34f, 0.92f, 1f, 1f);
                return;
            }

            if (preparingCount > 0)
            {
                labelText = LocalizationService.Format("rest.actor.cook_cooking", preparingCount);
                color = new Color(1f, 0.77f, 0.26f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.actor.cook_idle");
            color = new Color(0.76f, 0.86f, 0.76f, 1f);
        }

        private void BuildBartenderPresentation(out string labelText, out Color color)
        {
            int readyCount = runtime.GetBarReadyOrderCount();
            int preparingCount = runtime.GetBarPreparingCount();
            if (readyCount > 0)
            {
                labelText = LocalizationService.Format("rest.actor.bar_ready", readyCount);
                color = new Color(0.28f, 0.9f, 0.95f, 1f);
                return;
            }

            if (preparingCount > 0)
            {
                labelText = LocalizationService.Format("rest.actor.bar_mixing", preparingCount);
                color = new Color(1f, 0.72f, 0.34f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.actor.bar_idle");
            color = new Color(0.78f, 0.84f, 0.89f, 1f);
        }

        private void BuildManagerPresentation(out string labelText, out Color color)
        {
            int dirtyTables = runtime.GetCleanupTableCount();
            int waitingBills = runtime.GetWaitingBillCount();
            int queueGuests = runtime.QueueGuestCount;

            if (runtime.LoyaltyScore < 0)
            {
                labelText = LocalizationService.Get("rest.actor.manager_mood_down");
                color = new Color(1f, 0.45f, 0.38f, 1f);
                return;
            }

            if (dirtyTables > 0)
            {
                labelText = LocalizationService.Format("rest.actor.manager_clean", dirtyTables);
                color = new Color(1f, 0.67f, 0.31f, 1f);
                return;
            }

            if (waitingBills > 0)
            {
                labelText = LocalizationService.Format("rest.actor.manager_bills", waitingBills);
                color = new Color(0.95f, 0.87f, 0.35f, 1f);
                return;
            }

            if (queueGuests > 0)
            {
                labelText = LocalizationService.Format("rest.actor.manager_queue", queueGuests);
                color = new Color(0.78f, 0.9f, 0.42f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.actor.manager_watching");
            color = new Color(0.76f, 0.9f, 0.8f, 1f);
        }

        private void ApplyRendererTint(Color color)
        {
            if (targetRenderer == null)
            {
                return;
            }

            if (lastTintColor == color)
            {
                return;
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            targetRenderer.SetPropertyBlock(propertyBlock);
            lastTintColor = color;
        }
    }
}
