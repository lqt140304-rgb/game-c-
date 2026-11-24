using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Base class for a chest's own extra effect (phase 2: one style per chest). Put a subclass on the
    /// profile's customFxPrefab, or directly on the chest object: <see cref="ChestRevealSequence"/> finds it and
    /// calls these methods. Override only what you need. Hooks must not change the rolled tier; they only decorate.
    /// </summary>
    public abstract class ChestFxHook : MonoBehaviour
    {
        /// <summary>The roulette starts. <paramref name="mouth"/> is the world position of the chest mouth (Glow).</summary>
        public virtual void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth) { }

        /// <summary>The roulette shows a new colour (called for every tick).</summary>
        public virtual void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color) { }

        /// <summary>The roulette stops on the rolled tier.</summary>
        public virtual void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color) { }

        /// <summary>The silhouette reached its full size.</summary>
        public virtual void OnSilhouetteComplete(ChestRevealSequence reveal, Transform silhouette) { }

        /// <summary>The box is open and the silhouette complete.</summary>
        public virtual void OnRevealed(ChestRevealSequence reveal) { }

        /// <summary>The chest was reset to closed: clean up.</summary>
        public virtual void OnReset(ChestRevealSequence reveal) { }
    }
}
