using IdleRestaurant.Gameplay;
using IdleRestaurant.Localization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace IdleRestaurant.Editor
{
    public static class RestaurantHudSceneBuilder
    {
        [MenuItem("Tools/Idle Restaurant/Rebuild Mobile HUD")]
        public static void RebuildMobileHud()
        {
            GameObject gameplay = GameObject.Find("Gameplay");
            if (gameplay == null)
            {
                Debug.LogError("Restaurant HUD Builder: Gameplay object not found.");
                return;
            }

            RestaurantHud hud = gameplay.GetComponent<RestaurantHud>();
            if (hud == null)
            {
                Debug.LogError("Restaurant HUD Builder: RestaurantHud component not found on Gameplay.");
                return;
            }

            GameObject canvasObject = EnsureUiObject(gameplay, "RuntimeHudCanvas");
            EnsureCanvas(canvasObject);
            ClearChildren(canvasObject.transform);

            GameObject safeAreaRoot = EnsureUiObject(canvasObject, "SafeAreaRoot");
            StretchFull(Rect(safeAreaRoot));

            GameObject statsPanel = EnsureUiObject(safeAreaRoot, "StatsPanel");
            SetupPanel(statsPanel, new Color(0.08f, 0.08f, 0.08f, 0.82f));
            RectTransform statsPanelRect = Rect(statsPanel);
            statsPanelRect.anchorMin = new Vector2(0f, 1f);
            statsPanelRect.anchorMax = new Vector2(0f, 1f);
            statsPanelRect.pivot = new Vector2(0f, 1f);
            statsPanelRect.anchoredPosition = new Vector2(20f, -20f);
            statsPanelRect.sizeDelta = new Vector2(440f, 236f);

            Text statsText = EnsureText(statsPanel, "StatsText", 18, TextAnchor.UpperLeft, FontStyle.Normal);
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statsText.verticalOverflow = VerticalWrapMode.Overflow;
            statsText.raycastTarget = false;
            RectTransform statsTextRect = Rect(statsText.gameObject);
            statsTextRect.anchorMin = new Vector2(0f, 0f);
            statsTextRect.anchorMax = new Vector2(1f, 1f);
            statsTextRect.offsetMin = new Vector2(20f, 18f);
            statsTextRect.offsetMax = new Vector2(-20f, -18f);
            statsText.text =
                "<size=18><color=#B6C5D9>" + LocalizationService.Get("common.restaurant") + "</color></size>\n" +
                "<size=34><b>" + LocalizationService.Format("rest.stats.cash", 0) + "</b></size>\n" +
                LocalizationService.Format("rest.stats.guests_queue", 0, 0) + "\n" +
                LocalizationService.Format("rest.stats.served_walkouts", 0, 0) + "\n" +
                LocalizationService.Format("rest.stats.queue_loyalty", 0, 0) + "\n" +
                LocalizationService.Format("rest.stats.waiter", LocalizationService.Get("rest.waiter.task.idle"), LocalizationService.Get("rest.waiter.priority.balanced"));

            Button settingsButton = EnsureButton(safeAreaRoot, "SettingsButton");
            RectTransform settingsButtonRect = Rect(settingsButton.gameObject);
            settingsButtonRect.anchorMin = new Vector2(1f, 1f);
            settingsButtonRect.anchorMax = new Vector2(1f, 1f);
            settingsButtonRect.pivot = new Vector2(1f, 1f);
            settingsButtonRect.anchoredPosition = new Vector2(-18f, -18f);
            settingsButtonRect.sizeDelta = new Vector2(48f, 48f);
            settingsButton.GetComponent<Image>().color = new Color(0.17f, 0.21f, 0.28f, 0.97f);
            Text settingsButtonLabel = EnsureText(settingsButton.gameObject, "Label", 28, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform settingsButtonLabelRect = Rect(settingsButtonLabel.gameObject);
            StretchFull(settingsButtonLabelRect);
            settingsButtonLabel.raycastTarget = false;
            settingsButtonLabel.text = string.Empty;
            SetButtonIcon(settingsButton, "UI/Icons/SettingsGear", new Vector2(24f, 24f));

            GameObject settingsPanel = EnsureUiObject(safeAreaRoot, "SettingsPanel");
            SetupPanel(settingsPanel, new Color(0.08f, 0.1f, 0.14f, 0.94f));
            RectTransform settingsPanelRect = Rect(settingsPanel);
            settingsPanelRect.anchorMin = new Vector2(1f, 1f);
            settingsPanelRect.anchorMax = new Vector2(1f, 1f);
            settingsPanelRect.pivot = new Vector2(1f, 1f);
            settingsPanelRect.anchoredPosition = new Vector2(-18f, -74f);
            settingsPanelRect.sizeDelta = new Vector2(268f, 132f);
            settingsPanel.SetActive(false);

            Text settingsTitleText = EnsureText(settingsPanel, "TitleText", 18, TextAnchor.UpperLeft, FontStyle.Bold);
            RectTransform settingsTitleRect = Rect(settingsTitleText.gameObject);
            settingsTitleRect.anchorMin = new Vector2(0f, 1f);
            settingsTitleRect.anchorMax = new Vector2(1f, 1f);
            settingsTitleRect.pivot = new Vector2(0.5f, 1f);
            settingsTitleRect.offsetMin = new Vector2(14f, -30f);
            settingsTitleRect.offsetMax = new Vector2(-14f, -8f);
            settingsTitleText.raycastTarget = false;
            settingsTitleText.text = GetSettingsTitle();

            Text languageTitleText = EnsureText(settingsPanel, "LanguageLabel", 15, TextAnchor.MiddleLeft, FontStyle.Bold);
            RectTransform languageTitleRect = Rect(languageTitleText.gameObject);
            languageTitleRect.anchorMin = new Vector2(0f, 1f);
            languageTitleRect.anchorMax = new Vector2(0f, 1f);
            languageTitleRect.pivot = new Vector2(0f, 1f);
            languageTitleRect.anchoredPosition = new Vector2(14f, -48f);
            languageTitleRect.sizeDelta = new Vector2(112f, 24f);
            languageTitleText.raycastTarget = false;
            languageTitleText.text = GetLanguageLabel();

            Button languageButton = EnsureButton(settingsPanel, "LanguageButton");
            RectTransform languageButtonRect = Rect(languageButton.gameObject);
            languageButtonRect.anchorMin = new Vector2(1f, 1f);
            languageButtonRect.anchorMax = new Vector2(1f, 1f);
            languageButtonRect.pivot = new Vector2(1f, 1f);
            languageButtonRect.anchoredPosition = new Vector2(-14f, -42f);
            languageButtonRect.sizeDelta = new Vector2(96f, 34f);
            languageButton.GetComponent<Image>().color = new Color(0.2f, 0.28f, 0.38f, 0.96f);
            Text languageButtonLabel = EnsureText(languageButton.gameObject, "Label", 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform languageButtonLabelRect = Rect(languageButtonLabel.gameObject);
            StretchFull(languageButtonLabelRect);
            languageButtonLabel.raycastTarget = false;
            languageButtonLabel.text = LocalizationService.CurrentLanguageCode;

            Text soundLabelText = EnsureText(settingsPanel, "SoundLabel", 15, TextAnchor.MiddleLeft, FontStyle.Bold);
            RectTransform soundLabelRect = Rect(soundLabelText.gameObject);
            soundLabelRect.anchorMin = new Vector2(0f, 1f);
            soundLabelRect.anchorMax = new Vector2(0f, 1f);
            soundLabelRect.pivot = new Vector2(0f, 1f);
            soundLabelRect.anchoredPosition = new Vector2(14f, -92f);
            soundLabelRect.sizeDelta = new Vector2(120f, 24f);
            soundLabelText.raycastTarget = false;
            soundLabelText.text = GetSoundLabel();

            Button soundToggleButton = EnsureButton(settingsPanel, "SoundToggleButton");
            RectTransform soundToggleRect = Rect(soundToggleButton.gameObject);
            soundToggleRect.anchorMin = new Vector2(1f, 1f);
            soundToggleRect.anchorMax = new Vector2(1f, 1f);
            soundToggleRect.pivot = new Vector2(1f, 1f);
            soundToggleRect.anchoredPosition = new Vector2(-14f, -84f);
            soundToggleRect.sizeDelta = new Vector2(56f, 38f);
            soundToggleButton.GetComponent<Image>().color = new Color(0.23f, 0.47f, 0.32f, 0.98f);
            Text soundToggleLabel = EnsureText(soundToggleButton.gameObject, "Label", 14, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform soundToggleLabelRect = Rect(soundToggleLabel.gameObject);
            StretchFull(soundToggleLabelRect);
            soundToggleLabel.raycastTarget = false;
            soundToggleLabel.text = string.Empty;
            SetButtonIcon(soundToggleButton, "UI/Icons/SoundOn", new Vector2(22f, 22f));
            Image soundToggleIcon = soundToggleButton.transform.Find("Icon")?.GetComponent<Image>();

            GameObject actionBarPanel = EnsureUiObject(safeAreaRoot, "ActionBarPanel");
            SetupPanel(actionBarPanel, new Color(0f, 0f, 0f, 0f));
            RectTransform actionBarRect = Rect(actionBarPanel);
            actionBarRect.anchorMin = new Vector2(0f, 0f);
            actionBarRect.anchorMax = new Vector2(0f, 0f);
            actionBarRect.pivot = new Vector2(0f, 0f);
            actionBarRect.anchoredPosition = new Vector2(20f, 20f);
            actionBarRect.sizeDelta = new Vector2(140f, 64f);
            actionBarPanel.GetComponent<Image>().raycastTarget = false;

            Button upgradesToggleButton = EnsureButton(actionBarPanel, "UpgradesToggleButton");
            RectTransform toggleRect = Rect(upgradesToggleButton.gameObject);
            toggleRect.anchorMin = new Vector2(0f, 0f);
            toggleRect.anchorMax = new Vector2(0f, 0f);
            toggleRect.pivot = new Vector2(0f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(0f, 32f);
            toggleRect.sizeDelta = new Vector2(64f, 64f);
            upgradesToggleButton.GetComponent<Image>().color = new Color(0.27f, 0.43f, 0.24f, 0.97f);
            Text toggleLabel = EnsureText(upgradesToggleButton.gameObject, "Label", 30, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform toggleLabelRect = Rect(toggleLabel.gameObject);
            StretchFull(toggleLabelRect);
            toggleLabel.raycastTarget = false;
            toggleLabel.text = string.Empty;
            SetButtonIcon(upgradesToggleButton, "UI/Icons/UpgradeArrow", new Vector2(26f, 26f));

            Button priorityModeButton = EnsureButton(actionBarPanel, "PriorityModeButton");
            RectTransform priorityRect = Rect(priorityModeButton.gameObject);
            priorityRect.anchorMin = new Vector2(0f, 0f);
            priorityRect.anchorMax = new Vector2(0f, 0f);
            priorityRect.pivot = new Vector2(0f, 0.5f);
            priorityRect.anchoredPosition = new Vector2(76f, 32f);
            priorityRect.sizeDelta = new Vector2(64f, 64f);
            priorityModeButton.GetComponent<Image>().color = new Color(0.2f, 0.31f, 0.42f, 0.97f);
            Text priorityLabel = EnsureText(priorityModeButton.gameObject, "Label", 26, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform priorityLabelRect = Rect(priorityLabel.gameObject);
            StretchFull(priorityLabelRect);
            priorityLabel.raycastTarget = false;
            priorityLabel.text = string.Empty;
            SetButtonIcon(priorityModeButton, "UI/Icons/ModeSliders", new Vector2(26f, 26f));

            GameObject upgradesBackdrop = EnsureUiObject(safeAreaRoot, "UpgradesBackdrop");
            SetupPanel(upgradesBackdrop, new Color(0f, 0f, 0f, 0.56f), true);
            RectTransform upgradesBackdropRect = Rect(upgradesBackdrop);
            StretchFull(upgradesBackdropRect);
            Button upgradesBackdropButton = upgradesBackdrop.GetComponent<Button>();
            if (upgradesBackdropButton == null)
            {
                upgradesBackdropButton = upgradesBackdrop.AddComponent<Button>();
            }

            upgradesBackdropButton.targetGraphic = upgradesBackdrop.GetComponent<Image>();
            upgradesBackdrop.SetActive(false);

            GameObject upgradesPanel = EnsureUiObject(safeAreaRoot, "UpgradesPanel");
            SetupPanel(upgradesPanel, new Color(0.08f, 0.1f, 0.14f, 0.95f));
            RectTransform upgradesRect = Rect(upgradesPanel);
            upgradesRect.anchorMin = new Vector2(0f, 0f);
            upgradesRect.anchorMax = new Vector2(1f, 0f);
            upgradesRect.pivot = new Vector2(0.5f, 0f);
            upgradesRect.offsetMin = new Vector2(24f, 122f);
            upgradesRect.offsetMax = new Vector2(-24f, 620f);

            GameObject mainContent = EnsureUiObject(upgradesPanel, "MainContent");
            RectTransform mainContentRect = Rect(mainContent);
            StretchFull(mainContentRect);
            mainContentRect.offsetMin = new Vector2(0f, 0f);
            mainContentRect.offsetMax = new Vector2(0f, 0f);

            Button tableButton = EnsureButton(mainContent, "TableUpgradeButton");
            LayoutButtonStack(Rect(tableButton.gameObject), 0, 4);
            Text tableLabel = EnsureText(tableButton.gameObject, "Label", 20, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform tableLabelRect = Rect(tableLabel.gameObject);
            StretchFull(tableLabelRect);
            tableLabelRect.offsetMin = new Vector2(18f, 12f);
            tableLabelRect.offsetMax = new Vector2(-18f, -12f);
            tableLabel.raycastTarget = false;
            tableLabel.text = "Tables Lv.0  Need $120\nIncome x1.00";

            Button waiterButton = EnsureButton(mainContent, "WaiterUpgradeButton");
            LayoutButtonStack(Rect(waiterButton.gameObject), 1, 4);
            Text waiterLabel = EnsureText(waiterButton.gameObject, "Label", 20, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterLabelRect = Rect(waiterLabel.gameObject);
            StretchFull(waiterLabelRect);
            waiterLabelRect.offsetMin = new Vector2(18f, 12f);
            waiterLabelRect.offsetMax = new Vector2(-18f, -12f);
            waiterLabel.raycastTarget = false;
            waiterLabel.text = GetWaiterEntryLabel();

            Button kitchenButton = EnsureButton(mainContent, "KitchenUpgradeButton");
            LayoutButtonStack(Rect(kitchenButton.gameObject), 2, 4);
            Text kitchenLabel = EnsureText(kitchenButton.gameObject, "Label", 20, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform kitchenLabelRect = Rect(kitchenLabel.gameObject);
            StretchFull(kitchenLabelRect);
            kitchenLabelRect.offsetMin = new Vector2(18f, 12f);
            kitchenLabelRect.offsetMax = new Vector2(-18f, -12f);
            kitchenLabel.raycastTarget = false;
            kitchenLabel.text = "Kitchen Lv.0  Need $105\nSpeed x1.00";

            Button barButton = EnsureButton(mainContent, "BarUpgradeButton");
            LayoutButtonStack(Rect(barButton.gameObject), 3, 4);
            Text barLabel = EnsureText(barButton.gameObject, "Label", 20, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform barLabelRect = Rect(barLabel.gameObject);
            StretchFull(barLabelRect);
            barLabelRect.offsetMin = new Vector2(18f, 12f);
            barLabelRect.offsetMax = new Vector2(-18f, -12f);
            barLabel.raycastTarget = false;
            barLabel.text = "Bar Lv.0  Need $95\nSpeed x1.00";

            GameObject waiterDetailsContent = EnsureUiObject(upgradesPanel, "WaiterDetailsContent");
            RectTransform waiterDetailsRect = Rect(waiterDetailsContent);
            StretchFull(waiterDetailsRect);
            waiterDetailsRect.offsetMin = new Vector2(0f, 0f);
            waiterDetailsRect.offsetMax = new Vector2(0f, 0f);
            waiterDetailsContent.SetActive(false);

            Button waiterBackButton = EnsureButton(waiterDetailsContent, "BackButton");
            RectTransform waiterBackRect = Rect(waiterBackButton.gameObject);
            waiterBackRect.anchorMin = new Vector2(0f, 1f);
            waiterBackRect.anchorMax = new Vector2(0f, 1f);
            waiterBackRect.pivot = new Vector2(0f, 1f);
            waiterBackRect.anchoredPosition = new Vector2(16f, -14f);
            waiterBackRect.sizeDelta = new Vector2(132f, 42f);
            waiterBackButton.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.22f, 0.98f);
            Text waiterBackLabel = EnsureText(waiterBackButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterBackLabelRect = Rect(waiterBackLabel.gameObject);
            StretchFull(waiterBackLabelRect);
            waiterBackLabel.raycastTarget = false;
            waiterBackLabel.text = GetWaiterBackLabel();

            Text waiterTitleText = EnsureText(waiterDetailsContent, "TitleText", 26, TextAnchor.UpperCenter, FontStyle.Bold);
            waiterTitleText.raycastTarget = false;
            RectTransform waiterTitleRect = Rect(waiterTitleText.gameObject);
            waiterTitleRect.anchorMin = new Vector2(0.5f, 1f);
            waiterTitleRect.anchorMax = new Vector2(0.5f, 1f);
            waiterTitleRect.pivot = new Vector2(0.5f, 1f);
            waiterTitleRect.anchoredPosition = new Vector2(0f, -18f);
            waiterTitleRect.sizeDelta = new Vector2(660f, 38f);
            waiterTitleText.text = GetWaiterDetailsTitle();

            Text waiterSummaryText = EnsureText(waiterDetailsContent, "SummaryText", 16, TextAnchor.UpperCenter, FontStyle.Normal);
            waiterSummaryText.horizontalOverflow = HorizontalWrapMode.Wrap;
            waiterSummaryText.verticalOverflow = VerticalWrapMode.Overflow;
            waiterSummaryText.raycastTarget = false;
            RectTransform waiterSummaryRect = Rect(waiterSummaryText.gameObject);
            waiterSummaryRect.anchorMin = new Vector2(0f, 1f);
            waiterSummaryRect.anchorMax = new Vector2(1f, 1f);
            waiterSummaryRect.pivot = new Vector2(0.5f, 1f);
            waiterSummaryRect.offsetMin = new Vector2(26f, -96f);
            waiterSummaryRect.offsetMax = new Vector2(-26f, -48f);
            waiterSummaryText.text = GetWaiterSummary();

            GameObject waiterButtonsRoot = EnsureUiObject(waiterDetailsContent, "ButtonsRoot");
            RectTransform waiterButtonsRect = Rect(waiterButtonsRoot);
            StretchFull(waiterButtonsRect);
            waiterButtonsRect.offsetMin = new Vector2(0f, 18f);
            waiterButtonsRect.offsetMax = new Vector2(0f, -144f);

            Button waiterMoveSpeedButton = EnsureButton(waiterButtonsRoot, "MoveSpeedUpgradeButton");
            LayoutButtonStack(Rect(waiterMoveSpeedButton.gameObject), 0, 5);
            Text waiterMoveSpeedLabel = EnsureText(waiterMoveSpeedButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterMoveSpeedLabelRect = Rect(waiterMoveSpeedLabel.gameObject);
            StretchFull(waiterMoveSpeedLabelRect);
            waiterMoveSpeedLabelRect.offsetMin = new Vector2(18f, 10f);
            waiterMoveSpeedLabelRect.offsetMax = new Vector2(-18f, -10f);
            waiterMoveSpeedLabel.raycastTarget = false;
            waiterMoveSpeedLabel.text = GetWaiterMoveLabel();

            Button waiterTakeOrderButton = EnsureButton(waiterButtonsRoot, "TakeOrderUpgradeButton");
            LayoutButtonStack(Rect(waiterTakeOrderButton.gameObject), 1, 5);
            Text waiterTakeOrderLabel = EnsureText(waiterTakeOrderButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterTakeOrderLabelRect = Rect(waiterTakeOrderLabel.gameObject);
            StretchFull(waiterTakeOrderLabelRect);
            waiterTakeOrderLabelRect.offsetMin = new Vector2(18f, 10f);
            waiterTakeOrderLabelRect.offsetMax = new Vector2(-18f, -10f);
            waiterTakeOrderLabel.raycastTarget = false;
            waiterTakeOrderLabel.text = GetWaiterTakeOrderLabel();

            Button waiterSubmitOrderButton = EnsureButton(waiterButtonsRoot, "SubmitOrderUpgradeButton");
            LayoutButtonStack(Rect(waiterSubmitOrderButton.gameObject), 2, 5);
            Text waiterSubmitOrderLabel = EnsureText(waiterSubmitOrderButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterSubmitOrderLabelRect = Rect(waiterSubmitOrderLabel.gameObject);
            StretchFull(waiterSubmitOrderLabelRect);
            waiterSubmitOrderLabelRect.offsetMin = new Vector2(18f, 10f);
            waiterSubmitOrderLabelRect.offsetMax = new Vector2(-18f, -10f);
            waiterSubmitOrderLabel.raycastTarget = false;
            waiterSubmitOrderLabel.text = GetWaiterSubmitLabel();

            Button waiterPickupButton = EnsureButton(waiterButtonsRoot, "PickupUpgradeButton");
            LayoutButtonStack(Rect(waiterPickupButton.gameObject), 3, 5);
            Text waiterPickupLabel = EnsureText(waiterPickupButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterPickupLabelRect = Rect(waiterPickupLabel.gameObject);
            StretchFull(waiterPickupLabelRect);
            waiterPickupLabelRect.offsetMin = new Vector2(18f, 10f);
            waiterPickupLabelRect.offsetMax = new Vector2(-18f, -10f);
            waiterPickupLabel.raycastTarget = false;
            waiterPickupLabel.text = GetWaiterPickupLabel();

            Button waiterCharismaButton = EnsureButton(waiterButtonsRoot, "CharismaUpgradeButton");
            LayoutButtonStack(Rect(waiterCharismaButton.gameObject), 4, 5);
            Text waiterCharismaLabel = EnsureText(waiterCharismaButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterCharismaLabelRect = Rect(waiterCharismaLabel.gameObject);
            StretchFull(waiterCharismaLabelRect);
            waiterCharismaLabelRect.offsetMin = new Vector2(18f, 10f);
            waiterCharismaLabelRect.offsetMax = new Vector2(-18f, -10f);
            waiterCharismaLabel.raycastTarget = false;
            waiterCharismaLabel.text = GetWaiterCharismaLabel();

            Button closeButton = EnsureButton(upgradesPanel, "CloseButton");
            RectTransform closeButtonRect = Rect(closeButton.gameObject);
            closeButtonRect.anchorMin = new Vector2(1f, 1f);
            closeButtonRect.anchorMax = new Vector2(1f, 1f);
            closeButtonRect.pivot = new Vector2(1f, 1f);
            closeButtonRect.anchoredPosition = new Vector2(-12f, -12f);
            closeButtonRect.sizeDelta = new Vector2(68f, 46f);
            closeButton.GetComponent<Image>().color = new Color(0.19f, 0.23f, 0.28f, 0.98f);
            Text closeLabel = EnsureText(closeButton.gameObject, "Label", 26, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform closeLabelRect = Rect(closeLabel.gameObject);
            StretchFull(closeLabelRect);
            closeLabel.raycastTarget = false;
            closeLabel.text = "X";

            GameObject notificationPanel = EnsureUiObject(safeAreaRoot, "NotificationPanel");
            SetupPanel(notificationPanel, new Color(0.12f, 0.16f, 0.21f, 0.94f));
            RectTransform notificationRect = Rect(notificationPanel);
            notificationRect.anchorMin = new Vector2(0.5f, 1f);
            notificationRect.anchorMax = new Vector2(0.5f, 1f);
            notificationRect.pivot = new Vector2(0.5f, 1f);
            notificationRect.anchoredPosition = new Vector2(0f, -18f);
            notificationRect.sizeDelta = new Vector2(660f, 72f);

            Text notificationText = EnsureText(notificationPanel, "NotificationText", 19, TextAnchor.MiddleCenter, FontStyle.Bold);
            notificationText.horizontalOverflow = HorizontalWrapMode.Wrap;
            notificationText.verticalOverflow = VerticalWrapMode.Overflow;
            notificationText.raycastTarget = false;
            RectTransform notificationTextRect = Rect(notificationText.gameObject);
            StretchFull(notificationTextRect);
            notificationTextRect.offsetMin = new Vector2(20f, 10f);
            notificationTextRect.offsetMax = new Vector2(-20f, -10f);
            notificationText.text = "Notification";

            GameObject offlinePopupOverlay = EnsureUiObject(safeAreaRoot, "OfflinePopupOverlay");
            SetupPanel(offlinePopupOverlay, new Color(0f, 0f, 0f, 0.64f));
            RectTransform popupOverlayRect = Rect(offlinePopupOverlay);
            StretchFull(popupOverlayRect);

            GameObject popupCard = EnsureUiObject(offlinePopupOverlay, "Card");
            SetupPanel(popupCard, new Color(0.1f, 0.12f, 0.16f, 0.98f));
            RectTransform popupCardRect = Rect(popupCard);
            popupCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupCardRect.pivot = new Vector2(0.5f, 0.5f);
            popupCardRect.anchoredPosition = Vector2.zero;
            popupCardRect.sizeDelta = new Vector2(760f, 428f);

            Text popupTitleText = EnsureText(popupCard, "TitleText", 34, TextAnchor.UpperCenter, FontStyle.Bold);
            popupTitleText.raycastTarget = false;
            RectTransform popupTitleRect = Rect(popupTitleText.gameObject);
            popupTitleRect.anchorMin = new Vector2(0f, 1f);
            popupTitleRect.anchorMax = new Vector2(1f, 1f);
            popupTitleRect.pivot = new Vector2(0.5f, 1f);
            popupTitleRect.anchoredPosition = new Vector2(0f, -24f);
            popupTitleRect.sizeDelta = new Vector2(0f, 60f);
            popupTitleText.text = LocalizationService.Get("common.offline.title");

            Text popupBodyText = EnsureText(popupCard, "BodyText", 23, TextAnchor.UpperCenter, FontStyle.Normal);
            popupBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            popupBodyText.verticalOverflow = VerticalWrapMode.Overflow;
            popupBodyText.raycastTarget = false;
            RectTransform popupBodyRect = Rect(popupBodyText.gameObject);
            popupBodyRect.anchorMin = new Vector2(0f, 0f);
            popupBodyRect.anchorMax = new Vector2(1f, 1f);
            popupBodyRect.offsetMin = new Vector2(28f, 132f);
            popupBodyRect.offsetMax = new Vector2(-28f, -98f);
            popupBodyText.text = "Welcome back!\nAway: 0m\nEarned $0";

            Button popupClaimButton = EnsureButton(popupCard, "ClaimButton");
            RectTransform popupClaimRect = Rect(popupClaimButton.gameObject);
            popupClaimRect.anchorMin = new Vector2(0.5f, 0f);
            popupClaimRect.anchorMax = new Vector2(0.5f, 0f);
            popupClaimRect.pivot = new Vector2(0.5f, 0f);
            popupClaimRect.anchoredPosition = new Vector2(0f, 24f);
            popupClaimRect.sizeDelta = new Vector2(440f, 80f);
            popupClaimButton.GetComponent<Image>().color = new Color(0.23f, 0.47f, 0.32f, 0.98f);

            Text popupClaimLabel = EnsureText(popupClaimButton.gameObject, "Label", 26, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform popupClaimLabelRect = Rect(popupClaimLabel.gameObject);
            StretchFull(popupClaimLabelRect);
            popupClaimLabel.raycastTarget = false;
            popupClaimLabel.text = "Claim +$0";

            offlinePopupOverlay.SetActive(false);

            GameObject eventSystemObject = EnsureEventSystem(gameplay);

            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("runtimeCanvas").objectReferenceValue = canvasObject.GetComponent<Canvas>();
            serializedHud.FindProperty("safeAreaRoot").objectReferenceValue = safeAreaRoot.GetComponent<RectTransform>();
            serializedHud.FindProperty("statsPanel").objectReferenceValue = statsPanelRect;
            serializedHud.FindProperty("statsText").objectReferenceValue = statsText;
            serializedHud.FindProperty("settingsButton").objectReferenceValue = settingsButton;
            serializedHud.FindProperty("settingsButtonText").objectReferenceValue = settingsButtonLabel;
            serializedHud.FindProperty("settingsPanel").objectReferenceValue = settingsPanelRect;
            serializedHud.FindProperty("settingsTitleText").objectReferenceValue = settingsTitleText;
            serializedHud.FindProperty("languageLabelText").objectReferenceValue = languageTitleText;
            serializedHud.FindProperty("languageButton").objectReferenceValue = languageButton;
            serializedHud.FindProperty("languageButtonText").objectReferenceValue = languageButtonLabel;
            serializedHud.FindProperty("soundLabelText").objectReferenceValue = soundLabelText;
            serializedHud.FindProperty("soundToggleButton").objectReferenceValue = soundToggleButton;
            serializedHud.FindProperty("soundToggleButtonText").objectReferenceValue = soundToggleLabel;
            serializedHud.FindProperty("soundToggleButtonIcon").objectReferenceValue = soundToggleIcon;
            serializedHud.FindProperty("actionBarPanel").objectReferenceValue = actionBarRect;
            serializedHud.FindProperty("upgradesToggleButton").objectReferenceValue = upgradesToggleButton;
            serializedHud.FindProperty("upgradesToggleButtonText").objectReferenceValue = toggleLabel;
            serializedHud.FindProperty("waiterPriorityButton").objectReferenceValue = priorityModeButton;
            serializedHud.FindProperty("waiterPriorityButtonText").objectReferenceValue = priorityLabel;
            serializedHud.FindProperty("upgradesBackdropImage").objectReferenceValue = upgradesBackdrop.GetComponent<Image>();
            serializedHud.FindProperty("upgradesBackdropButton").objectReferenceValue = upgradesBackdropButton;
            serializedHud.FindProperty("upgradesPanel").objectReferenceValue = upgradesRect;
            serializedHud.FindProperty("upgradesCloseButton").objectReferenceValue = closeButton;
            serializedHud.FindProperty("upgradesCloseButtonText").objectReferenceValue = closeLabel;
            serializedHud.FindProperty("upgradesMainContent").objectReferenceValue = mainContentRect;
            serializedHud.FindProperty("tableUpgradeButton").objectReferenceValue = tableButton;
            serializedHud.FindProperty("tableUpgradeButtonText").objectReferenceValue = tableLabel;
            serializedHud.FindProperty("waiterUpgradeButton").objectReferenceValue = waiterButton;
            serializedHud.FindProperty("waiterUpgradeButtonText").objectReferenceValue = waiterLabel;
            serializedHud.FindProperty("kitchenUpgradeButton").objectReferenceValue = kitchenButton;
            serializedHud.FindProperty("kitchenUpgradeButtonText").objectReferenceValue = kitchenLabel;
            serializedHud.FindProperty("barUpgradeButton").objectReferenceValue = barButton;
            serializedHud.FindProperty("barUpgradeButtonText").objectReferenceValue = barLabel;
            serializedHud.FindProperty("waiterDetailsContent").objectReferenceValue = waiterDetailsRect;
            serializedHud.FindProperty("waiterDetailsBackButton").objectReferenceValue = waiterBackButton;
            serializedHud.FindProperty("waiterDetailsBackButtonText").objectReferenceValue = waiterBackLabel;
            serializedHud.FindProperty("waiterDetailsTitleText").objectReferenceValue = waiterTitleText;
            serializedHud.FindProperty("waiterDetailsSummaryText").objectReferenceValue = waiterSummaryText;
            serializedHud.FindProperty("waiterMoveSpeedUpgradeButton").objectReferenceValue = waiterMoveSpeedButton;
            serializedHud.FindProperty("waiterMoveSpeedUpgradeButtonText").objectReferenceValue = waiterMoveSpeedLabel;
            serializedHud.FindProperty("waiterTakeOrderUpgradeButton").objectReferenceValue = waiterTakeOrderButton;
            serializedHud.FindProperty("waiterTakeOrderUpgradeButtonText").objectReferenceValue = waiterTakeOrderLabel;
            serializedHud.FindProperty("waiterSubmitOrderUpgradeButton").objectReferenceValue = waiterSubmitOrderButton;
            serializedHud.FindProperty("waiterSubmitOrderUpgradeButtonText").objectReferenceValue = waiterSubmitOrderLabel;
            serializedHud.FindProperty("waiterPickupUpgradeButton").objectReferenceValue = waiterPickupButton;
            serializedHud.FindProperty("waiterPickupUpgradeButtonText").objectReferenceValue = waiterPickupLabel;
            serializedHud.FindProperty("waiterCharismaUpgradeButton").objectReferenceValue = waiterCharismaButton;
            serializedHud.FindProperty("waiterCharismaUpgradeButtonText").objectReferenceValue = waiterCharismaLabel;
            serializedHud.FindProperty("notificationPanelImage").objectReferenceValue = notificationPanel.GetComponent<Image>();
            serializedHud.FindProperty("notificationText").objectReferenceValue = notificationText;
            serializedHud.FindProperty("offlinePopupOverlayImage").objectReferenceValue = offlinePopupOverlay.GetComponent<Image>();
            serializedHud.FindProperty("offlinePopupTitleText").objectReferenceValue = popupTitleText;
            serializedHud.FindProperty("offlinePopupBodyText").objectReferenceValue = popupBodyText;
            serializedHud.FindProperty("offlinePopupClaimButton").objectReferenceValue = popupClaimButton;
            serializedHud.FindProperty("offlinePopupClaimButtonText").objectReferenceValue = popupClaimLabel;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(canvasObject);
            EditorUtility.SetDirty(eventSystemObject);
            EditorSceneManager.MarkSceneDirty(gameplay.scene);
            Debug.Log("Restaurant HUD Builder: mobile HUD rebuilt in scene.");
        }

        private static GameObject EnsureUiObject(GameObject parent, string name)
        {
            Transform existing = parent.transform.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void EnsureCanvas(GameObject canvasObject)
        {
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            StretchFull(canvasObject.GetComponent<RectTransform>());
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void SetupPanel(GameObject panelObject, Color color, bool useSimpleBackground = false)
        {
            Image image = panelObject.GetComponent<Image>();
            if (image == null)
            {
                image = panelObject.AddComponent<Image>();
            }

            ApplyPanelStyle(image, color);
            if (useSimpleBackground)
            {
                image.sprite = null;
                image.type = Image.Type.Simple;
            }
        }

        private static Button EnsureButton(GameObject parent, string name)
        {
            GameObject buttonObject = EnsureUiObject(parent, name);
            Image image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            ApplyButtonStyle(button, new Color(0.19f, 0.23f, 0.28f, 0.98f));
            return button;
        }

        private static Slider EnsureSlider(GameObject sliderObject)
        {
            Slider slider = sliderObject.GetComponent<Slider>();
            if (slider == null)
            {
                slider = sliderObject.AddComponent<Slider>();
            }

            Image background = EnsureImageChild(sliderObject, "Background", new Color(0.16f, 0.18f, 0.2f, 0.98f));
            RectTransform backgroundRect = Rect(background.gameObject);
            StretchFull(backgroundRect);
            backgroundRect.offsetMin = new Vector2(0f, 10f);
            backgroundRect.offsetMax = new Vector2(0f, -10f);

            GameObject fillArea = EnsureUiObject(sliderObject, "Fill Area");
            RectTransform fillAreaRect = Rect(fillArea);
            StretchFull(fillAreaRect);
            fillAreaRect.offsetMin = new Vector2(12f, 10f);
            fillAreaRect.offsetMax = new Vector2(-12f, -10f);

            Image fill = EnsureImageChild(fillArea, "Fill", new Color(0.27f, 0.55f, 0.34f, 1f));
            RectTransform fillRect = Rect(fill.gameObject);
            StretchFull(fillRect);

            GameObject handleSlideArea = EnsureUiObject(sliderObject, "Handle Slide Area");
            RectTransform handleAreaRect = Rect(handleSlideArea);
            StretchFull(handleAreaRect);
            handleAreaRect.offsetMin = new Vector2(12f, 0f);
            handleAreaRect.offsetMax = new Vector2(-12f, 0f);

            Image handle = EnsureImageChild(handleSlideArea, "Handle", new Color(0.95f, 0.96f, 0.97f, 1f));
            RectTransform handleRect = Rect(handle.gameObject);
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(22f, 22f);

            background.raycastTarget = false;
            fill.raycastTarget = false;
            handle.raycastTarget = true;

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            return slider;
        }

        private static void LayoutButtonStack(RectTransform rect, int rowIndex, int totalRows)
        {
            float normalizedTop = 1f - rowIndex / (float)totalRows;
            float normalizedBottom = 1f - (rowIndex + 1) / (float)totalRows;

            rect.anchorMin = new Vector2(0f, normalizedBottom);
            rect.anchorMax = new Vector2(1f, normalizedTop);

            float edgePadding = 18f;
            float gap = 10f;
            float top = rowIndex == 0 ? -edgePadding : -(gap * 0.5f);
            float bottom = rowIndex >= totalRows - 1 ? edgePadding : gap * 0.5f;
            float left = edgePadding;
            float right = -edgePadding;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static Text EnsureText(GameObject parent, string name, int fontSize, TextAnchor alignment, FontStyle fontStyle)
        {
            GameObject textObject = EnsureUiObject(parent, name);
            Text text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
            }

            ApplyTextStyle(text, fontSize, alignment, new Color(0.98f, 0.98f, 0.98f, 1f), fontStyle);
            return text;
        }

        private static Image EnsureImageChild(GameObject parent, string name, Color color)
        {
            GameObject imageObject = EnsureUiObject(parent, name);
            Image image = imageObject.GetComponent<Image>();
            if (image == null)
            {
                image = imageObject.AddComponent<Image>();
            }

            ApplyPanelStyle(image, color);
            return image;
        }

        private static void SetButtonIcon(Button button, string resourcePath, Vector2 size)
        {
            if (button == null)
            {
                return;
            }

            GameObject iconObject = EnsureUiObject(button.gameObject, "Icon");
            RectTransform iconRect = Rect(iconObject);
            iconRect.SetParent(button.transform, false);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = size;

            Image iconImage = iconObject.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = iconObject.AddComponent<Image>();
            }

            iconImage.sprite = Resources.Load<Sprite>(resourcePath);
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
            iconImage.color = new Color(0.97f, 0.98f, 1f, 1f);
            iconImage.raycastTarget = false;
            iconObject.transform.SetAsLastSibling();
        }

        private static void ApplyPanelStyle(Image image, Color color)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = GetUiSprite();
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = true;
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

            image.sprite = GetUiSprite();
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = true;
            image.color = backgroundColor;
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.96f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            colors.selectedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
            colors.disabledColor = new Color(0.58f, 0.58f, 0.62f, 0.9f);
            button.colors = colors;
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

        private static Sprite GetUiSprite()
        {
            return Resources.Load<Sprite>("UI/RoundedRect");
        }

        private static string GetWaiterEntryLabel()
        {
            return LocalizationService.IsRussian
                ? "Официант 1 (Анатолий)\nОткрыть характеристики"
                : "Waiter 1 (Anatoly)\nOpen stats";
        }

        private static string GetWaiterBackLabel()
        {
            return LocalizationService.IsRussian ? "Назад" : "Back";
        }

        private static string GetWaiterDetailsTitle()
        {
            return LocalizationService.IsRussian ? "Официант 1 (Анатолий)" : "Waiter 1 (Anatoly)";
        }

        private static string GetWaiterSummary()
        {
            return LocalizationService.IsRussian
                ? "Передвижение x1.00  Принятие x1.00\nПробитие x1.00  Забор x1.00\nЧаевые x1.00  Лояльность +0"
                : "Move x1.00  Taking x1.00\nInput x1.00  Pickup x1.00\nTips x1.00  Loyalty +0";
        }

        private static string GetWaiterMoveLabel()
        {
            return LocalizationService.IsRussian
                ? "Передвижение ур.0  Купить $90\nСкорость x1.00"
                : "Movement Lv.0  Buy $90\nSpeed x1.00";
        }

        private static string GetWaiterTakeOrderLabel()
        {
            return LocalizationService.IsRussian
                ? "Принятие заказа ур.0  Купить $80\nСкорость x1.00"
                : "Taking order Lv.0  Buy $80\nSpeed x1.00";
        }

        private static string GetWaiterSubmitLabel()
        {
            return LocalizationService.IsRussian
                ? "Пробитие заказа ур.0  Купить $85\nСкорость x1.00"
                : "Submitting order Lv.0  Buy $85\nSpeed x1.00";
        }

        private static string GetWaiterPickupLabel()
        {
            return LocalizationService.IsRussian
                ? "Забор заказа ур.0  Купить $95\nСкорость x1.00"
                : "Picking up Lv.0  Buy $95\nSpeed x1.00";
        }

        private static string GetWaiterCharismaLabel()
        {
            return LocalizationService.IsRussian
                ? "Обаятельность ур.0  Купить $110\nЧаевые x1.00  Лояльность +0"
                : "Charisma Lv.0  Buy $110\nTips x1.00  Loyalty +0";
        }

        private static string GetSettingsTitle()
        {
            return LocalizationService.IsRussian ? "\u041d\u0430\u0441\u0442\u0440\u043e\u0439\u043a\u0438" : "Settings";
        }

        private static string GetLanguageLabel()
        {
            return LocalizationService.IsRussian ? "\u042f\u0437\u044b\u043a" : "Language";
        }

        private static string GetSoundLabel()
        {
            return LocalizationService.IsRussian ? "\u0417\u0432\u0443\u043a" : "Sound";
        }

        private static RectTransform Rect(GameObject go)
        {
            return go.GetComponent<RectTransform>();
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject EnsureEventSystem(GameObject gameplay)
        {
            Transform existing = gameplay.transform.Find("RuntimeHudEventSystem");
            GameObject eventSystemObject = existing != null ? existing.gameObject : new GameObject("RuntimeHudEventSystem");
            if (existing == null)
            {
                eventSystemObject.transform.SetParent(gameplay.transform, false);
            }

            if (eventSystemObject.GetComponent<EventSystem>() == null)
            {
                eventSystemObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule legacyModule = eventSystemObject.GetComponent<StandaloneInputModule>();
#if ENABLE_INPUT_SYSTEM
            if (legacyModule != null)
            {
                Object.DestroyImmediate(legacyModule, true);
            }

            InputSystemUIInputModule inputSystemModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                inputSystemModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            if (inputSystemModule.actionsAsset == null)
            {
                inputSystemModule.AssignDefaultActions();
            }
#else
            if (legacyModule == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
            return eventSystemObject;
        }
    }
}
