using System;
using System.IO;
using System.Linq;
using ChoDoLa.Gameplay;
using Newtonsoft.Json.Linq;

var json = File.ReadAllText(args.Single());
string save = null;
var run = new MarketRun(json, 123, null, data => save = data);
void Check(bool condition, string label)
{
    if (!condition) throw new Exception(label);
}

var first = run.Shelf.Single(box => box.TypeId == "H1");
Check(first.Price == 100 && first.Clues.Length == 1, "initial offer");
Check(run.BuyClue("clue-1", first.Id).Success, "buy clue");
Check(run.Cash == 985 && run.Shelf[0].Clues.Length == 2, "clue purchase");
var boxId = run.BuyBox("buy-1", first.Id).EntityId;
Check(run.Cash == 885 && run.Table.Count == 1, "buy");
Check(!run.BuyBox("buy-1", run.Shelf[0].Id).Success && run.Cash == 885, "replay");
run = new MarketRun(json, 999, save, data => save = data);
Check(run.Table.Single().Id == boxId, "load bought box");
var item = run.OpenBox("open-1", boxId).EntityId;
Check(run.Table.Count == 0 && run.Inventory.Single().Id == item, "open transfers to inventory");
Check(!run.OpenBox("open-2", boxId).Success, "duplicate open");
Check(run.Inspect(item).Signs.Length > 0, "inspect");
var sale = run.Sell("sell-1", item);
Check(sale.Success && run.Cash == 885 + sale.Amount && run.Inventory.Count == 0, "sell");
Check(!run.Sell("sell-2", item).Success, "duplicate sale");
for (var i = 2; i <= 3; i++)
    Check(run.BuyBox("buy-" + i, run.Shelf[0].Id).Success, "buy remaining offer");
Check(run.BuyBox("buy-4", first.Id).Code == "DAILY_BOX_LIMIT", "fourth box");
Check(run.EndDay("day-1", 1).Success && run.Day == 2 && run.BoughtToday == 0, "day transition");
Check(!run.EndDay("day-1-double", 1).Success && run.Day == 2, "double click with new token");
Check(run.BuyBox("buy-day2", run.Shelf[0].Id).Success && run.Table.Count == 3, "table persists");
Check(run.BuyBox("buy-full", run.Shelf[0].Id).Code == "TABLE_FULL", "table capacity");

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
    Check(pricing.Cash == 900 + payout, "cash receives the actual payout");
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
    Check(loaded.Cash == 900 && loaded.Table.Single().Id == offer.Id, "atomic save reload");
    Check(loaded.BuyBox("persist-buy", loaded.Shelf[0].Id).Code == "DUPLICATE_TRANSACTION", "duplicate after reload");
}
finally
{
    if (Directory.Exists(temporaryDirectory)) Directory.Delete(temporaryDirectory, true);
}
Console.WriteLine("Gameplay checks passed.");
