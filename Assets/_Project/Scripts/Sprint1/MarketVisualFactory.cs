using System.Collections.Generic;
using UnityEngine;
using ChoDoLa.Contracts;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Procedural stylized 3D model builder for Sprint 1 stations, blind boxes, and inspectable artifacts.
    /// Produces multi-part URP visuals with emissive accents and hinged box lids.
    /// </summary>
    public static class MarketVisualFactory
    {
        private static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

        public static Material GetMaterial(string key, Color albedo, float metallic = 0.08f, float smoothness = 0.45f, Color? emission = null)
        {
            if (MaterialCache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader) { name = $"M_Sprint1_{key}" };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedo);
            else mat.color = albedo;

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
            }

            MaterialCache[key] = mat;
            return mat;
        }

        private static Material MatWoodLight => GetMaterial("WoodLight", new Color(0.46f, 0.31f, 0.19f), 0.02f, 0.32f);
        private static Material MatWoodDark => GetMaterial("WoodDark", new Color(0.24f, 0.15f, 0.09f), 0.04f, 0.38f);
        private static Material MatBrass => GetMaterial("Brass", new Color(0.78f, 0.57f, 0.20f), 0.85f, 0.72f);
        private static Material MatGold => GetMaterial("Gold", new Color(0.92f, 0.75f, 0.25f), 0.92f, 0.82f);
        private static Material MatIron => GetMaterial("Iron", new Color(0.22f, 0.24f, 0.27f), 0.80f, 0.55f);
        private static Material MatSlate => GetMaterial("Slate", new Color(0.16f, 0.16f, 0.20f), 0.12f, 0.35f);
        private static Material MatVelvet => GetMaterial("Velvet", new Color(0.45f, 0.07f, 0.10f), 0.02f, 0.15f);
        private static Material MatWaxRed => GetMaterial("WaxRed", new Color(0.68f, 0.10f, 0.12f), 0.05f, 0.60f);
        private static Material MatParchment => GetMaterial("Parchment", new Color(0.86f, 0.79f, 0.64f), 0.0f, 0.22f);
        private static Material MatDialWhite => GetMaterial("DialWhite", new Color(0.94f, 0.91f, 0.84f), 0.02f, 0.65f);
        private static Material MatJade => GetMaterial("Jade", new Color(0.14f, 0.62f, 0.42f), 0.08f, 0.78f);
        private static Material MatAmber => GetMaterial("Amber", new Color(0.85f, 0.48f, 0.08f), 0.05f, 0.88f, new Color(0.35f, 0.16f, 0.02f));
        private static Material MatObsidian => GetMaterial("Obsidian", new Color(0.08f, 0.07f, 0.12f), 0.30f, 0.90f);
        private static Material MatCyanGlow => GetMaterial("CyanGlow", new Color(0.25f, 0.82f, 0.98f), 0.20f, 0.85f, new Color(0.2f, 1.3f, 1.6f));
        private static Material MatArcaneGlow => GetMaterial("ArcaneGlow", new Color(0.58f, 0.32f, 0.95f), 0.20f, 0.75f, new Color(0.75f, 0.25f, 1.4f));
        private static Material MatWarmGlow => GetMaterial("WarmGlow", new Color(1.0f, 0.80f, 0.44f), 0.0f, 0.4f, new Color(0.95f, 0.65f, 0.25f));
        private static Material MatCrack => GetMaterial("Crack", new Color(0.06f, 0.05f, 0.05f), 0.0f, 0.1f);

        public static Material GetTexturedMaterial(
            string key,
            string albedoRes,
            string normalRes,
            Color tint,
            float metallic = 0.0f,
            float smoothness = 0.28f,
            float bumpScale = 0.85f,
            Vector2? tiling = null)
        {
            if (MaterialCache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Material mat = GetMaterial(key, tint, metallic, smoothness);
            Texture2D albedo = string.IsNullOrEmpty(albedoRes) ? null : Resources.Load<Texture2D>($"Blender/Textures/{albedoRes}");
            Texture2D normal = string.IsNullOrEmpty(normalRes) ? null : Resources.Load<Texture2D>($"Blender/Textures/{normalRes}");

            if (albedo != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
                else mat.mainTexture = albedo;
                if (tiling.HasValue && mat.HasProperty("_BaseMap_ST"))
                {
                    mat.SetTextureScale("_BaseMap", tiling.Value);
                }
                else if (tiling.HasValue)
                {
                    mat.mainTextureScale = tiling.Value;
                }
            }

            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normal);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", bumpScale);
            }

            return mat;
        }

        public static Material GetCobbleMaterial(Vector2? tiling = null)
        {
            string key = tiling.HasValue ? $"Cobble_PBR_{tiling.Value.x}_{tiling.Value.y}" : "Cobble_PBR";
            return GetTexturedMaterial(key, "T_Cobble_Albedo", "T_Cobble_Normal", Color.white, 0.0f, 0.26f, 0.9f, tiling);
        }

        public static Material GetHanoiPavingMaterial(Vector2? tiling = null)
        {
            string key = tiling.HasValue ? $"VnStreetMatte_PBR_{tiling.Value.x}_{tiling.Value.y}" : "VnStreetMatte_PBR";
            return GetTexturedMaterial(key, "T_VnStreetMatte_Albedo", "T_VnStreetMatte_Normal", Color.white, 0.0f, 0.14f, 0.55f, tiling);
        }

        private static Material GetSteamMistMaterial()
        {
            const string key = "Blender_SteamMist";
            if (MaterialCache.TryGetValue(key, out Material cached) && cached != null) return cached;

            Material mat = GetMaterial(key, new Color(0.96f, 0.98f, 1.0f, 0.38f), 0.0f, 0.15f, new Color(0.32f, 0.35f, 0.38f));
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            return mat;
        }

        private static Material GetGlassMaterial()
        {
            const string key = "Blender_Glass";
            if (MaterialCache.TryGetValue(key, out Material cached) && cached != null) return cached;

            Material mat = GetMaterial(key, new Color(0.72f, 0.86f, 0.94f, 0.42f), 0.12f, 0.92f);
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            return mat;
        }

        private static Material ResolveBlenderMaterial(string blenderMatName)
        {
            string n = string.IsNullOrEmpty(blenderMatName)
                ? string.Empty
                : blenderMatName.Replace(" (Instance)", string.Empty).Trim();

            switch (n)
            {
                case "Mat_Cobble_PBR":
                    return GetCobbleMaterial();
                case "Mat_HanoiTerrazzo":
                    return GetTexturedMaterial("B_HanoiTerrazzo", "T_HanoiTerrazzo_Albedo", "T_HanoiTerrazzo_Normal", Color.white, 0.0f, 0.14f, 0.65f);
                case "Mat_VnRedTile":
                    return GetTexturedMaterial("B_VnRedTile", "T_VnRedTile_Albedo", "T_VnRedTile_Normal", Color.white, 0.0f, 0.16f, 0.65f);
                case "Mat_VnStreetMatte":
                    return GetTexturedMaterial("B_VnStreetMatte", "T_VnStreetMatte_Albedo", "T_VnStreetMatte_Normal", Color.white, 0.0f, 0.14f, 0.55f);
                case "Mat_HanoiCreamWall":
                    return GetMaterial("B_HanoiCreamWall", new Color(0.88f, 0.85f, 0.79f), 0.0f, 0.12f);
                case "Mat_HanoiYellowWall":
                    return GetMaterial("B_HanoiYellowWall", new Color(0.86f, 0.73f, 0.42f), 0.0f, 0.12f);
                case "Mat_SteelBlack":
                    return GetMaterial("B_SteelBlack", new Color(0.12f, 0.13f, 0.15f), 0.68f, 0.45f);
                case "Mat_InoxSteel":
                    return GetMaterial("B_InoxSteel", new Color(0.75f, 0.78f, 0.81f), 0.88f, 0.70f);
                case "Mat_PlasticBlue":
                    return GetMaterial("B_PlasticBlue", new Color(0.10f, 0.36f, 0.80f), 0.0f, 0.42f);
                case "Mat_PlasticRed":
                    return GetMaterial("B_PlasticRed", new Color(0.78f, 0.16f, 0.15f), 0.0f, 0.42f);
                case "Mat_HanoiRedSign":
                    return GetMaterial("B_HanoiRedSign", new Color(0.68f, 0.10f, 0.10f), 0.02f, 0.32f, new Color(0.24f, 0.03f, 0.03f));
                case "Mat_HanoiBlueSign":
                    return GetMaterial("B_HanoiBlueSign", new Color(0.10f, 0.24f, 0.56f), 0.02f, 0.32f, new Color(0.03f, 0.08f, 0.22f));
                case "Mat_TreeBark":
                    return GetTexturedMaterial("B_TreeBark", "T_AgedWood_Albedo", "T_AgedWood_Normal", new Color(0.46f, 0.36f, 0.28f), 0.0f, 0.10f, 0.90f);
                case "Mat_TreeFoliage":
                    return GetMaterial("B_TreeFoliage", new Color(0.15f, 0.38f, 0.11f), 0.0f, 0.12f);
                case "Mat_SteamMist":
                    return GetSteamMistMaterial();
                case "Mat_WeatheredPlaster":
                    return GetTexturedMaterial("B_Plaster", "T_WeatheredPlaster_Albedo", "T_WeatheredPlaster_Normal", Color.white, 0.0f, 0.14f, 0.65f);
                case "Mat_PlasterAlt":
                    return GetTexturedMaterial("B_PlasterAlt", "T_WeatheredPlaster_Albedo", "T_WeatheredPlaster_Normal", new Color(0.84f, 0.80f, 0.74f), 0.0f, 0.14f, 0.65f);
                case "Mat_ExposedBrick":
                    return GetTexturedMaterial("B_ExposedBrick", "T_ExposedBrick_Albedo", "T_ExposedBrick_Normal", Color.white, 0.0f, 0.16f, 0.90f);
                case "Mat_GlazedBrick_Teal":
                    return GetTexturedMaterial("B_GlazedBrickTeal", "T_GlazedBrickTeal_Albedo", "T_GlazedBrickTeal_Normal", Color.white, 0.05f, 0.58f, 0.75f);
                case "Mat_WhitewashedBrick":
                    return GetTexturedMaterial("B_WhitewashBrick", "T_WhitewashedBrick_Albedo", "T_WhitewashedBrick_Normal", Color.white, 0.0f, 0.15f, 0.70f);
                case "Mat_AshlarStone":
                    return GetTexturedMaterial("B_AshlarStone", "T_AshlarStone_Albedo", "T_AshlarStone_Normal", Color.white, 0.0f, 0.16f, 0.80f);
                case "Mat_Stone":
                    return GetTexturedMaterial("B_Stone", "T_AshlarStone_Albedo", "T_AshlarStone_Normal", new Color(0.90f, 0.88f, 0.84f), 0.0f, 0.14f, 0.75f);
                case "Mat_StoneDark":
                    return GetTexturedMaterial("B_StoneDark", "T_AshlarStone_Albedo", "T_AshlarStone_Normal", new Color(0.54f, 0.52f, 0.48f), 0.0f, 0.16f, 0.75f);
                case "Mat_WetStone":
                    return GetTexturedMaterial("B_WetStone", "T_Cobble_Albedo", "T_Cobble_Normal", new Color(0.55f, 0.57f, 0.56f), 0.02f, 0.45f, 0.80f);
                case "Mat_AgedWood_Floor":
                    return GetTexturedMaterial("B_AgedWoodFloor", "T_AgedWood_Albedo", "T_AgedWood_Normal", Color.white, 0.02f, 0.22f, 0.75f);
                case "Mat_WoodLight_PBR":
                    return GetTexturedMaterial("B_WoodLight", "T_AgedWood_Albedo", "T_AgedWood_Normal", new Color(0.96f, 0.90f, 0.82f), 0.02f, 0.22f, 0.55f);
                case "Mat_WoodDark_PBR":
                    return GetTexturedMaterial("B_WoodDark", "T_AgedWood_Albedo", "T_AgedWood_Normal", new Color(0.54f, 0.41f, 0.33f), 0.02f, 0.22f, 0.55f);
                case "Mat_WorkshopSlab_Floor":
                    return GetTexturedMaterial("B_WorkshopFloor", "T_WorkshopFloor_Albedo", "T_WorkshopFloor_Normal", Color.white, 0.02f, 0.24f, 0.80f);
                case "Mat_RoofTile":
                    return GetTexturedMaterial("B_RoofTile", "T_RoofTile_Albedo", null, Color.white, 0.0f, 0.18f);
                case "Mat_RoofSlate":
                    return GetTexturedMaterial("B_RoofSlate", "T_RoofSlate_Albedo", null, Color.white, 0.02f, 0.25f);
                case "Mat_MossClump":
                    return GetMaterial("B_Moss", new Color(0.16f, 0.28f, 0.09f), 0.0f, 0.10f);
                case "Mat_Brass":
                    return MatBrass;
                case "Mat_Gold":
                    return MatGold;
                case "Mat_SignGold":
                    return GetMaterial("B_SignGold", new Color(0.95f, 0.79f, 0.30f), 0.35f, 0.45f, new Color(0.32f, 0.25f, 0.08f));
                case "Mat_SignSub":
                    return GetMaterial("B_SignSub", new Color(0.92f, 0.88f, 0.78f), 0.0f, 0.22f, new Color(0.18f, 0.16f, 0.12f));
                case "Mat_Iron":
                    return MatIron;
                case "Mat_Velvet":
                    return MatVelvet;
                case "Mat_CarpetGold":
                    return GetMaterial("B_CarpetGold", new Color(0.74f, 0.54f, 0.18f), 0.05f, 0.22f);
                case "Mat_WaxRed":
                    return MatWaxRed;
                case "Mat_DialWhite":
                    return MatDialWhite;
                case "Mat_Parchment":
                    return MatParchment;
                case "Mat_Blueprint":
                    return GetMaterial("B_Blueprint", new Color(0.12f, 0.26f, 0.48f), 0.0f, 0.25f);
                case "Mat_Jade":
                    return MatJade;
                case "Mat_Amber":
                    return MatAmber;
                case "Mat_Obsidian":
                    return MatObsidian;
                case "Mat_Water":
                    return GetMaterial("B_Water", new Color(0.10f, 0.62f, 0.70f), 0.10f, 0.94f, new Color(0.08f, 0.38f, 0.46f));
                case "Mat_CyanGlow":
                    return MatCyanGlow;
                case "Mat_ArcaneGlow":
                    return MatArcaneGlow;
                case "Mat_WarmGlow":
                    return MatWarmGlow;
                case "Mat_Glass":
                    return GetGlassMaterial();
                default:
                    return null;
            }
        }

        public static void ApplyBlenderMaterials(GameObject root)
        {
            if (root == null) return;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    Material mapped = ResolveBlenderMaterial(mats[i].name);
                    if (mapped != null)
                    {
                        mats[i] = mapped;
                        changed = true;
                    }
                }
                if (changed) r.sharedMaterials = mats;
            }
        }

        public static GameObject InstantiateBlenderModel(string modelName, Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>($"Blender/Models/{modelName}");
            if (prefab == null) return null;

            GameObject instance = Object.Instantiate(prefab, parent, false);
            instance.name = modelName;
            ApplyBlenderMaterials(instance);
            return instance;
        }

        private static Transform FindChildRecursive(Transform root, string targetName)
        {
            if (root == null) return null;
            if (root.name == targetName) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), targetName);
                if (found != null) return found;
            }
            return null;
        }

        public static GameObject CreatePrimitive(PrimitiveType type, Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, Vector3? localEuler = null, bool removeCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localEuler.HasValue ? Quaternion.Euler(localEuler.Value) : Quaternion.identity;
            go.transform.localScale = localScale;

            Renderer r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;

            if (removeCollider)
            {
                Collider col = go.GetComponent<Collider>();
                if (col != null)
                {
                    if (Application.isPlaying) Object.Destroy(col);
                    else Object.DestroyImmediate(col);
                }
            }
            return go;
        }

        public static void CreateBoxModel(Transform parent, string visualVariantKey, out Transform lidPivot, out Transform sealPart)
        {
            string fbxName = visualVariantKey switch
            {
                "WoodCrate_Old" => "SM_Box_WoodCrate",
                "BrassChest_Steam" => "SM_Box_BrassChest",
                "RunicBox_Arcane" => "SM_Box_RunicArcane",
                "HoloPod_SciFi" => "SM_Box_HoloSciFi",
                _ => null
            };

            if (!string.IsNullOrEmpty(fbxName))
            {
                GameObject fbx = InstantiateBlenderModel(fbxName, parent);
                if (fbx != null)
                {
                    lidPivot = FindChildRecursive(fbx.transform, "LidPivot");
                    sealPart = FindChildRecursive(fbx.transform, "SealPart");
                    return;
                }
            }

            GameObject root = new GameObject($"BoxModel_{visualVariantKey}");
            root.transform.SetParent(parent, false);

            Material bodyMat = MatWoodLight;
            Material trimMat = MatWoodDark;
            Material accentMat = MatWaxRed;

            if (visualVariantKey == "BrassChest_Steam")
            {
                bodyMat = MatWoodDark;
                trimMat = MatBrass;
                accentMat = MatGold;
            }
            else if (visualVariantKey == "RunicBox_Arcane")
            {
                bodyMat = MatSlate;
                trimMat = MatGold;
                accentMat = MatArcaneGlow;
            }
            else if (visualVariantKey == "HoloPod_SciFi")
            {
                bodyMat = MatIron;
                trimMat = MatSlate;
                accentMat = MatCyanGlow;
            }

            // Base body (0.34 x 0.20 x 0.26)
            CreatePrimitive(PrimitiveType.Cube, root.transform, "Body", new Vector3(0f, 0.10f, 0f), new Vector3(0.34f, 0.18f, 0.26f), bodyMat);

            // Corner vertical trims
            float cx = 0.165f;
            float cz = 0.125f;
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    CreatePrimitive(PrimitiveType.Cube, root.transform, "CornerTrim", new Vector3(sx * cx, 0.10f, sz * cz), new Vector3(0.025f, 0.19f, 0.025f), trimMat);
                }
            }

            // Bottom rim
            CreatePrimitive(PrimitiveType.Cube, root.transform, "BaseRim", new Vector3(0f, 0.015f, 0f), new Vector3(0.36f, 0.03f, 0.28f), trimMat);

            // Front accent strip or clasp
            CreatePrimitive(PrimitiveType.Cube, root.transform, "FrontStrip", new Vector3(0f, 0.11f, -0.132f), new Vector3(0.24f, 0.03f, 0.012f), accentMat);

            // Hinged Lid Pivot at back edge (y=0.19, z=+0.13)
            GameObject pivotGo = new GameObject("LidPivot");
            pivotGo.transform.SetParent(root.transform, false);
            pivotGo.transform.localPosition = new Vector3(0f, 0.19f, 0.13f);
            lidPivot = pivotGo.transform;

            // Lid plate
            CreatePrimitive(PrimitiveType.Cube, lidPivot, "LidPlate", new Vector3(0f, 0.025f, -0.13f), new Vector3(0.355f, 0.05f, 0.275f), bodyMat);
            CreatePrimitive(PrimitiveType.Cube, lidPivot, "LidTrim", new Vector3(0f, 0.052f, -0.13f), new Vector3(0.32f, 0.012f, 0.24f), trimMat);

            if (visualVariantKey == "BrassChest_Steam")
            {
                // Pressure dial on front
                CreatePrimitive(PrimitiveType.Cylinder, root.transform, "Gauge", new Vector3(0.09f, 0.11f, -0.135f), new Vector3(0.05f, 0.012f, 0.05f), MatBrass, new Vector3(90f, 0f, 0f));
                CreatePrimitive(PrimitiveType.Cylinder, root.transform, "GaugeFace", new Vector3(0.09f, 0.11f, -0.142f), new Vector3(0.036f, 0.005f, 0.036f), MatDialWhite, new Vector3(90f, 0f, 0f));
            }
            else if (visualVariantKey == "RunicBox_Arcane")
            {
                // Glowing sigil diamond on lid
                CreatePrimitive(PrimitiveType.Cube, lidPivot, "RuneSigil", new Vector3(0f, 0.062f, -0.13f), new Vector3(0.09f, 0.01f, 0.09f), MatArcaneGlow, new Vector3(0f, 45f, 0f));
            }
            else if (visualVariantKey == "HoloPod_SciFi")
            {
                // Glowing side strips
                CreatePrimitive(PrimitiveType.Cube, root.transform, "HoloSideL", new Vector3(-0.175f, 0.11f, 0f), new Vector3(0.01f, 0.025f, 0.20f), MatCyanGlow);
                CreatePrimitive(PrimitiveType.Cube, root.transform, "HoloSideR", new Vector3(0.175f, 0.11f, 0f), new Vector3(0.01f, 0.025f, 0.20f), MatCyanGlow);
            }

            // Seal on front latch
            GameObject sealGo = CreatePrimitive(PrimitiveType.Cylinder, root.transform, "SealPart", new Vector3(0f, 0.185f, -0.142f), new Vector3(0.055f, 0.014f, 0.055f), accentMat, new Vector3(90f, 0f, 0f));
            sealPart = sealGo.transform;
        }

        public static GameObject CreateArtifactModel(Transform parent, string visualPrefabKey, QualityTier quality, AuthenticityState auth)
        {
            string fbxName = visualPrefabKey switch
            {
                "PocketWatch_Brass" => "SM_Art_PocketWatch",
                "Chalice_Royal" => "SM_Art_Chalice",
                "HoloCube_SciFi" => "SM_Art_HoloCube",
                "RunicMask_Arcane" => "SM_Art_RunicMask",
                "FossilAmber_Ancient" => "SM_Art_AmberFossil",
                "Compass_Starlight" => "SM_Art_StarlightCompass",
                "Amulet_Jade" => "SM_Art_JadeAmulet",
                "GearCore_Steam" => "SM_Art_GearCore",
                "AutomatonBird_Brass" => "SM_Art_AutomatonBird",
                "VoidPrism_Alien" => "SM_Art_VoidPrism",
                _ => null
            };

            GameObject root = !string.IsNullOrEmpty(fbxName) ? InstantiateBlenderModel(fbxName, parent) : null;
            if (root == null)
            {
                root = new GameObject($"Artifact_{visualPrefabKey}");
                root.transform.SetParent(parent, false);

                switch (visualPrefabKey)
                {
                    case "PocketWatch_Brass":
                        BuildPocketWatch(root.transform);
                        break;
                    case "Chalice_Royal":
                        BuildRoyalChalice(root.transform);
                        break;
                    case "HoloCube_SciFi":
                        BuildHoloCube(root.transform);
                        break;
                    case "RunicMask_Arcane":
                        BuildRunicMask(root.transform);
                        break;
                    case "FossilAmber_Ancient":
                        BuildAmberFossil(root.transform);
                        break;
                    case "Compass_Starlight":
                        BuildStarlightCompass(root.transform);
                        break;
                    case "Amulet_Jade":
                        BuildJadeAmulet(root.transform);
                        break;
                    case "GearCore_Steam":
                        BuildGearCore(root.transform);
                        break;
                    case "AutomatonBird_Brass":
                        BuildAutomatonBird(root.transform);
                        break;
                    case "VoidPrism_Alien":
                        BuildVoidPrism(root.transform);
                        break;
                    default:
                        BuildPocketWatch(root.transform);
                        break;
                }
            }

            // Add subtle surface clues for Quality & Authenticity
            if (quality == QualityTier.Damaged || quality == QualityTier.Worn)
            {
                CreatePrimitive(PrimitiveType.Cube, root.transform, "SurfaceCrack_1", new Vector3(0.04f, 0.06f, 0.055f), new Vector3(0.06f, 0.008f, 0.008f), MatCrack, new Vector3(12f, 20f, 35f));
                if (quality == QualityTier.Damaged)
                {
                    CreatePrimitive(PrimitiveType.Cube, root.transform, "SurfaceCrack_2", new Vector3(-0.04f, 0.09f, -0.05f), new Vector3(0.07f, 0.01f, 0.01f), MatCrack, new Vector3(-20f, 10f, -40f));
                }
            }

            if (auth == AuthenticityState.Authentic)
            {
                // Tiny crisp golden maker's seal on base
                CreatePrimitive(PrimitiveType.Cylinder, root.transform, "AuthenticStamp", new Vector3(0f, 0.01f, -0.055f), new Vector3(0.022f, 0.006f, 0.022f), MatGold, new Vector3(90f, 0f, 0f));
            }

            return root;
        }

        private static void BuildPocketWatch(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cylinder, root, "Case", new Vector3(0f, 0.08f, 0f), new Vector3(0.16f, 0.024f, 0.16f), MatBrass, new Vector3(90f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Cylinder, root, "Dial", new Vector3(0f, 0.08f, -0.016f), new Vector3(0.13f, 0.012f, 0.13f), MatDialWhite, new Vector3(90f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Cube, root, "HourHand", new Vector3(0.012f, 0.092f, -0.03f), new Vector3(0.008f, 0.042f, 0.006f), MatIron, new Vector3(0f, 0f, -35f));
            CreatePrimitive(PrimitiveType.Cube, root, "MinHand", new Vector3(-0.015f, 0.095f, -0.031f), new Vector3(0.006f, 0.055f, 0.006f), MatBrass, new Vector3(0f, 0f, 50f));
            CreatePrimitive(PrimitiveType.Cylinder, root, "Crown", new Vector3(0f, 0.175f, 0f), new Vector3(0.028f, 0.02f, 0.028f), MatGold);
            CreatePrimitive(PrimitiveType.Sphere, root, "Ring", new Vector3(0f, 0.20f, 0f), new Vector3(0.035f, 0.035f, 0.014f), MatGold);
        }

        private static void BuildRoyalChalice(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cylinder, root, "Foot", new Vector3(0f, 0.015f, 0f), new Vector3(0.12f, 0.015f, 0.12f), MatGold);
            CreatePrimitive(PrimitiveType.Cylinder, root, "Stem", new Vector3(0f, 0.07f, 0f), new Vector3(0.035f, 0.055f, 0.035f), MatBrass);
            CreatePrimitive(PrimitiveType.Sphere, root, "Node", new Vector3(0f, 0.075f, 0f), new Vector3(0.055f, 0.035f, 0.055f), MatGold);
            CreatePrimitive(PrimitiveType.Cylinder, root, "Cup", new Vector3(0f, 0.155f, 0f), new Vector3(0.13f, 0.055f, 0.13f), MatGold);
            CreatePrimitive(PrimitiveType.Sphere, root, "GemFront", new Vector3(0f, 0.16f, -0.065f), new Vector3(0.025f, 0.025f, 0.018f), MatWaxRed);
            CreatePrimitive(PrimitiveType.Sphere, root, "GemBack", new Vector3(0f, 0.16f, 0.065f), new Vector3(0.025f, 0.025f, 0.018f), MatJade);
        }

        private static void BuildHoloCube(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cube, root, "OuterFrame", new Vector3(0f, 0.10f, 0f), new Vector3(0.14f, 0.14f, 0.14f), MatIron);
            CreatePrimitive(PrimitiveType.Cube, root, "InnerCore", new Vector3(0f, 0.10f, 0f), new Vector3(0.09f, 0.09f, 0.09f), MatCyanGlow, new Vector3(35f, 45f, 0f));
            CreatePrimitive(PrimitiveType.Cube, root, "BandH", new Vector3(0f, 0.10f, 0f), new Vector3(0.155f, 0.02f, 0.155f), MatCyanGlow);
            CreatePrimitive(PrimitiveType.Cube, root, "BandV", new Vector3(0f, 0.10f, 0f), new Vector3(0.02f, 0.155f, 0.155f), MatSlate);
        }

        private static void BuildRunicMask(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cube, root, "MaskPlate", new Vector3(0f, 0.11f, 0f), new Vector3(0.14f, 0.19f, 0.05f), MatSlate);
            CreatePrimitive(PrimitiveType.Cube, root, "EyeL", new Vector3(-0.032f, 0.13f, -0.028f), new Vector3(0.032f, 0.015f, 0.01f), MatArcaneGlow);
            CreatePrimitive(PrimitiveType.Cube, root, "EyeR", new Vector3(0.032f, 0.13f, -0.028f), new Vector3(0.032f, 0.015f, 0.01f), MatArcaneGlow);
            CreatePrimitive(PrimitiveType.Cube, root, "ForeheadSigil", new Vector3(0f, 0.175f, -0.028f), new Vector3(0.028f, 0.028f, 0.01f), MatGold, new Vector3(0f, 0f, 45f));
            CreatePrimitive(PrimitiveType.Cube, root, "HornL", new Vector3(-0.065f, 0.21f, 0f), new Vector3(0.025f, 0.06f, 0.025f), MatGold, new Vector3(0f, 0f, 20f));
            CreatePrimitive(PrimitiveType.Cube, root, "HornR", new Vector3(0.065f, 0.21f, 0f), new Vector3(0.025f, 0.06f, 0.025f), MatGold, new Vector3(0f, 0f, -20f));
        }

        private static void BuildAmberFossil(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cube, root, "StoneBase", new Vector3(0f, 0.025f, 0f), new Vector3(0.16f, 0.04f, 0.13f), MatSlate);
            CreatePrimitive(PrimitiveType.Sphere, root, "AmberBody", new Vector3(0f, 0.10f, 0f), new Vector3(0.13f, 0.14f, 0.10f), MatAmber, new Vector3(10f, 15f, -8f));
            CreatePrimitive(PrimitiveType.Sphere, root, "FossilCore", new Vector3(0f, 0.10f, -0.01f), new Vector3(0.045f, 0.06f, 0.035f), MatWoodDark, new Vector3(20f, 0f, 15f));
        }

        private static void BuildStarlightCompass(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cylinder, root, "Base", new Vector3(0f, 0.03f, 0f), new Vector3(0.16f, 0.02f, 0.16f), MatBrass);
            CreatePrimitive(PrimitiveType.Cylinder, root, "Face", new Vector3(0f, 0.052f, 0f), new Vector3(0.13f, 0.006f, 0.13f), MatSlate);
            CreatePrimitive(PrimitiveType.Cube, root, "NeedleA", new Vector3(0f, 0.065f, 0f), new Vector3(0.11f, 0.008f, 0.018f), MatGold, new Vector3(0f, 25f, 0f));
            CreatePrimitive(PrimitiveType.Cube, root, "NeedleB", new Vector3(0f, 0.065f, 0f), new Vector3(0.018f, 0.008f, 0.11f), MatCyanGlow, new Vector3(0f, 25f, 0f));
            CreatePrimitive(PrimitiveType.Cylinder, root, "PivotPin", new Vector3(0f, 0.072f, 0f), new Vector3(0.02f, 0.012f, 0.02f), MatGold);
        }

        private static void BuildJadeAmulet(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cylinder, root, "JadeDisc", new Vector3(0f, 0.10f, 0f), new Vector3(0.14f, 0.018f, 0.14f), MatJade, new Vector3(90f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Cylinder, root, "GoldCenter", new Vector3(0f, 0.10f, 0f), new Vector3(0.055f, 0.022f, 0.055f), MatGold, new Vector3(90f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Cube, root, "TopBail", new Vector3(0f, 0.18f, 0f), new Vector3(0.035f, 0.035f, 0.025f), MatGold);
        }

        private static void BuildGearCore(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cylinder, root, "MainWheel", new Vector3(0f, 0.10f, 0f), new Vector3(0.16f, 0.025f, 0.16f), MatBrass, new Vector3(90f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Sphere, root, "FurnaceCore", new Vector3(0f, 0.10f, 0f), new Vector3(0.065f, 0.065f, 0.065f), MatWarmGlow);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.085f, 0.10f + Mathf.Sin(angle) * 0.085f, 0f);
                CreatePrimitive(PrimitiveType.Cube, root, $"Tooth_{i}", pos, new Vector3(0.024f, 0.024f, 0.032f), MatGold, new Vector3(0f, 0f, i * 60f));
            }
        }

        private static void BuildAutomatonBird(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cylinder, root, "PerchBase", new Vector3(0f, 0.015f, 0f), new Vector3(0.12f, 0.015f, 0.12f), MatWoodDark);
            CreatePrimitive(PrimitiveType.Cylinder, root, "PerchPost", new Vector3(0f, 0.055f, 0f), new Vector3(0.016f, 0.04f, 0.016f), MatBrass);
            CreatePrimitive(PrimitiveType.Sphere, root, "Body", new Vector3(0f, 0.12f, 0f), new Vector3(0.085f, 0.075f, 0.12f), MatGold, new Vector3(-15f, 0f, 0f));
            CreatePrimitive(PrimitiveType.Sphere, root, "Head", new Vector3(0f, 0.17f, -0.05f), new Vector3(0.055f, 0.055f, 0.06f), MatBrass);
            CreatePrimitive(PrimitiveType.Cube, root, "Beak", new Vector3(0f, 0.165f, -0.09f), new Vector3(0.015f, 0.015f, 0.035f), MatGold);
            CreatePrimitive(PrimitiveType.Cube, root, "WingL", new Vector3(-0.05f, 0.13f, 0.01f), new Vector3(0.015f, 0.045f, 0.09f), MatBrass, new Vector3(10f, 0f, 20f));
            CreatePrimitive(PrimitiveType.Cube, root, "WingR", new Vector3(0.05f, 0.13f, 0.01f), new Vector3(0.015f, 0.045f, 0.09f), MatBrass, new Vector3(10f, 0f, -20f));
            CreatePrimitive(PrimitiveType.Sphere, root, "EyeL", new Vector3(-0.022f, 0.178f, -0.068f), new Vector3(0.014f, 0.014f, 0.014f), MatCyanGlow);
        }

        private static void BuildVoidPrism(Transform root)
        {
            CreatePrimitive(PrimitiveType.Cube, root, "Crystal", new Vector3(0f, 0.12f, 0f), new Vector3(0.08f, 0.15f, 0.08f), MatObsidian, new Vector3(15f, 45f, 15f));
            CreatePrimitive(PrimitiveType.Cylinder, root, "RingA", new Vector3(0f, 0.12f, 0f), new Vector3(0.17f, 0.006f, 0.17f), MatArcaneGlow, new Vector3(25f, 0f, 20f));
            CreatePrimitive(PrimitiveType.Cylinder, root, "RingB", new Vector3(0f, 0.12f, 0f), new Vector3(0.15f, 0.006f, 0.15f), MatCyanGlow, new Vector3(-30f, 15f, -15f));
        }
    }
}
