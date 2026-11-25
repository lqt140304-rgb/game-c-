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
            // Không có âm nền: TuoiThoHook phát búng bi + nhịp mộc cầm ngay lúc bấm mở.
            p.accentClips = Fill(p.accentClips, TuoiThoFxPreset.AccentFiles, null, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(TuoiThoFxPreset.CandyTinFile, ChestFxPresets.ThudFile, ref missing), volume = 0.7f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[TuoiThoFx] {missing} sound(s) not found (own or shared). The effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TuoiThoFx] Created " + ProfilePath + ". Select the Chest_TuoiTho root and run Tools/Cho Do La/Add Tuoi Tho Marble Machine.");
            return p;
        }

        /// <summary>
        /// Menu: gắn TuoiThoHook (cỗ máy bi ve) lên object đang chọn (gốc Chest_TuoiTho, nơi có ChestRevealSequence)
        /// và gán các âm trong tuoi_tho/may_bi/. Chạy lại được: chỉ điền ô âm còn trống.
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
            h.flickClip = h.flickClip != null ? h.flickClip : M("sfx_tt_flick.wav", ref missing);
            h.rollClip = h.rollClip != null ? h.rollClip : M("sfx_tt_roll.wav", ref missing);
            h.dominoClip = h.dominoClip != null ? h.dominoClip : M("sfx_tt_domino.wav", ref missing);
            h.glassClip = h.glassClip != null ? h.glassClip : M("sfx_tt_glass.wav", ref missing);
            h.carClip = h.carClip != null ? h.carClip : M("sfx_tt_car.wav", ref missing);
            h.topClip = h.topClip != null ? h.topClip : M("sfx_tt_top.wav", ref missing);
            h.bandClip = h.bandClip != null ? h.bandClip : M("sfx_tt_band.wav", ref missing);
            h.bonkClip = h.bonkClip != null ? h.bonkClip : M("sfx_tt_bonk.wav", ref missing);
            h.xyloClip = h.xyloClip != null ? h.xyloClip : M("sfx_tt_xylo.wav", ref missing);
            h.riserClip = h.riserClip != null ? h.riserClip : M("sfx_tt_riser.wav", ref missing);
            h.failClip = h.failClip != null ? h.failClip : M("sfx_tt_fail.wav", ref missing);
            h.winClip = h.winClip != null ? h.winClip : M("sfx_tt_win.wav", ref missing);
            h.planeClip = h.planeClip != null ? h.planeClip : M("sfx_tt_plane.wav", ref missing);
            h.crumpleClip = h.crumpleClip != null ? h.crumpleClip : M("sfx_tt_crumple.wav", ref missing);
            EditorUtility.SetDirty(h);
            if (missing > 0) Debug.LogWarning($"[TuoiThoFx] {missing} machine sound(s) missing; run Tools/Audio/make_tuoitho_machine_sfx.py.");
            else Debug.Log("[TuoiThoFx] Marble machine ready on " + go.name + ".");
        }

        private static AudioClip M(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TuoiThoFxPreset.MachineFolder + file);
            if (c == null) missing++;
            return c;
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
