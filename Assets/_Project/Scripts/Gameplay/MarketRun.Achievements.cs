using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChoDoLa.Gameplay
{
    public sealed partial class MarketRun
    {
        public sealed class AchievementView
        {
            public string Id { get; internal set; }
            public string Name { get; internal set; }
            public string Icon { get; internal set; }
            public string Description { get; internal set; }
            public int Target { get; internal set; }
            public int Progress { get; internal set; }
            public bool Unlocked { get; internal set; }
        }

        private sealed class AchievementProgress
        {
            public HashSet<string> OpenedTypes { get; set; } = new HashSet<string>();
            public HashSet<string> InspectedTypes { get; set; } = new HashSet<string>();
            public HashSet<string> Unlocked { get; set; } = new HashSet<string>();
            public bool SoldFake { get; set; }
            public bool CompletedCombo { get; set; }
            public bool WonRun { get; set; }
        }

        public event Action<AchievementView> AchievementUnlocked;
        public IReadOnlyList<AchievementView> Achievements => ReadAchievements(state.Progress).ToArray();

        public Result RecordAchievementInspection(string transactionId, string itemId)
        {
            lock (writeLock)
            {
                var item = state.Inventory.Find(entry => entry.Id == itemId);
                if (item == null) return new Result("ITEM_UNAVAILABLE");
                if (state.Progress.InspectedTypes.Contains(item.ItemId)) return new Result(entityId: itemId);
            }
            return Apply(transactionId, next =>
            {
                var item = next.Inventory.Find(entry => entry.Id == itemId);
                if (item == null) return new Result("ITEM_UNAVAILABLE");
                next.Progress.InspectedTypes.Add(item.ItemId);
                return new Result(entityId: itemId);
            });
        }

        public bool StartFreshRun(int seed)
        {
            lock (writeLock)
            {
                if (Status == "active") return false;
                var next = CreateFreshState(seed);
                next.Progress = state.Progress;
                persist?.Invoke(JsonConvert.SerializeObject(next));
                state = next;
            }
            Changed?.Invoke();
            return true;
        }

        private State CreateFreshState(int seed) => new State
        {
            Seed = seed,
            Cash = (int)rules["start_cash"],
            HeirloomId = InitialHeirloom(seed)
        };

        private static bool ValidAchievementSave(AchievementProgress progress) =>
            progress != null && progress.OpenedTypes != null &&
            progress.InspectedTypes != null && progress.Unlocked != null;

        private IEnumerable<AchievementView> ReadAchievements(AchievementProgress progress)
        {
            foreach (var definition in content["achievements"] ?? new JArray())
            {
                var id = (string)definition["id"];
                int target = (int)definition["target"];
                int count = (string)definition["condition"] switch
                {
                    "open" => progress.OpenedTypes.Count,
                    "open_types" => definition["box_types"].Values<string>().Count(progress.OpenedTypes.Contains),
                    "inspect_types" => progress.InspectedTypes.Count,
                    "sell_fake" => progress.SoldFake ? 1 : 0,
                    "combo" => progress.CompletedCombo ? 1 : 0,
                    "win" => progress.WonRun ? 1 : 0,
                    _ => 0
                };
                bool unlocked = progress.Unlocked.Contains(id);
                yield return new AchievementView
                {
                    Id = id,
                    Name = (string)definition["name"],
                    Icon = (string)definition["icon"],
                    Description = (string)definition["description"],
                    Target = target,
                    Progress = unlocked ? target : Math.Min(count, target),
                    Unlocked = unlocked
                };
            }
        }

        private AchievementView[] CaptureNewAchievements(State next)
        {
            var unlocked = ReadAchievements(next.Progress)
                .Where(view => !view.Unlocked && view.Progress >= view.Target).ToArray();
            foreach (var view in unlocked)
            {
                next.Progress.Unlocked.Add(view.Id);
                view.Unlocked = true;
            }
            return unlocked;
        }
    }
}
