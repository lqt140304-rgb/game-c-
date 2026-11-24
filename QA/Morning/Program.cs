using System;
using ChoDoLa.Morning;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var morning = new MorningSession(8, 1, true);
Check(!morning.TakeShortcut(false), "day one has no shortcuts");
Check(morning.StartBrushing(1, 2) && morning.Brush == 1 && morning.Paste == 2, "select equipment");
Check(morning.RareZone >= 0 && morning.RareZone < 3, "forced rare spot for a demo");
Check(!morning.BrushZone(0, 0) && morning.Cleanliness(0) == 0, "stationary brush");
for (int zone = 0; zone < 3; zone++)
    Check(morning.BrushZone(zone, 1), "clean each zone");
Check(morning.Phase == MorningPhase.ChooseShop, "brushing complete");
Check(morning.ChooseShop(0) && !morning.Order(2) && morning.Order(0), "shop and menu");
Check(!morning.EatBite() && morning.Serve(), "food only after serving");
Check(morning.IsHot && morning.BlowOnFood() && !morning.IsHot, "blowing cools the soup");
Check(morning.AddChili() && morning.AddChili(), "add chili");
Check(morning.EatBite() && morning.IsSpicy, "first bite triggers spice");
Check(morning.DrinkWater() && !morning.IsSpicy, "water soothes spice");
Check(morning.EatBite() && morning.EatBite() && !morning.EatBite(), "finish exactly three bites");
Check(morning.Phase == MorningPhase.Complete, "morning complete");

var quick = new MorningSession(8, 2);
Check(quick.TakeShortcut(false) && quick.BrushingSkipped && quick.Phase == MorningPhase.ChooseShop, "skip brushing");
Check(quick.TakeShortcut(true) && !quick.BreakfastSkipped && quick.Phase == MorningPhase.Complete, "quick breakfast");

var rareCount = 0;
for (var seed = 0; seed < 500; seed++)
{
    var a = new MorningSession(seed, 2);
    var b = new MorningSession(seed, 2);
    Check(a.RareZone == b.RareZone && a.RareKind == b.RareKind, "seeded variation");
    if (a.RareZone >= 0) rareCount++;
}
Check(rareCount >= 25 && rareCount <= 75, "rare spots near ten percent");
Console.WriteLine($"Morning checks passed, including {rareCount}/500 rare rolls.");
