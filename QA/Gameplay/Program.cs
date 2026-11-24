using System;
using System.IO;
using System.Linq;
using ChoDoLa.Gameplay;
using Newtonsoft.Json.Linq;

var originalJson = File.ReadAllText(args.Single());
var baseline = JObject.Parse(originalJson);
baseline["heirlooms"] = new JArray(baseline["heirlooms"][0].DeepClone());
var json = baseline.ToString();
string save = null;
var run = new MarketRun(json, 123, null, data => save = data);
void Check(bool condition, string label)
{
    if (!condition) throw new Exception(label);
}

var first = run.Shelf.Single(box => box.TypeId == "H1");
Check(first.Price == first.TodayPrice && first.TodayPrice >= 80 && first.TodayPrice <= 120 && first.Clues.Length == 1, "initial offer");
Check(run.BuyClue("clue-1", first.Id).Success, "buy clue");
Check(run.Cash == 985 && run.Shelf[0].Clues.SequenceEqual(first.Clues), "paid tip leaves free clues unchanged");
Check(run.BuyClue("clue-again", first.Id).Code == "CLUE_LIMIT" && run.Cash == 985, "one paid tip per box");
run = new MarketRun(json, 999, save, data => save = data);
var tipped = run.Shelf.Single(box => box.Id == first.Id);
Check(tipped.CertainOrigin == "qua_khu" && tipped.Price == first.Price, "paid tip survives reload without repricing");
Check(run.TodayPrices["H1"] == first.TodayPrice, "today price reloads unchanged");
var boxId = run.BuyBox("buy-1", first.Id).EntityId;
Check(run.Cash == 985 - first.Price && run.Table.Count == 1, "buy");
Check(!run.BuyBox("buy-1", run.Shelf[0].Id).Success && run.Cash == 985 - first.Price, "replay");
run = new MarketRun(json, 999, save, data => save = data);
Check(run.Table.Single().Id == boxId, "load bought box");
var item = run.OpenBox("open-1", boxId).EntityId;
Check(run.Table.Count == 0 && run.Inventory.Single().Id == item, "open transfers to inventory");
Check(!run.OpenBox("open-2", boxId).Success, "duplicate open");
Check(run.Inspect(item).Signs.Length > 0 && run.Inspect(item).Origin == tipped.CertainOrigin, "paid tip matches revealed origin");
var sale = run.Sell("sell-1", item);
Check(sale.Success && run.Cash == 985 - first.Price + sale.Amount && run.Inventory.Count == 0, "sell");
Check(!run.Sell("sell-2", item).Success, "duplicate sale");
for (var i = 2; i <= 3; i++)
    Check(run.BuyBox("buy-" + i, run.Shelf[0].Id).Success, "buy remaining offer");
Check(run.BuyBox("buy-4", first.Id).Code == "DAILY_BOX_LIMIT", "fourth box");
Check(run.EndDay("day-1", 1).Success && run.Day == 2 && run.BoughtToday == 0, "day transition");
Check(!run.EndDay("day-1-double", 1).Success && run.Day == 2, "double click with new token");
Check(run.BuyBox("buy-day2", run.Shelf[0].Id).Success && run.Table.Count == 3, "table persists");
Check(run.BuyBox("buy-full", run.Shelf[0].Id).Code == "TABLE_FULL", "table capacity");

var tippedOrigins = new System.Collections.Generic.HashSet<string>();
for (var seed = 0; seed < 20; seed++)
{
    var market = new MarketRun(json, seed);
    var offer = market.Shelf.Single(box => box.TypeId == "H2");
    Check(offer.CertainOrigin == null, "unpaid offer hides the certain origin");
    Check(market.BuyClue("tip", offer.Id).Success, "buy a tip for a mixed-origin box");
    var tip = market.Shelf.Single(box => box.Id == offer.Id).CertainOrigin;
    var bought = market.BuyBox("buy", offer.Id);
    var opened = market.OpenBox("open", bought.EntityId);
    Check(market.Inspect(opened.EntityId).Origin == tip, "mixed-origin tip matches the actual item");
    tippedOrigins.Add(tip);
}
Check(tippedOrigins.Count > 1, "mixed-origin test covers distinct origins");

