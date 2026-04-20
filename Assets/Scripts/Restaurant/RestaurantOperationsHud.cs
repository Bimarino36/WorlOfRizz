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
            RemovePanel();
            enabled = false;
        }

        private void LateUpdate()
        {
            ResolveReferences();
            RemovePanel();
            enabled = false;
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

        private void RemovePanel()
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            if (panelRoot == null && safeAreaRoot != null)
            {
                Transform existingPanel = safeAreaRoot.Find("OperationsPanel");
                if (existingPanel != null)
                {
                    panelRoot = existingPanel as RectTransform;
                }
            }

            if (panelRoot == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(panelRoot.gameObject);
            }
            else
            {
                DestroyImmediate(panelRoot.gameObject);
            }

            panelRoot = null;
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
