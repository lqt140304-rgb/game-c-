#if UNITY_EDITOR
using System.Reflection;
using ChoDoLa.ChestFx;
using ChoDoLa.Interaction;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Gắn kịch bản "Rương Đói" (MimicFeedHook) lên rương Mimic đang chọn: tắt bite intro cũ, gỡ MimicHook cũ
    /// (hai hook cùng co giãn rương sẽ đánh nhau), gán âm trong Audio/Chests/mimic_feed/. Profile ChestFx_Mimic
    /// vẫn do MimicFxSetup tạo (màu, hạt, tiếng chốt) — menu này gọi nó luôn.
    /// </summary>
    public static class MimicFeedSetup
    {
        public const string AudioFolder = "Assets/_Project/Audio/Chests/mimic_feed/";

        [MenuItem("Tools/Cho Do La/Mimic: Them kich ban Ruong Doi vao ruong dang chon")]
        public static void AddToSelected()
        {
            GameObject go = Selection.activeGameObject;
            BoxOpenSequence box = go != null ? go.GetComponent<BoxOpenSequence>() : null;
            if (box == null || go.GetComponent<ChestRevealSequence>() == null)
            {
                Debug.LogError("[MimicFeed] Chọn rig rương Mimic (có BoxOpenSequence + ChestRevealSequence) trước.");
                return;
            }
            Undo.RecordObject(box, "Mimic Ruong Doi");
            go.GetComponent<ChestRevealSequence>().SetProfile(MimicFxSetup.CreateOrUpdateProfile());

            MimicHook old = go.GetComponent<MimicHook>();
            if (old != null) Undo.DestroyObjectImmediate(old);
            FieldInfo bite = typeof(BoxOpenSequence).GetField("biteIntro", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bite != null) bite.SetValue(box, false);   // màn cho ăn thay cho cú đớp mở đầu

            MimicFeedHook h = go.GetComponent<MimicFeedHook>();
            if (h == null) h = Undo.AddComponent<MimicFeedHook>(go);
            FillClips(h);
            TuneProfile(go.GetComponent<ChestRevealSequence>().Profile, h);
            EditorUtility.SetDirty(h);
            EditorUtility.SetDirty(box);
            Debug.Log("[MimicFeed] Đã gắn Rương Đói. Play → mở rương → bấm chuột vào đồ rơi (hoặc phím F) để cho ăn.");
        }

        /// <summary>Phần chỉnh hồ sơ ChestFx_Mimic cho màn Rương Đói (dùng chung với thư viện menu cầm rương).</summary>
        public static void TuneProfile(ChestFxProfile prof, MimicFeedHook h)
        {
            // Nối khung: ChestTension (âm căng từ lúc bấm, rẽ theo bậc lúc chốt) dùng âm của Mimic.
            if (prof != null && prof.tension != null)
            {
                prof.tension.enabled = true;
                prof.tension.loopClip = h.stomachLoopClip;
                prof.tension.pulseClip = h.heartbeatClip;
                prof.tension.deflateClip = h.deflateClip;
                prof.tension.swellClip = h.boomClip;
                prof.tension.pitchEnd = 1.5f;   // bụng réo, không phải còi: đừng vọt quá cao
                EditorUtility.SetDirty(prof);
            }
        }

        /// <summary>Gán 16 âm trong Audio/Chests/mimic_feed/.</summary>
        public static void FillClips(MimicFeedHook h)
        {
            int missing = 0;
            h.wakeClip = Clip("sfx_mf_wake.wav", ref missing);
            h.stomachLoopClip = Clip("sfx_mf_stomach_loop.wav", ref missing);
            h.heartbeatClip = Clip("sfx_mf_heartbeat.wav", ref missing);
            h.chompClip = Clip("sfx_mf_chomp.wav", ref missing);
            h.chewClip = Clip("sfx_mf_chew.wav", ref missing);
            h.gulpClip = Clip("sfx_mf_gulp.wav", ref missing);
            h.burpSmallClip = Clip("sfx_mf_burp_small.wav", ref missing);
            h.burpBigClip = Clip("sfx_mf_burp_big.wav", ref missing);
            h.riserClip = Clip("sfx_mf_riser.wav", ref missing);
            h.deflateClip = Clip("sfx_mf_deflate.wav", ref missing);
            h.boomClip = Clip("sfx_mf_boom.wav", ref missing);
            h.spitClip = Clip("sfx_mf_spit.wav", ref missing);
            h.buzzLoopClip = Clip("sfx_mf_buzz_loop.wav", ref missing);
            h.tossClip = Clip("sfx_mf_toss.wav", ref missing);
            h.hopClip = Clip("sfx_mf_hop.wav", ref missing);
            h.giggleClip = Clip("sfx_mf_giggle.wav", ref missing);
            if (missing > 0) Debug.LogWarning($"[MimicFeed] Thiếu {missing} file âm trong {AudioFolder} (chạy Tools/Audio/make_mimic_feed_sfx.py).");
        }

        private static AudioClip Clip(string file, ref int missing)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file);
            if (c == null) missing++;
            return c;
        }
    }
}
#endif
