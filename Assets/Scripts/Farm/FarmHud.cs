using IdleRestaurant.Meta;
using IdleRestaurant.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace IdleRestaurant.Farm
{
    public sealed class FarmHud : MonoBehaviour
    {
        [SerializeField] private FarmRuntime runtime;
        [SerializeField] private Canvas runtimeCanvas;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private Text statsText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Text leaveButtonText;
        [SerializeField] private Button languageButton;
        [SerializeField] private Text languageButtonText;
        [SerializeField] private Button[] plotButtons = new Button[4];
        [SerializeField] private Text[] plotButtonTexts = new Text[4];
        [SerializeField] private Button[] orderButtons = new Button[2];
        [SerializeField] private Text[] orderButtonTexts = new Text[2];

        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private bool buttonHandlersBound;
        private string lastStatsValue = string.Empty;
        private string lastStatusValue = string.Empty;

        private void Awake()
        {
            ResolveRuntime();
            EnsureHud();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveRuntime();
            EnsureHud();
            EnsureEventSystem();
            WireButtons();
            ApplySafeArea();
            UpdateHud();
        }

        private void ResolveRuntime()
        {
            if (runtime == null)
            {
                runtime = GetComponent<FarmRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<FarmRuntime>();
            }
        }

        private void EnsureHud()
        {
            EnsureCanvas();
            EnsureSafeAreaRoot();
            EnsureStatsPanel();
            EnsureStatusPanel();
            EnsureLeaveButton();
            EnsureLanguageButton();
            EnsurePlotButtons();
            EnsureOrderButtons();
        }

        private void EnsureCanvas()
        {
            if (runtimeCanvas == null)
            {
                Transform existing = transform.Find("FarmHudCanvas");
                if (existing != null)
                {
                    runtimeCanvas = existing.GetComponent<Canvas>();
                }
            }

            if (runtimeCanvas == null)
            {
                GameObject canvasObject = new GameObject("FarmHudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(transform, false);
                runtimeCanvas = canvasObject.GetComponent<Canvas>();
            }

            runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            runtimeCanvas.sortingOrder = 50;

            CanvasScaler scaler = runtimeCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureSafeAreaRoot()
        {
            if (runtimeCanvas == null)
            {
                return;
            }

            if (safeAreaRoot == null)
            {
                safeAreaRoot = runtimeCanvas.transform.Find("SafeAreaRoot") as RectTransform;
            }

            if (safeAreaRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("SafeAreaRoot", typeof(RectTransform));
            safeAreaRoot = rootObject.GetComponent<RectTransform>();
            safeAreaRoot.SetParent(runtimeCanvas.transform, false);
            safeAreaRoot.anchorMin = Vector2.zero;
            safeAreaRoot.anchorMax = Vector2.one;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
        }

        private void EnsureStatsPanel()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (statsText == null)
            {
                statsText = safeAreaRoot.Find("StatsPanel/StatsText")?.GetComponent<Text>();
            }

            if (statsText != null)
            {
                return;
            }

            GameObject panelObject = CreatePanel("StatsPanel", safeAreaRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 120f), new Vector2(18f, -18f));
            statsText = CreateText(panelObject.transform, "StatsText", new Vector2(14f, 10f), new Vector2(-14f, -10f), 18, TextAnchor.UpperLeft);
        }

        private void EnsureStatusPanel()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (statusText == null)
            {
                statusText = safeAreaRoot.Find("StatusPanel/StatusText")?.GetComponent<Text>();
            }

            if (statusText != null)
            {
                return;
            }

            GameObject panelObject = CreatePanel("StatusPanel", safeAreaRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(360f, 56f), new Vector2(0f, -18f));
            statusText = CreateText(panelObject.transform, "StatusText", new Vector2(12f, 8f), new Vector2(-12f, -8f), 18, TextAnchor.MiddleCenter);
        }

        private void EnsureLeaveButton()
        {
            EnsureButton(
                ref leaveButton,
                ref leaveButtonText,
                "LeaveButton",
                LocalizationService.Get("common.restaurant"),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(170f, 48f),
                new Vector2(-18f, -18f),
                new Color(0.18f, 0.24f, 0.31f, 0.96f));
        }

        private void EnsureLanguageButton()
        {
            EnsureButton(
                ref languageButton,
                ref languageButtonText,
                "LanguageButton",
                LocalizationService.CurrentLanguageCode,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(104f, 40f),
                new Vector2(-198f, -18f),
                new Color(0.22f, 0.28f, 0.37f, 0.96f));
        }

        private void EnsurePlotButtons()
        {
            for (int index = 0; index < plotButtons.Length; index++)
            {
                int column = index % 2;
                int row = index / 2;
                Vector2 anchoredPosition = new Vector2(420f + column * 270f, 18f + (1 - row) * 96f);
                EnsureButton(
                    ref plotButtons[index],
                    ref plotButtonTexts[index],
                    "PlotButton_" + index,
                    LocalizationService.Get("farm.plot.default"),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(240f, 82f),
                    anchoredPosition,
                    new Color(0.28f, 0.37f, 0.24f, 0.96f));
            }
        }

        private void EnsureOrderButtons()
        {
            for (int index = 0; index < orderButtons.Length; index++)
            {
                Vector2 anchoredPosition = new Vector2(-18f, -160f - index * 108f);
                EnsureButton(
                    ref orderButtons[index],
                    ref orderButtonTexts[index],
                    "OrderButton_" + index,
                    LocalizationService.Get("farm.order.default"),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(280f, 92f),
                    anchoredPosition,
                    new Color(0.29f, 0.35f, 0.56f, 0.96f));
            }
        }

        private void EnsureButton(
            ref Button button,
            ref Text label,
            string name,
            string labelValue,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 anchoredPosition,
            Color tint)
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (button == null)
            {
                Transform existing = safeAreaRoot.Find(name);
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
                buttonRect.SetParent(safeAreaRoot, false);
                buttonRect.anchorMin = anchorMin;
                buttonRect.anchorMax = anchorMax;
                buttonRect.pivot = pivot;
                buttonRect.sizeDelta = size;
                buttonRect.anchoredPosition = anchoredPosition;

                Image image = buttonObject.GetComponent<Image>();
                image.color = tint;

                button = buttonObject.GetComponent<Button>();
                button.targetGraphic = image;

                label = CreateText(buttonRect, "Label", new Vector2(8f, 6f), new Vector2(-8f, -6f), 18, TextAnchor.MiddleCenter);
            }

            if (label != null)
            {
                label.text = labelValue;
            }
        }

        private void WireButtons()
        {
            if (buttonHandlersBound || runtime == null || leaveButton == null)
            {
                return;
            }

            leaveButton.onClick.AddListener(runtime.ReturnToRestaurant);
            if (languageButton != null)
            {
                languageButton.onClick.AddListener(LocalizationService.ToggleLanguage);
            }

            for (int index = 0; index < plotButtons.Length; index++)
            {
                int capturedIndex = index;
                if (plotButtons[index] != null)
                {
                    plotButtons[index].onClick.AddListener(() => runtime.InteractWithPlot(capturedIndex));
                }
            }

            for (int index = 0; index < orderButtons.Length; index++)
            {
                int capturedIndex = index;
                if (orderButtons[index] != null)
                {
                    orderButtons[index].onClick.AddListener(() => runtime.CompleteOrder(capturedIndex));
                }
            }

            buttonHandlersBound = true;
        }

        private void UpdateHud()
        {
            if (runtime == null)
            {
                return;
            }

            MetaResourceSnapshot snapshot = runtime.ResourceSnapshot;
            string nextStats =
                LocalizationService.Get("farm.stats.title") + "\n" +
                LocalizationService.Format("farm.stats.seeds", snapshot.Seeds) + "\n" +
                LocalizationService.Format("farm.stats.ingredients", snapshot.Ingredients) + "\n" +
                LocalizationService.Format("farm.stats.rare", snapshot.RareResources) + "\n" +
                LocalizationService.Format("farm.stats.pending", snapshot.PendingRestaurantCoins);

            if (lastStatsValue != nextStats && statsText != null)
            {
                statsText.text = nextStats;
                lastStatsValue = nextStats;
            }

            if (statusText != null && lastStatusValue != runtime.StatusMessage)
            {
                statusText.text = runtime.StatusMessage;
                lastStatusValue = runtime.StatusMessage;
            }

            for (int index = 0; index < plotButtons.Length; index++)
            {
                if (plotButtonTexts[index] != null)
                {
                    plotButtonTexts[index].text = runtime.GetPlotButtonLabel(index);
                }

                if (plotButtons[index] != null)
                {
                    plotButtons[index].interactable = runtime.CanInteractWithPlot(index);
                }
            }

            for (int index = 0; index < orderButtons.Length; index++)
            {
                if (orderButtonTexts[index] != null)
                {
                    orderButtonTexts[index].text = runtime.GetOrderButtonLabel(index);
                }

                if (orderButtons[index] != null)
                {
                    orderButtons[index].interactable = runtime.CanCompleteOrder(index);
                }
            }

            if (leaveButtonText != null)
            {
                leaveButtonText.text = LocalizationService.Get("common.restaurant");
            }

            if (languageButtonText != null)
            {
                languageButtonText.text = LocalizationService.CurrentLanguageCode;
            }
        }

        private void ApplySafeArea()
        {
            if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            if (safeArea == lastSafeArea)
            {
                return;
            }

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            safeAreaRoot.anchorMin = anchorMin;
            safeAreaRoot.anchorMax = anchorMax;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
            lastSafeArea = safeArea;
        }

        private void EnsureEventSystem()
        {
            EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("FarmHudEventSystem");
                eventSystemObject.transform.SetParent(transform, false);
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                Destroy(legacyModule);
            }

#if ENABLE_INPUT_SYSTEM
            InputSystemUIInputModule inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            if (inputSystemModule.actionsAsset == null)
            {
                inputSystemModule.AssignDefaultActions();
            }
#else
            if (legacyModule == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
            panelRect.pivot = pivot;
            panelRect.sizeDelta = size;
            panelRect.anchoredPosition = anchoredPosition;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.13f, 0.82f);
            return panelObject;
        }

        private Text CreateText(
            Transform parent,
            string name,
            Vector2 offsetMin,
            Vector2 offsetMax,
            int fontSize,
            TextAnchor anchor)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(parent, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = offsetMin;
            textRect.offsetMax = offsetMax;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            return text;
        }
    }
}
