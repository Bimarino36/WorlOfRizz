using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantPresentationBootstrap : MonoBehaviour
    {
        [SerializeField] private RestaurantRuntime runtime;
        [SerializeField] private string cookPath = "KitchenZone/Cook";
        [SerializeField] private string bartenderPath = "BarZone/Barmen";
        [SerializeField] private string managerPath = "StaffZone/ManagerZone";
        [SerializeField] private string entrancePath = "QueueZone/GuestSpawnPoint";
        [SerializeField] private string cashierPath = "QueueZone/CashierPoint";
        [SerializeField] private string orderDeskPath = "KitchenZone/WaiterOrderSubmitPoint";
        [SerializeField] private string kitchenPickupPath = "KitchenZone/KitchenPickupPoint";
        [SerializeField] private string barPickupPath = "BarZone/BarPickupPoint";
        [SerializeField] private string sinkPath = "KitchenZone/GarbagePoint";

        private bool installed;

        private void Awake()
        {
            ResolveRuntime();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (installed)
            {
                return;
            }

            ResolveRuntime();
            if (runtime == null || runtime.ConfiguredTableCount <= 0)
            {
                return;
            }

            InstallPresentationViews();
            installed = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveRuntime();
        }
#endif

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

        private void InstallPresentationViews()
        {
            EnsureActorView(cookPath, RestaurantActorRole.Cook, new Vector3(0f, 0.95f, 0f));
            EnsureActorView(bartenderPath, RestaurantActorRole.Bartender, new Vector3(0f, 0.95f, 0f));
            EnsureActorView(managerPath, RestaurantActorRole.Manager, new Vector3(0f, 0.55f, 0f));

            EnsureStationView(entrancePath, RestaurantStationStatusMode.Entrance);
            EnsureStationView(cashierPath, RestaurantStationStatusMode.Cashier);
            EnsureStationView(orderDeskPath, RestaurantStationStatusMode.OrderDesk);
            EnsureStationView(kitchenPickupPath, RestaurantStationStatusMode.KitchenPickup);
            EnsureStationView(barPickupPath, RestaurantStationStatusMode.BarPickup);
            EnsureStationView(sinkPath, RestaurantStationStatusMode.Sink);

            for (int index = 0; index < runtime.Tables.Count; index++)
            {
                RestaurantTable table = runtime.Tables[index];
                if (table == null)
                {
                    continue;
                }

                RestaurantTableMessView messView = table.GetComponent<RestaurantTableMessView>();
                if (messView == null)
                {
                    messView = table.gameObject.AddComponent<RestaurantTableMessView>();
                }

                messView.Configure(table);
            }

            EnsureMetaPreviewBridge();
            EnsureMetaPreviewHud();
        }

        private void EnsureActorView(string relativePath, RestaurantActorRole role, Vector3 labelOffset)
        {
            Transform target = transform.Find(relativePath);
            if (target == null)
            {
                return;
            }

            RestaurantActorStatusView actorView = target.GetComponent<RestaurantActorStatusView>();
            if (actorView == null)
            {
                actorView = target.gameObject.AddComponent<RestaurantActorStatusView>();
            }

            actorView.Configure(runtime, role, labelOffset);
        }

        private void EnsureStationView(string relativePath, RestaurantStationStatusMode mode)
        {
            Transform target = transform.Find(relativePath);
            if (target == null)
            {
                return;
            }

            RestaurantStationStatusView stationView = target.GetComponent<RestaurantStationStatusView>();
            if (stationView == null)
            {
                stationView = target.gameObject.AddComponent<RestaurantStationStatusView>();
            }

            stationView.Configure(runtime, mode);
        }

        private void EnsureMetaPreviewBridge()
        {
            RestaurantMetaPreviewBridge previewBridge = GetComponent<RestaurantMetaPreviewBridge>();
            if (previewBridge == null)
            {
                previewBridge = gameObject.AddComponent<RestaurantMetaPreviewBridge>();
            }

            previewBridge.Configure(runtime);
        }

        private void EnsureMetaPreviewHud()
        {
            RestaurantMetaPreviewBridge previewBridge = GetComponent<RestaurantMetaPreviewBridge>();
            if (previewBridge == null)
            {
                return;
            }

            RestaurantMetaPreviewHud previewHud = GetComponent<RestaurantMetaPreviewHud>();
            if (previewHud == null)
            {
                previewHud = gameObject.AddComponent<RestaurantMetaPreviewHud>();
            }

            previewHud.Configure(runtime, previewBridge);
        }
    }
}
