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

        private struct TrafficVehicle
        {
            public Transform Target;
            public bool IsEastWest;
            public float Speed;
            public float MinBound;
            public float MaxBound;
        }

        private struct NpcActor
        {
            public Transform Root;
            public Transform Head;
            public Transform Torso;
            public Transform ArmL;
            public Transform ArmR;
            public Transform LegL;
            public Transform LegR;
            public bool IsWalker;
            public float WalkSpeed;
            public float MinZ;
            public float MaxZ;
            public float BaseY;
            public float Phase;
        }

        private readonly List<SpinItem> _spinners = new List<SpinItem>();
        private readonly List<FloatItem> _floaters = new List<FloatItem>();
        private readonly List<FlickerLight> _flickers = new List<FlickerLight>();
        private readonly List<TrafficVehicle> _vehicles = new List<TrafficVehicle>();
        private readonly List<NpcActor> _npcs = new List<NpcActor>();
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
            _vehicles.Clear();
            _npcs.Clear();

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
                // Modern Hanoi Phở broth pot steam plumes & swaying Hanoi street tree canopies
                else if (n.StartsWith("PhoSteam_"))
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.14f, Speed = 1.9f, Phase = phaseCounter += 1.1f, SpinYDeg = 28f });
                }
                else if (n.StartsWith("TreeCanopy_"))
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.035f, Speed = 1.15f, Phase = phaseCounter += 0.7f, SpinYDeg = 1.2f });
                }
                else if (n.StartsWith("KimKhuy_Display_"))
                {
                    _floaters.Add(new FloatItem { Target = t, BaseLocalPos = t.localPosition, Amplitude = 0.03f, Speed = 2.0f, Phase = phaseCounter += 0.9f, SpinYDeg = 32f });
                }
                else if (n.StartsWith("HanoiTraffic_EW_Pos_"))
                {
                    _vehicles.Add(new TrafficVehicle { Target = t, IsEastWest = true, Speed = n.Contains("_Bike_") ? 6.0f : 7.6f, MinBound = -44f, MaxBound = 44f });
                }
                else if (n.StartsWith("HanoiTraffic_EW_Neg_"))
                {
                    _vehicles.Add(new TrafficVehicle { Target = t, IsEastWest = true, Speed = n.Contains("_Bike_") ? -5.8f : -7.2f, MinBound = -44f, MaxBound = 44f });
                }
                else if (n.StartsWith("HanoiTraffic_NS_Pos_"))
                {
                    _vehicles.Add(new TrafficVehicle { Target = t, IsEastWest = false, Speed = n.Contains("_Bike_") ? 5.6f : 6.8f, MinBound = 56f, MaxBound = 94f });
                }
                else if (n.StartsWith("HanoiTraffic_NS_Neg_"))
                {
                    _vehicles.Add(new TrafficVehicle { Target = t, IsEastWest = false, Speed = n.Contains("_Bike_") ? -5.4f : -6.5f, MinBound = 56f, MaxBound = 94f });
                }
                else if (n.StartsWith("NPC_"))
                {
                    bool isWalker = n.Contains("Shopper_Walk");
                    _npcs.Add(new NpcActor
                    {
                        Root = t,
                        Head = t.Find("Head"),
                        Torso = t.Find("Torso"),
                        ArmL = t.Find("ArmL"),
                        ArmR = t.Find("ArmR"),
                        LegL = t.Find("LegL"),
                        LegR = t.Find("LegR"),
                        IsWalker = isWalker,
                        WalkSpeed = n.Contains("_Rev") ? -1.15f : 1.15f,
                        MinZ = 10.0f,
                        MaxZ = 92.0f,
                        BaseY = t.localPosition.y,
                        Phase = phaseCounter += 1.3f
                    });
                }
                else if (n == "Hand1" || n == "MainWheel" || n == "SubWheel" || n == "RingH" ||
                         n == "Hero_OculusGear" || n == "Hero_OculusRingA" ||
                         n == "Hero_ResRingOuter" || n == "Hero_ResRingInner" ||
                         n == "Scanner_CyanRing1" || n == "Scanner_CyanRing2" ||
                         n == "CraftGearA" || n == "CraftGearB" || n == "Bench_GearA" || n == "Bench_GearB")
                {
                    Vector3 spd = n == "Hand1"
                        ? new Vector3(0f, 0f, -24f)
                        : (n.Contains("Ring") || n.Contains("Oculus")
                            ? new Vector3(12f, 28f, -10f)
                            : new Vector3(0f, 36f, 0f));
                    _spinners.Add(new SpinItem { Target = t, AxisSpeedDeg = spd });
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

            for (int i = 0; i < _vehicles.Count; i++)
            {
                TrafficVehicle v = _vehicles[i];
                if (v.Target == null) continue;
                Vector3 pos = v.Target.localPosition;
                if (v.IsEastWest)
                {
                    pos.x += v.Speed * dt;
                    if (pos.x > v.MaxBound) pos.x = v.MinBound;
                    else if (pos.x < v.MinBound) pos.x = v.MaxBound;
                }
                else
                {
                    pos.z += v.Speed * dt;
                    if (pos.z > v.MaxBound) pos.z = v.MinBound;
                    else if (pos.z < v.MinBound) pos.z = v.MaxBound;
                }
                v.Target.localPosition = pos;
            }

            for (int i = 0; i < _npcs.Count; i++)
            {
                NpcActor npc = _npcs[i];
                if (npc.Root == null) continue;
                if (npc.IsWalker)
                {
                    Vector3 pos = npc.Root.localPosition;
                    pos.z += npc.WalkSpeed * dt;
                    if (pos.z > npc.MaxZ)
                    {
                        pos.z = npc.MaxZ;
                        npc.WalkSpeed = -Mathf.Abs(npc.WalkSpeed);
                        npc.Root.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else if (pos.z < npc.MinZ)
                    {
                        pos.z = npc.MinZ;
                        npc.WalkSpeed = Mathf.Abs(npc.WalkSpeed);
                        npc.Root.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    _npcs[i] = npc;
                    float swing = Mathf.Sin(t * 5.2f + npc.Phase);
                    pos.y = npc.BaseY + Mathf.Abs(swing) * 0.025f;
                    npc.Root.localPosition = pos;
                    if (npc.LegL != null) npc.LegL.localRotation = Quaternion.Euler(swing * 26f, 0f, 0f);
                    if (npc.LegR != null) npc.LegR.localRotation = Quaternion.Euler(-swing * 26f, 0f, 0f);
                    if (npc.ArmL != null) npc.ArmL.localRotation = Quaternion.Euler(-swing * 20f, 0f, 0f);
                    if (npc.ArmR != null) npc.ArmR.localRotation = Quaternion.Euler(swing * 16f, 0f, 0f);
                }
                else
                {
                    float idle = Mathf.Sin(t * 1.8f + npc.Phase);
                    if (npc.Torso != null) npc.Torso.localRotation = Quaternion.Euler(idle * 2.5f, 0f, 0f);
                    if (npc.Head != null) npc.Head.localRotation = Quaternion.Euler(idle * 3f, Mathf.Sin(t * 0.9f + npc.Phase) * 14f, 0f);
                    if (npc.ArmR != null) npc.ArmR.localRotation = Quaternion.Euler(-12f + Mathf.Sin(t * 2.4f + npc.Phase) * 8f, 0f, 0f);
                }
            }
        }
    }
}