var data = JObject.Parse(json);
var testedMultiplier = false;
for (var seed = 0; seed < 20; seed++)
{
    var market = new MarketRun(json, seed);
    var offer = market.Shelf.Single(box => box.TypeId == "H2");
    var clueOrigin = ((JObject)data["clue_vocabulary"]).Properties()
        .Single(group => group.Value.Values<string>().Contains(offer.Clues[0])).Name;
    var priceRules = data["shelf"]["price_follows_clue"];
    var index = Array.IndexOf(priceRules["origins_order"].ToObject<string[]>(), clueOrigin);
    var multiplier = (decimal)priceRules["multiplier"]["H2"][index];
    testedMultiplier |= multiplier != 1;
    Check(offer.Price == (int)Math.Round(offer.TodayPrice * multiplier), "tag price uses multiplier");
    Check(market.StopBuying("stop", 1).Success && market.Shelf.Count == 0 &&
        market.TodayPrices["H2"] == offer.TodayPrice, "today prices remain available after stopping");
}
Check(testedMultiplier, "test covers a non-unit clue multiplier");

var e3Data = JObject.Parse(json);
e3Data["events"] = new JArray(e3Data["events"].Single(item => (string)item["id"] == "E3").DeepClone());
e3Data["event_schedule"]["probability"] = 1;
e3Data["shelf"]["day_price"]["swing_amplitude"] = 0;
var discounted = new MarketRun(e3Data.ToString(), 123);
Check(discounted.TodayPrices["H2"] == 350, "E3 cannot activate on day one");
Check(discounted.EndDay("day", 1).Success && discounted.TodayPrices["H2"] == 280 &&
    discounted.TodayPrices["H1"] == 100, "E3 applies to the H2 day price before clues");

var debtData = JObject.Parse(json);
debtData["rules"]["start_cash"] = 150;
var debt = new MarketRun(debtData.ToString(), 1);
Check(debt.EndDay("d1", 1).Success && debt.Arrears == 60, "partial debt");
Check(debt.EndDay("d2", 2).Success && debt.Arrears == 492, "include previous penalties in the remaining debt");
Check(debt.EndDay("d3", 3).Success && debt.Status == "lost", "arrears threshold");

debtData["rules"]["start_cash"] = 100000;
var rich = new MarketRun(debtData.ToString(), 1);
for (var day = 1; day <= 7; day++) Check(rich.EndDay("rich-" + day, day).Success, "seven settlements");
Check(rich.Status == "won" && rich.Day == 7, "win");
var full = new MarketRun(debtData.ToString(), 123);
for (var day = 1; day <= 3; day++)
{
    for (var turn = 1; turn <= 3; turn++)
    {
        var id = full.BuyBox($"full-buy-{day}-{turn}", full.Shelf[0].Id).EntityId;
        var opened = full.OpenBox($"full-open-{day}-{turn}", id);
        if (day == 3 && turn == 3)
            Check(opened.Code == "INVENTORY_FULL" && full.Table.Single().Id == id, "full inventory preserves box");
        else
            Check(opened.Success, "fill inventory");
    }
    Check(full.EndDay($"full-{day}", day).Success, "continue filling inventory");
}
Check(full.Shelf.Any(offer => offer.TypeId == "RD"), "black chest unlocks day four");
Check(full.Sell("full-sell", full.Inventory[0].Id).Success, "make inventory space");
Check(full.OpenBox("full-open-3-3", full.Table[0].Id).Success, "retry previously rejected open");

var priceData = JObject.Parse(json);
priceData["boxes"][0]["drops"] = JArray.Parse("[{\"item\":\"V02\",\"weight\":100}]");
priceData["rules"]["quality"] = new JArray(priceData["rules"]["quality"][1].DeepClone());
foreach (var fake in new[] { false, true })
{
    priceData["boxes"][0]["fake_rate"] = fake ? 1 : 0;
    var pricing = new MarketRun(priceData.ToString(), 123);
    var bought = pricing.BuyBox("price-buy", pricing.Shelf[0].Id);
    var opened = pricing.OpenBox("price-open", bought.EntityId);
    Check(opened.WasFake == null, "opening does not reveal authenticity");
    Check(pricing.Inventory.Single().ListedPrice == 269, "listed price floors and never reveals a fake");
    Check(pricing.Inspect(opened.EntityId).ListedPrice == 269, "inspection keeps the same listed price");
    var sold = pricing.Sell("price-sell", opened.EntityId);
    var payout = fake ? 53 : 269;
    Check(sold.Amount == payout && sold.WasFake == fake, "sale floors actual price and reveals authenticity");
    Check(pricing.Cash == 1000 - bought.Amount + payout, "cash receives the actual payout");
}

var temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
try
{
    var file = new MarketSaveFile(Path.Combine(temporaryDirectory, "run.json"));
    var failWrites = false;
    var persisted = new MarketRun(json, 123, null, data =>
    {
        if (failWrites) throw new IOException("Simulated write failure");
        file.Write(data);
    });
    var offer = persisted.Shelf[0];
    failWrites = true;
    try
    {
        persisted.BuyBox("persist-buy", offer.Id);
        throw new Exception("Expected write failure");
    }
    catch (IOException)
    {
        Check(persisted.Cash == 1000 && persisted.Table.Count == 0, "failed write leaves live state unchanged");
    }
    failWrites = false;
    Check(persisted.BuyBox("persist-buy", offer.Id).Success, "retry save without duplicate debit");
    var loaded = new MarketRun(json, 999, file.Read(), file.Write);
    Check(loaded.Cash == 1000 - offer.Price && loaded.Table.Single().Id == offer.Id, "atomic save reload");
    Check(loaded.BuyBox("persist-buy", loaded.Shelf[0].Id).Code == "DUPLICATE_TRANSACTION", "duplicate after reload");
    foreach (var badSave in new[] { "{", "{}", "{\"Schema\":99}", file.Read().Replace("\"Table\":[", "\"Table\":null,\"Ignored\":[") })
    {
        file.Write(badSave);
        string backup = null;
        var recovered = file.LoadOrStart(json, 42, path => backup = path);
        Check(recovered.Day == 1 && recovered.Cash == 1000 && recovered.Table.Count == 0, "fresh run after bad save");
        Check(File.ReadAllText(backup) == badSave, "bad save preserved");
        Check(file.LoadOrStart(json, 12).Cash == 1000, "replacement save reloads");
    }
}
finally
{
    if (Directory.Exists(temporaryDirectory)) Directory.Delete(temporaryDirectory, true);
}
Console.WriteLine("Gameplay checks passed.");

var sceneCore = new MarketRun(json, 123);
var sceneService = new ChoDoLa.Sprint1.Sprint1MarketService();
sceneService.Bind(new MarketRunHost { Run = sceneCore });
var sceneOffer = sceneService.CurrentOffers[0];
Check(sceneOffer.TodayPrice == sceneCore.TodayPrices[sceneOffer.BoxTypeId], "scene sees today's price");
Check(sceneService.RequestBuyClue(sceneOffer.OfferId), "scene purchases a tip");
Check(sceneService.RequestBuyBox(sceneOffer.OfferId), "scene buys the displayed offer");
Check(sceneService.CurrentRunState.Cash == 1000 - 15 - sceneOffer.Price, "scene uses the gameplay wallet");
Check(sceneService.RequestOpenBox(sceneService.UnopenedBoxes.Single().BoxInstanceId), "scene opens purchased box");
var sceneItem = sceneService.InventoryItems.Single();
Check(sceneItem.Authenticity == ChoDoLa.Contracts.AuthenticityState.Unverified, "scene does not reveal authenticity");
Check(sceneService.RequestSellArtifact(sceneItem.InstanceId), "scene sells the opened item");
Check(!sceneService.RequestSellArtifact(sceneItem.InstanceId), "scene cannot sell twice");
Check(sceneService.RequestEndDay(1) && !sceneService.RequestEndDay(1), "scene confirms the displayed day only once");
Check(sceneService.CurrentRunState.CurrentDay == 2 && sceneService.CurrentRunState.DailyBoxesPurchased == 0,
    "scene displays the new day and reset quota");
Console.WriteLine("Scene service checks passed.");

string resumeState = null;
var beforeReload = new MarketRun(json, 932, null, state => resumeState = state);
var resumeOffer = beforeReload.Shelf[0];
var resumeBox = beforeReload.BuyBox("resume-buy", resumeOffer.Id);
var resumedService = new ChoDoLa.Sprint1.Sprint1MarketService();
resumedService.Bind(new MarketRunHost { Run = new MarketRun(json, 0, resumeState) });
Check(resumedService.UnopenedBoxes.Single().BoxInstanceId == resumeBox.EntityId, "scene restores saved box ID");
Check(resumedService.CurrentRunState.Cash == 1000 - resumeOffer.Price, "scene restores saved wallet");

var tactData = JObject.Parse(json);
tactData["rules"]["start_cash"] = 100000;
string tactSave = null;
var tactRun = new MarketRun(tactData.ToString(), 123, null, value => tactSave = value);
Check(tactRun.EquipSlotCount == 4 && tactRun.EquippedTacts.Count == 4, "four initial slots");
Check(tactRun.EquipTact("not-owned", "CP1", 0).Code == "TACT_NOT_OWNED", "ownership gate");
foreach (var id in new[] { "CP1", "CP2", "CP3", "TC1", "TC2", "TC3" })
    Check(tactRun.BuyTact("buy-" + id, id).Success, "purchase tact");
