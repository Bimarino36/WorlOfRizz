using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IdleRestaurant.Gameplay
{
    [ExecuteAlways]
    public sealed class WorldBillboardLabel : MonoBehaviour
    {
        [SerializeField] private string labelText = "Label";
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.45f, 0f);
        [SerializeField] private Color textColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        [SerializeField, Min(1)] private int fontSize = 48;
        [SerializeField, Min(0.01f)] private float characterSize = 0.05f;
        [SerializeField] private bool hideWhenEmpty = true;

        private Transform labelTransform;
        private TextMesh labelMesh;

        private void Awake()
        {
            EnsureLabel();
            RefreshLabel();
        }

        private void OnEnable()
        {
            EnsureLabel();
            RefreshLabel();
        }

        private void LateUpdate()
        {
            EnsureLabel();
            RefreshLabel();
            BillboardToCamera();
        }

        private void OnValidate()
        {
            EnsureLabel();
            RefreshLabel();
        }

        public void SetText(string value)
        {
            string sanitizedValue = value ?? string.Empty;
            if (labelText == sanitizedValue)
            {
                return;
            }

            labelText = sanitizedValue;
            RefreshLabel();
        }

        public void SetColor(Color value)
        {
            if (textColor == value)
            {
                return;
            }

            textColor = value;
            RefreshLabel();
        }

        public void SetLocalOffset(Vector3 value)
        {
            if (localOffset == value)
            {
                return;
            }

            localOffset = value;
            RefreshLabel();
        }

        public void SetCharacterSize(float value)
        {
            float sanitizedValue = Mathf.Max(0.01f, value);
            if (Mathf.Approximately(characterSize, sanitizedValue))
            {
                return;
            }

            characterSize = sanitizedValue;
            RefreshLabel();
        }

        public void SetFontSize(int value)
        {
            int sanitizedValue = Mathf.Max(1, value);
            if (fontSize == sanitizedValue)
            {
                return;
            }

            fontSize = sanitizedValue;
            RefreshLabel();
        }

        private void EnsureLabel()
        {
            if (labelTransform == null)
            {
                labelTransform = transform.Find("WorldLabel");
                if (labelTransform == null)
                {
                    GameObject labelObject = new GameObject("WorldLabel");
                    labelTransform = labelObject.transform;
                    labelTransform.SetParent(transform, false);
                    labelTransform.localRotation = Quaternion.identity;
                    labelTransform.localScale = Vector3.one;
                }
            }

            if (labelMesh == null)
            {
                labelMesh = labelTransform.GetComponent<TextMesh>();
                if (labelMesh == null)
                {
                    labelMesh = labelTransform.gameObject.AddComponent<TextMesh>();
                }
            }

            labelMesh.anchor = TextAnchor.MiddleCenter;
            labelMesh.alignment = TextAlignment.Center;
            labelMesh.fontSize = fontSize;
            labelMesh.characterSize = characterSize;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                labelMesh.font = font;
                MeshRenderer labelRenderer = labelTransform.GetComponent<MeshRenderer>();
                if (labelRenderer != null)
                {
                    labelRenderer.sharedMaterial = font.material;
                }
            }
        }

        private void RefreshLabel()
        {
            if (labelTransform == null || labelMesh == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrWhiteSpace(labelText);
            labelTransform.localPosition = localOffset;
            labelTransform.gameObject.SetActive(!hideWhenEmpty || hasText);

            if (!hasText)
            {
                labelMesh.text = string.Empty;
                return;
            }

            labelMesh.text = labelText;
            labelMesh.color = textColor;
            labelMesh.fontSize = fontSize;
            labelMesh.characterSize = characterSize;
        }

        private void BillboardToCamera()
        {
            if (labelTransform == null || !labelTransform.gameObject.activeSelf)
            {
                return;
            }

            Camera targetCamera = ResolveCamera();
            if (targetCamera == null)
            {
                return;
            }

            labelTransform.LookAt(
                labelTransform.position + targetCamera.transform.rotation * Vector3.forward,
                targetCamera.transform.rotation * Vector3.up);
        }

        private static Camera ResolveCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
            {
                return SceneView.lastActiveSceneView.camera;
            }
#endif

            return null;
        }
    }
}
