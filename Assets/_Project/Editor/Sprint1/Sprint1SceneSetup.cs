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

            HideOldGreyboxArchitecture();
            ApplyGroundCobbleMaterial();

            GameObject root = new GameObject(RootName);

            Sprint1MarketService service = root.AddComponent<Sprint1MarketService>();
            ArtifactInspector3D inspector = root.AddComponent<ArtifactInspector3D>();
            OpenTableManager tableMgr = root.AddComponent<OpenTableManager>();
            Sprint1HudController hud = root.AddComponent<Sprint1HudController>();
            root.AddComponent<MarketAmbientAnimator>();

            GameObject player = GameObject.Find("Player");
            FirstPersonController fpc = player != null ? player.GetComponent<FirstPersonController>() : null;
            PlayerInteractor interactor = player != null ? player.GetComponent<PlayerInteractor>() : null;

            BuildHanoiCenterIsland(root.transform, new Vector3(0f, 0f, 22.0f));
            BuildPlazaStreetAndLanterns(root.transform);
            BuildBlindBoxWalkInHouse(root.transform, new Vector3(-7.2f, 0f, 22.0f), service, hud);
            BuildOpenBoxWalkInHouse(root.transform, new Vector3(7.2f, 0f, 22.0f), out Transform[] boxSlots, out Transform inspectAnchor);
            BuildHanoiStreetHighlightsAndTrees(root.transform);
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

        private static void HideOldGreyboxArchitecture()
        {
            HideDebugMarkers();

            GameObject townRoot = GameObject.Find("MarketTown");
            if (townRoot == null) return;

            foreach (Transform child in townRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == townRoot.transform) continue;
                string n = child.name;

                // Keep Ground_Town active and visible (paved with matte Vietnamese street material)
                if (n == "Ground_Town") continue;

                // Keep outer perimeter boundary walls as invisible physics fences so player cannot fall off map
                bool isOuterBoundaryFence =
                    n == "Wall_North" || n == "Wall_West_Edge" || n == "Wall_East_Edge" ||
                    n == "Wall_South_W" || n == "Wall_South_E";

                foreach (Renderer r in child.GetComponents<Renderer>())
                {
                    r.enabled = false;
                }

                if (!isOuterBoundaryFence)
                {
                    foreach (Collider c in child.GetComponents<Collider>())
                    {
                        if (!c.isTrigger)
                        {
                            c.enabled = false;
                        }
                    }
                }

                foreach (Light l in child.GetComponents<Light>())
                {
                    l.enabled = false;
                }
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

            foreach (ChoDoLa.Town.ShopSlotMarker slot in Object.FindObjectsByType<ChoDoLa.Town.ShopSlotMarker>())
            {
                foreach (Renderer r in slot.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
            }
        }

        private static void UpgradeTownBackgroundMaterials()
        {
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Stone.mat", "T_HanoiTerrazzo_Albedo", "T_HanoiTerrazzo_Normal", new Color(0.95f, 0.93f, 0.88f), new Vector2(4.0f, 4.0f), 0.0f, 0.30f);
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Moss.mat", null, null, new Color(0.92f, 0.89f, 0.80f), new Vector2(2.0f, 2.0f), 0.0f, 0.25f);
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Wood.mat", "T_AgedWood_Albedo", "T_AgedWood_Normal", new Color(0.76f, 0.64f, 0.50f), new Vector2(2.5f, 2.5f), 0.02f, 0.40f);
            ApplyMaterialTex("Assets/_Project/Art/Greybox/Materials/GB_Roof.mat", "T_RoofSlate_Albedo", null, new Color(0.32f, 0.35f, 0.40f), new Vector2(3.0f, 3.0f), 0.08f, 0.42f);
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
                r.sharedMaterial = MarketVisualFactory.GetHanoiPavingMaterial(new Vector2(36f, 38f));
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

        private static void BuildHanoiCenterIsland(Transform parent, Vector3 pos)
        {
            GameObject root = new GameObject("Hanoi_Center_Park_Island");
            root.transform.SetParent(parent, false);
            root.transform.position = pos;

            InstantiateModel("SM_Hanoi_CenterIsland.fbx", root.transform, Vector3.zero);
            AddBoxColliderChild(root.transform, "Col_PlanterBed", new Vector3(0f, 0.22f, 0f), new Vector3(2.8f, 0.44f, 4.6f));
            AddBoxColliderChild(root.transform, "Col_TreeTrunk", new Vector3(0f, 1.6f, 0.15f), new Vector3(0.55f, 3.2f, 0.55f));
            AddBoxColliderChild(root.transform, "Col_NoticeBoard", new Vector3(0f, 1.1f, -1.78f), new Vector3(1.55f, 2.2f, 0.35f));
            AddBoxColliderChild(root.transform, "Col_Bench_W", new Vector3(-1.45f, 0.45f, -0.35f), new Vector3(0.55f, 0.9f, 1.55f));
            AddBoxColliderChild(root.transform, "Col_Bench_E", new Vector3(1.45f, 0.45f, 0.35f), new Vector3(0.55f, 0.9f, 1.55f));
        }

        private static void BuildPlazaStreetAndLanterns(Transform parent)
        {
            // Raised Vietnamese red-and-grey terrazzo sidewalks, curbstones & cast-iron storm drains
            GameObject streetDetails = new GameObject("Plaza_Street_Details");
            streetDetails.transform.SetParent(parent, false);
            streetDetails.transform.position = new Vector3(0f, 0f, 22.0f);
            InstantiateModel("SM_Plaza_StreetDetails.fbx", streetDetails.transform, Vector3.zero);
            AddBoxColliderChild(streetDetails.transform, "Col_Sidewalk_West", new Vector3(-7.2f, 0.045f, 0f), new Vector3(5.6f, 0.09f, 10.6f));
            AddBoxColliderChild(streetDetails.transform, "Col_Sidewalk_East", new Vector3(7.2f, 0.045f, 0f), new Vector3(5.6f, 0.09f, 10.6f));

            // Neat Modern Hanoi Avenue Street Lamps lining both sidewalks cleanly
            Vector3[] lampPositions =
            {
                new Vector3(-3.8f, 0f, 12.2f),
                new Vector3(3.8f, 0f, 12.2f),
                new Vector3(-3.8f, 0f, 17.0f),
                new Vector3(3.8f, 0f, 17.0f),
                new Vector3(-3.8f, 0f, 27.2f),
                new Vector3(3.8f, 0f, 27.2f),
                new Vector3(-3.8f, 0f, 36.0f),
                new Vector3(3.8f, 0f, 36.0f),
                new Vector3(-3.8f, 0f, 48.0f),
                new Vector3(3.8f, 0f, 48.0f),
            };
            for (int i = 0; i < lampPositions.Length; i++)
            {
                SpawnBlenderStreetLantern(parent, $"Hanoi_StreetLamp_{i + 1}", lampPositions[i]);
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
            col.radius = 0.12f;
            col.height = 3.0f;

            AddPointLight(go.transform, "Lantern_WarmLight", new Vector3(0f, 2.95f, 0f), new Color(1.0f, 0.88f, 0.68f), 0.95f, 9.5f);
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

            AddPointLight(house.transform, "Interior_LanternLight", new Vector3(0.2f, 2.55f, 0f), new Color(1.0f, 0.90f, 0.74f), 1.15f, 7.5f, softShadows: true);
            AddPointLight(house.transform, "Porch_LanternLight", new Vector3(3.05f, 2.48f, 0f), new Color(1.0f, 0.88f, 0.68f), 0.75f, 5.5f);
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
            float[] slotZ = { 0.12f, 0.56f, 1.00f };
            for (int i = 0; i < 3; i++)
            {
                GameObject s = new GameObject($"BoxSlot_{i + 1}");
                s.transform.SetParent(house.transform, false);
                s.transform.localPosition = new Vector3(0.52f, 1.125f, slotZ[i]);
                s.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                boxSlots[i] = s.transform;
            }

            GameObject anchor = new GameObject("InspectAnchor");
            anchor.transform.SetParent(house.transform, false);
            anchor.transform.localPosition = new Vector3(0.48f, 1.14f, -0.52f);
            anchor.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            inspectAnchor = anchor.transform;

            GameObject spotGo = new GameObject("Workbench_SpotLight");
            spotGo.transform.SetParent(house.transform, false);
            spotGo.transform.localPosition = new Vector3(0.62f, 1.55f, -0.92f);
            spotGo.transform.LookAt(anchor.transform.position);
            Light spot = spotGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1.0f, 0.90f, 0.72f);
            spot.range = 4.5f;
            spot.spotAngle = 60f;
            spot.intensity = 1.65f;
            spot.shadows = LightShadows.Soft;
            spot.shadowStrength = 0.58f;

            AddPointLight(house.transform, "Interior_LanternLight", new Vector3(-0.2f, 2.55f, 0f), new Color(1.0f, 0.90f, 0.74f), 1.15f, 7.5f, softShadows: true);
            AddPointLight(house.transform, "Porch_LanternLight", new Vector3(-3.05f, 2.48f, 0f), new Color(1.0f, 0.88f, 0.68f), 0.75f, 5.5f);
        }

        private static void BuildHanoiStreetHighlightsAndTrees(Transform parent)
        {
            GameObject hanoiRoot = new GameObject("Hanoi_Modern_Neighborhood");
            hanoiRoot.transform.SetParent(parent, false);

            // 1. Highlight: Quán Phở Bò Hà Nội Hiện Đại (West avenue overlooking Main Street & North Park)
            SpawnHanoiPhoShop(hanoiRoot.transform, "Shop_Hanoi_Pho_South", new Vector3(-6.6f, 0f, 9.6f), 0f);
            SpawnHanoiPhoShop(hanoiRoot.transform, "Shop_Hanoi_Pho_EastMid", new Vector3(7.4f, 0f, 13.5f), 180f);
            SpawnHanoiPhoShop(hanoiRoot.transform, "Shop_Hanoi_Pho_North", new Vector3(22.0f, 0f, 78.0f), 180f);

            // 2. Highlight: Quầy Trà Đá & Vé Số Vỉa Hè (East avenue near spawn + North Park under shade trees)
            SpawnHanoiTraDaVeSo(hanoiRoot.transform, "Stall_TraDa_VeSo_South", new Vector3(5.4f, 0f, 5.5f), 180f);
            SpawnHanoiTraDaVeSo(hanoiRoot.transform, "Stall_TraDa_VeSo_North", new Vector3(-16.0f, 0f, 75.5f), 0f);

            // 3. Pure Modern 3-4 Story Hanoi Tube-House Shophouse Blocks framing BOTH inner street & outer district (replacing all old greybox architecture!)
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_SpawnBack_W", new Vector3(-6.2f, 0f, -0.5f), -90f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_SpawnBack_E", new Vector3(6.2f, 0f, -0.5f), -90f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_InnerW_North1", new Vector3(-7.6f, 0f, 32.5f), 0f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_InnerW_North2", new Vector3(-7.6f, 0f, 45.5f), 0f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_InnerE_North1", new Vector3(7.6f, 0f, 32.5f), 180f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_InnerE_North2", new Vector3(7.6f, 0f, 45.5f), 180f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_MidGate_W", new Vector3(-10.5f, 0f, 56.5f), 90f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_MidGate_E", new Vector3(10.5f, 0f, 56.5f), 90f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_NorthWalk_W", new Vector3(-7.6f, 0f, 65.0f), 0f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_NorthWalk_E", new Vector3(7.6f, 0f, 65.0f), 180f);

            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_SideW_1", new Vector3(-18.5f, 0f, 12.0f), 180f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_SideW_2", new Vector3(-18.5f, 0f, 36.0f), 180f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_SideE_1", new Vector3(18.5f, 0f, 12.0f), 0f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_SideE_2", new Vector3(18.5f, 0f, 36.0f), 0f);

            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_OuterW_1", new Vector3(-31.2f, 0f, 22.0f), 180f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_OuterW_2", new Vector3(-31.2f, 0f, 48.0f), 180f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_OuterE_1", new Vector3(31.2f, 0f, 22.0f), 0f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_OuterE_2", new Vector3(31.2f, 0f, 48.0f), 0f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_NorthAvenue_W", new Vector3(-16.0f, 0f, 90.0f), 90f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_NorthAvenue_C", new Vector3(0.0f, 0f, 90.0f), 90f);
            SpawnHanoiShophouseBlock(hanoiRoot.transform, "Hanoi_Block_NorthAvenue_E", new Vector3(16.0f, 0f, 90.0f), 90f);

            // 4. Lush Hanoi Street Trees (Cây Sấu / Xà Cừ) with stone planters across streets & North Park
            (Vector3 pos, float scale)[] treePlacements =
            {
                (new Vector3(-4.0f, 0f, 5.0f), 1.05f),
                (new Vector3(3.9f, 0f, 3.6f), 1.02f),
                (new Vector3(8.2f, 0f, 5.4f), 1.15f),
                (new Vector3(-6.2f, 0f, 15.8f), 1.10f),
                (new Vector3(-8.5f, 0f, 4.8f), 1.02f),
                (new Vector3(11.6f, 0f, 4.8f), 1.08f),
                (new Vector3(14.5f, 0f, 22.0f), 1.04f),
                (new Vector3(-39.2f, 0f, 18.0f), 1.08f),
                (new Vector3(-39.2f, 0f, 36.0f), 1.12f),
                (new Vector3(-39.2f, 0f, 54.0f), 1.05f),
                (new Vector3(39.2f, 0f, 18.0f), 1.08f),
                (new Vector3(39.2f, 0f, 36.0f), 1.12f),
                (new Vector3(39.2f, 0f, 54.0f), 1.05f),
                (new Vector3(-24.0f, 0f, 72.5f), 1.12f),
                (new Vector3(-12.0f, 0f, 72.5f), 1.15f),
                (new Vector3(-5.2f, 0f, 72.5f), 1.10f),
                (new Vector3(5.2f, 0f, 72.5f), 1.10f),
                (new Vector3(12.0f, 0f, 72.5f), 1.15f),
                (new Vector3(24.0f, 0f, 72.5f), 1.12f),
                (new Vector3(-24.0f, 0f, 83.5f), 1.08f),
                (new Vector3(-8.0f, 0f, 83.5f), 1.12f),
                (new Vector3(8.0f, 0f, 83.5f), 1.12f),
                (new Vector3(24.0f, 0f, 83.5f), 1.08f),
            };
            for (int i = 0; i < treePlacements.Length; i++)
            {
                GameObject tree = new GameObject($"Hanoi_StreetTree_{i + 1}");
                tree.transform.SetParent(hanoiRoot.transform, false);
                tree.transform.position = treePlacements[i].pos;
                tree.transform.rotation = Quaternion.Euler(0f, (i * 53f) % 360f, 0f);
                tree.transform.localScale = Vector3.one * treePlacements[i].scale;

                InstantiateModel("SM_Hanoi_StreetTree.fbx", tree.transform, Vector3.zero);
                AddBoxColliderChild(tree.transform, "Col_Planter", new Vector3(0f, 0.20f, 0f), new Vector3(1.45f, 0.40f, 1.45f));
                AddBoxColliderChild(tree.transform, "Col_Trunk", new Vector3(0f, 1.4f, 0f), new Vector3(0.45f, 2.8f, 0.45f));
            }
        }

        private static void SpawnHanoiPhoShop(Transform parent, string name, Vector3 pos, float rotY)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            InstantiateModel("SM_Hanoi_PhoShop.fbx", go.transform, Vector3.zero);

            AddBoxColliderChild(go.transform, "Col_Floor", new Vector3(0f, 0.12f, 0f), new Vector3(5.2f, 0.24f, 5.4f));
            AddBoxColliderChild(go.transform, "Col_BackWall", new Vector3(-2.42f, 1.85f, 0f), new Vector3(0.24f, 3.3f, 5.2f));
            AddBoxColliderChild(go.transform, "Col_SideL", new Vector3(0f, 1.85f, -2.55f), new Vector3(5.0f, 3.3f, 0.24f));
            AddBoxColliderChild(go.transform, "Col_SideR", new Vector3(0f, 1.85f, 2.55f), new Vector3(5.0f, 3.3f, 0.24f));
            AddBoxColliderChild(go.transform, "Col_InoxCounter", new Vector3(1.15f, 0.68f, 1.05f), new Vector3(0.92f, 1.35f, 2.35f));

            AddPointLight(go.transform, "Pho_InteriorLight", new Vector3(0.6f, 2.55f, 0f), new Color(1.0f, 0.92f, 0.78f), 1.1f, 7.5f);
            AddPointLight(go.transform, "Pho_SignLight", new Vector3(2.95f, 2.75f, 0f), new Color(1.0f, 0.88f, 0.68f), 0.7f, 5.5f);
        }

        private static void SpawnHanoiTraDaVeSo(Transform parent, string name, Vector3 pos, float rotY)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            InstantiateModel("SM_Hanoi_TraDaVeSo.fbx", go.transform, Vector3.zero);

            AddBoxColliderChild(go.transform, "Col_VeSoDesk", new Vector3(-0.45f, 0.55f, 0.55f), new Vector3(0.76f, 1.1f, 1.44f));
            AddBoxColliderChild(go.transform, "Col_PlasticTable", new Vector3(0.58f, 0.25f, -0.58f), new Vector3(0.62f, 0.5f, 0.62f));
            AddPointLight(go.transform, "TraDa_WarmLight", new Vector3(0.15f, 2.25f, -0.25f), new Color(1.0f, 0.90f, 0.72f), 0.85f, 5.5f);
        }

        private static void SpawnHanoiShophouseBlock(Transform parent, string name, Vector3 pos, float rotY)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            InstantiateModel("SM_Hanoi_ShophouseBlock.fbx", go.transform, Vector3.zero);
            AddBoxColliderChild(go.transform, "Col_Block", new Vector3(0f, 5.5f, 0f), new Vector3(5.4f, 11.0f, 11.8f));
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
                    sun.color = new Color(1.0f, 0.95f, 0.88f);
                    sun.intensity = 1.0f;
                    sun.shadows = LightShadows.Soft;
                    sun.shadowStrength = 0.58f;
                    sunGo.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
                }
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.58f, 0.64f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.66f, 0.62f, 0.58f);
            RenderSettings.ambientGroundColor = new Color(0.36f, 0.34f, 0.32f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.76f, 0.80f, 0.84f);
            RenderSettings.fogDensity = 0.0032f;

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
                    PropertyInfo aaProp = urpCamData.GetType().GetProperty("antialiasing", BindingFlags.Public | BindingFlags.Instance);
                    if (aaProp != null && aaProp.CanWrite)
                    {
                        aaProp.SetValue(urpCamData, System.Enum.ToObject(aaProp.PropertyType, 2)); // SMAA for clean modern building edges
                    }
                }
            }

            EnsureGlobalPostProcessVolume();
            EnsureSSAOOnRenderer();
        }

        private static void EnsureGlobalPostProcessVolume()
        {
            GameObject oldVol = GameObject.Find("Global_PostProcess_Volume");
            if (oldVol != null) Object.DestroyImmediate(oldVol);

            System.Type volType = System.Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
            System.Type profType = System.Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Core.Runtime");
            if (volType == null || profType == null) return;

            Object profile = AssetDatabase.LoadAssetAtPath("Assets/DefaultVolumeProfile.asset", profType);
            if (profile == null) return;

            GameObject volGo = new GameObject("Global_PostProcess_Volume");
            Component vol = volGo.AddComponent(volType);
            PropertyInfo isGlobalProp = volType.GetProperty("isGlobal", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo prioProp = volType.GetProperty("priority", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo profProp = volType.GetProperty("sharedProfile", BindingFlags.Public | BindingFlags.Instance);
            if (isGlobalProp != null && isGlobalProp.CanWrite) isGlobalProp.SetValue(vol, true);
            if (prioProp != null && prioProp.CanWrite) prioProp.SetValue(vol, 1f);
            if (profProp != null && profProp.CanWrite) profProp.SetValue(vol, profile);
        }

        private static void EnsureSSAOOnRenderer()
        {
            try
            {
                ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/_Project/ChoDoLa_URP.asset");
                if (rendererData == null) return;

                System.Type ssaoType = System.Type.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion, Unity.RenderPipelines.Universal.Runtime");
                if (ssaoType == null) return;

                SerializedObject so = new SerializedObject(rendererData);
                SerializedProperty featuresProp = so.FindProperty("m_RendererFeatures");
                if (featuresProp == null) return;

                for (int i = 0; i < featuresProp.arraySize; i++)
                {
                    Object existing = featuresProp.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (existing != null && existing.GetType() == ssaoType) return;
                }

                ScriptableObject ssao = ScriptableObject.CreateInstance(ssaoType);
                ssao.name = "ScreenSpaceAmbientOcclusion";
                FieldInfo settingsField = ssaoType.GetField("m_Settings", BindingFlags.NonPublic | BindingFlags.Instance);
                if (settingsField != null)
                {
                    object settings = settingsField.GetValue(ssao);
                    if (settings != null)
                    {
                        BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                        settings.GetType().GetField("Intensity", bf)?.SetValue(settings, 0.55f);
                        settings.GetType().GetField("Radius", bf)?.SetValue(settings, 0.45f);
                        settings.GetType().GetField("Downsample", bf)?.SetValue(settings, false);
                        settingsField.SetValue(ssao, settings);
                    }
                }

                AssetDatabase.AddObjectToAsset(ssao, rendererData);
                int idx = featuresProp.arraySize;
                featuresProp.InsertArrayElementAtIndex(idx);
                featuresProp.GetArrayElementAtIndex(idx).objectReferenceValue = ssao;

                SerializedProperty mapProp = so.FindProperty("m_RendererFeatureMap");
                if (mapProp != null)
                {
                    mapProp.InsertArrayElementAtIndex(idx);
                    mapProp.GetArrayElementAtIndex(idx).longValue = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ssao, out string _, out long localId) ? localId : 0L;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                MethodInfo dirtyMethod = rendererData.GetType().GetMethod("SetDirty", BindingFlags.Public | BindingFlags.Instance);
                dirtyMethod?.Invoke(rendererData, null);
                EditorUtility.SetDirty(rendererData);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ChoDoLa] Optional SSAO setup skipped: {ex.Message}");
            }
        }

        private static void EnsureEventSystem()
        {
            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
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
