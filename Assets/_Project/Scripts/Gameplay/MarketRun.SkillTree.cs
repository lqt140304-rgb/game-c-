using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChoDoLa.Gameplay
{
    public sealed partial class MarketRun
    {
        public sealed class SkillNode
        {
            public string Id { get; internal set; }
            public string Name { get; internal set; }
            public string Branch { get; internal set; }
            public string Parent { get; internal set; }
            public int Cost { get; internal set; }
            public string Condition { get; internal set; }
            public int Progress { get; internal set; }
            public int Target { get; internal set; }
            public string Reward { get; internal set; }
            public bool Unlocked { get; internal set; }
            public bool Available { get; internal set; }
        }

        private sealed class Development
        {
            public int Points { get; set; }
            public int Wins { get; set; }
            public HashSet<string> Collection { get; set; } = new HashSet<string>();
            public HashSet<string> Unlocked { get; set; } = new HashSet<string>();
        }

        private JToken Tree => content["skill_tree"];
        public int DevelopmentPoints => state.Meta.Points;
        public int RunsWon => state.Meta.Wins;
        public IReadOnlyList<string> CollectedItems => state.Meta.Collection
            .Select(id => items.TryGetValue(id, out var item) ? (string)item["name"] : id)
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();
        public bool CanStartWithEarlyBlackChest => state.Meta.Unlocked.Contains("M3");
        public IReadOnlyList<SkillNode> SkillNodes => Tree["nodes"].Select(definition =>
        {
            var condition = definition["condition"];
            var node = new SkillNode
            {
                Id = (string)definition["id"],
                Name = (string)definition["name"],
                Branch = (string)definition["branch"],
                Parent = (string)definition["parent"],
                Cost = (int)definition["cost"],
                Condition = (string)condition["label"],
                Progress = ConditionProgress(state.Meta, condition),
                Target = (int)condition["target"],
                Reward = (string)definition["reward"]
            };
            node.Unlocked = state.Meta.Unlocked.Contains(node.Id);
            node.Available = Status != "active" && !node.Unlocked &&
                (node.Parent == null || state.Meta.Unlocked.Contains(node.Parent)) &&
                node.Progress >= node.Target && DevelopmentPoints >= node.Cost;
            return node;
        }).ToArray();

        public Result RecordInspection(string transactionId, string itemId) => Apply(transactionId, next =>
        {
            var item = next.Inventory.Find(entry => entry.Id == itemId);
            if (item == null) return new Result("ITEM_UNAVAILABLE");
            var earned = next.Meta.Collection.Add(item.ItemId) ? (int)Tree["first_inspection_points"] : 0;
            next.Meta.Points += earned;
            return new Result(entityId: itemId, amount: earned);
        });

        public Result UnlockSkill(string transactionId, string skillId) => Apply(transactionId, next =>
        {
            var node = SkillNodes.FirstOrDefault(entry => entry.Id == skillId);
            if (node == null) return new Result("SKILL_UNAVAILABLE");
            if (node.Unlocked) return new Result("SKILL_UNLOCKED");
            if (node.Parent != null && !next.Meta.Unlocked.Contains(node.Parent)) return new Result("PARENT_LOCKED");
            if (node.Progress < node.Target) return new Result("CONDITION_NOT_MET");
            if (next.Meta.Points < node.Cost) return new Result("NOT_ENOUGH_POINTS");
            next.Meta.Points -= node.Cost;
            next.Meta.Unlocked.Add(skillId);
            return new Result(entityId: skillId, amount: node.Cost);
        }, betweenRuns: true);

        public bool StartNextRun(int seed, bool earlyBlackChest = false)
        {
            lock (writeLock)
            {
                if (Status == "active" || (earlyBlackChest && !CanStartWithEarlyBlackChest)) return false;
                var next = new State
                {
                    Seed = seed,
                    Cash = (int)rules["start_cash"],
                    Meta = state.Meta,
                    EarlyBlackChest = earlyBlackChest
                };
                persist?.Invoke(JsonConvert.SerializeObject(next));
                state = next;
            }
            Changed?.Invoke();
            return true;
        }

        private int ConditionProgress(Development meta, JToken condition) => (string)condition["kind"] switch
        {
            "wins" => meta.Wins,
            "collection" => meta.Collection.Count,
            "origin" => meta.Collection.Count(id => items.TryGetValue(id, out var item) &&
                (string)item["origin"] == (string)condition["origin"]),
            _ => throw new InvalidOperationException("Unknown skill tree condition.")
        };

        private void AwardRunDevelopment(State next)
        {
            if (next.Status == "won")
            {
                next.Meta.Wins++;
                next.Meta.Points += (int)Tree["win_points"];
            }
            else if (next.Status == "lost")
            {
                next.Meta.Points += (int)Tree["loss_points"];
            }
        }
    }
}
