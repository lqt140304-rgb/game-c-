#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Gắn kịch bản ĐÀO MỎ (MinerDigSequence) lên rương Thợ Mỏ đang chọn. Gỡ MinerHook cũ trên chính rương (tránh trùng
    /// đá/đèn), gán hồ sơ ChestFx_Miner. Nhớ: nơi bấm mở phải gọi MinerDigSequence.TryBegin(...) trước reveal.Play.
    /// </summary>
    public static class MinerDigSetup
    {
        [MenuItem("Tools/Cho Do La/Add Miner Dig Sequence To Selected Chest")]
        public static void AddMenu()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) { Debug.LogWarning("[MinerDig] Chọn gốc rương (vật có ChestRevealSequence) trước."); return; }
            ChestRevealSequence reveal = go.GetComponent<ChestRevealSequence>();
            if (reveal == null) { Debug.LogWarning("[MinerDig] " + go.name + " không có ChestRevealSequence."); return; }
            MinerHook old = go.GetComponent<MinerHook>();
            if (old != null) Undo.DestroyObjectImmediate(old);
            if (go.GetComponent<MinerDigSequence>() == null) Undo.AddComponent<MinerDigSequence>(go);
            reveal.SetProfile(MinerFxSetup.CreateOrUpdate());
            EditorUtility.SetDirty(go);
            Debug.Log("[MinerDig] Đã gắn MinerDigSequence lên " + go.name + ". Thử: Play, gọi MinerDigSequence.TryBegin; F/chuột trái = đập, G = thuốc nổ.");
        }
    }
}
#endif
