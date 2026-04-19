using UnityEngine;
using IdleRestaurant.Localization;

namespace IdleRestaurant.Gameplay
{
    public enum RestaurantStationStatusMode
    {
        Entrance = 0,
        OrderDesk = 1,
        KitchenPickup = 2,
        BarPickup = 3,
        Sink = 4,
        Cashier = 5
    }

    public sealed class RestaurantStationStatusView : MonoBehaviour
    {
        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField] private RestaurantStationStatusMode mode;
        [SerializeField] private WorldBillboardLabel worldLabel;
        [SerializeField] private Vector3 labelOffset = new Vector3(0f, 0.58f, 0f);
        [SerializeField, Min(0.02f)] private float labelCharacterSize = 0.055f;
        [SerializeField, Min(1)] private int labelFontSize = 60;
        private string lastLabelText = string.Empty;
        private Color lastLabelColor = Color.clear;

        public void Configure(RestaurantRuntime ownerRuntime, RestaurantStationStatusMode stationMode)
        {
            runtime = ownerRuntime;
            mode = stationMode;
            ResolveReferences();
            ApplyLabelLayout();
        }

        private void Awake()
        {
            ResolveReferences();
            ApplyLabelLayout();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
            ApplyLabelLayout();
        }
#endif

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveReferences();
            if (runtime == null || worldLabel == null)
            {
                return;
            }

            UpdateLabel();
        }

        private void ResolveReferences()
        {
            if (runtime == null)
            {
                runtime = GetComponentInParent<RestaurantRuntime>();
            }

            if (runtime == null)
            {
                runtime = FindAnyObjectByType<RestaurantRuntime>();
            }

            if (worldLabel == null)
            {
                worldLabel = GetComponent<WorldBillboardLabel>();
            }

            if (worldLabel == null)
            {
                worldLabel = gameObject.AddComponent<WorldBillboardLabel>();
            }
        }

        private void ApplyLabelLayout()
        {
            if (worldLabel == null)
            {
                return;
            }

            worldLabel.SetLocalOffset(labelOffset);
            worldLabel.SetCharacterSize(labelCharacterSize);
            worldLabel.SetFontSize(labelFontSize);
        }

        private void UpdateLabel()
        {
            string labelText;
            Color color;

            switch (mode)
            {
                case RestaurantStationStatusMode.Entrance:
                    BuildEntrancePresentation(out labelText, out color);
                    break;

                case RestaurantStationStatusMode.OrderDesk:
                    BuildOrderDeskPresentation(out labelText, out color);
                    break;

                case RestaurantStationStatusMode.KitchenPickup:
                    BuildKitchenPickupPresentation(out labelText, out color);
                    break;

                case RestaurantStationStatusMode.BarPickup:
                    BuildBarPickupPresentation(out labelText, out color);
                    break;

                case RestaurantStationStatusMode.Sink:
                    BuildSinkPresentation(out labelText, out color);
                    break;

                default:
                    BuildCashierPresentation(out labelText, out color);
                    break;
            }

            if (lastLabelText != labelText)
            {
                worldLabel.SetText(labelText);
                lastLabelText = labelText;
            }

            if (lastLabelColor != color)
            {
                worldLabel.SetColor(color);
                lastLabelColor = color;
            }
        }

        private void BuildEntrancePresentation(out string labelText, out Color color)
        {
            if (runtime.QueueGuestCount > 0)
            {
                labelText = LocalizationService.Format("rest.station.entrance_queue", runtime.QueueGuestCount);
                color = new Color(1f, 0.78f, 0.3f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.station.entrance_open");
            color = new Color(0.86f, 0.9f, 0.86f, 1f);
        }

        private void BuildOrderDeskPresentation(out string labelText, out Color color)
        {
            int submitCount = runtime.GetAwaitingOrderSubmissionCount();
            int waitingOrders = runtime.GetSeatCountByStatus(RestaurantSeatStatus.WaitingForOrder);

            if (submitCount > 0)
            {
                labelText = LocalizationService.Format("rest.station.order_submit", submitCount);
                color = new Color(1f, 0.62f, 0.24f, 1f);
                return;
            }

            if (waitingOrders > 0)
            {
                labelText = LocalizationService.Format("rest.station.order_new", waitingOrders);
                color = new Color(1f, 0.82f, 0.34f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.station.order_idle");
            color = new Color(0.84f, 0.88f, 0.84f, 1f);
        }

        private void BuildKitchenPickupPresentation(out string labelText, out Color color)
        {
            int readyCount = runtime.GetKitchenReadyOrderCount();
            int preparingCount = runtime.GetKitchenPreparingCount();

            if (readyCount > 0)
            {
                labelText = LocalizationService.Format("rest.station.kitchen_ready", readyCount);
                color = new Color(0.33f, 0.9f, 1f, 1f);
                return;
            }

            if (preparingCount > 0)
            {
                labelText = LocalizationService.Format("rest.station.kitchen_prep", preparingCount);
                color = new Color(1f, 0.74f, 0.31f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.station.kitchen_idle");
            color = new Color(0.86f, 0.9f, 0.9f, 1f);
        }

        private void BuildBarPickupPresentation(out string labelText, out Color color)
        {
            int readyCount = runtime.GetBarReadyOrderCount();
            int preparingCount = runtime.GetBarPreparingCount();

            if (readyCount > 0)
            {
                labelText = LocalizationService.Format("rest.station.bar_ready", readyCount);
                color = new Color(0.31f, 0.86f, 1f, 1f);
                return;
            }

            if (preparingCount > 0)
            {
                labelText = LocalizationService.Format("rest.station.bar_mix", preparingCount);
                color = new Color(1f, 0.72f, 0.34f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.station.bar_idle");
            color = new Color(0.86f, 0.9f, 0.93f, 1f);
        }

        private void BuildSinkPresentation(out string labelText, out Color color)
        {
            int dirtyTables = runtime.GetCleanupTableCount();
            if (dirtyTables > 0)
            {
                labelText = LocalizationService.Format("rest.station.sink_clear", dirtyTables);
                color = new Color(1f, 0.54f, 0.39f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.station.sink_idle");
            color = new Color(0.82f, 0.86f, 0.86f, 1f);
        }

        private void BuildCashierPresentation(out string labelText, out Color color)
        {
            int waitingBills = runtime.GetWaitingBillCount();
            if (waitingBills > 0)
            {
                labelText = LocalizationService.Format("rest.station.cashier_bills", waitingBills);
                color = new Color(0.98f, 0.86f, 0.32f, 1f);
                return;
            }

            labelText = LocalizationService.Get("rest.station.cashier_open");
            color = new Color(0.85f, 0.9f, 0.84f, 1f);
        }
    }
}
