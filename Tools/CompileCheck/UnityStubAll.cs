// Extra UnityEngine/UnityEditor API for Tools/CompileCheck/check_all.sh (one compile of every chest script).
// check_all.sh copies UnityStub.cs with every class/struct made partial; this file adds the members and types the
// whole set uses on top of it. Compile check only: bodies are dummies, and nothing here proves the real API matches.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public enum KeyCode { None, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, Space, Escape, Return, KeypadEnter,
        LeftBracket, RightBracket, UpArrow, DownArrow, LeftArrow, RightArrow, LeftShift, Tab, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Mouse0, Mouse1 }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public enum LightType { Spot, Directional, Point, Area }
    public enum LightShadows { None, Hard, Soft }
    public enum ParticleSystemStopBehavior { StopEmittingAndClear, StopEmitting }
    public enum ParticleSystemShapeType { Sphere, Hemisphere, Cone, Box, Mesh, Circle, Edge, Donut, Rectangle, ConeVolume }
    public enum ParticleSystemRenderMode { Billboard, Stretch, HorizontalBillboard, VerticalBillboard, Mesh, None }
    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }
    public enum CursorLockMode { None, Locked, Confined }
    public enum FontStyle { Normal, Bold }
    public enum Space { World, Self }
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
    public enum CollisionFlags { None }
    public partial class Collision { public Vector3 relativeVelocity, impulse; public Collider collider; public GameObject gameObject; public Rigidbody rigidbody; public Transform transform; } // MERGED_COLLISION
    public enum CameraClearFlags { Skybox, SolidColor }
    public enum ForceMode { Force, Impulse, Acceleration, VelocityChange }
    public enum RigidbodyConstraints { None, FreezeRotation }
    public enum LightmapBakeType { Realtime }
    public enum RuntimePlatform { WindowsEditor }
    public enum FindObjectsSortMode { None }
    public enum FindObjectsInactive { Exclude, Include }

    public static class Cursor { public static CursorLockMode lockState; public static bool visible; }
    public static class Screen { public static int width, height; }
    public static class Application { public static bool isPlaying; public static string dataPath; public static void Quit() {} }
    public struct LayerMask { public int value; public static implicit operator int(LayerMask m) => m.value; public static implicit operator LayerMask(int v) => default;
        public static int GetMask(params string[] n) => 0; public static int NameToLayer(string n) => 0; }

    public partial class Object
    {
        public static T Instantiate<T>(T o, Transform p, bool w) where T : Object => o;
        public static T Instantiate<T>(T o, Vector3 p, Quaternion r, Transform parent) where T : Object => o;
        public static T FindAnyObjectByType<T>() where T : Object => null;
        public static T[] FindObjectsByType<T>(FindObjectsSortMode m) where T : Object => null;
        public static T[] FindObjectsByType<T>(FindObjectsInactive i, FindObjectsSortMode m) where T : Object => null;
        public static void DontDestroyOnLoad(Object o) {}
        public int GetInstanceID() => 0;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object o) => ReferenceEquals(this, o);
        public override int GetHashCode() => 0;
    }
    public partial class Component
    {
        public T GetComponentInChildren<T>() => default; public T GetComponentInChildren<T>(bool b) => default;
        public T GetComponentInParent<T>() => default; public T GetComponentInParent<T>(bool b) => default;
        public T[] GetComponents<T>() => null; public T[] GetComponentsInParent<T>() => null;
        public bool TryGetComponent<T>(out T c) { c = default; return false; }
        public string tag; public bool CompareTag(string t) => false;
    }
    public partial class GameObject
    {
        public Component GetComponent(Type t) => null;
        public T GetComponentInChildren<T>() => default; public T GetComponentInChildren<T>(bool b) => default;
        public T GetComponentInParent<T>() => default; public T[] GetComponents<T>() => null;
        public bool TryGetComponent<T>(out T c) { c = default; return false; }
        public int layer; public UnityEngine.SceneManagement.Scene scene; public bool CompareTag(string t) => false;
        public static GameObject FindWithTag(string t) => null; public static GameObject FindGameObjectWithTag(string t) => null;
    }
    public partial class Transform
    {
        public Vector3 eulerAngles, localEulerAngles;
        public Transform GetChild(int i) => null; public int GetSiblingIndex() => 0; public void SetSiblingIndex(int i) {} public void SetAsFirstSibling() {}
        public Vector3 TransformDirection(Vector3 v) => v; public Vector3 InverseTransformDirection(Vector3 v) => v; public Vector3 TransformVector(Vector3 v) => v; public Vector3 InverseTransformVector(Vector3 v) => v;
        public void Rotate(Vector3 e) {} public void Rotate(Vector3 a, float d) {} public void Rotate(float x, float y, float z) {} public void Rotate(Vector3 a, float d, Space s) {}
        public void Translate(Vector3 v) {} public void LookAt(Vector3 p) {} public void LookAt(Transform t) {} public void LookAt(Vector3 p, Vector3 u) {}
        public void SetLocalPositionAndRotation(Vector3 p, Quaternion q) {} public void DetachChildren() {} public void GetPositionAndRotation(out Vector3 p, out Quaternion q) { p = default; q = default; }
    }
    public partial class MonoBehaviour
    {
        public Coroutine StartCoroutine(string n) => null; public void StopCoroutine(Coroutine c) {} public void StopCoroutine(IEnumerator e) {}
        public void Invoke(string n, float t) {} public void CancelInvoke() {} public bool isActiveAndEnabled;
        public static void print(object o) {}
    }
    public partial class ScriptableObject { public static ScriptableObject CreateInstance(Type t) => null; }
    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float s) {} }
    public class WaitForEndOfFrame {}
    public class WaitUntil { public WaitUntil(Func<bool> f) {} }
    public class YieldInstruction {}

    public partial struct Vector2
    {
        public static Vector2 zero => default; public static Vector2 one => default; public static Vector2 up => default; public static Vector2 right => default;
        public float magnitude => 0; public float sqrMagnitude => 0; public Vector2 normalized => this;
        public static Vector2 operator +(Vector2 a, Vector2 b) => a; public static Vector2 operator -(Vector2 a, Vector2 b) => a; public static Vector2 operator -(Vector2 a) => a;
        public static Vector2 operator *(Vector2 a, float b) => a; public static Vector2 operator *(float b, Vector2 a) => a; public static Vector2 operator /(Vector2 a, float b) => a;
        public static Vector2 ClampMagnitude(Vector2 v, float m) => v; public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a;
        public static implicit operator Vector3(Vector2 v) => default; public static implicit operator Vector2(Vector3 v) => default;
        public static bool operator ==(Vector2 a, Vector2 b) => true; public static bool operator !=(Vector2 a, Vector2 b) => false;
        public override bool Equals(object o) => true; public override int GetHashCode() => 0;
    }
    public partial struct Vector3
    {
        public static Vector3 back => default; public static Vector3 left => default;
        public float magnitude => 0; public float this[int i] { get => 0; set {} }
        public static float Dot(Vector3 a, Vector3 b) => 0; public static Vector3 Cross(Vector3 a, Vector3 b) => a; public static float Distance(Vector3 a, Vector3 b) => 0;
        public static float Angle(Vector3 a, Vector3 b) => 0; public static float SignedAngle(Vector3 a, Vector3 b, Vector3 axis) => 0;
        public static Vector3 Normalize(Vector3 v) => v; public static Vector3 ClampMagnitude(Vector3 v, float m) => v; public static Vector3 MoveTowards(Vector3 a, Vector3 b, float d) => a;
        public static Vector3 Slerp(Vector3 a, Vector3 b, float t) => a; public static Vector3 SmoothDamp(Vector3 a, Vector3 b, ref Vector3 v, float t) => a;
        public static Vector3 Max(Vector3 a, Vector3 b) => a; public static Vector3 Min(Vector3 a, Vector3 b) => a; public static Vector3 Project(Vector3 a, Vector3 n) => a;
        public static Vector3 operator *(Vector3 a, Vector3 b) => a;
        public static bool operator ==(Vector3 a, Vector3 b) => true; public static bool operator !=(Vector3 a, Vector3 b) => false;
        public override bool Equals(object o) => true; public override int GetHashCode() => 0;
    }
    public partial struct Vector4 { public float x, y, z, w; public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; } }
    public partial struct Quaternion
    {
        public float x, y, z, w; public Vector3 eulerAngles; public Quaternion normalized => this;
        public static Quaternion Euler(Vector3 e) => identity; public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => a; public static Quaternion Lerp(Quaternion a, Quaternion b, float t) => a;
        public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float t) => a; public static Quaternion Inverse(Quaternion q) => q;
        public static Quaternion RotateTowards(Quaternion a, Quaternion b, float d) => a; public static Quaternion FromToRotation(Vector3 a, Vector3 b) => identity;
        public static float Angle(Quaternion a, Quaternion b) => 0; public void ToAngleAxis(out float a, out Vector3 v) { a = 0; v = default; }
        public static Vector3 operator *(Quaternion q, Vector3 v) => v;
    }
    public partial struct Color
    {
        public static Color red => default; public static Color green => default; public static Color blue => default; public static Color yellow => default; public static Color cyan => default;
        public static Color magenta => default; public static Color gray => default; public static Color grey => default; public static Color clear => default;
        public float maxColorComponent => 0;
        public static Color operator *(Color a, Color b) => a; public static Color operator +(Color a, Color b) => a; public static Color operator -(Color a, Color b) => a; public static Color operator *(float b, Color a) => a; public static Color operator /(Color a, float b) => a;
        public static Color LerpUnclamped(Color a, Color b, float t) => a; public static Color HSVToRGB(float h, float s, float v) => default; public static void RGBToHSV(Color c, out float h, out float s, out float v) { h = s = v = 0; }
        public Color linear => this; public Color gamma => this;
        public static implicit operator Color(Color32 c) => default; public static implicit operator Vector4(Color c) => default;
        public static bool operator ==(Color a, Color b) => true; public static bool operator !=(Color a, Color b) => false;
        public override bool Equals(object o) => true; public override int GetHashCode() => 0;
    }
    public partial struct Color32 { public byte r { get => 0; set {} } public byte g { get => 0; set {} } public byte b { get => 0; set {} } public byte a { get => 0; set {} } public static implicit operator Color32(Color c) => default; }
    public partial struct Bounds { public bool Contains(Vector3 p) => false; public Vector3 ClosestPoint(Vector3 p) => p; public void Expand(float a) {} }
    public partial struct Rect { public float x { get => 0; set {} } public float y { get => 0; set {} } public float width { get => 0; set {} } public float height { get => 0; set {} } public float xMin => 0; public float xMax => 0; public float yMin => 0; public float yMax => 0; public Vector2 center => default; public Vector2 position { get => default; set {} } public Vector2 size => default; public bool Contains(Vector2 p) => false; }
    public struct RectOffset { public RectOffset(int l, int r, int t, int b) {} }
    public static partial class Mathf
    {
        public const float Rad2Deg = 57.29f, Epsilon = 1e-6f, Infinity = float.PositiveInfinity;
        public static float Atan2(float y, float x) => 0; public static float Atan(float a) => 0; public static float Asin(float a) => 0; public static float Acos(float a) => 0; public static float Tan(float a) => 0;
        public static int Min(int a, int b) => a; public static float Min(params float[] v) => 0; public static float Max(params float[] v) => 0; public static int Abs(int a) => a;
        public static float Pow(float a, float b) => a; public static float Log(float a) => a; public static float Sign(float a) => a; public static float Repeat(float t, float l) => t; public static float PingPong(float t, float l) => t;
        public static int FloorToInt(float f) => 0; public static int CeilToInt(float f) => 0; public static float Floor(float f) => f; public static float Ceil(float f) => f; public static float Round(float f) => f;
        public static float MoveTowards(float a, float b, float d) => a; public static float MoveTowardsAngle(float a, float b, float d) => a; public static float InverseLerp(float a, float b, float v) => 0;
        public static float SmoothDamp(float a, float b, ref float v, float t) => a; public static float DeltaAngle(float a, float b) => 0; public static float LerpAngle(float a, float b, float t) => a;
        public static bool IsPowerOfTwo(int v) => true; public static int NextPowerOfTwo(int v) => v; public static int RoundToInt(double d) => 0;
    }
    public static partial class Time { public static float unscaledDeltaTime, unscaledTime, timeScale, fixedDeltaTime, realtimeSinceStartup; public static int frameCount; }
    public static partial class Debug
    {
        public static void Log(object o, Object c) {} public static void LogError(object o, Object c) {} public static void LogException(Exception e) {} public static void LogFormat(string f, params object[] a) {}
        public static void LogWarningFormat(string f, params object[] a) {} public static void DrawLine(Vector3 a, Vector3 b, Color c) {} public static void DrawRay(Vector3 a, Vector3 b, Color c) {}
    }
    public static partial class Random
    {
        public static float Range(float a, float b) => a; public static float value => 0; public static Vector3 insideUnitSphere => default; public static Vector2 insideUnitCircle => default;
        public static Vector3 onUnitSphere => default; public static Quaternion rotation => default; public static void InitState(int s) {}
        public static Color ColorHSV(float a, float b, float c, float d, float e, float f) => default;
    }
    public partial class Renderer { public Material material; public Material[] materials; public int sortingOrder; public bool forceRenderingOff; public bool isVisible; }
    public partial class MeshFilter { public Mesh mesh; }
    public partial class Collider { public bool enabled, isTrigger; public Bounds bounds; public Rigidbody attachedRigidbody; }
    public class BoxCollider : Collider { public Vector3 center, size; }
    public class SphereCollider : Collider { public Vector3 center; public float radius; }
    public class CapsuleCollider : Collider { public Vector3 center; public float radius, height; public int direction; }
    public class MeshCollider : Collider { public Mesh sharedMesh; public bool convex; }
    public partial class Rigidbody : Component { public bool isKinematic, useGravity; public Vector3 velocity, linearVelocity, angularVelocity; public float mass, drag, angularDrag, linearDamping, angularDamping;
        public RigidbodyConstraints constraints; public void AddForce(Vector3 f) {} public void AddForce(Vector3 f, ForceMode m) {} public void MovePosition(Vector3 p) {} public void MoveRotation(Quaternion q) {}
        public Vector3 position; public float maxAngularVelocity; public void AddForceAtPosition(Vector3 f, Vector3 p, ForceMode m) {} public CollisionDetectionMode collisionDetectionMode; public RigidbodyInterpolation interpolation; public void AddTorque(Vector3 t) {} public void AddTorque(Vector3 t, ForceMode m) {} public void AddExplosionForce(float f, Vector3 p, float r, float up, ForceMode m) {} }
    public enum RigidbodyInterpolation { None, Interpolate, Extrapolate }
    public enum CollisionDetectionMode { Discrete, Continuous, ContinuousDynamic, ContinuousSpeculative }
    public enum PhysicsMaterialCombine { Average, Minimum, Multiply, Maximum }
    public partial class PhysicsMaterial : Object { public PhysicsMaterial(string n) {} public float bounciness, dynamicFriction, staticFriction; public PhysicsMaterialCombine bounceCombine; }
    public partial class Collider { public PhysicsMaterial sharedMaterial; }
    public static partial class Physics { public static void IgnoreCollision(Collider a, Collider b, bool ignore) {} public static void IgnoreCollision(Collider a, Collider b) {} public static Collider[] OverlapSphere(Vector3 p, float r) => null; }
    public partial struct Quaternion { public static bool operator ==(Quaternion a, Quaternion b) => true; public static bool operator !=(Quaternion a, Quaternion b) => false; public override bool Equals(object o) => true; public override int GetHashCode() => 0; }
    public class CharacterController : Collider { public float height, radius, slopeLimit, stepOffset, skinWidth, minMoveDistance; public Vector3 center, velocity; public bool isGrounded;
        public CollisionFlags Move(Vector3 m) => CollisionFlags.None; public bool SimpleMove(Vector3 s) => false; }
    public partial class Mesh
    {
        public Mesh() {} public Vector3[] vertices, normals; public int[] triangles; public Vector2[] uv; public Color[] colors; public int vertexCount; public string name;
        public void RecalculateNormals() {} public void SetVertices(Vector3[] v) {} public void SetTriangles(int[] t, int s) {} public void SetNormals(List<Vector3> n) {} public void SetUVs(int ch, Vector2[] u) {}
        public int[] GetTriangles(int s) => null; public void Clear() {} public void UploadMeshData(bool b) {} public void MarkDynamic() {} public void SetColors(Color[] c) {}
        public UnityEngine.Rendering.IndexFormat indexFormat;
    }
    public partial class Shader { public static int PropertyToID(string n) => 0; }
    public partial class Texture { public int width, height; public TextureWrapMode wrapMode; public FilterMode filterMode; }
    public partial class Texture2D { public Texture2D(int w, int h) {} public void SetPixel(int x, int y, Color c) {} public void SetPixels(Color[] c) {} public void Apply() {} public static Texture2D blackTexture; }
    public partial class Material
    {
        public Material(Material m) {} public Color color; public Shader shader; public Texture mainTexture;
        public void SetFloat(int id, float v) {} public void SetColor(int id, Color c) {} public void SetInt(string n, int v) {} public float GetFloat(string n) => 0; public void DisableKeyword(string k) {}
        public void SetVector(string n, Vector4 v) {} public bool IsKeywordEnabled(string k) => false; public void CopyPropertiesFromMaterial(Material m) {}
    }
    public partial class MaterialPropertyBlock { public void SetColor(int id, Color c) {} public void SetFloat(string n, float v) {} public void SetFloat(int id, float v) {} public void Clear() {} }
    public partial class Light { public float spotAngle, shadowStrength, bounceIntensity; public LightmapBakeType lightmapBakeType; public bool useColorTemperature; public float colorTemperature; }
    public partial class Camera
    {
        public float fieldOfView, nearClipPlane, farClipPlane, depth, aspect; public Color backgroundColor; public CameraClearFlags clearFlags; public int cullingMask; public bool orthographic;
        public Ray ScreenPointToRay(Vector3 p) => default; public Ray ViewportPointToRay(Vector3 p) => default; public Vector3 WorldToScreenPoint(Vector3 p) => p; public Vector3 WorldToViewportPoint(Vector3 p) => p;
        public static Camera[] allCameras;
    }
    public struct Ray { public Ray(Vector3 o, Vector3 d) { origin = o; direction = d; } public Vector3 origin, direction; public Vector3 GetPoint(float d) => origin; }
    public partial struct RaycastHit { public Vector3 normal; public float distance; public Collider collider; public Transform transform; public Rigidbody rigidbody; }
    public static partial class Physics
    {
        public static bool Raycast(Ray r, out RaycastHit h, float m) { h = default; return false; }
        public static bool Raycast(Ray r, out RaycastHit h, float m, int mask) { h = default; return false; }
        public static bool Raycast(Ray r, out RaycastHit h, float m, int mask, QueryTriggerInteraction q) { h = default; return false; }
        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit h, float m, int mask) { h = default; return false; }
        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit h, float m, int mask, QueryTriggerInteraction q) { h = default; return false; }
        public static bool Raycast(Vector3 o, Vector3 d, float m) => false;
        public static bool SphereCast(Ray r, float rad, out RaycastHit h, float m, int mask, QueryTriggerInteraction q) { h = default; return false; }
        public static bool SphereCast(Ray r, float rad, out RaycastHit h, float m, int mask) { h = default; return false; }
        public static bool SphereCast(Vector3 o, float rad, Vector3 d, out RaycastHit h, float m, int mask, QueryTriggerInteraction q) { h = default; return false; }
        public static Vector3 gravity;
    }
    public partial class AudioClip { public static AudioClip Create(string n, int len, int ch, int f, bool s) => null; public bool SetData(float[] d, int o) => true; public string name; public int samples, frequency, channels; }
    public partial class AudioSource
    {
        public AudioClip clip; public float volume, minDistance, maxDistance, time; public bool loop, isPlaying, mute; public float dopplerLevel; public int priority;
        public void Play() {} public void Stop() {} public void Pause() {} public void PlayOneShot(AudioClip c) {} public static void PlayClipAtPoint(AudioClip c, Vector3 p) {} public static void PlayClipAtPoint(AudioClip c, Vector3 p, float v) {}
        public AudioRolloffMode rolloffMode;
    }
    public enum AudioRolloffMode { Logarithmic, Linear }
    public class AudioListener : Behaviour { public static float volume; }
    public partial class AnimationCurve { public AnimationCurve() {} public Keyframe[] keys; public static AnimationCurve EaseInOut(float a, float b, float c, float d) => null; public static AnimationCurve Linear(float a, float b, float c, float d) => null; public int AddKey(float t, float v) => 0; }
    public partial class Gradient { public Color Evaluate(float t) => default; }
    public partial class ParticleSystem
    {
        public partial struct MinMaxCurve { public float constant { get => 0; set {} } public float constantMin { get => 0; set {} } public float constantMax { get => 0; set {} } }
        public partial struct MinMaxGradient { public MinMaxGradient(Color c) {} public static implicit operator MinMaxGradient(Color c) => default; public static implicit operator MinMaxGradient(Gradient g) => default; public Color color { get => default; set {} } }
        public partial struct MainModule { public bool prewarm; public float startDelay_; public ParticleSystemScalingMode scalingMode; public MinMaxCurve startDelay; public bool startSize3D; public MinMaxCurve startSizeX, startSizeY, startSizeZ; public float simulationSpeed; public ParticleSystemStopAction stopAction; }
        public partial struct EmissionModule { public bool enabled; public MinMaxCurve rateOverDistance; public void SetBursts(Burst[] b) {} public int burstCount; }
        public struct Burst { public Burst(float t, short c) {} public Burst(float t, short a, short b) {} public Burst(float t, MinMaxCurve c) {} }
        public partial struct ShapeModule { public Vector3 position, scale; public float radiusThickness, arc, length; public float randomDirectionAmount; public Mesh mesh; }
        public partial struct VelocityOverLifetimeModule { public MinMaxCurve orbitalX, orbitalZ, speedModifier, offsetX, offsetY, offsetZ; }
        public partial struct SizeOverLifetimeModule { }
        public struct NoiseModule { public bool enabled; public MinMaxCurve strength; public float frequency; public float scrollSpeed_; public MinMaxCurve scrollSpeed; public int octaveCount; }
        public struct TrailModule { public bool enabled; public float ratio; public MinMaxCurve lifetime, widthOverTrail; public MinMaxGradient colorOverLifetime; public bool dieWithParticles; }
        public struct LimitVelocityOverLifetimeModule { public bool enabled; public MinMaxCurve limit; public float dampen; public float drag_; public MinMaxCurve drag; }
        public struct ForceOverLifetimeModule { public bool enabled; public MinMaxCurve x, y, z; public ParticleSystemSimulationSpace space; }
        public struct CollisionModule { public bool enabled; }
        public struct TextureSheetAnimationModule { public bool enabled; }
        public NoiseModule noise; public TrailModule trails; public LimitVelocityOverLifetimeModule limitVelocityOverLifetime; public ForceOverLifetimeModule forceOverLifetime; public CollisionModule collision; public TextureSheetAnimationModule textureSheetAnimation;
        public int particleCount; public bool isEmitting, isStopped, isPaused, isAlive_; public bool IsAlive() => false; public bool IsAlive(bool c) => false;
        public void Stop() {} public void Stop(bool c) {} public void Clear() {} public void Clear(bool c) {} public void Pause() {} public void Simulate(float t) {} public void Play(bool c) {}
        public struct EmitParams { public Vector3 position, velocity; public float startSize, startLifetime; public Color32 startColor; public bool applyShapeToPosition; public float rotation; }
        public void Emit(EmitParams p, int n) {}
        public struct Particle { public Vector3 position, velocity; public float remainingLifetime, startLifetime, startSize; public Color32 startColor; }
        public int GetParticles(Particle[] p) => 0; public void SetParticles(Particle[] p, int n) {} public void SetParticles(Particle[] p) {}
    }
    public enum ParticleSystemScalingMode { Hierarchy, Local, Shape }
    public enum ParticleSystemStopAction { None, Destroy, Disable }
    public partial class ParticleSystemRenderer { public Mesh mesh; public float maxParticleSize, minParticleSize, sortingFudge; public ParticleSystemSortMode sortMode; public Material trailMaterial; public ParticleSystemRenderSpace alignment; }
    public enum ParticleSystemSortMode { None, Distance, OldestInFront, YoungestInFront }
    public enum ParticleSystemRenderSpace { View, World, Local, Facing, Velocity }
    public class LineRenderer : Renderer { public int positionCount; public float startWidth, endWidth, widthMultiplier; public Color startColor, endColor; public bool useWorldSpace, loop; public AnimationCurve widthCurve; public Gradient colorGradient;
        public void SetPosition(int i, Vector3 p) {} public void SetPositions(Vector3[] p) {} public int numCornerVertices, numCapVertices; public LineAlignment alignment; public LineTextureMode textureMode; }
    public enum LineAlignment { View, TransformZ }
    public enum LineTextureMode { Stretch, Tile }
    public class TrailRenderer : Renderer { public float time, startWidth, endWidth, widthMultiplier, minVertexDistance; public Color startColor, endColor; public bool emitting, autodestruct; public Gradient colorGradient; public AnimationCurve widthCurve; public void Clear() {} }
    public class TextMesh : Component { public string text; public float characterSize; public int fontSize; public Color color; public TextAnchor anchor; public TextAlignment alignment; public Font font; }
    public enum TextAlignment { Left, Center, Right }
    public class Font : Object { public Material material; }

    public partial class GUIStyle { public GUIStyle() {} public FontStyle fontStyle; public RectOffset padding, margin; public bool richText; public float CalcHeight(GUIContent c, float w) => 0; public Vector2 CalcSize(GUIContent c) => default; public GUIStyleState hover, active; public float fixedHeight, fixedWidth; public bool stretchWidth; }
    public partial class GUIStyleState { public Texture2D background; }
    public partial class GUISkin { public GUIStyle label, button, textField, window, horizontalSlider, toggle; }
    public class GUIContent { public static GUIContent none; public GUIContent() {} public GUIContent(string t) {} }
    public class GUILayoutOption {}
    public static partial class GUI
    {
        public static Color color, backgroundColor, contentColor; public static int depth; public static bool enabled; public static Matrix4x4 matrix;
        public static void Box(Rect r, string t) {} public static void Box(Rect r, GUIContent c) {} public static void Box(Rect r, GUIContent c, GUIStyle s) {}
        public static void Label(Rect r, string t) {} public static void Label(Rect r, string t, GUIStyle s) {} public static bool Button(Rect r, string t) => false; public static bool Button(Rect r, string t, GUIStyle s) => false;
        public static void DrawTexture(Rect r, Texture t) {} public static float HorizontalSlider(Rect r, float v, float a, float b) => v;
    }
    public static class GUILayout
    {
        public static void BeginArea(Rect r) {} public static void EndArea() {} public static Vector2 BeginScrollView(Vector2 p, params GUILayoutOption[] o) => p; public static void EndScrollView() {}
        public static bool Button(string t, params GUILayoutOption[] o) => false; public static bool Button(string t, GUIStyle s, params GUILayoutOption[] o) => false;
        public static void Label(string t, params GUILayoutOption[] o) {} public static void Label(string t, GUIStyle s, params GUILayoutOption[] o) {}
        public static GUILayoutOption Height(float h) => null; public static GUILayoutOption Width(float w) => null; public static GUILayoutOption ExpandWidth(bool b) => null;
        public static void BeginHorizontal(params GUILayoutOption[] o) {} public static void EndHorizontal() {} public static void BeginVertical(params GUILayoutOption[] o) {} public static void EndVertical() {}
        public static void Space(float p) {} public static void FlexibleSpace() {} public static bool Toggle(bool v, string t, params GUILayoutOption[] o) => v; public static float HorizontalSlider(float v, float a, float b, params GUILayoutOption[] o) => v;
    }
    public static partial class Input
    {
        public static bool GetKey(KeyCode k) => false; public static bool GetKeyUp(KeyCode k) => false; public static float GetAxis(string n) => 0; public static float GetAxisRaw(string n) => 0;
        public static bool GetMouseButtonDown(int b) => false; public static bool GetMouseButton(int b) => false; public static bool GetMouseButtonUp(int b) => false; public static Vector3 mousePosition; public static Vector2 mouseScrollDelta;
        public static bool GetButtonDown(string n) => false; public static bool GetButton(string n) => false; public static bool anyKeyDown;
    }
    public static partial class Resources
    {
        public static T[] FindObjectsOfTypeAll<T>() where T : Object => new T[0]; public static T Load<T>(string p) where T : Object => null; public static T GetBuiltinResource<T>(string p) where T : Object => null;
    }
    public class RenderSettings { public static Color ambientLight, ambientSkyColor, ambientEquatorColor, ambientGroundColor, fogColor; public static bool fog; public static float fogDensity, ambientIntensity; public static Material skybox; public static UnityEngine.Rendering.AmbientMode ambientMode; public static Light sun; }
    public class RequireComponent : Attribute { public RequireComponent(Type t) {} public RequireComponent(Type a, Type b) {} }
    public class AddComponentMenu : Attribute { public AddComponentMenu(string s) {} }
    public class ExecuteAlways : Attribute {} public class ExecuteInEditMode : Attribute {}
    public class HideInInspector : Attribute {} public class SpaceAttribute : Attribute { public SpaceAttribute() {} public SpaceAttribute(float h) {} }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute() {} public TextAreaAttribute(int a, int b) {} }
    public class MultilineAttribute : Attribute { public MultilineAttribute() {} public MultilineAttribute(int l) {} }
    public class MinAttribute : Attribute { public MinAttribute(float m) {} }
    public class ContextMenu : Attribute { public ContextMenu(string s) {} }
    public class FormerlySerializedAsAttribute : Attribute { public FormerlySerializedAsAttribute(string s) {} }
    [AttributeUsage(AttributeTargets.All)] public class ColorUsageAttribute : Attribute { public ColorUsageAttribute(bool a) {} public ColorUsageAttribute(bool a, bool b) {} }
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute() {} }
}
namespace UnityEngine.Serialization { public class FormerlySerializedAsAttribute : System.Attribute { public FormerlySerializedAsAttribute(string s) {} } }
namespace UnityEngine.Rendering { public enum ShadowCastingMode { Off, On, TwoSided, ShadowsOnly } public enum IndexFormat { UInt16, UInt32 } public enum AmbientMode { Skybox, Trilight, Flat } public enum CompareFunction { Always, LessEqual }
    public class GraphicsSettings { public static RenderPipelineAsset defaultRenderPipeline, renderPipelineAsset; public static RenderPipelineAsset currentRenderPipeline; }
    public class RenderPipelineAsset : UnityEngine.ScriptableObject { public UnityEngine.Material defaultMaterial; public UnityEngine.Shader defaultShader; } }
