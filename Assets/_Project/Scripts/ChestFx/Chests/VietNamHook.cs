using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Bản sắc riêng của Chest_VietNam, chỉ trang trí (không đổi bậc):
    /// - Khi chốt: "sóng trống đồng" — các vòng sáng đồng tâm lan ra trên mặt phẳng vành nón, số vòng = 1 + bậc
    ///   (Rác 1 vòng ... Huyền thoại 5 vòng), cách nhau 0,12 s như vân trống đồng.
    /// - Từ Hiếm: mưa hạt lúa vàng (Streak, Fountain, rơi lại); từ Rất hiếm thêm cánh sen hồng lơ lửng bay lên.
    /// Gắn lên gốc rương (VietNamFxSetup làm sẵn prefab và đặt vào customFxPrefab).
    /// </summary>
    public class VietNamHook : ChestFxHook
    {
        [SerializeField] private Color bronze = new Color(0.95f, 0.7f, 0.35f);
        [SerializeField] private Color riceGold = new Color(1f, 0.88f, 0.45f);
        [SerializeField] private Color lotusPink = new Color(1f, 0.55f, 0.72f);
        [SerializeField] private float ringRadius = 0.9f;
        [SerializeField] private float ringSeconds = 1.1f;
        [SerializeField] private float ringGap = 0.12f;
        [SerializeField] private int riceGrains = 40;
        [SerializeField] private int lotusPetals = 24;

        private readonly List<GameObject> _rings = new List<GameObject>();
        private readonly List<Object> _owned = new List<Object>();
        private ParticleSystem _rice;
        private ParticleSystem _lotus;
        private ChestFxProfile _riceStyle;
        private ChestFxProfile _lotusStyle;

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            Vector3 mouth = reveal.MouthPosition;
            int rings = 1 + tier.Rank();
            for (int i = 0; i < rings; i++)
            {
                Color c = Color.Lerp(color, bronze, i % 2 == 0 ? 0.25f : 0.6f);
                StartCoroutine(Ring(mouth, reveal.transform.rotation, c, i * ringGap, 1f + 0.15f * i));
            }

            if (tier >= ChestTier.Hiem)
            {
                if (_rice == null) _rice = MakeRice();
                _rice.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
                ChestFxFactory.SetParticleColor(_rice, riceGold, _riceStyle, 1f);
                _rice.Play();
                _rice.Emit(Mathf.RoundToInt(riceGrains * (1f + tier.Rank01())));
            }
            if (tier >= ChestTier.RatHiem)
            {
                if (_lotus == null) _lotus = MakeLotus();
                _lotus.transform.SetPositionAndRotation(mouth, reveal.transform.rotation);
                ChestFxFactory.SetParticleColor(_lotus, lotusPink, _lotusStyle, 1f);
                _lotus.Play();
                _lotus.Emit(tier == ChestTier.HuyenThoai ? lotusPetals * 2 : lotusPetals);
            }
        }

        public override void OnReset(ChestRevealSequence reveal)
        {
            StopAllCoroutines();
            foreach (GameObject r in _rings) if (r != null) Destroy(r);
            _rings.Clear();
            if (_rice != null) _rice.Clear();
            if (_lotus != null) _lotus.Clear();
        }

        private IEnumerator Ring(Vector3 center, Quaternion rot, Color color, float delay, float sizeMul)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            GameObject go = new GameObject("VietNamDrumRing");
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetPositionAndRotation(center, rot);
            Mesh mesh = ChestFxFactory.CreateRingMesh(0.82f);
            Material mat = ChestFxFactory.CreateAdditive(ChestFxFactory.GetTexture(ParticleLook.SoftDot), "VietNamDrumRing");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            _rings.Add(go);

            float t = 0f;
            while (t < ringSeconds && go != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / ringSeconds);
                float ease = 1f - (1f - k) * (1f - k);
                float r = Mathf.Lerp(0.05f, ringRadius * sizeMul, ease);
                go.transform.localScale = new Vector3(r, 1f, r);
                ChestFxFactory.SetColor(mat, color * (1f - k) * 1.6f);
                yield return null;
            }
            _rings.Remove(go);
            if (go != null) Destroy(go);
            Destroy(mesh);
            Destroy(mat);
        }

        private ParticleSystem MakeRice()
        {
            _riceStyle = Style(ParticleLook.Streak, ParticleDirection.Fountain, 0.9f, 0.018f, 1.5f, 1.1f, Color.white, 0.2f);
            return ChestFxFactory.CreateParticles("VietNamRice", null, _riceStyle, 1f, true);
        }

        private ParticleSystem MakeLotus()
        {
            _lotusStyle = Style(ParticleLook.SoftDot, ParticleDirection.Swirl, 0.3f, 0.045f, 2.6f, -0.05f, Color.white, 0.25f);
            return ChestFxFactory.CreateParticles("VietNamLotus", null, _lotusStyle, 1.2f, true);
        }

        private ChestFxProfile Style(ParticleLook look, ParticleDirection dir, float speed, float size, float life, float gravity, Color secondary, float mix)
        {
            ChestFxProfile s = ScriptableObject.CreateInstance<ChestFxProfile>();
            s.hideFlags = HideFlags.DontSave;
            s.particleLook = look;
            s.particleDirection = dir;
            s.particleSpeed = speed;
            s.particleSize = size;
            s.particleLifetime = life;
            s.burstGravity = gravity;
            s.secondaryColor = secondary;
            s.secondaryMix = mix;
            _owned.Add(s);
            return s;
        }

        private void OnDestroy()
        {
            OnReset(null);
            foreach (ParticleSystem ps in new[] { _rice, _lotus })
            {
                if (ps == null) continue;
                Destroy(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                Destroy(ps.gameObject);
            }
            foreach (Object o in _owned) if (o != null) Destroy(o);
            _owned.Clear();
        }
    }
}
