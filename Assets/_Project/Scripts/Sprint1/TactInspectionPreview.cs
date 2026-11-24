using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChoDoLa.Sprint1
{
    // Preview-only scene objects are isolated from the player camera and physics.
    public sealed class TactInspectionPreview : MonoBehaviour
    {
        private const int PreviewLayer = 31;
        private static readonly Vector3 StudioPosition = new Vector3(0, -5000, 0);
        private static readonly int InspectTrigger = Animator.StringToHash("InspectTact");

        private RawImage image;
        private GameObject studio;
        private RenderTexture target;
        private GameObject character;
        private GameObject prop;
        private Animator animator;
        private Transform hand;
        private float inspectUntil;

        public void Configure(RawImage previewImage)
        {
            image = previewImage;
            image.color = new Color(.19f, .20f, .17f);
        }

        public void Inspect(string id)
        {
            ClearProp();
            var prefab = Resources.Load<GameObject>($"TactEquipment/Models/{id}") ??
                Resources.Load<GameObject>($"Blender/Models/Artifact_{id}");
            if (prefab == null) return;
            EnsureStudio();

            prop = Instantiate(prefab, hand != null ? hand : studio.transform, false);
            prop.transform.localPosition = hand != null ? Vector3.zero : new Vector3(0, 1.05f, -.22f);
            SetPreviewLayer(prop);
            inspectUntil = Time.unscaledTime + 1.1f;
            if (animator != null && animator.runtimeAnimatorController != null &&
                animator.parameters.Any(parameter => parameter.nameHash == InspectTrigger &&
                    parameter.type == AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(InspectTrigger);
        }

        private void OnEnable()
        {
            if (image != null) EnsureStudio();
        }

        private void Update()
        {
            if (prop == null) return;
            if (Time.unscaledTime >= inspectUntil) ClearProp();
            else prop.transform.Rotate(Vector3.up, 35f * Time.unscaledDeltaTime, Space.Self);
        }

        private void EnsureStudio()
        {
            if (studio != null) return;
            studio = new GameObject("Tact Preview Studio");
            studio.transform.position = StudioPosition;

            var characterPrefab = Resources.Load<GameObject>("TactEquipment/Character");
            if (characterPrefab != null)
            {
                character = Instantiate(characterPrefab, studio.transform, false);
                character.transform.localRotation = Quaternion.Euler(0, 180, 0);
                SetPreviewLayer(character);
                animator = character.GetComponentInChildren<Animator>();
                if (animator != null) animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                hand = character.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == "TactPreviewHand");
                if (hand == null && animator != null && animator.isHuman)
                    hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            }

            target = new RenderTexture(768, 576, 24) { name = "Tact Preview" };
            var cameraObject = new GameObject("Preview Camera");
            cameraObject.transform.SetParent(studio.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 1.12f, -3.45f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.19f, .20f, .17f);
            camera.cullingMask = 1 << PreviewLayer;
            camera.orthographic = true;
            camera.orthographicSize = 1.20f;
            camera.farClipPlane = 10;
            camera.targetTexture = target;

            var lightObject = new GameObject("Preview Light");
            lightObject.transform.SetParent(studio.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(27, -20, 0);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.cullingMask = 1 << PreviewLayer;
            image.texture = target;
            image.color = Color.white;
        }

        private static void SetPreviewLayer(GameObject root)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = PreviewLayer;
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }

        private void ClearProp()
        {
            if (prop == null) return;
            prop.SetActive(false);
            Destroy(prop);
            prop = null;
        }

        private void OnDisable()
        {
            ClearProp();
            if (studio != null)
            {
                studio.SetActive(false);
                Destroy(studio);
            }
            studio = null;
            character = null;
            animator = null;
            hand = null;
            if (image != null) image.texture = null;
            if (target != null)
            {
                target.Release();
                Destroy(target);
            }
            target = null;
        }
    }
}
