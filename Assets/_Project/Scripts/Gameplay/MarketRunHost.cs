using System;
using System.IO;
using UnityEngine;

namespace ChoDoLa.Gameplay
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class MarketRunHost : MonoBehaviour
    {
        public const string SourceContentPath = "Docs/Content/data/content.json";
        public const string PlayerContentName = "cho-do-la-content.json";

        private MarketSaveFile save;
        private string content;
        private float elapsedFraction;
        private float retryClockAt;

        public bool TimedDaysEnabled { get; private set; }
        public event Action ClockPhaseChanged;

        public void EnableTimedDays() => TimedDaysEnabled = true;

        public MarketRun Run { get; private set; }
        public event Action RunReplaced;

        private void Awake()
        {
#if UNITY_EDITOR
            var contentPath = Path.Combine(Application.dataPath, "..", SourceContentPath);
#else
            var contentPath = Path.Combine(Application.streamingAssetsPath, PlayerContentName);
#endif
            content = File.ReadAllText(contentPath);
            save = new MarketSaveFile(Path.Combine(Application.persistentDataPath, "market-run.json"));
            Run = save.LoadOrStart(content, NewSeed(), backupPath =>
                Debug.LogWarning($"Started a fresh market run. Invalid save kept at {backupPath}.", this));
        }

        private void Update()
        {
            if (!TimedDaysEnabled || !Application.isFocused || Run == null ||
                Time.unscaledTime < retryClockAt) return;

            elapsedFraction += Time.deltaTime;
            int seconds = Mathf.FloorToInt(elapsedFraction);
            if (seconds == 0) return;
            elapsedFraction -= seconds;
            try
            {
                if (Run.AdvanceClock(seconds)) ClockPhaseChanged?.Invoke();
            }
            catch (IOException exception) { ReportClockSaveFailure(exception); }
            catch (UnauthorizedAccessException exception) { ReportClockSaveFailure(exception); }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) CheckpointClock();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) CheckpointClock();
        }

        private void OnApplicationQuit() => CheckpointClock();

        private void CheckpointClock()
        {
            if (!TimedDaysEnabled || Run == null) return;
            try { Run.SaveClock(); }
            catch (IOException exception) { ReportClockSaveFailure(exception); }
            catch (UnauthorizedAccessException exception) { ReportClockSaveFailure(exception); }
        }

        private void ReportClockSaveFailure(Exception exception)
        {
            retryClockAt = Time.unscaledTime + 3f;
            Debug.LogWarning($"Không lưu được giờ trong ván: {exception.Message}", this);
        }

        public bool StartNextRun()
        {
            if (Run.Status == "active") return false;
            Run = new MarketRun(content, NewSeed(), null, save.Write);
            elapsedFraction = 0f;
            RunReplaced?.Invoke();
            return true;
        }

        private static int NewSeed() => BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
    }
}
