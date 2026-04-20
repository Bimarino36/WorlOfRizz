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
        [SerializeField] private RectTransform operationsPanelRoot;
        [SerializeField] private RectTransform portalActionPanel;
        [SerializeField] private RectTransform resourceStripRoot;
        [SerializeField] private Text resourceStripText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Text claimButtonText;
        [SerializeField] private RectTransform specialOrdersRoot;
        [SerializeField] private Text specialOrdersTitleText;
        [SerializeField] private Text specialOrdersBodyText;
        [SerializeField] private Button specialOrdersCollapseButton;
        [SerializeField] private Text specialOrdersCollapseButtonText;
        [SerializeField] private RectTransform orderOneRowRoot;
        [SerializeField] private Text orderOneInfoText;
        [SerializeField] private Button orderOneButton;
        [SerializeField] private Text orderOneButtonText;
        [SerializeField] private RectTransform orderTwoRowRoot;
        [SerializeField] private Text orderTwoInfoText;
        [SerializeField] private Button orderTwoButton;
        [SerializeField] private Text orderTwoButtonText;
        [SerializeField] private Button adventureButton;
        [SerializeField] private Text adventureButtonText;
        [SerializeField] private Button farmButton;
        [SerializeField] private Text farmButtonText;

        private const float SpecialOrdersExpandedHeight = 308f;
        private const float SpecialOrdersCollapsedHeight = 46f;
        private const float SpecialOrdersWidth = 336f;
        private const float ContractsHorizontalPadding = 14f;
        private const float ContractsSectionGap = 10f;
        private const float ContractRowMinHeight = 72f;
        private const float ContractRowVerticalPadding = 10f;
        private const float ContractRowButtonWidth = 84f;
        private const float PortalButtonSize = 64f;
        private const float StackTopOffset = -74f;
        private const float StackGap = 12f;

        private bool buttonHandlersBound;
        private bool collapseButtonBound;
        private bool specialOrdersCollapsed;
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

            if (operationsPanelRoot == null && safeAreaRoot != null)
            {
                operationsPanelRoot = safeAreaRoot.Find("OperationsPanel") as RectTransform;
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
            EnsurePortalActionPanel();
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
                ApplyPanelStyle(stripImage, new Color(0.08f, 0.1f, 0.14f, 0.82f));
            }

            resourceStripRoot.anchorMin = new Vector2(0.5f, 1f);
            resourceStripRoot.anchorMax = new Vector2(0.5f, 1f);
            resourceStripRoot.pivot = new Vector2(0.5f, 1f);
            resourceStripRoot.sizeDelta = new Vector2(500f, 52f);
            resourceStripRoot.anchoredPosition = new Vector2(0f, -18f);
            AlignResourceStripBelowNotifications();

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
                ApplyTextStyle(resourceStripText, 15, TextAnchor.MiddleLeft, new Color(0.94f, 0.95f, 0.96f, 1f), FontStyle.Bold);
            }

            RectTransform resourceTextRect = resourceStripText.rectTransform;
            resourceTextRect.anchorMin = Vector2.zero;
            resourceTextRect.anchorMax = Vector2.one;
            resourceTextRect.offsetMin = new Vector2(14f, 8f);
            resourceTextRect.offsetMax = new Vector2(-130f, -10f);

            EnsureStripButton(
                ref claimButton,
                ref claimButtonText,
                "ClaimButton",
                LocalizationService.Get("common.claim_none"),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-10f, 0f),
                new Vector2(112f, 34f),
                new Color(0.21f, 0.46f, 0.61f, 0.96f));
        }

        private void AlignResourceStripBelowNotifications()
        {
            if (resourceStripRoot == null || safeAreaRoot == null)
            {
                return;
            }

            Transform notificationPanel = safeAreaRoot.Find("NotificationPanel");
            if (notificationPanel == null || notificationPanel == resourceStripRoot)
            {
                return;
            }

            int notificationIndex = notificationPanel.GetSiblingIndex();
            int currentIndex = resourceStripRoot.GetSiblingIndex();
            if (currentIndex > notificationIndex)
            {
                int targetIndex = Mathf.Clamp(notificationIndex, 0, Mathf.Max(0, safeAreaRoot.childCount - 1));
                resourceStripRoot.SetSiblingIndex(targetIndex);
            }
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
                ApplyPanelStyle(panelImage, new Color(0.08f, 0.1f, 0.14f, 0.88f));
            }

            specialOrdersRoot.anchorMin = new Vector2(1f, 1f);
            specialOrdersRoot.anchorMax = new Vector2(1f, 1f);
            specialOrdersRoot.pivot = new Vector2(1f, 1f);
            specialOrdersRoot.sizeDelta = new Vector2(SpecialOrdersWidth, specialOrdersCollapsed ? SpecialOrdersCollapsedHeight : SpecialOrdersExpandedHeight);
            specialOrdersRoot.anchoredPosition = ResolveStackedPosition();

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
                ApplyTextStyle(specialOrdersTitleText, 18, TextAnchor.UpperLeft, new Color(0.96f, 0.96f, 0.96f, 1f), FontStyle.Bold);
                specialOrdersTitleText.resizeTextForBestFit = true;
                specialOrdersTitleText.resizeTextMinSize = 13;
                specialOrdersTitleText.resizeTextMaxSize = 18;
                specialOrdersTitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                specialOrdersTitleText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            RectTransform specialOrdersTitleRect = specialOrdersTitleText.rectTransform;
            specialOrdersTitleRect.anchorMin = new Vector2(0f, 1f);
            specialOrdersTitleRect.anchorMax = new Vector2(1f, 1f);
            specialOrdersTitleRect.pivot = new Vector2(0.5f, 1f);
            specialOrdersTitleRect.offsetMin = new Vector2(14f, -32f);
            specialOrdersTitleRect.offsetMax = new Vector2(-52f, -8f);

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
                ApplyTextStyle(specialOrdersBodyText, 13, TextAnchor.UpperLeft, new Color(0.9f, 0.92f, 0.94f, 1f), FontStyle.Normal);
                specialOrdersBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                specialOrdersBodyText.verticalOverflow = VerticalWrapMode.Overflow;
            }

            RectTransform specialOrdersBodyRect = specialOrdersBodyText.rectTransform;
            specialOrdersBodyRect.anchorMin = new Vector2(0f, 1f);
            specialOrdersBodyRect.anchorMax = new Vector2(1f, 1f);
            specialOrdersBodyRect.pivot = new Vector2(0.5f, 1f);
            specialOrdersBodyRect.offsetMin = new Vector2(14f, -82f);
            specialOrdersBodyRect.offsetMax = new Vector2(-14f, -40f);

            EnsureCollapseButton();
            EnsureOrderRows();
            ApplySpecialOrdersLayout();
        }

        private void EnsureButtons()
        {
            EnsureOrderButton(
                ref orderOneButton,
                ref orderOneButtonText,
                orderOneRowRoot,
                "OrderOneButton",
                LocalizationService.Get("farm.order.default"),
                new Color(0.47f, 0.32f, 0.16f, 0.96f));

            EnsureOrderButton(
                ref orderTwoButton,
                ref orderTwoButtonText,
                orderTwoRowRoot,
                "OrderTwoButton",
                LocalizationService.Get("farm.order.default"),
                new Color(0.48f, 0.24f, 0.18f, 0.96f));

            EnsureQuickPortalButton(
                ref adventureButton,
                ref adventureButtonText,
                "AdventureQuickButton",
                new Vector2(0f, 0f),
                new Color(0.2f, 0.42f, 0.62f, 0.95f),
                "UI/Icons/AdventureCompass");

            EnsureQuickPortalButton(
                ref farmButton,
                ref farmButtonText,
                "FarmQuickButton",
                new Vector2(76f, 0f),
                new Color(0.23f, 0.52f, 0.34f, 0.95f),
                "UI/Icons/FarmSprout");
        }

        private void EnsureCollapseButton()
        {
            if (specialOrdersRoot == null)
            {
                return;
            }

            if (specialOrdersCollapseButton == null)
            {
                Transform existingButton = specialOrdersRoot.Find("CollapseButton");
                if (existingButton != null)
                {
                    specialOrdersCollapseButton = existingButton.GetComponent<Button>();
                    specialOrdersCollapseButtonText = existingButton.GetComponentInChildren<Text>();
                }
            }

            if (specialOrdersCollapseButton == null)
            {
                GameObject buttonObject = new GameObject("CollapseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.SetParent(specialOrdersRoot, false);
                buttonRect.anchorMin = new Vector2(1f, 1f);
                buttonRect.anchorMax = new Vector2(1f, 1f);
                buttonRect.pivot = new Vector2(1f, 1f);
                buttonRect.anchoredPosition = new Vector2(-10f, -8f);
                buttonRect.sizeDelta = new Vector2(28f, 24f);

                specialOrdersCollapseButton = buttonObject.GetComponent<Button>();
                ApplyButtonStyle(specialOrdersCollapseButton, new Color(0.16f, 0.2f, 0.25f, 0.98f));

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                specialOrdersCollapseButtonText = labelObject.GetComponent<Text>();
                ApplyTextStyle(specialOrdersCollapseButtonText, 18, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            }
        }

        private void EnsureOrderRows()
        {
            EnsureOrderRow(ref orderOneRowRoot, ref orderOneInfoText, "OrderOneRow", new Vector2(14f, -164f), new Vector2(-14f, -92f));
            EnsureOrderRow(ref orderTwoRowRoot, ref orderTwoInfoText, "OrderTwoRow", new Vector2(14f, -246f), new Vector2(-14f, -174f));
        }

        private void EnsureOrderRow(ref RectTransform rowRoot, ref Text infoText, string name, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (specialOrdersRoot == null)
            {
                return;
            }

            if (rowRoot == null)
            {
                Transform existingRow = specialOrdersRoot.Find(name);
                if (existingRow != null)
                {
                    rowRoot = existingRow as RectTransform;
                }
            }

            if (rowRoot == null)
            {
                GameObject rowObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                rowRoot = rowObject.GetComponent<RectTransform>();
                rowRoot.SetParent(specialOrdersRoot, false);
                ApplyPanelStyle(rowObject.GetComponent<Image>(), new Color(0.13f, 0.16f, 0.2f, 0.94f));
            }

            rowRoot.anchorMin = new Vector2(0f, 1f);
            rowRoot.anchorMax = new Vector2(1f, 1f);
            rowRoot.pivot = new Vector2(0.5f, 1f);
            rowRoot.offsetMin = offsetMin;
            rowRoot.offsetMax = offsetMax;

            if (infoText == null)
            {
                Transform existingText = rowRoot.Find("InfoText");
                if (existingText != null)
                {
                    infoText = existingText.GetComponent<Text>();
                }
            }

            if (infoText == null)
            {
                GameObject textObject = new GameObject("InfoText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                textRect.SetParent(rowRoot, false);
                infoText = textObject.GetComponent<Text>();
                ApplyTextStyle(infoText, 12, TextAnchor.UpperLeft, new Color(0.92f, 0.93f, 0.95f, 1f), FontStyle.Normal);
                infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
                infoText.verticalOverflow = VerticalWrapMode.Overflow;
            }

            RectTransform infoRect = infoText.rectTransform;
            infoRect.anchorMin = Vector2.zero;
            infoRect.anchorMax = Vector2.one;
            infoRect.offsetMin = new Vector2(12f, 10f);
            infoRect.offsetMax = new Vector2(-106f, -10f);
        }

        private void ApplySpecialOrdersLayout()
        {
            if (specialOrdersRoot != null)
            {
                specialOrdersRoot.sizeDelta = new Vector2(SpecialOrdersWidth, specialOrdersCollapsed ? SpecialOrdersCollapsedHeight : CalculateExpandedPanelHeight());
                specialOrdersRoot.anchoredPosition = ResolveStackedPosition();
            }

            bool contentVisible = !specialOrdersCollapsed;

            if (specialOrdersBodyText != null)
            {
                specialOrdersBodyText.gameObject.SetActive(contentVisible);
            }

            if (orderOneRowRoot != null)
            {
                orderOneRowRoot.gameObject.SetActive(contentVisible);
            }

            if (orderTwoRowRoot != null)
            {
                orderTwoRowRoot.gameObject.SetActive(contentVisible);
            }

            if (specialOrdersCollapseButtonText != null)
            {
                specialOrdersCollapseButtonText.text = specialOrdersCollapsed ? "+" : "-";
            }

            if (specialOrdersTitleText != null)
            {
                RectTransform titleRect = specialOrdersTitleText.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.pivot = new Vector2(0f, 1f);
                titleRect.offsetMin = new Vector2(18f, -34f);
                titleRect.offsetMax = new Vector2(-52f, -8f);
            }

            if (specialOrdersBodyText != null)
            {
                RectTransform bodyRect = specialOrdersBodyText.rectTransform;
                bodyRect.anchorMin = new Vector2(0f, 1f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.pivot = new Vector2(0f, 1f);
                bodyRect.offsetMin = new Vector2(18f, -(44f + GetContractsIntroHeight()));
                bodyRect.offsetMax = new Vector2(-18f, -44f);
            }

            LayoutOrderRow(orderOneRowRoot, orderOneInfoText, 0);
            LayoutOrderRow(orderTwoRowRoot, orderTwoInfoText, 1);
        }

        private Vector2 ResolveStackedPosition()
        {
            return new Vector2(-18f, StackTopOffset);
        }

        private void EnsureOrderButton(
            ref Button button,
            ref Text label,
            RectTransform rowRoot,
            string name,
            string buttonText,
            Color backgroundColor)
        {
            if (rowRoot == null)
            {
                return;
            }

            if (button == null)
            {
                Transform existing = rowRoot.Find(name);
                if (existing == null && specialOrdersRoot != null)
                {
                    existing = specialOrdersRoot.Find(name);
                }

                if (existing != null)
                {
                    button = existing.GetComponent<Button>();
                    label = existing.GetComponentInChildren<Text>();
                }
            }

            if (button == null)
            {
                GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                button = buttonObject.GetComponent<Button>();
                ApplyButtonStyle(button, backgroundColor);

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonObject.GetComponent<RectTransform>(), false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(6f, 4f);
                labelRect.offsetMax = new Vector2(-6f, -4f);

                label = labelObject.GetComponent<Text>();
                ApplyTextStyle(label, 12, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            }

            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            if (buttonTransform.parent != rowRoot)
            {
                buttonTransform.SetParent(rowRoot, false);
            }

            buttonTransform.anchorMin = new Vector2(1f, 0.5f);
            buttonTransform.anchorMax = new Vector2(1f, 0.5f);
            buttonTransform.pivot = new Vector2(1f, 0.5f);
            buttonTransform.anchoredPosition = new Vector2(-10f, 0f);
            buttonTransform.sizeDelta = new Vector2(ContractRowButtonWidth, 36f);

            if (label != null)
            {
                label.text = buttonText;
            }
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
                button = buttonObject.GetComponent<Button>();
                ApplyButtonStyle(button, backgroundColor);

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 5f);
                labelRect.offsetMax = new Vector2(-8f, -5f);

                label = labelObject.GetComponent<Text>();
                ApplyTextStyle(label, 13, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
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

        private void EnsurePortalActionPanel()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (portalActionPanel == null)
            {
                Transform existing = safeAreaRoot.Find("PortalActionPanel");
                if (existing != null)
                {
                    portalActionPanel = existing as RectTransform;
                }
            }

            if (portalActionPanel == null)
            {
                GameObject panelObject = new GameObject("PortalActionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                portalActionPanel = panelObject.GetComponent<RectTransform>();
                portalActionPanel.SetParent(safeAreaRoot, false);
                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0f);
                panelImage.raycastTarget = false;
            }

            portalActionPanel.anchorMin = new Vector2(1f, 0f);
            portalActionPanel.anchorMax = new Vector2(1f, 0f);
            portalActionPanel.pivot = new Vector2(1f, 0f);
            portalActionPanel.anchoredPosition = new Vector2(-20f, 20f);
            portalActionPanel.sizeDelta = new Vector2(140f, PortalButtonSize);
        }

        private void EnsureQuickPortalButton(
            ref Button button,
            ref Text label,
            string name,
            Vector2 anchoredPosition,
            Color backgroundColor,
            string iconResourcePath)
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            EnsurePortalActionPanel();
            if (portalActionPanel == null)
            {
                return;
            }

            if (button == null)
            {
                Transform existing = portalActionPanel.Find(name);
                if (existing == null)
                {
                    existing = safeAreaRoot.Find(name);
                }
                if (existing == null && specialOrdersRoot != null)
                {
                    existing = specialOrdersRoot.Find(name);
                }

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
                buttonRect.SetParent(portalActionPanel, false);

                button = buttonObject.GetComponent<Button>();
                ApplyButtonStyle(button, backgroundColor);

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 4f);
                labelRect.offsetMax = new Vector2(-8f, -4f);

                label = labelObject.GetComponent<Text>();
                ApplyTextStyle(label, 13, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            }

            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            if (buttonTransform.parent != portalActionPanel)
            {
                buttonTransform.SetParent(portalActionPanel, false);
            }

            button.gameObject.name = name;
            buttonTransform.anchorMin = new Vector2(0f, 0f);
            buttonTransform.anchorMax = new Vector2(0f, 0f);
            buttonTransform.pivot = new Vector2(0f, 0f);
            buttonTransform.anchoredPosition = anchoredPosition;
            buttonTransform.sizeDelta = new Vector2(PortalButtonSize, PortalButtonSize);

            if (label != null)
            {
                label.text = string.Empty;
            }

            SetButtonIcon(button, iconResourcePath, new Vector2(28f, 28f));
        }

        private void WireButtons()
        {
            if (!collapseButtonBound && specialOrdersCollapseButton != null)
            {
                specialOrdersCollapseButton.onClick.RemoveAllListeners();
                specialOrdersCollapseButton.onClick.AddListener(ToggleSpecialOrdersCollapsed);
                collapseButtonBound = true;
            }

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

            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(previewBridge.TryClaimPendingRestaurantCoins);
            orderOneButton.onClick.RemoveAllListeners();
            orderOneButton.onClick.AddListener(HandleOrderOnePressed);
            orderTwoButton.onClick.RemoveAllListeners();
            orderTwoButton.onClick.AddListener(HandleOrderTwoPressed);
            adventureButton.onClick.RemoveAllListeners();
            adventureButton.onClick.AddListener(previewBridge.TryOpenAdventurePortal);
            farmButton.onClick.RemoveAllListeners();
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

        private void ToggleSpecialOrdersCollapsed()
        {
            specialOrdersCollapsed = !specialOrdersCollapsed;
            ApplySpecialOrdersLayout();
            UpdatePortalButtons();
        }

        private void UpdateHud()
        {
            if (previewBridge == null || resourceStripText == null || specialOrdersTitleText == null)
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
            string nextOrders = LocalizationService.Get("rest.contracts.intro");
            if (lastOrdersValue != nextOrders)
            {
                specialOrdersTitleText.text = LocalizationService.Get("rest.contracts.title");
                if (specialOrdersBodyText != null)
                {
                    specialOrdersBodyText.text = nextOrders;
                }
                lastOrdersValue = nextOrders;
            }

            UpdateClaimButton();
            UpdateOrderButtons(statuses);
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

        private void UpdateOrderButtons(IReadOnlyList<SpecialOrderStatus> statuses)
        {
            SpecialOrderStatus orderOneStatus = statuses != null && statuses.Count > 0 ? statuses[0] : default;
            SpecialOrderStatus orderTwoStatus = statuses != null && statuses.Count > 1 ? statuses[1] : default;

            UpdateOrderInfo(orderOneInfoText, orderOneStatus);
            UpdateOrderInfo(orderTwoInfoText, orderTwoStatus);
            ApplySpecialOrdersLayout();

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
            bool canShowAdventure = SceneTransitionService.CanLoadPortal(MetaPortalId.Adventure);
            bool canShowFarm = SceneTransitionService.CanLoadPortal(MetaPortalId.Farm);

            if (adventureButton != null)
            {
                adventureButton.gameObject.SetActive(canShowAdventure);
            }

            if (farmButton != null)
            {
                farmButton.gameObject.SetActive(canShowFarm);
            }

            if (adventureButtonText != null)
            {
                adventureButtonText.text = string.Empty;
            }

            if (farmButtonText != null)
            {
                farmButtonText.text = string.Empty;
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

        private static void UpdateOrderInfo(Text infoText, SpecialOrderStatus status)
        {
            if (infoText == null)
            {
                return;
            }

            string statusText;
            if (status.IsComplete)
            {
                statusText = LocalizationService.Get("common.status.complete");
            }
            else if (status.IsUnlocked)
            {
                statusText = LocalizationService.Get("common.status.ready_to_serve");
            }
            else
            {
                statusText = string.IsNullOrWhiteSpace(status.LockReason)
                    ? LocalizationService.Get("common.status.locked")
                    : status.LockReason;
            }

            infoText.text =
                "<b>" + status.Title + "</b>\n" +
                LocalizationService.Format("rest.contracts.need", BuildRequirementsText(status.Requirements)) + "\n" +
                LocalizationService.Format("rest.contracts.reward", status.RewardLabel) + "\n" +
                LocalizationService.Format("rest.contracts.status", statusText);
        }

        private float GetContractsIntroHeight()
        {
            if (specialOrdersBodyText == null)
            {
                return 40f;
            }

            return Mathf.Max(32f, GetPreferredTextHeight(specialOrdersBodyText, SpecialOrdersWidth - ContractsHorizontalPadding * 2f));
        }

        private float GetOrderRowHeight(Text infoText)
        {
            if (infoText == null)
            {
                return ContractRowMinHeight;
            }

            float textWidth = SpecialOrdersWidth - ContractsHorizontalPadding * 2f - ContractRowButtonWidth - 12f;
            float textHeight = GetPreferredTextHeight(infoText, textWidth);
            return Mathf.Max(ContractRowMinHeight, textHeight + ContractRowVerticalPadding * 2f);
        }

        private float CalculateExpandedPanelHeight()
        {
            float introHeight = GetContractsIntroHeight();
            float orderOneHeight = GetOrderRowHeight(orderOneInfoText);
            float orderTwoHeight = GetOrderRowHeight(orderTwoInfoText);
            return 12f + 24f + ContractsSectionGap + introHeight + ContractsSectionGap + orderOneHeight + ContractsSectionGap + orderTwoHeight + 12f;
        }

        private void LayoutOrderRow(RectTransform rowRoot, Text infoText, int rowIndex)
        {
            if (rowRoot == null)
            {
                return;
            }

            float introHeight = GetContractsIntroHeight();
            float firstRowTop = 44f + introHeight + ContractsSectionGap;
            float rowOneHeight = GetOrderRowHeight(orderOneInfoText);
            float rowHeight = rowIndex == 0 ? rowOneHeight : GetOrderRowHeight(orderTwoInfoText);
            float topOffset = rowIndex == 0
                ? firstRowTop
                : firstRowTop + rowOneHeight + ContractsSectionGap;

            rowRoot.anchorMin = new Vector2(0f, 1f);
            rowRoot.anchorMax = new Vector2(1f, 1f);
            rowRoot.pivot = new Vector2(0.5f, 1f);
            rowRoot.anchoredPosition = new Vector2(0f, -topOffset);
            rowRoot.sizeDelta = new Vector2(0f, rowHeight);

            if (infoText != null)
            {
                RectTransform infoRect = infoText.rectTransform;
                infoRect.anchorMin = Vector2.zero;
                infoRect.anchorMax = Vector2.one;
                infoRect.offsetMin = new Vector2(12f, ContractRowVerticalPadding);
                infoRect.offsetMax = new Vector2(-(ContractRowButtonWidth + 22f), -ContractRowVerticalPadding);
            }
        }

        private static float GetPreferredTextHeight(Text text, float width)
        {
            if (text == null || text.font == null)
            {
                return 0f;
            }

            var settings = text.GetGenerationSettings(new Vector2(width, 0f));
            return text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text ?? string.Empty, settings);
        }

        private static void SetButtonIcon(Button button, string resourcePath, Vector2 size)
        {
            if (button == null)
            {
                return;
            }

            Transform existingIcon = button.transform.Find("Icon");
            RectTransform iconRect;
            Image iconImage;

            if (existingIcon == null)
            {
                GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.SetParent(button.transform, false);
                iconImage = iconObject.GetComponent<Image>();
            }
            else
            {
                iconRect = existingIcon as RectTransform;
                iconImage = existingIcon.GetComponent<Image>();
            }

            if (iconRect == null || iconImage == null)
            {
                return;
            }

            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = size;

            iconImage.sprite = Resources.Load<Sprite>(resourcePath);
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.color = new Color(0.97f, 0.98f, 1f, 1f);
            iconImage.raycastTarget = false;
            iconRect.SetAsLastSibling();
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

        private static void ApplyPanelStyle(Image image, Color color)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = Resources.Load<Sprite>("UI/RoundedRect");
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
        }

        private static void ApplyButtonStyle(Button button, Color backgroundColor)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                image = button.gameObject.AddComponent<Image>();
            }

            image.sprite = Resources.Load<Sprite>("UI/RoundedRect");
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = backgroundColor;
            button.targetGraphic = image;
        }

        private static void ApplyTextStyle(Text text, int fontSize, TextAnchor alignment, Color color, FontStyle fontStyle)
        {
            if (text == null)
            {
                return;
            }

            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.supportRichText = true;
            text.color = color;
        }
    }
}
