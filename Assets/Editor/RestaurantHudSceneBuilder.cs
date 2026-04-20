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
        private enum CategoryIconKind
        {
            Waiter = 0,
            Furniture = 1,
            Bar = 2,
            Kitchen = 3
        }

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
            statsPanelRect.sizeDelta = new Vector2(188f, 60f);

            Image statsIcon = EnsureImageChild(statsPanel, "Icon", new Color(0.98f, 0.89f, 0.38f, 1f));
            statsIcon.sprite = Resources.Load<Sprite>("UI/Icons/CoinStack");
            statsIcon.type = Image.Type.Simple;
            statsIcon.preserveAspect = true;
            statsIcon.raycastTarget = false;
            RectTransform statsIconRect = Rect(statsIcon.gameObject);
            statsIconRect.anchorMin = new Vector2(0f, 0.5f);
            statsIconRect.anchorMax = new Vector2(0f, 0.5f);
            statsIconRect.pivot = new Vector2(0f, 0.5f);
            statsIconRect.anchoredPosition = new Vector2(14f, 0f);
            statsIconRect.sizeDelta = new Vector2(28f, 28f);

            Text statsText = EnsureText(statsPanel, "StatsText", 28, TextAnchor.MiddleLeft, FontStyle.Bold);
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statsText.verticalOverflow = VerticalWrapMode.Overflow;
            statsText.raycastTarget = false;
            statsText.resizeTextForBestFit = true;
            statsText.resizeTextMinSize = 16;
            statsText.resizeTextMaxSize = 28;
            RectTransform statsTextRect = Rect(statsText.gameObject);
            statsTextRect.anchorMin = new Vector2(0f, 0f);
            statsTextRect.anchorMax = new Vector2(1f, 1f);
            statsTextRect.offsetMin = new Vector2(52f, 0f);
            statsTextRect.offsetMax = new Vector2(-14f, 0f);
            statsText.text = BuildMoneyChipLabel(0);

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

            Button menuButton = EnsureButton(actionBarPanel, "MenuButton");
            RectTransform menuButtonRect = Rect(menuButton.gameObject);
            menuButtonRect.anchorMin = new Vector2(0f, 0f);
            menuButtonRect.anchorMax = new Vector2(0f, 0f);
            menuButtonRect.pivot = new Vector2(0f, 0.5f);
            menuButtonRect.anchoredPosition = new Vector2(76f, 32f);
            menuButtonRect.sizeDelta = new Vector2(64f, 64f);
            menuButton.GetComponent<Image>().color = new Color(0.2f, 0.31f, 0.42f, 0.97f);
            Text menuLabel = EnsureText(menuButton.gameObject, "Label", 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform menuLabelRect = Rect(menuLabel.gameObject);
            StretchFull(menuLabelRect);
            menuLabel.raycastTarget = false;
            menuLabel.text = GetMenuButtonLabel();
            SetButtonIcon(menuButton, "UI/Icons/RecipeBook", new Vector2(28f, 28f));

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

            Button waiterButton = EnsureButton(mainContent, "WaiterUpgradeButton");
            LayoutCategoryGridButton(Rect(waiterButton.gameObject), 0, 2, 2, new Vector2(220f, 220f), new Vector2(24f, 24f), new Vector2(0f, -6f));
            Text waiterLabel = EnsureText(waiterButton.gameObject, "Label", 24, TextAnchor.LowerCenter, FontStyle.Bold);
            ConfigureCategoryButton(waiterButton, waiterLabel, GetWaiterCategoryLabel(), CategoryIconKind.Waiter, new Color(0.22f, 0.43f, 0.29f, 0.98f));

            Button tableButton = EnsureButton(mainContent, "TableUpgradeButton");
            LayoutCategoryGridButton(Rect(tableButton.gameObject), 1, 2, 2, new Vector2(220f, 220f), new Vector2(24f, 24f), new Vector2(0f, -6f));
            Text tableLabel = EnsureText(tableButton.gameObject, "Label", 24, TextAnchor.LowerCenter, FontStyle.Bold);
            ConfigureCategoryButton(tableButton, tableLabel, GetFurnitureCategoryLabel(), CategoryIconKind.Furniture, new Color(0.41f, 0.3f, 0.22f, 0.98f));

            Button barButton = EnsureButton(mainContent, "BarUpgradeButton");
            LayoutCategoryGridButton(Rect(barButton.gameObject), 2, 2, 2, new Vector2(220f, 220f), new Vector2(24f, 24f), new Vector2(0f, -6f));
            Text barLabel = EnsureText(barButton.gameObject, "Label", 24, TextAnchor.LowerCenter, FontStyle.Bold);
            ConfigureCategoryButton(barButton, barLabel, GetBarCategoryLabel(), CategoryIconKind.Bar, new Color(0.2f, 0.33f, 0.47f, 0.98f));

            Button kitchenButton = EnsureButton(mainContent, "KitchenUpgradeButton");
            LayoutCategoryGridButton(Rect(kitchenButton.gameObject), 3, 2, 2, new Vector2(220f, 220f), new Vector2(24f, 24f), new Vector2(0f, -6f));
            Text kitchenLabel = EnsureText(kitchenButton.gameObject, "Label", 24, TextAnchor.LowerCenter, FontStyle.Bold);
            ConfigureCategoryButton(kitchenButton, kitchenLabel, GetKitchenCategoryLabel(), CategoryIconKind.Kitchen, new Color(0.47f, 0.3f, 0.16f, 0.98f));

            GameObject waiterListContent = EnsureUiObject(upgradesPanel, "WaiterListContent");
            RectTransform waiterListRect = Rect(waiterListContent);
            StretchFull(waiterListRect);
            waiterListRect.offsetMin = new Vector2(0f, 0f);
            waiterListRect.offsetMax = new Vector2(0f, 0f);
            waiterListContent.SetActive(false);

            Button waiterListBackButton = EnsureButton(waiterListContent, "BackButton");
            RectTransform waiterListBackRect = Rect(waiterListBackButton.gameObject);
            waiterListBackRect.anchorMin = new Vector2(0f, 1f);
            waiterListBackRect.anchorMax = new Vector2(0f, 1f);
            waiterListBackRect.pivot = new Vector2(0f, 1f);
            waiterListBackRect.anchoredPosition = new Vector2(16f, -14f);
            waiterListBackRect.sizeDelta = new Vector2(132f, 42f);
            waiterListBackButton.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.22f, 0.98f);
            Text waiterListBackLabel = EnsureText(waiterListBackButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform waiterListBackLabelRect = Rect(waiterListBackLabel.gameObject);
            StretchFull(waiterListBackLabelRect);
            waiterListBackLabel.raycastTarget = false;
            waiterListBackLabel.text = GetWaiterBackLabel();

            Text waiterListTitleText = EnsureText(waiterListContent, "TitleText", 28, TextAnchor.UpperCenter, FontStyle.Bold);
            waiterListTitleText.raycastTarget = false;
            RectTransform waiterListTitleRect = Rect(waiterListTitleText.gameObject);
            waiterListTitleRect.anchorMin = new Vector2(0.5f, 1f);
            waiterListTitleRect.anchorMax = new Vector2(0.5f, 1f);
            waiterListTitleRect.pivot = new Vector2(0.5f, 1f);
            waiterListTitleRect.anchoredPosition = new Vector2(0f, -22f);
            waiterListTitleRect.sizeDelta = new Vector2(620f, 42f);
            waiterListTitleText.text = GetWaiterListTitle();

            Button waiterRosterEntryButton = EnsureButton(waiterListContent, "WaiterRosterEntryButton");
            LayoutCategoryGridButton(Rect(waiterRosterEntryButton.gameObject), 0, 2, 1, new Vector2(240f, 252f), new Vector2(28f, 0f), new Vector2(0f, -8f));
            Text waiterRosterEntryLabel = EnsureText(waiterRosterEntryButton.gameObject, "Label", 18, TextAnchor.UpperCenter, FontStyle.Bold);
            ConfigureWaiterRosterCard(waiterRosterEntryButton, waiterRosterEntryLabel, GetWaiterEntryLabel(), false, new Color(0.2f, 0.39f, 0.28f, 0.98f));

            Button hireWaiterButton = EnsureButton(waiterListContent, "HireWaiterButton");
            LayoutCategoryGridButton(Rect(hireWaiterButton.gameObject), 1, 2, 1, new Vector2(240f, 252f), new Vector2(28f, 0f), new Vector2(0f, -8f));
            Text hireWaiterLabel = EnsureText(hireWaiterButton.gameObject, "Label", 18, TextAnchor.UpperCenter, FontStyle.Bold);
            ConfigureWaiterRosterCard(hireWaiterButton, hireWaiterLabel, GetHireWaiterLabel(), true, new Color(0.19f, 0.29f, 0.43f, 0.98f));

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

            GameObject categoryDetailsContent = EnsureUiObject(upgradesPanel, "CategoryDetailsContent");
            RectTransform categoryDetailsRect = Rect(categoryDetailsContent);
            StretchFull(categoryDetailsRect);
            categoryDetailsRect.offsetMin = new Vector2(0f, 0f);
            categoryDetailsRect.offsetMax = new Vector2(0f, 0f);
            categoryDetailsContent.SetActive(false);

            Button categoryBackButton = EnsureButton(categoryDetailsContent, "BackButton");
            RectTransform categoryBackRect = Rect(categoryBackButton.gameObject);
            categoryBackRect.anchorMin = new Vector2(0f, 1f);
            categoryBackRect.anchorMax = new Vector2(0f, 1f);
            categoryBackRect.pivot = new Vector2(0f, 1f);
            categoryBackRect.anchoredPosition = new Vector2(16f, -14f);
            categoryBackRect.sizeDelta = new Vector2(132f, 42f);
            categoryBackButton.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.22f, 0.98f);
            Text categoryBackLabel = EnsureText(categoryBackButton.gameObject, "Label", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform categoryBackLabelRect = Rect(categoryBackLabel.gameObject);
            StretchFull(categoryBackLabelRect);
            categoryBackLabel.raycastTarget = false;
            categoryBackLabel.text = GetCategoryBackLabel();

            Text categoryTitleText = EnsureText(categoryDetailsContent, "TitleText", 28, TextAnchor.UpperCenter, FontStyle.Bold);
            categoryTitleText.raycastTarget = false;
            RectTransform categoryTitleRect = Rect(categoryTitleText.gameObject);
            categoryTitleRect.anchorMin = new Vector2(0.5f, 1f);
            categoryTitleRect.anchorMax = new Vector2(0.5f, 1f);
            categoryTitleRect.pivot = new Vector2(0.5f, 1f);
            categoryTitleRect.anchoredPosition = new Vector2(0f, -22f);
            categoryTitleRect.sizeDelta = new Vector2(620f, 42f);
            categoryTitleText.text = GetFurnitureCategoryLabel();

            Text categorySummaryText = EnsureText(categoryDetailsContent, "SummaryText", 20, TextAnchor.MiddleCenter, FontStyle.Normal);
            categorySummaryText.horizontalOverflow = HorizontalWrapMode.Wrap;
            categorySummaryText.verticalOverflow = VerticalWrapMode.Overflow;
            categorySummaryText.raycastTarget = false;
            RectTransform categorySummaryRect = Rect(categorySummaryText.gameObject);
            categorySummaryRect.anchorMin = new Vector2(0.5f, 0.5f);
            categorySummaryRect.anchorMax = new Vector2(0.5f, 0.5f);
            categorySummaryRect.pivot = new Vector2(0.5f, 0.5f);
            categorySummaryRect.anchoredPosition = new Vector2(0f, 34f);
            categorySummaryRect.sizeDelta = new Vector2(720f, 120f);
            categorySummaryText.text = GetFurniturePreviewSummary();

            Button categoryActionButton = EnsureButton(categoryDetailsContent, "ActionButton");
            RectTransform categoryActionRect = Rect(categoryActionButton.gameObject);
            categoryActionRect.anchorMin = new Vector2(0.5f, 0f);
            categoryActionRect.anchorMax = new Vector2(0.5f, 0f);
            categoryActionRect.pivot = new Vector2(0.5f, 0f);
            categoryActionRect.anchoredPosition = new Vector2(0f, 28f);
            categoryActionRect.sizeDelta = new Vector2(560f, 94f);
            categoryActionButton.GetComponent<Image>().color = new Color(0.23f, 0.5f, 0.31f, 0.98f);
            Text categoryActionLabel = EnsureText(categoryActionButton.gameObject, "Label", 24, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform categoryActionLabelRect = Rect(categoryActionLabel.gameObject);
            StretchFull(categoryActionLabelRect);
            categoryActionLabelRect.offsetMin = new Vector2(18f, 10f);
            categoryActionLabelRect.offsetMax = new Vector2(-18f, -10f);
            categoryActionLabel.raycastTarget = false;
            categoryActionLabel.text = GetFurniturePreviewAction();

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

            GameObject menuPanel = EnsureUiObject(safeAreaRoot, "MenuPanel");
            SetupPanel(menuPanel, new Color(0.06f, 0.08f, 0.11f, 0.98f), true);
            RectTransform menuPanelRect = Rect(menuPanel);
            StretchFull(menuPanelRect);
            menuPanel.SetActive(false);

            Button menuCloseButton = EnsureButton(menuPanel, "CloseButton");
            RectTransform menuCloseRect = Rect(menuCloseButton.gameObject);
            menuCloseRect.anchorMin = new Vector2(1f, 1f);
            menuCloseRect.anchorMax = new Vector2(1f, 1f);
            menuCloseRect.pivot = new Vector2(1f, 1f);
            menuCloseRect.anchoredPosition = new Vector2(-24f, -24f);
            menuCloseRect.sizeDelta = new Vector2(180f, 54f);
            menuCloseButton.GetComponent<Image>().color = new Color(0.19f, 0.23f, 0.28f, 0.98f);
            Text menuCloseLabel = EnsureText(menuCloseButton.gameObject, "Label", 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform menuCloseLabelRect = Rect(menuCloseLabel.gameObject);
            StretchFull(menuCloseLabelRect);
            menuCloseLabel.raycastTarget = false;
            menuCloseLabel.text = GetMenuCloseButtonLabel();

            Text menuTitleText = EnsureText(menuPanel, "TitleText", 42, TextAnchor.UpperLeft, FontStyle.Bold);
            menuTitleText.raycastTarget = false;
            RectTransform menuTitleRect = Rect(menuTitleText.gameObject);
            menuTitleRect.anchorMin = new Vector2(0f, 1f);
            menuTitleRect.anchorMax = new Vector2(0f, 1f);
            menuTitleRect.pivot = new Vector2(0f, 1f);
            menuTitleRect.anchoredPosition = new Vector2(28f, -24f);
            menuTitleRect.sizeDelta = new Vector2(780f, 56f);
            menuTitleText.text = GetMenuPanelTitle();

            Text menuSubtitleText = EnsureText(menuPanel, "SubtitleText", 22, TextAnchor.UpperLeft, FontStyle.Normal);
            menuSubtitleText.raycastTarget = false;
            RectTransform menuSubtitleRect = Rect(menuSubtitleText.gameObject);
            menuSubtitleRect.anchorMin = new Vector2(0f, 1f);
            menuSubtitleRect.anchorMax = new Vector2(0f, 1f);
            menuSubtitleRect.pivot = new Vector2(0f, 1f);
            menuSubtitleRect.anchoredPosition = new Vector2(28f, -84f);
            menuSubtitleRect.sizeDelta = new Vector2(640f, 34f);
            menuSubtitleText.text = GetMenuPanelSubtitle();

            GameObject recipeCard = EnsureUiObject(menuPanel, "RecipeCard");
            SetupPanel(recipeCard, new Color(0.12f, 0.16f, 0.21f, 0.98f));
            RectTransform recipeCardRect = Rect(recipeCard);
            recipeCardRect.anchorMin = new Vector2(0.5f, 1f);
            recipeCardRect.anchorMax = new Vector2(0.5f, 1f);
            recipeCardRect.pivot = new Vector2(0.5f, 1f);
            recipeCardRect.anchoredPosition = new Vector2(0f, -148f);
            recipeCardRect.sizeDelta = new Vector2(1180f, 360f);

            Text recipeTitleText = EnsureText(recipeCard, "TitleText", 34, TextAnchor.UpperLeft, FontStyle.Bold);
            recipeTitleText.raycastTarget = false;
            RectTransform recipeTitleRect = Rect(recipeTitleText.gameObject);
            recipeTitleRect.anchorMin = new Vector2(0f, 1f);
            recipeTitleRect.anchorMax = new Vector2(1f, 1f);
            recipeTitleRect.pivot = new Vector2(0f, 1f);
            recipeTitleRect.offsetMin = new Vector2(28f, -58f);
            recipeTitleRect.offsetMax = new Vector2(-28f, -16f);
            recipeTitleText.text = GetWildBoarBurgerRecipeTitle();

            Text recipeDescriptionText = EnsureText(recipeCard, "DescriptionText", 22, TextAnchor.UpperLeft, FontStyle.Normal);
            recipeDescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            recipeDescriptionText.verticalOverflow = VerticalWrapMode.Overflow;
            recipeDescriptionText.raycastTarget = false;
            RectTransform recipeDescriptionRect = Rect(recipeDescriptionText.gameObject);
            recipeDescriptionRect.anchorMin = new Vector2(0f, 1f);
            recipeDescriptionRect.anchorMax = new Vector2(1f, 1f);
            recipeDescriptionRect.pivot = new Vector2(0f, 1f);
            recipeDescriptionRect.offsetMin = new Vector2(28f, -136f);
            recipeDescriptionRect.offsetMax = new Vector2(-28f, -72f);
            recipeDescriptionText.text = GetWildBoarBurgerRecipeDescription();

            Text recipeUnlockText = EnsureText(recipeCard, "UnlockText", 20, TextAnchor.UpperLeft, FontStyle.Bold);
            recipeUnlockText.horizontalOverflow = HorizontalWrapMode.Wrap;
            recipeUnlockText.verticalOverflow = VerticalWrapMode.Overflow;
            recipeUnlockText.raycastTarget = false;
            RectTransform recipeUnlockRect = Rect(recipeUnlockText.gameObject);
            recipeUnlockRect.anchorMin = new Vector2(0f, 1f);
            recipeUnlockRect.anchorMax = new Vector2(1f, 1f);
            recipeUnlockRect.pivot = new Vector2(0f, 1f);
            recipeUnlockRect.offsetMin = new Vector2(28f, -208f);
            recipeUnlockRect.offsetMax = new Vector2(-28f, -148f);
            recipeUnlockText.text = GetWildBoarBurgerUnlockText();

            Text recipeStatusText = EnsureText(recipeCard, "StatusText", 20, TextAnchor.UpperLeft, FontStyle.Bold);
            recipeStatusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            recipeStatusText.verticalOverflow = VerticalWrapMode.Overflow;
            recipeStatusText.raycastTarget = false;
            RectTransform recipeStatusRect = Rect(recipeStatusText.gameObject);
            recipeStatusRect.anchorMin = new Vector2(0f, 1f);
            recipeStatusRect.anchorMax = new Vector2(1f, 1f);
            recipeStatusRect.pivot = new Vector2(0f, 1f);
            recipeStatusRect.offsetMin = new Vector2(28f, -252f);
            recipeStatusRect.offsetMax = new Vector2(-28f, -192f);
            recipeStatusText.text = GetWildBoarBurgerStatusText();

            Button recipeActionButton = EnsureButton(recipeCard, "ActionButton");
            RectTransform recipeActionRect = Rect(recipeActionButton.gameObject);
            recipeActionRect.anchorMin = new Vector2(0f, 0f);
            recipeActionRect.anchorMax = new Vector2(0f, 0f);
            recipeActionRect.pivot = new Vector2(0f, 0f);
            recipeActionRect.anchoredPosition = new Vector2(28f, 28f);
            recipeActionRect.sizeDelta = new Vector2(320f, 66f);
            recipeActionButton.GetComponent<Image>().color = new Color(0.23f, 0.5f, 0.31f, 0.98f);
            Text recipeActionLabel = EnsureText(recipeActionButton.gameObject, "Label", 24, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform recipeActionLabelRect = Rect(recipeActionLabel.gameObject);
            StretchFull(recipeActionLabelRect);
            recipeActionLabel.raycastTarget = false;
            recipeActionLabel.text = GetWildBoarBurgerActionLabel();

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
            serializedHud.FindProperty("statsIcon").objectReferenceValue = statsIcon;
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
            serializedHud.FindProperty("menuButton").objectReferenceValue = menuButton;
            serializedHud.FindProperty("menuButtonText").objectReferenceValue = menuLabel;
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
            serializedHud.FindProperty("waiterListContent").objectReferenceValue = waiterListRect;
            serializedHud.FindProperty("waiterListBackButton").objectReferenceValue = waiterListBackButton;
            serializedHud.FindProperty("waiterListBackButtonText").objectReferenceValue = waiterListBackLabel;
            serializedHud.FindProperty("waiterListTitleText").objectReferenceValue = waiterListTitleText;
            serializedHud.FindProperty("waiterRosterEntryButton").objectReferenceValue = waiterRosterEntryButton;
            serializedHud.FindProperty("waiterRosterEntryButtonText").objectReferenceValue = waiterRosterEntryLabel;
            serializedHud.FindProperty("hireWaiterButton").objectReferenceValue = hireWaiterButton;
            serializedHud.FindProperty("hireWaiterButtonText").objectReferenceValue = hireWaiterLabel;
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
            serializedHud.FindProperty("categoryDetailsContent").objectReferenceValue = categoryDetailsRect;
            serializedHud.FindProperty("categoryDetailsBackButton").objectReferenceValue = categoryBackButton;
            serializedHud.FindProperty("categoryDetailsBackButtonText").objectReferenceValue = categoryBackLabel;
            serializedHud.FindProperty("categoryDetailsTitleText").objectReferenceValue = categoryTitleText;
            serializedHud.FindProperty("categoryDetailsSummaryText").objectReferenceValue = categorySummaryText;
            serializedHud.FindProperty("categoryDetailsActionButton").objectReferenceValue = categoryActionButton;
            serializedHud.FindProperty("categoryDetailsActionButtonText").objectReferenceValue = categoryActionLabel;
            serializedHud.FindProperty("menuPanel").objectReferenceValue = menuPanelRect;
            serializedHud.FindProperty("menuCloseButton").objectReferenceValue = menuCloseButton;
            serializedHud.FindProperty("menuCloseButtonText").objectReferenceValue = menuCloseLabel;
            serializedHud.FindProperty("menuTitleText").objectReferenceValue = menuTitleText;
            serializedHud.FindProperty("menuSubtitleText").objectReferenceValue = menuSubtitleText;
            serializedHud.FindProperty("menuRecipeTitleText").objectReferenceValue = recipeTitleText;
            serializedHud.FindProperty("menuRecipeDescriptionText").objectReferenceValue = recipeDescriptionText;
            serializedHud.FindProperty("menuRecipeUnlockText").objectReferenceValue = recipeUnlockText;
            serializedHud.FindProperty("menuRecipeStatusText").objectReferenceValue = recipeStatusText;
            serializedHud.FindProperty("menuRecipeActionButton").objectReferenceValue = recipeActionButton;
            serializedHud.FindProperty("menuRecipeActionButtonText").objectReferenceValue = recipeActionLabel;
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

        private static void LayoutCategoryGridButton(
            RectTransform rect,
            int index,
            int columns,
            int rows,
            Vector2 buttonSize,
            Vector2 spacing,
            Vector2 centerOffset)
        {
            int column = index % columns;
            int row = index / columns;
            float totalWidth = columns * buttonSize.x + (columns - 1) * spacing.x;
            float totalHeight = rows * buttonSize.y + (rows - 1) * spacing.y;
            float startX = -totalWidth * 0.5f + buttonSize.x * 0.5f;
            float startY = totalHeight * 0.5f - buttonSize.y * 0.5f;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = new Vector2(
                startX + column * (buttonSize.x + spacing.x),
                startY - row * (buttonSize.y + spacing.y)) + centerOffset;
        }

        private static void ConfigureCategoryButton(Button button, Text label, string labelText, CategoryIconKind iconKind, Color backgroundColor)
        {
            if (button == null || label == null)
            {
                return;
            }

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = backgroundColor;
            }

            RectTransform labelRect = Rect(label.gameObject);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 16f);
            labelRect.sizeDelta = new Vector2(0f, 34f);
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = 24;
            label.raycastTarget = false;
            label.text = labelText;

            GameObject iconRoot = EnsureUiObject(button.gameObject, "CategoryIcon");
            RectTransform iconRect = Rect(iconRoot);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, 20f);
            iconRect.sizeDelta = new Vector2(96f, 96f);
            ClearChildren(iconRoot.transform);
            BuildCategoryIcon(iconRoot, iconKind);
        }

        private static void ConfigureWaiterRosterCard(Button button, Text label, string labelText, bool isHireCard, Color backgroundColor)
        {
            if (button == null || label == null)
            {
                return;
            }

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = backgroundColor;
            }

            RectTransform labelRect = Rect(label.gameObject);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 16f);
            labelRect.sizeDelta = new Vector2(0f, 108f);
            label.alignment = TextAnchor.UpperCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 13;
            label.resizeTextMaxSize = 18;
            label.raycastTarget = false;
            label.text = labelText;

            GameObject iconRoot = EnsureUiObject(button.gameObject, "RosterIcon");
            RectTransform iconRect = Rect(iconRoot);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, 44f);
            iconRect.sizeDelta = new Vector2(96f, 96f);
            ClearChildren(iconRoot.transform);

            if (isHireCard)
            {
                EnsureIconPiece(iconRoot, "Circle", new Vector2(0f, 6f), new Vector2(70f, 70f), new Color(0.95f, 0.97f, 1f, 0.18f));
                EnsureIconPiece(iconRoot, "PlusVertical", new Vector2(0f, 6f), new Vector2(14f, 52f), new Color(0.95f, 0.97f, 1f, 1f));
                EnsureIconPiece(iconRoot, "PlusHorizontal", new Vector2(0f, 6f), new Vector2(52f, 14f), new Color(0.95f, 0.97f, 1f, 1f));
                EnsureIconPiece(iconRoot, "Accent", new Vector2(0f, -34f), new Vector2(34f, 6f), new Color(0.41f, 0.74f, 0.98f, 1f));
            }
            else
            {
                BuildCategoryIcon(iconRoot, CategoryIconKind.Waiter);
                EnsureIconPiece(iconRoot, "Badge", new Vector2(0f, -34f), new Vector2(48f, 8f), new Color(0.91f, 0.79f, 0.32f, 1f));
            }
        }

        private static void BuildCategoryIcon(GameObject root, CategoryIconKind iconKind)
        {
            switch (iconKind)
            {
                case CategoryIconKind.Waiter:
                    EnsureIconPiece(root, "Head", new Vector2(-10f, 22f), new Vector2(22f, 22f), new Color(0.98f, 0.96f, 0.92f, 1f));
                    EnsureIconPiece(root, "Body", new Vector2(-10f, -2f), new Vector2(20f, 34f), new Color(0.97f, 0.98f, 1f, 1f));
                    EnsureIconPiece(root, "Arm", new Vector2(14f, 8f), new Vector2(28f, 8f), new Color(0.97f, 0.98f, 1f, 1f));
                    EnsureIconPiece(root, "Tray", new Vector2(24f, 18f), new Vector2(34f, 8f), new Color(0.9f, 0.81f, 0.42f, 1f));
                    break;

                case CategoryIconKind.Furniture:
                    EnsureIconPiece(root, "Top", new Vector2(0f, 16f), new Vector2(58f, 18f), new Color(0.79f, 0.59f, 0.38f, 1f));
                    EnsureIconPiece(root, "LegLeftTop", new Vector2(-20f, -6f), new Vector2(8f, 28f), new Color(0.95f, 0.95f, 0.95f, 1f));
                    EnsureIconPiece(root, "LegRightTop", new Vector2(20f, -6f), new Vector2(8f, 28f), new Color(0.95f, 0.95f, 0.95f, 1f));
                    EnsureIconPiece(root, "LegLeftBottom", new Vector2(-20f, -30f), new Vector2(8f, 20f), new Color(0.95f, 0.95f, 0.95f, 1f));
                    EnsureIconPiece(root, "LegRightBottom", new Vector2(20f, -30f), new Vector2(8f, 20f), new Color(0.95f, 0.95f, 0.95f, 1f));
                    break;

                case CategoryIconKind.Bar:
                    EnsureIconPiece(root, "BottleBody", new Vector2(-14f, -2f), new Vector2(18f, 42f), new Color(0.74f, 0.91f, 1f, 1f));
                    EnsureIconPiece(root, "BottleNeck", new Vector2(-14f, 26f), new Vector2(10f, 14f), new Color(0.74f, 0.91f, 1f, 1f));
                    EnsureIconPiece(root, "Glass", new Vector2(16f, -4f), new Vector2(20f, 34f), new Color(0.95f, 0.97f, 1f, 1f));
                    EnsureIconPiece(root, "Drink", new Vector2(16f, -14f), new Vector2(14f, 12f), new Color(0.96f, 0.68f, 0.28f, 1f));
                    break;

                default:
                    EnsureIconPiece(root, "PotBody", new Vector2(0f, -2f), new Vector2(50f, 26f), new Color(0.95f, 0.96f, 0.97f, 1f));
                    EnsureIconPiece(root, "PotLid", new Vector2(0f, 18f), new Vector2(38f, 8f), new Color(0.95f, 0.96f, 0.97f, 1f));
                    EnsureIconPiece(root, "PotKnob", new Vector2(0f, 28f), new Vector2(10f, 10f), new Color(0.95f, 0.96f, 0.97f, 1f));
                    EnsureIconPiece(root, "HandleLeft", new Vector2(-32f, -2f), new Vector2(12f, 8f), new Color(0.95f, 0.96f, 0.97f, 1f));
                    EnsureIconPiece(root, "HandleRight", new Vector2(32f, -2f), new Vector2(12f, 8f), new Color(0.95f, 0.96f, 0.97f, 1f));
                    break;
            }
        }

        private static Image EnsureIconPiece(GameObject parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            Image piece = EnsureImageChild(parent, name, color);
            RectTransform pieceRect = Rect(piece.gameObject);
            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.pivot = new Vector2(0.5f, 0.5f);
            pieceRect.anchoredPosition = anchoredPosition;
            pieceRect.sizeDelta = size;
            piece.raycastTarget = false;
            return piece;
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

        private static string GetCategoryBackLabel()
        {
            return LocalizationService.IsRussian ? "Назад" : "Back";
        }

        private static string GetWaiterCategoryLabel()
        {
            return LocalizationService.IsRussian ? "Официант" : "Waiter";
        }

        private static string GetFurnitureCategoryLabel()
        {
            return LocalizationService.IsRussian ? "Мебель" : "Furniture";
        }

        private static string GetBarCategoryLabel()
        {
            return LocalizationService.IsRussian ? "Бар" : "Bar";
        }

        private static string GetKitchenCategoryLabel()
        {
            return LocalizationService.IsRussian ? "Кухня" : "Kitchen";
        }

        private static string GetFurniturePreviewSummary()
        {
            return LocalizationService.IsRussian
                ? "Столы ур.0\nДоход x1.00  Чаевые x1.00"
                : "Tables Lv.0\nIncome x1.00  Tips x1.00";
        }

        private static string GetFurniturePreviewAction()
        {
            return LocalizationService.IsRussian
                ? "Столы ур.0  Купить $120\nДоход x1.00"
                : "Tables Lv.0  Buy $120\nIncome x1.00";
        }

        private static string GetWaiterEntryLabel()
        {
            return LocalizationService.IsRussian
                ? "Официант 1 (Анатолий)\nОткрыть характеристики"
                : "Waiter 1 (Anatoly)\nOpen stats";
        }

        private static string GetWaiterListTitle()
        {
            return LocalizationService.IsRussian ? "Официанты" : "Waiters";
        }

        private static string GetHireWaiterLabel()
        {
            return LocalizationService.IsRussian
                ? "Нанять\nНовый слот\nСкоро"
                : "Hire\nNew Slot\nSoon";
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

        private static string GetMenuButtonLabel()
        {
            return string.Empty;
        }

        private static string BuildMoneyChipLabel(int moneyAmount)
        {
            return "$" + Mathf.Max(0, moneyAmount);
        }

        private static string GetInitialWaiterStatusLine()
        {
            string idleLabel = LocalizationService.Get("rest.waiter.task.idle");
            return LocalizationService.IsRussian ? "Официант: " + idleLabel : "Waiter: " + idleLabel;
        }

        private static string GetMenuCloseButtonLabel()
        {
            return LocalizationService.IsRussian ? "Закрыть" : "Close";
        }

        private static string GetMenuPanelTitle()
        {
            return LocalizationService.IsRussian ? "Книга рецептов" : "Recipe book";
        }

        private static string GetMenuPanelSubtitle()
        {
            return LocalizationService.IsRussian ? "Рецепты" : "Recipes";
        }

        private static string GetWildBoarBurgerRecipeTitle()
        {
            return LocalizationService.IsRussian ? "Бургер с мясом дикого кабана" : "Wild boar burger";
        }

        private static string GetWildBoarBurgerRecipeDescription()
        {
            return LocalizationService.IsRussian
                ? "Сытный бургер с котлетой из дикого кабана. После изучения рецепт останется доступным в книге рецептов."
                : "A hearty burger with a wild boar patty. Once studied, the recipe stays available in the recipe book.";
        }

        private static string GetWildBoarBurgerUnlockText()
        {
            return LocalizationService.IsRussian
                ? "Требование: пройти 3 уровень приключений. Лучший результат: 0."
                : "Requirement: clear adventure level 3. Best result: 0.";
        }

        private static string GetWildBoarBurgerStatusText()
        {
            return LocalizationService.IsRussian ? "Статус: закрыто" : "Status: locked";
        }

        private static string GetWildBoarBurgerActionLabel()
        {
            return LocalizationService.IsRussian ? "Нужно пройти 3 уровень" : "Need level 3";
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
