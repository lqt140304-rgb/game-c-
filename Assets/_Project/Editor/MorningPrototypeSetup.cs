using ChoDoLa.Morning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoDoLa.Editor
{
    internal static class MorningPrototypeSetup
    {
        [MenuItem("Tools/Cho Do La/Open Morning Prototype")]
        private static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var prototype = new GameObject("Morning Prototype");
            prototype.AddComponent<MorningPrototype>();
            Selection.activeGameObject = prototype;
        }
    }
}
