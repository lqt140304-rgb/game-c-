using System.Collections;
using UnityEngine;
using ChoDoLa.Contracts;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Procedural Web/Desktop-friendly SFX manager synthesizing coin chimes, box unsealing,
    /// rarity reveal harmonies, debt stamp, and Bí Khế / Combo fanfare via AudioClip.Create.
    /// </summary>
    public class Sprint1AudioManager : MonoBehaviour
    {
        [SerializeField] private Sprint1MarketService marketService;
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 0.45f;

        private AudioSource _source;
        private AudioClip _coinClip;
        private AudioClip _unsealClip;
        private AudioClip _rarityCommonClip;
        private AudioClip _rarityUncommonClip;
        private AudioClip _rarityRareClip;
        private AudioClip _rarityMythicClip;
        private AudioClip _fanfareClip;
        private AudioClip _debtClip;
        private AudioClip _rejectClip;

        private int _lastCash = -1;
        private bool _isSubscribed;

        public void Configure(Sprint1MarketService service)
        {
            Unsubscribe();
            marketService = service;
            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            BuildAllProceduralClips();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_isSubscribed || marketService == null) return;
            _isSubscribed = true;
            _lastCash = marketService.CurrentRunState != null ? marketService.CurrentRunState.Cash : -1;

            marketService.OnBoxPurchased += HandleBoxPurchased;
            marketService.OnBoxOpened += HandleBoxOpened;
            marketService.OnRunStateChanged += HandleRunStateChanged;
            marketService.OnDebtSettled += HandleDebtSettled;
            marketService.OnActionRejected += HandleActionRejected;
            marketService.OnTactLoadoutChanged += HandleTactLoadoutChanged;
            marketService.OnBikheUnlocked += HandleBikheUnlocked;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || marketService == null) return;
            _isSubscribed = false;

            marketService.OnBoxPurchased -= HandleBoxPurchased;
            marketService.OnBoxOpened -= HandleBoxOpened;
            marketService.OnRunStateChanged -= HandleRunStateChanged;
            marketService.OnDebtSettled -= HandleDebtSettled;
            marketService.OnActionRejected -= HandleActionRejected;
            marketService.OnTactLoadoutChanged -= HandleTactLoadoutChanged;
            marketService.OnBikheUnlocked -= HandleBikheUnlocked;
        }

        private void HandleBoxPurchased(BoxInstanceView _)
        {
            PlayClip(_coinClip, 0.9f);
        }

        private void HandleBoxOpened(BoxInstanceView _, ArtifactInstanceView artifact)
        {
            PlayClip(_unsealClip, 0.85f);
            RarityTier tier = artifact != null ? artifact.Rarity : RarityTier.Common;
            StartCoroutine(PlayDelayedRarityChord(tier, 0.42f));
        }

        private void HandleRunStateChanged(RunStateSnapshot s)
        {
            if (s == null) return;
            if (_lastCash >= 0 && s.Cash > _lastCash)
            {
                PlayClip(_coinClip, 0.85f);
            }
            _lastCash = s.Cash;
        }

        private void HandleDebtSettled(bool paid, int _)
        {
            PlayClip(paid ? _debtClip : _rejectClip, 0.95f);
        }

        private void HandleActionRejected(string _, string __)
        {
            PlayClip(_rejectClip, 0.75f);
        }

        private void HandleTactLoadoutChanged()
        {
            PlayClip(_rarityUncommonClip, 0.55f);
        }

        private void HandleBikheUnlocked(BikheView _)
        {
            PlayClip(_fanfareClip, 1.0f);
        }

        private IEnumerator PlayDelayedRarityChord(RarityTier rarity, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioClip clip = rarity switch
            {
                RarityTier.Uncommon => _rarityUncommonClip,
                RarityTier.Rare => _rarityRareClip,
                RarityTier.Mythic => _rarityMythicClip,
                _ => _rarityCommonClip
            };
            PlayClip(clip, 0.95f);
        }

        private void PlayClip(AudioClip clip, float scale)
        {
            if (_source != null && clip != null)
            {
                _source.PlayOneShot(clip, masterVolume * scale);
            }
        }

        private void BuildAllProceduralClips()
        {
            _coinClip = BuildArpeggioClip("SFX_Coin", new[] { 987.77f, 1318.51f }, 0.065f, 0.14f, 0.35f);
            _unsealClip = BuildSweepClip("SFX_Unseal", 210f, 440f, 0.18f, 0.35f);
            _rarityCommonClip = BuildChordClip("SFX_RarityCommon", new[] { 440f, 554.37f }, 0.30f, 0.28f);
            _rarityUncommonClip = BuildChordClip("SFX_RarityUncommon", new[] { 523.25f, 659.25f, 783.99f }, 0.38f, 0.28f);
            _rarityRareClip = BuildArpeggioClip("SFX_RarityRare", new[] { 587.33f, 739.99f, 880.00f }, 0.06f, 0.32f, 0.32f);
            _rarityMythicClip = BuildArpeggioClip("SFX_RarityMythic", new[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.07f, 0.48f, 0.36f);
            _fanfareClip = BuildArpeggioClip("SFX_BikheFanfare", new[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f }, 0.075f, 0.45f, 0.38f);
            _debtClip = BuildArpeggioClip("SFX_DebtPaid", new[] { 293.66f, 440f, 587.33f }, 0.07f, 0.26f, 0.34f);
            _rejectClip = BuildArpeggioClip("SFX_Reject", new[] { 196f, 155.56f }, 0.08f, 0.12f, 0.30f);
        }

        private static AudioClip BuildChordClip(string clipName, float[] freqs, float duration, float peakGain)
        {
            const int sampleRate = 22050;
            int totalSamples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float attack = Mathf.Clamp01(t / 0.015f);
                float decay = Mathf.Exp(-4.5f * t / duration);
                float sum = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                }
                data[i] = (sum / freqs.Length) * peakGain * attack * decay;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildArpeggioClip(string clipName, float[] freqs, float stepTime, float noteDecay, float peakGain)
        {
            const int sampleRate = 22050;
            float totalDuration = (freqs.Length - 1) * stepTime + noteDecay;
            int totalSamples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * totalDuration));
            float[] data = new float[totalSamples];

            for (int n = 0; n < freqs.Length; n++)
            {
                float start = n * stepTime;
                int startSample = Mathf.RoundToInt(start * sampleRate);
                int len = Mathf.RoundToInt(noteDecay * sampleRate);
                for (int j = 0; j < len && startSample + j < totalSamples; j++)
                {
                    float lt = (float)j / sampleRate;
                    float attack = Mathf.Clamp01(lt / 0.010f);
                    float decay = Mathf.Exp(-5.0f * lt / noteDecay);
                    float sample = Mathf.Sin(2f * Mathf.PI * freqs[n] * lt) * peakGain * attack * decay;
                    data[startSample + j] = Mathf.Clamp(data[startSample + j] + sample * 0.7f, -0.95f, 0.95f);
                }
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildSweepClip(string clipName, float fStart, float fEnd, float duration, float peakGain)
        {
            const int sampleRate = 22050;
            int totalSamples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[totalSamples];
            float phase = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float norm = (float)i / totalSamples;
                float freq = Mathf.Lerp(fStart, fEnd, norm);
                phase += (2f * Mathf.PI * freq) / sampleRate;
                float env = Mathf.Clamp01(norm / 0.08f) * Mathf.Exp(-3.5f * norm);
                data[i] = Mathf.Sin(phase) * peakGain * env;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
