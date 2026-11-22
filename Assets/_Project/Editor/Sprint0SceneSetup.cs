#if UNITY_EDITOR
using System.IO;
using ChoDoLa.Interaction;
using ChoDoLa.Player;
using ChoDoLa.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChoDoLa.Editor
{
    public static class Sprint0SceneSetup
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/Market_Test.unity";

        [MenuItem("Tools/Cho Do La/Create Sprint 0 Test Scene")]
        public static void CreateSprint0TestScene()
        {
            if (!Directory.Exists(SceneFolder))
            {
                Directory.CreateDirectory(SceneFolder);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            CreateEnvironmentPlaceholders();
            Sprint0HudView hud = CreateHud();
            CreatePlayer(hud);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[Sprint0] Created and saved test scene at {ScenePath}. Press Play to test First-Person & interaction.");
        }

        private static void CreateLighting()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.92f, 0.78f);
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateEnvironmentPlaceholders()
        {
            GameObject envRoot = new GameObject("Environment_Placeholders");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(envRoot.transform);
            floor.transform.localScale = new Vector3(3f, 1f, 3f);

            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter_NiemPhong_Placeholder";
            counter.transform.SetParent(envRoot.transform);
            counter.transform.position = new Vector3(-2.5f, 0.5f, 3.0f);
            counter.transform.localScale = new Vector3(2.2f, 1.0f, 0.9f);

            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table_OpenBox_Placeholder";
            table.transform.SetParent(envRoot.transform);
            table.transform.position = new Vector3(0f, 0.45f, 2.5f);
            table.transform.localScale = new Vector3(1.6f, 0.9f, 1.0f);

            GameObject testBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testBox.name = "TestBox_Placeholder";
            testBox.transform.SetParent(envRoot.transform);
            testBox.transform.position = new Vector3(0f, 1.08f, 2.5f);
            testBox.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            testBox.AddComponent<TestBoxInteractable>();
        }

        private static Sprint0HudView CreateHud()
        {
            GameObject canvasGo = new GameObject("HUD_Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            Sprint0HudView hudView = canvasGo.AddComponent<Sprint0HudView>();

            Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Status bar (top-left)
            GameObject statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(canvasGo.transform, false);
            Text statusText = statusGo.AddComponent<Text>();
            statusText.font = builtinFont;
            statusText.fontSize = 20;
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.UpperLeft;
            RectTransform statusRect = statusText.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(0f, 1f);
            statusRect.pivot = new Vector2(0f, 1f);
            statusRect.anchoredPosition = new Vector2(20f, -20f);
            statusRect.sizeDelta = new Vector2(800f, 40f);

            // Crosshair (center)
            GameObject crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform, false);
            Image crosshairImg = crosshairGo.AddComponent<Image>();
            crosshairImg.color = new Color(1f, 1f, 1f, 0.85f);
            RectTransform crossRect = crosshairImg.rectTransform;
            crossRect.anchorMin = new Vector2(0.5f, 0.5f);
            crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.sizeDelta = new Vector2(6f, 6f);

            // Interact prompt (below center)
            GameObject promptGo = new GameObject("InteractPromptText");
            promptGo.transform.SetParent(canvasGo.transform, false);
            Text promptText = promptGo.AddComponent<Text>();
            promptText.font = builtinFont;
            promptText.fontSize = 22;
            promptText.color = new Color(1f, 0.92f, 0.55f);
            promptText.alignment = TextAnchor.MiddleCenter;
            RectTransform promptRect = promptText.rectTransform;
            promptRect.anchorMin = new Vector2(0.5f, 0.5f);
            promptRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptRect.anchoredPosition = new Vector2(0f, -45f);
            promptRect.sizeDelta = new Vector2(500f, 40f);

            SerializedObject serializedHud = new SerializedObject(hudView);
            serializedHud.FindProperty("statusText").objectReferenceValue = statusText;
            serializedHud.FindProperty("interactPromptText").objectReferenceValue = promptText;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            return hudView;
        }

        private static void CreatePlayer(Sprint0HudView hudView)
        {
            GameObject playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 1.0f, 0f);

            CharacterController cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;

            FirstPersonController fps = playerGo.AddComponent<FirstPersonController>();
            PlayerInteractor interactor = playerGo.AddComponent<PlayerInteractor>();

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(playerGo.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            Camera cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();

            SerializedObject serializedFps = new SerializedObject(fps);
            serializedFps.FindProperty("cameraPivot").objectReferenceValue = camGo.transform;
            serializedFps.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedInteractor = new SerializedObject(interactor);
            serializedInteractor.FindProperty("playerCamera").objectReferenceValue = cam;
            serializedInteractor.FindProperty("hudView").objectReferenceValue = hudView;
            serializedInteractor.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene s in existing)
            {
                if (s.path == scenePath) return;
            }

            EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[existing.Length + 1];
            existing.CopyTo(updated, 0);
            updated[existing.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
