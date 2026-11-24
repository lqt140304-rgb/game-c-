using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// Opens a mystery box slowly, in dramatic stages, to build suspense while the player waits.
    /// The box model is split into layers (see Docs/Art/BanGiao3D.md): child objects named
    /// "Lid" (pivot on the hinge), "Seal", "Binding" (cord / latch / lock) and "Glow" (light inside).
    /// Missing layers are skipped, so a box without a seal or a lock still works.
    ///
    /// Stages: 1 Rung (shake), 2 Go niem (seal and cord/lock come off), 3 He nap (lid cracks open, light leaks),
    /// 4 Bat nap (lid pops up), 5 Mo toang (fully open, glow peaks). Each stage has a travel time and a hold,
    /// and the whole thing can wait for the player between stages.
    /// The same stage data is mirrored by the three.js preview used for the reference screenshots.
    /// </summary>
    public class BoxOpenSequence : MonoBehaviour
    {
        [Serializable]
        public class Stage
        {
            public string name = "Stage";
            [Tooltip("Seconds to move from the previous pose to this stage's pose.")] public float travel = 1f;
            [Tooltip("Seconds to hold this pose before the next stage (the suspense).")] public float hold = 0.5f;
            [Tooltip("Lid opening angle in degrees.")] public float lidAngle;
            [Range(0f, 1f), Tooltip("Brightness of the light inside the box.")] public float glow;
            [Range(0f, 1f), Tooltip("0 = seal on, 1 = seal gone.")] public float sealRelease;
            [Range(0f, 1f), Tooltip("0 = cord/lock on, 1 = cord/lock gone.")] public float bindingRelease;
            [Tooltip("Whole-box shake amplitude in metres.")] public float shake;
            [Tooltip("If true the sequence waits for Advance() after this stage (for example when the player presses a key).")] public bool waitForPlayer;
        }

        [Header("Stages")]
        [SerializeField]
        private List<Stage> stages = new List<Stage>
        {
            new Stage { name = "Rung",     travel = 0.9f, hold = 0.3f, lidAngle = 0f,   glow = 0f,    sealRelease = 0f, bindingRelease = 0f, shake = 0.004f },
            new Stage { name = "Go niem",  travel = 0.9f, hold = 0.4f, lidAngle = 0f,   glow = 0.05f, sealRelease = 1f, bindingRelease = 1f, shake = 0.003f },
            new Stage { name = "He nap",   travel = 1.2f, hold = 0.8f, lidAngle = 8f,   glow = 0.4f,  sealRelease = 1f, bindingRelease = 1f, shake = 0.002f },
            new Stage { name = "Bat nap",  travel = 0.8f, hold = 0.5f, lidAngle = 55f,  glow = 0.8f,  sealRelease = 1f, bindingRelease = 1f, shake = 0.006f },
            new Stage { name = "Mo toang", travel = 0.7f, hold = 0.6f, lidAngle = 105f, glow = 1f,    sealRelease = 1f, bindingRelease = 1f, shake = 0f },
        };

        [Header("Model orientation")]
        [Tooltip("Local axis the lid rotates around (X for the supplied boxes).")]
        [SerializeField] private Vector3 lidHingeAxis = Vector3.right;
        [Tooltip("+1 or -1. Flip this if the lid opens the wrong way after importing the .glb.")]
        [SerializeField] private float lidOpenSign = 1f;
        [Tooltip("Direction the box fronts toward the player in local space. Flip if the seal and lock fall backwards.")]
        [SerializeField] private Vector3 frontDirection = new Vector3(0f, 0f, -1f);

        [Header("Light inside the box")]
        [SerializeField] private Light innerLight;
        [SerializeField] private float innerLightMaxIntensity = 2f;

        [Header("Events")]
        public UnityEvent<int> onStageReached;   // stage index, after its travel finishes (play sound, particles, UI here)
        public UnityEvent onOpened;              // after the last stage

        private struct LayerState
        {
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        private LayerState _lid, _seal, _binding, _glow;
        private Vector3 _rootStart;
        private bool _ready;
        private bool _playing;
        private bool _advance;

        public bool IsPlaying => _playing;

        // ---------------------------------------------------------------- Public API

        public void Play()
        {
            EnsureReady();
            if (_playing) return;
            StartCoroutine(Run());
        }

        /// <summary>Continue past a stage that has waitForPlayer set.</summary>
        public void Advance()
        {
            _advance = true;
        }

        /// <summary>Put the box back in the closed state.</summary>
        public void ResetToClosed()
        {
            EnsureReady();
            StopAllCoroutines();
            _playing = false;
            Apply(Pose(-1, -1, 0f), 0f);
        }

        // ---------------------------------------------------------------- Setup

        private void Awake()
        {
            EnsureReady();
        }

        private void EnsureReady()
        {
            if (_ready) return;
            _ready = true;
            _rootStart = transform.localPosition;
            _lid = Capture(transform.Find("Lid"));
            _seal = Capture(transform.Find("Seal"));
            _binding = Capture(transform.Find("Binding"));
            _glow = Capture(transform.Find("Glow"));
            Apply(Pose(-1, -1, 0f), 0f);
        }

        private static LayerState Capture(Transform t)
        {
            LayerState s = new LayerState { transform = t };
            if (t != null)
            {
                s.position = t.localPosition;
                s.rotation = t.localRotation;
                s.scale = t.localScale;
            }
            return s;
        }

        // ---------------------------------------------------------------- Sequence

        private IEnumerator Run()
        {
            _playing = true;
            int previous = -1;

            for (int i = 0; i < stages.Count; i++)
            {
                Stage stage = stages[i];

                // Travel: move from the previous stage's pose to this one, with the shake of this stage.
                float t = 0f;
                float travel = Mathf.Max(0.01f, stage.travel);
                while (t < travel)
                {
                    t += Time.deltaTime;
                    float u = Mathf.Clamp01(t / travel);
                    float eased = i == 3 ? EaseOutBack(u) : Mathf.SmoothStep(0f, 1f, u);   // the pop-open overshoots a little
                    Apply(Pose(previous, i, eased), stage.shake * (0.5f + 0.5f * u));
                    yield return null;
                }

                Apply(Pose(previous, i, 1f), stage.shake);
                onStageReached?.Invoke(i);

                // Hold: stay on the pose (the suspense), the shake fades out.
                float held = 0f;
                float hold = Mathf.Max(0f, stage.hold);
                while (held < hold)
                {
                    held += Time.deltaTime;
                    Apply(Pose(previous, i, 1f), stage.shake * (1f - Mathf.Clamp01(held / Mathf.Max(0.01f, hold))));
                    yield return null;
                }

                if (stage.waitForPlayer)
                {
                    _advance = false;
                    while (!_advance) yield return null;
                }

                previous = i;
            }

            _playing = false;
            onOpened?.Invoke();
        }

        // ---------------------------------------------------------------- Pose maths

        private struct PoseValues
        {
            public float lid, glow, seal, binding;
        }

        /// <summary>Interpolated values between the end of stage <paramref name="from"/> (-1 = closed) and stage <paramref name="to"/>.</summary>
        private PoseValues Pose(int from, int to, float u)
        {
            PoseValues a = ValuesOf(from);
            PoseValues b = ValuesOf(to);
            return new PoseValues
            {
                lid = Mathf.LerpUnclamped(a.lid, b.lid, u),
                glow = Mathf.Lerp(a.glow, b.glow, Mathf.Clamp01(u)),
                seal = Mathf.Lerp(a.seal, b.seal, Mathf.Clamp01(u)),
                binding = Mathf.Lerp(a.binding, b.binding, Mathf.Clamp01(u)),
            };
        }

        private PoseValues ValuesOf(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= stages.Count) return new PoseValues();
            Stage s = stages[stageIndex];
            return new PoseValues { lid = s.lidAngle, glow = s.glow, seal = s.sealRelease, binding = s.bindingRelease };
        }

        private void Apply(PoseValues p, float shakeAmplitude)
        {
            if (_lid.transform != null)
            {
                _lid.transform.localRotation = _lid.rotation * Quaternion.AngleAxis(lidOpenSign * p.lid, lidHingeAxis);
            }

            Vector3 front = frontDirection.normalized;

            if (_seal.transform != null)
            {
                float s = p.seal;
                _seal.transform.localPosition = _seal.position + Vector3.down * (0.10f * s * s) + front * (0.05f * s);
                _seal.transform.localScale = _seal.scale * Mathf.Max(0.0001f, 1f - s * s * s);
                _seal.transform.gameObject.SetActive(s < 0.98f);
            }

            if (_binding.transform != null)
            {
                float b = p.binding;
                _binding.transform.localPosition = _binding.position + Vector3.down * (0.14f * b * b) + front * (0.04f * b);
                _binding.transform.localRotation = _binding.rotation * Quaternion.AngleAxis(-lidOpenSign * 34f * b, lidHingeAxis);
                _binding.transform.localScale = _binding.scale * Mathf.Max(0.0001f, 1f - b * b * b * b);
                _binding.transform.gameObject.SetActive(b < 0.98f);
            }

            if (_glow.transform != null)
            {
                _glow.transform.localScale = _glow.scale * Mathf.Max(0.0001f, p.glow);
                _glow.transform.gameObject.SetActive(p.glow > 0.01f);
            }

            if (innerLight != null)
            {
                innerLight.intensity = innerLightMaxIntensity * p.glow;
                innerLight.enabled = p.glow > 0.01f;
            }

            // Whole-box shake: smooth noise so it trembles instead of jittering.
            if (shakeAmplitude > 0f)
            {
                float time = Time.time * 28f;
                Vector3 offset = new Vector3(Mathf.PerlinNoise(time, 0.3f) - 0.5f, 0f, Mathf.PerlinNoise(0.7f, time) - 0.5f) * 2f * shakeAmplitude;
                transform.localPosition = _rootStart + offset;
            }
            else
            {
                transform.localPosition = _rootStart;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.2f;
            const float c3 = c1 + 1f;
            float x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
