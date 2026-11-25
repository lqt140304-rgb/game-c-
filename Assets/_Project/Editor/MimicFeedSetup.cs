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
        public const string AudioFolder = "Assets/_Project/Audio/Chests/mimic/";
        public const string SkinTexture = "Assets/_Project/Art/Source/MeshyChests/Mimic/Meshy_AI_Monster_Mimic_Chest_20261008_texture.png";

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

        /// <summary>Phần chỉnh hồ sơ ChestFx_Mimic cho màn Rương Đói: âm lõi tắt (A1), hook tự phát nền/nhịp/chốt.</summary>
        public static void TuneProfile(ChestFxProfile prof, MimicFeedHook h)
        {
            if (prof == null) return;
            ChestAudioRule.ApplyToProfile(prof);   // MỖI lần chạy: tension.enabled = false, ô âm lõi trống
            EditorUtility.SetDirty(prof);
        }

        /// <summary>Gán âm .mp3 trong Audio/Chests/mimic/ (theo Tools/Audio/elevenlabs_chest_manifest.json) và ảnh da rương.</summary>
        public static void FillClips(MimicFeedHook h)
        {
            int missing = 0;
            h.stingClips = new[]
            {
                Clip("sfx_mimic_lock_rac.mp3", ref missing), Clip("sfx_mimic_lock_thuong.mp3", ref missing),
                Clip("sfx_mimic_lock_hiem.mp3", ref missing), Clip("sfx_mimic_lock_ratHiem.mp3", ref missing),
                Clip("sfx_mimic_lock_huyenThoai.mp3", ref missing),
            };
            h.stomachLoopClip = Clip("sfx_mimic_stomach_loop.mp3", ref missing);
            h.heartbeatClip = Clip("sfx_mimic_tick_heartbeat.mp3", ref missing);
            h.chompClip = Clip("sfx_mimic_bite.mp3", ref missing);
            h.chewClip = Clip("sfx_mimic_chew.mp3", ref missing);
            h.gulpClip = Clip("sfx_mimic_gulp.mp3", ref missing);
            h.burpSmallClip = Clip("sfx_mimic_burp_small.mp3", ref missing);
            h.burpBigClip = Clip("sfx_mimic_burp_big.mp3", ref missing);
            h.deflateClip = Clip("sfx_mimic_deflate.mp3", ref missing);
            h.boomClip = Clip("sfx_mimic_boom.mp3", ref missing);
            h.spitClip = Clip("sfx_mimic_whoosh_spit.mp3", ref missing);
            h.tossClip = Clip("sfx_mimic_toss.mp3", ref missing);
            h.hopClip = Clip("sfx_mimic_hop.mp3", ref missing);
            h.giggleClip = Clip("sfx_mimic_teeth_chatter.mp3", ref missing);
            h.wakeClip = null;   // không có trong danh mục âm ElevenLabs; để trống
            if (missing > 0) Debug.LogWarning($"[MimicFeed] Thiếu {missing} file âm trong {AudioFolder}.");

            // Rương trắng: gắn ảnh da Meshy làm dự phòng khi vật liệu GLB không có ảnh nền.
            if (h.skinTexture == null)
            {
                h.skinTexture = AssetDatabase.LoadAssetAtPath<Texture>(SkinTexture);
                if (h.skinTexture == null) Debug.LogWarning($"[MimicFeed] Không thấy ảnh da {SkinTexture}.");
            }
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
