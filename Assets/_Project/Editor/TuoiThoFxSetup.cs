#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Menu Tools/Cho Do La/Create Tuoi Tho Chest FX Profile: tạo ChestFx_TuoiTho.asset (gọi TuoiThoFxPreset.Apply),
    /// gắn âm riêng ở Audio/Chests/tuoi_tho/; thiếu file riêng nào thì lấy bộ chung ở Audio/Chests/ (và Synth/).
    /// Đã có asset thì chỉ điền clip còn trống, không ghi đè giá trị đã chỉnh tay.
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

            int missing = 0;
            p.tickClips = Fill(p.tickClips, TuoiThoFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, TuoiThoFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(TuoiThoFxPreset.GrowFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(TuoiThoFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, TuoiThoFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(TuoiThoFxPreset.BambooFile, ChestFxPresets.CreakFile, ref missing), volume = 0.6f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(TuoiThoFxPreset.CandyTinFile, ChestFxPresets.ThudFile, ref missing), volume = 0.7f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[TuoiThoFx] {missing} sound(s) not found (own or shared). The effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TuoiThoFx] Created " + ProfilePath + ". Add TuoiThoHook to the Chest_TuoiTho root for the candle and firefly star.");
            return p;
        }

        /// <summary>Own clip in tuoi_tho/, else the shared clip (mp3, then Synth/ fallback).</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TuoiThoFxPreset.AudioFolder + own);
            if (c != null) return c;
            if (shared != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
                string fb = c == null ? ChestFxPresets.SynthFallback(shared) : null;
                if (fb != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + fb);
                if (c != null) Debug.LogWarning($"[TuoiThoFx] {own} not found; using shared {c.name}.");
            }
            if (c == null) missing++;
            return c;
        }

        private static AudioClip[] Fill(AudioClip[] current, string[] own, string[] shared, ref int missing)
        {
            AudioClip[] result = new AudioClip[own.Length];
            for (int i = 0; i < own.Length; i++)
            {
                AudioClip existing = current != null && i < current.Length ? current[i] : null;
                string sh = shared != null && i < shared.Length ? shared[i] : null;
                result[i] = existing != null ? existing : Clip(own[i], sh, ref missing);
            }
            return result;
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
