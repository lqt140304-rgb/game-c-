using UnityEngine;

namespace ChoDoLa.ChestFx
{
    /// <summary>
    /// Chest_TuoiTho: đèn nến trong lồng đèn nhấp nháy suốt vòng quay (mỗi tick nến chập chờn mạnh hơn), rồi
    /// khi khoá bậc thì một đàn đom đóm bay ra thành vòng năm cánh quanh đèn. Rác: nến tắt phụt (hài, như pin hết).
    /// Chỉ trang trí, không đổi bậc.
    /// </summary>
    public class TuoiThoHook : ChestFxHook
    {
        [SerializeField] private Color candleColor = new Color(1f, 0.62f, 0.25f);
        [SerializeField] private float candleIntensity = 1.6f;
        [SerializeField] private float candleRange = 1.2f;
        [SerializeField] private int fireflies = 25;
        [SerializeField] private float starRadius = 0.28f;

        private Light _candle;
        private ParticleSystem _ps;
        private ChestFxProfile _style;
        private float _flickerBoost;
        private float _target;
        private bool _dying;

        public override void OnRouletteStart(ChestRevealSequence reveal, Vector3 mouth)
        {
            if (_candle == null)
            {
                GameObject go = new GameObject("TuoiThoCandle");
                go.transform.SetParent(reveal.transform, false);
                _candle = go.AddComponent<Light>();
                _candle.type = LightType.Point;
                _candle.color = candleColor;
                _candle.range = candleRange;
                _candle.shadows = LightShadows.None;
            }
            _candle.transform.position = mouth;
            _candle.enabled = true;
            _target = candleIntensity;
            _dying = false;
        }

        public override void OnRouletteStep(ChestRevealSequence reveal, ChestTier shown, Color color)
        {
            _flickerBoost = 1f;   // ngọn nến giật lên mỗi tick
            if (_candle != null) _candle.color = Color.Lerp(candleColor, color, 0.25f);
        }

        public override void OnTierLocked(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (tier == ChestTier.Rac)
            {
                _dying = true;   // nến tắt phụt
                return;
            }
            if (_candle != null) _candle.color = Color.Lerp(candleColor, color, 0.5f);
            _target = candleIntensity * (1f + tier.Rank01());
            EmitStar(reveal, tier, color);
        }

        private void EmitStar(ChestRevealSequence reveal, ChestTier tier, Color color)
        {
            if (_ps == null)
            {
                _style = ScriptableObject.CreateInstance<ChestFxProfile>();
                _style.hideFlags = HideFlags.DontSave;
                _style.particleLook = ParticleLook.SoftDot;
                _style.particleDirection = ParticleDirection.Radial;
                _style.particleSpeed = 0.04f;
                _style.particleSize = 0.03f;
                _style.particleLifetime = 1.8f;
                _style.secondaryColor = candleColor;
                _style.secondaryMix = 0.5f;
                _ps = ChestFxFactory.CreateParticles("TuoiThoFireflies", null, _style, starRadius * 4f, true);
            }
            _ps.transform.SetPositionAndRotation(reveal.MouthPosition, reveal.transform.rotation);
            ChestFxFactory.SetParticleColor(_ps, color, _style, 1f);
            _ps.Play();

            // Vòng năm cánh: đom đóm đặt dọc năm đỉnh và năm đáy của ngôi sao (nằm ngang trên miệng đèn).
            int count = Mathf.RoundToInt(fireflies * (1f + tier.Rank01() * 2f));
            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            Vector3 up = reveal.transform.up;
            Vector3 right = reveal.transform.right;
            Vector3 fwd = reveal.transform.forward;
            for (int i = 0; i < count; i++)
            {
                float t = (i / (float)count) * 10f;            // 10 đoạn: đỉnh, đáy, đỉnh...
                int seg = Mathf.FloorToInt(t);
                float f = t - seg;
                float r0 = seg % 2 == 0 ? starRadius : starRadius * 0.45f;
                float r1 = seg % 2 == 0 ? starRadius * 0.45f : starRadius;
                float a0 = seg * Mathf.PI / 5f, a1 = (seg + 1) * Mathf.PI / 5f;
                Vector3 p0 = (right * Mathf.Sin(a0) + fwd * Mathf.Cos(a0)) * r0;
                Vector3 p1 = (right * Mathf.Sin(a1) + fwd * Mathf.Cos(a1)) * r1;
                Vector3 local = Vector3.Lerp(p0, p1, f);
                ep.position = reveal.MouthPosition + local + up * 0.05f;
                ep.velocity = up * 0.08f + local * 0.15f;
                _ps.Emit(ep, 1);
            }
        }

        private void Update()
        {
            if (_candle == null || !_candle.enabled) return;
            if (_dying)
            {
                _candle.intensity = Mathf.MoveTowards(_candle.intensity, 0f, Time.deltaTime * 4f);
                if (_candle.intensity <= 0f) _candle.enabled = false;
                return;
            }
            _flickerBoost = Mathf.MoveTowards(_flickerBoost, 0f, Time.deltaTime * 6f);
            float noise = Mathf.PerlinNoise(Time.time * 7f, 0.3f);
            _candle.intensity = _target * (0.75f + 0.35f * noise + 0.4f * _flickerBoost);
        }

        public override void OnReset(ChestRevealSequence reveal) => Cleanup();

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            if (_ps != null)
            {
                Destroy(_ps.GetComponent<ParticleSystemRenderer>().sharedMaterial);
                Destroy(_ps.gameObject);
            }
            if (_style != null) Destroy(_style);
            if (_candle != null) Destroy(_candle.gameObject);
            _ps = null;
            _style = null;
            _candle = null;
        }
    }
}
