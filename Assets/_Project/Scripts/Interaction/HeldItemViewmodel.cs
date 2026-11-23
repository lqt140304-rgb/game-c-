using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Interaction
{
    /// <summary>
    /// First-person "viewmodel" for a held item, in the style of weapon inspect in Valorant.
    /// Put this on an empty child of the player camera (for example "HeldItemAnchor").
    /// Controls: F = inspect, Q = put the item away.
    /// The item is a plain static mesh; every movement here is driven by code.
    /// </summary>
    public class HeldItemViewmodel : MonoBehaviour
    {
        [Header("Rest pose (camera space)")]
        [SerializeField] private Vector3 restPosition = new Vector3(0.24f, -0.18f, 0.5f);
        [SerializeField] private float equipDuration = 0.4f;
        [SerializeField] private float putAwayDuration = 0.25f;

        [Header("Idle sway")]
        [SerializeField] private float breathAmount = 0.004f;
        [SerializeField] private float walkBobAmount = 0.012f;

        private enum State { Empty, Equipping, Idle, Inspecting, PuttingAway }

        private struct Pose
        {
            public Vector3 position;
            public Quaternion rotation;
            public float travel;
            public float hold;
            public string label;
        }

        private State _state = State.Empty;
        private Transform _pivot;
        private HeldItemProfile _profile;
        private Quaternion _baseRotation;
        private Action _onPutAway;
        private ScrollUnroll _scroll;   // set when the held item is a scroll: F opens or closes it instead of the flourish

        private float _stateTime;
        private readonly List<Pose> _inspectPoses = new List<Pose>();
        private Pose _inspectStart;
        private string _currentLabel = string.Empty;

        private Vector3 _lastRootPosition;
        private float _walkPhase;
        private float _walkAmount;

        public bool HasItem => _state != State.Empty;
        public bool IsInspecting => _state == State.Inspecting;

        // -------------------------------------------------------------- Public API

        /// <summary>
        /// Show a copy of <paramref name="source"/> in the player's hand. The source is not modified;
        /// the caller decides what to do with the world object (usually hide it).
        /// </summary>
        public void Equip(GameObject source, string itemId, Action onPutAway = null)
        {
            if (HasItem || source == null) return;

            _profile = HeldItemProfile.For(itemId);
            _baseRotation = Quaternion.Euler(_profile.baseEuler);
            _onPutAway = onPutAway;

            GameObject pivotGo = new GameObject("HeldItemPivot");
            _pivot = pivotGo.transform;
            _pivot.SetParent(transform, false);

            GameObject model = Instantiate(source, _pivot);
            model.SetActive(true);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            StripForHolding(model);

            _scroll = model.GetComponentInChildren<ScrollUnroll>();
            if (_scroll != null) _scroll.SetOpenImmediate(1f);   // size the item by its open shape
            FitToSize(model.transform, _profile.targetSize, _pivot);
            if (_scroll != null) _scroll.SetOpenImmediate(0f);

            _pivot.localPosition = restPosition + new Vector3(0f, -0.35f, -0.1f);
            _pivot.localRotation = Quaternion.Euler(45f, -25f, 0f);

            _currentLabel = string.Empty;
            _lastRootPosition = transform.root.position;
            SetState(State.Equipping);
        }

        public void PutAway()
        {
            if (_state == State.Empty || _state == State.PuttingAway) return;
            _currentLabel = string.Empty;
            SetState(State.PuttingAway);
        }

        public void Inspect()
        {
            if (_state != State.Idle) return;
            BuildInspectPoses();
            _inspectStart = new Pose { position = _pivot.localPosition, rotation = _pivot.localRotation };
            SetState(State.Inspecting);
        }

        /// <summary>
        /// Scale <paramref name="model"/> so its longest side is <paramref name="targetSize"/> metres
        /// and move it so its bounds centre sits on <paramref name="pivot"/> (so it spins around its middle).
        /// </summary>
        public static void FitToSize(Transform model, float targetSize, Transform pivot)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = CombinedBounds(renderers);
            float longest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (longest > 0.0001f)
            {
                model.localScale = Vector3.one * (targetSize / longest);
            }

            bounds = CombinedBounds(renderers);
            model.position += pivot.position - bounds.center;
        }

        // ---------------------------------------------------------------- Unity

        private void Update()
        {
            if (_state == State.Empty || _pivot == null) return;

            _stateTime += Time.deltaTime;
            UpdateWalkBob();

            switch (_state)
            {
                case State.Equipping: UpdateEquip(); break;
                case State.Idle: UpdateIdle(); break;
                case State.Inspecting: UpdateInspect(); break;
                case State.PuttingAway: UpdatePutAway(); break;
            }
        }

        private void OnGUI()
        {
            if (_state == State.Empty || _state == State.PuttingAway) return;

            GUIStyle hint = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 14,
                normal = { textColor = new Color(1f, 1f, 1f, 0.8f) }
            };
            string hintText = _scroll != null ? "[F] Mở / đóng cuộn   [Q] Cất" : "[F] Soi   [Q] Cất";
            GUI.Label(new Rect(0f, Screen.height - 36f, Screen.width, 28f), hintText, hint);

            if (!string.IsNullOrEmpty(_currentLabel))
            {
                GUIStyle label = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.92f, 0.6f, 1f) }
                };
                GUI.Label(new Rect(0f, Screen.height * 0.72f, Screen.width, 40f), _currentLabel, label);
            }
        }

        // ---------------------------------------------------------------- States

        private void UpdateEquip()
        {
            float t = Mathf.Clamp01(_stateTime / equipDuration);
            float e = EaseOut(t);
            Vector3 from = restPosition + new Vector3(0f, -0.35f, -0.1f);
            _pivot.localPosition = Vector3.Lerp(from, restPosition, e);
            _pivot.localRotation = Quaternion.Slerp(Quaternion.Euler(45f, -25f, 0f), _baseRotation, e);
            if (t >= 1f) SetState(State.Idle);
        }

        private void UpdateIdle()
        {
            if (WasInspectPressed())
            {
                if (_scroll != null) _scroll.Toggle();
                else Inspect();
                return;
            }
            if (WasPutAwayPressed()) { PutAway(); return; }

            ApplyRestPose(1f);
        }

        private void UpdateInspect()
        {
            // Walk the list of poses: for each one, travel to it and hold, then return to rest at the end.
            float time = _stateTime;
            Pose from = _inspectStart;
            _currentLabel = string.Empty;

            for (int i = 0; i < _inspectPoses.Count; i++)
            {
                Pose to = _inspectPoses[i];

                if (time < to.travel)
                {
                    float e = EaseInOut(Mathf.Clamp01(time / Mathf.Max(0.0001f, to.travel)));
                    _pivot.localPosition = Vector3.Lerp(from.position, to.position, e);
                    _pivot.localRotation = Quaternion.Slerp(from.rotation, to.rotation, e);
                    return;
                }

                time -= to.travel;

                if (time < to.hold)
                {
                    _pivot.localPosition = to.position;
                    _pivot.localRotation = to.rotation;
                    _currentLabel = to.label;
                    return;
                }

                time -= to.hold;
                from = to;
            }

            // Last segment: settle back to the rest pose.
            const float returnTime = 0.45f;
            Vector3 restPos = restPosition;
            if (time < returnTime)
            {
                float e = EaseInOut(time / returnTime);
                _pivot.localPosition = Vector3.Lerp(from.position, restPos, e);
                _pivot.localRotation = Quaternion.Slerp(from.rotation, _baseRotation, e);
                return;
            }

            SetState(State.Idle);
        }

        private void UpdatePutAway()
        {
            float t = Mathf.Clamp01(_stateTime / putAwayDuration);
            float e = t * t;
            Vector3 down = restPosition + new Vector3(0f, -0.4f, -0.1f);
            _pivot.localPosition = Vector3.Lerp(_pivot.localPosition, down, e);
            _pivot.localRotation = Quaternion.Slerp(_pivot.localRotation, Quaternion.Euler(50f, -25f, 0f), e);

            if (t >= 1f)
            {
                Destroy(_pivot.gameObject);
                _pivot = null;
                _scroll = null;
                Action callback = _onPutAway;
                _onPutAway = null;
                SetState(State.Empty);
                callback?.Invoke();
            }
        }

        // -------------------------------------------------------------- Helpers

        private void ApplyRestPose(float weight)
        {
            float breath = Mathf.Sin(Time.time * 1.6f) * breathAmount;
            float bobY = Mathf.Sin(_walkPhase * 2f) * walkBobAmount * _walkAmount;
            float bobX = Mathf.Cos(_walkPhase) * walkBobAmount * 0.5f * _walkAmount;
            Vector3 target = restPosition + new Vector3(bobX, breath + bobY, 0f);
            _pivot.localPosition = Vector3.Lerp(_pivot.localPosition, target, weight * Mathf.Clamp01(Time.deltaTime * 12f));
            _pivot.localRotation = Quaternion.Slerp(_pivot.localRotation, _baseRotation, Mathf.Clamp01(Time.deltaTime * 8f));
        }

        private void UpdateWalkBob()
        {
            Vector3 root = transform.root.position;
            float speed = (root - _lastRootPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastRootPosition = root;

            float moving = Mathf.Clamp01(speed / 4f);
            _walkAmount = Mathf.Lerp(_walkAmount, moving, Time.deltaTime * 8f);
            _walkPhase += Time.deltaTime * (4f + speed * 1.2f) * moving;
        }

        private void BuildInspectPoses()
        {
            _inspectPoses.Clear();
            foreach (InspectStop stop in _profile.stops)
            {
                _inspectPoses.Add(new Pose
                {
                    position = restPosition + stop.offset,
                    rotation = Quaternion.Euler(stop.euler) * _baseRotation,
                    travel = Mathf.Max(0.05f, stop.travel),
                    hold = Mathf.Max(0f, stop.hold),
                    label = stop.label
                });
            }
        }

        private void SetState(State next)
        {
            _state = next;
            _stateTime = 0f;
        }

        private static void StripForHolding(GameObject model)
        {
            // The held copy must not collide with anything or be picked up again.
            foreach (Collider col in model.GetComponentsInChildren<Collider>(true)) Destroy(col);
            foreach (PickupInteractable pickup in model.GetComponentsInChildren<PickupInteractable>(true)) Destroy(pickup);
            foreach (Rigidbody body in model.GetComponentsInChildren<Rigidbody>(true)) Destroy(body);
            foreach (Renderer rend in model.GetComponentsInChildren<Renderer>(true))
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInOut(float t) => t * t * (3f - 2f * t);

        private static bool WasInspectPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F);
#endif
        }

        private static bool WasPutAwayPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Q);
#endif
        }
    }
}
