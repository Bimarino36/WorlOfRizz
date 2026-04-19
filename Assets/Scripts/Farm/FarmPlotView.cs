using IdleRestaurant.Meta;
using IdleRestaurant.Localization;
using UnityEngine;

namespace IdleRestaurant.Farm
{
    public sealed class FarmPlotView : MonoBehaviour
    {
        [SerializeField] private int plotIndex;
        [SerializeField] private Renderer soilRenderer;
        [SerializeField] private Renderer cropRenderer;
        [SerializeField] private TextMesh labelText;

        public int PlotIndex => plotIndex;

        public void Configure(int index)
        {
            plotIndex = index;
            EnsureVisuals();
            UpdateLabel(LocalizationService.Format("farm.plot_view.base", plotIndex + 1));
        }

        public void Apply(FarmPlotData plotData, float remainingSeconds)
        {
            EnsureVisuals();

            Color soilColor = new Color(0.34f, 0.26f, 0.17f, 1f);
            Color cropColor = new Color(0.45f, 0.78f, 0.32f, 1f);
            string suffix = LocalizationService.Get("farm.plot_view.empty");

            switch (plotData.State)
            {
                case FarmPlotState.Growing:
                    soilColor = new Color(0.39f, 0.3f, 0.2f, 1f);
                    cropColor = new Color(0.89f, 0.76f, 0.28f, 1f);
                    suffix = LocalizationService.Format("farm.plot_view.grow", Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds)));
                    break;
                case FarmPlotState.Ready:
                    soilColor = new Color(0.28f, 0.38f, 0.16f, 1f);
                    cropColor = new Color(0.34f, 0.92f, 0.42f, 1f);
                    suffix = LocalizationService.Format("farm.plot_view.ready", Mathf.Max(1, plotData.IngredientYield));
                    break;
            }

            if (soilRenderer != null)
            {
                soilRenderer.material.color = soilColor;
            }

            if (cropRenderer != null)
            {
                bool showCrop = plotData.State != FarmPlotState.Empty;
                cropRenderer.gameObject.SetActive(showCrop);
                if (showCrop)
                {
                    cropRenderer.material.color = cropColor;
                }
            }

            UpdateLabel(LocalizationService.Format("farm.plot_view.base", plotIndex + 1) + "\n" + suffix);
        }

        private void EnsureVisuals()
        {
            if (soilRenderer == null)
            {
                Transform soilTransform = transform.Find("Soil");
                if (soilTransform == null)
                {
                    GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    soil.name = "Soil";
                    soil.transform.SetParent(transform, false);
                    soil.transform.localPosition = Vector3.zero;
                    soil.transform.localScale = new Vector3(1.7f, 0.2f, 1.7f);
                    soilRenderer = soil.GetComponent<Renderer>();
                }
                else
                {
                    soilRenderer = soilTransform.GetComponent<Renderer>();
                }
            }

            if (cropRenderer == null)
            {
                Transform cropTransform = transform.Find("Crop");
                if (cropTransform == null)
                {
                    GameObject crop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crop.name = "Crop";
                    crop.transform.SetParent(transform, false);
                    crop.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                    crop.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                    cropRenderer = crop.GetComponent<Renderer>();
                }
                else
                {
                    cropRenderer = cropTransform.GetComponent<Renderer>();
                }
            }

            if (labelText == null)
            {
                Transform labelTransform = transform.Find("Label");
                if (labelTransform == null)
                {
                    GameObject labelObject = new GameObject("Label");
                    labelObject.transform.SetParent(transform, false);
                    labelObject.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                    labelText = labelObject.AddComponent<TextMesh>();
                }
                else
                {
                    labelText = labelTransform.GetComponent<TextMesh>();
                    if (labelText == null)
                    {
                        labelText = labelTransform.gameObject.AddComponent<TextMesh>();
                    }
                }

                labelText.fontSize = 40;
                labelText.characterSize = 0.08f;
                labelText.anchor = TextAnchor.MiddleCenter;
                labelText.alignment = TextAlignment.Center;
                labelText.color = new Color(0.97f, 0.97f, 0.97f, 1f);
            }
        }

        private void UpdateLabel(string value)
        {
            if (labelText == null)
            {
                return;
            }

            labelText.text = value;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Transform labelTransform = labelText.transform;
                labelTransform.LookAt(labelTransform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
}
