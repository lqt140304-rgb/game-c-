// Minimal API surface of UnityEngine/UnityEditor used by the chest FX scripts. Compile check only.
using System;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine
{
    public class Object { public string name; public HideFlags hideFlags;
        public static void Destroy(Object o) {} public static void Destroy(Object o, float t) {} public static void DestroyImmediate(Object o) {}
        public static T Instantiate<T>(T o) where T : Object => o; public static T Instantiate<T>(T o, Transform p) where T : Object => o;
        public static T Instantiate<T>(T o, Vector3 p, Quaternion r) where T : Object => o;
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static implicit operator bool(Object o) => o != null; }
    public enum HideFlags { None, DontSave }
    public partial class Component : Object { public Transform transform; public GameObject gameObject;
        public T GetComponent<T>() => default; public T[] GetComponentsInChildren<T>() => null; public T[] GetComponentsInChildren<T>(bool b) => null; }
    public partial class Component { public Component GetComponent(Type t) => null; }   // used by ChestFxLibrary
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour { public Coroutine StartCoroutine(IEnumerator e) => null; public void StopAllCoroutines() {} }
    public class Coroutine {}
    public class WaitForSeconds { public WaitForSeconds(float s) {} }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject => default; }
    public partial class GameObject : Object { public GameObject() {} public GameObject(string n) {} public Transform transform; public bool activeSelf; public bool activeInHierarchy;
        public void SetActive(bool b) {} public T AddComponent<T>() where T : Component => default; public T GetComponent<T>() => default;
        public T[] GetComponentsInChildren<T>() => null; public T[] GetComponentsInChildren<T>(bool b) => null; public string tag;
        public static GameObject CreatePrimitive(PrimitiveType t) => null; public static GameObject Find(string n) => null; }
    public partial class GameObject { public Component AddComponent(Type t) => null; }   // used by ChestFxLibrary
    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }
    public class Transform : Component, IEnumerable { public Vector3 position, localPosition, localScale, lossyScale, up, forward, right; public Quaternion rotation, localRotation;
        public Transform parent; public Transform root; public Matrix4x4 worldToLocalMatrix, localToWorldMatrix;
        public void SetParent(Transform p, bool w) {} public void SetParent(Transform p) {} public Transform Find(string n) => null; public bool IsChildOf(Transform t) => false;
        public Vector3 TransformPoint(Vector3 v) => v; public Vector3 InverseTransformPoint(Vector3 v) => v; public void SetPositionAndRotation(Vector3 p, Quaternion q) {}
        public IEnumerator GetEnumerator() => null; public int childCount; }
    public struct Vector2 { public float x, y; public Vector2(float a, float b) { x = a; y = b; } }
    public struct Vector3 { public float x, y, z; public Vector3(float a, float b, float c) { x = a; y = b; z = c; }
        public static Vector3 zero => default; public static Vector3 one => default; public static Vector3 up => default; public static Vector3 down => default; public static Vector3 forward => default; public static Vector3 right => default; public float sqrMagnitude => 0; public void Normalize() {} public Vector3 normalized => this;
        public static Vector3 operator +(Vector3 a, Vector3 b) => a; public static Vector3 operator -(Vector3 a, Vector3 b) => a; public static Vector3 operator -(Vector3 a) => a;
        public static Vector3 operator *(Vector3 a, float b) => a; public static Vector3 operator *(float b, Vector3 a) => a; public static Vector3 operator /(Vector3 a, float b) => a;
        public static Vector3 Scale(Vector3 a, Vector3 b) => a; public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) => a; public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a;
        public static Vector3 ProjectOnPlane(Vector3 a, Vector3 n) => a;
        public static implicit operator Vector3(Vector4 v) => default; }
    public struct Vector4 { }
    public struct Quaternion { public static Quaternion identity => default; public static Quaternion AngleAxis(float a, Vector3 v) => identity; public static Quaternion Euler(float x, float y, float z) => identity;
        public static Quaternion LookRotation(Vector3 f, Vector3 u) => identity; public static Quaternion LookRotation(Vector3 f) => identity;
        public static Quaternion operator *(Quaternion a, Quaternion b) => a; }
    public struct Matrix4x4 { public Vector4 GetColumn(int i) => default; public Quaternion rotation => default; public Vector3 lossyScale => default; public Vector3 MultiplyPoint3x4(Vector3 v) => v;
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b) => a; }
    public struct Color { public float r, g, b, a; public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; } public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => default; public static Color black => default; public static Color Lerp(Color a, Color b, float t) => a; public static Color operator *(Color a, float b) => a; }
    public struct Color32 { public Color32(byte r, byte g, byte b, byte a) {} }
    public struct Bounds { public Bounds(Vector3 c, Vector3 s) { center = c; size = s; extents = s; min = max = c; } public Vector3 center, size, extents, min, max; public void Encapsulate(Vector3 p) {} public void Encapsulate(Bounds b) {} }
    public struct Rect { public Rect(float x, float y, float w, float h) {} }
    public static class Mathf { public const float PI = 3.14f, Deg2Rad = 0.0174f; public static float Clamp01(float v) => v; public static float Clamp(float v, float a, float b) => v; public static int Clamp(int v, int a, int b) => v;
        public static float Lerp(float a, float b, float t) => a; public static float LerpUnclamped(float a, float b, float t) => a; public static float Max(float a, float b) => a; public static int Max(int a, int b) => a; public static float Min(float a, float b) => a;
        public static float Sin(float a) => a; public static float Cos(float a) => a; public static float Exp(float a) => a; public static float Abs(float a) => a; public static float Sqrt(float a) => a;
        public static float SmoothStep(float a, float b, float t) => a; public static float PerlinNoise(float x, float y) => x; public static int RoundToInt(float f) => 0; public static bool Approximately(float a, float b) => true; }
    public static class Time { public static float deltaTime, time; }
    public static class Debug { public static void Log(object o) {} public static void LogWarning(object o) {} public static void LogWarning(object o, Object c) {} public static void LogError(object o) {} }
    public static class Random { public static int Range(int a, int b) => a; }
    public class Renderer : Component { public Bounds bounds; public Material sharedMaterial; public Material[] sharedMaterials; public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode; public bool receiveShadows;
        public UnityEngine.Rendering.LightProbeUsage lightProbeUsage; public UnityEngine.Rendering.ReflectionProbeUsage reflectionProbeUsage; public bool enabled;
        public void GetPropertyBlock(MaterialPropertyBlock b) {} public void SetPropertyBlock(MaterialPropertyBlock b) {} }
    public class MeshRenderer : Renderer {} public class SkinnedMeshRenderer : Renderer { public Mesh sharedMesh; }
    public class MeshFilter : Component { public Mesh sharedMesh; }
    public class Collider : Component {}
    public class Mesh : Object { public Bounds bounds; public int subMeshCount; public void SetVertices(List<Vector3> v) {} public void SetColors(List<Color> c) {} public void SetUVs(int ch, List<Vector2> u) {} public void SetTriangles(List<int> t, int s) {} public void RecalculateBounds() {} }
    public class Shader : Object { public static Shader Find(string n) => null; }
    public class Texture : Object {}
    public enum TextureFormat { RGBA32 } public enum TextureWrapMode { Clamp, Repeat } public enum FilterMode { Bilinear, Point }
    public class Texture2D : Texture { public Texture2D(int w, int h, TextureFormat f, bool m) {} public TextureWrapMode wrapMode; public FilterMode filterMode; public static Texture2D whiteTexture;
        public void SetPixels32(Color32[] p) {} public void Apply(bool a, bool b) {} }
    public class Material : Object { public Material(Shader s) {} public bool HasProperty(string n) => true; public void SetFloat(string n, float v) {} public void SetColor(string n, Color c) {} public Color GetColor(string n) => default;
        public void SetTexture(string n, Texture t) {} public void EnableKeyword(string k) {} public void SetOverrideTag(string a, string b) {} public int renderQueue; }
    public class MaterialPropertyBlock { public void SetColor(string n, Color c) {} }
    public enum LightType { Point } public enum LightShadows { None }
    public class Light : Behaviour { public LightType type; public float range, intensity; public LightShadows shadows; public Color color; }
    public class Camera : Behaviour { public static Camera main; }
    public class AudioClip : Object { public float length; }
    public class AudioSource : Behaviour { public bool playOnAwake; public float spatialBlend, pitch; public UnityEngine.Audio.AudioMixerGroup outputAudioMixerGroup; public void PlayOneShot(AudioClip c, float v) {} }
    public struct Keyframe { public Keyframe(float t, float v) {} public Keyframe(float t, float v, float i, float o) {} }
    public class AnimationCurve { public AnimationCurve(params Keyframe[] k) {} public float Evaluate(float t) => t; public int length; }
    public struct GradientColorKey { public GradientColorKey(Color c, float t) {} } public struct GradientAlphaKey { public GradientAlphaKey(float a, float t) {} }
    public class Gradient { public void SetKeys(GradientColorKey[] c, GradientAlphaKey[] a) {} }
    public enum ParticleSystemSimulationSpace { Local, World } public enum ParticleSystemShapeType { Sphere, Cone, Circle } public enum ParticleSystemStopBehavior { StopEmittingAndClear }
    public enum ParticleSystemRenderMode { Billboard, Stretch }
    public class ParticleSystemRenderer : Renderer { public ParticleSystemRenderMode renderMode; public float lengthScale, velocityScale; }
    public class ParticleSystem : Component {
        public struct MinMaxCurve { public MinMaxCurve(float c) {} public MinMaxCurve(float a, float b) {} public MinMaxCurve(float m, AnimationCurve c) {} public static implicit operator MinMaxCurve(float f) => default; }
        public struct MinMaxGradient { public MinMaxGradient(Color a, Color b) {} public MinMaxGradient(Gradient g) {} }
        public struct MainModule { public bool playOnAwake, loop; public float duration; public ParticleSystemSimulationSpace simulationSpace; public MinMaxCurve startLifetime, startSpeed, startSize, startRotation, gravityModifier; public int maxParticles; public MinMaxGradient startColor; }
        public struct EmissionModule { public MinMaxCurve rateOverTime; }
        public struct ShapeModule { public bool enabled; public ParticleSystemShapeType shapeType; public float radius, angle; public Vector3 rotation; }
        public struct VelocityOverLifetimeModule { public bool enabled; public ParticleSystemSimulationSpace space; public MinMaxCurve x, y, z, orbitalY, radial; }
        public struct ColorOverLifetimeModule { public bool enabled; public MinMaxGradient color; }
        public struct SizeOverLifetimeModule { public bool enabled; public MinMaxCurve size; }
        public struct RotationOverLifetimeModule { public bool enabled; public MinMaxCurve z; }
        public MainModule main; public EmissionModule emission; public ShapeModule shape; public VelocityOverLifetimeModule velocityOverLifetime; public ColorOverLifetimeModule colorOverLifetime;
        public SizeOverLifetimeModule sizeOverLifetime; public RotationOverLifetimeModule rotationOverLifetime; public bool isPlaying;
        public void Play() {} public void Stop(bool c, ParticleSystemStopBehavior b) {} public void Emit(int n) {} }
    public class GUIStyle { public GUIStyle(GUIStyle o) {} public TextAnchor alignment; public int fontSize; public bool wordWrap; public GUIStyleState normal; }
    public class GUIStyleState { public Color textColor; }
    public class GUISkin { public GUIStyle box; }
    public enum TextAnchor { UpperLeft }
    public static class GUI { public static GUISkin skin; public static void Box(Rect r, string t, GUIStyle s) {} }
    public enum KeyCode { T, Y, Space, R, C }
    public static class Input { public static bool GetKeyDown(KeyCode k) => false; }
    public struct RaycastHit { public Vector3 point; }
    public static class Physics { public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit h, float m) { h = default; return false; } }
    public static class Resources {}
    [AttributeUsage(AttributeTargets.All)] public class SerializeField : Attribute {}
    [AttributeUsage(AttributeTargets.All)] public class HeaderAttribute : Attribute { public HeaderAttribute(string s) {} }
    [AttributeUsage(AttributeTargets.All)] public class TooltipAttribute : Attribute { public TooltipAttribute(string s) {} }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)] public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) {} }
    [AttributeUsage(AttributeTargets.All)] public class CreateAssetMenuAttribute : Attribute { public string menuName, fileName; }
    [AttributeUsage(AttributeTargets.All)] public class DisallowMultipleComponent : Attribute {}
}
namespace UnityEngine.Audio { public class AudioMixerGroup : UnityEngine.Object {} }
namespace UnityEngine.Rendering { public enum CullMode { Off, Front, Back } public enum BlendMode { One = 1, SrcAlpha = 5 } public enum RenderQueue { Transparent = 3000 }
    public enum ShadowCastingMode { Off } public enum LightProbeUsage { Off } public enum ReflectionProbeUsage { Off } }
