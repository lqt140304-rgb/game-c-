#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using ChoDoLa.Contracts;
using ChoDoLa.Interaction;
using ChoDoLa.Player;
using ChoDoLa.Sprint1;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ChoDoLa.Sprint1.Editor
{
    /// <summary>
    /// Builds the Sprint 1 plaza (Central Multiverse Fountain with animated flowing water,
    /// raised stone sidewalks/curbs, Blender street lanterns, two facing walk-in Blender shops:
    /// Quán Bán Hộp Mù on West & Tiệm Mở Hộp on East, Inn Ledger Desk, and natural daylight).
    /// </summary>
    public static class Sprint1SceneSetup
    {
        private const string RootName = "Sprint1_Market";
        private const string ModelBase = "Assets/_Project/Resources/Blender/Models";

        [MenuItem("Tools/Cho Do La/Setup Sprint 1 Market Loop")]
        public static void SetupMenu()
        {
            AssetDatabase.Refresh();
            BuildInCurrentScene(saveScene: true);
            Debug.Log("[ChoDoLa] Sprint 1 Market Loop (Blender Walk-In Shops + Flowing Fountain + Natural Lighting) built.");
        }

        public static void BuildInCurrentScene(bool saveScene = false)
        {
            GameObject oldProps = GameObject.Find("OpenTable_Props");
            if (oldProps != null) Object.DestroyImmediate(oldProps);

            GameObject oldRoot = GameObject.Find(RootName);
            if (oldRoot != null) Object.DestroyImmediate(oldRoot);

            GameObject sprint0Hud = GameObject.Find("HUD_Canvas");
            if (sprint0Hud != null) sprint0Hud.SetActive(false);

            HideDebugMarkers();
            ApplyGroundCobbleMaterial();
            UpgradeTownBackgroundMaterials();

            GameObject root = new GameObject(RootName);

            Sprint1MarketService service = root.AddComponent<Sprint1MarketService>();
            ArtifactInspector3D inspector = root.AddComponent<ArtifactInspector3D>();
            OpenTableManager tableMgr = root.AddComponent<OpenTableManager>();
            Sprint1HudController hud = root.AddComponent<Sprint1HudController>();
            root.AddComponent<MarketAmbientAnimator>();

            GameObject player = GameObject.Find("Player");
            FirstPersonController fpc = player != null ? player.GetComponent<FirstPersonController>() : null;
            PlayerInteractor interactor = player != null ? player.GetComponent<PlayerInteractor>() : null;

            BuildCentralFountain(root.transform, new Vector3(0f, 0f, 22.0f));
            BuildPlazaStreetAndLanterns(root.transform);
            DressArcaneWell(root.transform);
            BuildBlindBoxWalkInHouse(root.transform, new Vector3(-7.2f, 0f, 22.0f), service, hud);
            BuildOpenBoxWalkInHouse(root.transform, new Vector3(7.2f, 0f, 22.0f), out Transform[] boxSlots, out Transform inspectAnchor);
            BuildInnLedgerDesk(root.transform, new Vector3(1.85f, 0f, 4.8f), hud);

            inspector.Configure(inspectAnchor, fpc, interactor, hud);
            tableMgr.Configure(service, inspector, hud, boxSlots, inspectAnchor);
            hud.Configure(service, inspector, tableMgr, fpc, interactor);

            ConfigureNaturalLighting();
            EnsureEventSystem();

            if (saveScene)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
        }

        private static void HideDebugMarkers()
        {
            foreach (string markerName in new[] { "OpenTable_Zone", "Inn_Spawn" })
            {
                GameObject m = GameObject.Find(markerName);
                if (m == null) continue;
                foreach (Renderer r in m.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
            }

            foreach (ChoDoLa.Town.ShopSlotMarker slot in Object.FindObjectsByType<ChoDoLa.Town.ShopSlotMarker>(FindObjectsSortMode.None))
            {
                foreach (Renderer r in slot.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
            }
        }

        private static void UpgradeTownBackgroundMaterials()
        {
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Stone.mat", "T_AshlarStone_Albedo", "T_AshlarStone_Normal", new Color(0.88f, 0.85f, 0.80f), new Vector2(3.5f, 3.5f), 0.0f, 0.22f);
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Moss.mat", "T_WeatheredPlaster_Albedo", "T_WeatheredPlaster_Normal", new Color(0.86f, 0.88f, 0.80f), new Vector2(3.0f, 2.5f), 0.0f, 0.20f);
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Wood.mat", "T_AgedWood_Albedo", "T_AgedWood_Normal", new Color(0.92f, 0.86f, 0.80f), new Vector2(2.5f, 2.5f), 0.02f, 0.35f);
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Roof.mat", "T_RoofTile_Albedo", null, Color.white, new Vector2(3.0f, 3.0f), 0.0f, 0.28f);
            ApplyMaterialGlow("Assets/_Project/Art/Greybox/Materials/GB_Brass.mat", new Color(0.78f, 0.57f, 0.20f), 0.85f, 0.72f, null);
            ApplyMaterialGlow("Assets/_Project/Art/Greybox/Materials/GB_SciFi.mat", new Color(0.20f, 0.78f, 0.96f), 0.35f, 0.85f, new Color(0.18f, 0.85f, 1.1f));
            ApplyMaterialGlow("Assets/_Project/Art/Greybox/Materials/GB_Arcane.mat", new Color(0.54f, 0.28f, 0.92f), 0.20f, 0.72f, new Color(0.55f, 0.20f, 0.95f));
        }

        private static void ApplyMaterialTex(string matPath, string albedoRes, string normalRes, Color tint, Vector2 tiling, float metallic, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) return;

            Texture2D albedo = Resources.Load<Texture2D>($"Blender/Textures/{albedoRes}");
            Texture2D normal = string.IsNullOrEmpty(normalRes) ? null : Resources.Load<Texture2D>($"Blender/Textures/{normalRes}");

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (albedo != null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", albedo);
                    mat.SetTextureScale("_BaseMap", tiling);
                }
                else
                {
                    mat.mainTexture = albedo;
                    mat.mainTextureScale = tiling;
                }
            }
            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normal);
                mat.SetTextureScale("_BumpMap", tiling);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 0.75f);
            }
            EditorUtility.SetDirty(mat);
        }

        private static void ApplyMaterialGlow(string matPath, Color color, float metallic, float smoothness, Color? emission)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) return;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
            }
            EditorUtility.SetDirty(mat);
        }

        private static void DressArcaneWell(Transform parent)
        {
            GameObject well = GameObject.Find("Well_Arcane");
            if (well == null) return;

            foreach (Renderer r in well.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled = false;
            }

            GameObject deco = new GameObject("Well_Arcane_Decor");
            deco.transform.SetParent(parent, false);
            deco.transform.position = new Vector3(well.transform.position.x, 0f, well.transform.position.z);

            Material stoneMat = MarketVisualFactory.GetTexturedMaterial("B_AshlarStone", "T_AshlarStone_Albedo", "T_AshlarStone_Normal", Color.white, 0f, 0.26f, 0.85f);
            Material woodDark = MarketVisualFactory.GetMaterial("WoodDark", new Color(0.24f, 0.15f, 0.09f), 0.04f, 0.38f);
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cylinder, deco.transform, "Well_StoneRim", new Vector3(0f, 0.45f, 0f), new Vector3(1.65f, 0.45f, 1.65f), stoneMat);
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cylinder, deco.transform, "Well_Water", new Vector3(0f, 0.88f, 0f), new Vector3(1.30f, 0.04f, 1.30f), MarketVisualFactory.GetMaterial("B_Water", new Color(0.10f, 0.56f, 0.68f), 0.1f, 0.92f, new Color(0.08f, 0.35f, 0.48f)));
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cube, deco.transform, "Well_PostL", new Vector3(-0.72f, 1.35f, 0f), new Vector3(0.14f, 1.75f, 0.14f), woodDark);
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cube, deco.transform, "Well_PostR", new Vector3(0.72f, 1.35f, 0f), new Vector3(0.14f, 1.75f, 0.14f), woodDark);
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cube, deco.transform, "Well_Crossbar", new Vector3(0f, 2.22f, 0f), new Vector3(1.68f, 0.14f, 0.16f), woodDark);

            GameObject crystal = InstantiateModel("SM_Art_VoidPrism.fbx", deco.transform, new Vector3(0f, 1.08f, 0f));
            if (crystal != null)
            {
                crystal.name = "Well_Crystal";
                crystal.transform.localScale = Vector3.one * 3.8f;
            }
            AddPointLight(deco.transform, "Well_ArcaneLight", new Vector3(0f, 1.65f, 0f), new Color(0.62f, 0.32f, 1.0f), 2.2f, 6.5f);
        }

        private static void ApplyGroundCobbleMaterial()
        {
            GameObject ground = GameObject.Find("Ground_Town");
            if (ground == null) return;
            Renderer r = ground.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = MarketVisualFactory.GetCobbleMaterial(new Vector2(24f, 27f));
            }
        }

        private static GameObject InstantiateModel(string fbxFileName, Transform parent, Vector3 localPos)
        {
            string assetPath = $"{ModelBase}/{fbxFileName}";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject go;
            if (asset != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            }
            else
            {
                string resName = fbxFileName.Replace(".fbx", string.Empty);
                go = MarketVisualFactory.InstantiateBlenderModel(resName, parent);
            }

            if (go != null)
            {
                go.transform.localPosition = localPos;
                go.transform.localRotation = Quaternion.identity;
                MarketVisualFactory.ApplyBlenderMaterials(go);
            }
            return go;
        }

        private static void AddBoxColliderChild(Transform parent, string name, Vector3 center, Vector3 size)
        {
            GameObject colGo = new GameObject(name);
            colGo.transform.SetParent(parent, false);
            BoxCollider bc = colGo.AddComponent<BoxCollider>();
            bc.center = center;
            bc.size = size;
        }

        private static Light AddPointLight(Transform parent, string name, Vector3 localPos, Color color, float intensity, float range, bool softShadows = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            if (softShadows)
            {
                l.shadows = LightShadows.Soft;
                l.shadowStrength = 0.7f;
            }
            return l;
        }

        private static void BuildCentralFountain(Transform parent, Vector3 pos)
        {
            GameObject root = new GameObject("Fountain_Central_Plaza");
            root.transform.SetParent(parent, false);
            root.transform.position = pos;

            InstantiateModel("SM_Fountain_Central.fbx", root.transform, Vector3.zero);
            root.AddComponent<FountainWaterController>();
            AddBoxColliderChild(root.transform, "Collider_Basin", new Vector3(0f, 0.70f, 0f), new Vector3(3.6f, 1.4f, 3.6f));
            AddPointLight(root.transform, "Fountain_AquaLight", new Vector3(0f, 1.35f, 0f), new Color(0.35f, 0.90f, 1.0f), 2.4f, 7.5f);
        }

        private static void BuildPlazaStreetAndLanterns(Transform parent)
        {
            // Raised ashlar sidewalks, beveled curbstones, moss clumps & scattered stepping flagstones
            GameObject streetDetails = new GameObject("Plaza_Street_Details");
            streetDetails.transform.SetParent(parent, false);
            streetDetails.transform.position = new Vector3(0f, 0f, 22.0f);
            InstantiateModel("SM_Plaza_StreetDetails.fbx", streetDetails.transform, Vector3.zero);
            AddBoxColliderChild(streetDetails.transform, "Col_Sidewalk_West", new Vector3(-7.2f, 0.045f, 0f), new Vector3(5.6f, 0.09f, 10.6f));
            AddBoxColliderChild(streetDetails.transform, "Col_Sidewalk_East", new Vector3(7.2f, 0.045f, 0f), new Vector3(5.6f, 0.09f, 10.6f));

            // Replace greybox wooden blockout streetlights with Blender wrought-iron street lanterns
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (!obj.name.StartsWith("Light_")) continue;
                // Avoid re-replacing if already converted
                if (obj.transform.Find("SM_StreetLantern") != null) continue;

                foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
                foreach (Light l in obj.GetComponentsInChildren<Light>(true))
                {
                    l.enabled = false;
                }

                Vector3 groundPos = new Vector3(obj.transform.position.x, 0f, obj.transform.position.z);
                SpawnBlenderStreetLantern(parent, $"Lantern_{obj.name}", groundPos);
            }

            // Also place the 4 signature Plaza Corner Lanterns around the Fountain Plaza
            Vector3[] plazaCorners =
            {
                new Vector3(-3.8f, 0f, 22.0f + 5.2f),
                new Vector3(3.8f, 0f, 22.0f + 5.2f),
                new Vector3(-3.8f, 0f, 22.0f - 5.2f),
                new Vector3(3.8f, 0f, 22.0f - 5.2f),
            };
            for (int i = 0; i < plazaCorners.Length; i++)
            {
                SpawnBlenderStreetLantern(parent, $"Plaza_Lantern_{i + 1}", plazaCorners[i]);
            }
        }

        private static void SpawnBlenderStreetLantern(Transform parent, string name, Vector3 worldPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;

            InstantiateModel("SM_StreetLantern.fbx", go.transform, Vector3.zero);
            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.5f, 0f);
            col.radius = 0.14f;
            col.height = 3.0f;

            AddPointLight(go.transform, "Lantern_WarmLight", new Vector3(0f, 2.95f, 0f), new Color(1.0f, 0.82f, 0.52f), 2.4f, 10.5f);
        }

        private static void BuildBlindBoxWalkInHouse(Transform parent, Vector3 pos, Sprint1MarketService service, Sprint1HudController hud)
        {
            GameObject house = new GameObject("Shop_BlindBox_House");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            InstantiateModel("SM_House_BlindBox.fbx", house.transform, Vector3.zero);

            AddBoxColliderChild(house.transform, "Col_Floor", new Vector3(0f, 0.12f, 0f), new Vector3(5.2f, 0.24f, 6.2f));
            AddBoxColliderChild(house.transform, "Col_BackWall", new Vector3(-2.42f, 1.8f, 0f), new Vector3(0.28f, 3.6f, 6.0f));
            AddBoxColliderChild(house.transform, "Col_SideWall_S", new Vector3(0f, 1.8f, -2.90f), new Vector3(5.0f, 3.6f, 0.28f));
            AddBoxColliderChild(house.transform, "Col_SideWall_N", new Vector3(0f, 1.8f, 2.90f), new Vector3(5.0f, 3.6f, 0.28f));
            AddBoxColliderChild(house.transform, "Col_FrontPier_S", new Vector3(2.42f, 1.8f, -2.10f), new Vector3(0.28f, 3.6f, 1.80f));
            AddBoxColliderChild(house.transform, "Col_FrontPier_N", new Vector3(2.42f, 1.8f, 2.10f), new Vector3(0.28f, 3.6f, 1.80f));
            AddBoxColliderChild(house.transform, "Col_DoorLintel", new Vector3(2.42f, 3.30f, 0f), new Vector3(0.28f, 1.0f, 2.40f));
            AddBoxColliderChild(house.transform, "Col_Turret", new Vector3(1.15f, 2.2f, -3.15f), new Vector3(2.0f, 4.4f, 2.0f));

            GameObject counterCol = new GameObject("Counter_InteractZone");
            counterCol.transform.SetParent(house.transform, false);
            BoxCollider cbc = counterCol.AddComponent<BoxCollider>();
            cbc.center = new Vector3(-0.62f, 0.60f, 0f);
            cbc.size = new Vector3(1.04f, 1.20f, 2.92f);
            CounterInteractable ci = counterCol.AddComponent<CounterInteractable>();
            ci.Configure(hud);

            var offers = Sprint1Catalog.CreateDailyOffers();
            float[] zOffsets = { -1.02f, -0.34f, 0.34f, 1.02f };
            for (int i = 0; i < offers.Count && i < zOffsets.Length; i++)
            {
                BoxOfferView offer = offers[i];
                GameObject disp = new GameObject($"DisplayBox_{offer.VisualVariantKey}");
                disp.transform.SetParent(house.transform, false);
                disp.transform.localPosition = new Vector3(-0.52f, 1.20f, zOffsets[i]);
                disp.transform.localRotation = Quaternion.Euler(0f, -90f + (i - 1.5f) * 6f, 0f);

                MarketVisualFactory.CreateBoxModel(disp.transform, offer.VisualVariantKey, out _, out _);

                BoxCollider col = disp.AddComponent<BoxCollider>();
                col.center = new Vector3(0f, 0.12f, 0f);
                col.size = new Vector3(0.38f, 0.26f, 0.30f);

                BlindBoxProp prop = disp.AddComponent<BlindBoxProp>();
                prop.ConfigureCounterDisplay(offer.OfferId, offer.DisplayName, offer.Price, service, hud);
            }

            AddPointLight(house.transform, "Interior_LanternLight", new Vector3(0.2f, 2.55f, 0f), new Color(1.0f, 0.82f, 0.55f), 2.3f, 7.5f, softShadows: true);
            AddPointLight(house.transform, "Porch_LanternLight", new Vector3(3.05f, 2.48f, 0f), new Color(1.0f, 0.78f, 0.45f), 1.8f, 5.5f);
            AddPointLight(house.transform, "Turret_ArcaneLight", new Vector3(1.15f, 3.6f, -3.15f), new Color(0.65f, 0.32f, 1.0f), 1.8f, 5.5f);
        }

        private static void BuildOpenBoxWalkInHouse(Transform parent, Vector3 pos, out Transform[] boxSlots, out Transform inspectAnchor)
        {
            GameObject house = new GameObject("Shop_OpenBox_House");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            InstantiateModel("SM_House_OpenBox.fbx", house.transform, Vector3.zero);

            AddBoxColliderChild(house.transform, "Col_Floor", new Vector3(0f, 0.12f, 0f), new Vector3(5.2f, 0.24f, 6.2f));
            AddBoxColliderChild(house.transform, "Col_BackWall", new Vector3(2.42f, 1.8f, 0f), new Vector3(0.28f, 3.6f, 6.0f));
            AddBoxColliderChild(house.transform, "Col_SideWall_S", new Vector3(0f, 1.8f, -2.90f), new Vector3(5.0f, 3.6f, 0.28f));
            AddBoxColliderChild(house.transform, "Col_SideWall_N", new Vector3(0f, 1.8f, 2.90f), new Vector3(5.0f, 3.6f, 0.28f));
            AddBoxColliderChild(house.transform, "Col_FrontPier_S", new Vector3(-2.42f, 1.8f, -2.10f), new Vector3(0.28f, 3.6f, 1.80f));
            AddBoxColliderChild(house.transform, "Col_FrontPier_N", new Vector3(-2.42f, 1.8f, 2.10f), new Vector3(0.28f, 3.6f, 1.80f));
            AddBoxColliderChild(house.transform, "Col_DoorLintel", new Vector3(-2.42f, 3.30f, 0f), new Vector3(0.28f, 1.0f, 2.40f));

            AddBoxColliderChild(house.transform, "Col_Workbench", new Vector3(0.55f, 0.56f, 0f), new Vector3(1.12f, 1.12f, 2.65f));

            boxSlots = new Transform[3];
            float[] slotZ = { 0.92f, 0.50f, -0.54f };
            for (int i = 0; i < 3; i++)
            {
                GameObject s = new GameObject($"BoxSlot_{i + 1}");
                s.transform.SetParent(house.transform, false);
                s.transform.localPosition = new Vector3(0.52f, 1.125f, slotZ[i]);
                s.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                boxSlots[i] = s.transform;
            }

            GameObject anchor = new GameObject("InspectAnchor");
            anchor.transform.SetParent(house.transform, false);
            anchor.transform.localPosition = new Vector3(0.48f, 1.14f, -0.10f);
            anchor.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            inspectAnchor = anchor.transform;

            GameObject spotGo = new GameObject("Workbench_SpotLight");
            spotGo.transform.SetParent(house.transform, false);
            spotGo.transform.localPosition = new Vector3(0.62f, 1.52f, -0.62f);
            spotGo.transform.LookAt(anchor.transform.position);
            Light spot = spotGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1.0f, 0.90f, 0.72f);
            spot.range = 4.5f;
            spot.spotAngle = 60f;
            spot.intensity = 3.2f;
            spot.shadows = LightShadows.Soft;
            spot.shadowStrength = 0.75f;

            AddPointLight(house.transform, "Interior_LanternLight", new Vector3(-0.2f, 2.55f, 0f), new Color(1.0f, 0.82f, 0.55f), 2.2f, 7.5f, softShadows: true);
            AddPointLight(house.transform, "SubFloor_CyanLight", new Vector3(-0.95f, 0.45f, 0f), new Color(0.25f, 0.85f, 1.0f), 1.6f, 3.5f);
            AddPointLight(house.transform, "Porch_LanternLight", new Vector3(-3.05f, 2.48f, 0f), new Color(1.0f, 0.78f, 0.45f), 1.8f, 5.5f);
        }

        private static void BuildInnLedgerDesk(Transform parent, Vector3 pos, Sprint1HudController hud)
        {
            GameObject desk = new GameObject("Inn_Ledger_Desk");
            desk.transform.SetParent(parent, false);
            desk.transform.position = pos;

            var woodDark = MarketVisualFactory.GetMaterial("WoodDark", new Color(0.24f, 0.15f, 0.09f), 0.04f, 0.38f);
            var parchment = MarketVisualFactory.GetMaterial("Parchment", new Color(0.86f, 0.79f, 0.64f), 0f, 0.22f);
            var brass = MarketVisualFactory.GetMaterial("Brass", new Color(0.78f, 0.57f, 0.20f), 0.85f, 0.72f);

            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cube, desk.transform, "Body", new Vector3(0f, 0.45f, 0f), new Vector3(1.1f, 0.90f, 0.7f), woodDark, removeCollider: false);
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cube, desk.transform, "LedgerCover", new Vector3(-0.08f, 0.91f, 0f), new Vector3(0.38f, 0.025f, 0.28f), woodDark, new Vector3(0f, -12f, 0f));
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cube, desk.transform, "LedgerPages", new Vector3(-0.08f, 0.925f, 0f), new Vector3(0.34f, 0.02f, 0.24f), parchment, new Vector3(0f, -12f, 0f));
            MarketVisualFactory.CreatePrimitive(PrimitiveType.Cylinder, desk.transform, "Inkwell", new Vector3(0.24f, 0.93f, 0.12f), new Vector3(0.06f, 0.04f, 0.06f), brass);

            InnLedgerInteractable ledger = desk.AddComponent<InnLedgerInteractable>();
            ledger.Configure(hud);
        }

        private static void ConfigureNaturalLighting()
        {
            GameObject sunGo = GameObject.Find("Directional Light");
            if (sunGo != null)
            {
                Light sun = sunGo.GetComponent<Light>();
                if (sun != null)
                {
                    sun.type = LightType.Directional;
                    sun.color = new Color(1.0f, 0.93f, 0.80f);
                    sun.intensity = 1.45f;
                    sun.shadows = LightShadows.Soft;
                    sun.shadowStrength = 0.82f;
                    sunGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
                }
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.54f, 0.66f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.78f, 0.70f, 0.60f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.25f, 0.22f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.76f, 0.80f, 0.86f);
            RenderSettings.fogDensity = 0.0055f;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Component urpCamData = mainCam.GetComponent("UniversalAdditionalCameraData");
                if (urpCamData != null)
                {
                    PropertyInfo prop = urpCamData.GetType().GetProperty("renderPostProcessing", BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(urpCamData, true);
                    }
                }
            }
        }

        private static void EnsureEventSystem()
        {
            EventSystem es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            if (es.GetComponent<InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule old = es.GetComponent<StandaloneInputModule>();
                if (old != null) Object.DestroyImmediate(old);
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}
#endif
