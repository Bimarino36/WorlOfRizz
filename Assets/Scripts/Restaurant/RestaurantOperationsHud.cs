using UnityEngine;
using UnityEngine.UI;
using IdleRestaurant.Localization;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantOperationsHud : MonoBehaviour
    {
        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button collapseButton;
        [SerializeField] private Text collapseButtonText;
        [SerializeField] private bool collapsed;

        private const float ExpandedHeight = 148f;
        private const float CollapsedHeight = 44f;

        private bool collapseButtonBound;
        private string lastBodyValue = string.Empty;

        private void Awake()
        {
            ResolveReferences();
            EnsurePanel();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveReferences();
            EnsurePanel();
            UpdatePanel();
        }

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

            if (safeAreaRoot == null)
            {
                Transform safeAreaTransform = transform.Find("RuntimeHudCanvas/SafeAreaRoot");
                safeAreaRoot = safeAreaTransform as RectTransform;
            }
        }

        private void EnsurePanel()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (panelRoot == null)
            {
                Transform existingPanel = safeAreaRoot.Find("OperationsPanel");
                if (existingPanel != null)
                {
                    panelRoot = existingPanel as RectTransform;
                }
            }

            if (panelRoot == null)
            {
                GameObject panelObject = new GameObject("OperationsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelRoot = panelObject.GetComponent<RectTransform>();
                panelRoot.SetParent(safeAreaRoot, false);
                panelRoot.anchorMin = new Vector2(1f, 1f);
                panelRoot.anchorMax = new Vector2(1f, 1f);
                panelRoot.pivot = new Vector2(1f, 1f);
                panelRoot.sizeDelta = new Vector2(308f, 148f);
                panelRoot.anchoredPosition = new Vector2(-18f, -74f);

                Image panelImage = panelObject.GetComponent<Image>();
                ApplyPanelStyle(panelImage, new Color(0.08f, 0.1f, 0.14f, 0.86f));
            }

            EnsureTitle();
            EnsureBody();
            EnsureCollapseButton();
            WireCollapseButton();
            ApplyPanelLayout();
        }

        private void EnsureTitle()
        {
            if (titleText != null)
            {
                return;
            }

            Transform existingTitle = panelRoot.Find("TitleText");
            if (existingTitle != null)
            {
                titleText = existingTitle.GetComponent<Text>();
                if (titleText != null)
                {
                    return;
                }
            }

            GameObject titleObject = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.SetParent(panelRoot, false);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(14f, -34f);
            titleRect.offsetMax = new Vector2(-52f, -8f);

            titleText = titleObject.GetComponent<Text>();
            ApplyTextStyle(titleText, 19, TextAnchor.UpperLeft, new Color(0.95f, 0.95f, 0.95f, 1f), FontStyle.Bold);
            titleText.text = LocalizationService.Get("rest.ops.title");
        }

        private void EnsureBody()
        {
            if (bodyText != null)
            {
                return;
            }

            Transform existingBody = panelRoot.Find("BodyText");
            if (existingBody != null)
            {
                bodyText = existingBody.GetComponent<Text>();
                if (bodyText != null)
                {
                    return;
                }
            }

            GameObject bodyObject = new GameObject("BodyText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
            bodyRect.SetParent(panelRoot, false);
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.offsetMin = new Vector2(14f, 12f);
            bodyRect.offsetMax = new Vector2(-14f, -38f);

            bodyText = bodyObject.GetComponent<Text>();
            ApplyTextStyle(bodyText, 15, TextAnchor.UpperLeft, new Color(0.9f, 0.92f, 0.94f, 1f), FontStyle.Normal);
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void EnsureCollapseButton()
        {
            if (panelRoot == null)
            {
                return;
            }

            if (collapseButton == null)
            {
                Transform existingButton = panelRoot.Find("CollapseButton");
                if (existingButton != null)
                {
                    collapseButton = existingButton.GetComponent<Button>();
                    collapseButtonText = existingButton.GetComponentInChildren<Text>();
                }
            }

            if (collapseButton == null)
            {
                GameObject buttonObject = new GameObject("CollapseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.SetParent(panelRoot, false);
                buttonRect.anchorMin = new Vector2(1f, 1f);
                buttonRect.anchorMax = new Vector2(1f, 1f);
                buttonRect.pivot = new Vector2(1f, 1f);
                buttonRect.anchoredPosition = new Vector2(-10f, -8f);
                buttonRect.sizeDelta = new Vector2(28f, 24f);

                collapseButton = buttonObject.GetComponent<Button>();
                ApplyPanelStyle(buttonObject.GetComponent<Image>(), new Color(0.16f, 0.2f, 0.25f, 0.98f));
                collapseButton.targetGraphic = buttonObject.GetComponent<Image>();

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(buttonRect, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                collapseButtonText = labelObject.GetComponent<Text>();
                ApplyTextStyle(collapseButtonText, 18, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            }
        }

        private void WireCollapseButton()
        {
            if (collapseButtonBound || collapseButton == null)
            {
                return;
            }

            collapseButton.onClick.RemoveAllListeners();
            collapseButton.onClick.AddListener(ToggleCollapsed);
            collapseButtonBound = true;
        }

        private void ToggleCollapsed()
        {
            collapsed = !collapsed;
            ApplyPanelLayout();
        }

        private void ApplyPanelLayout()
        {
            if (panelRoot != null)
            {
                panelRoot.sizeDelta = new Vector2(308f, collapsed ? CollapsedHeight : ExpandedHeight);
            }

            if (bodyText != null)
            {
                bodyText.gameObject.SetActive(!collapsed);
            }

            if (collapseButtonText != null)
            {
                collapseButtonText.text = collapsed ? "+" : "-";
            }
        }

        private void UpdatePanel()
        {
            if (runtime == null || panelRoot == null || titleText == null || bodyText == null)
            {
                return;
            }

            titleText.text = LocalizationService.Get("rest.ops.title");
            string nextBodyValue =
                LocalizationService.Format("rest.ops.kitchen", BuildKitchenLine()) + "\n" +
                LocalizationService.Format("rest.ops.bar", BuildBarLine()) + "\n" +
                LocalizationService.Format("rest.ops.floor", runtime.GetCleanupTableCount(), runtime.GetWaitingBillCount()) + "\n" +
                LocalizationService.Format("rest.ops.queue", runtime.QueueGuestCount, runtime.LoyaltyScore);

            if (lastBodyValue == nextBodyValue)
            {
                return;
            }

            bodyText.text = nextBodyValue;
            lastBodyValue = nextBodyValue;
        }

        private string BuildKitchenLine()
        {
            int ready = runtime.GetKitchenReadyOrderCount();
            int prep = runtime.GetKitchenPreparingCount();
            if (ready > 0)
            {
                return LocalizationService.Format("rest.ops.ready", ready);
            }

            if (prep > 0)
            {
                return LocalizationService.Format("rest.ops.prep", prep);
            }

            return LocalizationService.Get("rest.ops.idle");
        }

        private string BuildBarLine()
        {
            int ready = runtime.GetBarReadyOrderCount();
            int prep = runtime.GetBarPreparingCount();
            if (ready > 0)
            {
                return LocalizationService.Format("rest.ops.ready", ready);
            }

            if (prep > 0)
            {
                return LocalizationService.Format("rest.ops.mix", prep);
            }

            return LocalizationService.Get("rest.ops.idle");
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
