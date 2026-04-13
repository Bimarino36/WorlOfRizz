using IdleRestaurant.Gameplay;
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
            statsPanelRect.sizeDelta = new Vector2(420f, 212f);

            Text statsText = EnsureText(statsPanel, "StatsText", 20, TextAnchor.UpperLeft);
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statsText.verticalOverflow = VerticalWrapMode.Overflow;
            statsText.raycastTarget = false;
            RectTransform statsTextRect = Rect(statsText.gameObject);
            statsTextRect.anchorMin = new Vector2(0f, 0f);
            statsTextRect.anchorMax = new Vector2(1f, 1f);
            statsTextRect.offsetMin = new Vector2(14f, 12f);
            statsTextRect.offsetMax = new Vector2(-14f, -12f);
            statsText.text =
                "Cash $0\n" +
                "Guests 0  Queue 0\n" +
                "Served 0  Walkouts 0\n" +
                "Queue WO 0  Loyalty 0\n" +
                "Waiter: Idle (Balanced)";

            GameObject actionBarPanel = EnsureUiObject(safeAreaRoot, "ActionBarPanel");
            SetupPanel(actionBarPanel, new Color(0.08f, 0.08f, 0.08f, 0.84f));
            RectTransform actionBarRect = Rect(actionBarPanel);
            actionBarRect.anchorMin = new Vector2(0f, 0f);
            actionBarRect.anchorMax = new Vector2(1f, 0f);
            actionBarRect.pivot = new Vector2(0.5f, 0f);
            actionBarRect.offsetMin = new Vector2(24f, 20f);
            actionBarRect.offsetMax = new Vector2(-24f, 92f);

            Button upgradesToggleButton = EnsureButton(actionBarPanel, "UpgradesToggleButton");
            RectTransform toggleRect = Rect(upgradesToggleButton.gameObject);
            toggleRect.anchorMin = new Vector2(0f, 0f);
            toggleRect.anchorMax = new Vector2(0.58f, 1f);
            toggleRect.pivot = new Vector2(0f, 0.5f);
            toggleRect.offsetMin = new Vector2(8f, 8f);
            toggleRect.offsetMax = new Vector2(-6f, -8f);
            Text toggleLabel = EnsureText(upgradesToggleButton.gameObject, "Label", 26, TextAnchor.MiddleCenter);
            RectTransform toggleLabelRect = Rect(toggleLabel.gameObject);
            StretchFull(toggleLabelRect);
            toggleLabel.raycastTarget = false;
            toggleLabel.text = "Upgrades";

            Button priorityModeButton = EnsureButton(actionBarPanel, "PriorityModeButton");
            RectTransform priorityRect = Rect(priorityModeButton.gameObject);
            priorityRect.anchorMin = new Vector2(0.58f, 0f);
            priorityRect.anchorMax = new Vector2(1f, 1f);
            priorityRect.pivot = new Vector2(1f, 0.5f);
            priorityRect.offsetMin = new Vector2(6f, 8f);
            priorityRect.offsetMax = new Vector2(-8f, -8f);
            Text priorityLabel = EnsureText(priorityModeButton.gameObject, "Label", 22, TextAnchor.MiddleCenter);
            RectTransform priorityLabelRect = Rect(priorityLabel.gameObject);
            StretchFull(priorityLabelRect);
            priorityLabel.raycastTarget = false;
            priorityLabel.text = "Mode: Balanced";

            GameObject upgradesBackdrop = EnsureUiObject(safeAreaRoot, "UpgradesBackdrop");
            SetupPanel(upgradesBackdrop, new Color(0f, 0f, 0f, 0.5f));
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
            SetupPanel(upgradesPanel, new Color(0.08f, 0.08f, 0.08f, 0.88f));
            RectTransform upgradesRect = Rect(upgradesPanel);
            upgradesRect.anchorMin = new Vector2(0f, 0f);
            upgradesRect.anchorMax = new Vector2(1f, 0f);
            upgradesRect.pivot = new Vector2(0.5f, 0f);
            upgradesRect.offsetMin = new Vector2(24f, 104f);
            upgradesRect.offsetMax = new Vector2(-24f, 560f);

            Button tableButton = EnsureButton(upgradesPanel, "TableUpgradeButton");
            LayoutButtonStack(Rect(tableButton.gameObject), 0, 4);
            Text tableLabel = EnsureText(tableButton.gameObject, "Label", 22, TextAnchor.MiddleCenter);
            RectTransform tableLabelRect = Rect(tableLabel.gameObject);
            StretchFull(tableLabelRect);
            tableLabelRect.offsetMin = new Vector2(18f, 12f);
            tableLabelRect.offsetMax = new Vector2(-18f, -12f);
            tableLabel.raycastTarget = false;
            tableLabel.text = "Tables Lv.0  Need $120\nIncome x1.00";

            Button waiterButton = EnsureButton(upgradesPanel, "WaiterUpgradeButton");
            LayoutButtonStack(Rect(waiterButton.gameObject), 1, 4);
            Text waiterLabel = EnsureText(waiterButton.gameObject, "Label", 22, TextAnchor.MiddleCenter);
            RectTransform waiterLabelRect = Rect(waiterLabel.gameObject);
            StretchFull(waiterLabelRect);
            waiterLabelRect.offsetMin = new Vector2(18f, 12f);
            waiterLabelRect.offsetMax = new Vector2(-18f, -12f);
            waiterLabel.raycastTarget = false;
            waiterLabel.text = "Waiter Lv.0  Need $90\nSpeed x1.00";

            Button kitchenButton = EnsureButton(upgradesPanel, "KitchenUpgradeButton");
            LayoutButtonStack(Rect(kitchenButton.gameObject), 2, 4);
            Text kitchenLabel = EnsureText(kitchenButton.gameObject, "Label", 22, TextAnchor.MiddleCenter);
            RectTransform kitchenLabelRect = Rect(kitchenLabel.gameObject);
            StretchFull(kitchenLabelRect);
            kitchenLabelRect.offsetMin = new Vector2(18f, 12f);
            kitchenLabelRect.offsetMax = new Vector2(-18f, -12f);
            kitchenLabel.raycastTarget = false;
            kitchenLabel.text = "Kitchen Lv.0  Need $105\nSpeed x1.00";

            Button barButton = EnsureButton(upgradesPanel, "BarUpgradeButton");
            LayoutButtonStack(Rect(barButton.gameObject), 3, 4);
            Text barLabel = EnsureText(barButton.gameObject, "Label", 22, TextAnchor.MiddleCenter);
            RectTransform barLabelRect = Rect(barLabel.gameObject);
            StretchFull(barLabelRect);
            barLabelRect.offsetMin = new Vector2(18f, 12f);
            barLabelRect.offsetMax = new Vector2(-18f, -12f);
            barLabel.raycastTarget = false;
            barLabel.text = "Bar Lv.0  Need $95\nSpeed x1.00";

            Button closeButton = EnsureButton(upgradesPanel, "CloseButton");
            RectTransform closeButtonRect = Rect(closeButton.gameObject);
            closeButtonRect.anchorMin = new Vector2(1f, 1f);
            closeButtonRect.anchorMax = new Vector2(1f, 1f);
            closeButtonRect.pivot = new Vector2(1f, 1f);
            closeButtonRect.anchoredPosition = new Vector2(-8f, 48f);
            closeButtonRect.sizeDelta = new Vector2(92f, 58f);
            Text closeLabel = EnsureText(closeButton.gameObject, "Label", 32, TextAnchor.MiddleCenter);
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
            notificationRect.sizeDelta = new Vector2(620f, 64f);

            Text notificationText = EnsureText(notificationPanel, "NotificationText", 20, TextAnchor.MiddleCenter);
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
            popupCardRect.sizeDelta = new Vector2(760f, 420f);

            Text popupTitleText = EnsureText(popupCard, "TitleText", 34, TextAnchor.UpperCenter);
            popupTitleText.raycastTarget = false;
            RectTransform popupTitleRect = Rect(popupTitleText.gameObject);
            popupTitleRect.anchorMin = new Vector2(0f, 1f);
            popupTitleRect.anchorMax = new Vector2(1f, 1f);
            popupTitleRect.pivot = new Vector2(0.5f, 1f);
            popupTitleRect.anchoredPosition = new Vector2(0f, -24f);
            popupTitleRect.sizeDelta = new Vector2(0f, 60f);
            popupTitleText.text = "Offline Income";

            Text popupBodyText = EnsureText(popupCard, "BodyText", 24, TextAnchor.UpperCenter);
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
            popupClaimRect.sizeDelta = new Vector2(420f, 86f);

            Text popupClaimLabel = EnsureText(popupClaimButton.gameObject, "Label", 28, TextAnchor.MiddleCenter);
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
            serializedHud.FindProperty("tableUpgradeButton").objectReferenceValue = tableButton;
            serializedHud.FindProperty("tableUpgradeButtonText").objectReferenceValue = tableLabel;
            serializedHud.FindProperty("waiterUpgradeButton").objectReferenceValue = waiterButton;
            serializedHud.FindProperty("waiterUpgradeButtonText").objectReferenceValue = waiterLabel;
            serializedHud.FindProperty("kitchenUpgradeButton").objectReferenceValue = kitchenButton;
            serializedHud.FindProperty("kitchenUpgradeButtonText").objectReferenceValue = kitchenLabel;
            serializedHud.FindProperty("barUpgradeButton").objectReferenceValue = barButton;
            serializedHud.FindProperty("barUpgradeButtonText").objectReferenceValue = barLabel;
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

        private static void SetupPanel(GameObject panelObject, Color color)
        {
            Image image = panelObject.GetComponent<Image>();
            if (image == null)
            {
                image = panelObject.AddComponent<Image>();
            }

            image.color = color;
        }

        private static Button EnsureButton(GameObject parent, string name)
        {
            GameObject buttonObject = EnsureUiObject(parent, name);
            Image image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            image.color = new Color(0.22f, 0.24f, 0.27f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.9f);
            button.colors = colors;
            return button;
        }

        private static void LayoutButtonStack(RectTransform rect, int rowIndex, int totalRows)
        {
            float normalizedTop = 1f - rowIndex / (float)totalRows;
            float normalizedBottom = 1f - (rowIndex + 1) / (float)totalRows;

            rect.anchorMin = new Vector2(0f, normalizedBottom);
            rect.anchorMax = new Vector2(1f, normalizedTop);

            float edgePadding = 12f;
            float gap = 8f;
            float top = rowIndex == 0 ? -edgePadding : -(gap * 0.5f);
            float bottom = rowIndex >= totalRows - 1 ? edgePadding : gap * 0.5f;
            float left = edgePadding;
            float right = -edgePadding;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static Text EnsureText(GameObject parent, string name, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = EnsureUiObject(parent, name);
            Text text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
            }

            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.98f, 0.98f, 0.98f, 1f);
            return text;
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
