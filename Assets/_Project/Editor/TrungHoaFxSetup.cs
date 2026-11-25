#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Trung Hoa Chest FX Profile: creates ChestFx_TrungHoa.asset with TrungHoaFxPreset values
    /// and wires the clips in Audio/Chests/trung_hoa/ (falls back to the shared set for any missing file).
    /// Tools/Cho Do La/Add Trung Hoa Hook To Selected: adds TrungHoaHook to the selected chest.
    /// </summary>
    public static class TrungHoaFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_TrungHoa.asset";

        [MenuItem("Tools/Cho Do La/Create Trung Hoa Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdateProfile();
        }

        [MenuItem("Tools/Cho Do La/Add Trung Hoa Hook To Selected")]
        public static void AddHookMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[TrungHoaFx] Select the chest (the object with ChestRevealSequence) first."); return; }
            TrungHoaHook hook = go.GetComponent<TrungHoaHook>();
            if (hook == null) hook = go.AddComponent<TrungHoaHook>();
            WireHookClips(hook);
            EditorUtility.SetDirty(go);
        }

        /// <summary>Gán âm ElevenLabs (Tools/Audio/elevenlabs_chest_manifest.json, mục trung_hoa) vào hook. File thiếu: để ô trống.</summary>
        public static void WireHookClips(TrungHoaHook h)
        {
            int missing = 0;
            h.drumLoop = C("lion_drum_loop", ref missing);
            h.matchStrike = C("match", ref missing);
            h.pops = new[] { C("pop", ref missing), C("firecracker", ref missing) };
            h.bigBang = C("big_bang", ref missing);
            h.fizzle = C("dud", ref missing);
            h.rocketWhistle = C("rocket", ref missing);
            h.rocketBoom = C("rocket_burst", ref missing);
            h.sneeze = C("sneeze", ref missing);
            h.stingClips = new AudioClip[ChestTierUtil.Count];
            for (int i = 0; i < h.stingClips.Length && i < TrungHoaFxPreset.LockFiles.Length; i++)
                h.stingClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(TrungHoaFxPreset.AudioFolder + TrungHoaFxPreset.LockFiles[i]);
            if (missing > 0) Debug.LogWarning($"[TrungHoaFx] Thiếu {missing} file âm trong {TrungHoaFxPreset.AudioFolder}: xem Tools/Audio/make_elevenlabs_chest_sfx.py --chest trung_hoa");
        }

        private static AudioClip C(string n, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TrungHoaFxPreset.AudioFolder + "sfx_trunghoa_" + n + ".mp3");
            if (c == null) missing++;
            return c;
        }

        [MenuItem("Tools/Cho Do La/Spawn Trung Hoa Incense (nen nhang)")]
        public static void SpawnIncense() => Spawn(TrungHoaPlayerItem.Kind.Incense);

        [MenuItem("Tools/Cho Do La/Spawn Trung Hoa Bonus Rocket (phao thang thien)")]
        public static void SpawnRocket() => Spawn(TrungHoaPlayerItem.Kind.BonusRocket);

        private static void Spawn(TrungHoaPlayerItem.Kind kind)
        {
            Vector3 at = Selection.activeGameObject != null ? Selection.activeGameObject.transform.position + new Vector3(0.4f, 0.5f, 0.4f) : new Vector3(0f, 1f, 0f);
            Selection.activeGameObject = TrungHoaPlayerItem.Create(kind, at).gameObject;
        }

        public static ChestFxProfile CreateOrUpdateProfile()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                TrungHoaFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            // A1: hồ sơ lõi gần như câm, ghi lại MỖI LẦN chạy menu (tiếng do TrungHoaHook phát)
            ChestAudioRule.ApplyToProfile(p);

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TrungHoaFx] Created " + ProfilePath);
            return p;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
#endif
