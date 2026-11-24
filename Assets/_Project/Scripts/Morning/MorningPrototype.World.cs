using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ChoDoLa.Morning
{
    public sealed partial class MorningPrototype
    {
        private enum TableInteraction { None, Reading, Ordering }

        private readonly Dictionary<Collider, Action> worldActions = new Dictionary<Collider, Action>();
        private readonly List<Material> worldMaterials = new List<Material>();
        private GameObject world;
        private Camera worldCamera;
        private Collider staffCollider;
        private Collider staffHeadCollider;
        private bool savedCamera;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Color originalBackground;
        private CameraClearFlags originalClearFlags;
        private Transform servedBowl;
        private MorningPhase worldPhase;
        private TableInteraction tableInteraction;
        private bool dirtyWorld = true;
        private bool bowlPlaced;

        private static bool IsWorldPhase(MorningPhase phase) =>
            phase == MorningPhase.ChooseBrush || phase == MorningPhase.ChooseShop ||
            phase == MorningPhase.ChooseMeal || phase == MorningPhase.Serving;

        private void RefreshWorld()
        {
            if (!dirtyWorld && worldPhase == morning.Phase) return;
            dirtyWorld = false;
            worldPhase = morning.Phase;
            ClearWorld();
            if (!IsWorldPhase(worldPhase)) return;

            worldCamera = Camera.main;
            if (worldCamera == null) return;
            if (!savedCamera)
            {
                originalPosition = worldCamera.transform.position;
                originalRotation = worldCamera.transform.rotation;
                originalBackground = worldCamera.backgroundColor;
                originalClearFlags = worldCamera.clearFlags;
                savedCamera = true;
            }
            worldCamera.transform.SetPositionAndRotation(new Vector3(0f, 3.1f, -6.2f),
                Quaternion.LookRotation(new Vector3(0f, -2f, 6.2f)));
            worldCamera.backgroundColor = new Color(.18f, .26f, .25f);
            worldCamera.clearFlags = CameraClearFlags.SolidColor;
            world = new GameObject("Buổi sáng");
            world.transform.SetParent(transform, false);

            switch (worldPhase)
            {
                case MorningPhase.ChooseBrush: BuildSink(); break;
                case MorningPhase.ChooseShop: BuildShops(); break;
                default: BuildTable(); break;
            }
            Physics.SyncTransforms();
        }

        private void BuildSink()
        {
            Block("Kệ rửa mặt", new Vector3(0, .7f, 0), new Vector3(5.4f, .20f, 2f), new Color(.74f, .77f, .68f));
            Block("Bồn rửa", new Vector3(0, .85f, .55f), new Vector3(1.35f, .22f, .55f), Color.white);
            Block("Gương", new Vector3(0, 1.65f, .9f), new Vector3(2.2f, 1.5f, .12f), new Color(.46f, .67f, .68f));

            for (int i = 0; i < Brushes.Length; i++)
            {
                int index = i;
                float x = -2.2f + i * .78f;
                var color = Foam[(i + 1) % Foam.Length];
                Action pick = () =>
                {
                    brush = index;
                    feedback = $"Đã lấy bàn chải {Brushes[index].ToLower()}. Chọn kem đánh răng.";
                };
                Block("Cán bàn chải", new Vector3(x, .86f, -.44f), new Vector3(.15f, .11f, .62f), color, pick);
                Block("Lông bàn chải", new Vector3(x, .90f, -.74f),
                    new Vector3(.24f + i * .045f, .15f, .25f), Color.white, pick);
                Label(Brushes[i], new Vector3(x, .77f, -1.03f), .22f);
            }

            for (int i = 0; i < Pastes.Length; i++)
            {
                int index = i;
                float x = .65f + i * .75f;
                Action pick = () =>
                {
                    if (brush < 0)
                    {
                        feedback = "Lấy một bàn chải trên kệ trước.";
                        return;
                    }
                    if (morning.StartBrushing(brush, index)) feedback = "";
                };
                Block("Tuýp kem", new Vector3(x, .88f, -.49f), new Vector3(.32f, .15f, .55f), Foam[i], pick);
                Block("Nắp kem", new Vector3(x, .88f, -.8f), new Vector3(.19f, .13f, .15f), Color.white, pick);
                Label(Pastes[i], new Vector3(x, .77f, -1.03f), .22f);
            }
        }

        private void BuildShops()
        {
            for (int i = 0; i < Shops.Length; i++)
            {
                int index = i;
                float x = -1.38f + i * 2.76f;
                Action enter = () =>
                {
                    if (morning.ChooseShop(index))
                    {
                        tableInteraction = TableInteraction.None;
                        feedback = "";
                    }
                };
                Block(Shops[i], new Vector3(x, .75f, .05f), new Vector3(2.15f, 1.65f, .9f),
                    i == 0 ? Green : new Color(.59f, .33f, .21f), enter);
                Block("Mái", new Vector3(x, 1.72f, -.20f), new Vector3(2.4f, .21f, 1.14f),
                    i == 0 ? new Color(.67f, .36f, .23f) : new Color(.42f, .56f, .46f), enter);
                Label(Shops[i], new Vector3(x, .95f, -.43f), .37f);
            }
        }

        private void BuildTable()
        {
            Block("Bàn ăn", new Vector3(-.62f, .77f, 0), new Vector3(3.1f, .18f, 1.75f), new Color(.55f, .36f, .23f));
            Block("Chân bàn trái", new Vector3(-1.75f, .32f, .45f), new Vector3(.2f, .7f, .2f), Ink);
            Block("Chân bàn phải", new Vector3(.5f, .32f, .45f), new Vector3(.2f, .7f, .2f), Ink);
            Action read = () => tableInteraction = TableInteraction.Reading;
            Block("Thực đơn giấy", new Vector3(-.9f, .875f, -.4f), new Vector3(.92f, .045f, .62f), Cream, read);
            Label("Thực đơn", new Vector3(-.9f, .92f, -.75f), .26f);

            staffCollider = Block("Nhân viên", new Vector3(1.76f, 1.1f, -.08f),
                new Vector3(.65f, 1.9f, .64f), Green, shape: PrimitiveType.Capsule);
            staffCollider.enabled = true;
            staffHeadCollider = Block("Đầu nhân viên", new Vector3(1.76f, 2.12f, -.08f), new Vector3(.54f, .54f, .54f),
                new Color(.86f, .62f, .46f), shape: PrimitiveType.Sphere);
            staffHeadCollider.enabled = true;
            Label("Nhân viên · E", new Vector3(1.76f, 2.55f, -.13f), .26f);
            if (worldPhase == MorningPhase.Serving)
            {
                bowlPlaced = false;
                servedBowl = Block("Bát đang được phục vụ", new Vector3(1.53f, 1.35f, -.52f),
                    new Vector3(.49f, .16f, .49f), Color.white, shape: PrimitiveType.Cylinder).transform;
            }
        }

        private void UpdateServing()
        {
            if (servedBowl == null) return;
            var from = new Vector3(1.53f, 1.35f, -.52f);
            var to = new Vector3(-.22f, 1.02f, -.28f);
            float remaining = serveAt - Time.unscaledTime;
            servedBowl.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01((1.3f - remaining) / .82f));
            if (remaining <= .48f && !bowlPlaced)
            {
                bowlPlaced = true;
                sound.pitch = 1f;
                sound.PlayOneShot(tap);
                feedback = "Nhân viên: Món của bạn đây!";
            }
            if (remaining <= 0f) morning.Serve();
        }

        private void ReadWorldInput()
        {
            if (world == null || worldCamera == null || tableInteraction == TableInteraction.Ordering ||
                morning.Phase == MorningPhase.Serving) return;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return;
            var pointer = Mouse.current.position.ReadValue();
            bool clicked = Mouse.current.leftButton.wasPressedThisFrame;
            bool talked = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            var pointer = Input.mousePosition;
            bool clicked = Input.GetMouseButtonDown(0);
            bool talked = Input.GetKeyDown(KeyCode.E);
#endif
            if ((!clicked && !talked) || !Physics.Raycast(worldCamera.ScreenPointToRay(pointer), out var hit)) return;
            if (talked && (hit.collider == staffCollider || hit.collider == staffHeadCollider))
            {
                tableInteraction = TableInteraction.Ordering;
                return;
            }
            if (clicked && tableInteraction == TableInteraction.None && worldActions.TryGetValue(hit.collider, out var action)) action();
        }

        private void DrawTableInteraction()
        {
            if (tableInteraction == TableInteraction.Reading)
            {
                Fill(new Rect(224, 121, 412, 296), Cream);
                GUI.Label(new Rect(256, 150, 350, 36), $"Thực đơn · {Shops[morning.Shop]}", darkLine);
                for (int i = 0; i < 2; i++)
                    GUI.Label(new Rect(256, 211 + i * 39, 350, 36), Meals[morning.Shop, i], darkLine);
                if (GUI.Button(new Rect(256, 332, 348, 49), "Đặt thực đơn xuống", button))
                    tableInteraction = TableInteraction.None;
                return;
            }

            if (tableInteraction == TableInteraction.Ordering)
            {
                Fill(new Rect(32, 216, 796, 220), Ink);
                GUI.Label(new Rect(57, 231, 733, 37), "Nhân viên", title);
                GUI.Label(new Rect(59, 277, 730, 36), "Bạn dùng món gì hôm nay?", line);
                for (int i = 0; i < 2; i++)
                {
                    if (GUI.Button(new Rect(58 + i * 246, 345, 230, 54), Meals[morning.Shop, i], button) &&
                        morning.Order(i))
                    {
                        serveAt = Time.unscaledTime + 1.3f;
                        tableInteraction = TableInteraction.None;
                        feedback = "";
                    }
                }
                if (GUI.Button(new Rect(557, 345, 180, 54), "Để tôi xem thêm", button))
                    tableInteraction = TableInteraction.None;
                return;
            }

            if (GUI.Button(new Rect(42, 397, 174, 48), "Rời quán", button)) morning.ReturnToShops();
            DrawShortcuts();
        }

        private Collider Block(string label, Vector3 position, Vector3 size, Color color, Action action = null,
            PrimitiveType shape = PrimitiveType.Cube)
        {
            var prop = GameObject.CreatePrimitive(shape);
            prop.name = label;
            prop.transform.SetParent(world.transform, false);
            prop.transform.localPosition = position;
            prop.transform.localScale = size;

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var material = new Material(shader);
                material.color = color;
                worldMaterials.Add(material);
                prop.GetComponent<Renderer>().sharedMaterial = material;
            }

            var collider = prop.GetComponent<Collider>();
            if (action != null) worldActions.Add(collider, action);
            else collider.enabled = false;
            return collider;
        }

        private void Label(string text, Vector3 position, float size)
        {
            var label = new GameObject(text);
            label.transform.SetParent(world.transform, false);
            label.transform.localPosition = position;
            label.transform.rotation = worldCamera.transform.rotation;
            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.characterSize = size;
            mesh.fontSize = 64;
            mesh.color = Color.white;
        }

        private void RestoreCamera()
        {
            if (!savedCamera) return;
            if (worldCamera != null)
            {
                worldCamera.transform.SetPositionAndRotation(originalPosition, originalRotation);
                worldCamera.backgroundColor = originalBackground;
                worldCamera.clearFlags = originalClearFlags;
            }
            savedCamera = false;
        }

        private void ClearWorld()
        {
            if (world != null)
            {
                world.SetActive(false);
                Destroy(world);
                world = null;
            }
            foreach (var material in worldMaterials) Destroy(material);
            worldMaterials.Clear();
            worldActions.Clear();
            staffCollider = null;
            staffHeadCollider = null;
            servedBowl = null;
        }
    }
}
