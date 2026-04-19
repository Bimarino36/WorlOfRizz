using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IdleRestaurant.Localization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantHud : MonoBehaviour
    {
        private const float NotificationDuration = 2.25f;
        private const float NotificationFadeDuration = 0.3f;
        private const float UpgradesPanelHiddenOffsetY = -56f;
        private const float UpgradesBackdropMaxAlpha = 0.46f;

        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField] private Canvas runtimeCanvas;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private RectTransform statsPanel;
        [SerializeField] private Text statsText;
        [SerializeField] private Button languageButton;
        [SerializeField] private Text languageButtonText;
        [SerializeField, Range(0.08f, 0.35f)] private float upgradesPanelAnimationDuration = 0.18f;
        [SerializeField] private RectTransform actionBarPanel;
        [SerializeField] private Button upgradesToggleButton;
        [SerializeField] private Text upgradesToggleButtonText;
        [SerializeField] private Button waiterPriorityButton;
        [SerializeField] private Text waiterPriorityButtonText;
        [SerializeField] private Image upgradesBackdropImage;
        [SerializeField] private Button upgradesBackdropButton;
        [SerializeField] private RectTransform upgradesPanel;
        [SerializeField] private Button upgradesCloseButton;
        [SerializeField] private Text upgradesCloseButtonText;
        [SerializeField] private Button tableUpgradeButton;
        [SerializeField] private Text tableUpgradeButtonText;
        [SerializeField] private Button waiterUpgradeButton;
        [SerializeField] private Text waiterUpgradeButtonText;
        [SerializeField] private Button kitchenUpgradeButton;
        [SerializeField] private Text kitchenUpgradeButtonText;
        [SerializeField] private Button barUpgradeButton;
        [SerializeField] private Text barUpgradeButtonText;
        [SerializeField] private Image notificationPanelImage;
        [SerializeField] private Text notificationText;
        [SerializeField] private Image offlinePopupOverlayImage;
        [SerializeField] private Text offlinePopupTitleText;
        [SerializeField] private Text offlinePopupBodyText;
        [SerializeField] private Button offlinePopupClaimButton;
        [SerializeField] private Text offlinePopupClaimButtonText;

        private bool buttonHandlersBound;
        private bool upgradesPanelVisible;
        private bool offlinePopupVisible;
        private bool offlineReportResolved;
        private int offlinePopupClaimAmount;
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private float notificationHideAt;
        private RestaurantRuntime subscribedRuntime;
        private Color notificationBasePanelColor = new Color(0.12f, 0.16f, 0.21f, 0.94f);
        private Color notificationBaseTextColor = new Color(0.98f, 0.98f, 0.98f, 1f);
        private CanvasGroup upgradesPanelCanvasGroup;
        private CanvasGroup upgradesBackdropCanvasGroup;
        private Vector2 upgradesPanelShownPosition;
        private Vector2 upgradesPanelHiddenPosition;
        private float upgradesPanelAnimationValue;
        private bool upgradesPanelAnimationInitialized;

        private void Awake()
        {
            ResolveRuntime();
            AutoAssignReferences();
            ConfigureCanvas();
            ForceUpgradesPanelState(false);
            SetOfflinePopupVisible(false);
        }

        private void OnEnable()
        {
            ResolveRuntime();
            AutoAssignReferences();
            CaptureNotificationBaseColors();
            buttonHandlersBound = false;
            upgradesPanelVisible = false;
            offlinePopupVisible = false;
            offlineReportResolved = false;
            offlinePopupClaimAmount = 0;
            upgradesPanelAnimationInitialized = false;
            ForceUpgradesPanelState(false);
            SetOfflinePopupVisible(false);
            BindRuntimeNotifications();
        }

        private void OnDisable()
        {
            UnbindRuntimeNotifications();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveRuntime();
            AutoAssignReferences();
            ConfigureCanvas();
            CaptureNotificationBaseColors();
        }
