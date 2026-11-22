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

        public bool StartNextRun()
        {
            if (Run.Status == "active") return false;
            Run = new MarketRun(content, NewSeed(), null, save.Write);
            RunReplaced?.Invoke();
            return true;
        }

        private static int NewSeed() => BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
    }
}
