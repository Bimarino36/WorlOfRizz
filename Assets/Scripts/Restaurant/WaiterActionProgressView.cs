using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class WaiterActionProgressView : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.62f, 0f);
        [SerializeField, Min(0f)] private float towardCameraOffset = 0.06f;
        [SerializeField, Min(0.05f)] private float faceDiameter = 0.24f;
        [SerializeField, Min(0f)] private float borderPadding = 0.02f;
        [SerializeField, Min(0.005f)] private float handWidth = 0.022f;
        [SerializeField, Min(0.03f)] private float handLength = 0.1f;
        [SerializeField, Min(0.01f)] private float centerDotSize = 0.04f;
        [SerializeField, Range(0f, 0.25f)] private float pulseScale = 0.06f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 2.2f;
        [SerializeField] private int sortingOrder = 140;
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.18f);
        [SerializeField] private Color faceColor = new Color(0.98f, 0.95f, 0.89f, 0.98f);
        [SerializeField] private Color borderColor = new Color(0.18f, 0.16f, 0.14f, 0.92f);
        [SerializeField] private Color handColor = new Color(0.98f, 0.76f, 0.3f, 0.98f);

        private Transform visualRoot;
        private Transform shadowTransform;
        private Transform borderTransform;
        private Transform faceTransform;
        private Transform handPivotTransform;
        private Transform handTransform;
        private Transform centerDotTransform;
        private SpriteRenderer shadowRenderer;
        private SpriteRenderer borderRenderer;
        private SpriteRenderer faceRenderer;
        private SpriteRenderer handRenderer;
        private SpriteRenderer centerDotRenderer;
        private bool shouldBeVisible;
        private float currentProgress;
        private float pulseSeed;
        private static Sprite circleSprite;
        private static Sprite squareSprite;

        public bool IsVisible => visualRoot != null && visualRoot.gameObject.activeSelf;

        private void Awake()
        {
            pulseSeed = Mathf.Abs(GetInstanceID() * 0.173f);
            EnsureView();
            HideImmediate();
        }

        private void LateUpdate()
        {
            if (visualRoot == null || !shouldBeVisible)
            {
                return;
            }

            UpdateVisualTransform();
        }

        public void SetProgress(float normalizedProgress, Color color)
        {
            EnsureView();
            if (visualRoot == null || handRenderer == null)
            {
                return;
            }

            shouldBeVisible = true;
            currentProgress = Mathf.Clamp01(normalizedProgress);
            handColor = color;
            handRenderer.color = handColor;
            centerDotRenderer.color = handColor;
            ApplyLayout();
            visualRoot.gameObject.SetActive(true);
            UpdateVisualTransform();
        }

        public void Hide()
        {
            shouldBeVisible = false;
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(false);
            }
        }

        public void HideImmediate()
        {
            shouldBeVisible = false;
            currentProgress = 0f;
            if (visualRoot != null)
            {
                visualRoot.localScale = Vector3.one;
                ApplyLayout();
                visualRoot.gameObject.SetActive(false);
            }
        }

        private void EnsureView()
        {
            if (visualRoot == null)
            {
                GameObject rootObject = new GameObject($"WaiterActionProgress_{GetInstanceID()}");
                rootObject.hideFlags = HideFlags.DontSave;
                visualRoot = rootObject.transform;
            }

            shadowRenderer = EnsureRenderer(ref shadowTransform, ref shadowRenderer, "Shadow", sortingOrder);
            borderRenderer = EnsureRenderer(ref borderTransform, ref borderRenderer, "Border", sortingOrder + 1);
            faceRenderer = EnsureRenderer(ref faceTransform, ref faceRenderer, "Face", sortingOrder + 2);
            centerDotRenderer = EnsureRenderer(ref centerDotTransform, ref centerDotRenderer, "CenterDot", sortingOrder + 4);

            if (handPivotTransform == null)
            {
                Transform existingPivot = visualRoot.Find("HandPivot");
                if (existingPivot != null)
                {
                    handPivotTransform = existingPivot;
                }
                else
                {
                    GameObject pivotObject = new GameObject("HandPivot");
                    handPivotTransform = pivotObject.transform;
                    handPivotTransform.SetParent(visualRoot, false);
                }
            }

            handRenderer = EnsureRenderer(ref handTransform, ref handRenderer, "Hand", sortingOrder + 3, handPivotTransform);

            Sprite circle = ResolveCircleSprite();
            Sprite square = ResolveSquareSprite();

            shadowRenderer.sprite = circle;
            borderRenderer.sprite = circle;
            faceRenderer.sprite = circle;
            centerDotRenderer.sprite = circle;
            handRenderer.sprite = square;

            shadowRenderer.color = shadowColor;
            borderRenderer.color = borderColor;
            faceRenderer.color = faceColor;
            handRenderer.color = handColor;
            centerDotRenderer.color = handColor;

            ApplyLayout();
        }

        private SpriteRenderer EnsureRenderer(ref Transform targetTransform, ref SpriteRenderer targetRenderer, string childName, int order, Transform parent = null)
        {
            Transform actualParent = parent == null ? visualRoot : parent;
            if (targetTransform == null)
            {
                Transform existing = actualParent.Find(childName);
                if (existing != null)
                {
                    targetTransform = existing;
                }
                else
                {
                    GameObject child = new GameObject(childName, typeof(SpriteRenderer));
                    targetTransform = child.transform;
                    targetTransform.SetParent(actualParent, false);
                }
            }

            if (targetRenderer == null)
            {
                targetRenderer = targetTransform.GetComponent<SpriteRenderer>();
            }

            targetRenderer.sortingOrder = order;
            targetRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            targetRenderer.receiveShadows = false;
            targetRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            targetRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            return targetRenderer;
        }

        private void ApplyLayout()
        {
            if (visualRoot == null || shadowTransform == null || borderTransform == null || faceTransform == null ||
                handPivotTransform == null || handTransform == null || centerDotTransform == null)
            {
                return;
            }

            float clampedFaceDiameter = Mathf.Max(0.05f, faceDiameter);
            float clampedBorderPadding = Mathf.Max(0f, borderPadding);
            float borderDiameter = clampedFaceDiameter + (clampedBorderPadding * 2f);
            float clampedHandWidth = Mathf.Clamp(handWidth, 0.005f, clampedFaceDiameter * 0.5f);
            float clampedHandLength = Mathf.Clamp(handLength, 0.03f, clampedFaceDiameter * 0.8f);
            float clampedDotSize = Mathf.Clamp(centerDotSize, 0.01f, clampedFaceDiameter * 0.45f);

            shadowTransform.localPosition = new Vector3(0.014f, -0.016f, 0.04f);
            shadowTransform.localRotation = Quaternion.identity;
            shadowTransform.localScale = new Vector3(borderDiameter, borderDiameter, 1f);

            borderTransform.localPosition = new Vector3(0f, 0f, 0.03f);
            borderTransform.localRotation = Quaternion.identity;
            borderTransform.localScale = new Vector3(borderDiameter, borderDiameter, 1f);

            faceTransform.localPosition = new Vector3(0f, 0f, 0.02f);
            faceTransform.localRotation = Quaternion.identity;
            faceTransform.localScale = new Vector3(clampedFaceDiameter, clampedFaceDiameter, 1f);

            handPivotTransform.localPosition = new Vector3(0f, 0f, 0.01f);
            handPivotTransform.localRotation = Quaternion.Euler(0f, 0f, -360f * currentProgress);
            handPivotTransform.localScale = Vector3.one;

            handTransform.localPosition = new Vector3(0f, clampedHandLength * 0.5f, 0f);
            handTransform.localRotation = Quaternion.identity;
            handTransform.localScale = new Vector3(clampedHandWidth, clampedHandLength, 1f);

            centerDotTransform.localPosition = new Vector3(0f, 0f, 0f);
            centerDotTransform.localRotation = Quaternion.identity;
            centerDotTransform.localScale = new Vector3(clampedDotSize, clampedDotSize, 1f);
        }

        private void UpdateVisualTransform()
        {
            Camera targetCamera = ResolveCamera();
            if (targetCamera == null)
            {
                return;
            }

            visualRoot.position = transform.position + worldOffset - (targetCamera.transform.forward * towardCameraOffset);
            visualRoot.rotation = Quaternion.LookRotation(targetCamera.transform.forward, targetCamera.transform.up);
            visualRoot.localScale = Vector3.one * GetPulseMultiplier();
        }

        private float GetPulseMultiplier()
        {
            float amplitude = Mathf.Clamp(pulseScale, 0f, 0.25f);
            if (amplitude <= 0.0001f)
            {
                return 1f;
            }

            float wave = Mathf.Sin((Time.time + pulseSeed) * pulseSpeed * Mathf.PI * 2f);
            return 1f + (wave * amplitude);
        }

        private static Camera ResolveCamera()
        {
            return Camera.main;
        }

        private static Sprite ResolveCircleSprite()
        {
            if (circleSprite != null)
            {
                return circleSprite;
            }

            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.name = "GeneratedClockCircle";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = (size - 2) * 0.5f;
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : clear);
                }
            }

            texture.Apply();
            circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return circleSprite;
        }

        private static Sprite ResolveSquareSprite()
        {
            if (squareSprite != null)
            {
                return squareSprite;
            }

            squareSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            return squareSprite;
        }

        private void OnDestroy()
        {
            if (visualRoot == null)
            {
                return;
            }

            GameObject visualObject = visualRoot.gameObject;
            visualRoot = null;
            shadowTransform = null;
            borderTransform = null;
            faceTransform = null;
            handPivotTransform = null;
            handTransform = null;
            centerDotTransform = null;
            shadowRenderer = null;
            borderRenderer = null;
            faceRenderer = null;
            handRenderer = null;
            centerDotRenderer = null;

            if (Application.isPlaying)
            {
                Destroy(visualObject);
            }
            else
            {
                DestroyImmediate(visualObject);
            }
        }
    }
}
