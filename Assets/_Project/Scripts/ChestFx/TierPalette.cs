using System;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>Shape of the light rays fired when the roulette locks on a tier.</summary>
    public enum RayShape
    {
        Stub,     // short blunt rays (Rác: a "pfff")
        Beam,     // straight rectangular beams
        Spike,    // thin pointed triangles
        Diamond,  // rhombus rays
        Star,     // alternating long and short spikes
    }

    /// <summary>
    /// Colours and shape cues of the five tiers. The colours are a PROPOSAL waiting for the project owner
    /// (see docs/superpowers/specs/2026-11-24-hieu-ung-mo-rong-design.md). Every tier also differs by
    /// ray count, ray shape, ring count and the number of rank pips, so it reads without colour vision.
    /// Create one with Assets > Create > Cho Do La > Tier Palette, or leave the field empty to use the defaults.
    /// </summary>
    [CreateAssetMenu(menuName = "Cho Do La/Tier Palette", fileName = "TierPalette")]
    public class TierPalette : ScriptableObject
    {
        [Serializable]
        public class TierStyle
        {
            public ChestTier tier;
            public Color color = Color.white;
            [Tooltip("Number of light rays when the roulette locks on this tier.")] public int rayCount = 4;
            public RayShape rayShape = RayShape.Beam;
            [Tooltip("Ray length relative to the chest size.")] public float rayLength = 1f;
            [Tooltip("Expanding rings when locking (0..3).")] public int ringCount = 1;
            [Tooltip("Brightness multiplier for the flash, light and particles.")] public float intensity = 1f;
        }

        [SerializeField] private TierStyle[] styles = CreateDefaultStyles();

        public TierStyle Get(ChestTier tier)
        {
            if (styles != null)
            {
                for (int i = 0; i < styles.Length; i++)
                {
                    if (styles[i] != null && styles[i].tier == tier) return styles[i];
                }
            }
            return DefaultStyles[(int)tier];
        }

        public Color ColorOf(ChestTier tier) => Get(tier).color;

        private static TierStyle[] _defaultStyles;
        private static TierStyle[] DefaultStyles => _defaultStyles ?? (_defaultStyles = CreateDefaultStyles());

        private static TierPalette _runtimeDefault;

        /// <summary>A palette with the proposed default values, used when none is assigned.</summary>
        public static TierPalette Default
        {
            get
            {
                if (_runtimeDefault == null)
                {
                    _runtimeDefault = CreateInstance<TierPalette>();
                    _runtimeDefault.name = "TierPalette (default)";
                    _runtimeDefault.hideFlags = HideFlags.DontSave;
                }
                return _runtimeDefault;
            }
        }

        /// <summary>Proposed values (not approved yet). Rác grey-brown, Thường ivory, Hiếm blue, Rất hiếm purple, Huyền thoại gold-orange.</summary>
        public static TierStyle[] CreateDefaultStyles()
        {
            return new[]
            {
                new TierStyle { tier = ChestTier.Rac,        color = Hex(0x8A7D6B), rayCount = 3,  rayShape = RayShape.Stub,    rayLength = 0.45f, ringCount = 0, intensity = 0.45f },
                new TierStyle { tier = ChestTier.Thuong,     color = Hex(0xF3ECD6), rayCount = 4,  rayShape = RayShape.Beam,    rayLength = 0.75f, ringCount = 1, intensity = 0.75f },
                new TierStyle { tier = ChestTier.Hiem,       color = Hex(0x3D8BFF), rayCount = 6,  rayShape = RayShape.Spike,   rayLength = 1.0f,  ringCount = 1, intensity = 1.0f },
                new TierStyle { tier = ChestTier.RatHiem,    color = Hex(0xA64DFF), rayCount = 8,  rayShape = RayShape.Diamond, rayLength = 1.25f, ringCount = 2, intensity = 1.35f },
                new TierStyle { tier = ChestTier.HuyenThoai, color = Hex(0xFFA41C), rayCount = 12, rayShape = RayShape.Star,    rayLength = 1.6f,  ringCount = 3, intensity = 1.8f },
            };
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }
    }
}