Check(!tactRun.BuyTact("buy-CP1-again", "CP1").Success, "no duplicate purchase");
Check(tactRun.Inventory.Count == 0 && tactRun.TactCatalog.Count(item => item.Owned) == 6, "separate bag");
Check(tactRun.EquipTact("outside", "CP1", 4).Code == "INVALID_SLOT", "fifth slot locked");
Check(tactRun.EquipTact("equip-cp1", "CP1", 0).Success, "equip first");
Check(tactRun.EquipTact("same", "CP1", 1).Success && tactRun.EquippedTacts[0] == null, "move without duplicate");
Check(tactRun.EquipTact("equip-cp2", "CP2", 0).Success, "equip second");
Check(tactRun.ActiveCombos.Contains("CB1") && tactRun.Resonance["co_phong"] == 2 &&
    tactRun.Bikhe1Unlocked && tactRun.EquipSlotCount == 5, "small combo earns BK1");
Check(tactRun.EquipTact("equip-cp3", "CP3", 4).Success && tactRun.Resonance["co_phong"] == 3, "fifth slot and tier three");
Check(tactRun.Shelf[0].Clues.Length == 2, "tier three Cổ Phong adds a clue");
Check(tactRun.EquipTact("equip-tc3", "TC3", 2).Success && tactRun.ActiveCombos.Contains("CB3"), "cross-set combo");
tactRun = new MarketRun(tactData.ToString(), 999, tactSave);
Check(tactRun.EquippedTacts[4] == "CP3" && tactRun.Bikhe1Unlocked && tactRun.Cash == 97880, "load equipment and exact cash");
Check(tactRun.UnequipTact("remove-cp2", 0).Success && !tactRun.ActiveCombos.Contains("CB1") &&
    tactRun.Resonance["co_phong"] == 2 && tactRun.EquipSlotCount == 5, "removing combo keeps BK1");
var lateTact = new MarketRun(tactData.ToString(), 123);
for (var day = 1; day <= 4; day++) Check(lateTact.EndDay("late-" + day, day).Success, "advance to cutoff");
Check(lateTact.BuyTact("late-buy-1", "TC1").Success && lateTact.BuyTact("late-buy-2", "TC2").Success &&
    lateTact.EquipTact("late-equip-1", "TC1", 0).Success && lateTact.EquipTact("late-equip-2", "TC2", 1).Success,
    "equip late combo");
Check(lateTact.ActiveCombos.Contains("CB2") && lateTact.Shelf[0].Clues.Length == 2 &&
    !lateTact.Bikhe1Unlocked && lateTact.EquipSlotCount == 4, "day five activates combo without BK1");
Console.WriteLine("Tact equipment checks passed.");

var priceTactData = JObject.Parse(json);
priceTactData["rules"]["start_cash"] = 100000;
priceTactData["boxes"][0]["drops"] = JArray.Parse("[{\"item\":\"V01\",\"weight\":100}]");
priceTactData["boxes"][0]["fake_rate"] = 0;
priceTactData["rules"]["quality"] = new JArray(priceTactData["rules"]["quality"][0].DeepClone());
var priceTactRun = new MarketRun(priceTactData.ToString(), 123);
var priceTactBox = priceTactRun.BuyBox("price-tact-box", priceTactRun.Shelf[0].Id);
var priceTactItem = priceTactRun.OpenBox("price-tact-item", priceTactBox.EntityId).EntityId;
Check(priceTactRun.Inspect(priceTactItem).ListedPrice == 230, "unmodified base price");
foreach (var id in new[] { "CP1", "CP2", "CP3", "TC1", "TC2" })
    Check(priceTactRun.BuyTact("price-tact-buy-" + id, id).Success, "buy equipment for price test");
Check(priceTactRun.EquipTact("price-tact-equip-1", "CP1", 0).Success &&
    priceTactRun.EquipTact("price-tact-equip-2", "CP2", 1).Success, "enable price combo");
Check(priceTactRun.Inspect(priceTactItem).ListedPrice == 276, "add item, combo and tier two effects");
Check(priceTactRun.EquipTact("price-tact-equip-3", "CP3", 2).Success &&
    priceTactRun.Inspect(priceTactItem).ListedPrice == 264, "tier three replaces tier two bonus");
Check(priceTactRun.EquipTact("price-tact-equip-4", "TC1", 3).Success &&
    priceTactRun.EquipTact("price-tact-equip-5", "TC2", 4).Success, "equip mixed five-slot layout");
Check(priceTactRun.Shelf[0].Clues.Length == 3, "two free clue bonuses reach three clues");
Check(priceTactRun.Sell("price-tact-sell", priceTactItem).Amount == 264, "same effects used for actual sale");

