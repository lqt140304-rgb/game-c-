using System;
using System.Collections.Generic;
using System.Reflection;
using ChoDoLa.Interaction;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Every chest FX profile plus the demo items, as plain references. Filled by the editor menu
    /// Tools/Cho Do La/Create All Chest FX Profiles (ScriptableObjects/ChestFx/ChestFxLibrary.asset); nothing is
    /// scanned at runtime. Used by ChestHandMenu's [P] key ("open with the full effect") through <see cref="PlayOn"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Cho Do La/Chest FX Library", fileName = "ChestFxLibrary")]
    public class ChestFxLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Chest model name, e.g. Chest_Golden (same as the profile's chestId).")]
            public string chestId;
            public ChestFxProfile profile;
            [Tooltip("Optional: a hook that the chest's own setup menu puts ON the chest root (not via customFxPrefab). " +
                     "This is a configured component on a prefab asset; at [P] the same type is added to the held chest and its fields are copied.")]
            public ChestFxHook rootHookTemplate;
            [Tooltip("Turn on BoxOpenSequence.biteIntro for this chest (Mimic).")]
            public bool biteIntro;
            [Tooltip("Turn on BoxOpenSequence.spinRecord for this chest (Âm nhạc).")]
            public bool spinRecord;
        }

        public List<Entry> entries = new List<Entry>();

        [Tooltip("Demo items (Art/Items/LowPoly/**/*.glb) used for the silhouette.")]
        public List<GameObject> items = new List<GameObject>();

        /// <summary>Silhouette size (metres) for a chest at full size; scaled down with the chest in the hand.</summary>
        public float silhouetteSizeAtFullScale = 0.3f;

        /// <summary>Entry for a chest model name: exact match first, then the longest chestId it starts with.</summary>
        public Entry Find(string chestName)
        {
            if (string.IsNullOrEmpty(chestName)) return null;
            Entry best = null;
            foreach (Entry e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.chestId)) continue;
                if (string.Equals(e.chestId, chestName, StringComparison.OrdinalIgnoreCase)) return e;
                if (chestName.StartsWith(e.chestId, StringComparison.OrdinalIgnoreCase) && (best == null || e.chestId.Length > best.chestId.Length)) best = e;
            }
            return best;
        }

        public GameObject RandomItem() => items.Count > 0 ? items[UnityEngine.Random.Range(0, items.Count)] : null;

        /// <summary>
        /// Full reveal on a chest rig (the object holding BoxOpenSequence): adds ChestRevealSequence if missing, loads the
        /// matching profile, adds the root hook if needed, scales the silhouette with <paramref name="chestScale"/> and plays.
        /// Returns the reveal, or null when <paramref name="rig"/> is null. <paramref name="message"/> says what was missing.
        /// </summary>
        public ChestRevealSequence PlayOn(Transform rig, string chestName, ChestTier tier, GameObject item, float chestScale, out string message)
        {
            message = string.Empty;
            if (rig == null) { message = "Không có rương."; return null; }

            BoxOpenSequence box = rig.GetComponent<BoxOpenSequence>();
            Entry e = Find(chestName);
            if (e == null || e.profile == null) message = $"Chưa có hồ sơ hiệu ứng cho {chestName}: chạy Tools/Cho Do La/Create All Chest FX Profiles.";

            if (e != null && box != null)
            {
                if (e.biteIntro) SetPrivate(box, "biteIntro", true);
                if (e.spinRecord) SetPrivate(box, "spinRecord", true);
            }

            ChestRevealSequence reveal = rig.GetComponent<ChestRevealSequence>();
            if (reveal == null) reveal = rig.gameObject.AddComponent<ChestRevealSequence>();
            reveal.SetProfile(e != null ? e.profile : null);
            SetPrivate(reveal, "targetSize", silhouetteSizeAtFullScale * Mathf.Max(0.01f, chestScale));
            reveal.ResetReveal();   // also adds the AudioSource, which some hooks look up in Awake

            // Added after the reveal so hooks that grab BoxOpenSequence / AudioSource in Awake find them.
            if (e != null && e.rootHookTemplate != null)
            {
                Type hookType = e.rootHookTemplate.GetType();
                if (rig.GetComponent(hookType) == null)
                {
                    Component hook = rig.gameObject.AddComponent(hookType);
                    CopySerializedFields(e.rootHookTemplate, hook);
                }
            }

            reveal.Play(tier, item);

            // The effect root is created in world space; keep it with the chest so it follows the hand.
            FieldInfo fxRoot = typeof(ChestRevealSequence).GetField("_fxRoot", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fxRoot != null && fxRoot.GetValue(reveal) is Transform t && t != null) t.SetParent(rig, true);
            return reveal;
        }

        private static void SetPrivate(object target, string field, object value)
        {
            FieldInfo f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null && f.FieldType.IsInstanceOfType(value)) f.SetValue(target, value);
            else Debug.LogWarning($"[ChestFxLibrary] {target.GetType().Name} không có trường {field}; bỏ qua.");
        }

        /// <summary>Copy every field Unity would serialise (public, or [SerializeField]) from one component to another of the same type.</summary>
        private static void CopySerializedFields(Component from, Component to)
        {
            for (Type t = from.GetType(); t != null && t != typeof(MonoBehaviour); t = t.BaseType)
            {
                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (f.IsNotSerialized || f.IsInitOnly) continue;
                    if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) continue;
                    f.SetValue(to, f.GetValue(from));
                }
            }
        }
    }
}
