using System.Collections;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_Robot extras (decoration only, never touches the rolled tier):
    /// - an "eye" point light that blinks on every roulette tick in the shown tier colour;
    /// - a cog ring above the open head that spins fast and winds down with the roulette;
    /// - at the lock: Rác = short circuit (eye flickers, cog stalls and sinks, grey smoke puffs);
    ///   Thường+ = spark burst that grows with the tier, cog spins back up; Huyền thoại = eye strobes 3 times.
    /// Put it on the chest root (RobotFxSetup does) or on the profile's customFxPrefab.
    /// </summary>
    public class RobotHook : ChestFxHook
    {
        [SerializeField] private Color eyeColor = new Color(0.35f, 0.95f, 1f);
        [SerializeField] private float eyeIntensity = 2.5f;
        [SerializeField] private float cogRadius = 0.2f;
        [SerializeField] private int cogSegments = 10;        // few segments = polygon, reads as a cog
        [SerializeField] private float cogStartSpeed = 720f;  // degrees per second

        private Light _eye;
        private GameObject _cog;
        private Material _cogMat;
        private ParticleSystem _sparks, _smoke;
        private ChestFxProfile _sparkStyle, _smokeStyle;
        private float _cogSpeed, _cogAngle, _cogScale = 1f, _cogDrop;
        private bool _eyeOn;
        private Transform _chest;
        private Vector3 _mouth;
        private readonly System.Random _rng = new System.Random();

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            _chest = reveal.transform;
            _mouth = mouth;
            if (_eye == null)
            {
                GameObject e = new GameObject("RobotEye");
                e.hideFlags = HideFlags.DontSave;
                _eye = e.AddComponent<Light>();
                _eye.type = LightType.Point;
                _eye.range = 0.8f;
                _eye.shadows = LightShadows.None;
            }
            _eye.transform.position = mouth + _chest.up * 0.06f + _chest.forward * 0.12f;
            _eye.color = eyeColor;
            _eye.intensity = eyeIntensity;
            _eyeOn = true;

            if (_cog == null)
            {
                _cog = new GameObject("RobotCog");
                _cog.hideFlags = HideFlags.DontSave;
                _cog.AddComponent<MeshFilter>().sharedMesh = ChestFxFactory.CreateRingMesh(0.78f, cogSegments);
                _cogMat = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "RobotCog");
                _cog.AddComponent<MeshRenderer>().sharedMaterial = _cogMat;
            }
            _cog.SetActive(true);
            ChestFxFactory.SetColor(_cogMat, eyeColor);
            _cogSpeed = cogStartSpeed;
            _cogScale = 1f;
            _cogDrop = 0f;
            PlaceCog();
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            if (_eye == null) return;
            _eyeOn = !_eyeOn;                                 // blink: on / dim each tick
            _eye.color = color;
            _eye.intensity = _eyeOn ? eyeIntensity : eyeIntensity * 0.15f;
            if (_cogMat != null) ChestFxFactory.SetColor(_cogMat, color);
            _cogSpeed = Mathf.Max(60f, _cogSpeed * 0.94f);    // winds down with the roulette
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            StopAllCoroutines();
            if (_eye != null) { _eye.color = color; _eye.intensity = eyeIntensity * 1.6f; }
            if (_cogMat != null) ChestFxFactory.SetColor(_cogMat, color);
            if (tier == ChestTier.Rac)
            {
                StartCoroutine(ShortCircuit(reveal, color));
                return;
            }
            _cogSpeed = 240f + tier.Rank() * 260f;
            _cogScale = 1f + tier.Rank01() * 0.8f;
            EmitSparks(reveal, color, Mathf.RoundToInt(15f * (tier.Rank() + 1)));
            if (tier == ChestTier.HuyenThoai) StartCoroutine(Strobe(color, 3));
        }

        public override void OnRevealed(ChestRevealSequence reveal)
        {
            if (_eye != null && !IsShortCircuited) _eye.intensity = eyeIntensity;
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private bool IsShortCircuited => _cogSpeed == 0f;

        private void Update()
        {
            if (_cog == null || _chest == null) return;
            _cogAngle += _cogSpeed * Time.deltaTime;
            PlaceCog();
        }

        private void PlaceCog()
        {
            if (_cog == null || _chest == null) return;
            Transform t = _cog.transform;
            t.position = _mouth + _chest.up * (0.04f - _cogDrop);
            t.rotation = _chest.rotation * Quaternion.AngleAxis(_cogAngle, Vector3.up);
            float s = cogRadius * _cogScale;
            t.localScale = new Vector3(s, s, s);
        }

        private IEnumerator ShortCircuit(ChestRevealSequence reveal, Color color)
        {
            _cogSpeed = 0f;
            EmitSparks(reveal, color, 8);
            EmitSmoke(reveal, 14);
            for (int i = 0; i < 12; i++)                        // eye sputters
            {
                if (_eye != null) _eye.intensity = _rng.NextDouble() < 0.5 ? 0f : eyeIntensity * (float)_rng.NextDouble();
                _cogDrop += 0.006f;                             // cog sags into the head
                PlaceCog();
                yield return new WaitForSeconds(0.04f + 0.06f * (float)_rng.NextDouble());
            }
            if (_eye != null) _eye.intensity = eyeIntensity * 0.1f;   // "hụt hơi": stays dim
        }

        private IEnumerator Strobe(Color color, int times)
        {
            for (int i = 0; i < times; i++)
            {
                if (_eye != null) _eye.intensity = 0f;
                yield return new WaitForSeconds(0.08f);
                if (_eye != null) { _eye.color = i % 2 == 0 ? Color.white : color; _eye.intensity = eyeIntensity * 3f; }
                yield return new WaitForSeconds(0.12f);
            }
            if (_eye != null) { _eye.color = color; _eye.intensity = eyeIntensity * 1.6f; }
        }

        private void EmitSparks(ChestRevealSequence reveal, Color color, int count)
        {
            if (_sparks == null)
            {
                _sparkStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
                _sparkStyle.hideFlags = HideFlags.DontSave;
                _sparkStyle.particleLook = ParticleLook.Streak;
                _sparkStyle.particleDirection = ParticleDirection.Radial;
                _sparkStyle.particleSpeed = 1.4f;
                _sparkStyle.particleSize = 0.012f;
                _sparkStyle.particleLifetime = 0.45f;
                _sparkStyle.burstGravity = 1f;
                _sparkStyle.secondaryColor = Color.white;
                _sparkStyle.secondaryMix = 0.5f;
                _sparks = ChestFxFactory.CreateParticles("RobotSparks", null, _sparkStyle, 0.6f, true);
            }
            _sparks.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_sparks, color, _sparkStyle, 1f);
            _sparks.Play();
            _sparks.Emit(count);
        }

        private void EmitSmoke(ChestRevealSequence reveal, int count)
        {
            if (_smoke == null)
            {
                _smokeStyle = ScriptableObject.CreateInstance<ChestFxProfile>();
                _smokeStyle.hideFlags = HideFlags.DontSave;
                _smokeStyle.particleLook = ParticleLook.SoftDot;
                _smokeStyle.particleDirection = ParticleDirection.Up;
                _smokeStyle.particleSpeed = 0.2f;
                _smokeStyle.particleSize = 0.06f;
                _smokeStyle.particleLifetime = 1.6f;
                _smokeStyle.secondaryColor = new Color(0.25f, 0.25f, 0.25f);
                _smokeStyle.secondaryMix = 0.8f;
                _smoke = ChestFxFactory.CreateParticles("RobotSmoke", null, _smokeStyle, 0.6f, true);
            }
            _smoke.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_smoke, new Color(0.45f, 0.43f, 0.4f), _smokeStyle, 0.5f);
            _smoke.Play();
            _smoke.Emit(count);
        }

        private void Cleanup()
        {
            StopAllCoroutines();
            if (_eye != null) Destroy(_eye.gameObject);
            if (_cog != null)
            {
                Destroy(_cog.GetComponent<MeshFilter>().sharedMesh);
                Destroy(_cog);
            }
            if (_cogMat != null) Destroy(_cogMat);
            DestroyPs(_sparks);
            DestroyPs(_smoke);
            if (_sparkStyle != null) Destroy(_sparkStyle);
            if (_smokeStyle != null) Destroy(_smokeStyle);
            _eye = null; _cog = null; _cogMat = null; _sparks = null; _smoke = null; _sparkStyle = null; _smokeStyle = null;
            _chest = null;
        }

        private static void DestroyPs(ParticleSystem ps)
        {
            if (ps == null) return;
            Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            Destroy(ps.gameObject);
        }
    }
}
