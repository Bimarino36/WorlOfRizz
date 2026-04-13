using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantTableStatusView : MonoBehaviour
    {
        [SerializeField] private RestaurantTable table;
        [SerializeField] private Vector3 markerLocalPosition = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private Vector3 markerLocalScale = new Vector3(0.08f, 0.08f, 0.08f);
        [SerializeField] private Vector3 labelLocalPosition = new Vector3(0f, 0.14f, 0f);
        [SerializeField] private bool showEatingStatus = true;

        private Transform statusRoot;
        private Renderer markerRenderer;
        private Transform labelTransform;
        private TextMesh labelMesh;

        private void Awake()
        {
            if (table == null)
            {
                table = GetComponent<RestaurantTable>();
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (table == null)
            {
                table = GetComponent<RestaurantTable>();
                if (table == null)
                {
                    return;
                }
            }

            EnsureVisuals();
            if (statusRoot == null)
            {
                return;
            }

            RestaurantSeatStatus status = table.GetDisplayStatus();
            if (ShouldHide(status))
            {
                statusRoot.gameObject.SetActive(false);
                return;
            }

            statusRoot.gameObject.SetActive(true);
            statusRoot.localPosition = markerLocalPosition;
            statusRoot.localRotation = Quaternion.identity;
            statusRoot.localScale = Vector3.one;

            float urgency = table.GetUrgencyNormalized();
            Color color = GetStatusColor(status, urgency);
            if (markerRenderer != null)
            {
                markerRenderer.material.color = color;
            }

            if (labelTransform != null)
            {
                labelTransform.localPosition = labelLocalPosition;
                labelTransform.localRotation = Quaternion.identity;
            }

            if (labelMesh != null)
            {
                labelMesh.color = color;
                labelMesh.text = GetStatusLabel(status, urgency);
            }

            Camera cameraToFace = Camera.main;
            if (cameraToFace != null && labelTransform != null)
            {
                labelTransform.LookAt(
                    labelTransform.position + cameraToFace.transform.rotation * Vector3.forward,
                    cameraToFace.transform.rotation * Vector3.up);
            }
        }

        private void OnDestroy()
        {
            if (statusRoot != null)
            {
                Destroy(statusRoot.gameObject);
            }
        }

        private void EnsureVisuals()
        {
            if (statusRoot != null && markerRenderer != null && labelTransform != null && labelMesh != null)
            {
                return;
            }

            statusRoot = transform.Find("MarkerPoint");
            if (statusRoot == null)
            {
                GameObject rootObject = new GameObject("MarkerPoint");
                statusRoot = rootObject.transform;
                statusRoot.SetParent(transform, false);
            }

            statusRoot.localPosition = markerLocalPosition;
            statusRoot.localRotation = Quaternion.identity;
            statusRoot.localScale = Vector3.one;

            Transform cubeTransform = statusRoot.Find("Cube");
            if (cubeTransform == null)
            {
                GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                markerObject.name = "Cube";
                markerObject.transform.SetParent(statusRoot, false);
                cubeTransform = markerObject.transform;
            }

            cubeTransform.localPosition = Vector3.zero;
            cubeTransform.localRotation = Quaternion.identity;
            cubeTransform.localScale = markerLocalScale;

            Collider markerCollider = cubeTransform.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            markerRenderer = cubeTransform.GetComponent<Renderer>();

            labelTransform = statusRoot.Find("Label");
            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject("Label");
                labelTransform = labelObject.transform;
                labelTransform.SetParent(statusRoot, false);
            }

            labelTransform.localPosition = labelLocalPosition;
            labelTransform.localRotation = Quaternion.identity;
            labelTransform.localScale = Vector3.one;

            labelMesh = labelTransform.GetComponent<TextMesh>();
            if (labelMesh == null)
            {
                labelMesh = labelTransform.gameObject.AddComponent<TextMesh>();
            }

            labelMesh.fontSize = 48;
            labelMesh.characterSize = 0.05f;
            labelMesh.anchor = TextAnchor.MiddleCenter;
            labelMesh.alignment = TextAlignment.Center;

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

        private bool ShouldHide(RestaurantSeatStatus status)
        {
            if (status == RestaurantSeatStatus.Available || status == RestaurantSeatStatus.GuestLeaving)
            {
                return true;
            }

            if (!showEatingStatus && status == RestaurantSeatStatus.Eating)
            {
                return true;
            }

            return false;
        }

        private static Color GetStatusColor(RestaurantSeatStatus status, float urgency)
        {
            Color baseColor;

            switch (status)
            {
                case RestaurantSeatStatus.Reserved:
                    baseColor = new Color(0.4f, 0.75f, 1f);
                    break;
                case RestaurantSeatStatus.WaitingForOrder:
                    baseColor = new Color(1f, 0.85f, 0.2f);
                    break;
                case RestaurantSeatStatus.AwaitingOrderSubmission:
                case RestaurantSeatStatus.AwaitingPickup:
                    baseColor = new Color(1f, 0.55f, 0.2f);
                    break;
                case RestaurantSeatStatus.AwaitingDelivery:
                    baseColor = new Color(0.3f, 0.9f, 1f);
                    break;
                case RestaurantSeatStatus.Eating:
                    baseColor = new Color(0.35f, 1f, 0.45f);
                    break;
                case RestaurantSeatStatus.NeedsCleanup:
                    baseColor = new Color(0.75f, 0.75f, 0.75f);
                    break;
                case RestaurantSeatStatus.WaitingBill:
                    baseColor = new Color(0.95f, 0.95f, 0.3f);
                    break;
                default:
                    baseColor = Color.white;
                    break;
            }

            Color urgentColor = new Color(1f, 0.22f, 0.22f);
            return Color.Lerp(baseColor, urgentColor, Mathf.Clamp01(urgency));
        }

        private static string GetStatusLabel(RestaurantSeatStatus status, float urgency)
        {
            string baseLabel;

            switch (status)
            {
                case RestaurantSeatStatus.Reserved:
                    baseLabel = "Seating";
                    break;
                case RestaurantSeatStatus.WaitingForOrder:
                    baseLabel = "Order";
                    break;
                case RestaurantSeatStatus.AwaitingOrderSubmission:
                case RestaurantSeatStatus.AwaitingPickup:
                    baseLabel = "Prep";
                    break;
                case RestaurantSeatStatus.AwaitingDelivery:
                    baseLabel = "Serve";
                    break;
                case RestaurantSeatStatus.Eating:
                    baseLabel = "Eating";
                    break;
                case RestaurantSeatStatus.NeedsCleanup:
                    baseLabel = "Clean";
                    break;
                case RestaurantSeatStatus.WaitingBill:
                    baseLabel = "Bill";
                    break;
                default:
                    baseLabel = string.Empty;
                    break;
            }

            if (string.IsNullOrEmpty(baseLabel))
            {
                return baseLabel;
            }

            if (urgency >= 0.8f)
            {
                return baseLabel + " !!";
            }

            if (urgency >= 0.5f)
            {
                return baseLabel + " !";
            }

            return baseLabel;
        }
    }
}
