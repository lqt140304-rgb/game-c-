using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoDoLa.Core
{
    /// <summary>
    /// Lõi gameplay vòng chơi 1 ngày, không phụ thuộc Unity. Cầu nối Unity chỉ gọi lệnh và đọc view/sự kiện.
    ///
    /// Nguyên tắc:
    ///  - Lệnh bị từ chối KHÔNG đổi trạng thái và trả mã lý do (xem <see cref="Reject"/>).
    ///  - Lệnh tiền (BuyClue, BuyBox, SellItem, EndDay) nhận mã giao dịch; mã đã áp dụng thì bị từ chối,
    ///    mã chỉ ghi nhận khi lệnh thành công. EndDay còn nhận ngày dự kiến để chặn bấm đúp (lỗi D-01).
    ///  - Mọi kết quả ngẫu nhiên xác định theo khóa (seed, ngày, lượt, loại hộp), không dùng bộ sinh chung.
    ///  - Mọi con số lấy từ <see cref="ContentData"/>.
    /// </summary>
    public sealed class RunSession
    {
        private readonly ContentData _c;
        private readonly RunState _s;
        private readonly List<GameEvent> _events = new List<GameEvent>();

        /// <summary>Bắt đầu ván mới: tiền đầu ván, ngày 1, công bố giá hôm nay và bày kệ lượt 1.</summary>
        public RunSession(ContentData content, long seed)
        {
            _c = content;
            _s = new RunState { Seed = seed, Cash = content.Rules.StartCash };
            StartDay();
        }

        /// <summary>Khôi phục từ bản lưu. Không phát lại sự kiện, không đổi gì trong state.</summary>
        public RunSession(ContentData content, RunState restored)
        {
            _c = content;
            _s = restored;
        }

        public RunState State => _s;

        // ===== Đọc =====

        public RunSnapshot Snapshot() => new RunSnapshot
        {
            Day = _s.Day,
            Cash = _s.Cash,
            Arrears = _s.Arrears,
            DebtDueToday = _s.Status == RunStatus.Playing ? DebtDue() : 0,
            BoxesBoughtToday = _s.BoxesBoughtToday,
            MaxDailyBoxes = _c.Rules.MaxBoxesPerDay,
            ShelfTurn = _s.Shelf.Count > 0 ? _s.BoxesBoughtToday + 1 : 0,
            Status = _s.Status,
            LoseReason = _s.LoseReason,
            TodayPrice = new Dictionary<string, int>(_s.TodayPrice),
        };

        public IReadOnlyList<BoxOfferView> Offers() => _s.Shelf.Select(ToOffer).ToList();
        public IReadOnlyList<BoxOfferView> TableBoxes() => _s.Table.Select(ToOffer).ToList();
        public IReadOnlyList<ItemView> Inventory() => _s.Inventory.Select(ToItemView).ToList();

        /// <summary>Lấy và xóa hàng đợi sự kiện, theo thứ tự xảy ra.</summary>
        public IReadOnlyList<GameEvent> DrainEvents()
        {
            var copy = _events.ToArray();
            _events.Clear();
            return copy;
        }

        // ===== Lệnh =====

        /// <summary>Lời mách của Dì Mận: trả phí để biết chắc nhóm nguồn gốc thật. Một lần mỗi hộp, giá niêm yết không đổi.</summary>
        public CommandResult BuyClue(string offerId, string txId)
        {
            var gate = Gate(txId);
            if (!gate.Ok) return gate;
            var box = _s.Shelf.Find(b => b.BoxId == offerId);
            if (box == null) return Rejected(Reject.OfferNotFound);
            if (box.CertainOriginBought) return Rejected(Reject.ClueAlreadyBought);
            int fee = _c.Shelf.ExtraCluePrice;
            if (_s.Cash < fee) return Rejected(Reject.NotEnoughCash);

            _s.Cash -= fee;
            box.CertainOriginBought = true;
            Commit(txId);
            Emit(GameEventKind.ClueBought, offerId, fee, _c.Item(box.ItemId).Origin);
            return CommandResult.Success();
        }

        public CommandResult BuyBox(string offerId, string txId)
        {
            var gate = Gate(txId);
            if (!gate.Ok) return gate;
            if (_s.StoppedBuyingToday) return Rejected(Reject.StoppedBuying);
            if (_s.BoxesBoughtToday >= _c.Rules.MaxBoxesPerDay) return Rejected(Reject.DailyBoxLimit);
            var box = _s.Shelf.Find(b => b.BoxId == offerId);
            if (box == null) return Rejected(Reject.OfferNotFound);
            if (_s.Table.Count >= _c.Rules.TableSlots) return Rejected(Reject.TableFull);
            if (_s.Cash < box.TagPrice) return Rejected(Reject.NotEnoughCash);

            _s.Cash -= box.TagPrice;
            _s.BoxesBoughtToday++;
            _s.Table.Add(box);
            _s.Shelf.Clear();               // hộp bị bỏ qua không quay lại
            Commit(txId);
            Emit(GameEventKind.BoxBought, box.BoxId, box.TagPrice, box.TypeId);
            GenerateShelf();                // lượt kế tiếp (nếu còn)
            return CommandResult.Success();
        }

        /// <summary>Dừng mua trong ngày. Không phải lệnh tiền nên không cần mã giao dịch.</summary>
        public CommandResult StopBuying()
        {
            if (_s.Status != RunStatus.Playing) return Rejected(Reject.RunEnded);
            _s.StoppedBuyingToday = true;
            _s.Shelf.Clear();
            Emit(GameEventKind.BuyingStopped, null, 0, null);
            return CommandResult.Success();
        }

        /// <summary>Mở hộp trên bàn: hiện vật vào kho. Kho đầy thì hộp ở lại bàn, không mất.</summary>
        public CommandResult OpenBox(string boxId)
        {
            if (_s.Status != RunStatus.Playing) return Rejected(Reject.RunEnded);
            var box = _s.Table.Find(b => b.BoxId == boxId);
            if (box == null) return Rejected(Reject.BoxAlreadyOpened);
            if (_s.Inventory.Count >= _c.Rules.InventorySlots) return Rejected(Reject.InventoryFull);

            _s.Table.Remove(box);
            var item = new ItemInstance
            {
                InstanceId = "I" + _s.NextItemNumber++,
                ItemId = box.ItemId,
                QualityId = box.QualityId,
                IsFake = box.IsFake,
            };
            _s.Inventory.Add(item);
            Emit(GameEventKind.BoxOpened, item.InstanceId, 0, item.ItemId);
            return CommandResult.Success();
        }

        public CommandResult SellItem(string instanceId, string txId)
        {
            var gate = Gate(txId);
            if (!gate.Ok) return gate;
            var item = _s.Inventory.Find(i => i.InstanceId == instanceId);
            if (item == null) return Rejected(Reject.ItemNotFound);

            int price = Pricing.SellPrice(_c, item.ItemId, item.QualityId, item.IsFake);
            _s.Inventory.Remove(item);
            _s.Cash += price;
            Commit(txId);
            Emit(GameEventKind.ItemSold, instanceId, price, null);
            return CommandResult.Success();
        }

        /// <summary>
        /// Kết thúc ngày và thu nợ. <paramref name="forDay"/> là ngày UI đang hiển thị: nếu đã sang ngày khác
        /// (bấm đúp) thì bị từ chối STALE_DAY, không thu thêm kỳ của ngày sau.
        /// </summary>
        public CommandResult EndDay(string txId, int forDay)
        {
            var gate = Gate(txId);
            if (!gate.Ok) return gate;
            if (forDay != _s.Day) return Rejected(Reject.StaleDay);

            int due = DebtDue();
            int pay = Math.Min(Math.Max(_s.Cash, 0), due);
            _s.Cash -= pay;
            int remaining = due - pay;
            _s.Arrears = remaining > 0 ? Pricing.ApplyPenalty(remaining, _c.Rules.PenaltyRate) : 0;
            if (_s.Arrears == 0) _s.PaidDays.Add(_s.Day);
            Commit(txId);

            string outcome;
            if (_s.Arrears > _c.Rules.LoseArrearsOver)
            {
                _s.Status = RunStatus.Lost; _s.LoseReason = "arrears_over_limit"; outcome = "lost";
            }
            else if (_s.Day == _c.Rules.Days)
            {
                if (_s.Arrears == 0) { _s.Status = RunStatus.Won; outcome = "won"; }
                else { _s.Status = RunStatus.Lost; _s.LoseReason = "arrears_on_day7"; outcome = "lost"; }
            }
            else outcome = "next_day";

            Emit(GameEventKind.DebtSettled, null, pay, outcome);
            if (_s.Status != RunStatus.Playing)
            {
                _s.Shelf.Clear();
                Emit(GameEventKind.RunEnded, null, _s.Cash, _s.Status == RunStatus.Won ? "won" : _s.LoseReason);
                return CommandResult.Success();
            }
            _s.Day++;
            StartDay();
            return CommandResult.Success();
        }

        // ===== Nội bộ =====

        private int DebtDue() => _c.Rules.Debt[_s.Day - 1] + _s.Arrears;

        private IEnumerable<BoxDef> AvailableBoxes() =>
            _c.Boxes.Where(b => b.Id != "RD" || _s.Day >= _c.Rules.RedChestFromDay);

        private void StartDay()
        {
            _s.BoxesBoughtToday = 0;
            _s.StoppedBuyingToday = false;
            _s.TodayPrice.Clear();
            for (int i = 0; i < _c.Boxes.Length; i++)
            {
                var b = _c.Boxes[i];
                if (b.Id == "RD" && _s.Day < _c.Rules.RedChestFromDay) continue;
                var rng = new DeterministicRng(_s.Seed, _s.Day, i, 1);   // khóa (seed, ngày, loại hộp)
                _s.TodayPrice[b.Id] = Pricing.TodayPrice(_c, b, rng.NextDouble());
            }
            Emit(GameEventKind.DayStarted, null, _s.Day, null);
            GenerateShelf();
        }

        private void GenerateShelf()
        {
            _s.Shelf.Clear();
            if (_s.StoppedBuyingToday || _s.BoxesBoughtToday >= _c.Rules.MaxBoxesPerDay) return;
            int turn = _s.BoxesBoughtToday + 1;
            for (int i = 0; i < _c.Boxes.Length; i++)
            {
                var def = _c.Boxes[i];
                if (def.Id == "RD" && _s.Day < _c.Rules.RedChestFromDay) continue;
                _s.Shelf.Add(MakeBox(def, i, turn));
            }
        }

        private BoxInstance MakeBox(BoxDef def, int typeIndex, int turn)
        {
            var rng = new DeterministicRng(_s.Seed, _s.Day, turn, typeIndex, 2);   // khóa (seed, ngày, lượt, loại hộp)
            string itemId = def.Drops[rng.PickWeighted(def.Drops.Select(d => d.Weight).ToArray())].Item;
            var item = _c.Item(itemId);
            string qualityId = _c.Rules.Quality[rng.PickWeighted(_c.Rules.Quality.Select(q => q.P).ToArray())].Id;

            double fakeRate = def.FakeRate;
            if (_c.ItemFakeRateFloor != null && _c.ItemFakeRateFloor.TryGetValue(itemId, out double floor))
                fakeRate = Math.Max(fakeRate, floor);
            bool fake = rng.NextDouble() < fakeRate;

            // Manh mối: clue_true_count cái đúng, phần còn lại sai (đều trong các nhóm khác), rồi xáo thứ tự.
            int n = _c.Clues.PerBox;
            var others = _c.Origins.Select(o => o.Id).Where(id => id != item.Origin).ToArray();
            var origins = new string[n];
            for (int k = 0; k < n; k++)
                origins[k] = k < def.ClueTrueCount ? item.Origin : others[rng.NextInt(others.Length)];
            rng.Shuffle(origins);
            var texts = origins.Select(o =>
            {
                var vocab = _c.ClueVocabulary[o];
                return vocab[rng.NextInt(vocab.Length)];
            }).ToArray();

            int today = _s.TodayPrice[def.Id];
            return new BoxInstance
            {
                BoxId = $"B{_s.Day}-{turn}-{def.Id}",
                TypeId = def.Id,
                TagPrice = Pricing.TagPrice(_c, def.Id, today, origins[0]),
                ItemId = itemId,
                QualityId = qualityId,
                IsFake = fake,
                ClueOrigins = origins,
                ClueTexts = texts,
            };
        }

        private BoxOfferView ToOffer(BoxInstance b)
        {
            var def = _c.Box(b.TypeId);
            return new BoxOfferView
            {
                OfferId = b.BoxId,
                TypeId = b.TypeId,
                DisplayName = def.Name,
                TodayPrice = _s.TodayPrice.TryGetValue(b.TypeId, out int p) ? p : def.Price,
                TagPrice = b.TagPrice,
                FakeRate = def.FakeRate,
                FreeClueOrigin = b.ClueOrigins[0],
                FreeClueText = b.ClueTexts[0],
                CertainOriginBought = b.CertainOriginBought,
                CertainOrigin = b.CertainOriginBought ? _c.Item(b.ItemId).Origin : null,
            };
        }

        private ItemView ToItemView(ItemInstance i)
        {
            var def = _c.Item(i.ItemId);
            return new ItemView
            {
                InstanceId = i.InstanceId,
                ItemId = i.ItemId,
                DisplayName = def.Name,
                OriginId = def.Origin,
                QualityId = i.QualityId,
                InspectSigns = def.InspectSigns,
                ListedPrice = Pricing.ListedPrice(_c, i.ItemId, i.QualityId),
            };
        }

        private CommandResult Gate(string txId)
        {
            if (_s.Status != RunStatus.Playing) return Rejected(Reject.RunEnded);
            if (string.IsNullOrEmpty(txId)) throw new ArgumentException("txId bắt buộc cho lệnh tiền", nameof(txId));
            if (_s.AppliedTx.Contains(txId)) return Rejected(Reject.DuplicateTransaction);
            return CommandResult.Success();
        }

        private void Commit(string txId) => _s.AppliedTx.Add(txId);

        private CommandResult Rejected(string reason)
        {
            Emit(GameEventKind.ActionRejected, reason, 0, null);
            return CommandResult.Fail(reason);
        }

        private void Emit(GameEventKind kind, string id, int amount, string detail) =>
            _events.Add(new GameEvent { Kind = kind, Id = id, Amount = amount, Detail = detail });
    }
}
