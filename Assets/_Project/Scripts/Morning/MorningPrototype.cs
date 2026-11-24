using System;
using UnityEngine;

namespace ChoDoLa.Morning
{
    [RequireComponent(typeof(AudioSource))]
    public sealed partial class MorningPrototype : MonoBehaviour
    {
        private static readonly string[] Brushes = { "Thường", "Đầu to", "Cá sấu" };
        private static readonly string[] Pastes = { "Bạc hà", "Dâu", "Trà đá" };
        private static readonly string[] Shops = { "Quán phở", "Quán xôi" };
        private static readonly string[,] Meals = { { "Phở bò", "Phở gà" }, { "Xôi gà", "Xôi chà bông" } };
        private static readonly string[] SpotLabels = { "Mèo", "Tảng đá", "San hô", "Dép tổ ong" };
        private static readonly Color[] Foam = { new Color(.47f, .88f, .70f), new Color(1f, .58f, .75f), new Color(.89f, .72f, .35f) };
        private static readonly Color Ink = new Color(.12f, .18f, .17f);
        private static readonly Color Cream = new Color(.99f, .94f, .82f);
        private static readonly Color Green = new Color(.24f, .50f, .39f);
        private static readonly Rect Bowl = new Rect(90, 190, 335, 160);
        private static readonly Rect Mouth = new Rect(515, 190, 255, 160);

        [SerializeField] private bool standalonePreview = true;
        [SerializeField] private bool previewCat = true;

        private MorningSession morning;
        private AudioSource sound;
        private AudioClip gasp;
        private AudioClip meow;
        private AudioClip tap;
        private AudioClip scrub;
        private Texture2D[] spots;
        private GUIStyle title;
        private GUIStyle line;
        private GUIStyle darkLine;
        private GUIStyle button;
        private int brush;
        private int seed;
        private int brushZone = -1;
        private bool draggingBite;
        private bool shake = true;
        private bool notified;
        private float serveAt;
        private float effectUntil;
        private float nextScrub;
        private string feedback = "";

        public event Action<MorningSession> MorningCompleted;

        private void Awake()
        {
            sound = GetComponent<AudioSource>();
            sound.playOnAwake = false;
            gasp = MakeSound("Thở vì cay", .64f, 140f, .82f);
            meow = MakeSound("Mèo", .32f, 480f, .12f);
            tap = MakeSound("Chạm bát", .15f, 540f, .22f);
            scrub = MakeSound("Chà răng", .12f, 230f, .95f);
            spots = new Texture2D[SpotLabels.Length];
            for (int i = 0; i < spots.Length; i++) spots[i] = MakeSpotSprite((RareSpot)i);
            if (standalonePreview) BeginMorning(1, Environment.TickCount);
        }

        public void BeginMorning(int day, int runSeed)
        {
            seed = runSeed;
            morning = new MorningSession(runSeed, day, standalonePreview && previewCat);
            brushZone = -1;
            draggingBite = false;
            notified = false;
            effectUntil = 0;
            feedback = "";
            brush = -1;
            tableInteraction = TableInteraction.None;
            dirtyWorld = true;
        }

        private void Update()
        {
            if (morning == null)
            {
                ClearWorld();
                RestoreCamera();
                return;
            }

            RefreshWorld();
            ReadWorldInput();
            if (morning.Phase == MorningPhase.Serving) UpdateServing();
        }

        private void OnGUI()
        {
            if (morning == null) return;
            CreateStyles();
            var previous = GUI.matrix;
            var guiPointer = Event.current.mousePosition;
            float scale = Mathf.Min(Screen.width / 900f, Screen.height / 600f);
            float jolt = shake && Time.unscaledTime < effectUntil ? Mathf.Sin(Time.unscaledTime * 75f) * 3f : 0f;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 860 * scale) / 2 + jolt,
                (Screen.height - 550 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);

            if (IsWorldPhase(morning.Phase)) DrawWorldLabels();
            else Fill(new Rect(0, 0, 860, 550), Cream);
            Fill(new Rect(0, 0, 860, 94), Ink);
            GUI.Label(new Rect(26, 12, 790, 40), Heading(), title);
            GUI.Label(new Rect(29, 53, 800, 30), $"Ngày {morning.Day} · {Hint()}", line);

