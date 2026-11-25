#if UNITY_EDITOR
using ChoDoLa.ChestFx;
using UnityEditor;
using UnityEngine;

namespace ChoDoLa.Editor
{
    /// <summary>
    /// Tools/Cho Do La/Robot Bien Hinh (gan len ruong dang chon): gắn RobotTransformHook lên rương đang chọn (có
    /// ChestRevealSequence), gỡ RobotHook cũ để không chồng hiệu ứng, và điền âm từ Audio/Chests/robot/transform/.
    /// Tạo âm: python3 Tools/Audio/make_robot_sfx.py
    /// </summary>
    public static class RobotTransformSetup
    {
        private const string Folder = "Assets/_Project/Audio/Chests/robot/transform/";

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
            h.pressClip = Load("sfx_robotx_press.wav");
            h.humLoopClip = Load("sfx_robotx_hum_loop.wav");
            h.slotClip = Load("sfx_robotx_slot.wav");
            h.beepClip = Load("sfx_robotx_beep.wav");
            h.servoClip = Load("sfx_robotx_servo.wav");
            h.typeClip = Load("sfx_robotx_type.wav");
            h.droneLoopClip = Load("sfx_robotx_drone_loop.wav");
            h.fizzleClip = Load("sfx_robotx_fizzle.wav");
            h.blastClips = new[]
            {
                Load("sfx_robotx_blast_1_thuong.wav"), Load("sfx_robotx_blast_2_hiem.wav"),
                Load("sfx_robotx_blast_3_rathiem.wav"), Load("sfx_robotx_blast_4_huyenthoai.wav"),
            };
        }

        private static AudioClip Load(string f)
        {
            AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + f);
            if (c == null) Debug.LogWarning("[RobotBienHinh] Thiếu âm " + Folder + f + " (chạy Tools/Audio/make_robot_sfx.py).");
            return c;
        }
    }
}
#endif
