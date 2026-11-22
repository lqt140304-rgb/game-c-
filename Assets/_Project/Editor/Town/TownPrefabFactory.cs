#if UNITY_EDITOR
using System.IO;
using ChoDoLa.Town.Layout;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Town.Editor
{
    public static class TownPrefabFactory
    {
        private const string PrefabRoot = "Assets/_Project/Prefabs/Town";
        private const string MaterialRoot = "Assets/_Project/Art/Greybox/Materials";

        public static GameObject GetOrCreate(TownPieceKind kind, TownPalette palette)
        {
            string subfolder = GetSubfolder(kind);
            string folder = $"{PrefabRoot}/{subfolder}";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string prefabPath = $"{folder}/GB_{kind}_{palette}.prefab";
            Material mat = GetOrCreateMaterial(palette);

            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temp.name = $"GB_{kind}_{palette}";

            if (kind == TownPieceKind.ShopSlot ||
                kind == TownPieceKind.InnSpawn ||
                kind == TownPieceKind.OpenTableZone)
            {
                Collider col = temp.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
            }

            Renderer renderer = temp.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        private static string GetSubfolder(TownPieceKind kind)
        {
            switch (kind)
            {
                case TownPieceKind.Deck:
                case TownPieceKind.Ramp:
                case TownPieceKind.Bridge:
                    return "Verticality";
                case TownPieceKind.LandmarkTower:
                case TownPieceKind.LandmarkWell:
                case TownPieceKind.LandmarkHoloArch:
                    return "Landmarks";
                case TownPieceKind.AccentSteam:
                case TownPieceKind.AccentSciFi:
                case TownPieceKind.AccentArcane:
                    return "AccentSockets";
                case TownPieceKind.ShopSlot:
                case TownPieceKind.InnSpawn:
                case TownPieceKind.OpenTableZone:
                    return "Slots";
                default:
                    return "Architecture";
            }
        }

        private static Material GetOrCreateMaterial(TownPalette palette)
        {
            if (!Directory.Exists(MaterialRoot)) Directory.CreateDirectory(MaterialRoot);
            string matPath = $"{MaterialRoot}/GB_{palette}.mat";

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            Color color = GetColor(palette);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;

            if (palette == TownPalette.Marker)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
                if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Color GetColor(TownPalette palette)
        {
            switch (palette)
            {
                case TownPalette.Stone: return Hex("#8A8A85");
                case TownPalette.Moss: return Hex("#6E7560");
                case TownPalette.Wood: return Hex("#5A4130");
                case TownPalette.Roof: return Hex("#8C3F2E");
                case TownPalette.Brass: return Hex("#B8862B");
                case TownPalette.SciFi: return Hex("#5FA8D3");
                case TownPalette.Arcane: return Hex("#8E6CCF");
                case TownPalette.Marker:
                    Color c = Hex("#3CCB5A");
                    c.a = 0.35f;
                    return c;
                default: return Color.gray;
            }
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
#endif
