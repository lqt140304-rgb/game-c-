#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Create Nhat Ban Chest FX Profile: creates ChestFx_NhatBan.asset (NhatBanFxPreset values),
    /// the prefab ChestFx_NhatBanHook.prefab (NhatBanHook) as its customFxPrefab, and fills the clips from
    /// Audio/Chests/nhat_ban/ (mp3 ElevenLabs). Core profile is silent (ChestAudioRule.ApplyToProfile, every run).
    /// Kịch bản "Nhát kiếm Iai" (2026-11-25): mỗi lần chạy ÁP LẠI preset + âm (ghi đè bản Daruma cũ), và gắn âm riêng
    /// (sáo, taiko, rút kiếm, chém, nhập vỏ, dây đứt, dép, koto) và tiếng chốt stingClips[5] vào NhatBanHook trên prefab.
    /// </summary>
    public static class NhatBanFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_NhatBan.asset";
        private const string HookPrefabPath = ProfileFolder + "/ChestFx_NhatBanHook.prefab";

        [MenuItem("Tools/Cho Do La/Create Nhat Ban Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            EnsureFolder(ProfileFolder);
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                AssetDatabase.CreateAsset(p, ProfilePath);
            }
            NhatBanFxPreset.Apply(p);
            int missing = 0;
            // A1: lõi câm, ghi lại MỖI lần chạy menu (tension.enabled = false, mọi ô âm lõi trống, lockClipsByTier 5 ô null).
            ChestAudioRule.ApplyToProfile(p);
            p.customFxPrefab = HookPrefab(ref missing);
            if (missing > 0) Debug.LogWarning($"[NhatBanFx] {missing} sound(s) not found; the effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[NhatBanFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject HookPrefab(ref int missing)
        {
            GameObject temp = new GameObject("ChestFx_NhatBanHook");
            NhatBanHook hook = temp.AddComponent<NhatBanHook>();
            hook.stingClips = new AudioClip[NhatBanFxPreset.StingFiles.Length];
            for (int i = 0; i < hook.stingClips.Length; i++) hook.stingClips[i] = Clip(NhatBanFxPreset.StingFiles[i], null, ref missing);
            hook.sounds.drone = Clip(NhatBanFxPreset.DroneFile, null, ref missing);
            hook.sounds.heartbeat = Clip(NhatBanFxPreset.HeartbeatFile, null, ref missing);
            hook.sounds.draw = Clip(NhatBanFxPreset.DrawFile, null, ref missing);
            hook.sounds.slash = Clip(NhatBanFxPreset.SlashFile, null, ref missing);
            hook.sounds.sheath = Clip(NhatBanFxPreset.SheathFile, null, ref missing);
            hook.sounds.ropeSnap = Clip(NhatBanFxPreset.RopeSnapFile, null, ref missing);
            hook.sounds.cutFruit = Clip(NhatBanFxPreset.CutFruitFile, null, ref missing);
            hook.sounds.sandal = Clip(NhatBanFxPreset.SandalFile, null, ref missing);
            hook.sounds.bow = Clip(NhatBanFxPreset.BowFile, null, ref missing);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, HookPrefabPath);   // ghi đè prefab cũ
            Object.DestroyImmediate(temp);
            return prefab;
        }

        /// <summary>Own clip from nhat_ban/, else the shared one (mp3, then its Synth/ fallback).</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(NhatBanFxPreset.AudioFolder + own);
            if (c != null) return c;
            if (shared != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
                string fb = ChestFxPresets.SynthFallback(shared);
                if (c == null && fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
            }
            if (c != null) Debug.LogWarning($"[NhatBanFx] {own} not found; using the shared sound {shared}.");
            else missing++;
            return c;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
#endif
