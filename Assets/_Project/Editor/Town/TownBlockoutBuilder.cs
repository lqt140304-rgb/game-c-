#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ChoDoLa.Editor;
using ChoDoLa.Interaction;
using ChoDoLa.Town.Layout;
using ChoDoLa.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoDoLa.Town.Editor
{
    public static class TownBlockoutBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Market_Town.unity";

        [MenuItem("Tools/Cho Do La/Build Market Town Blockout")]
        public static void Build()
        {
            string folder = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight();

            GameObject townRoot = new GameObject("MarketTown");
            var groupRoots = new Dictionary<string, Transform>();

            foreach (TownPiece piece in TownLayout.Pieces)
            {
                Transform parent = GetOrCreateGroup(townRoot.transform, groupRoots, GetGroupName(piece.Kind));
                BuildPiece(piece, parent);
            }

            Sprint0HudView hud = Sprint0SceneSetup.CreateHud();
            Sprint0SceneSetup.CreatePlayer(hud);

            TownPiece spawn = TownLayout.Spawn;
            GameObject player = GameObject.Find("Player");
            if (player != null && spawn != null)
            {
                player.transform.position = new Vector3(spawn.Center.X, spawn.Center.Y + 1.0f, spawn.Center.Z);
                player.transform.rotation = Quaternion.Euler(0f, spawn.YawDegrees, 0f);
            }

            CreateOpenTableProps(townRoot.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TownBlockout] Built and saved {ScenePath} with {TownLayout.Pieces.Count} pieces.");
        }

        private static void BuildPiece(TownPiece piece, Transform parent)
        {
            GameObject prefab = TownPrefabFactory.GetOrCreate(piece.Kind, piece.Palette);

            if (piece.ClearanceHeight > 0f &&
                (piece.Kind == TownPieceKind.Arch ||
                 piece.Kind == TownPieceKind.LandmarkTower ||
                 piece.Kind == TownPieceKind.LandmarkHoloArch))
            {
                BuildWalkThroughStructure(piece, prefab, parent);
                return;
            }

            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = piece.Id;

            if (piece.Kind == TownPieceKind.Ramp)
            {
                float slopeLen = Mathf.Sqrt(piece.Size.Z * piece.Size.Z + piece.Size.Y * piece.Size.Y);
                go.transform.position = new Vector3(piece.Center.X, piece.Center.Y, piece.Center.Z);
                go.transform.rotation = Quaternion.Euler(-piece.RampDegrees, piece.YawDegrees, 0f);
                go.transform.localScale = new Vector3(piece.Size.X, 0.2f, slopeLen);
                go.transform.position -= go.transform.up * 0.1f;
                return;
            }

            float yCenter = piece.IsMarker || piece.Kind == TownPieceKind.Ground || piece.Kind == TownPieceKind.Deck || piece.Kind == TownPieceKind.Bridge || piece.Kind == TownPieceKind.Railing || piece.Kind == TownPieceKind.Building || piece.Kind == TownPieceKind.CoverBuilding || piece.Kind == TownPieceKind.Wall || piece.Kind == TownPieceKind.AccentSteam || piece.Kind == TownPieceKind.AccentSciFi || piece.Kind == TownPieceKind.AccentArcane || piece.Kind == TownPieceKind.StreetLight
                ? piece.Center.Y
                : piece.Center.Y + piece.Size.Y * 0.5f;

            if (piece.Kind == TownPieceKind.Bridge)
            {
                yCenter = piece.Center.Y - piece.Size.Y * 0.5f;
            }
            else if (piece.Kind == TownPieceKind.LandmarkWell || piece.IsMarker)
            {
                yCenter = piece.Center.Y + piece.Size.Y * 0.5f;
            }

            go.transform.position = new Vector3(piece.Center.X, yCenter, piece.Center.Z);
            go.transform.rotation = Quaternion.Euler(0f, piece.YawDegrees, 0f);
            go.transform.localScale = new Vector3(piece.Size.X, piece.Size.Y, piece.Size.Z);

            if (piece.Kind == TownPieceKind.ShopSlot)
            {
                ShopSlotMarker marker = go.AddComponent<ShopSlotMarker>();
                marker.SlotId = piece.Id;
                marker.SizeMeters = new Vector2(piece.Size.X, piece.Size.Z);
                marker.IsUpperFloor = piece.Center.Y >= TownSpec.DeckElevation - TownSpec.GapTolerance;
            }
            else if (piece.Kind == TownPieceKind.StreetLight)
            {
                GameObject lightGo = new GameObject("PointLight");
                lightGo.transform.SetParent(go.transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                Light pt = lightGo.AddComponent<Light>();
                pt.type = LightType.Point;
                pt.color = new Color(1.0f, 0.85f, 0.6f);
                pt.range = 10f;
                pt.intensity = 2.2f;
            }
        }

        private static void BuildWalkThroughStructure(TownPiece piece, GameObject prefab, Transform parent)
        {
            GameObject root = new GameObject(piece.Id);
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(piece.Center.X, piece.Center.Y, piece.Center.Z);
            root.transform.rotation = Quaternion.Euler(0f, piece.YawDegrees, 0f);

            float openingWidth = Mathf.Min(4.0f, piece.Size.X * 0.55f);
            float pillarWidth = (piece.Size.X - openingWidth) * 0.5f;
            float pillarOffsetX = openingWidth * 0.5f + pillarWidth * 0.5f;
            float clearance = piece.ClearanceHeight;
            float lintelHeight = Mathf.Max(0.8f, piece.Size.Y - clearance);

            SpawnSubBlock(prefab, root.transform, "Pillar_L",
                new Vector3(-pillarOffsetX, clearance * 0.5f, 0f),
                new Vector3(pillarWidth, clearance, piece.Size.Z));

            SpawnSubBlock(prefab, root.transform, "Pillar_R",
                new Vector3(pillarOffsetX, clearance * 0.5f, 0f),
                new Vector3(pillarWidth, clearance, piece.Size.Z));

            SpawnSubBlock(prefab, root.transform, "Lintel_Top",
                new Vector3(0f, clearance + lintelHeight * 0.5f, 0f),
                new Vector3(piece.Size.X, lintelHeight, piece.Size.Z));
        }

        private static void SpawnSubBlock(GameObject prefab, Transform parent, string name, Vector3 localPos, Vector3 scale)
        {
            GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            block.name = name;
            block.transform.localPosition = localPos;
            block.transform.localRotation = Quaternion.identity;
            block.transform.localScale = scale;
        }

        private static void CreateOpenTableProps(Transform parent)
        {
            GameObject group = new GameObject("OpenTable_Props");
            group.transform.SetParent(parent, false);

            GameObject woodPrefab = TownPrefabFactory.GetOrCreate(TownPieceKind.Wall, TownPalette.Wood);
            GameObject brassPrefab = TownPrefabFactory.GetOrCreate(TownPieceKind.AccentSteam, TownPalette.Brass);

            GameObject counter = (GameObject)PrefabUtility.InstantiatePrefab(woodPrefab, group.transform);
            counter.name = "Counter_NiemPhong_Placeholder";
            counter.transform.position = new Vector3(-1.2f, 0.5f, 22.0f);
            counter.transform.localScale = new Vector3(2.2f, 1.0f, 0.9f);

            GameObject table = (GameObject)PrefabUtility.InstantiatePrefab(woodPrefab, group.transform);
            table.name = "Table_OpenBox_Placeholder";
            table.transform.position = new Vector3(1.0f, 0.45f, 22.0f);
            table.transform.localScale = new Vector3(1.6f, 0.9f, 1.0f);

            GameObject testBox = (GameObject)PrefabUtility.InstantiatePrefab(brassPrefab, group.transform);
            testBox.name = "TestBox_Placeholder";
            testBox.transform.position = new Vector3(1.0f, 1.08f, 22.0f);
            testBox.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            testBox.AddComponent<TestBoxInteractable>();
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            ColorUtility.TryParseHtmlString("#FFE2B8", out Color warm);
            light.color = warm;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static string GetGroupName(TownPieceKind kind)
        {
            switch (kind)
            {
                case TownPieceKind.Deck:
                case TownPieceKind.Ramp:
                case TownPieceKind.Bridge:
                case TownPieceKind.Railing:
                    return "Verticality";
                case TownPieceKind.LandmarkTower:
                case TownPieceKind.LandmarkWell:
                case TownPieceKind.LandmarkHoloArch:
                    return "Landmarks";
                case TownPieceKind.AccentSteam:
                case TownPieceKind.AccentSciFi:
                case TownPieceKind.AccentArcane:
                    return "Accents";
                case TownPieceKind.ShopSlot:
                case TownPieceKind.InnSpawn:
                case TownPieceKind.OpenTableZone:
                    return "Markers";
                default:
                    return "Architecture";
            }
        }

        private static Transform GetOrCreateGroup(Transform root, Dictionary<string, Transform> cache, string name)
        {
            if (cache.TryGetValue(name, out Transform t)) return t;
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            cache[name] = go.transform;
            return go.transform;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene s in existing)
            {
                if (s.path == scenePath) return;
            }

            EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[existing.Length + 1];
            existing.CopyTo(updated, 0);
            updated[existing.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
