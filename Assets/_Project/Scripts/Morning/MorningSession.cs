using System;

namespace ChoDoLa.Morning
{
    public enum MorningPhase { ChooseBrush, Brushing, ChooseShop, ChooseMeal, Serving, Eating, Complete }
    public enum RareSpot { Cat, Rock, Coral, Sandal }

    public sealed class MorningSession
    {
        public const int ZoneCount = 3;
        public const int BitesPerMeal = 3;
        public const int SpiceThreshold = 2;

        private readonly float[] clean = new float[ZoneCount];

        public int Day { get; }
        public MorningPhase Phase { get; private set; }
        public int RareZone { get; }
        public RareSpot RareKind { get; }
        public int Brush { get; private set; }
        public int Paste { get; private set; }
        public int Shop { get; private set; }
        public int Meal { get; private set; }
        public int Bites { get; private set; }
        public int Chili { get; private set; }
        public bool IsSpicy { get; private set; }
        public bool IsHot { get; private set; }
        public bool LastBiteWasHot { get; private set; }
        public bool BrushingSkipped { get; private set; }
        public bool BreakfastSkipped { get; private set; }

        public MorningSession(int seed, int day, bool previewRareSpot = false)
        {
            if (day < 1) throw new ArgumentOutOfRangeException(nameof(day));
            Day = day;
            Phase = MorningPhase.ChooseBrush;
            var random = new Random(unchecked((seed * 397) ^ day));
            RareZone = random.Next(10) == 0 || previewRareSpot ? random.Next(ZoneCount) : -1;
            RareKind = previewRareSpot ? RareSpot.Cat : (RareSpot)random.Next(4);
        }

        public bool StartBrushing(int brush, int paste)
        {
            if (Phase != MorningPhase.ChooseBrush || brush < 0 || brush >= 3 || paste < 0 || paste >= 3)
                return false;
            Brush = brush;
            Paste = paste;
            Phase = MorningPhase.Brushing;
            return true;
        }

        public float Cleanliness(int zone) => clean[zone];

        public bool BrushZone(int zone, float travel)
        {
            if (Phase != MorningPhase.Brushing || zone < 0 || zone >= ZoneCount ||
                float.IsNaN(travel) || float.IsInfinity(travel) || travel <= 0 || clean[zone] >= 1) return false;

            clean[zone] = Math.Min(1, clean[zone] + travel);
            if (clean[0] >= 1 && clean[1] >= 1 && clean[2] >= 1)
                Phase = MorningPhase.ChooseShop;
            return true;
        }

        public bool ChooseShop(int shop)
        {
            if (Phase != MorningPhase.ChooseShop || shop < 0 || shop >= 2) return false;
            Shop = shop;
            Phase = MorningPhase.ChooseMeal;
            return true;
        }

        public bool ReturnToShops()
        {
            if (Phase != MorningPhase.ChooseMeal) return false;
            Phase = MorningPhase.ChooseShop;
            return true;
        }

        public bool Order(int meal)
        {
            if (Phase != MorningPhase.ChooseMeal || meal < 0 || meal >= 2) return false;
            Meal = meal;
            Phase = MorningPhase.Serving;
            return true;
        }

        public bool Serve()
        {
            if (Phase != MorningPhase.Serving) return false;
            Phase = MorningPhase.Eating;
            IsHot = Shop == 0;
            return true;
        }

        public bool AddChili()
        {
            if (Phase != MorningPhase.Eating || Chili >= SpiceThreshold) return false;
            Chili++;
            return true;
        }

        public bool BlowOnFood()
        {
            if (Phase != MorningPhase.Eating || !IsHot) return false;
            IsHot = false;
            return true;
        }

        public bool EatBite()
        {
            if (Phase != MorningPhase.Eating) return false;
            LastBiteWasHot = IsHot;
            Bites++;
            IsSpicy = Chili >= SpiceThreshold;
            if (Bites == BitesPerMeal)
                Phase = MorningPhase.Complete;
            return true;
        }

        public bool DrinkWater()
        {
            if ((Phase != MorningPhase.Eating && Phase != MorningPhase.Complete) || !IsSpicy) return false;
            IsSpicy = false;
            return true;
        }

        public bool TakeShortcut(bool quick)
        {
            if (Day == 1) return false;
            if (Phase == MorningPhase.ChooseBrush || Phase == MorningPhase.Brushing)
            {
                BrushingSkipped = !quick;
                Phase = MorningPhase.ChooseShop;
                return true;
            }
            if (Phase != MorningPhase.ChooseShop && Phase != MorningPhase.ChooseMeal) return false;
            BreakfastSkipped = !quick;
            Phase = MorningPhase.Complete;
            return true;
        }
    }
}
