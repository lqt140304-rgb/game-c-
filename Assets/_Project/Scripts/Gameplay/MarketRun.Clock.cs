using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChoDoLa.Gameplay
{
    public sealed partial class MarketRun
    {
        private JToken ClockRules => content["day_clock"];
        private int MorningSeconds => (int)ClockRules["morning_seconds"];
        private int CloseSeconds => (int)ClockRules["market_close_seconds"];
        private int MaxDaySeconds => (int)ClockRules["day_seconds"];

        public int ElapsedDaySeconds => state.ElapsedDaySeconds;
        public string ClockPhase => DayPhase(state.ElapsedDaySeconds);
        public bool IsNight => state.ElapsedDaySeconds >= CloseSeconds;
        public int DayClockMinute
        {
            get
            {
                var start = (int)ClockRules["start_minute"];
                var close = (int)ClockRules["market_close_minute"];
                var end = (int)ClockRules["end_minute"];
                var elapsed = state.ElapsedDaySeconds;
                if (elapsed < CloseSeconds) return start + (close - start) * elapsed / CloseSeconds;
                return close + (end - close) * (elapsed - CloseSeconds) / (MaxDaySeconds - CloseSeconds);
            }
        }

        public bool AdvanceClock(int seconds)
        {
            lock (writeLock)
            {
                if (seconds <= 0 || state.Status != "active" || state.ElapsedDaySeconds >= MaxDaySeconds) return false;
                var previous = state.ElapsedDaySeconds;
                var elapsed = previous + Math.Min(seconds, MaxDaySeconds - previous);
                bool phaseChanged = DayPhase(previous) != DayPhase(elapsed);
                int checkpoint = (int)ClockRules["checkpoint_seconds"];
                state.ElapsedDaySeconds = elapsed;
                try
                {
                    if (phaseChanged || elapsed / checkpoint != previous / checkpoint || elapsed == MaxDaySeconds)
                        persist?.Invoke(JsonConvert.SerializeObject(state));
                }
                catch
                {
                    state.ElapsedDaySeconds = previous;
                    throw;
                }
                return phaseChanged;
            }
        }

        public void SaveClock()
        {
            lock (writeLock)
                persist?.Invoke(JsonConvert.SerializeObject(state));
        }

        public Result Sleep(string transactionId, int expectedDay, bool postpone = false) =>
            SettleDay(transactionId, expectedDay, postpone, requireNight: true);

        private string DayPhase(int elapsed) =>
            elapsed < MorningSeconds ? "morning" : elapsed < CloseSeconds ? "market" : "night";
    }
}
