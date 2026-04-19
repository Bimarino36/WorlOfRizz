using System.Collections.Generic;
using System.Text;
using IdleRestaurant.Localization;
using IdleRestaurant.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantMetaPreviewHud : MonoBehaviour
    {
        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField] private RestaurantMetaPreviewBridge previewBridge;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private RectTransform resourceStripRoot;
        [SerializeField] private Text resourceStripText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Text claimButtonText;
        [SerializeField] private RectTransform specialOrdersRoot;
        [SerializeField] private Text specialOrdersTitleText;
        [SerializeField] private Text specialOrdersBodyText;
        [SerializeField] private Button orderOneButton;
        [SerializeField] private Text orderOneButtonText;
        [SerializeField] private Button orderTwoButton;
        [SerializeField] private Text orderTwoButtonText;
        [SerializeField] private Button adventureButton;
        [SerializeField] private Text adventureButtonText;
        [SerializeField] private Button farmButton;
        [SerializeField] private Text farmButtonText;

        private bool buttonHandlersBound;
        private string lastResourceValue = string.Empty;
        private string lastOrdersValue = string.Empty;
        private string lastClaimValue = string.Empty;
        private string lastOrderOneValue = string.Empty;
        private string lastOrderTwoValue = string.Empty;

        public void Configure(RestaurantRuntime configuredRuntime, RestaurantMetaPreviewBridge configuredBridge)
        {
            runtime = configuredRuntime;
            previewBridge = configuredBridge;
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureHud();
            WireButtons();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveReferences();
            EnsureHud();
            WireButtons();
            UpdateHud();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
            EnsureHud();
        }
