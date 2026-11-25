#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Menu Tools/Cho Do La/Create Tuoi Tho Chest FX Profile: tạo ChestFx_TuoiTho.asset (gọi TuoiThoFxPreset.Apply),
    /// rồi gọi ChestAudioRule.ApplyToProfile (A1) MỖI lần chạy: lõi tắt tension và không có tiếng nào, mọi âm do TuoiThoHook phát.
    /// </summary>
    public static class TuoiThoFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_TuoiTho.asset";

        [MenuItem("Tools/Cho Do La/Create Tuoi Tho Chest FX Profile")]
        public static void CreateMenu()
        {
            ChestFxProfile p = CreateOrUpdate();
            Selection.activeObject = p;
        }

        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                TuoiThoFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            // A1: hồ sơ lõi gần như câm, chạy MỖI LẦN (kể cả hồ sơ đã có). Âm do TuoiThoHook phát.
            ChestAudioRule.ApplyToProfile(p);

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TuoiThoFx] Created " + ProfilePath + ". Select the Chest_TuoiTho root and run Tools/Cho Do La/Add Tuoi Tho Marble Machine.");
            else Debug.Log("[TuoiThoFx] Updated " + ProfilePath + " (core audio silenced, tension off).");
            return p;
        }

        /// <summary>
        /// Menu: gắn TuoiThoHook (cỗ máy bi ve) lên object đang chọn (gốc Chest_TuoiTho, nơi có ChestRevealSequence)
        /// và gán các âm ElevenLabs .mp3 trong Audio/Chests/tuoi_tho/ (không còn .wav numpy). Chạy lại được: chỉ điền ô âm còn trống.
        /// </summary>
        [MenuItem("Tools/Cho Do La/Add Tuoi Tho Marble Machine")]
        public static void AddMachineMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[TuoiThoFx] Select the Chest_TuoiTho root first."); return; }
            if (go.GetComponent<ChestRevealSequence>() == null)
                Debug.LogWarning("[TuoiThoFx] The selected object has no ChestRevealSequence: the machine starts only when that component plays.");
            TuoiThoHook h = go.GetComponent<TuoiThoHook>();
            if (h == null) h = Undo.AddComponent<TuoiThoHook>(go);
            Undo.RecordObject(h, "Assign Tuoi Tho sounds");
            int missing = 0;
            // Chỉ file ElevenLabs .mp3 trong Audio/Chests/tuoi_tho/ (A9). Ô đã có thì giữ, trống thì điền.
            h.rollClip = h.rollClip != null ? h.rollClip : M("sfx_tuoitho_roll_loop.mp3", ref missing);
            h.xyloClip = h.xyloClip != null ? h.xyloClip : M("sfx_tuoitho_xylo.mp3", ref missing);
            h.flickClip = h.flickClip != null ? h.flickClip : M("sfx_tuoitho_flick.mp3", ref missing);
            h.dominoClip = h.dominoClip != null ? h.dominoClip : M("sfx_tuoitho_domino.mp3", ref missing);
            h.bandClip = h.bandClip != null ? h.bandClip : M("sfx_tuoitho_band.mp3", ref missing);
            h.bonkClip = h.bonkClip != null ? h.bonkClip : M("sfx_tuoitho_bonk.mp3", ref missing);
            h.marblesClip = h.marblesClip != null ? h.marblesClip : M("sfx_tuoitho_marbles.mp3", ref missing);
            h.planeClip = h.planeClip != null ? h.planeClip : M("sfx_tuoitho_plane.mp3", ref missing);
            h.crumpleClip = h.crumpleClip != null ? h.crumpleClip : M("sfx_tuoitho_crumple.mp3", ref missing);
            if (h.stingClips == null || h.stingClips.Length != TuoiThoFxPreset.LockFiles.Length)
                h.stingClips = new AudioClip[TuoiThoFxPreset.LockFiles.Length];
            for (int i = 0; i < h.stingClips.Length; i++)
                if (h.stingClips[i] == null) h.stingClips[i] = M(TuoiThoFxPreset.LockFiles[i], ref missing);
            EditorUtility.SetDirty(h);
            if (missing > 0) Debug.LogWarning($"[TuoiThoFx] {missing} sound(s) missing in {TuoiThoFxPreset.AudioFolder}; see Tools/Audio/make_elevenlabs_chest_sfx.py.");
            else Debug.Log("[TuoiThoFx] Marble machine ready on " + go.name + ".");
        }

        private static AudioClip M(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TuoiThoFxPreset.AudioFolder + file);
            if (c == null) missing++;
            return c;
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
