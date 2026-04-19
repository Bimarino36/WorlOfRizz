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
                panelRoot.sizeDelta = new Vector2(300f, 134f);
                panelRoot.anchoredPosition = new Vector2(-18f, -18f);

                Image panelImage = panelObject.GetComponent<Image>();
                panelImage.color = new Color(0.08f, 0.1f, 0.13f, 0.84f);
            }

            EnsureTitle();
            EnsureBody();
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
            titleRect.offsetMax = new Vector2(-14f, -8f);

            titleText = titleObject.GetComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 20;
            titleText.alignment = TextAnchor.UpperLeft;
            titleText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
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
            bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bodyText.fontSize = 16;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.color = new Color(0.9f, 0.92f, 0.94f, 1f);
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
    }
}