#endif

        private void ResolveReferences()
        {
            if (runtime == null)
            {
                runtime = GetComponent<RestaurantRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<RestaurantRuntime>();
            }

            if (previewBridge == null)
            {
                previewBridge = GetComponent<RestaurantMetaPreviewBridge>();
            }

            if (previewBridge == null)
            {
                previewBridge = FindAnyObjectByType<RestaurantMetaPreviewBridge>();
            }

            if (safeAreaRoot == null)
            {
                Transform safeAreaTransform = transform.Find("RuntimeHudCanvas/SafeAreaRoot");
                safeAreaRoot = safeAreaTransform as RectTransform;
            }
        }

        private void EnsureHud()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            EnsureResourceStrip();
            EnsureSpecialOrdersPanel();
            EnsureButtons();
        }

        private void EnsureResourceStrip()
        {
            if (resourceStripRoot == null)
            {
                Transform existing = safeAreaRoot.Find("MetaResourceStrip");
                resourceStripRoot = existing as RectTransform;
            }

            if (resourceStripRoot == null)
            {
                GameObject stripObject = new GameObject("MetaResourceStrip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                resourceStripRoot = stripObject.GetComponent<RectTransform>();
                resourceStripRoot.SetParent(safeAreaRoot, false);

                Image stripImage = stripObject.GetComponent<Image>();
                stripImage.color = new Color(0.08f, 0.1f, 0.13f, 0.82f);
            }

            resourceStripRoot.anchorMin = new Vector2(0.5f, 1f);
            resourceStripRoot.anchorMax = new Vector2(0.5f, 1f);
            resourceStripRoot.pivot = new Vector2(0.5f, 1f);
            resourceStripRoot.sizeDelta = new Vector2(472f, 44f);
            resourceStripRoot.anchoredPosition = new Vector2(0f, -18f);

            if (resourceStripText == null)
            {
                Transform existingText = resourceStripRoot.Find("ResourceText");
                if (existingText != null)
                {
                    resourceStripText = existingText.GetComponent<Text>();
                }
            }

            if (resourceStripText == null)
            {
                GameObject textObject = new GameObject("ResourceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.SetParent(resourceStripRoot, false);
                resourceStripText = textObject.GetComponent<Text>();
                resourceStripText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                resourceStripText.fontSize = 15;
                resourceStripText.alignment = TextAnchor.MiddleLeft;
                resourceStripText.color = new Color(0.94f, 0.95f, 0.96f, 1f);
            }

            RectTransform resourceTextRect = resourceStripText.rectTransform;
            resourceTextRect.anchorMin = Vector2.zero;
            resourceTextRect.anchorMax = Vector2.one;
            resourceTextRect.offsetMin = new Vector2(14f, 8f);
            resourceTextRect.offsetMax = new Vector2(-118f, -8f);

            EnsureStripButton(
                ref claimButton,
                ref claimButtonText,
                "ClaimButton",
                LocalizationService.Get("common.claim_none"),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-8f, 0f),
                new Vector2(100f, 30f),
                new Color(0.21f, 0.46f, 0.61f, 0.96f));
        }

        private void EnsureSpecialOrdersPanel()
        {
            if (specialOrdersRoot == null)
            {
                Transform existing = safeAreaRoot.Find("SpecialOrdersPanel");
                specialOrdersRoot = existing as RectTransform;
            }

            if (specialOrdersRoot == null)
            {
                GameObject panelObject = new GameObject("SpecialOrdersPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                specialOrdersRoot = panelObject.GetComponent<RectTransform>();
                specialOrdersRoot.SetParent(safeAreaRoot, false);

                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.08f, 0.1f, 0.13f, 0.86f);
            }

            specialOrdersRoot.anchorMin = new Vector2(1f, 1f);
            specialOrdersRoot.anchorMax = new Vector2(1f, 1f);
            specialOrdersRoot.pivot = new Vector2(1f, 1f);
            specialOrdersRoot.sizeDelta = new Vector2(320f, 282f);
            specialOrdersRoot.anchoredPosition = new Vector2(-18f, -162f);

            if (specialOrdersTitleText == null)
            {
                Transform existingTitle = specialOrdersRoot.Find("TitleText");
                if (existingTitle != null)
                {
                    specialOrdersTitleText = existingTitle.GetComponent<Text>();
                }
            }

            if (specialOrdersTitleText == null)
            {
                GameObject titleObject = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform titleRect = titleObject.GetComponent<RectTransform>();
                titleRect.SetParent(specialOrdersRoot, false);
                specialOrdersTitleText = titleObject.GetComponent<Text>();
                specialOrdersTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                specialOrdersTitleText.fontSize = 20;
                specialOrdersTitleText.alignment = TextAnchor.UpperLeft;
                specialOrdersTitleText.color = new Color(0.96f, 0.96f, 0.96f, 1f);
            }

            RectTransform specialOrdersTitleRect = specialOrdersTitleText.rectTransform;
            specialOrdersTitleRect.anchorMin = new Vector2(0f, 1f);
            specialOrdersTitleRect.anchorMax = new Vector2(1f, 1f);
            specialOrdersTitleRect.pivot = new Vector2(0.5f, 1f);
            specialOrdersTitleRect.offsetMin = new Vector2(14f, -32f);
            specialOrdersTitleRect.offsetMax = new Vector2(-14f, -8f);

            if (specialOrdersBodyText == null)
            {
                Transform existingBody = specialOrdersRoot.Find("BodyText");
                if (existingBody != null)
                {
                    specialOrdersBodyText = existingBody.GetComponent<Text>();
                }
            }

            if (specialOrdersBodyText == null)
            {
                GameObject bodyObject = new GameObject("BodyText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
                bodyRect.SetParent(specialOrdersRoot, false);
                specialOrdersBodyText = bodyObject.GetComponent<Text>();
                specialOrdersBodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                specialOrdersBodyText.fontSize = 13;
                specialOrdersBodyText.alignment = TextAnchor.UpperLeft;
                specialOrdersBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                specialOrdersBodyText.verticalOverflow = VerticalWrapMode.Overflow;
                specialOrdersBodyText.color = new Color(0.9f, 0.92f, 0.94f, 1f);
            }

            RectTransform specialOrdersBodyRect = specialOrdersBodyText.rectTransform;
            specialOrdersBodyRect.anchorMin = new Vector2(0f, 0f);
            specialOrdersBodyRect.anchorMax = new Vector2(1f, 1f);
            specialOrdersBodyRect.offsetMin = new Vector2(14f, 100f);
            specialOrdersBodyRect.offsetMax = new Vector2(-14f, -40f);
        }

        private void EnsureButtons()
        {
            EnsurePanelButton(
                ref orderOneButton,
                ref orderOneButtonText,
                "OrderOneButton",
                LocalizationService.Get("farm.order.default"),
                new Vector2(14f, 56f),
                new Vector2(142f, 32f),
                new Color(0.47f, 0.32f, 0.16f, 0.96f));

            EnsurePanelButton(
                ref orderTwoButton,
                ref orderTwoButtonText,
                "OrderTwoButton",
                LocalizationService.Get("farm.order.default"),
                new Vector2(164f, 56f),
                new Vector2(142f, 32f),
                new Color(0.48f, 0.24f, 0.18f, 0.96f));

            EnsurePanelButton(
                ref adventureButton,
                ref adventureButtonText,
                "AdventureButton",
                LocalizationService.Get("common.adventure"),
                new Vector2(14f, 12f),
                new Vector2(142f, 36f),
                new Color(0.2f, 0.42f, 0.62f, 0.95f));

            EnsurePanelButton(
                ref farmButton,
                ref farmButtonText,
                "FarmButton",
                LocalizationService.Get("common.farm"),
                new Vector2(164f, 12f),
                new Vector2(142f, 36f),
                new Color(0.23f, 0.52f, 0.34f, 0.95f));
        }

        private void EnsureStripButton(
            ref Button button,
            ref Text label,
            string name,
            string buttonText,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size,
            Color backgroundColor)
        {
            if (resourceStripRoot == null)
            {
                return;
            }

            if (button == null)
            {
                Transform existing = resourceStripRoot.Find(name);
                if (existing != null)
                {
                    button = existing.GetComponent<Button>();
                    label = existing.GetComponentInChildren<Text>();
                }
            }

            if (button == null)
            {
                GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.SetParent(resourceStripRoot, false);

                Image buttonImage = buttonObject.GetComponent<Image>();
                buttonImage.color = backgroundColor;

                button = buttonObject.GetComponent<Button>();
                button.targetGraphic = buttonImage;

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 5f);
                labelRect.offsetMax = new Vector2(-8f, -5f);

                label = labelObject.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 13;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
            }

            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            buttonTransform.anchorMin = anchorMin;
            buttonTransform.anchorMax = anchorMax;
            buttonTransform.pivot = pivot;
            buttonTransform.anchoredPosition = anchoredPosition;
            buttonTransform.sizeDelta = size;

            if (label != null)
            {
                label.text = buttonText;
            }
        }

        private void EnsurePanelButton(
            ref Button button,
            ref Text label,
            string name,
            string buttonText,
            Vector2 anchoredPosition,
            Vector2 size,
            Color backgroundColor)
        {
            if (specialOrdersRoot == null)
            {
                return;
            }

            if (button == null)
            {
                Transform existing = specialOrdersRoot.Find(name);
                if (existing != null)
                {
                    button = existing.GetComponent<Button>();
                    label = existing.GetComponentInChildren<Text>();
                }
            }

            if (button == null)
            {
                GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.SetParent(specialOrdersRoot, false);

                Image buttonImage = buttonObject.GetComponent<Image>();
                buttonImage.color = backgroundColor;

                button = buttonObject.GetComponent<Button>();
                button.targetGraphic = buttonImage;

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 4f);
                labelRect.offsetMax = new Vector2(-8f, -4f);

                label = labelObject.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 13;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
            }

            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            buttonTransform.anchorMin = new Vector2(0f, 0f);
            buttonTransform.anchorMax = new Vector2(0f, 0f);
            buttonTransform.pivot = new Vector2(0f, 0f);
            buttonTransform.anchoredPosition = anchoredPosition;
            buttonTransform.sizeDelta = size;

            if (label != null)
            {
                label.text = buttonText;
            }
        }

        private void WireButtons()
        {
            if (buttonHandlersBound ||
                claimButton == null ||
                orderOneButton == null ||
                orderTwoButton == null ||
                adventureButton == null ||
                farmButton == null ||
                previewBridge == null)
            {
                return;
            }

            claimButton.onClick.AddListener(previewBridge.TryClaimPendingRestaurantCoins);
            orderOneButton.onClick.AddListener(HandleOrderOnePressed);
            orderTwoButton.onClick.AddListener(HandleOrderTwoPressed);
            adventureButton.onClick.AddListener(previewBridge.TryOpenAdventurePortal);
            farmButton.onClick.AddListener(previewBridge.TryOpenFarmPortal);
            buttonHandlersBound = true;
        }

        private void HandleOrderOnePressed()
        {
            if (previewBridge != null)
            {
                previewBridge.TryCompleteSpecialOrder(0);
            }
        }

        private void HandleOrderTwoPressed()
        {
            if (previewBridge != null)
            {
                previewBridge.TryCompleteSpecialOrder(1);
            }
        }

        private void UpdateHud()
        {
            if (previewBridge == null || resourceStripText == null || specialOrdersBodyText == null || specialOrdersTitleText == null)
            {
                return;
            }

            MetaResourceSnapshot snapshot = previewBridge.GetResourceSnapshot();
            string nextResources =
                LocalizationService.Get("common.resource.rare") + " " + snapshot.RareResources +
                "   |   " + LocalizationService.Get("common.resource.seeds") + " " + snapshot.Seeds +
                "   |   " + LocalizationService.Get("common.resource.ingredients_short") + " " + snapshot.Ingredients;

            if (lastResourceValue != nextResources)
            {
                resourceStripText.text = nextResources;
                lastResourceValue = nextResources;
            }

            IReadOnlyList<SpecialOrderStatus> statuses = previewBridge.GetSpecialOrderStatuses();
            string nextOrders = BuildSpecialOrdersText(statuses);
            if (lastOrdersValue != nextOrders)
            {
                specialOrdersTitleText.text = LocalizationService.Get("rest.contracts.title");
                specialOrdersBodyText.text = nextOrders;
                lastOrdersValue = nextOrders;
            }

            UpdateClaimButton();
            UpdateOrderButtons();
            UpdatePortalButtons();
        }

        private void UpdateClaimButton()
        {
            if (claimButton == null || claimButtonText == null || previewBridge == null)
            {
                return;
            }

            string nextLabel = previewBridge.GetClaimButtonLabel();
            if (lastClaimValue != nextLabel)
            {
                claimButtonText.text = nextLabel;
                lastClaimValue = nextLabel;
            }

            SetButtonVisual(
                claimButton,
                claimButtonText,
                previewBridge.CanClaimPendingRestaurantCoins(),
                new Color(0.21f, 0.46f, 0.61f, 0.96f));
        }

        private void UpdateOrderButtons()
        {
            UpdateOrderButton(
                orderOneButton,
                orderOneButtonText,
                previewBridge != null ? previewBridge.GetSpecialOrderActionLabel(0) : LocalizationService.Get("farm.order.default"),
                previewBridge != null && previewBridge.CanCompleteSpecialOrder(0),
                ref lastOrderOneValue);

            UpdateOrderButton(
                orderTwoButton,
                orderTwoButtonText,
                previewBridge != null ? previewBridge.GetSpecialOrderActionLabel(1) : LocalizationService.Get("farm.order.default"),
                previewBridge != null && previewBridge.CanCompleteSpecialOrder(1),
                ref lastOrderTwoValue);
        }

        private void UpdatePortalButtons()
        {
            if (adventureButtonText != null)
            {
                adventureButtonText.text = LocalizationService.Get("common.adventure");
            }

            if (farmButtonText != null)
            {
                farmButtonText.text = LocalizationService.Get("common.farm");
            }
        }

        private static void UpdateOrderButton(
            Button button,
            Text label,
            string nextLabel,
            bool interactable,
            ref string lastValue)
        {
            if (button == null || label == null)
            {
                return;
            }

            if (lastValue != nextLabel)
            {
                label.text = nextLabel;
                lastValue = nextLabel;
            }

            SetButtonVisual(button, label, interactable, new Color(0.57f, 0.34f, 0.12f, 0.96f));
        }

        private static string BuildSpecialOrdersText(IReadOnlyList<SpecialOrderStatus> statuses)
        {
            StringBuilder builder = new StringBuilder(384);
            builder.Append(LocalizationService.Get("rest.contracts.intro"));

            for (int index = 0; index < statuses.Count; index++)
            {
                SpecialOrderStatus status = statuses[index];
                builder.Append("\n\n");
                builder.Append(index + 1).Append(". ").Append(status.Title);
                builder.Append("\n").Append(LocalizationService.Format("rest.contracts.need", BuildRequirementsText(status.Requirements)));
                builder.Append("\n").Append(LocalizationService.Format("rest.contracts.reward", status.RewardLabel));

                if (status.IsComplete)
                {
                    builder.Append("\n").Append(LocalizationService.Format("rest.contracts.status", LocalizationService.Get("common.status.complete")));
                }
                else if (status.IsUnlocked)
                {
                    builder.Append("\n").Append(LocalizationService.Format("rest.contracts.status", LocalizationService.Get("common.status.ready_to_serve")));
                }
                else
                {
                    builder.Append("\n")
                        .Append(LocalizationService.Format(
                            "rest.contracts.status",
                            string.IsNullOrWhiteSpace(status.LockReason)
                                ? LocalizationService.Get("common.status.locked")
                                : status.LockReason));
                }
            }

            return builder.ToString();
        }

        private static string BuildRequirementsText(IReadOnlyList<SpecialOrderRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0)
            {
                return LocalizationService.Get("common.requirements.none");
            }

            StringBuilder builder = new StringBuilder(64);
            for (int index = 0; index < requirements.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                SpecialOrderRequirement requirement = requirements[index];
                builder.Append(GetResourceLabel(requirement.ResourceKind));
                builder.Append(" x");
                builder.Append(requirement.Amount);
            }

            return builder.ToString();
        }

        private static void SetButtonVisual(Button button, Text buttonText, bool interactable, Color activeColor)
        {
            if (button == null || buttonText == null)
            {
                return;
            }

            button.interactable = interactable;
            if (button.targetGraphic is Image buttonImage)
            {
                buttonImage.color = interactable
                    ? activeColor
                    : new Color(0.2f, 0.22f, 0.25f, 0.94f);
            }

            buttonText.color = interactable
                ? new Color(0.98f, 0.98f, 0.98f, 1f)
                : new Color(0.74f, 0.76f, 0.8f, 1f);
        }

        private static string GetResourceLabel(MetaResourceKind resourceKind)
        {
            switch (resourceKind)
            {
                case MetaResourceKind.RareResource:
                    return LocalizationService.Get("common.resource.rare");
                case MetaResourceKind.Seeds:
                    return LocalizationService.Get("common.resource.seeds");
                case MetaResourceKind.Ingredients:
                    return LocalizationService.Get("common.resource.ingredients");
                default:
                    return LocalizationService.Get("common.requirements.none");
            }
        }
    }
}
