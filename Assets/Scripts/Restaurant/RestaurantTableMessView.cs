using System.Collections.Generic;
using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public sealed class RestaurantTableMessView : MonoBehaviour
    {
        [SerializeField] private RestaurantTable table;
        [SerializeField] private Color messColor = new Color(0.55f, 0.4f, 0.26f, 1f);

        private Transform messRoot;
        private readonly List<Renderer> messRenderers = new List<Renderer>();

        public void Configure(RestaurantTable ownerTable)
        {
            table = ownerTable;
            EnsureVisuals();
        }

        private void Awake()
        {
            ResolveTable();
            EnsureVisuals();
        }

        private void LateUpdate()
        {
            ResolveTable();
            EnsureVisuals();

            if (table == null || messRoot == null)
            {
                return;
            }

            bool showMess = Application.isPlaying && table.GetDisplayStatus() == RestaurantSeatStatus.NeedsCleanup;
            messRoot.gameObject.SetActive(showMess);
            if (showMess)
            {
                UpdateMessPlacement();
            }
        }

        private void ResolveTable()
        {
            if (table == null)
            {
                table = GetComponent<RestaurantTable>();
            }
        }

        private void EnsureVisuals()
        {
            if (messRoot == null)
            {
                messRoot = transform.Find("MessVisual");
                if (messRoot == null)
                {
                    GameObject messObject = new GameObject("MessVisual");
                    messRoot = messObject.transform;
                    messRoot.SetParent(transform, false);
                }
            }

            if (messRenderers.Count == 0)
            {
                CreateMessPiece("Mess_01", new Vector3(-0.08f, 0f, -0.02f), new Vector3(0.12f, 0.02f, 0.1f));
                CreateMessPiece("Mess_02", new Vector3(0.06f, 0.01f, 0.04f), new Vector3(0.09f, 0.02f, 0.09f));
                CreateMessPiece("Mess_03", new Vector3(0.01f, 0.015f, -0.07f), new Vector3(0.06f, 0.015f, 0.06f));
            }

            for (int index = 0; index < messRenderers.Count; index++)
            {
                Renderer pieceRenderer = messRenderers[index];
                if (pieceRenderer != null)
                {
                    pieceRenderer.material.color = messColor;
                }
            }

            UpdateMessPlacement();
            messRoot.gameObject.SetActive(false);
        }

        private void CreateMessPiece(string pieceName, Vector3 localPosition, Vector3 localScale)
        {
            Transform existingPiece = messRoot.Find(pieceName);
            if (existingPiece == null)
            {
                GameObject pieceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pieceObject.name = pieceName;
                pieceObject.transform.SetParent(messRoot, false);
                existingPiece = pieceObject.transform;
            }

            existingPiece.localPosition = localPosition;
            existingPiece.localRotation = Quaternion.identity;
            existingPiece.localScale = localScale;

            Collider pieceCollider = existingPiece.GetComponent<Collider>();
            if (pieceCollider != null)
            {
                Destroy(pieceCollider);
            }

            Renderer pieceRenderer = existingPiece.GetComponent<Renderer>();
            if (pieceRenderer != null && !messRenderers.Contains(pieceRenderer))
            {
                messRenderers.Add(pieceRenderer);
            }
        }

        private void UpdateMessPlacement()
        {
            if (messRoot == null)
            {
                return;
            }

            Bounds bounds = GetSurfaceBounds();
            Vector3 surfaceCenter = bounds.center;
            surfaceCenter.y = bounds.min.y + bounds.size.y * 0.58f;

            messRoot.position = surfaceCenter;
            messRoot.rotation = transform.rotation;

            float spread = Mathf.Clamp(Mathf.Min(bounds.size.x, bounds.size.z) * 0.22f, 0.08f, 0.18f);
            SetPieceTransform("Mess_01", new Vector3(-spread, 0f, -spread * 0.25f));
            SetPieceTransform("Mess_02", new Vector3(spread * 0.65f, 0.008f, spread * 0.35f));
            SetPieceTransform("Mess_03", new Vector3(spread * 0.1f, 0.015f, -spread * 0.72f));
        }

        private void SetPieceTransform(string pieceName, Vector3 localPosition)
        {
            Transform piece = messRoot.Find(pieceName);
            if (piece == null)
            {
                return;
            }

            piece.localPosition = localPosition;
        }

        private Bounds GetSurfaceBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            Renderer bestRenderer = null;

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || renderer.transform == null || renderer.transform.IsChildOf(messRoot))
                {
                    continue;
                }

                string lowerName = renderer.transform.name.ToLowerInvariant();
                if (lowerName.Contains("chair") || lowerName.Contains("marker") || lowerName.Contains("label"))
                {
                    continue;
                }

                if (bestRenderer == null)
                {
                    bestRenderer = renderer;
                    continue;
                }

                if (renderer.bounds.center.y > bestRenderer.bounds.center.y)
                {
                    bestRenderer = renderer;
                }
            }

            if (bestRenderer != null)
            {
                return bestRenderer.bounds;
            }

            return new Bounds(transform.position + Vector3.up * 0.7f, new Vector3(0.8f, 0.2f, 0.8f));
        }
    }
}