            switch (morning.Phase)
            {
                case MorningPhase.ChooseBrush: DrawShortcuts(); break;
                case MorningPhase.Brushing: DrawBrushing(); break;
                case MorningPhase.ChooseShop: DrawShortcuts(); break;
                case MorningPhase.ChooseMeal: DrawTableInteraction(); break;
                case MorningPhase.Serving:
                    GUI.Label(new Rect(29, 115, 760, 40), $"Nhân viên đang mang {Meals[morning.Shop, morning.Meal].ToLower()} ra…", line);
                    break;
                case MorningPhase.Eating: DrawEating(); break;
                case MorningPhase.Complete: DrawComplete(); break;
            }

            if (!string.IsNullOrEmpty(feedback))
            {
                if (IsWorldPhase(morning.Phase)) Fill(new Rect(20, 454, 820, 48), Cream);
                GUI.Label(new Rect(27, 458, 804, 45), feedback, darkLine);
            }
            if (morning.IsSpicy && Time.unscaledTime < effectUntil) DrawSpiceBorder();
            if (!IsWorldPhase(morning.Phase))
                shake = GUI.Toggle(new Rect(28, 512, 230, 24), shake, "Rung nhẹ khi ăn cay", darkLine);
            HandleWorldClick(guiPointer);
            GUI.matrix = previous;
        }

        private string Heading() => morning.Phase switch
        {
            MorningPhase.ChooseBrush or MorningPhase.Brushing => "Bồn rửa phòng trọ",
            MorningPhase.ChooseShop => "Ăn sáng ở chợ",
            MorningPhase.Complete => "Xong buổi sáng",
            _ => Shops[morning.Shop]
        };

        private string Hint() => morning.Phase switch
        {
            MorningPhase.ChooseBrush => brush < 0 ? "Bấm vào một bàn chải trên kệ" : "Bấm vào tuýp kem trên kệ để bắt đầu chà",
            MorningPhase.Brushing => "Giữ chuột trái và chà qua lại trên từng vùng răng",
            MorningPhase.ChooseShop => "Bấm vào quán phở hoặc quán xôi",
            MorningPhase.ChooseMeal => "Bấm vào thực đơn để xem · Chỉ vào nhân viên và nhấn E để gọi",
            MorningPhase.Eating => "Giữ chuột từ bát, kéo miếng ăn đến vùng miệng rồi thả",
            MorningPhase.Complete => "Bạn có thể ra chợ săn hộp",
            _ => "Chờ chủ quán đặt món xuống bàn"
        };

        private void DrawBrushing()
        {
            var input = Event.current;
            for (int zone = 0; zone < MorningSession.ZoneCount; zone++)
            {
                var rect = Teeth(zone);
                float clean = morning.Cleanliness(zone);
                Fill(rect, new Color(.97f, .99f, .98f));
                if (clean < 1)
                {
                    var dirt = new Rect(rect.x + 40, rect.y + 27, 130, 64);
                    if (zone == morning.RareZone)
                    {
                        var previous = GUI.color;
                        GUI.color = new Color(1, 1, 1, 1 - clean);
                        GUI.DrawTexture(dirt, spots[(int)morning.RareKind], ScaleMode.ScaleToFit);
                        GUI.color = previous;
                    }
                    else Fill(dirt, Color.Lerp(new Color(.42f, .32f, .16f), Cream, clean));
                }

                Fill(new Rect(rect.x, rect.yMax + 10, rect.width, 9), Ink);
                Fill(new Rect(rect.x, rect.yMax + 10, rect.width * clean, 9), Foam[morning.Paste]);
                if (clean > 0 && clean < 1)
                    Fill(new Rect(rect.x + 22, rect.yMax - 27, 165 * clean, 15), Foam[morning.Paste]);
                if (clean >= 1) GUI.Label(new Rect(rect.x + 18, rect.y + 43, 178, 40), "Sạch rồi", darkLine);
            }

            int hovered = -1;
            for (int zone = 0; zone < MorningSession.ZoneCount; zone++)
                if (Teeth(zone).Contains(input.mousePosition)) hovered = zone;

            if (input.type == EventType.MouseDown && input.button == 0 && hovered >= 0)
            {
                brushZone = hovered;
                input.Use();
            }
            else if (input.type == EventType.MouseDrag && brushZone >= 0)
            {
                if (hovered >= 0 && morning.BrushZone(hovered, Mathf.Min(50f, input.delta.magnitude) / 680f))
                {
                    if (Time.unscaledTime >= nextScrub)
                    {
                        sound.pitch = 1f + morning.Brush * .14f;
                        sound.PlayOneShot(scrub, .35f);
                        nextScrub = Time.unscaledTime + .12f;
                    }
                    if (hovered == morning.RareZone && morning.Cleanliness(hovered) >= 1)
                    {
                        sound.pitch = 1f;
                        sound.PlayOneShot(morning.RareKind == RareSpot.Cat ? meow : tap);
                        feedback = $"{SpotLabels[(int)morning.RareKind]} trôi theo bọt!";
                    }
                    if (morning.Phase == MorningPhase.ChooseShop)
                    {
                        brushZone = -1;
                        feedback = string.IsNullOrEmpty(feedback) ? "Đã súc miệng." : feedback + " Đã súc miệng.";
                    }
                }
                input.Use();
            }
            else if (input.type == EventType.MouseUp) brushZone = -1;

            if (brushZone >= 0)
            {
                var cursor = input.mousePosition;
                Fill(new Rect(cursor.x - 22, cursor.y, 96, 7), Green);
                Fill(new Rect(cursor.x - 22, cursor.y - 9, 29 + morning.Brush * 6, 9), Foam[morning.Paste]);
            }
            DrawShortcuts();
        }