var kimKhuy = new ChoDoLa.Sprint1.Sprint1MarketService();
kimKhuy.Bind(new MarketRunHost { Run = new MarketRun(json, 271) });
Check(kimKhuy.TactCatalog.Count == 6 && kimKhuy.EquippedArtifacts.Count == 4, "Kim Khuy reads gameplay catalogue");
Check(kimKhuy.TactCatalog.Single(item => item.TactId == "CP1").IconModelName == "CP1", "model binding uses the gameplay ID");
var kimCombo = kimKhuy.ActiveCombos.Single(combo => combo.ComboId == "CB1");
Check(!kimCombo.IsActive && kimCombo.MissingTactIds.SequenceEqual(new[] { "CP1", "CP2" }), "inactive combo exposes missing icons");
Check(kimKhuy.RequestBuyTactArtifact("CP1") && kimKhuy.RequestEquipTactArtifact("CP1", 0), "Kim Khuy buys and equips");
Check(kimKhuy.ActiveCombos.Single(combo => combo.ComboId == "CB1").MissingTactIds.SequenceEqual(new[] { "CP2" }), "one icon stays missing");
Check(kimKhuy.RequestBuyTactArtifact("CP2") && kimKhuy.RequestEquipTactArtifact("CP2", 1), "Kim Khuy finishes combo");
Check(kimKhuy.CurrentRunState.MaxEquipSlots == 5 && kimKhuy.ActiveCombos.Single(combo => combo.ComboId == "CB1").IsActive,
    "Kim Khuy displays BK1 and active combo");
Check(kimKhuy.RequestUnequipTactArtifact(1) && kimKhuy.CurrentRunState.MaxEquipSlots == 5, "unequip preserves slot five");
Console.WriteLine("Kim Khuy checks passed.");

var heirlooms = Enumerable.Range(0, 32).Select(seed => new MarketRun(originalJson, seed).ActiveHeirloomId).Distinct().ToArray();
Check(heirlooms.Length == 2, "both heirlooms are available from deterministic seeds");
var gt2Seed = Enumerable.Range(0, 32).First(seed => new MarketRun(originalJson, seed).ActiveHeirloomId == "GT2");
var lamp = new MarketRun(originalJson, gt2Seed);
Check(lamp.Shelf.Single(offer => offer.TypeId == "H1").Clues.Length == 2 &&
    lamp.Shelf.Single(offer => offer.TypeId == "H2").Clues.Length == 1, "GT2 bonus only on H1");
Check(!lamp.EndDay("lamp-postpone", 1, postpone: true).Success && lamp.Day == 1, "GT2 cannot postpone");

var heirData = JObject.Parse(json);
heirData["rules"]["start_cash"] = 100000;
string heirSave = null;
var book = new MarketRun(heirData.ToString(), 123, null, saveData => heirSave = saveData);
for (int day = 1; day <= 3; day++) Check(book.EndDay($"book-day-{day}", day).Success, "reach GT1 example");
var beforePostpone = book.Cash;
Check(book.EndDay("book-postpone", 4, postpone: true).Success && book.Day == 5 &&
    book.Cash == beforePostpone && book.CarryDebt == 863 && book.DueToday == 1913, "defer only current installment with premium");
book = new MarketRun(heirData.ToString(), 999, heirSave);
Check(book.ActiveHeirloomId == "GT1" && !book.PostponeAvailable && book.CarryDebt == 863, "reload deferred debt and used charge");
Check(!book.EndDay("book-postpone-second", 5, postpone: true).Success, "cannot postpone twice");
Check(book.EndDay("book-pay-carry", 5).Amount == 1913 && book.CarryDebt == 0, "carry collected exactly once");
var finalDay = new MarketRun(heirData.ToString(), 123);
for (int day = 1; day <= 6; day++) Check(finalDay.EndDay($"final-day-{day}", day).Success, "reach last day");
Check(!finalDay.PostponeAvailable && !finalDay.EndDay("last-day-postpone", 7, postpone: true).Success, "no last-day postpone");
Console.WriteLine("Heirloom checks passed.");
heirData["rules"]["start_cash"] = 0;
var arrearsBook = new MarketRun(heirData.ToString(), 123);
Check(arrearsBook.EndDay("arrears-first", 1).Success && arrearsBook.Arrears == 240, "record existing arrears");
var deferredReceipt = arrearsBook.EndDay("arrears-postpone", 2, postpone: true);
Check(deferredReceipt.Penalty == 48 && arrearsBook.Arrears == 288 && arrearsBook.CarryDebt == 403,
    "GT1 does not defer old arrears and deferred amount is not penalized that day");
