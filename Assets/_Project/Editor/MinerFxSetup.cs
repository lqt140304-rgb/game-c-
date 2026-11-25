#if UNITY_EDITOR
using System.IO;
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Creates ChestFx_Miner.asset (MinerFxPreset values + ChestScript_Miner, core audio muted per ChestAudioRule).
    /// </summary>
    public static class MinerFxSetup
    {
        private const string ProfileFolder = "Assets/_Project/ScriptableObjects/ChestFx";
        private const string ProfilePath = ProfileFolder + "/ChestFx_Miner.asset";
        private const string ScriptPath = ProfileFolder + "/ChestScript_Miner.asset";

        [MenuItem("Tools/Cho Do La/Create Miner Chest FX Profile")]
        public static void CreateMenu()
        {
            Selection.activeObject = CreateOrUpdate();
        }

        [MenuItem("Tools/Cho Do La/Add Miner Hook To Selected Chest")]
        public static void AddHookMenu()
        {
            // MinerHook cũ không còn được gắn: menu này chuyển sang kịch bản đào (một rương chỉ một hook).
            MinerDigSetup.AddMenu();
        }

        /// <summary>
        /// Creates the asset with the preset values (or reuses it), then EVERY run: rebuilds ChestScript_Miner and mutes the core
        /// audio (ChestAudioRule.ApplyToProfile). The miner hook (MinerDigSequence) plays its own ElevenLabs sounds.
        /// </summary>
        public static ChestFxProfile CreateOrUpdate()
        {
            ChestFxProfile p = AssetDatabase.LoadAssetAtPath<ChestFxProfile>(ProfilePath);
            bool created = p == null;
            if (created)
            {
                EnsureFolder(ProfileFolder);
                p = ScriptableObject.CreateInstance<ChestFxProfile>();
                MinerFxPreset.Apply(p);
                AssetDatabase.CreateAsset(p, ProfilePath);
            }

            // Kịch bản: bấm → cue "dao_mo" (đào + nổ, ≤ 3 s) → mở rương. Dựng lại MỖI lần chạy menu.
            ChestScript script = AssetDatabase.LoadAssetAtPath<ChestScript>(ScriptPath);
            if (script == null)
            {
                script = ScriptableObject.CreateInstance<ChestScript>();
                BuildScript(script);
                AssetDatabase.CreateAsset(script, ScriptPath);
            }
            else BuildScript(script);
            EditorUtility.SetDirty(script);
            p.script = script;

            ChestAudioRule.ApplyToProfile(p);   // A1: lõi câm, tension tắt (mỗi lần chạy)

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[MinerFx] Created " + ProfilePath);
            return p;
        }

        private static void BuildScript(ChestScript s)
        {
            s.description = "Thợ Mỏ: bấm là đào ngay (cue dao_mo của MinerDigSequence, ≤ 3 s), xong mới mở rương.";
            s.steps.Clear();
            ChestStep dig = ChestStepBuilder.Add(s, "dao-mo", StepKind.Cue, StepStart.OnPress);
            dig.cue = "dao_mo";
            ChestStepBuilder.Add(s, "mo-ruong", StepKind.OpenBox, StepStart.AfterPrevious);
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
