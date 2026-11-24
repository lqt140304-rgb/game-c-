using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_RacQuy (thùng rác tròn móp, tem "QUÝ"): a lazy, chaotic, smelly opening. The roulette tumbles at random
    /// like junk rattling in a bin (Shuffle), slower than the other chests, with a high near-miss chance (the bin
    /// "burps" past the result and falls back). Particles are fat yellow-green stink puffs drifting up and orbiting
    /// (Swirl, low speed, big size); the lock burst is heavy (gravity) like garbage bits falling back. Rays are short
    /// and spin backwards. Extras (flies, stink cloud, holy column on Huyền thoại) live in <see cref="RacQuyHook"/>.
    /// Sounds are filled by Editor/RacQuyFxSetup.cs. Spec: docs/superpowers/specs/2026-11-24-hieu-ung-rac-quy-design.md
    /// </summary>
    public static class RacQuyFxPreset
    {
        public const string ChestId = "Chest_RacQuy";

        public static void Apply(ChestFxProfile p)
        {
            p.chestId = ChestId;

            p.tickVolume = 0.45f;
            p.tickPitchStart = 0.8f;    // tin can taps start low and dull...
            p.tickPitchEnd = 1.25f;     // ...and only rise a little: the bin is lazy
            p.lockVolume = 0.95f;
            p.whooshVolume = 0.65f;
            p.ambientVolume = 0.45f;
            p.accentVolume = 0.4f;
            p.accentsBase = 1;          // one rolling can after the lock, whatever the tier
            p.accentsPerTier = 0;

            p.secondaryColor = new Color(0.62f, 0.78f, 0.18f);   // sickly yellow-green stink
            p.secondaryMix = 0.75f;
            p.particleLook = ParticleLook.SoftDot;
            p.particleDirection = ParticleDirection.Swirl;
            p.ambientRate = 16f;
            p.burstCount = 34;
            p.particleSpeed = 0.28f;
            p.particleSize = 0.055f;
            p.particleLifetime = 2.1f;
            p.burstGravity = 0.6f;

            p.rouletteStyle = RouletteStyle.Shuffle;
            p.minInterval = 0.07f;
            p.maxInterval = 0.5f;
            // Stays fast and messy for long, then gives up suddenly (like a can rolling to a stop).
            p.slowdownCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.7f, 0.12f, 0.4f, 0.4f), new Keyframe(1f, 1f, 4f, 0f));
            p.nearMissChance = 0.8f;
            p.minRouletteSeconds = 3.0f;

            p.rayLengthMultiplier = 0.8f;
            p.rayDuration = 0.9f;
            p.raySpin = -40f;
            p.lightFlashIntensity = 4.5f;
        }
    }
}
