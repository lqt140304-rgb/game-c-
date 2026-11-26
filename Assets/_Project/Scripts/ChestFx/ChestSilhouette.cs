using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoDoLa.ChestFx
{
    public enum SilhouetteStyle
    {
        DarkTintWithRim,  // fill = tier colour darkened, rim = bright tier colour (default)
        BlackWithRim,     // pure black fill, bright rim
        DarkTintOnly,     // no rim shell
    }

    /// <summary>
    /// The "shadow" of the won item: a copy of its meshes only (no scripts, colliders or lights are copied),
    /// drawn with an unlit fill and an inverted-hull rim. The root is the pivot (centre of the item bounds);
    /// its localScale 1 means the item's largest side equals targetSize.
    /// Built by <see cref="ChestRevealSequence"/>; the game replaces it with the real item after onSilhouetteComplete.
    /// </summary>
    public class ChestSilhouette : MonoBehaviour
    {
        private Material _fill;
        private Material _rim;

        public Material FillMaterial => _fill;
        public Material RimMaterial => _rim;

        /// <summary>Size of the item's largest side before scaling (source units).</summary>
        public float NaturalSize { get; private set; }

        public static ChestSilhouette Build(GameObject source, float targetSize, SilhouetteStyle style, float rimFraction)
        {
            GameObject root = new GameObject("ItemSilhouette");
            ChestSilhouette sil = root.AddComponent<ChestSilhouette>();
            sil._fill = ChestFxFactory.CreateUnlit(Color.black, false, "Silhouette_Fill");
            sil._rim = style == SilhouetteStyle.DarkTintOnly ? null : ChestFxFactory.CreateUnlit(Color.white, true, "Silhouette_Rim");

            GameObject content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);

            bool hasBounds = false;
            Bounds bounds = new Bounds();
            List<KeyValuePair<Transform, Mesh>> parts = new List<KeyValuePair<Transform, Mesh>>();

            if (source != null)
            {
                Transform srcRoot = source.transform;
                Matrix4x4 toRoot = srcRoot.worldToLocalMatrix;

                foreach (Renderer r in source.GetComponentsInChildren<Renderer>(true))
                {
                    if (!r.enabled || !ActiveInSource(r.transform, srcRoot)) continue;
                    Mesh mesh = null;
                    if (r is MeshRenderer)
                    {
                        MeshFilter mf = r.GetComponent<MeshFilter>();
                        if (mf != null) mesh = mf.sharedMesh;
                    }
                    else if (r is SkinnedMeshRenderer smr)
                    {
                        mesh = smr.sharedMesh;
                    }
                    if (mesh == null) continue;

                    Matrix4x4 m = toRoot * r.transform.localToWorldMatrix;
                    GameObject part = new GameObject(r.name);
                    part.transform.SetParent(content.transform, false);
                    part.transform.localPosition = m.GetColumn(3);
                    part.transform.localRotation = m.rotation;
                    part.transform.localScale = m.lossyScale;
                    parts.Add(new KeyValuePair<Transform, Mesh>(part.transform, mesh));

                    Bounds mb = mesh.bounds;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 corner = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                        Vector3 p = m.MultiplyPoint3x4(corner);
                        if (!hasBounds) { bounds = new Bounds(p, Vector3.zero); hasBounds = true; }
                        else bounds.Encapsulate(p);
                    }
                }
            }

            if (parts.Count == 0)
            {
                // No usable mesh: a sphere stands in, so the effect still plays.
                GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Mesh sphere = tmp.GetComponent<MeshFilter>().sharedMesh;
                Destroy(tmp);
                GameObject part = new GameObject("Placeholder");
                part.transform.SetParent(content.transform, false);
                parts.Add(new KeyValuePair<Transform, Mesh>(part.transform, sphere));
                bounds = new Bounds(Vector3.zero, Vector3.one);
                if (source != null) Debug.LogWarning($"[ChestFx] {source.name} has no mesh renderer; the silhouette is a sphere.");
            }

            float natural = Mathf.Max(1e-4f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));
            sil.NaturalSize = natural;
            float fit = targetSize / natural;
            content.transform.localScale = Vector3.one * fit;
            content.transform.localPosition = -bounds.center * fit;

            foreach (KeyValuePair<Transform, Mesh> part in parts)
            {
                // Vật liệu null sẽ vẽ màu tím: không có ruột thì bỏ cả bóng (đã cảnh báo ở ChestShaderResolver), không có viền thì chỉ bỏ viền.
                if (sil._fill == null) continue;
                sil.AddRenderer(part.Key.gameObject, part.Value, sil._fill);
                if (sil._rim == null) continue;

                // Inverted hull: a slightly bigger copy that only shows its back faces, so it reads as an outline.
                Vector3 lossy = part.Key.localScale;
                float meshSize = Mathf.Max(1e-4f, Mathf.Max(part.Value.bounds.size.x * Mathf.Abs(lossy.x), Mathf.Max(part.Value.bounds.size.y * Mathf.Abs(lossy.y), part.Value.bounds.size.z * Mathf.Abs(lossy.z))));
                float s = Mathf.Clamp(1f + rimFraction * natural / meshSize, 1.01f, 1.5f);
                GameObject rim = new GameObject("Rim");
                rim.transform.SetParent(part.Key, false);
                rim.transform.localScale = Vector3.one * s;
                rim.transform.localPosition = part.Value.bounds.center * (1f - s);
                sil.AddRenderer(rim, part.Value, sil._rim);
            }

            return sil;
        }

        private void AddRenderer(GameObject go, Mesh mesh, Material material)
        {
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            Material[] mats = new Material[Mathf.Max(1, mesh.subMeshCount)];
            for (int i = 0; i < mats.Length; i++) mats[i] = material;
            mr.sharedMaterials = mats;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static bool ActiveInSource(Transform t, Transform root)
        {
            for (Transform c = t; c != null; c = c.parent)
            {
                if (!c.gameObject.activeSelf) return false;
                if (c == root) break;
            }
            return true;
        }

        public void SetColors(Color fill, Color rim)
        {
            ChestFxFactory.SetColor(_fill, fill);
            ChestFxFactory.SetColor(_rim, rim);
        }

        private void OnDestroy()
        {
            if (_fill != null) Destroy(_fill);
            if (_rim != null) Destroy(_rim);
        }
    }
}