namespace UnityEngine.Events { public delegate void UnityAction(); public delegate void UnityAction<T>(T a);
    public class UnityEvent { public void Invoke() {} public void AddListener(UnityAction a) {} public void RemoveListener(UnityAction a) {} }
    public class UnityEvent<T> { public void Invoke(T t) {} public void AddListener(UnityAction<T> a) {} public void RemoveListener(UnityAction<T> a) {} } }
namespace UnityEngine.InputSystem { public enum Key { T, Y, Space, R, C, F, Q, Escape, G, K, L } public class KeyControl { public bool wasPressedThisFrame, isPressed; }
    public class Keyboard { public static Keyboard current; public KeyControl this[Key k] => null; } }
namespace UnityEditor
{
    using UnityEngine;
    [AttributeUsage(AttributeTargets.Method)] public class MenuItem : Attribute { public MenuItem(string s) {} }
    public static class AssetDatabase { public static T LoadAssetAtPath<T>(string p) where T : UnityEngine.Object => null; public static bool IsValidFolder(string p) => true;
        public static void CreateAsset(UnityEngine.Object o, string p) {} public static void SaveAssets() {} public static string CreateFolder(string a, string b) => ""; }
    public static class PrefabUtility { public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object o, Transform p) => o; }
    public static class Undo { public static void DestroyObjectImmediate(UnityEngine.Object o) {} public static void RegisterCreatedObjectUndo(UnityEngine.Object o, string n) {} }
    public static class Selection { public static GameObject activeGameObject; public static UnityEngine.Object activeObject; }
    public static class EditorUtility { public static void SetDirty(UnityEngine.Object o) {} }
    public class SerializedObject { public SerializedObject(UnityEngine.Object o) {} public SerializedProperty FindProperty(string n) => null; public void ApplyModifiedPropertiesWithoutUndo() {} }
    public class SerializedProperty { public UnityEngine.Object objectReferenceValue; public string stringValue; public void ClearArray() {} public void InsertArrayElementAtIndex(int i) {} public SerializedProperty GetArrayElementAtIndex(int i) => null; }
}
namespace UnityEditor.SceneManagement { public static class EditorSceneManager { public static object GetActiveScene() => null; public static bool MarkSceneDirty(object s) => true; } }
