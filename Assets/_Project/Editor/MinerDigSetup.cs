#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Gắn kịch bản ĐÀO MỎ (MinerDigSequence) lên rương Thợ Mỏ đang chọn. Gỡ MinerHook cũ trên chính rương (tránh trùng
    /// đá/đèn), gán hồ sơ ChestFx_Miner (đã mang ChestScript_Miner: cue "dao_mo" rồi OpenBox), nạp âm ElevenLabs.
    /// Kịch bản TỰ CHẠY khi bấm mở: không cần gọi MinerDigSequence.TryBegin nữa.
    /// </summary>
    public static class MinerDigSetup
    {
        private const string Folder = "Assets/_Project/Audio/Chests/miner/";

        [MenuItem("Tools/Cho Do La/Add Miner Dig Sequence To Selected Chest")]
        public static void AddMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[MinerDig] Chọn gốc rương (vật có ChestRevealSequence) trước."); return; }
            ChestRevealSequence reveal = go.GetComponent<ChestRevealSequence>();
            if (reveal == null) { Debug.LogWarning("[MinerDig] " + go.name + " không có ChestRevealSequence."); return; }
            MinerHook old = go.GetComponent<MinerHook>();
            if (old != null) Undo.DestroyObjectImmediate(old);
            MinerDigSequence dig = go.GetComponent<MinerDigSequence>();
            if (dig == null) dig = Undo.AddComponent<MinerDigSequence>(go);
            FillClips(dig);
            reveal.SetProfile(MinerFxSetup.CreateOrUpdate());
            EditorUtility.SetDirty(go);
            Debug.Log("[MinerDig] Đã gắn MinerDigSequence lên " + go.name + ". Bấm mở là tự đào; F/chuột trái = đập, G = thuốc nổ.");
        }

        /// <summary>Nạp các mp3 ElevenLabs (Tools/Audio/elevenlabs_chest_manifest.json, mục miner) vào hook. File thiếu thì để trống.</summary>
        public static void FillClips(MinerDigSequence h)
        {
            if (h == null) return;
            h.rumbleClip = Load("sfx_miner_rumble_loop.mp3");
            h.thumpClip = Load("sfx_miner_thump.mp3");
            h.pickClip = Load("sfx_miner_pick.mp3");
            h.crumbleClip = Load("sfx_miner_crumble.mp3");
            h.fuseClip = Load("sfx_miner_fuse.mp3");
            h.boomClip = Load("sfx_miner_dynamite.mp3");
            h.chirpClip = Load("sfx_miner_canary.mp3");
            h.faintClip = Load("sfx_miner_canary_faint.mp3");
            h.nuggetClip = Load("sfx_miner_nugget.mp3");
            h.pebbleClip = Load("sfx_miner_pebbles.mp3");
            h.stingClips = new[]
            {
                Load("sfx_miner_lock_rac.mp3"), Load("sfx_miner_lock_thuong.mp3"), Load("sfx_miner_lock_hiem.mp3"),
                Load("sfx_miner_lock_ratHiem.mp3"), Load("sfx_miner_lock_huyenThoai.mp3"),
            };
            EditorUtility.SetDirty(h);
        }

        private static AudioClip Load(string file)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + file);
            if (c == null) Debug.LogWarning("[MinerDig] Không thấy âm " + Folder + file + " (để trống, hiệu ứng vẫn chạy).");
            return c;
        }
    }
}
#endif
