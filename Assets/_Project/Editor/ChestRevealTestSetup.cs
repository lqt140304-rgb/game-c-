#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Puts a chest in front of the main camera with BoxOpenSequence + ChestRevealSequence + AudioSource + the
    /// Chest_Golden FX profile, and a ChestRevealDemo (T tier, Y item, Space open, R close, C next chest).
    /// Run "Tools/Cho Do La/Create Sprint 0 Test Scene" first (or any scene with a MainCamera).
    /// </summary>
    public static class ChestRevealTestSetup
    {
        private const string ChestsFolder = "Assets/_Project/Art/Chests";
        private const string ItemsFolder = "Assets/_Project/Art/Items/LowPoly";
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string GoldenProfilePath = ProfileFolder + "/ChestFx_Golden.asset";
        private const string TestRootName = "ChestRevealTest";

        [MenuItem("Tools/Cho Do La/Add Chest Reveal Test")]
        public static void AddChestRevealTest()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[ChestRevealTest] No camera tagged MainCamera in the open scene. Run Tools/Cho Do La/Create Sprint 0 Test Scene first.");
                return;
            }

            List<GameObject> chests = LoadGlbs(ChestsFolder, "Chest_", out int chestFailures);
            List<GameObject> items = LoadGlbs(ItemsFolder, null, out int itemFailures);
            if (chestFailures + itemFailures > 0)
            {
                Debug.LogWarning($"[ChestRevealTest] {chestFailures + itemFailures} .glb file(s) could not be loaded by the AssetDatabase. " +
                                 "Install the glTF importer (package com.unity.cloud.gltfast, via Window > Package Manager > + > Add package by name) " +
                                 "and run `git lfs pull` (files of ~130 bytes are LFS pointers, not models). This menu does not edit Packages/manifest.json.");
            }

            ChestFxProfile golden = CreateOrUpdateGoldenProfile();

            GameObject old = GameObject.Find(TestRootName);
            if (old != null) Undo.DestroyObjectImmediate(old);
            GameObject testRoot = new GameObject(TestRootName);
            Undo.RegisterCreatedObjectUndo(testRoot, "Add Chest Reveal Test");

            // Place the chest about 1.3 m in front of the camera, on a small pedestal so it sits near eye level.
            Vector3 flat = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
            if (flat.sqrMagnitude < 1e-4f) flat = Vector3.forward;
            flat.Normalize();
            Vector3 spot = cam.transform.position + flat * 1.3f;
            float floorY = cam.transform.position.y - 1.6f;
            if (Physics.Raycast(spot + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 5f)) floorY = hit.point.y;

            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(testRoot.transform, false);
            pedestal.transform.position = new Vector3(spot.x, floorY + 0.45f, spot.z);
            pedestal.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);

            GameObject chestAsset = chests.Find(c => c.name.Contains("Golden"));
            if (chestAsset == null && chests.Count > 0) chestAsset = chests[0];

            GameObject chest;
            bool placeholder = chestAsset == null;
            if (!placeholder)
            {
                chest = (GameObject)PrefabUtility.InstantiatePrefab(chestAsset, testRoot.transform);
                if (chests.Remove(chestAsset)) chests.Insert(0, chestAsset);   // the C key starts from this one
            }
            else
            {
                chest = BuildPlaceholderChest(testRoot.transform);
                Debug.LogWarning("[ChestRevealTest] No Chest_*.glb could be loaded; using a placeholder box chest made of cubes.");
            }
            chest.transform.position = new Vector3(spot.x, floorY + 0.9f, spot.z);
            // glTF models face +Z after import; turn the front toward the camera. Rotate 180° in Y if it shows its back.
            // The placeholder fronts -Z (BoxOpenSequence default frontDirection).
            chest.transform.rotation = Quaternion.LookRotation(placeholder ? flat : -flat, Vector3.up);

            Transform rig = ChestRevealDemo.FindRig(chest.transform);
            if (rig.Find("Lid") == null) Debug.LogWarning($"[ChestRevealTest] {chest.name} has no \"Lid\" child: the lid will not move.");
            if (rig.GetComponent<BoxOpenSequence>() == null) rig.gameObject.AddComponent<BoxOpenSequence>();
            AudioSource audio = rig.GetComponent<AudioSource>();
            if (audio == null) audio = rig.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0.5f;
            ChestRevealSequence reveal = rig.GetComponent<ChestRevealSequence>();
            if (reveal == null) reveal = rig.gameObject.AddComponent<ChestRevealSequence>();
            SerializedObject so = new SerializedObject(reveal);
            so.FindProperty("profile").objectReferenceValue = golden;
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (chest.name.Contains("Golden") && rig.GetComponent<GoldenGlintHook>() == null) rig.gameObject.AddComponent<GoldenGlintHook>();

            GameObject demoGo = new GameObject("ChestRevealDemo");
            demoGo.transform.SetParent(testRoot.transform, false);
            ChestRevealDemo demo = demoGo.AddComponent<ChestRevealDemo>();
            SerializedObject dso = new SerializedObject(demo);
            dso.FindProperty("reveal").objectReferenceValue = reveal;
            FillList(dso.FindProperty("items"), items);
            FillList(dso.FindProperty("chests"), chests);
            FillList(dso.FindProperty("profiles"), new List<Object> { golden });
            dso.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = rig.gameObject;
            Debug.Log($"[ChestRevealTest] Added {chest.name} with {items.Count} demo items. Press Play: [T] tier, [Y] item, [Space] open, [R] close, [C] next chest.");
        }

        [MenuItem("Tools/Cho Do La/Create Golden Chest FX Profile")]
        public static void CreateGoldenProfileMenu()
        {
            ChestFxProfile p = CreateOrUpdateGoldenProfile();
            Selection.activeObject = p;
        }

        /// <summary>Creates ChestFx_Golden.asset with the preset values, or fills only its missing clips if it exists.</summary>
        public static ChestFxProfile CreateOrUpdateGoldenProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(GoldenProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                ChestFxPresets.ApplyGolden(p);
                AssetDatabase.CreateAsset(p, GoldenProfilePath);
            }

            int missing = 0;
            p.tickClips = FillClips(p.tickClips, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = FillClips(p.lockClipsByTier, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = FillClips(p.accentClips, ChestFxPresets.CoinFiles, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(ChestFxPresets.ThudFile, ref missing), volume = 0.6f },
                };
            }
            if (missing > 0)
            {
                Debug.LogWarning($"[ChestRevealTest] {missing} sound(s) not found in {ChestFxPresets.AudioFolder}. " +
                                 "The mp3 set is on branch claude/chest-sfx; `python3 Tools/Audio/make_chest_sfx.py` remakes the Synth/ fallback. The effect still runs without sound.");
            }

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[ChestRevealTest] Created " + GoldenProfilePath);
            return p;
        }

        // ------------------------------------------------------------------ Helpers

        private static List<GameObject> LoadGlbs(string folder, string namePrefix, out int failures)
        {
            failures = 0;
            List<GameObject> result = new List<GameObject>();
            if (!AssetDatabase.IsValidFolder(folder)) return result;
            string[] files = Directory.GetFiles(folder, "*.glb", SearchOption.AllDirectories);
            System.Array.Sort(files, System.StringComparer.Ordinal);
            foreach (string file in files)
            {
                string path = file.Replace('\\', '/');
                if (namePrefix != null && !Path.GetFileName(path).StartsWith(namePrefix)) continue;
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    result.Add(go);
                    continue;
                }
                failures++;
                long size = new FileInfo(path).Length;
                if (size < 1024) Debug.LogWarning($"[ChestRevealTest] {path} is {size} bytes: a Git LFS pointer, not the model. Run `git lfs pull`.");
            }
            return result;
        }

        /// <summary>Loads the mp3 (ElevenLabs); if it is missing, the synthesised placeholder in Synth/.</summary>
        private static AudioClip Clip(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + file);
            if (c != null) return c;
            string fallback = ChestFxPresets.SynthFallback(file);
            if (fallback != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fallback);
            if (c != null) Debug.LogWarning($"[ChestRevealTest] {file} not found; using the synthesised placeholder {fallback}.");
            else missing++;
            return c;
        }

        private static AudioClip[] FillClips(AudioClip[] current, string[] files, ref int missing)
        {
            AudioClip[] result = new AudioClip[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                result[i] = existing != null ? existing : Clip(files[i], ref missing);
            }
            return result;
        }

        private static void FillList<T>(SerializedProperty list, List<T> values) where T : Object
        {
            list.ClearArray();
            for (int i = 0; i < values.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        /// <summary>Box chest made of cubes (Body, Lid with hinge pivot, Glow), used when no .glb loads.</summary>
        private static GameObject BuildPlaceholderChest(Transform parent)
        {
            GameObject root = new GameObject("Chest_Placeholder");
            root.transform.SetParent(parent, false);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.13f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.26f, 0.34f);

            GameObject lid = new GameObject("Lid");
            lid.transform.SetParent(root.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.26f, 0.17f);   // hinge at the back top edge (+Z), front is -Z
            GameObject lidMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lidMesh.name = "LidMesh";
            lidMesh.transform.SetParent(lid.transform, false);
            lidMesh.transform.localPosition = new Vector3(0f, 0.04f, -0.17f);
            lidMesh.transform.localScale = new Vector3(0.52f, 0.08f, 0.36f);

            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "Glow";
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            glow.transform.localScale = new Vector3(0.36f, 0.04f, 0.22f);

            foreach (Collider c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            return root;
        }
    }
}
#endif
