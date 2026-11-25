#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Menu Tools/Cho Do La/Create Rac Quy Chest FX Profile: creates (or fills the missing clips of)
    /// ScriptableObjects/ChestFx/ChestFx_RacQuy.asset with <see cref="RacQuyFxPreset"/>, plus a prefab
    /// ChestFx_RacQuy_Hook.prefab holding <see cref="RacQuyHook"/> (set as the profile's customFxPrefab).
    /// Mọi âm do hook phát từ .mp3 ElevenLabs trong Audio/Chests/rac_quy/; hồ sơ lõi để câm (ChestAudioRule.ApplyToProfile).
    /// </summary>
    public static class RacQuyFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_RacQuy.asset";
        private const string HookPrefabPath = ProfileFolder + "/ChestFx_RacQuy_Hook.prefab";
        private const string OwnFolder = "Assets/_Project/Audio/Chests/rac_quy/";

        private static readonly string[] OwnLockFiles =
        {
            "sfx_racquy_lock_rac.mp3", "sfx_racquy_lock_thuong.mp3", "sfx_racquy_lock_hiem.mp3",
            "sfx_racquy_lock_ratHiem.mp3", "sfx_racquy_lock_huyenThoai.mp3",
        };

        [MenuItem("Tools/Cho Do La/Create Rac Quy Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                RacQuyFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            int missing = 0;
            // A1: hồ sơ lõi gần như câm, ghi lại MỖI LẦN chạy menu (tension tắt, mọi ô âm lõi trống).
            // Mọi tiếng do RacQuyHook phát (nền, tiếng lẻ, TC theo bậc) từ các .mp3 ElevenLabs trong rac_quy/.
            ChestAudioRule.ApplyToProfile(p);

            // Luôn chạy: prefab cũ cũng được nạp lại clip .mp3 + stingClips (không để sót .wav cũ).
            GameObject hook = CreateHookPrefab(ref missing);
            if (p.customFxPrefab == null || p.customFxPrefab.GetComponent<RacQuyHook>() == null) p.customFxPrefab = hook;

            if (missing > 0)
                Debug.LogWarning($"[RacQuyFx] {missing} sound(s) not found in {OwnFolder}. The effect still runs; those sounds are silent.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[RacQuyFx] Created " + ProfilePath);
            return p;
        }

        private static GameObject CreateHookPrefab(ref int missing)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HookPrefabPath);
            if (existing != null && existing.GetComponent<RacQuyHook>() != null)
            {
                FillClips(existing.GetComponent<RacQuyHook>(), ref missing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            GameObject temp = new GameObject("ChestFx_RacQuy_Hook");
            FillClips(temp.AddComponent<RacQuyHook>(), ref missing);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, HookPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        /// <summary>
        /// Gán clip cho hook theo Tools/Audio/elevenlabs_chest_manifest.json (mục rac_quy, ô `field`). Luôn ghi đè để bỏ
        /// các .wav tổng hợp cũ (meo/); file thiếu thì để ô trống và đếm vào cảnh báo (A9–A11).
        /// </summary>
        private static void FillClips(RacQuyHook h, ref int missing)
        {
            h.gurgleClip = Own("sfx_racquy_gurgle.mp3", ref missing);
            h.fliesClip = Own("sfx_racquy_flies.mp3", ref missing);
            h.buildLoopClip = Own("sfx_racquy_bed_loop.mp3", ref missing);
            h.meowClip = Own("sfx_racquy_meow.mp3", ref missing);
            h.meowWantsClip = Own("sfx_racquy_meow_wants.mp3", ref missing);
            h.yowlClip = Own("sfx_racquy_yowl.mp3", ref missing);
            h.hissClip = Own("sfx_racquy_hiss.mp3", ref missing);
            h.clatterClip = Own("sfx_racquy_clatter.mp3", ref missing);
            h.clankClip = Own("sfx_racquy_clank.mp3", ref missing);
            h.deflateClip = Own("sfx_racquy_deflate.mp3", ref missing);
            h.boomClip = Own("sfx_racquy_boom.mp3", ref missing);
            h.throwClip = Own("sfx_racquy_throw.mp3", ref missing);
            // Không còn dùng: fart (A4 bỏ), canRoll (lõi câm). Để trống cho khỏi lẫn.
            h.fartClip = null;
            h.canRollClip = null;

            h.stingClips = new AudioClip[OwnLockFiles.Length];
            for (int i = 0; i < OwnLockFiles.Length; i++) h.stingClips[i] = Own(OwnLockFiles[i], ref missing);
        }

        /// <summary>mp3 ElevenLabs trong rac_quy/ (null + đếm thiếu nếu chưa có).</summary>
        private static AudioClip Own(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(OwnFolder + file);
            if (c == null) missing++;
            return c;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
#endif
