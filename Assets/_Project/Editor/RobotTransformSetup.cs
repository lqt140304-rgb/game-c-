#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Robot Bien Hinh (gan len ruong dang chon): gắn RobotTransformHook lên rương đang chọn (có
    /// ChestRevealSequence), gỡ RobotHook cũ để không chồng hiệu ứng, và điền âm từ Audio/Chests/robot/ (.mp3 ElevenLabs).
    /// Tạo âm: python3 -I Tools/Audio/make_elevenlabs_chest_sfx.py --chest robot
    /// </summary>
    public static class RobotTransformSetup
    {
        private const string Folder = "Assets/_Project/Audio/Chests/robot/";

        [MenuItem("Tools/Cho Do La/Robot Bien Hinh (gan len ruong dang chon)")]
        public static void Menu()
        {
            GameObject sel = Selection.activeGameObject;
            if (sel == null || sel.GetComponent<ChestRevealSequence>() == null)
            {
                Debug.LogWarning("[RobotBienHinh] Chọn gốc rương Chest_Robot (có ChestRevealSequence) rồi chạy lại.");
                return;
            }
            RobotHook old = sel.GetComponent<RobotHook>();
            if (old != null) Undo.DestroyObjectImmediate(old);
            RobotTransformHook h = sel.GetComponent<RobotTransformHook>();
            if (h == null) h = Undo.AddComponent<RobotTransformHook>(sel);
            Fill(h);
            EditorUtility.SetDirty(sel);
            Debug.Log("[RobotBienHinh] Đã gắn RobotTransformHook lên " + sel.name);
        }

        public static void Fill(RobotTransformHook h)
        {
            // Âm ElevenLabs .mp3 theo Tools/Audio/elevenlabs_chest_manifest.json (mục "robot"); file thiếu thì để trống.
            h.pressClip = Load("sfx_robot_press.mp3");
            h.humLoopClip = Load("sfx_robot_hum_loop.mp3");
            h.beepClip = Load("sfx_robot_beep.mp3");
            h.slotClip = Load("sfx_robot_slot.mp3");
            h.servoClip = Load("sfx_robot_servo.mp3");
            h.sparkClip = Load("sfx_robot_spark.mp3");
            h.stingClips = new[]
            {
                Load("sfx_robot_lock_rac.mp3"), Load("sfx_robot_lock_thuong.mp3"), Load("sfx_robot_lock_hiem.mp3"),
                Load("sfx_robot_lock_ratHiem.mp3"), Load("sfx_robot_lock_huyenThoai.mp3"),
            };
        }

        private static AudioClip Load(string f)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + f);
            if (c == null) Debug.LogWarning("[RobotBienHinh] Thiếu âm " + Folder + f + ".");
            return c;
        }
    }
}
#endif