        private void DrawEating()
        {
            Fill(Bowl, new Color(.79f, .61f, .38f));
            Fill(Mouth, new Color(.30f, .49f, .42f));
            GUI.Label(new Rect(Bowl.x + 20, Bowl.y + 12, 290, 35), Meals[morning.Shop, morning.Meal], darkLine);
            GUI.Label(new Rect(Mouth.x + 25, Mouth.y + 40, 205, 60), "Đưa lên miệng", line);
            for (int i = morning.Bites; i < MorningSession.BitesPerMeal; i++)
                Fill(new Rect(Bowl.x + 32 + i * 90, Bowl.y + 65, 66, 50), new Color(.99f, .86f, .55f));

            var input = Event.current;
            if (input.button == 0 && input.type == EventType.MouseDown && Bowl.Contains(input.mousePosition))
            {
                draggingBite = true;
                input.Use();
            }
            else if (input.button == 0 && input.type == EventType.MouseUp && draggingBite)
            {
                draggingBite = false;
                if (Mouth.Contains(input.mousePosition) && morning.EatBite()) PlayBiteReaction();
                input.Use();
            }
            if (draggingBite)
            {
                var cursor = input.mousePosition;
                Fill(new Rect(cursor.x, cursor.y, 82, 6), Ink);
                Fill(new Rect(cursor.x - 8, cursor.y - 10, 26, 20), new Color(.96f, .69f, .34f));
            }

            if (GUI.Button(new Rect(90, 380, 255, 52), $"Thêm ớt · {morning.Chili}/2", button))
                morning.AddChili();
            if (GUI.Button(new Rect(355, 380, 150, 52), "Thổi nguội", button) && morning.BlowOnFood())
            {
                sound.pitch = 1f;
                sound.PlayOneShot(gasp, .25f);
                feedback = "Bớt nóng rồi.";
            }
            DrawWater(new Rect(515, 380, 255, 52));
        }

        private void PlayBiteReaction()
        {
            sound.pitch = 1f;
            if (morning.IsSpicy)
            {
                feedback = "Hà… hà… Cay quá! Chủ quán đưa cốc nước tới.";
                effectUntil = Time.unscaledTime + 1.15f;
                sound.PlayOneShot(gasp, .75f);
            }
            else if (morning.LastBiteWasHot)
            {
                feedback = "Nóng quá! Thử thổi trước miếng kế tiếp.";
                sound.PlayOneShot(gasp, .42f);
            }
            else
            {
                feedback = "Vừa miệng. Chủ quán: Mai ăn như cũ nha?";
                sound.PlayOneShot(tap, .4f);
            }
        }

        private void DrawWater(Rect rect)
        {
            var enabled = GUI.enabled;
            GUI.enabled = morning.IsSpicy;
            if (GUI.Button(rect, "Uống nước", button) && morning.DrinkWater())
            {
                effectUntil = 0;
                feedback = "Dịu cay rồi.";
            }
            GUI.enabled = enabled;
        }

