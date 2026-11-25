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

        private const string FxFolder = TrungHoaFxPreset.AudioFolder + "fx/sfx_trunghoa_fx_";

        /// <summary>Gán âm riêng của kịch bản Đêm Giao Thừa (tạo bằng Tools/Audio/make_trunghoa_fx.py) vào hook.</summary>
        public static void WireHookClips(TrungHoaHook h)
        {
            h.drumLoop = C("drum_loop");
            h.fuseHissLoop = C("fuse_hiss");
            h.matchStrike = C("match");
            h.pops = new[] { C("pop_1"), C("pop_2"), C("pop_3") };
            h.bigBang = C("big_bang");
            h.fizzle = C("fizzle");
            h.slideDown = C("slide_down");
            h.rocketWhistle = C("rocket_whistle");
            h.rocketBoom = C("rocket_boom");
            h.cymbal = C("cymbal");
            h.dudThud = C("dud_thud");
            h.sneeze = C("sneeze");
            if (h.drumLoop == null) Debug.LogWarning("[TrungHoaFx] Không thấy âm trong " + FxFolder + "*: chạy python3 Tools/Audio/make_trunghoa_fx.py");
        }

        private static AudioClip C(string n) => AssetDatabase.LoadAssetAtPath<AudioClip>(FxFolder + n + ".wav");

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

            int missing = 0;
            p.tickClips = Fill(p.tickClips, TrungHoaFxPreset.TickFiles, ChestFxPresets.TickFiles, ref missing);
            p.lockClipsByTier = Fill(p.lockClipsByTier, TrungHoaFxPreset.LockFiles, ChestFxPresets.LockFiles, ref missing);
            if (p.whooshClip == null) p.whooshClip = Clip(TrungHoaFxPreset.WhooshFile, ChestFxPresets.WhooshFile, ref missing);
            if (p.openAmbientClip == null) p.openAmbientClip = Clip(TrungHoaFxPreset.AmbientFile, ChestFxPresets.AmbientFile, ref missing);
            p.accentClips = Fill(p.accentClips, TrungHoaFxPreset.AccentFiles, ChestFxPresets.CoinFiles, ref missing);
            if (p.stageClips == null || p.stageClips.Length == 0)
            {
                p.stageClips = new[]
                {
                    new ChestFxProfile.StageClip { stage = 2, clip = Clip(TrungHoaFxPreset.LidFile, ChestFxPresets.CreakFile, ref missing), volume = 0.7f },
                    new ChestFxProfile.StageClip { stage = 4, clip = Clip(TrungHoaFxPreset.ThudFile, ChestFxPresets.ThudFile, ref missing), volume = 0.65f },
                };
            }
            if (missing > 0) Debug.LogWarning($"[TrungHoaFx] {missing} sound(s) not found; the effect still runs without them.");

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            if (created) Debug.Log("[TrungHoaFx] Created " + ProfilePath);
            return p;
        }

        /// <summary>Own clip in trung_hoa/, else the shared clip (same slot), else its synth fallback.</summary>
        private static AudioClip Clip(string own, string shared, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(TrungHoaFxPreset.AudioFolder + own);
            if (c == null && shared != null)
            {
                c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + shared);
                string synth = ChestFxPresets.SynthFallback(shared);
                if (c == null && synth != null) c = AssetDatabase.LoadAssetAtPath<AudioClip>(ChestFxPresets.AudioFolder + synth);
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
                string fallback = shared != null && shared.Length > 0 ? shared[System.Math.Min(i, shared.Length - 1)] : null;
                result[i] = existing != null ? existing : Clip(own[i], fallback, ref missing);
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
