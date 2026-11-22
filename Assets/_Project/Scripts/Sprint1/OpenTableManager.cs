using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChoDoLa.Contracts;
using ChoDoLa.Interaction;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Simple interactable for an already-opened artifact resting on the velvet mat
    /// if the player exits inspection without selling or keeping it yet.
    /// </summary>
    public class TableArtifactProp : MonoBehaviour, IInteractable
    {
        private ArtifactInstanceView _artifact;
        private ArtifactInspector3D _inspector;

        public string ArtifactInstanceId => _artifact?.InstanceId;
        public string PromptText => _artifact != null ? $"Soi lại {_artifact.DisplayName}" : string.Empty;
        public bool CanInteract => _artifact != null && !_artifact.IsSold && !_artifact.IsKeptInInventory;

        public void Initialize(ArtifactInstanceView artifact, ArtifactInspector3D inspector)
        {
            _artifact = artifact;
            _inspector = inspector;
        }

        public void Interact()
        {
            if (!CanInteract || _inspector == null) return;
            ArtifactInstanceView target = _artifact;
            Destroy(gameObject);
            _inspector.BeginInspect(target);
        }
    }

    /// <summary>
    /// Coordinates spawned blind boxes on the Opening Table and transitions opened boxes into 3D inspection.
    /// </summary>
    public class OpenTableManager : MonoBehaviour
    {
        [SerializeField] private Sprint1MarketService marketService;
        [SerializeField] private ArtifactInspector3D inspector3D;
        [SerializeField] private Sprint1HudController hudController;
        [SerializeField] private Transform[] boxSlots;
        [SerializeField] private Transform artifactRestAnchor;

        private readonly Dictionary<string, BlindBoxProp> _spawnedBoxes = new Dictionary<string, BlindBoxProp>();
        private TableArtifactProp _restingArtifactProp;

        public void Configure(
            Sprint1MarketService service,
            ArtifactInspector3D inspector,
            Sprint1HudController hud,
            Transform[] slots,
            Transform restAnchor)
        {
            marketService = service;
            inspector3D = inspector;
            hudController = hud;
            boxSlots = slots;
            artifactRestAnchor = restAnchor;
        }

        private void OnEnable()
        {
            if (marketService == null) return;
            marketService.OnBoxPurchased += HandleBoxPurchased;
            marketService.OnBoxOpened += HandleBoxOpened;
            marketService.OnInventoryChanged += HandleInventoryChanged;
            marketService.OnRunStateChanged += HandleRunStateChanged;
        }

        private void OnDisable()
        {
            if (marketService == null) return;
            marketService.OnBoxPurchased -= HandleBoxPurchased;
            marketService.OnBoxOpened -= HandleBoxOpened;
            marketService.OnInventoryChanged -= HandleInventoryChanged;
            marketService.OnRunStateChanged -= HandleRunStateChanged;
        }

        public void PlaceArtifactOnMat(ArtifactInstanceView artifact)
        {
            ClearRestingArtifact();
            if (artifact == null || artifact.IsSold || artifact.IsKeptInInventory || artifactRestAnchor == null) return;

            GameObject holder = new GameObject($"Resting_{artifact.ItemId}");
            holder.transform.SetParent(artifactRestAnchor, false);
            holder.transform.localPosition = Vector3.zero;

            MarketVisualFactory.CreateArtifactModel(holder.transform, artifact.VisualPrefabKey, artifact.Quality, artifact.Authenticity);

            BoxCollider col = holder.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.11f, 0f);
            col.size = new Vector3(0.24f, 0.24f, 0.24f);

            _restingArtifactProp = holder.AddComponent<TableArtifactProp>();
            _restingArtifactProp.Initialize(artifact, inspector3D);
        }

        private void HandleBoxPurchased(BoxInstanceView box)
        {
            if (box == null || boxSlots == null || boxSlots.Length == 0) return;

            Transform targetSlot = boxSlots[0];
            foreach (Transform slot in boxSlots)
            {
                if (slot != null && slot.childCount == 0)
                {
                    targetSlot = slot;
                    break;
                }
            }

            GameObject boxGo = new GameObject($"PurchasedBox_{box.BoxInstanceId}");
            boxGo.transform.SetParent(targetSlot, false);
            boxGo.transform.localPosition = Vector3.zero;

            MarketVisualFactory.CreateBoxModel(boxGo.transform, box.VisualVariantKey, out Transform lidPivot, out Transform sealPart);

            BoxCollider col = boxGo.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.12f, 0f);
            col.size = new Vector3(0.38f, 0.26f, 0.30f);

            BlindBoxProp prop = boxGo.AddComponent<BlindBoxProp>();
            prop.InitializePurchasedBox(box, marketService, hudController, lidPivot, sealPart);
            _spawnedBoxes[box.BoxInstanceId] = prop;
        }

        private void HandleBoxOpened(BoxInstanceView box, ArtifactInstanceView artifact)
        {
            StartCoroutine(OpenAndInspectRoutine(box, artifact));
        }

        private IEnumerator OpenAndInspectRoutine(BoxInstanceView box, ArtifactInstanceView artifact)
        {
            if (_spawnedBoxes.TryGetValue(box.BoxInstanceId, out BlindBoxProp prop) && prop != null)
            {
                yield return prop.PlayUnboxAnimation();
                yield return new WaitForSeconds(0.22f);
                Destroy(prop.gameObject);
                _spawnedBoxes.Remove(box.BoxInstanceId);
            }

            ClearRestingArtifact();
            if (inspector3D != null)
            {
                inspector3D.BeginInspect(artifact);
            }
        }

        private void HandleInventoryChanged()
        {
            if (_restingArtifactProp == null) return;
            if (!_restingArtifactProp.CanInteract)
            {
                ClearRestingArtifact();
            }
        }

        private void HandleRunStateChanged(RunStateSnapshot snapshot)
        {
            if (snapshot != null && snapshot.CurrentDay == 1 && snapshot.DailyBoxesPurchased == 0 && marketService.UnopenedBoxes.Count == 0)
            {
                // Clean table on fresh run reset
                foreach (var kv in _spawnedBoxes)
                {
                    if (kv.Value != null) Destroy(kv.Value.gameObject);
                }
                _spawnedBoxes.Clear();
                ClearRestingArtifact();
            }
        }

        private void ClearRestingArtifact()
        {
            if (_restingArtifactProp != null)
            {
                Destroy(_restingArtifactProp.gameObject);
                _restingArtifactProp = null;
            }
        }
    }
}