        private void DrawComplete()
        {
            DrawWater(new Rect(45, 158, 235, 54));
            if (GUI.Button(new Rect(45, 233, 350, 60), standalonePreview ? "Tiếp tục · Thử ngày sau" : "Tiếp tục ra chợ", button))
            {
                if (notified) return;
                notified = true;
                var completed = morning;
                if (standalonePreview) BeginMorning(completed.Day % 7 + 1, seed);
                else morning = null;
                MorningCompleted?.Invoke(completed);
            }
        }

        private void DrawShortcuts()
        {
            if (morning.Day == 1) return;
            if (GUI.Button(new Rect(544, 397, 130, 48), "Làm nhanh", button) && morning.TakeShortcut(true))
                feedback = "Đã làm nhanh.";
            if (GUI.Button(new Rect(689, 397, 130, 48), "Bỏ qua", button) && morning.TakeShortcut(false))
                feedback = "Đã bỏ qua.";
        }

        private static Rect Teeth(int zone) => new Rect(65 + zone * 250, 182, 212, 130);

        private void CreateStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 29, fontStyle = FontStyle.Bold };
            title.normal.textColor = Cream;
            line = new GUIStyle(GUI.skin.label) { fontSize = 17 };
            line.normal.textColor = Cream;
            darkLine = new GUIStyle(line);
            darkLine.normal.textColor = Ink;
            button = new GUIStyle(GUI.skin.button) { fontSize = 19 };
        }

        private static void Fill(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static void DrawSpiceBorder()
        {
            var red = new Color(.85f, .08f, .04f, .76f);
            Fill(new Rect(0, 0, 860, 9), red);
            Fill(new Rect(0, 541, 860, 9), red);
            Fill(new Rect(0, 0, 9, 550), red);
            Fill(new Rect(851, 0, 9, 550), red);
        }

        private static AudioClip MakeSound(string name, float duration, float frequency, float noise)
        {
            const int sampleRate = 22050;
            var samples = new float[Mathf.CeilToInt(duration * sampleRate)];
            var random = new System.Random(73);
            float air = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float position = (float)i / samples.Length;
                float t = (float)i / sampleRate;
                air = .78f * air + .22f * ((float)random.NextDouble() * 2f - 1f);
                float envelope = Mathf.Sin(position * Mathf.PI) * (1f - position);
                float tone = Mathf.Sin(t * frequency * (1.15f - .25f * position) * 2f * Mathf.PI);
                float pulse = noise > .7f ? .55f + .45f * Mathf.Cos(t * 27f) : 1f;
                samples[i] = envelope * pulse * (.55f * air * noise + .22f * tone * (1f - noise));
            }
            var clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Texture2D MakeSpotSprite(RareSpot kind)
        {
            var texture = new Texture2D(48, 48, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 48; y++)
                for (int x = 0; x < 48; x++)
                {
                    bool head = (x - 24) * (x - 24) + (y - 21) * (y - 21) < 260;
                    bool ear = y > 28 && y < 43 && (Mathf.Abs(x - 12) < (44 - y) / 2 || Mathf.Abs(x - 36) < (44 - y) / 2);
                    bool eyes = y >= 21 && y <= 24 && (Mathf.Abs(x - 17) < 2 || Mathf.Abs(x - 31) < 2);
                    float dx = Mathf.Abs(x - 24);
                    float dy = Mathf.Abs(y - 21);
                    Color pixel = kind switch
                    {
                        RareSpot.Cat => eyes ? Ink : head || ear ? new Color(.92f, .57f, .23f) : Color.clear,
                        RareSpot.Rock => dx + dy < 20 && y > 9 && y < 32 ? new Color(.48f, .49f, .51f) : Color.clear,
                        RareSpot.Coral => y > 6 && y < 40 && (dx < 3 || Mathf.Abs(dx - (y - 10) * .45f) < 3)
                            ? new Color(.95f, .44f, .48f) : Color.clear,
                        _ => dx * dx * 2.8f + dy * dy < 330
                            ? (dx < 5 && y > 17 && y < 28 ? Ink : new Color(.27f, .49f, .71f)) : Color.clear
                    };
                    texture.SetPixel(x, y, pixel);
                }
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            ClearWorld();
            RestoreCamera();
            Destroy(gasp);
            Destroy(meow);
            Destroy(tap);
            Destroy(scrub);
            if (spots != null)
                foreach (var sprite in spots) Destroy(sprite);
        }
    }
}
