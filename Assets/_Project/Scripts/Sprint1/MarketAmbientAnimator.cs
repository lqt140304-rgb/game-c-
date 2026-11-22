using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.Sprint1
{
    /// <summary>
    /// Animates dynamic props across the market town every frame:
    /// - Fountain orrery rings, levitating runestones & orb
    /// - Shop 2 interlocking gable gears, sub-floor chrono-gears, toolboard gears, observatory dome core & shelf relics
    /// - Shop 1 arcane turret halo, sunken floor medallion rings & hovering mystic counter boxes
    /// - Warm lantern flame flicker and town facade accents
    /// </summary>
    public class MarketAmbientAnimator : MonoBehaviour
    {
        private struct SpinItem
        {
            public Transform Target;
            public Vector3 AxisSpeedDeg;
        }

        private struct FloatItem
        {
            public Transform Target;
            public Vector3 BaseLocalPos;
            public float Amplitude;
            public float Speed;
            public float Phase;
            public float SpinYDeg;
        }

        private struct FlickerLight
        {
            public Light Light;
            public float BaseIntensity;
            public float Seed;
        }

        private readonly List<SpinItem> _spinners = new List<SpinItem>();
        private readonly List<FloatItem> _floaters = new List<FloatItem>();
        private readonly List<FlickerLight> _flickers = new List<FlickerLight>();
        private bool _discovered;

        private void Start()
        {
            DiscoverTargets();
        }

        public void DiscoverTargets()
        {
            if (_discovered) return;
            _discovered = true;

            _spinners.Clear();
            _floaters.Clear();
            _flickers.Clear();

            // Scan Sprint1_Market and MarketTown pieces
            RegisterRecursive(transform);

            GameObject townRoot = GameObject.Find("MarketTown");
            if (townRoot != null)
            {
                RegisterTownAccents(townRoot.transform);
            }
        }

        private void RegisterRecursive(Transform root)
        {
            float phaseCounter = 0f;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;

                // Fountain Astrolabe & Levitating Runes
                if (n == "Orrery_RingA")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(18f, 34f, 8f) });
                }
                else if (n == "Orrery_RingB")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(-24f, -28f, 14f) });
                }
                else if (n.StartsWith("LevitatingRune_"))
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.11f, Speed = 1.6f, Phase = phaseCounter += 1.57f, SpinYDeg = 35f });
                }
                else if (n == "Finial_Orb" || n == "Turret_Finial")
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.06f, Speed = 2.0f, Phase = phaseCounter += 0.9f, SpinYDeg = 45f });
                }
                else if (n == "Turret_LevitatingHalo")
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.08f, Speed = 1.5f, Phase = 0.4f, SpinYDeg = -42f });
                }
                // Shop 1 Sunken Floor Medallion
                else if (n == "FloorMedallion_GlowRing")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 18f, 0f) });
                }
                else if (n == "FloorMedallion_InnerBrass")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, -26f, 0f) });
                }
                // Shop 2 Clockwork Gears (Meshing Pairs!)
                else if (n == "Ext_GableGear_Big")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(32f, 0f, 0f) });
                }
                else if (n == "Ext_GableGear_Small")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(-52f, 0f, 0f) });
                }
                else if (n == "FloorPit_GearA")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 40f, 0f) });
                }
                else if (n == "FloorPit_GearB")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, -64f, 0f) });
                }
                else if (n == "FloorPit_CyanConduit")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, -20f, 0f) });
                }
                else if (n == "Wall_Gear_1")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 0f, 38f) });
                }
                else if (n == "Wall_Gear_2")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 0f, -58f) });
                }
                else if (n == "Dome_InnerAether" || n == "Well_Crystal")
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.09f, Speed = 1.8f, Phase = 0.2f, SpinYDeg = 55f });
                }
                // Shop 2 Display Shelf Relics
                else if (n == "Shelf_HoloCube" || n == "Shelf_VoidPrism")
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.045f, Speed = 2.1f, Phase = phaseCounter += 1.2f, SpinYDeg = 48f });
                }
                else if (n == "Shelf_Bird")
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 22f, 0f) });
                }
                // Counter Mystic Display Boxes (gentle hover on Arcane & Sci-Fi boxes)
                else if (n == "DisplayBox_RunicBox_Arcane" || n == "DisplayBox_HoloPod_SciFi")
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.014f, Speed = 2.2f, Phase = phaseCounter += 1.5f, SpinYDeg = 0f });
                }
            }

            foreach (Light l in root.GetComponentsInChildren<Light>(true))
            {
                if (l.type != LightType.Point) continue;
                if (l.name.Contains("Lantern") || l.name.Contains("Aqua") || l.name.Contains("Arcane"))
                {
                    _flickers.Add(new FlickerLight
                    {
                        Light = l,
                        BaseIntensity = l.intensity,
                        Seed = Random.Range(0f, 100f)
                    });
                }
            }
        }

        private void RegisterTownAccents(Transform townRoot)
        {
            float p = 0f;
            foreach (Transform t in townRoot.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                if (n.StartsWith("Acc_Arcane_"))
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.14f, Speed = 1.4f, Phase = p += 0.8f, SpinYDeg = 28f });
                }
                else if (n.StartsWith("Acc_SciFi_"))
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 18f, 10f) });
                }
                else if (n.StartsWith("Acc_Steam_"))
                {
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = new Vector3(0f, 0f, 26f) });
                }
                else if (n == "Well_Crystal")
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.16f, Speed = 1.5f, Phase = 0.5f, SpinYDeg = 35f });
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            for (int i = 0; i < _spinners.Count; i++)
            {
                SpinItem s = _spinners[i];
                if (s.Target == null) continue;
                s.Target.Rotate(s.AxisSpeedDeg * dt, Space.Self);
            }

            for (int i = 0; i < _floaters.Count; i++)
            {
                FloatItem f = _floaters[i];
                if (f.Target == null) continue;
                Vector3 pos = f.BaseLocalPos;
                pos.y += Mathf.Sin(t * f.Speed + f.Phase) * f.Amplitude;
                f.Target.localPosition = pos;
                if (Mathf.Abs(f.SpinYDeg) > 0.01f)
                {
                    f.Target.Rotate(0f, f.SpinYDeg * dt, 0f, Space.Self);
                }
            }

            for (int i = 0; i < _flickers.Count; i++)
            {
                FlickerLight fl = _flickers[i];
                if (fl.Light == null) continue;
                float n = Mathf.PerlinNoise(fl.Seed, t * 4.5f) - 0.5f;
                fl.Light.intensity = fl.BaseIntensity * (1f + n * 0.22f);
            }
        }
    }
}
