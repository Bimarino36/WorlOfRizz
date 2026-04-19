using IdleRestaurant.Meta;
using IdleRestaurant.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace IdleRestaurant.Adventure
{
    public sealed class AdventureHud : MonoBehaviour
    {
        [SerializeField] private AdventureRuntime runtime;
        [SerializeField] private Canvas runtimeCanvas;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private Text statsText;
        [SerializeField] private Button skillButton;
        [SerializeField] private Text skillButtonText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Text leaveButtonText;
        [SerializeField] private Button languageButton;
        [SerializeField] private Text languageButtonText;
        [SerializeField] private GameObject resultsOverlayRoot;
        [SerializeField] private Text resultsTitleText;
        [SerializeField] private Text resultsBodyText;
        [SerializeField] private Button returnButton;
        [SerializeField] private Text returnButtonText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Text retryButtonText;

        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private bool buttonHandlersBound;
        private string lastStatsValue = string.Empty;
        private string lastResultsValue = string.Empty;

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
            UpdateStats();
            UpdateButtons();
            UpdateResultsOverlay();
        }

        private void ResolveRuntime()
        {
            if (runtime == null)
            {
                runtime = GetComponent<AdventureRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<AdventureRuntime>();
            }
        }

        private void EnsureHud()
        {
            EnsureCanvas();
            EnsureSafeAreaRoot();
            EnsureStatsPanel();
            EnsureTopButtons();
            EnsureSkillButton();
            EnsureLanguageButton();
            EnsureResultsOverlay();
        }

        private void EnsureCanvas()
        {
            if (runtimeCanvas == null)
            {
                Transform existing = transform.Find("AdventureHudCanvas");
                if (existing != null)
                {
                    runtimeCanvas = existing.GetComponent<Canvas>();
                }
            }

            if (runtimeCanvas == null)
            {
                GameObject canvasObject = new GameObject("AdventureHudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(transform, false);
                runtimeCanvas = canvasObject.GetComponent<Canvas>();
            }

            runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            runtimeCanvas.pixelPerfect = false;
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
                Transform existing = runtimeCanvas.transform.Find("SafeAreaRoot");
                safeAreaRoot = existing as RectTransform;
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
                Transform existing = safeAreaRoot.Find("StatsPanel/StatsText");
                if (existing != null)
                {
                    statsText = existing.GetComponent<Text>();
                }
            }

            if (statsText != null)
            {
                return;
            }

            GameObject panelObject = new GameObject("StatsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(safeAreaRoot, false);
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.sizeDelta = new Vector2(310f, 126f);
            panelRect.anchoredPosition = new Vector2(18f, -18f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.13f, 0.82f);

            GameObject textObject = new GameObject("StatsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(panelRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(14f, 10f);
            textRect.offsetMax = new Vector2(-14f, -10f);

            statsText = textObject.GetComponent<Text>();
            statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsText.fontSize = 18;
            statsText.alignment = TextAnchor.UpperLeft;
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statsText.verticalOverflow = VerticalWrapMode.Overflow;
            statsText.color = Color.white;
        }

        private void EnsureTopButtons()
        {
            EnsureButton(
                ref leaveButton,
                ref leaveButtonText,
                "LeaveButton",
                new Vector2(-18f, -18f),
                new Vector2(170f, 48f),
                new Color(0.18f, 0.24f, 0.31f, 0.96f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f));
        }

        private void EnsureLanguageButton()
        {
            EnsureButton(
                ref languageButton,
                ref languageButtonText,
                "LanguageButton",
                new Vector2(-198f, -18f),
                new Vector2(104f, 48f),
                new Color(0.22f, 0.28f, 0.37f, 0.96f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f));
        }

        private void EnsureSkillButton()
        {
            EnsureButton(
                ref skillButton,
                ref skillButtonText,
                "SkillButton",
                new Vector2(-18f, 18f),
                new Vector2(260f, 70f),
                new Color(0.24f, 0.43f, 0.71f, 0.96f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f));
        }

        private void EnsureResultsOverlay()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (resultsOverlayRoot == null)
            {
                Transform existing = safeAreaRoot.Find("ResultsOverlay");
                if (existing != null)
                {
                    resultsOverlayRoot = existing.gameObject;
                }
            }

            if (resultsOverlayRoot == null)
            {
                GameObject overlayObject = new GameObject("ResultsOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
                overlayRect.SetParent(safeAreaRoot, false);
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                Image overlayImage = overlayObject.GetComponent<Image>();
                overlayImage.color = new Color(0.04f, 0.04f, 0.06f, 0.72f);
                resultsOverlayRoot = overlayObject;

                GameObject cardObject = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform cardRect = cardObject.GetComponent<RectTransform>();
                cardRect.SetParent(overlayRect, false);
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
                cardRect.sizeDelta = new Vector2(560f, 340f);

                Image cardImage = cardObject.GetComponent<Image>();
                cardImage.color = new Color(0.11f, 0.13f, 0.18f, 0.96f);

                resultsTitleText = CreateText(cardRect, "TitleText", new Vector2(24f, -56f), new Vector2(-24f, -20f), 34, TextAnchor.UpperCenter);
                resultsBodyText = CreateText(cardRect, "BodyText", new Vector2(32f, 116f), new Vector2(-32f, -92f), 20, TextAnchor.UpperCenter);

                returnButton = CreateButton(cardRect, "ReturnButton", LocalizationService.Get("adv.results.return"), new Vector2(-118f, -112f), new Vector2(220f, 54f), new Color(0.21f, 0.46f, 0.34f, 0.96f), out returnButtonText);
                retryButton = CreateButton(cardRect, "RetryButton", LocalizationService.Get("adv.results.retry"), new Vector2(118f, -112f), new Vector2(180f, 54f), new Color(0.31f, 0.4f, 0.67f, 0.96f), out retryButtonText);
            }

            if (resultsTitleText == null)
            {
                resultsTitleText = resultsOverlayRoot.transform.Find("Card/TitleText")?.GetComponent<Text>();
            }

            if (resultsBodyText == null)
            {
                resultsBodyText = resultsOverlayRoot.transform.Find("Card/BodyText")?.GetComponent<Text>();
            }

            if (returnButton == null)
            {
                returnButton = resultsOverlayRoot.transform.Find("Card/ReturnButton")?.GetComponent<Button>();
            }

            if (returnButtonText == null)
            {
                returnButtonText = resultsOverlayRoot.transform.Find("Card/ReturnButton/Label")?.GetComponent<Text>();
            }

            if (retryButton == null)
            {
                retryButton = resultsOverlayRoot.transform.Find("Card/RetryButton")?.GetComponent<Button>();
            }

            if (retryButtonText == null)
            {
                retryButtonText = resultsOverlayRoot.transform.Find("Card/RetryButton/Label")?.GetComponent<Text>();
            }

            if (resultsOverlayRoot != null && resultsOverlayRoot.activeSelf)
            {
                resultsOverlayRoot.SetActive(false);
            }
        }

        private void EnsureButton(
            ref Button button,
            ref Text label,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            Color tint,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot)
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

            if (button != null)
            {
                return;
            }

            button = CreateButton(safeAreaRoot, name, name, anchoredPosition, size, tint, out label);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.pivot = pivot;
            buttonRect.anchoredPosition = anchoredPosition;
        }

        private Button CreateButton(
            Transform parent,
            string name,
            string labelValue,
            Vector2 anchoredPosition,
            Vector2 size,
            Color tint,
            out Text label)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = anchoredPosition;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = tint;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;

            label = CreateText(buttonRect, "Label", new Vector2(8f, 6f), new Vector2(-8f, -6f), 20, TextAnchor.MiddleCenter);
            label.text = labelValue;
            return button;
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

        private void UpdateStats()
        {
            if (runtime == null || statsText == null)
            {
                return;
            }

            string nextValue =
                LocalizationService.Get("adv.stats.title") + "\n" +
                LocalizationService.Format("adv.stats.wave", runtime.CurrentWaveNumber, runtime.TotalWaveCount) + "\n" +
                LocalizationService.Format("adv.stats.enemies", runtime.AliveEnemyCount) + "\n" +
                LocalizationService.Format("adv.stats.hp", Mathf.CeilToInt(runtime.PlayerCurrentHealth), Mathf.CeilToInt(runtime.PlayerMaxHealth)) + "\n" +
                LocalizationService.Format("adv.stats.status", runtime.StatusLabel);

            if (lastStatsValue == nextValue)
            {
                return;
            }

            statsText.text = nextValue;
            lastStatsValue = nextValue;
        }

        private void UpdateButtons()
        {
            if (runtime == null)
            {
                return;
            }

            if (skillButton != null)
            {
                skillButton.gameObject.SetActive(!runtime.HasFinishedRun);
                skillButton.interactable = runtime.CanUseSkill;
            }

            if (skillButtonText != null)
            {
                skillButtonText.text = runtime.CanUseSkill
                    ? LocalizationService.Get("adv.button.use_burst")
                    : LocalizationService.Format("adv.button.burst_cd", runtime.SkillCooldownRemaining.ToString("0.0"));
            }

            if (leaveButtonText != null)
            {
                leaveButtonText.text = runtime.HasFinishedRun
                    ? LocalizationService.Get("common.restaurant")
                    : LocalizationService.Get("adv.button.leave_run");
            }

            if (languageButtonText != null)
            {
                languageButtonText.text = LocalizationService.CurrentLanguageCode;
            }
        }

        private void UpdateResultsOverlay()
        {
            if (runtime == null || resultsOverlayRoot == null || resultsTitleText == null || resultsBodyText == null)
            {
                return;
            }

            bool shouldShow = runtime.HasFinishedRun;
            if (resultsOverlayRoot.activeSelf != shouldShow)
            {
                resultsOverlayRoot.SetActive(shouldShow);
            }

            if (!shouldShow)
            {
                lastResultsValue = string.Empty;
                return;
            }

            AdventureRewardResult reward = runtime.LastRewardResult;
            string nextBody =
                LocalizationService.Format("adv.results.waves", reward.WavesCleared) + "\n" +
                LocalizationService.Format("adv.results.rare", reward.RareResources) + "\n" +
                LocalizationService.Format("adv.results.seeds", reward.Seeds) + "\n" +
                LocalizationService.Format("adv.results.coins", reward.PendingRestaurantCoins);

            if (lastResultsValue == nextBody)
            {
                return;
            }

            resultsTitleText.text = runtime.IsVictory
                ? LocalizationService.Get("adv.results.victory")
                : LocalizationService.Get("adv.results.defeat");
            resultsBodyText.text = nextBody;
            returnButtonText.text = LocalizationService.Get("adv.results.return");
            retryButtonText.text = LocalizationService.Get("adv.results.retry");
            lastResultsValue = nextBody;
        }

        private void WireButtons()
        {
            if (buttonHandlersBound || skillButton == null || leaveButton == null || returnButton == null || retryButton == null || runtime == null)
            {
                return;
            }

            skillButton.onClick.AddListener(() => runtime.TryUseSkill());
            leaveButton.onClick.AddListener(runtime.ReturnToRestaurant);
            if (languageButton != null)
            {
                languageButton.onClick.AddListener(LocalizationService.ToggleLanguage);
            }
            returnButton.onClick.AddListener(runtime.ReturnToRestaurant);
            retryButton.onClick.AddListener(runtime.RestartRun);
            buttonHandlersBound = true;
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
                GameObject eventSystemObject = new GameObject("AdventureHudEventSystem");
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
    }
}
