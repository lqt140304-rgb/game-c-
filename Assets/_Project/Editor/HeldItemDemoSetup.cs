#if UNITY_EDITOR
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Adds the held-item viewmodel to the player camera and drops a few pickup items in front of
    /// the player so the hold + inspect flow can be tried in Play Mode.
    /// Run "Tools/Cho Do La/Create Sprint 0 Test Scene" first, then this menu item.
    /// </summary>
    public static class HeldItemDemoSetup
    {
        private static readonly string[] DemoItemIds = { "V01", "V02", "V03", "V07", "V08", "V10", "H1", "CP3" };

        [MenuItem("Tools/Cho Do La/Add Held Item Demo")]
        public static void AddHeldItemDemo()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[HeldItemDemo] No camera tagged MainCamera in the open scene. Create the Sprint 0 test scene first.");
                return;
            }

            HeldItemViewmodel viewmodel = cam.GetComponentInChildren<HeldItemViewmodel>();
            if (viewmodel == null)
            {
                GameObject anchor = new GameObject("HeldItemAnchor");
                anchor.transform.SetParent(cam.transform, false);
                viewmodel = anchor.AddComponent<HeldItemViewmodel>();
            }

            Transform player = cam.transform.root;
            Vector3 origin = player.position + player.forward * 2.5f;

            GameObject parent = GameObject.Find("DemoPickups");
            if (parent == null) parent = new GameObject("DemoPickups");

            for (int i = 0; i < DemoItemIds.Length; i++)
            {
                string id = DemoItemIds[i];
                if (parent.transform.Find("Pickup_" + id) != null) continue;

                Vector3 position = origin + player.right * ((i - (DemoItemIds.Length - 1) * 0.5f) * 0.45f);
                position.y = 0.9f;
                CreatePickup(parent.transform, id, position, viewmodel);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[HeldItemDemo] Added the viewmodel and demo pickups. Press Play: look at an item, [E] to pick up, [F] to inspect, [Q] to put away.");
        }

        private static void CreatePickup(Transform parent, string id, Vector3 position, HeldItemViewmodel viewmodel)
        {
            GameObject root = new GameObject("Pickup_" + id);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            GameObject visual = null;
            string path = ModelPath(id);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
            {
                visual = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
            }
            else
            {
                // The .glb could not be imported (the glTF importer package is missing, or Git LFS was not pulled).
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.SetParent(root.transform, false);
                Debug.LogWarning($"[HeldItemDemo] Could not load {path}; using a cube placeholder for {id}.");
            }

            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            HeldItemViewmodel.FitToSize(visual.transform, 0.28f, root.transform);

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                BoxCollider box = root.AddComponent<BoxCollider>();
                box.center = root.transform.InverseTransformPoint(bounds.center);
                box.size = bounds.size;
            }

            PickupInteractable pickup = root.AddComponent<PickupInteractable>();
            SerializedObject serialized = new SerializedObject(pickup);
            serialized.FindProperty("itemId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = id;
            serialized.FindProperty("viewmodel").objectReferenceValue = viewmodel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string ModelPath(string id)
        {
            const string root = "Assets/_Project/Art/Items/";
            if (id.StartsWith("V")) return root + id + ".glb";
            if (id.StartsWith("H") || id == "RD") return root + "Boxes/" + id + ".glb";
            if (id.StartsWith("CP") || id.StartsWith("TC")) return root + "Artifacts/" + id + ".glb";
            return root + "Heirlooms/" + id + ".glb";
        }
    }
}
#endif