namespace UnityEngine.SceneManagement
{
    public struct Scene { public bool IsValid() => true; public string name, path; public bool isLoaded, isDirty; public UnityEngine.GameObject[] GetRootGameObjects() => null; }
    public enum LoadSceneMode { Single, Additive }
    public static class SceneManager { public static Scene GetActiveScene() => default; public static void LoadScene(string n) {} public static void LoadScene(int i) {} public static void MoveGameObjectToScene(UnityEngine.GameObject g, Scene s) {} }
}
namespace UnityEngine.UI
{
    public class Graphic : UnityEngine.Behaviour { public UnityEngine.Color color; public bool raycastTarget; public UnityEngine.RectTransform rectTransform; }
    public class MaskableGraphic : Graphic {}
    public class Text : MaskableGraphic { public string text; public int fontSize; public UnityEngine.Font font; public UnityEngine.TextAnchor alignment; public UnityEngine.FontStyle fontStyle; public bool supportRichText; public HorizontalWrapMode horizontalOverflow; public VerticalWrapMode verticalOverflow; }
    public class Image : MaskableGraphic { public float fillAmount; public UnityEngine.Sprite sprite; public Type type; public enum Type { Simple, Filled } }
    public class RawImage : MaskableGraphic { public UnityEngine.Texture texture; }
    public enum HorizontalWrapMode { Wrap, Overflow } public enum VerticalWrapMode { Truncate, Overflow }
    public class CanvasScaler : UnityEngine.Behaviour { public ScaleMode uiScaleMode; public UnityEngine.Vector2 referenceResolution; public float matchWidthOrHeight; public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize } }
    public class GraphicRaycaster : UnityEngine.Behaviour {}
    public class Button : UnityEngine.Behaviour { public UnityEngine.Events.UnityEvent onClick; }
}
namespace UnityEngine
{
    public class Sprite : Object {}
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public class Canvas : Behaviour { public RenderMode renderMode; public int sortingOrder; public Camera worldCamera; public float planeDistance; }
    public class RectTransform : Transform { public Vector2 anchorMin, anchorMax, anchoredPosition, sizeDelta, pivot, offsetMin, offsetMax; public Rect rect; }
    public class CanvasGroup : Behaviour { public float alpha; public bool interactable, blocksRaycasts; }
}
namespace UnityEngine.InputSystem
{
    public enum Key { None, Space, Enter, Tab, Backquote, Quote, Semicolon, Comma, Period, Slash, Backslash, LeftBracket, RightBracket, Minus, Equals,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9, Digit0, LeftShift, RightShift, LeftAlt, RightAlt, LeftCtrl, RightCtrl,
        Escape, LeftArrow, RightArrow, UpArrow, DownArrow, NumpadEnter }
    public partial class KeyControl { }
    public partial class Keyboard
    {
        public KeyControl aKey, bKey, cKey, dKey, eKey, fKey, gKey, hKey, iKey, jKey, kKey, lKey, mKey, nKey, oKey, pKey, qKey, rKey, sKey, tKey, uKey, vKey, wKey, xKey, yKey, zKey;
        public KeyControl spaceKey, enterKey, numpadEnterKey, escapeKey, tabKey, leftShiftKey, rightShiftKey, leftCtrlKey, upArrowKey, downArrowKey, leftArrowKey, rightArrowKey, leftBracketKey, rightBracketKey;
        public KeyControl digit1Key, digit2Key, digit3Key, digit4Key, digit5Key, digit6Key, digit7Key, digit8Key, digit9Key, digit0Key;
        public bool anyKeyPressed_; public ButtonControl anyKey;
    }
    public class ButtonControl { public bool wasPressedThisFrame, isPressed, wasReleasedThisFrame; }
    public class Vector2Control { public UnityEngine.Vector2 ReadValue() => default; }
    public class AxisControl { public float ReadValue() => 0; }
    public class Mouse { public static Mouse current; public Vector2Control delta, position, scroll; public ButtonControl leftButton, rightButton, middleButton; }
    public class Gamepad { public static Gamepad current; public Vector2Control leftStick, rightStick; public ButtonControl buttonSouth, buttonNorth, buttonEast, buttonWest, startButton, leftShoulder, rightShoulder; }
}
namespace UnityEditor
{
    using UnityEngine;
    public enum InteractionMode { AutomatedAction, UserAction }
    public static partial class AssetDatabase
    {
        public static string[] FindAssets(string f) => new string[0]; public static string[] FindAssets(string f, string[] folders) => new string[0];
        public static string GUIDToAssetPath(string g) => ""; public static string AssetPathToGUID(string p) => ""; public static void Refresh() {} public static string GetAssetPath(UnityEngine.Object o) => "";
        public static UnityEngine.Object LoadAssetAtPath(string p, System.Type t) => null; public static UnityEngine.Object[] LoadAllAssetsAtPath(string p) => null;
        public static bool DeleteAsset(string p) => true; public static void ImportAsset(string p) {} public static void AddObjectToAsset(UnityEngine.Object o, string p) {} public static void AddObjectToAsset(UnityEngine.Object o, UnityEngine.Object a) {}
        public static bool Contains(UnityEngine.Object o) => false; public static string GenerateUniqueAssetPath(string p) => p; public static void SaveAssetIfDirty(UnityEngine.Object o) {}
    }
    public static partial class PrefabUtility
    {
        public static GameObject SaveAsPrefabAsset(GameObject g, string p) => g; public static GameObject SaveAsPrefabAsset(GameObject g, string p, out bool s) { s = true; return g; }
        public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object o) => o; public static GameObject SaveAsPrefabAssetAndConnect(GameObject g, string p, InteractionMode m) => g;
        public static void UnpackPrefabInstance(GameObject g, PrefabUnpackMode a, InteractionMode m) {} public static GameObject LoadPrefabContents(string p) => null; public static void UnloadPrefabContents(GameObject g) {}
    }
    public class EditorBuildSettingsScene { public EditorBuildSettingsScene() {} public EditorBuildSettingsScene(string p, bool e) {} public string path; public bool enabled; }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes; }
    public enum PrefabUnpackMode { OutermostRoot, Completely }
    public static partial class Undo
    {
        public static T AddComponent<T>(GameObject g) where T : Component => default; public static Component AddComponent(GameObject g, System.Type t) => null;
        public static void RecordObject(UnityEngine.Object o, string n) {} public static void SetTransformParent(Transform t, Transform p, string n) {} public static void RegisterCompleteObjectUndo(UnityEngine.Object o, string n) {}
    }
    public static partial class Selection { public static UnityEngine.Object[] objects; public static GameObject[] gameObjects; public static Transform activeTransform; }
    public static partial class EditorUtility
    {
        public static bool IsPersistent(UnityEngine.Object o) => false; public static bool DisplayDialog(string a, string b, string c) => true; public static bool DisplayDialog(string a, string b, string c, string d) => true;
        public static void DisplayProgressBar(string a, string b, float p) {} public static void ClearProgressBar() {} public static void FocusProjectWindow() {}
    }
    public static class EditorApplication { public static bool ExecuteMenuItem(string m) => true; public static bool isPlaying, isCompiling; public static void Beep() {} }
    public static class EditorGUIUtility { public static void PingObject(UnityEngine.Object o) {} }
    public partial class SerializedObject { public bool ApplyModifiedProperties() => true; public void Update() {} public UnityEngine.Object targetObject; }
    public partial class SerializedProperty
    {
        public bool boolValue; public int intValue, arraySize; public float floatValue; public Vector3 vector3Value; public Color colorValue; public int enumValueIndex;
        public void DeleteArrayElementAtIndex(int i) {} public SerializedProperty FindPropertyRelative(string n) => null;
    }
}
namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects } public enum NewSceneMode { Single, Additive }
    public static partial class EditorSceneManager
    {
        public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup s, NewSceneMode m) => default; public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup s) => default;
        public static bool SaveScene(UnityEngine.SceneManagement.Scene s, string p) => true; public static bool SaveScene(UnityEngine.SceneManagement.Scene s) => true;
        public static bool MarkSceneDirty(UnityEngine.SceneManagement.Scene s) => true; public static bool SaveCurrentModifiedScenesIfUserWantsTo() => true;
        public static UnityEngine.SceneManagement.Scene OpenScene(string p) => default;
    }
}
// Thêm cho MinerDigSequence/MinerSynth (rương Thợ Mỏ)
namespace UnityEngine
{
    public struct Plane { public Plane(Vector3 n, Vector3 p) {} public bool Raycast(Ray r, out float d) { d = 0; return false; } }
    public partial struct Vector3 { public static Vector3 Reflect(Vector3 a, Vector3 n) => a; }
}
namespace UnityEngine { public class DefaultExecutionOrder : System.Attribute { public DefaultExecutionOrder(int o) {} } }
// Chest_TheThao (bóng Rigidbody): phần lõi chưa có.
namespace UnityEngine
{
    public partial class PhysicsMaterial { public PhysicsMaterialCombine frictionCombine; }
    public partial class Rigidbody { public bool detectCollisions; }
}

// Chest_NhatBan (Nhát kiếm Iai): cắt lưới đôi + bỏ va chạm giữa mảnh dây và thân rương.
namespace UnityEngine
{
    public partial class Mesh { public bool isReadable; }
}

// ---- Thêm cho ThienCoHook (vật lý thật: lực xoắn, lực nổ, khớp bản lề, va chạm)
namespace UnityEngine
{
    public class Joint : Component { public Rigidbody connectedBody; public Vector3 anchor, axis, connectedAnchor; public bool autoConfigureConnectedAnchor; }
    public class HingeJoint : Joint { public bool useSpring, useLimits; }
}

// Gộp từ UnityStubAmNhacBass.cs / UnityStubGameVoxel.cs để check_all.sh biên dịch cả hai rương.
namespace UnityEngine
{
    public partial class Transform { public void Rotate(float x, float y, float z, Space s) {} }
    public partial class AudioSource { public void PlayDelayed(float d) {} }
    public static partial class GUI { public static void DrawTextureWithTexCoords(Rect r, Texture t, Rect uv) {} }
}
