using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ChoDoLa.Gameplay
{
    public sealed partial class MarketRun
    {
        public sealed class TactView
        {
            public string Id { get; internal set; }
            public string Name { get; internal set; }
            public string System { get; internal set; }
            public int Tier { get; internal set; }
            public int Price { get; internal set; }
            public bool Owned { get; internal set; }
        }

        public sealed class TactEffect
        {
            public string Source { get; internal set; }
            public string Type { get; internal set; }
            public decimal Value { get; internal set; }
            public int Count { get; internal set; }
            public IReadOnlyList<string> Origins { get; internal set; }
            public bool RestoredOnly { get; internal set; }
            public bool Unlimited { get; internal set; }
        }

        public sealed class TactCombo
        {
            public string Id { get; internal set; }
            public string Name { get; internal set; }
            public IReadOnlyList<string> RequiredIds { get; internal set; }
            public IReadOnlyList<string> MissingIds { get; internal set; }
            public IReadOnlyList<TactEffect> Effects { get; internal set; }
        }

        public IReadOnlyList<TactCombo> TactCombos => ComboDefinitions.Select(combo =>
        {
            var id = (string)combo["id"];
            var required = combo["requires"].Values<string>().ToArray();
            return new TactCombo
            {
                Id = id,
                Name = (string)combo["name"],
                RequiredIds = required,
                MissingIds = required.Where(tactId => !state.TactSlots.ContainsValue(tactId)).ToArray(),
                Effects = combo["effects"].Select(effect => ViewEffect(id, effect)).ToArray()
            };
        }).ToArray();

        private JToken TactDefinitions => content["artifacts"];
        private JToken ComboDefinitions => content["combos"];
        public bool Bikhe1Unlocked => state.Bikhe1Unlocked;
        public int EquipSlotCount => SlotCount(state);
        public IReadOnlyList<string> EquippedTacts => Enumerable.Range(0, EquipSlotCount)
            .Select(slot => state.TactSlots.TryGetValue(slot, out var id) ? id : null).ToArray();
        public IReadOnlyList<TactView> TactCatalog => TactDefinitions.Select(item => new TactView
        {
            Id = (string)item["id"],
            Name = (string)item["name"],
            System = (string)item["set"],
            Tier = (int)item["tier"],
            Price = (int)item["price"],
            Owned = state.OwnedTacts.Contains((string)item["id"])
        }).ToArray();
        public IReadOnlyDictionary<string, int> Resonance => GetResonance(state);
        public IReadOnlyList<string> ActiveCombos => ActiveComboDefinitions(state).Select(combo => (string)combo["id"]).ToArray();
        public IReadOnlyList<TactEffect> ActiveTactEffects => Effects(state).ToArray();

        public Result BuyTact(string transactionId, string tactId) => Apply(transactionId, next =>
        {
            var definition = TactDefinitions.FirstOrDefault(item => (string)item["id"] == tactId);
            if (definition == null) return new Result("TACT_UNAVAILABLE");
            if (next.OwnedTacts.Contains(tactId)) return new Result("TACT_ALREADY_OWNED");

            var price = (int)definition["price"];
            if (next.Cash < price) return new Result("NOT_ENOUGH_CASH");
            next.Cash -= price;
            next.OwnedTacts.Add(tactId);
            return new Result(entityId: tactId, amount: price);
        });

        public Result EquipTact(string transactionId, string tactId, int slot) => Apply(transactionId, next =>
        {
            if (slot < 0 || slot >= SlotCount(next)) return new Result("INVALID_SLOT");
            if (!next.OwnedTacts.Contains(tactId)) return new Result("TACT_NOT_OWNED");

            foreach (int previous in next.TactSlots.Where(pair => pair.Value == tactId).Select(pair => pair.Key).ToArray())
                next.TactSlots.Remove(previous);
            next.TactSlots[slot] = tactId;

            var condition = content["bikhe"].First(entry => (string)entry["id"] == "BK1")["condition"];
            if (!next.Bikhe1Unlocked && next.Day < (int)condition["before_day"] &&
                ActiveComboDefinitions(next).Any(combo => (string)combo["size"] == "small"))
                next.Bikhe1Unlocked = true;

            return new Result(entityId: tactId);
        });

        public Result UnequipTact(string transactionId, int slot) => Apply(transactionId, next =>
        {
            if (slot < 0 || slot >= SlotCount(next)) return new Result("INVALID_SLOT");
            if (!next.TactSlots.TryGetValue(slot, out var id)) return new Result("SLOT_EMPTY");
            next.TactSlots.Remove(slot);
            return new Result(entityId: id);
        });

        private int SlotCount(State current) =>
            (int)rules[current.Bikhe1Unlocked ? "equip_slots_with_bk1" : "equip_slots"];

        private bool ValidTactSave(State saved)
        {
            if (saved.OwnedTacts == null || saved.TactSlots == null) return false;
            var ids = TactDefinitions.Select(item => (string)item["id"]).ToHashSet();
            return saved.OwnedTacts.All(ids.Contains) &&
                saved.TactSlots.All(pair => pair.Key >= 0 && pair.Key < SlotCount(saved) &&
                    saved.OwnedTacts.Contains(pair.Value)) &&
                saved.TactSlots.Values.Distinct().Count() == saved.TactSlots.Count;
        }

        private IEnumerable<JToken> EquippedDefinitions(State current) =>
            TactDefinitions.Where(item => current.TactSlots.ContainsValue((string)item["id"]));

        private Dictionary<string, int> GetResonance(State current) => EquippedDefinitions(current)
            .GroupBy(item => (string)item["set"])
            .Where(set => set.Count() >= 2)
            .ToDictionary(set => set.Key, set => set.Count());

        private IEnumerable<JToken> ActiveComboDefinitions(State current)
        {
            var equipped = current.TactSlots.Values.ToHashSet();
            return ComboDefinitions.Where(combo => combo["requires"].Values<string>().All(equipped.Contains));
        }

        private IEnumerable<TactEffect> Effects(State current)
        {
            var definitions = EquippedDefinitions(current).Concat(ActiveComboDefinitions(current));
            foreach (var definition in definitions)
                foreach (var effect in definition["effects"])
                    yield return ViewEffect((string)definition["id"], effect);

            var resonance = content["resonance"];
            foreach (var set in GetResonance(current))
            {
                var tier = set.Value >= 3 ? "three_of_a_set" : "two_of_a_set";
                foreach (var effect in resonance[tier][set.Key])
                    yield return ViewEffect(set.Key, effect);
            }
        }

        private static TactEffect ViewEffect(string source, JToken effect) => new TactEffect
        {
            Source = source,
            Type = (string)effect["type"],
            Value = (decimal?)effect["value"] ?? 0m,
            Count = (int?)effect["count"] ?? 0,
            Origins = effect["origins"]?.ToObject<string[]>() ?? Array.Empty<string>(),
            RestoredOnly = (bool?)effect["restored_only"] ?? false,
            Unlimited = (bool?)effect["unlimited"] ?? false
        };

        private decimal TactSellBonus(string origin) => Effects(state)
            .Where(effect => effect.Type == "sell_bonus" && !effect.RestoredOnly &&
                (effect.Origins.Count == 0 || effect.Origins.Contains(origin)))
            .Sum(effect => effect.Value);

        private int FreeClueCount(string boxType) => Math.Min((int)rules["max_clues_per_box"],
            (int)shelfRules["free_clues_base"] + HeirloomFreeClues(boxType) + Effects(state)
                .Where(effect => effect.Type == "extra_free_clue").Sum(effect => effect.Count));
    }
}
