#!/usr/bin/env bash
# Biên dịch thử các file Chest_TheThao (kịch bản luân lưu) với stub UnityEngine (không cần Unity). Cần mcs (mono).
# UnityStub.cs dùng chung thiếu API vật lý/âm: script này chép nó ra thư mục tạm rồi vá thêm (KHÔNG sửa file dùng chung).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT
python3 - "$here/UnityStub.cs" "$out/Stub.cs" <<'PY'
import sys
s = open(sys.argv[1]).read()
def rep(a, b):
    global s
    assert a in s, a
    s = s.replace(a, b, 1)
rep("public class Collider : Component {}", "public class Collider : Component { public PhysicsMaterial sharedMaterial; public bool isTrigger; }")
rep("public float sqrMagnitude => 0;", "public float sqrMagnitude => 0; public float magnitude => 0; public static Vector3 Cross(Vector3 a, Vector3 b) => a; public static bool operator ==(Vector3 a, Vector3 b) => true; public static bool operator !=(Vector3 a, Vector3 b) => false; public override bool Equals(object o) => true; public override int GetHashCode() => 0;")
rep("public static Quaternion operator *(Quaternion a, Quaternion b) => a;", "public static Quaternion operator *(Quaternion a, Quaternion b) => a; public static Quaternion Inverse(Quaternion q) => q; public static bool operator ==(Quaternion a, Quaternion b) => true; public static bool operator !=(Quaternion a, Quaternion b) => false; public override bool Equals(object o) => true; public override int GetHashCode() => 0;")
rep("public Vector3 TransformPoint(Vector3 v) => v;", "public Vector3 TransformPoint(Vector3 v) => v; public Vector3 TransformDirection(Vector3 v) => v; public Vector3 InverseTransformVector(Vector3 v) => v;")
rep("public T GetComponent<T>() => default; public T[] GetComponentsInChildren<T>() => null; public T[] GetComponentsInChildren<T>(bool b) => null; }",
    "public T GetComponent<T>() => default; public T[] GetComponentsInChildren<T>() => null; public T[] GetComponentsInChildren<T>(bool b) => null; public T GetComponentInParent<T>() => default; }")
rep("public void StopAllCoroutines() {}", "public void StopAllCoroutines() {} public void StopCoroutine(Coroutine c) {}")
rep("public void PlayOneShot(AudioClip c, float v) {} }", "public void PlayOneShot(AudioClip c, float v) {} public bool loop; public AudioClip clip; public float volume; public void Play() {} public void Stop() {} }")
rep("public enum KeyCode { T, Y, Space, R, C }", "public enum KeyCode { T, Y, Space, R, C, B, K }")
rep("public enum Key { T, Y, Space, R, C, F, Q, Escape }", "public enum Key { T, Y, Space, R, C, F, Q, Escape, B, K }")
s += """
namespace UnityEngine {
    public enum ForceMode { Force, Impulse }
    public enum RigidbodyInterpolation { None, Interpolate }
    public enum CollisionDetectionMode { Discrete, ContinuousSpeculative }
    public enum PhysicsMaterialCombine { Average, Maximum }
    public class PhysicsMaterial : Object { public PhysicsMaterial(string n) {} public float bounciness, dynamicFriction, staticFriction; public PhysicsMaterialCombine bounceCombine, frictionCombine; }
    public class Rigidbody : Component { public float mass, linearDamping, angularDamping; public bool isKinematic, detectCollisions; public Vector3 linearVelocity, angularVelocity;
        public RigidbodyInterpolation interpolation; public CollisionDetectionMode collisionDetectionMode;
        public void AddForce(Vector3 f, ForceMode m) {} public void AddExplosionForce(float f, Vector3 p, float r, float up, ForceMode m) {} }
    public class BoxCollider : Collider { public Vector3 size, center; }
    public class Collision { public Vector3 relativeVelocity; public Collider collider; }
}
"""
open(sys.argv[2], "w").write(s)
PY
mcs -langversion:experimental -target:library -nowarn:0169,0649,0414,0067,0660,0661 -out:"$out/UnityStub.dll" "$out/Stub.cs"
src=("$repo"/Assets/_Project/Scripts/ChestFx/*.cs "$repo"/Assets/_Project/Scripts/ChestFx/Chests/TheThao*.cs
     "$repo/Assets/_Project/Scripts/Interaction/BoxOpenSequence.cs" "$repo/Assets/_Project/Editor/TheThaoFxSetup.cs")
for defs in "UNITY_EDITOR" "UNITY_EDITOR;ENABLE_INPUT_SYSTEM" "NONE"; do
  echo "== defines: $defs"
  mcs -langversion:experimental -target:library -r:"$out/UnityStub.dll" -define:"$defs" -out:"$out/TheThao.dll" "${src[@]}"
done
echo OK
