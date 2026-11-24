using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// One pose of the inspect flourish. Euler angles rotate the item in VIEW space
    /// (X pitch: negative tilts the top toward the player, positive shows the bottom;
    /// Y yaw: spins the item around its vertical axis), on top of the item's base pose.
    /// </summary>
    [Serializable]
    public struct InspectStop
    {
        public string label;      // Shown on screen while this pose is held (e.g. an inspect sign from content.json).
        public Vector3 euler;     // View-space rotation added to the base rotation.
        public Vector3 offset;    // Position offset from the rest position (metres, camera space).
        public float travel;      // Seconds to move from the previous pose to this one.
        public float hold;        // Seconds to stay on this pose.

        public InspectStop(string label, Vector3 euler, Vector3 offset, float travel = 0.5f, float hold = 0.55f)
        {
            this.label = label;
            this.euler = euler;
            this.offset = offset;
            this.travel = travel;
            this.hold = hold;
        }
    }

    /// <summary>
    /// How an item is held and inspected in first person. Presets are keyed by item id
    /// (ids match Docs/Content/data/content.json). Unknown ids fall back to a generic flourish.
    /// </summary>
    [Serializable]
    public class HeldItemProfile
    {
        public string itemId;
        public float targetSize = 0.28f;                          // Longest side of the held item, in metres.
        public Vector3 baseEuler = new Vector3(8f, -25f, 0f);     // Resting tilt toward the viewer.
        public List<InspectStop> stops = new List<InspectStop>();

        private static readonly Vector3 Close = new Vector3(0f, 0.02f, 0.13f);
        private static readonly Vector3 Closer = new Vector3(0f, 0.03f, 0.18f);

        public static HeldItemProfile For(string itemId)
        {
            HeldItemProfile profile = new HeldItemProfile { itemId = itemId };

            // Chests (Assets/_Project/Art/Chests/Chest_*.glb) share one preset, whatever their name.
            if (itemId != null && itemId.StartsWith("Chest_", StringComparison.Ordinal))
            {
                ApplyChestPreset(profile);
                return profile;
            }

            switch (itemId)
            {
                // ---- Items (inspect signs come from content.json) ----
                case "V01": // Vase: crazing near the rim, seal under the base.
                    profile.stops.Add(new InspectStop("Vết rạn men gần miệng", new Vector3(-50f, 20f, 0f), Closer));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(-10f, 200f, 0f), Close));
                    profile.stops.Add(new InspectStop("Dấu triện dưới đáy", new Vector3(130f, 0f, 0f), Closer, 0.6f, 0.8f));
                    break;
                case "V02": // Book: faded text, repaired spine.
                    profile.baseEuler = new Vector3(-10f, -35f, 0f);
                    profile.stops.Add(new InspectStop("Chữ mờ nhưng còn mục lục", new Vector3(-30f, 25f, 0f), Closer));
                    profile.stops.Add(new InspectStop("Gáy sách có vết sửa", new Vector3(0f, 90f, 0f), Close, 0.5f, 0.8f));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 200f, 0f), Close));
                    break;
                case "V03": // Pendulum clock.
                    profile.targetSize = 0.34f;
                    profile.stops.Add(new InspectStop("Kim bị gãy hoặc vẫn chạy", new Vector3(-8f, 10f, 0f), Closer, 0.5f, 0.8f));
                    profile.stops.Add(new InspectStop("Mặt kính có vết mờ", new Vector3(-20f, -30f, 0f), Closer));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 180f, 0f), Close));
                    break;
                case "V04": // Astronomical clock glass shard.
                    profile.stops.Add(new InspectStop("Khung kim loại in số", new Vector3(-10f, 15f, 0f), Closer));
                    profile.stops.Add(new InspectStop("Mép kính có vết cắt thô", new Vector3(0f, 80f, 0f), Closer, 0.5f, 0.8f));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 190f, 0f), Close));
                    break;
                case "V05": // Broken communicator.
                    profile.stops.Add(new InspectStop("Màn hình vỡ", new Vector3(-15f, 10f, 0f), Closer));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 180f, 0f), Close));
                    profile.stops.Add(new InspectStop("Mã máy còn đọc được", new Vector3(120f, 180f, 0f), Closer, 0.6f, 0.8f));
                    break;
                case "V06": // Quantum battery.
                    profile.stops.Add(new InspectStop("Nhãn in mờ", new Vector3(-12f, 15f, 0f), Closer));
                    profile.stops.Add(new InspectStop("Linh kiện bạc còn sáng", new Vector3(-10f, 90f, 0f), Closer, 0.5f, 0.8f));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 190f, 0f), Close));
                    break;
                case "V07": // Crystal on a handle.
                    profile.targetSize = 0.34f;
                    profile.baseEuler = new Vector3(-20f, -25f, 25f);
                    profile.stops.Add(new InspectStop("Phát sáng yếu khi xoay", new Vector3(-20f, 120f, 0f), Closer, 0.7f, 0.5f));
                    profile.stops.Add(new InspectStop("Bề mặt có vân", new Vector3(-35f, 260f, 0f), Closer, 0.7f, 0.8f));
                    break;
                case "V08": // Fairy-tale mirror.
                    profile.targetSize = 0.32f;
                    profile.stops.Add(new InspectStop("Ảnh phản chiếu lệch góc so với người soi", new Vector3(-10f, 18f, 0f), Closer, 0.6f, 1.0f));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 180f, 0f), Close));
                    break;
                case "V09": // Yellow paper talisman.
                    profile.targetSize = 0.32f;
                    profile.stops.Add(new InspectStop("Mực đỏ dễ phai", new Vector3(-10f, 8f, 0f), Closer, 0.5f, 0.8f));
                    profile.stops.Add(new InspectStop("Có thể là đồ giả", new Vector3(0f, 180f, 0f), Closer, 0.6f, 0.8f));
                    break;
                case "V10": // Dragon scale.
                    profile.stops.Add(new InspectStop("Vết cắt sắc", new Vector3(0f, 70f, 0f), Closer));
                    profile.stops.Add(new InspectStop("Ánh kim đổi màu theo góc nhìn", new Vector3(-30f, -40f, 0f), Closer, 0.8f, 0.8f));
                    profile.stops.Add(new InspectStop(string.Empty, new Vector3(20f, 40f, 0f), Closer, 0.8f, 0.5f));
                    break;

                // ---- Models that lie flat by nature ----
                case "BK1": // Oath scroll: procedural ScrollUnroll, F opens and closes it (no inspect stops).
                    profile.targetSize = 0.36f;
                    profile.baseEuler = new Vector3(6f, -12f, 0f);
                    break;
                case "GT1": // Debt ledger.
                    profile.baseEuler = new Vector3(-55f, -20f, 0f);
                    profile.stops.AddRange(GenericStops());
                    break;

                default:
                    profile.stops.AddRange(GenericStops());
                    break;
            }

            return profile;
        }

        /// <summary>
        /// Chests are 0.3–0.6 m tall, so they are shrunk to fit the hand. ChestHandMenu turns the chest so its
        /// front (the lock) faces the camera; the base yaw turns it a little more toward the camera, because the
        /// hand sits to the right. The lid hinges at the back, so it opens away from the camera.
        /// Inspect stops move the chest toward the middle of the screen (negative X offset).
        /// </summary>
        private static void ApplyChestPreset(HeldItemProfile profile)
        {
            Vector3 centre = new Vector3(-0.2f, 0.06f, 0.1f);
            profile.targetSize = 0.26f;
            profile.baseEuler = new Vector3(14f, 22f, 0f);
            profile.stops.Add(new InspectStop("Mặt trước và ổ khóa", new Vector3(-4f, -20f, 0f), centre, 0.55f, 0.9f));
            profile.stops.Add(new InspectStop(string.Empty, new Vector3(0f, 80f, 0f), centre, 0.55f, 0.2f));
            profile.stops.Add(new InspectStop("Mặt sau và bản lề", new Vector3(5f, 160f, 0f), centre, 0.55f, 0.7f));
            profile.stops.Add(new InspectStop("Nhìn vào lòng rương", new Vector3(40f, -20f, 0f), centre, 0.65f, 0.9f));
        }

        /// <summary>Generic Valorant-style flourish: bring it close, spin, flip to the bottom, settle.</summary>
        private static IEnumerable<InspectStop> GenericStops()
        {
            yield return new InspectStop(string.Empty, new Vector3(-25f, 30f, 0f), Close);
            yield return new InspectStop(string.Empty, new Vector3(-15f, 150f, 0f), Close, 0.45f, 0.15f);
            yield return new InspectStop(string.Empty, new Vector3(-15f, 270f, 0f), Close, 0.45f, 0.15f);
            yield return new InspectStop(string.Empty, new Vector3(130f, 0f, 0f), Closer, 0.55f, 0.5f);
        }
    }
}
