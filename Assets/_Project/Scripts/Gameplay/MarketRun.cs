using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChoDoLa.Gameplay
{
    public sealed class MarketRun
    {
        public sealed class Offer
        {
            public string Id { get; internal set; }
            public string TypeId { get; internal set; }
            public string Name { get; internal set; }
            public int Price { get; internal set; }
            public decimal FakeRate { get; internal set; }
            public string[] Clues { get; internal set; }
        }

        public sealed class BoxView
        {
            public string Id { get; internal set; }
            public string TypeId { get; internal set; }
        }

        public sealed class ItemView
        {
            public string Id { get; internal set; }
            public string TypeId { get; internal set; }
            public string Name { get; internal set; }
            public string Quality { get; internal set; }
            public string Rarity { get; internal set; }
            public string Origin { get; internal set; }
            public int BasePrice { get; internal set; }
            public int ListedPrice { get; internal set; }
            public string[] Signs { get; internal set; }
        }

        public sealed class Result
        {
            public string Code { get; }
            public bool Success => Code == "OK";
            public string EntityId { get; }
            public int Amount { get; }
            public int Penalty { get; }
            public bool? WasFake { get; }

            internal Result(string code = "OK", string entityId = null, int amount = 0, int penalty = 0,
                bool? wasFake = null)
            {
                Code = code;
                EntityId = entityId;
                Amount = amount;
                Penalty = penalty;
                WasFake = wasFake;
            }
        }

        private sealed class Box
        {
            public string Id { get; set; }
            public string TypeId { get; set; }
            public string ItemId { get; set; }
            public string Quality { get; set; }
            public bool Fake { get; set; }
        }

        private sealed class State
        {
            public int Schema { get; set; } = 1;
            public int Seed { get; set; }
            public int Day { get; set; } = 1;
            public int Cash { get; set; }
            public int Bought { get; set; }
            public int Arrears { get; set; }
            public bool StoppedBuying { get; set; }
            public string Status { get; set; } = "active";
            public string EndReason { get; set; }
            public List<Box> Table { get; set; } = new List<Box>();
            public List<Box> Inventory { get; set; } = new List<Box>();
            public Dictionary<string, int> ExtraClues { get; set; } = new Dictionary<string, int>();
            public HashSet<string> AppliedTransactions { get; set; } = new HashSet<string>();
        }

        private readonly JObject content;
        private readonly JToken rules;
        private readonly JToken shelfRules;
        private readonly string[] origins;
        private readonly Dictionary<string, JToken> items;
        private readonly Dictionary<string, JToken> qualities;
        private readonly Action<string> persist;
        private readonly object writeLock = new object();
        private State state;

        public event Action Changed;
        public int Day => state.Day;
        public int Cash => state.Cash;
        public int BoughtToday => state.Bought;
        public int Arrears => state.Arrears;
        public int DueToday => Status == "active" ? ScheduledDebt(Day) + Arrears : 0;
        public int InventoryCapacity => (int)rules["inventory_slots"];
        public int TableCapacity => (int)rules["table_slots"];
        public int DailyLimit => (int)rules["max_boxes_per_day"];
        public int ExtraCluePrice => (int)shelfRules["extra_clue_price"];
        public string Status => state.Status;
        public string EndReason => state.EndReason;
        public IReadOnlyList<BoxView> Table => state.Table.Select(box => new BoxView
        {
            Id = box.Id,
            TypeId = box.TypeId
        }).ToArray();
        public IReadOnlyList<ItemView> Inventory => state.Inventory.Select(ViewItem).ToArray();
        public IReadOnlyList<Offer> Shelf =>
            AvailableBoxes().Select(box => MakeOffer(box).offer).ToArray();

        public MarketRun(string contentJson, int seed, string savedState = null, Action<string> persist = null)
        {
            content = JObject.Parse(contentJson);
            rules = content["rules"];
            shelfRules = content["shelf"];
            origins = shelfRules["price_follows_clue"]["origins_order"].ToObject<string[]>();
            items = content["items"].ToDictionary(item => (string)item["id"]);
            qualities = rules["quality"].ToDictionary(quality => (string)quality["id"]);
            this.persist = persist;
            state = savedState == null
                ? new State { Seed = seed, Cash = (int)rules["start_cash"] }
                : JsonConvert.DeserializeObject<State>(savedState);
            if (state == null || state.Schema != 1 || state.Day < 1 || state.Day > (int)rules["days"])
                throw new InvalidOperationException("Unsupported market run save.");
            if (savedState == null)
                persist?.Invoke(JsonConvert.SerializeObject(state));
        }

        public Result BuyClue(string transactionId, string offerId) => Apply(transactionId, next =>
        {
            var box = FindOffer(offerId);
            if (box == null) return new Result("OFFER_UNAVAILABLE");
            var offer = MakeOffer(box).offer;
            if (offer.Clues.Length >= (int)rules["max_clues_per_box"]) return new Result("CLUE_LIMIT");
            if (next.Cash < ExtraCluePrice) return new Result("NOT_ENOUGH_CASH");
            next.Cash -= ExtraCluePrice;
            next.ExtraClues.TryGetValue(offerId, out var count);
            next.ExtraClues[offerId] = count + 1;
            return new Result(entityId: offerId, amount: ExtraCluePrice);
        });

        public Result BuyBox(string transactionId, string offerId) => Apply(transactionId, next =>
        {
            if (next.Bought >= DailyLimit) return new Result("DAILY_BOX_LIMIT");
            if (next.Table.Count >= TableCapacity) return new Result("TABLE_FULL");
            var definition = FindOffer(offerId);
            if (definition == null) return new Result("OFFER_UNAVAILABLE");
            var (offer, box) = MakeOffer(definition);
            if (next.Cash < offer.Price) return new Result("NOT_ENOUGH_CASH");
            next.Cash -= offer.Price;
            next.Table.Add(box);
            next.Bought++;
            next.ExtraClues.Clear();
            return new Result(entityId: box.Id, amount: offer.Price);
        });

        public Result StopBuying(string transactionId, int expectedDay) => Apply(transactionId, next =>
        {
            if (next.Day != expectedDay) return new Result("STALE_DAY");
            if (next.StoppedBuying) return new Result("BUYING_STOPPED");
            next.StoppedBuying = true;
            next.ExtraClues.Clear();
            return new Result();
        });

        public Result OpenBox(string transactionId, string boxId) => Apply(transactionId, next =>
        {
            var box = next.Table.Find(entry => entry.Id == boxId);
            if (box == null) return new Result("BOX_UNAVAILABLE");
            if (next.Inventory.Count >= InventoryCapacity) return new Result("INVENTORY_FULL");
            next.Table.Remove(box);
            box.Id = "item:" + box.Id;
            next.Inventory.Add(box);
            return new Result(entityId: box.Id);
        });

        public ItemView Inspect(string itemId)
        {
            var item = state.Inventory.Find(entry => entry.Id == itemId);
            return item == null ? null : ViewItem(item);
        }

        public Result Sell(string transactionId, string itemId) => Apply(transactionId, next =>
        {
            var item = next.Inventory.Find(entry => entry.Id == itemId);
            if (item == null) return new Result("ITEM_UNAVAILABLE");
            var fakeFactor = item.Fake ? (decimal)rules["fake_price_factor"] : 1m;
            var price = (int)Math.Floor(PriceBeforeAuthenticity(item) * fakeFactor);
            next.Inventory.Remove(item);
            next.Cash += price;
            return new Result(entityId: itemId, amount: price, wasFake: item.Fake);
        });

        public Result EndDay(string transactionId, int expectedDay) => Apply(transactionId, next =>
        {
            if (next.Day != expectedDay) return new Result("STALE_DAY");
            var due = ScheduledDebt(next.Day) + next.Arrears;
            var paid = Math.Min(next.Cash, due);
            next.Cash -= paid;
            var remaining = due - paid;
            var penalty = (int)Math.Ceiling(remaining * (decimal)rules["penalty_rate"]);
            var arrears = remaining + penalty;
            next.Arrears = arrears;
            if (arrears > (int)rules["lose_arrears_over"])
            {
                next.Status = "lost";
                next.EndReason = "arrears_over_limit";
            }
            else if (next.Day == (int)rules["days"])
            {
                next.Status = arrears == 0 ? "won" : "lost";
                next.EndReason = arrears == 0 ? "all_debt_paid" : "arrears_on_last_day";
            }
            else
            {
                next.Day++;
                next.Bought = 0;
                next.StoppedBuying = false;
                next.ExtraClues.Clear();
            }

            return new Result(amount: paid, penalty: penalty);
        });

        private Result Apply(string transactionId, Func<State, Result> change)
        {
            Result result;
            lock (writeLock)
            {
                if (string.IsNullOrWhiteSpace(transactionId)) return new Result("INVALID_TRANSACTION");
                if (state.AppliedTransactions.Contains(transactionId)) return new Result("DUPLICATE_TRANSACTION");
                if (Status != "active") return new Result("RUN_ENDED");

                var next = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(state));
                result = change(next);
                if (!result.Success) return result;
                next.AppliedTransactions.Add(transactionId);
                persist?.Invoke(JsonConvert.SerializeObject(next));
                state = next;
            }
            Changed?.Invoke();
            return result;
        }

        private int ScheduledDebt(int day) => (int)rules["debt"][day - 1];

        private IEnumerable<JToken> AvailableBoxes()
        {
            if (Status != "active" || BoughtToday >= DailyLimit || state.StoppedBuying)
                return Enumerable.Empty<JToken>();
            return content["boxes"].Where(box => (string)box["id"] != "RD" || Day >= (int)rules["red_chest_from_day"]);
        }

        private string OfferId(string typeId) => FormattableString.Invariant($"{Day}:{BoughtToday + 1}:{typeId}");

        private JToken FindOffer(string offerId) =>
            AvailableBoxes().FirstOrDefault(box => OfferId((string)box["id"]) == offerId);

        private (Offer offer, Box box) MakeOffer(JToken definition)
        {
            var typeId = (string)definition["id"];
            var id = OfferId(typeId);
            using var rng = new OfferRandom(state.Seed, id);
            var itemId = (string)PickWeighted(definition["drops"], "weight", rng)["item"];
            var quality = (string)PickWeighted(rules["quality"], "p", rng)["id"];
            var fakeRate = (decimal)definition["fake_rate"];
            var itemFakeFloor = content["item_fake_rate_floor"][itemId]?.Value<decimal>() ?? 0;
            var box = new Box
            {
                Id = id,
                TypeId = typeId,
                ItemId = itemId,
                Quality = quality,
                Fake = rng.Next() < Math.Max(fakeRate, itemFakeFloor)
            };

            var origin = (string)items[itemId]["origin"];
            var otherOrigins = origins.Where(value => value != origin).ToArray();
            var clueOrigins = new string[(int)content["clues"]["per_box"]];
            for (var i = 0; i < clueOrigins.Length; i++)
                clueOrigins[i] = i < (int)definition["clue_true_count"]
                    ? origin
                    : otherOrigins[rng.Index(otherOrigins.Length)];
            for (var i = clueOrigins.Length - 1; i > 0; i--)
            {
                var j = rng.Index(i + 1);
                (clueOrigins[i], clueOrigins[j]) = (clueOrigins[j], clueOrigins[i]);
            }
            var usedText = new HashSet<string>();
            var clues = clueOrigins.Select(group =>
            {
                var vocabulary = content["clue_vocabulary"][group].ToObject<string[]>();
                var unused = vocabulary.Where(text => !usedText.Contains(text)).ToArray();
                var choices = unused.Length > 0 ? unused : vocabulary;
                var text = choices[rng.Index(choices.Length)];
                usedText.Add(text);
                return text;
            }).ToArray();

            state.ExtraClues.TryGetValue(id, out var extraClues);
            var price = shelfRules["price_follows_clue"]["tag_price"][typeId][Array.IndexOf(origins, clueOrigins[0])];
            var offer = new Offer
            {
                Id = id,
                TypeId = typeId,
                Name = (string)definition["name"],
                FakeRate = fakeRate,
                Price = (int)price,
                Clues = clues.Take((int)shelfRules["free_clues_base"] + extraClues).ToArray()
            };
            return (offer, box);
        }

        private ItemView ViewItem(Box entry)
        {
            var item = items[entry.ItemId];
            var quality = qualities[entry.Quality];
            var basePrice = (int)item["base_price"];
            return new ItemView
            {
                Id = entry.Id,
                TypeId = entry.ItemId,
                Name = (string)item["name"],
                Quality = (string)quality["name"],
                Rarity = (string)item["rarity"],
                Origin = (string)item["origin"],
                BasePrice = basePrice,
                ListedPrice = (int)Math.Floor(PriceBeforeAuthenticity(entry)),
                Signs = item["inspect_signs"].ToObject<string[]>()
            };
        }

        private decimal PriceBeforeAuthenticity(Box entry) =>
            (int)items[entry.ItemId]["base_price"] * (decimal)qualities[entry.Quality]["price_mult"];

        private static JToken PickWeighted(JToken entries, string field, OfferRandom rng)
        {
            var target = rng.Next() * entries.Sum(entry => (decimal)entry[field]);
            foreach (var entry in entries)
            {
                target -= (decimal)entry[field];
                if (target < 0) return entry;
            }
            throw new InvalidOperationException("Invalid weighted content table.");
        }

        private sealed class OfferRandom : IDisposable
        {
            private readonly SHA256 hash = SHA256.Create();
            private readonly string key;
            private int draw;

            public OfferRandom(int seed, string id)
            {
                key = seed.ToString(CultureInfo.InvariantCulture) + ":" + id + ":";
            }

            public decimal Next()
            {
                var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(key + (draw++).ToString(CultureInfo.InvariantCulture)));
                var value = ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
                return value / 4294967296m;
            }

            public int Index(int count) => (int)(Next() * count);
            public void Dispose() => hash.Dispose();
        }
    }
}
