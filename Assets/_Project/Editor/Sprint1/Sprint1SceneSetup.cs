#if UNITY_EDITOR
using System.Reflection;
using ChoDoLa.Contracts;
using ChoDoLa.Interaction;
using ChoDoLa.Player;
using ChoDoLa.Sprint1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ChoDoLa.Sprint1.Editor
{
    /// <summary>
    /// Clean-slate Stylized Low Poly Chợ Đồ Lạ scene builder matching concept_lowpoly_map_overview_birdseye_1795435659758.jpg 1:1.
    /// Separates Nhà Niêm Phong (blind-box display + Dì Mận counter) from Tiệm Mở Hòm (dedicated Unboxing Table)
    /// and wires PlayerBoxCarrier + UnboxingTableDropZone so players physically carry purchased boxes with both hands.
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
            Debug.Log("[ChoDoLa] Clean Stylized Low Poly Chợ Đồ Lạ scene built matching overview layout.");
        }

        public static void BuildInCurrentScene(bool saveScene = false)
        {
            GameObject oldProps = GameObject.Find("OpenTable_Props");
            if (oldProps != null) Object.DestroyImmediate(oldProps);

            GameObject oldRoot = GameObject.Find(RootName);
            if (oldRoot != null) Object.DestroyImmediate(oldRoot);

            GameObject sprint0Hud = GameObject.Find("HUD_Canvas");
            if (sprint0Hud != null) sprint0Hud.SetActive(false);

            HideLegacyTownObjects();

            GameObject root = new GameObject(RootName);

            Sprint1MarketService service = root.AddComponent<Sprint1MarketService>();
            ArtifactInspector3D inspector = root.AddComponent<ArtifactInspector3D>();
            OpenTableManager tableMgr = root.AddComponent<OpenTableManager>();
            PlayerBoxCarrier boxCarrier = root.AddComponent<PlayerBoxCarrier>();
            Sprint1HudController hud = root.AddComponent<Sprint1HudController>();
            Sprint1AudioManager audioMgr = root.AddComponent<Sprint1AudioManager>();
            root.AddComponent<MarketAmbientAnimator>();

            GameObject player = GameObject.Find("Player");
            FirstPersonController fpc = player != null ? player.GetComponent<FirstPersonController>() : null;
            PlayerInteractor interactor = player != null ? player.GetComponent<PlayerInteractor>() : null;

            BuildStreetPlaza(root.transform);
            BuildNhaNiemPhong(root.transform, new Vector3(-12.0f, 0f, 18.0f), service, hud);
            BuildTiemMoHom(root.transform, new Vector3(-11.5f, 0f, 29.3f), tableMgr, boxCarrier, out Transform[] boxSlots, out Transform inspectAnchor);
            BuildStallPho(root.transform, new Vector3(-5.6f, 0.08f, 25.4f));
            BuildAnhBay(root.transform, new Vector3(11.5f, 0f, 18.0f), hud);
            BuildStallTraDa(root.transform, new Vector3(5.6f, 0.08f, 14.5f));
            BuildKimKhuy(root.transform, new Vector3(11.5f, 0f, 28.2f), hud);
            BuildForegroundStreetTrees(root.transform);

            if (player != null)
            {
                foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
                {
                    if (t != null && t.name == "FirstPerson_CarryBoxRig")
                    {
                        Object.DestroyImmediate(t.gameObject);
                    }
                }

                CharacterController cc = player.GetComponent<CharacterController>();
                bool wasEnabled = cc != null && cc.enabled;
                if (cc != null) cc.enabled = false;
                player.transform.position = new Vector3(0f, 1.05f, 1.2f);
                player.transform.rotation = Quaternion.identity;
                if (cc != null) cc.enabled = wasEnabled;
            }

            inspector.Configure(inspectAnchor, fpc, interactor, hud);
            boxCarrier.Configure(fpc, inspector);
            tableMgr.Configure(service, inspector, hud, boxSlots, inspectAnchor, boxCarrier);
            hud.Configure(service, inspector, tableMgr, fpc, interactor, boxCarrier);
            audioMgr.Configure(service);

            ConfigureLowPolyDaylight();
            EnsureEventSystem();

            if (saveScene)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
        }

        private static void HideLegacyTownObjects()
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

            GameObject townRoot = GameObject.Find("MarketTown");
            if (townRoot == null) return;

            foreach (Transform child in townRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == townRoot.transform) continue;
                foreach (Renderer r in child.GetComponents<Renderer>()) r.enabled = false;
                foreach (Collider c in child.GetComponents<Collider>()) c.enabled = false;
                foreach (Light l in child.GetComponents<Light>()) l.enabled = false;
            }
        }

        private static void BuildStreetPlaza(Transform parent)
        {
            GameObject plaza = new GameObject("LP_Street_Plaza");
            plaza.transform.SetParent(parent, false);
            plaza.transform.position = Vector3.zero;

            InstantiateModel("LP_StreetPlaza.fbx", plaza.transform, Vector3.zero);

            // Clean flat safety ground collider (Y top = 0) + raised sidewalks + foreground rocky islands
            AddBoxColliderChild(plaza.transform, "Col_StreetGround", new Vector3(0f, -0.10f, 22.0f), new Vector3(95f, 0.20f, 95f));
            AddBoxColliderChild(plaza.transform, "Col_Sidewalk_W", new Vector3(-13.0f, 0.04f, 23.0f), new Vector3(18.0f, 0.08f, 29.0f));
            AddBoxColliderChild(plaza.transform, "Col_Sidewalk_E", new Vector3(13.0f, 0.04f, 23.0f), new Vector3(18.0f, 0.08f, 29.0f));
            AddBoxColliderChild(plaza.transform, "Col_FG_Island", new Vector3(-0.3f, 0.22f, 5.2f), new Vector3(2.25f, 0.44f, 2.75f));
            AddBoxColliderChild(plaza.transform, "Col_FG_Tuft", new Vector3(-1.2f, 0.20f, 2.6f), new Vector3(1.60f, 0.40f, 1.60f));

            // Invisible perimeter bounds keeping the player cleanly inside the pedestrian market street
            AddBoxColliderChild(plaza.transform, "Col_Bound_W", new Vector3(-19.0f, 3.0f, 22.0f), new Vector3(1.0f, 6.0f, 60.0f));
            AddBoxColliderChild(plaza.transform, "Col_Bound_E", new Vector3(19.0f, 3.0f, 22.0f), new Vector3(1.0f, 6.0f, 60.0f));
            AddBoxColliderChild(plaza.transform, "Col_Bound_S", new Vector3(0f, 3.0f, -3.5f), new Vector3(40.0f, 6.0f, 1.0f));
            AddBoxColliderChild(plaza.transform, "Col_Bound_N", new Vector3(0f, 3.0f, 46.0f), new Vector3(40.0f, 6.0f, 1.0f));
        }

        private static void BuildNhaNiemPhong(Transform parent, Vector3 pos, Sprint1MarketService service, Sprint1HudController hud)
        {
            GameObject house = new GameObject("Shop_NhaNiemPhong");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            InstantiateModel("LP_Shop_NhaNiemPhong.fbx", house.transform, Vector3.zero);
            AddWalkInHouseColliders(house.transform, facingEast: true, depthX: 10.0f, widthZ: 12.0f, doorWidth: 3.0f, doorCenterZ: 0f, doorHeight: 2.95f);

            // Timber porch deck, 2 wide stairs, and side charcoal handrails aligned 1:1 to LP_Shop_NhaNiemPhong
            AddBoxColliderChild(house.transform, "Col_PorchDeck", new Vector3(5.52f, 0.12f, 0f), new Vector3(1.06f, 0.24f, 4.30f));
            AddBoxColliderChild(house.transform, "Col_StairUpper", new Vector3(6.23f, 0.08f, 0f), new Vector3(0.38f, 0.16f, 3.40f));
            AddBoxColliderChild(house.transform, "Col_StairLower", new Vector3(6.61f, 0.05f, 0f), new Vector3(0.38f, 0.10f, 3.40f));
            AddBoxColliderChild(house.transform, "Col_Handrail_S", new Vector3(5.90f, 0.62f, -1.72f), new Vector3(1.80f, 1.00f, 0.14f));
            AddBoxColliderChild(house.transform, "Col_Handrail_N", new Vector3(5.90f, 0.62f, 1.72f), new Vector3(1.80f, 1.00f, 0.14f));

            AddBoxColliderChild(house.transform, "Col_Counter", new Vector3(-3.10f, 0.53f, 0f), new Vector3(1.10f, 1.06f, 3.90f));
            AddBoxColliderChild(house.transform, "Col_Cabinet", new Vector3(-4.52f, 1.78f, 0f), new Vector3(0.55f, 3.30f, 4.40f));
            AddBoxColliderChild(house.transform, "Col_Credenza_S", new Vector3(1.20f, 0.62f, -5.25f), new Vector3(4.70f, 0.78f, 0.72f));
            AddBoxColliderChild(house.transform, "Col_Credenza_N", new Vector3(1.20f, 0.62f, 5.25f), new Vector3(4.70f, 0.78f, 0.72f));
            AddBoxColliderChild(house.transform, "Col_CrateL", new Vector3(5.56f, 0.36f, -5.20f), new Vector3(0.64f, 0.64f, 0.64f));
            AddBoxColliderChild(house.transform, "Col_CrateR", new Vector3(5.56f, 0.36f, 5.20f), new Vector3(0.64f, 0.64f, 0.64f));

            // 6 Display Boxes on the 6 octagonal pedestals (inspect-only; purchase via Dì Mận counter)
            var offers = Sprint1Catalog.CreateDailyOffers();
            Vector3[] pedCoords =
            {
                new Vector3(2.0f, 1.06f, -2.5f),
                new Vector3(0.0f, 1.06f, -2.5f),
                new Vector3(-1.6f, 1.06f, -2.5f),
                new Vector3(2.0f, 1.06f, 2.5f),
                new Vector3(0.0f, 1.06f, 2.5f),
                new Vector3(-1.6f, 1.06f, 2.5f),
            };
            for (int i = 0; i < pedCoords.Length; i++)
            {
                AddBoxColliderChild(house.transform, $"Col_Pedestal_{i + 1}", new Vector3(pedCoords[i].x, 0.53f, pedCoords[i].z), new Vector3(0.90f, 1.06f, 0.90f));
                if (i >= offers.Count) continue;

                BoxOfferView offer = offers[i];
                GameObject disp = new GameObject($"DisplayBox_{i + 1}_{offer.VisualVariantKey}");
                disp.transform.SetParent(house.transform, false);
                disp.transform.localPosition = pedCoords[i];
                disp.transform.localRotation = Quaternion.Euler(0f, pedCoords[i].z < 0f ? 0f : 180f, 0f);

                MarketVisualFactory.CreateBoxModel(disp.transform, offer.VisualVariantKey, out _, out _);

                BoxCollider col = disp.AddComponent<BoxCollider>();
                col.center = new Vector3(0f, 0.12f, 0f);
                col.size = new Vector3(0.42f, 0.28f, 0.34f);

                BlindBoxProp prop = disp.AddComponent<BlindBoxProp>();
                prop.ConfigureCounterDisplay(offer.OfferId, offer.DisplayName, offer.Price, service, hud);
            }

            // Cohesive flat-shaded Low-Poly showcases flush on shelf/counter tops behind/on Dì Mận counter
            (string file, Vector3 lpos)[] cabinetProps =
            {
                ("LP_Box_H1.fbx",  new Vector3(-4.36f, 1.050f, -1.56f)),
                ("LP_Box_H2.fbx",  new Vector3(-4.36f, 1.050f,  1.56f)),
                ("LP_Item_V02.fbx",new Vector3(-4.42f, 1.050f, -0.52f)),
                ("LP_Item_V04.fbx",new Vector3(-4.42f, 1.050f,  0.52f)),
                ("LP_Item_V05.fbx",new Vector3(-4.42f, 1.750f, -0.52f)),
                ("LP_Item_V08.fbx",new Vector3(-4.42f, 1.750f,  0.52f)),
                ("LP_Item_V06.fbx",new Vector3(-3.08f, 1.070f, -0.55f)),
                ("LP_Item_V10.fbx",new Vector3(-3.08f, 1.070f,  0.55f)),
            };
            for (int i = 0; i < cabinetProps.Length; i++)
            {
                SpawnDecorProp(house.transform, $"CabinetShowcase_{i + 1}", cabinetProps[i].file, cabinetProps[i].lpos, -90f);
            }

            GameObject npcDiMan = SpawnDecorProp(house.transform, "NPC_DiMan", "SM_NPC_DiMan.fbx", new Vector3(-3.95f, 0.24f, 0f), -90f);
            if (npcDiMan != null)
            {
                BoxCollider nbc = npcDiMan.AddComponent<BoxCollider>();
                nbc.center = new Vector3(0f, 0.95f, 0f);
                nbc.size = new Vector3(0.75f, 1.90f, 0.75f);
                CounterInteractable npcInteract = npcDiMan.AddComponent<CounterInteractable>();
                npcInteract.Configure(hud);
            }

            AddPointLight(house.transform, "NhaNiemPhong_MainWarm", new Vector3(0.6f, 3.35f, 0f), new Color(1.0f, 0.86f, 0.62f), 1.35f, 12.5f, softShadows: true);
            AddPointLight(house.transform, "Counter_WarmFill", new Vector3(-3.0f, 2.35f, 0f), new Color(1.0f, 0.80f, 0.48f), 1.05f, 7.5f);
            AddPointLight(house.transform, "Cabinet_Glow_L", new Vector3(-3.98f, 2.30f, -1.05f), new Color(1.0f, 0.80f, 0.48f), 0.45f, 4.2f);
            AddPointLight(house.transform, "Cabinet_Glow_R", new Vector3(-3.98f, 2.30f, 1.05f), new Color(1.0f, 0.80f, 0.48f), 0.45f, 4.2f);
            AddPointLight(house.transform, "Porch_LanternWarm_Nha", new Vector3(5.55f, 2.55f, 0f), new Color(1.0f, 0.82f, 0.52f), 0.55f, 5.5f);
        }

        private static void BuildTiemMoHom(
            Transform parent,
            Vector3 pos,
            OpenTableManager tableMgr,
            PlayerBoxCarrier boxCarrier,
            out Transform[] boxSlots,
            out Transform inspectAnchor)
        {
            GameObject house = new GameObject("Shop_TiemMoHom");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            InstantiateModel("LP_Shop_TiemMoHom.fbx", house.transform, Vector3.zero);
            AddWalkInHouseColliders(house.transform, facingEast: true, depthX: 9.0f, widthZ: 10.0f, doorWidth: 2.6f, doorCenterZ: 0.8f, doorHeight: 2.85f);

            // Timber porch deck, 2 wide stairs, and side charcoal handrails aligned to LP_Shop_TiemMoHom
            AddBoxColliderChild(house.transform, "Col_PorchDeck", new Vector3(5.06f, 0.12f, 0.80f), new Vector3(1.16f, 0.24f, 3.76f));
            AddBoxColliderChild(house.transform, "Col_StairUpper", new Vector3(5.84f, 0.08f, 0.80f), new Vector3(0.38f, 0.16f, 3.10f));
            AddBoxColliderChild(house.transform, "Col_StairLower", new Vector3(6.22f, 0.05f, 0.80f), new Vector3(0.38f, 0.10f, 3.10f));
            AddBoxColliderChild(house.transform, "Col_Handrail_S", new Vector3(5.51f, 0.62f, -0.82f), new Vector3(1.80f, 1.00f, 0.14f));
            AddBoxColliderChild(house.transform, "Col_Handrail_N", new Vector3(5.51f, 0.62f, 2.42f), new Vector3(1.80f, 1.00f, 0.14f));

            AddBoxColliderChild(house.transform, "Col_AlchemyRack", new Vector3(-3.85f, 1.50f, 0f), new Vector3(0.72f, 3.00f, 5.20f));
            AddBoxColliderChild(house.transform, "Col_SideBench_S", new Vector3(-0.40f, 0.56f, -4.28f), new Vector3(3.92f, 1.12f, 0.86f));
            AddBoxColliderChild(house.transform, "Col_NorthCrates", new Vector3(-2.70f, 0.56f, 4.16f), new Vector3(2.50f, 1.12f, 0.90f));
            AddBoxColliderChild(house.transform, "Col_UnboxingTable", new Vector3(-0.30f, 0.52f, 0f), new Vector3(1.52f, 1.04f, 3.30f));

            // Dedicated DropZone interactable on the Velvet Unboxing Table (raycast collider active ONLY when carrying a box!)
            GameObject dropZoneGo = new GameObject("UnboxingTable_DropZone");
            dropZoneGo.transform.SetParent(house.transform, false);
            BoxCollider dropCol = dropZoneGo.AddComponent<BoxCollider>();
            dropCol.isTrigger = true;
            dropCol.center = new Vector3(-0.30f, 0.82f, 0f);
            dropCol.size = new Vector3(1.78f, 1.25f, 3.45f);
            UnboxingTableDropZone dropZone = dropZoneGo.AddComponent<UnboxingTableDropZone>();
            dropZone.Configure(tableMgr, boxCarrier);

            // 3 gold-trimmed velvet tray slots along Z facing East (+X) toward the player walking in
            boxSlots = new Transform[3];
            float[] slotZ = { -0.95f, 0f, 0.95f };
            for (int i = 0; i < 3; i++)
            {
                GameObject s = new GameObject($"BoxSlot_{i + 1}");
                s.transform.SetParent(house.transform, false);
                s.transform.localPosition = new Vector3(-0.30f, 1.073f, slotZ[i]);
                s.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                boxSlots[i] = s.transform;
            }

            GameObject anchor = new GameObject("InspectAnchor");
            anchor.transform.SetParent(house.transform, false);
            anchor.transform.localPosition = new Vector3(0.28f, 1.20f, 0f);
            anchor.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            inspectAnchor = anchor.transform;

            // Showcase 2 cohesive Flat-Shaded Low Poly hero pieces on the South craft bench
            SpawnDecorProp(house.transform, "BenchShowcase_V01", "LP_Item_V01.fbx", new Vector3(-1.85f, 1.12f, -4.25f), 25f);
            SpawnDecorProp(house.transform, "BenchShowcase_V03", "LP_Item_V03.fbx", new Vector3(-0.45f, 1.12f, -4.25f), 0f);

            // Cozy golden-amber workshop & porch lighting contrasting against cool afternoon skylight
            AddPointLight(house.transform, "TiemMoHom_PendantMain", new Vector3(-0.30f, 2.90f, 0f), new Color(1.0f, 0.84f, 0.58f), 1.35f, 10.5f, softShadows: true);
            AddPointLight(house.transform, "Workbench_BankerGlow", new Vector3(-0.75f, 1.72f, -0.85f), new Color(1.0f, 0.76f, 0.42f), 0.95f, 5.2f);
            AddPointLight(house.transform, "AlchemyRack_Accent", new Vector3(-3.00f, 2.15f, 0.20f), new Color(1.0f, 0.80f, 0.50f), 0.75f, 6.5f);
            AddPointLight(house.transform, "Porch_LanternWarm", new Vector3(5.10f, 2.48f, 0.80f), new Color(1.0f, 0.82f, 0.52f), 0.42f, 5.0f);
        }

        private static void BuildStallPho(Transform parent, Vector3 pos)
        {
            GameObject stall = new GameObject("Stall_Pho");
            stall.transform.SetParent(parent, false);
            stall.transform.position = pos;
            InstantiateModel("LP_Stall_Pho.fbx", stall.transform, Vector3.zero);
            AddBoxColliderChild(stall.transform, "Col_PhoCart", new Vector3(0f, 0.55f, 0f), new Vector3(1.15f, 1.10f, 2.05f));
        }

        private static void BuildStallTraDa(Transform parent, Vector3 pos)
        {
            GameObject stall = new GameObject("Stall_TraDa");
            stall.transform.SetParent(parent, false);
            stall.transform.position = pos;
            InstantiateModel("LP_Stall_TraDa.fbx", stall.transform, Vector3.zero);
            AddBoxColliderChild(stall.transform, "Col_TraDaCart", new Vector3(0f, 0.55f, 0f), new Vector3(1.15f, 1.10f, 2.05f));
        }

        private static void BuildAnhBay(Transform parent, Vector3 pos, Sprint1HudController hud)
        {
            GameObject house = new GameObject("Shop_AnhBay");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            InstantiateModel("LP_Shop_AnhBay.fbx", house.transform, Vector3.zero);
            AddWalkInHouseColliders(house.transform, facingEast: false, depthX: 9.0f, widthZ: 10.0f, doorWidth: 2.6f, doorCenterZ: 1.4f, doorHeight: 2.80f);

            // Timber porch deck, 2 wide stairs, and side charcoal handrails aligned to LP_Shop_AnhBay
            AddBoxColliderChild(house.transform, "Col_PorchDeck", new Vector3(-5.06f, 0.12f, 1.40f), new Vector3(1.16f, 0.24f, 3.80f));
            AddBoxColliderChild(house.transform, "Col_StairUpper", new Vector3(-5.84f, 0.08f, 1.40f), new Vector3(0.38f, 0.16f, 3.10f));
            AddBoxColliderChild(house.transform, "Col_StairLower", new Vector3(-6.22f, 0.05f, 1.40f), new Vector3(0.38f, 0.10f, 3.10f));
            AddBoxColliderChild(house.transform, "Col_Handrail_S", new Vector3(-5.51f, 0.62f, -0.22f), new Vector3(1.80f, 1.00f, 0.14f));
            AddBoxColliderChild(house.transform, "Col_Handrail_N", new Vector3(-5.51f, 0.62f, 3.02f), new Vector3(1.80f, 1.00f, 0.14f));

            AddBoxColliderChild(house.transform, "Col_VaultWallCab_S", new Vector3(3.85f, 1.40f, -3.15f), new Vector3(0.70f, 2.80f, 2.15f));
            AddBoxColliderChild(house.transform, "Col_VaultWallCab_N", new Vector3(3.85f, 1.40f, 3.15f), new Vector3(0.70f, 2.80f, 2.15f));
            AddBoxColliderChild(house.transform, "Col_PokerTable", new Vector3(-1.65f, 0.45f, -2.85f), new Vector3(1.35f, 0.90f, 1.35f));
            AddBoxColliderChild(house.transform, "Col_FloorSafe", new Vector3(0.0f, 0.45f, -4.34f), new Vector3(0.65f, 0.90f, 0.65f));
            AddBoxColliderChild(house.transform, "Col_Credenza_N", new Vector3(1.70f, 0.52f, 4.30f), new Vector3(2.35f, 1.04f, 0.72f));
            AddBoxColliderChild(house.transform, "Col_NorthCrates", new Vector3(-3.00f, 0.52f, 4.20f), new Vector3(2.10f, 1.04f, 0.85f));

            GameObject counterZone = new GameObject("AnhBay_InteractCounter");
            counterZone.transform.SetParent(house.transform, false);
            BoxCollider abc = counterZone.AddComponent<BoxCollider>();
            abc.center = new Vector3(2.00f, 0.56f, 0f);
            abc.size = new Vector3(1.20f, 1.12f, 3.60f);
            InnLedgerInteractable ledger = counterZone.AddComponent<InnLedgerInteractable>();
            ledger.Configure(hud);

            GameObject npcAnhBay = SpawnDecorProp(house.transform, "NPC_AnhBay", "SM_NPC_AnhBay.fbx", new Vector3(2.96f, 0.24f, 0f), 90f);
            if (npcAnhBay != null)
            {
                BoxCollider nbc = npcAnhBay.AddComponent<BoxCollider>();
                nbc.center = new Vector3(0f, 0.95f, 0f);
                nbc.size = new Vector3(0.75f, 1.90f, 0.75f);
                InnLedgerInteractable npcInteract = npcAnhBay.AddComponent<InnLedgerInteractable>();
                npcInteract.Configure(hud);
            }

            // Moody Crimson-Amber Syndicate Boss Office Lighting (high contrast without crushing dark details)
            AddPointLight(house.transform, "AnhBay_MainWarm", new Vector3(-0.4f, 3.25f, 0.2f), new Color(1.0f, 0.74f, 0.46f), 1.15f, 12.5f, softShadows: true);
            AddPointLight(house.transform, "AnhBay_CounterFill", new Vector3(1.65f, 2.35f, 0f), new Color(1.0f, 0.80f, 0.48f), 1.05f, 6.8f);
            AddPointLight(house.transform, "AnhBay_CrimsonMood", new Vector3(-1.60f, 2.35f, -2.60f), new Color(0.95f, 0.26f, 0.18f), 0.62f, 6.5f);
            AddPointLight(house.transform, "AnhBay_VaultAccent", new Vector3(3.35f, 2.25f, 0f), new Color(1.0f, 0.78f, 0.46f), 0.60f, 5.2f);
            AddPointLight(house.transform, "Porch_LanternRed_AB", new Vector3(-5.15f, 2.52f, 1.40f), new Color(1.0f, 0.44f, 0.22f), 0.65f, 6.0f);
        }

        private static void BuildKimKhuy(Transform parent, Vector3 pos, Sprint1HudController hud)
        {
            GameObject house = new GameObject("Shop_KimKhuy");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            InstantiateModel("LP_Shop_KimKhuy.fbx", house.transform, Vector3.zero);
            AddWalkInHouseColliders(house.transform, facingEast: false, depthX: 9.0f, widthZ: 9.0f, doorWidth: 2.6f, doorCenterZ: 0f, doorHeight: 2.75f);

            // Exterior porch, 2 stairs & charcoal handrails aligned to LP_Shop_KimKhuy
            AddBoxColliderChild(house.transform, "Col_PorchDeck", new Vector3(-5.06f, 0.12f, 0f), new Vector3(1.16f, 0.24f, 3.80f));
            AddBoxColliderChild(house.transform, "Col_StairUpper", new Vector3(-5.84f, 0.08f, 0f), new Vector3(0.38f, 0.16f, 3.10f));
            AddBoxColliderChild(house.transform, "Col_StairLower", new Vector3(-6.22f, 0.05f, 0f), new Vector3(0.38f, 0.10f, 3.10f));
            AddBoxColliderChild(house.transform, "Col_Handrail_S", new Vector3(-5.51f, 0.62f, -1.62f), new Vector3(1.80f, 1.00f, 0.14f));
            AddBoxColliderChild(house.transform, "Col_Handrail_N", new Vector3(-5.51f, 0.62f, 1.62f), new Vector3(1.80f, 1.00f, 0.14f));

            // Floor 1 workshop equipment colliders (leaving CraftBench central aisle & stair bottom wide open)
            AddBoxColliderChild(house.transform, "Col_Forge", new Vector3(-1.10f, 1.00f, -3.72f), new Vector3(1.60f, 2.00f, 0.96f));
            AddBoxColliderChild(house.transform, "Col_Anvil", new Vector3(-2.65f, 0.50f, -2.42f), new Vector3(0.75f, 1.00f, 1.15f));
            AddBoxColliderChild(house.transform, "Col_F1_SouthBench", new Vector3(1.50f, 0.52f, -3.86f), new Vector3(1.82f, 1.04f, 0.72f));
            AddBoxColliderChild(house.transform, "Col_SpecCab_S", new Vector3(3.85f, 1.45f, -2.75f), new Vector3(0.66f, 2.90f, 1.80f));
            AddBoxColliderChild(house.transform, "Col_SpecCab_N", new Vector3(3.85f, 1.45f, 2.75f), new Vector3(0.66f, 2.90f, 1.80f));
            AddBoxColliderChild(house.transform, "Col_CraftGearPed_S", new Vector3(1.80f, 0.48f, -2.06f), new Vector3(0.52f, 0.96f, 0.52f));
            AddBoxColliderChild(house.transform, "Col_CraftGearPed_N", new Vector3(1.80f, 0.48f, 2.06f), new Vector3(0.52f, 0.96f, 0.52f));
            AddBoxColliderChild(house.transform, "Col_UnderStairCargo", new Vector3(-0.15f, 0.48f, 3.76f), new Vector3(1.85f, 0.96f, 0.72f));
            AddBoxColliderChild(house.transform, "Col_F2Post_SE", new Vector3(0.85f, 1.50f, -2.55f), new Vector3(0.24f, 3.00f, 0.24f));
            AddBoxColliderChild(house.transform, "Col_F2Post_NE", new Vector3(0.85f, 1.50f, 3.25f), new Vector3(0.24f, 3.00f, 0.24f));
            AddBoxColliderChild(house.transform, "Col_F2Post_SW", new Vector3(-4.12f, 1.50f, -2.55f), new Vector3(0.24f, 3.00f, 0.24f));

            // Walkable Smooth Stair Ramp up to Floor 2 (X=-3.40, Y=0.253 -> X=+0.80, Y=3.250 along Z=3.72)
            GameObject stairRamp = new GameObject("Col_StairRamp");
            stairRamp.transform.SetParent(house.transform, false);
            stairRamp.transform.localPosition = new Vector3(-1.265f, 1.702f, 3.72f);
            stairRamp.transform.localRotation = Quaternion.Euler(0f, 0f, 35.51f);
            BoxCollider srCol = stairRamp.AddComponent<BoxCollider>();
            srCol.size = new Vector3(5.16f, 0.12f, 0.86f);

            GameObject stairRail = new GameObject("Col_StairRail");
            stairRail.transform.SetParent(house.transform, false);
            stairRail.transform.localPosition = new Vector3(-1.30f, 2.25f, 3.28f);
            stairRail.transform.localRotation = Quaternion.Euler(0f, 0f, 35.51f);
            BoxCollider srlCol = stairRail.AddComponent<BoxCollider>();
            srlCol.size = new Vector3(4.30f, 1.00f, 0.14f);

            // Walkable Floor 2 Drafting & Observatory Loft Deck + Guard Rails + F2 Walls + Loft Furniture
            AddBoxColliderChild(house.transform, "Col_F2_EastDeck", new Vector3(2.525f, 3.17f, 0f), new Vector3(3.45f, 0.16f, 8.50f));
            AddBoxColliderChild(house.transform, "Col_F2_SouthCatwalk", new Vector3(-1.725f, 3.17f, -3.40f), new Vector3(5.05f, 0.16f, 1.70f));
            AddBoxColliderChild(house.transform, "Col_F2_Rail_East", new Vector3(0.85f, 3.72f, 0.35f), new Vector3(0.14f, 0.98f, 5.82f));
            AddBoxColliderChild(house.transform, "Col_F2_Rail_Catwalk", new Vector3(-1.70f, 3.72f, -2.58f), new Vector3(5.10f, 0.98f, 0.14f));
            AddBoxColliderChild(house.transform, "Col_F2_Wall_W", new Vector3(-4.36f, 4.80f, 0f), new Vector3(0.28f, 3.00f, 9.00f));
            AddBoxColliderChild(house.transform, "Col_F2_Wall_E", new Vector3(4.36f, 4.80f, 0f), new Vector3(0.28f, 3.00f, 9.00f));
            AddBoxColliderChild(house.transform, "Col_F2_Wall_S", new Vector3(0f, 4.80f, -4.36f), new Vector3(9.00f, 3.00f, 0.28f));
            AddBoxColliderChild(house.transform, "Col_F2_Wall_N", new Vector3(0f, 4.80f, 4.36f), new Vector3(9.00f, 3.00f, 0.28f));
            AddBoxColliderChild(house.transform, "Col_F2_Drafting", new Vector3(2.75f, 3.72f, 1.55f), new Vector3(0.95f, 0.95f, 1.50f));
            AddBoxColliderChild(house.transform, "Col_F2_ResTable", new Vector3(2.75f, 3.70f, -1.65f), new Vector3(1.25f, 0.90f, 2.05f));
            AddBoxColliderChild(house.transform, "Col_F2_Credenza", new Vector3(3.82f, 3.68f, 0f), new Vector3(0.72f, 0.85f, 2.00f));
            AddBoxColliderChild(house.transform, "Col_F2_Telescope", new Vector3(-3.15f, 3.85f, -3.22f), new Vector3(0.85f, 1.25f, 0.85f));
            AddBoxColliderChild(house.transform, "Col_F2_ForgeFlue", new Vector3(-1.10f, 4.10f, -3.90f), new Vector3(0.62f, 2.20f, 0.62f));

            GameObject interactZone = new GameObject("KimKhuy_InteractBench");
            interactZone.transform.SetParent(house.transform, false);
            BoxCollider kbc = interactZone.AddComponent<BoxCollider>();
            kbc.center = new Vector3(1.80f, 0.56f, 0f);
            kbc.size = new Vector3(1.22f, 1.12f, 3.55f);
            KimKhuyInteractable kimInteract = interactZone.AddComponent<KimKhuyInteractable>();
            kimInteract.Configure(hud);

            GameObject npcKimKhuy = SpawnDecorProp(house.transform, "NPC_KimKhuy", "SM_NPC_KimKhuy.fbx", new Vector3(2.90f, 0.24f, 0f), 90f);
            if (npcKimKhuy != null)
            {
                BoxCollider knc = npcKimKhuy.AddComponent<BoxCollider>();
                knc.center = new Vector3(0f, 0.95f, 0f);
                knc.size = new Vector3(0.75f, 1.90f, 0.75f);
                KimKhuyInteractable npcInteract = npcKimKhuy.AddComponent<KimKhuyInteractable>();
                npcInteract.Configure(hud);
            }

            string[] displayModels =
            {
                "Artifact_CP1.fbx",
                "Artifact_CP2.fbx",
                "Artifact_CP3.fbx",
                "Artifact_TC1.fbx",
                "Artifact_TC2.fbx",
                "Artifact_TC3.fbx"
            };
            for (int i = 0; i < displayModels.Length; i++)
            {
                float zOff = -1.25f + i * 0.50f;
                GameObject disp = SpawnDecorProp(house.transform, $"KimKhuy_Display_{i + 1}", displayModels[i], new Vector3(1.75f, 1.14f, zOff), 90f);
                if (disp != null) disp.transform.localScale = Vector3.one * 0.85f;
            }

            // Cozy 2-Story Artificer Atelier Lighting (Forge warm amber, F1 under-loft pendants, F2 atrium & loft daylight)
            AddPointLight(house.transform, "KimKhuy_F1_Main", new Vector3(-0.9f, 2.95f, 0.2f), new Color(1.0f, 0.86f, 0.64f), 1.15f, 10.5f, softShadows: true);
            AddPointLight(house.transform, "KimKhuy_ForgeGlow", new Vector3(-1.10f, 1.65f, -3.15f), new Color(1.0f, 0.48f, 0.16f), 1.35f, 6.2f);
            AddPointLight(house.transform, "KimKhuy_CounterFill", new Vector3(1.65f, 2.55f, 0f), new Color(1.0f, 0.82f, 0.54f), 0.95f, 6.5f);
            AddPointLight(house.transform, "KimKhuy_F2_Atrium", new Vector3(-1.45f, 4.95f, 0.20f), new Color(1.0f, 0.86f, 0.62f), 1.25f, 11.5f);
            AddPointLight(house.transform, "KimKhuy_F2_Loft", new Vector3(2.45f, 5.45f, 0f), new Color(1.0f, 0.88f, 0.68f), 1.05f, 8.5f);
            AddPointLight(house.transform, "Porch_LanternWarm_KK", new Vector3(-5.15f, 2.50f, 0f), new Color(1.0f, 0.82f, 0.52f), 0.50f, 5.5f);
        }

        private static void BuildForegroundStreetTrees(Transform parent)
        {
            Vector3[] treePositions =
            {
                new Vector3(-9.2f, 0.08f, 9.8f),
                new Vector3(6.4f, 0.08f, 9.8f),
                new Vector3(12.2f, 0.08f, 10.2f),
            };
            for (int i = 0; i < treePositions.Length; i++)
            {
                GameObject tree = new GameObject($"FG_StreetTree_{i + 1}");
                tree.transform.SetParent(parent, false);
                tree.transform.position = treePositions[i];
                tree.transform.rotation = Quaternion.Euler(0f, i * 52f, 0f);

                InstantiateModel("LP_StreetTree.fbx", tree.transform, Vector3.zero);
                AddBoxColliderChild(tree.transform, "Col_Planter", new Vector3(0f, 0.20f, 0f), new Vector3(1.55f, 0.40f, 1.55f));
                AddBoxColliderChild(tree.transform, "Col_Trunk", new Vector3(0f, 1.40f, 0f), new Vector3(0.45f, 2.80f, 0.45f));
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

        private static GameObject SpawnDecorProp(Transform parent, string name, string fbxFile, Vector3 localPos, float rotY)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
            InstantiateModel(fbxFile, go.transform, Vector3.zero);
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

        private static void AddWalkInHouseColliders(
            Transform house,
            bool facingEast,
            float depthX,
            float widthZ,
            float doorWidth,
            float doorCenterZ = 0f,
            float doorHeight = 2.90f)
        {
            float hx = depthX * 0.5f;
            float hz = widthZ * 0.5f;
            float dir = facingEast ? 1f : -1f;

            AddBoxColliderChild(house, "Col_Floor", new Vector3(0f, 0.12f, 0f), new Vector3(depthX, 0.24f, widthZ));
            AddBoxColliderChild(house, "Col_BackWall", new Vector3(-(hx - 0.14f) * dir, 1.8f, 0f), new Vector3(0.28f, 3.6f, widthZ - 0.15f));
            AddBoxColliderChild(house, "Col_SideWall_S", new Vector3(0f, 1.8f, -(hz - 0.14f)), new Vector3(depthX - 0.15f, 3.6f, 0.28f));
            AddBoxColliderChild(house, "Col_SideWall_N", new Vector3(0f, 1.8f, hz - 0.14f), new Vector3(depthX - 0.15f, 3.6f, 0.28f));

            float dMin = doorCenterZ - doorWidth * 0.5f;
            float dMax = doorCenterZ + doorWidth * 0.5f;
            float pierSW = Mathf.Max(0.25f, dMin - (-hz));
            float pierSC = -hz + pierSW * 0.5f;
            float pierNW = Mathf.Max(0.25f, hz - dMax);
            float pierNC = hz - pierNW * 0.5f;

            AddBoxColliderChild(house, "Col_FrontPier_S", new Vector3((hx - 0.14f) * dir, 2.2f, pierSC), new Vector3(0.28f, 4.4f, pierSW));
            AddBoxColliderChild(house, "Col_FrontPier_N", new Vector3((hx - 0.14f) * dir, 2.2f, pierNC), new Vector3(0.28f, 4.4f, pierNW));
            AddBoxColliderChild(house, "Col_DoorLintel", new Vector3((hx - 0.14f) * dir, doorHeight + 0.85f, doorCenterZ), new Vector3(0.28f, 1.7f, doorWidth + 0.12f));

            // Stepped porch colliders from sidewalk (Y=0.08) cleanly up onto floor (Y=0.24)
            AddBoxColliderChild(house, "Col_StepMid", new Vector3((hx + 0.34f) * dir, 0.095f, doorCenterZ), new Vector3(0.68f, 0.19f, doorWidth + 0.2f));
            AddBoxColliderChild(house, "Col_StepLow", new Vector3((hx + 0.86f) * dir, 0.065f, doorCenterZ), new Vector3(0.40f, 0.13f, doorWidth + 0.2f));
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
                l.shadowStrength = 0.55f;
            }
            return l;
        }

        private static void ConfigureLowPolyDaylight()
        {
            GameObject sunGo = GameObject.Find("Directional Light");
            if (sunGo != null)
            {
                Light sun = sunGo.GetComponent<Light>();
                if (sun != null)
                {
                    sun.type = LightType.Directional;
                    sun.color = new Color(1.0f, 0.87f, 0.68f);
                    sun.intensity = 1.25f;
                    sun.shadows = LightShadows.Soft;
                    sun.shadowStrength = 0.72f;
                    sun.shadowBias = 0.04f;
                    sun.shadowNormalBias = 0.25f;
                    sunGo.transform.rotation = Quaternion.Euler(28f, -42f, 0f);
                }
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.48f, 0.68f);
            RenderSettings.ambientEquatorColor = new Color(0.56f, 0.54f, 0.52f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.21f, 0.19f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.68f, 0.77f, 0.86f);
            RenderSettings.fogDensity = 0.0055f;

            Material skyMat = EnsureCozySkyboxMaterial();
            if (skyMat != null)
            {
                RenderSettings.skybox = skyMat;
                DynamicGI.UpdateEnvironment();
            }

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.Skybox;
                mainCam.backgroundColor = new Color(0.44f, 0.64f, 0.84f, 1f);
                Component urpCamData = mainCam.GetComponent("UniversalAdditionalCameraData");
                if (urpCamData != null)
                {
                    PropertyInfo prop = urpCamData.GetType().GetProperty("renderPostProcessing", BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null && prop.CanWrite) prop.SetValue(urpCamData, true);
                    PropertyInfo aaProp = urpCamData.GetType().GetProperty("antialiasing", BindingFlags.Public | BindingFlags.Instance);
                    if (aaProp != null && aaProp.CanWrite) aaProp.SetValue(urpCamData, System.Enum.ToObject(aaProp.PropertyType, 2));
                }
            }

            EnsureGlobalPostProcessVolume();
        }

        private static Material EnsureCozySkyboxMaterial()
        {
            const string dirPath = "Assets/_Project/Resources/Materials";
            const string assetPath = dirPath + "/M_CozyLowPolySky.mat";
            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader == null) return RenderSettings.skybox;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                System.IO.Directory.CreateDirectory(dirPath);
                mat = new Material(skyShader);
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            else if (mat.shader != skyShader)
            {
                mat.shader = skyShader;
            }

            if (mat.HasProperty("_SunSize")) mat.SetFloat("_SunSize", 0.045f);
            if (mat.HasProperty("_SunSizeConvergence")) mat.SetFloat("_SunSizeConvergence", 6f);
            if (mat.HasProperty("_AtmosphereThickness")) mat.SetFloat("_AtmosphereThickness", 0.72f);
            if (mat.HasProperty("_SkyTint")) mat.SetColor("_SkyTint", new Color(0.36f, 0.56f, 0.80f));
            if (mat.HasProperty("_GroundColor")) mat.SetColor("_GroundColor", new Color(0.34f, 0.31f, 0.28f));
            if (mat.HasProperty("_Exposure")) mat.SetFloat("_Exposure", 1.05f);
            EditorUtility.SetDirty(mat);
            return mat;
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