#endif

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveRuntime();
            AutoAssignReferences();
            ConfigureCanvas();
            EnsureEventSystem();
            EnsureLanguageButton();
            BindRuntimeNotifications();
            WireButtons();
            ApplySafeArea();
            TryResolveOfflineIncomePopup();
            UpdateStats();
            UpdateUpgradeButtons();
            UpdateUpgradesToggleLabel();
            UpdateWaiterPriorityButton();
            UpdateLanguageButton();
            UpdateUpgradesPanelAnimation();
            UpdateNotificationVisual();
        }

        [ContextMenu("Auto Assign HUD References")]
        public void AutoAssignReferences()
        {
            if (runtime == null)
            {
                runtime = GetComponent<RestaurantRuntime>();
            }

            if (runtimeCanvas == null)
            {
                Transform canvasTransform = transform.Find("RuntimeHudCanvas");
                if (canvasTransform != null)
                {
                    runtimeCanvas = canvasTransform.GetComponent<Canvas>();
                }
            }

            if (safeAreaRoot == null)
            {
                safeAreaRoot = FindRectTransform("RuntimeHudCanvas/SafeAreaRoot");
            }

            if (statsPanel == null)
            {
                statsPanel = FindRectTransform("RuntimeHudCanvas/SafeAreaRoot/StatsPanel");
            }

            if (statsText == null)
            {
                statsText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/StatsPanel/StatsText");
            }

            if (languageButton == null)
            {
                languageButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/LanguageButton");
            }

            if (languageButtonText == null)
            {
                languageButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/LanguageButton/Label");
            }

            if (actionBarPanel == null)
            {
                actionBarPanel = FindRectTransform("RuntimeHudCanvas/SafeAreaRoot/ActionBarPanel");
            }

            if (upgradesToggleButton == null)
            {
                upgradesToggleButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/ActionBarPanel/UpgradesToggleButton");
            }

            if (upgradesToggleButtonText == null)
            {
                upgradesToggleButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/ActionBarPanel/UpgradesToggleButton/Label");
            }

            if (waiterPriorityButton == null)
            {
                waiterPriorityButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/ActionBarPanel/PriorityModeButton");
            }

            if (waiterPriorityButtonText == null)
            {
                waiterPriorityButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/ActionBarPanel/PriorityModeButton/Label");
            }

            if (upgradesBackdropImage == null)
            {
                upgradesBackdropImage = FindComponent<Image>("RuntimeHudCanvas/SafeAreaRoot/UpgradesBackdrop");
            }

            if (upgradesBackdropButton == null)
            {
                upgradesBackdropButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/UpgradesBackdrop");
            }

            if (upgradesPanel == null)
            {
                upgradesPanel = FindRectTransform("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel");
            }

            if (upgradesCloseButton == null)
            {
                upgradesCloseButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/CloseButton");
            }

            if (upgradesCloseButtonText == null)
            {
                upgradesCloseButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/CloseButton/Label");
            }

            if (tableUpgradeButton == null)
            {
                tableUpgradeButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/TableUpgradeButton");
            }

            if (tableUpgradeButtonText == null)
            {
                tableUpgradeButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/TableUpgradeButton/Label");
            }

            if (waiterUpgradeButton == null)
            {
                waiterUpgradeButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/WaiterUpgradeButton");
            }

            if (waiterUpgradeButtonText == null)
            {
                waiterUpgradeButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/WaiterUpgradeButton/Label");
            }

            if (notificationPanelImage == null)
            {
                notificationPanelImage = FindComponent<Image>("RuntimeHudCanvas/SafeAreaRoot/NotificationPanel");
            }

            if (notificationText == null)
            {
                notificationText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/NotificationPanel/NotificationText");
            }

            if (kitchenUpgradeButton == null)
            {
                kitchenUpgradeButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/KitchenUpgradeButton");
            }

            if (kitchenUpgradeButtonText == null)
            {
                kitchenUpgradeButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/KitchenUpgradeButton/Label");
            }

            if (barUpgradeButton == null)
            {
                barUpgradeButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/BarUpgradeButton");
            }

            if (barUpgradeButtonText == null)
            {
                barUpgradeButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/UpgradesPanel/BarUpgradeButton/Label");
            }

            if (offlinePopupOverlayImage == null)
            {
                offlinePopupOverlayImage = FindComponent<Image>("RuntimeHudCanvas/SafeAreaRoot/OfflinePopupOverlay");
            }

            if (offlinePopupTitleText == null)
            {
                offlinePopupTitleText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/OfflinePopupOverlay/Card/TitleText");
            }

            if (offlinePopupBodyText == null)
            {
                offlinePopupBodyText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/OfflinePopupOverlay/Card/BodyText");
            }

            if (offlinePopupClaimButton == null)
            {
                offlinePopupClaimButton = FindComponent<Button>("RuntimeHudCanvas/SafeAreaRoot/OfflinePopupOverlay/Card/ClaimButton");
            }

            if (offlinePopupClaimButtonText == null)
            {
                offlinePopupClaimButtonText = FindComponent<Text>("RuntimeHudCanvas/SafeAreaRoot/OfflinePopupOverlay/Card/ClaimButton/Label");
            }
        }

        private void ResolveRuntime()
        {
            if (runtime == null)
            {
                runtime = GetComponent<RestaurantRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<RestaurantRuntime>();
            }
        }

        private void ConfigureCanvas()
        {
            if (runtimeCanvas == null)
            {
                return;
            }

            runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            runtimeCanvas.sortingOrder = 100;

            CanvasScaler scaler = runtimeCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = runtimeCanvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (runtimeCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                runtimeCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void BindRuntimeNotifications()
        {
            if (runtime == null || subscribedRuntime == runtime)
            {
                return;
            }

            UnbindRuntimeNotifications();
            subscribedRuntime = runtime;
            subscribedRuntime.NotificationRaised += HandleRuntimeNotification;
        }

        private void UnbindRuntimeNotifications()
        {
            if (subscribedRuntime == null)
            {
                return;
            }

            subscribedRuntime.NotificationRaised -= HandleRuntimeNotification;
            subscribedRuntime = null;
        }

        private void WireButtons()
        {
            if (buttonHandlersBound)
            {
                return;
            }

            EnsureLanguageButton();

            if (tableUpgradeButton != null)
            {
                RebindButton(tableUpgradeButton, HandleTableUpgradePressed);
            }

            if (waiterUpgradeButton != null)
            {
                RebindButton(waiterUpgradeButton, HandleWaiterUpgradePressed);
            }

            if (kitchenUpgradeButton != null)
            {
                RebindButton(kitchenUpgradeButton, HandleKitchenUpgradePressed);
            }

            if (barUpgradeButton != null)
            {
                RebindButton(barUpgradeButton, HandleBarUpgradePressed);
            }

            if (upgradesToggleButton != null)
            {
                RebindButton(upgradesToggleButton, HandleUpgradesTogglePressed);
            }

            if (waiterPriorityButton != null)
            {
                RebindButton(waiterPriorityButton, HandleWaiterPriorityPressed);
            }

            if (upgradesBackdropButton != null)
            {
                RebindButton(upgradesBackdropButton, HandleUpgradesDismissPressed);
            }

            if (upgradesCloseButton != null)
            {
                RebindButton(upgradesCloseButton, HandleUpgradesDismissPressed);
            }

            if (offlinePopupClaimButton != null)
            {
                RebindButton(offlinePopupClaimButton, HandleOfflineClaimPressed);
            }

            if (languageButton != null)
            {
                RebindButton(languageButton, HandleLanguagePressed);
            }

            buttonHandlersBound = true;
        }

        private void UpdateStats()
        {
            if (statsText == null || runtime == null)
            {
                return;
            }

            string waiterState = runtime.Waiter != null ? runtime.Waiter.CurrentTaskLabel : LocalizationService.Get("common.status.missing");
            statsText.text =
                LocalizationService.Format("rest.stats.cash", runtime.TotalMoney) + "\n" +
                LocalizationService.Format("rest.stats.guests_queue", runtime.ActiveGuestCount, runtime.QueueGuestCount) + "\n" +
                LocalizationService.Format("rest.stats.served_walkouts", runtime.ServedGuests, runtime.WalkedOutGuests) + "\n" +
                LocalizationService.Format("rest.stats.queue_loyalty", runtime.QueueWalkedOutGuests, runtime.LoyaltyScore) + "\n" +
                LocalizationService.Format("rest.stats.waiter", waiterState, runtime.WaiterPriorityLabel);
        }

        private void UpdateUpgradeButtons()
        {
            if (runtime == null)
            {
                return;
            }

            bool canAffordTableUpgrade = runtime.CanAffordTableRevenueUpgrade();
            if (tableUpgradeButton != null)
            {
                tableUpgradeButton.interactable = true;
                SetButtonVisual(tableUpgradeButton, tableUpgradeButtonText, canAffordTableUpgrade);
            }

            if (tableUpgradeButtonText != null)
            {
                tableUpgradeButtonText.text =
                    LocalizationService.Format(
                        "rest.upgrade.tables",
                        runtime.TableRevenueLevel,
                        canAffordTableUpgrade ? LocalizationService.Get("rest.ui.action_buy") : LocalizationService.Get("rest.ui.action_need"),
                        runtime.NextTableRevenueUpgradeCost,
                        runtime.TableRevenueMultiplier.ToString("0.00"));
            }

            bool canAffordWaiterUpgrade = runtime.CanAffordWaiterSpeedUpgrade();
            if (waiterUpgradeButton != null)
            {
                waiterUpgradeButton.interactable = true;
                SetButtonVisual(waiterUpgradeButton, waiterUpgradeButtonText, canAffordWaiterUpgrade);
            }

            if (waiterUpgradeButtonText != null)
            {
                waiterUpgradeButtonText.text =
                    LocalizationService.Format(
                        "rest.upgrade.waiter",
                        runtime.WaiterSpeedLevel,
                        canAffordWaiterUpgrade ? LocalizationService.Get("rest.ui.action_buy") : LocalizationService.Get("rest.ui.action_need"),
                        runtime.NextWaiterSpeedUpgradeCost,
                        runtime.WaiterSpeedMultiplier.ToString("0.00"));
            }

            bool canAffordKitchenUpgrade = runtime.CanAffordKitchenSpeedUpgrade();
            if (kitchenUpgradeButton != null)
            {
                kitchenUpgradeButton.interactable = true;
                SetButtonVisual(kitchenUpgradeButton, kitchenUpgradeButtonText, canAffordKitchenUpgrade);
            }

            if (kitchenUpgradeButtonText != null)
            {
                kitchenUpgradeButtonText.text =
                    LocalizationService.Format(
                        "rest.upgrade.kitchen",
                        runtime.KitchenSpeedLevel,
                        canAffordKitchenUpgrade ? LocalizationService.Get("rest.ui.action_buy") : LocalizationService.Get("rest.ui.action_need"),
                        runtime.NextKitchenSpeedUpgradeCost,
                        runtime.KitchenSpeedMultiplier.ToString("0.00"));
            }

            bool canAffordBarUpgrade = runtime.CanAffordBarSpeedUpgrade();
            if (barUpgradeButton != null)
            {
                barUpgradeButton.interactable = true;
                SetButtonVisual(barUpgradeButton, barUpgradeButtonText, canAffordBarUpgrade);
            }

            if (barUpgradeButtonText != null)
            {
                barUpgradeButtonText.text =
                    LocalizationService.Format(
                        "rest.upgrade.bar",
                        runtime.BarSpeedLevel,
                        canAffordBarUpgrade ? LocalizationService.Get("rest.ui.action_buy") : LocalizationService.Get("rest.ui.action_need"),
                        runtime.NextBarSpeedUpgradeCost,
                        runtime.BarSpeedMultiplier.ToString("0.00"));
            }
        }

        private void HandleTableUpgradePressed()
        {
            if (runtime == null)
            {
                ShowNotification(LocalizationService.Get("rest.notify.runtime_not_ready"), RestaurantNotificationType.Warning);
                return;
            }

            if (!runtime.TryPurchaseTableRevenueUpgrade())
            {
                ShowNotification(
                    LocalizationService.Format("rest.notify.need_table_upgrade", runtime.NextTableRevenueUpgradeCost),
                    RestaurantNotificationType.Warning);
            }
        }

        private void HandleWaiterUpgradePressed()
        {
            if (runtime == null)
            {
                ShowNotification(LocalizationService.Get("rest.notify.runtime_not_ready"), RestaurantNotificationType.Warning);
                return;
            }

            if (!runtime.TryPurchaseWaiterSpeedUpgrade())
            {
                ShowNotification(
                    LocalizationService.Format("rest.notify.need_waiter_upgrade", runtime.NextWaiterSpeedUpgradeCost),
                    RestaurantNotificationType.Warning);
            }
        }

        private void HandleKitchenUpgradePressed()
        {
            if (runtime == null)
            {
                ShowNotification(LocalizationService.Get("rest.notify.runtime_not_ready"), RestaurantNotificationType.Warning);
                return;
            }

            if (!runtime.TryPurchaseKitchenSpeedUpgrade())
            {
                ShowNotification(
                    LocalizationService.Format("rest.notify.need_kitchen_upgrade", runtime.NextKitchenSpeedUpgradeCost),
                    RestaurantNotificationType.Warning);
            }
        }

        private void HandleBarUpgradePressed()
        {
            if (runtime == null)
            {
                ShowNotification(LocalizationService.Get("rest.notify.runtime_not_ready"), RestaurantNotificationType.Warning);
                return;
            }

            if (!runtime.TryPurchaseBarSpeedUpgrade())
            {
                ShowNotification(
                    LocalizationService.Format("rest.notify.need_bar_upgrade", runtime.NextBarSpeedUpgradeCost),
                    RestaurantNotificationType.Warning);
            }
        }

        private void HandleUpgradesTogglePressed()
        {
            if (offlinePopupVisible)
            {
                return;
            }

            SetUpgradesPanelVisible(!upgradesPanelVisible);
            UpdateUpgradesToggleLabel();
        }

        private void HandleWaiterPriorityPressed()
        {
            if (runtime == null)
            {
                ShowNotification(LocalizationService.Get("rest.notify.runtime_not_ready"), RestaurantNotificationType.Warning);
                return;
            }

            runtime.CycleWaiterPriorityMode();
            UpdateWaiterPriorityButton();
        }

        private void HandleLanguagePressed()
        {
            LocalizationService.ToggleLanguage();
        }

        private void HandleUpgradesDismissPressed()
        {
            if (offlinePopupVisible || !upgradesPanelVisible)
            {
                return;
            }

            SetUpgradesPanelVisible(false);
            UpdateUpgradesToggleLabel();
        }

        private void HandleOfflineClaimPressed()
        {
            if (!offlinePopupVisible)
            {
                return;
            }

            int claimedAmount = offlinePopupClaimAmount;
            SetOfflinePopupVisible(false);
            ShowNotification(LocalizationService.Format("rest.notify.offline_income", claimedAmount), RestaurantNotificationType.Success);
        }

        private void HandleRuntimeNotification(string message, RestaurantNotificationType notificationType)
        {
            ShowNotification(message, notificationType);
        }

        private void ShowNotification(string message, RestaurantNotificationType notificationType)
        {
            if (string.IsNullOrWhiteSpace(message) || notificationPanelImage == null || notificationText == null)
            {
                return;
            }

            notificationText.text = message;
            notificationBasePanelColor = GetNotificationPanelColor(notificationType);
            notificationBaseTextColor = GetNotificationTextColor(notificationType);
            notificationHideAt = Time.unscaledTime + NotificationDuration;
            notificationPanelImage.gameObject.SetActive(true);
            ApplyNotificationAlpha(1f);
        }

        private void UpdateNotificationVisual()
        {
            if (notificationPanelImage == null || notificationText == null)
            {
                return;
            }

            if (notificationHideAt <= 0f)
            {
                notificationPanelImage.gameObject.SetActive(false);
                return;
            }

            float remaining = notificationHideAt - Time.unscaledTime;
            if (remaining <= 0f)
            {
                notificationHideAt = 0f;
                notificationPanelImage.gameObject.SetActive(false);
                return;
            }

            notificationPanelImage.gameObject.SetActive(true);
            float alpha = remaining < NotificationFadeDuration ? remaining / NotificationFadeDuration : 1f;
            ApplyNotificationAlpha(alpha);
        }

        private void TryResolveOfflineIncomePopup()
        {
            if (offlineReportResolved || runtime == null || !runtime.IsInitialized)
            {
                return;
            }

            offlineReportResolved = true;
            RestaurantRuntime.OfflineIncomeReport report;
            if (!runtime.TryConsumeOfflineIncomeReport(out report))
            {
                return;
            }

            ShowOfflineIncomePopup(report);
        }

        private void ShowOfflineIncomePopup(RestaurantRuntime.OfflineIncomeReport report)
        {
            offlinePopupClaimAmount = Mathf.Max(0, report.FinalIncome);

            if (offlinePopupTitleText != null)
            {
                offlinePopupTitleText.text = LocalizationService.Get("common.offline.title");
            }

            if (offlinePopupBodyText != null)
            {
                offlinePopupBodyText.text = BuildOfflineIncomeText(report);
            }

            if (offlinePopupClaimButtonText != null)
            {
                offlinePopupClaimButtonText.text = LocalizationService.Format("common.offline.claim", offlinePopupClaimAmount);
            }

            ForceUpgradesPanelState(false);
            SetOfflinePopupVisible(true);
            UpdateUpgradesToggleLabel();
        }

        private static string BuildOfflineIncomeText(RestaurantRuntime.OfflineIncomeReport report)
        {
            string durationLabel = FormatDuration(report.ElapsedSeconds);
            if (report.WasCapped)
            {
                return
                    LocalizationService.Get("common.offline.welcome") + "\n" +
                    LocalizationService.Format("common.offline.away", durationLabel) + "\n" +
                    LocalizationService.Format("common.offline.raw_final", report.RawIncome, report.FinalIncome) + "\n" +
                    LocalizationService.Format("common.offline.cap", report.SoftCap, report.Stage);
            }

            return
                LocalizationService.Get("common.offline.welcome") + "\n" +
                LocalizationService.Format("common.offline.away", durationLabel) + "\n" +
                LocalizationService.Format("common.offline.earned", report.FinalIncome);
        }

        private static string FormatDuration(int totalSeconds)
        {
            if (totalSeconds <= 0)
            {
                return "0m";
            }

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            if (hours <= 0)
            {
                return LocalizationService.Format("common.duration.minutes", Mathf.Max(1, minutes));
            }

            return LocalizationService.Format("common.duration.hours_minutes", hours, minutes);
        }

        private void CaptureNotificationBaseColors()
        {
            if (notificationPanelImage != null)
            {
                Color panelColor = notificationPanelImage.color;
                if (panelColor.a > 0f)
                {
                    notificationBasePanelColor = panelColor;
                }
            }

            if (notificationText != null)
            {
                Color textColor = notificationText.color;
                if (textColor.a > 0f)
                {
                    notificationBaseTextColor = textColor;
                }
            }
        }

        private void ApplyNotificationAlpha(float alpha)
        {
            if (notificationPanelImage == null || notificationText == null)
            {
                return;
            }

            Color panelColor = notificationBasePanelColor;
            panelColor.a *= alpha;
            notificationPanelImage.color = panelColor;

            Color textColor = notificationBaseTextColor;
            textColor.a *= alpha;
            notificationText.color = textColor;
        }

        private void SetUpgradesPanelVisible(bool visible)
        {
            upgradesPanelVisible = visible;
            EnsureUpgradesPanelAnimationSetup();
            if (upgradesPanel != null && visible)
            {
                upgradesPanel.gameObject.SetActive(true);
            }

            if (upgradesBackdropImage != null && visible)
            {
                upgradesBackdropImage.gameObject.SetActive(true);
            }
        }

        private void SetOfflinePopupVisible(bool visible)
        {
            offlinePopupVisible = visible;
            if (offlinePopupOverlayImage != null)
            {
                offlinePopupOverlayImage.gameObject.SetActive(visible);
            }
        }

        private void UpdateUpgradesToggleLabel()
        {
            if (upgradesToggleButtonText != null)
            {
                upgradesToggleButtonText.text = LocalizationService.Get("rest.ui.upgrades");
            }

            if (upgradesToggleButton != null)
            {
                upgradesToggleButton.interactable = !offlinePopupVisible && !upgradesPanelVisible;
            }
        }

        private void UpdateWaiterPriorityButton()
        {
            if (waiterPriorityButton != null)
            {
                waiterPriorityButton.interactable = !offlinePopupVisible;
            }

            if (waiterPriorityButtonText != null)
            {
                string label = runtime != null ? runtime.WaiterPriorityLabel : LocalizationService.Get("rest.waiter.priority.balanced");
                waiterPriorityButtonText.text = LocalizationService.Format("rest.ui.mode", label);
                waiterPriorityButtonText.color = new Color(0.96f, 0.96f, 0.96f, 1f);
            }

            if (waiterPriorityButton != null && waiterPriorityButton.targetGraphic is Image buttonImage)
            {
                buttonImage.color = GetWaiterPriorityButtonColor();
            }
        }

        private Color GetWaiterPriorityButtonColor()
        {
            if (runtime == null)
            {
                return new Color(0.2f, 0.22f, 0.24f, 0.96f);
            }

            switch (runtime.WaiterPriority)
            {
                case WaiterPriorityMode.Speed:
                    return new Color(0.17f, 0.28f, 0.43f, 0.96f);
                case WaiterPriorityMode.TipFocus:
                    return new Color(0.31f, 0.22f, 0.12f, 0.96f);
                default:
                    return new Color(0.18f, 0.32f, 0.22f, 0.96f);
            }
        }

        private void ForceUpgradesPanelState(bool visible)
        {
            upgradesPanelVisible = visible;
            EnsureUpgradesPanelAnimationSetup();
            upgradesPanelAnimationValue = visible ? 1f : 0f;
            ApplyUpgradesPanelVisual(upgradesPanelAnimationValue);

            if (upgradesPanel != null)
            {
                upgradesPanel.gameObject.SetActive(visible);
            }

            if (upgradesBackdropImage != null)
            {
                upgradesBackdropImage.gameObject.SetActive(visible);
            }
        }

        private void UpdateUpgradesPanelAnimation()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureUpgradesPanelAnimationSetup();
            if (upgradesPanel == null)
            {
                return;
            }

            float targetValue = upgradesPanelVisible ? 1f : 0f;
            float duration = Mathf.Max(0.01f, upgradesPanelAnimationDuration);
            float step = Time.unscaledDeltaTime / duration;
            upgradesPanelAnimationValue = Mathf.MoveTowards(upgradesPanelAnimationValue, targetValue, step);
            ApplyUpgradesPanelVisual(upgradesPanelAnimationValue);

            if (!upgradesPanelVisible && upgradesPanelAnimationValue <= 0.0001f && upgradesPanel.gameObject.activeSelf)
            {
                upgradesPanel.gameObject.SetActive(false);
            }

            if (!upgradesPanelVisible &&
                upgradesPanelAnimationValue <= 0.0001f &&
                upgradesBackdropImage != null &&
                upgradesBackdropImage.gameObject.activeSelf)
            {
                upgradesBackdropImage.gameObject.SetActive(false);
            }
        }

        private void EnsureUpgradesPanelAnimationSetup()
        {
            if (upgradesPanel == null)
            {
                return;
            }

            if (upgradesPanelCanvasGroup == null)
            {
                upgradesPanelCanvasGroup = upgradesPanel.GetComponent<CanvasGroup>();
                if (upgradesPanelCanvasGroup == null)
                {
                    upgradesPanelCanvasGroup = upgradesPanel.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (upgradesBackdropImage != null && upgradesBackdropCanvasGroup == null)
            {
                upgradesBackdropCanvasGroup = upgradesBackdropImage.GetComponent<CanvasGroup>();
                if (upgradesBackdropCanvasGroup == null)
                {
                    upgradesBackdropCanvasGroup = upgradesBackdropImage.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (!upgradesPanelAnimationInitialized)
            {
                upgradesPanelShownPosition = upgradesPanel.anchoredPosition;
                upgradesPanelHiddenPosition = upgradesPanelShownPosition + new Vector2(0f, UpgradesPanelHiddenOffsetY);
                upgradesPanelAnimationValue = upgradesPanelVisible ? 1f : 0f;
                upgradesPanelAnimationInitialized = true;
                ApplyUpgradesPanelVisual(upgradesPanelAnimationValue);
            }
        }

        private void ApplyUpgradesPanelVisual(float normalized)
        {
            if (upgradesPanel == null || upgradesPanelCanvasGroup == null)
            {
                return;
            }

            float eased = normalized * normalized * (3f - 2f * normalized);
            upgradesPanel.anchoredPosition = Vector2.Lerp(upgradesPanelHiddenPosition, upgradesPanelShownPosition, eased);
            upgradesPanelCanvasGroup.alpha = eased;
            upgradesPanelCanvasGroup.interactable = upgradesPanelVisible && eased > 0.98f;
            upgradesPanelCanvasGroup.blocksRaycasts = upgradesPanelVisible && eased > 0.05f;

            if (upgradesBackdropCanvasGroup != null)
            {
                upgradesBackdropCanvasGroup.alpha = eased * UpgradesBackdropMaxAlpha;
                upgradesBackdropCanvasGroup.interactable = upgradesPanelVisible && eased > 0.05f;
                upgradesBackdropCanvasGroup.blocksRaycasts = upgradesPanelVisible && eased > 0.05f;
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
                GameObject eventSystemObject = new GameObject("RuntimeHudEventSystem");
                eventSystemObject.transform.SetParent(transform, false);
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();

#if ENABLE_INPUT_SYSTEM
            if (legacyModule != null)
            {
                legacyModule.enabled = false;
                Destroy(legacyModule);
            }

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

        private RectTransform FindRectTransform(string path)
        {
            Transform target = transform.Find(path);
            return target as RectTransform;
        }

        private T FindComponent<T>(string path) where T : Component
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static void RebindButton(Button button, UnityEngine.Events.UnityAction clickHandler)
        {
            if (button == null || clickHandler == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(clickHandler);
        }

        private static void SetButtonVisual(Button button, Text buttonText, bool canAfford)
        {
            if (button != null && button.targetGraphic is Image buttonImage)
            {
                buttonImage.color = canAfford
                    ? new Color(0.16f, 0.45f, 0.22f, 0.96f)
                    : new Color(0.22f, 0.24f, 0.27f, 0.96f);
            }

            if (buttonText != null)
            {
                buttonText.color = canAfford
                    ? new Color(0.98f, 0.98f, 0.98f, 1f)
                    : new Color(0.78f, 0.78f, 0.78f, 1f);
            }
        }

        private void EnsureLanguageButton()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (languageButton == null)
            {
                Transform existing = safeAreaRoot.Find("LanguageButton");
                if (existing != null)
                {
                    languageButton = existing.GetComponent<Button>();
                    languageButtonText = existing.GetComponentInChildren<Text>();
                }
            }

            if (languageButton != null)
            {
                return;
            }

            GameObject buttonObject = new GameObject("LanguageButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(safeAreaRoot, false);
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.sizeDelta = new Vector2(104f, 38f);
            buttonRect.anchoredPosition = new Vector2(18f, -154f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.22f, 0.28f, 0.37f, 0.96f);

            languageButton = buttonObject.GetComponent<Button>();
            languageButton.targetGraphic = buttonImage;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(buttonRect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 6f);
            labelRect.offsetMax = new Vector2(-8f, -6f);

            languageButtonText = labelObject.GetComponent<Text>();
            languageButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            languageButtonText.fontSize = 16;
            languageButtonText.alignment = TextAnchor.MiddleCenter;
            languageButtonText.color = Color.white;
        }

        private void UpdateLanguageButton()
        {
            if (languageButton == null || languageButtonText == null)
            {
                return;
            }

            languageButtonText.text = LocalizationService.CurrentLanguageCode;
        }

        private static Color GetNotificationPanelColor(RestaurantNotificationType notificationType)
        {
            switch (notificationType)
            {
                case RestaurantNotificationType.Success:
                    return new Color(0.12f, 0.34f, 0.16f, 0.94f);
                case RestaurantNotificationType.Warning:
                    return new Color(0.44f, 0.19f, 0.12f, 0.94f);
                default:
                    return new Color(0.12f, 0.16f, 0.21f, 0.94f);
            }
        }

        private static Color GetNotificationTextColor(RestaurantNotificationType notificationType)
        {
            switch (notificationType)
            {
                case RestaurantNotificationType.Warning:
                    return new Color(1f, 0.92f, 0.86f, 1f);
                default:
                    return new Color(0.98f, 0.98f, 0.98f, 1f);
            }
        }
    }
}
