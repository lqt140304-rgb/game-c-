using System;
using System.Collections.Generic;
using System.Linq;
using ChoDoLa.Town.Layout;
using NUnit.Framework;

namespace ChoDoLa.Town.Layout.Tests
{
    public class TownLayoutValidatorTests
    {
        private sealed class LayoutData
        {
            public List<TownPiece> Pieces = new List<TownPiece>();
            public List<Walkway> Walkways = new List<Walkway>();
        }

        private static LayoutData MinimalValidLayout()
        {
            var data = new LayoutData();

            data.Walkways.Add(new Walkway("MainStreet_S", new Rect2(-3f, 2.5f, 3f, 27f), 0f));
            data.Walkways.Add(new Walkway("MainStreet_N", new Rect2(-3f, 27f, 3f, 43f), 0f));
            data.Walkways.Add(new Walkway("TowerArch", new Rect2(-2f, 43f, 2f, 51f), 0f));
            data.Walkways.Add(new Walkway("RearLane", new Rect2(-12.5f, 51f, 12.5f, 54.5f), 0f));
            data.Walkways.Add(new Walkway("Alley_W", new Rect2(-12.5f, 8f, -9f, 54.5f), 0f));
            data.Walkways.Add(new Walkway("Alley_E", new Rect2(9f, 8f, 12.5f, 54.5f), 0f));
            data.Walkways.Add(new Walkway("Alley_LinkW", new Rect2(-9f, 18f, -3f, 21.5f), 0f));
            data.Walkways.Add(new Walkway("Alley_LinkE", new Rect2(3f, 18f, 9f, 21.5f), 0f));
            data.Walkways.Add(new Walkway("Deck_W", new Rect2(-16.5f, 33f, -3f, 39f), 3.8f));
            data.Walkways.Add(new Walkway("Bridge", new Rect2(-3f, 34.4f, 3f, 37.6f), 3.8f));
            data.Walkways.Add(new Walkway("Deck_E", new Rect2(3f, 33f, 16.5f, 39f), 3.8f));
            data.Walkways.Add(new Walkway("RampBase_W", new Rect2(-16.5f, 21.5f, -12.5f, 25f), 0f));
            data.Walkways.Add(new Walkway("RampBase_E", new Rect2(12.5f, 21.5f, 16.5f, 25f), 0f));

            data.Pieces.Add(new TownPiece("Inn_Spawn", TownPieceKind.InnSpawn, new Float3(0f, 0f, 4f), new Float3(4f, 0.1f, 3f), 0f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("OpenTable_Zone", TownPieceKind.OpenTableZone, new Float3(0f, 0f, 22f), new Float3(4f, 0.1f, 4f), 0f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("Tower_Main", TownPieceKind.LandmarkTower, new Float3(0f, 0f, 47f), new Float3(8f, 15f, 8f), 0f, TownPalette.Stone, clearanceHeight: 3.6f));
            data.Pieces.Add(new TownPiece("Bridge_Main", TownPieceKind.Bridge, new Float3(0f, 3.8f, 36f), new Float3(6f, 0.4f, 3.2f), 0f, TownPalette.Stone, clearanceHeight: 3.4f));
            data.Pieces.Add(new TownPiece("Ramp_W", TownPieceKind.Ramp, new Float3(-14.75f, 1.9f, 29f), new Float3(3f, 3.8f, 8f), 0f, TownPalette.Stone));
            data.Pieces.Add(new TownPiece("Ramp_E", TownPieceKind.Ramp, new Float3(14.75f, 1.9f, 29f), new Float3(3f, 3.8f, 8f), 0f, TownPalette.Stone));

            data.Pieces.Add(new TownPiece("ShopSlot_01", TownPieceKind.ShopSlot, new Float3(-4.5f, 0f, 9f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_02", TownPieceKind.ShopSlot, new Float3(4.5f, 0f, 9f), new Float3(4f, 0.1f, 3f), 270f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_03", TownPieceKind.ShopSlot, new Float3(-14f, 0f, 16f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_04", TownPieceKind.ShopSlot, new Float3(14.5f, 0f, 16f), new Float3(5f, 0.1f, 4f), 270f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_05", TownPieceKind.ShopSlot, new Float3(-7.5f, 0f, 31f), new Float3(4f, 0.1f, 3f), 270f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_06", TownPieceKind.ShopSlot, new Float3(9.5f, 3.8f, 36f), new Float3(5f, 0.1f, 4f), 270f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_07", TownPieceKind.ShopSlot, new Float3(-7.5f, 0f, 48f), new Float3(4f, 0.1f, 3f), 270f, TownPalette.Marker));
            data.Pieces.Add(new TownPiece("ShopSlot_08", TownPieceKind.ShopSlot, new Float3(-9.5f, 3.8f, 36f), new Float3(5f, 0.1f, 4f), 90f, TownPalette.Marker));

            return data;
        }

        private static void AssertRule(Action<LayoutData> mutate, string expectedRuleCode)
        {
            var data = MinimalValidLayout();
            mutate(data);
            var errors = TownLayoutValidator.Validate(data.Pieces, data.Walkways);
            Assert.That(errors.Any(e => e.StartsWith(expectedRuleCode)), Is.True,
                $"Expected {expectedRuleCode}, got: [{string.Join("; ", errors)}]");
        }

        [Test]
        public void MinimalLayout_IsValid()
        {
            var data = MinimalValidLayout();
            var errors = TownLayoutValidator.Validate(data.Pieces, data.Walkways);
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Slots_WrongSizeFor04_Reported()
        {
            AssertRule(d =>
            {
                int i = d.Pieces.FindIndex(p => p.Id == "ShopSlot_04");
                var old = d.Pieces[i];
                d.Pieces[i] = new TownPiece(old.Id, old.Kind, old.Center, new Float3(4f, 0.1f, 3f), old.YawDegrees, old.Palette);
            }, "[SLOTS]");
        }

        [Test]
        public void Ramps_MeetDeckAndGroundWithoutGap()
        {
            AssertRule(d =>
            {
                int i = d.Pieces.FindIndex(p => p.Id == "Ramp_W");
                var old = d.Pieces[i];
                d.Pieces[i] = new TownPiece(old.Id, old.Kind, new Float3(old.Center.X, 1.8f, old.Center.Z), new Float3(old.Size.X, 3.6f, old.Size.Z), old.YawDegrees, old.Palette);
            }, "[RAMP]");
        }

        [Test]
        public void Ramp_Steeper_Than28_Reported()
        {
            AssertRule(d =>
            {
                int i = d.Pieces.FindIndex(p => p.Id == "Ramp_W");
                var old = d.Pieces[i];
                d.Pieces[i] = new TownPiece(old.Id, old.Kind, old.Center, new Float3(old.Size.X, old.Size.Y, 6.5f), old.YawDegrees, old.Palette);
            }, "[RAMP]");
        }

        [Test]
        public void Walkways_DoNotTouchTownBoundary()
        {
            AssertRule(d =>
            {
                d.Walkways.Add(new Walkway("Alley_Edge", new Rect2(12.5f, 18f, 22.5f, 21.5f), 0f));
            }, "[BOUNDARY]");
        }

        [Test]
        public void Overheads_HaveMinimumClearance()
        {
            AssertRule(d =>
            {
                int i = d.Pieces.FindIndex(p => p.Id == "Tower_Main");
                var old = d.Pieces[i];
                d.Pieces[i] = new TownPiece(old.Id, old.Kind, old.Center, old.Size, old.YawDegrees, old.Palette, clearanceHeight: 3.0f);
            }, "[CLEARANCE]");
        }

        [Test]
        public void Building_OnWalkway_Reported()
        {
            AssertRule(d =>
            {
                d.Pieces.Add(new TownPiece("Blocker", TownPieceKind.Building, new Float3(0f, 0f, 12f), new Float3(2f, 5f, 2f), 0f, TownPalette.Stone));
            }, "[WALK_CLEAR]");
        }

        [Test]
        public void Marker_OnWalkway_Allowed()
        {
            var data = MinimalValidLayout();
            data.Pieces.Add(new TownPiece("ExtraMarker", TownPieceKind.OpenTableZone, new Float3(0f, 0f, 12f), new Float3(2f, 0.1f, 2f), 0f, TownPalette.Marker));
            var errors = TownLayoutValidator.Validate(data.Pieces, data.Walkways);
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void ThreeAccents_OnOneFacade_Reported()
        {
            AssertRule(d =>
            {
                d.Pieces.Add(new TownPiece("A1", TownPieceKind.AccentSteam, new Float3(-5f, 2f, 10f), new Float3(0.5f, 0.5f, 0.5f), 0f, TownPalette.Brass, "Facade_W1"));
                d.Pieces.Add(new TownPiece("A2", TownPieceKind.AccentSciFi, new Float3(-5f, 2f, 11f), new Float3(0.5f, 0.5f, 0.5f), 0f, TownPalette.SciFi, "Facade_W1"));
                d.Pieces.Add(new TownPiece("A3", TownPieceKind.AccentArcane, new Float3(-5f, 2f, 12f), new Float3(0.5f, 0.5f, 0.5f), 0f, TownPalette.Arcane, "Facade_W1"));
            }, "[ACCENTS]");
        }

        [Test]
        public void Disconnected_Walkway_Reported()
        {
            AssertRule(d =>
            {
                d.Walkways.Add(new Walkway("Alley_Isolated", new Rect2(-20f, 10f, -16.8f, 15f), 0f));
            }, "[CONNECTED]");
        }

        [Test]
        public void Alley_Width3_Reported()
        {
            AssertRule(d =>
            {
                d.Walkways.Add(new Walkway("Alley_Narrow", new Rect2(-12.5f, 20f, -9f, 23f), 0f));
            }, "[WIDTH]");
        }
    }
}

namespace ChoDoLa.Town.Layout.Tests
{
    public class TownLayoutValidatorReviewTests
    {
        [Test]
        public void SolidBuilding_OverlappingSlot_Reported()
        {
            var pieces = new System.Collections.Generic.List<TownPiece>(TownLayout.Pieces);
            pieces.Add(new TownPiece("BadBuilding", TownPieceKind.Building, new Float3(-4.5f, 4f, 9f), new Float3(3f, 8f, 3f), 0f, TownPalette.Stone));
            var errors = TownLayoutValidator.Validate(pieces, TownLayout.Walkways);
            Assert.That(errors.Any(e => e.StartsWith("[SLOT_CLEAR]")), Is.True, string.Join("; ", errors));
        }

        [Test]
        public void Arch_PillarsBlockWalkway_Reported()
        {
            var pieces = new System.Collections.Generic.List<TownPiece>(TownLayout.Pieces);
            int idx = pieces.FindIndex(p => p.Id == "HoloArch_SE");
            var old = pieces[idx];
            pieces[idx] = new TownPiece(old.Id, old.Kind, old.Center, new Float3(6f, 4.5f, 3.5f), 0f, old.Palette, old.FacadeId, old.ClearanceHeight);
            var errors = TownLayoutValidator.Validate(pieces, TownLayout.Walkways);
            Assert.That(errors.Any(e => e.StartsWith("[WALK_CLEAR]") && e.Contains("HoloArch_SE")), Is.True, string.Join("; ", errors));
        }

        [Test]
        public void UpperSlot_OverhangingDeck_Reported()
        {
            var pieces = new System.Collections.Generic.List<TownPiece>(TownLayout.Pieces);
            int idx = pieces.FindIndex(p => p.Id == "ShopSlot_06");
            var old = pieces[idx];
            pieces[idx] = new TownPiece(old.Id, old.Kind, new Float3(old.Center.X, old.Center.Y, 37.5f), old.Size, old.YawDegrees, old.Palette);
            var errors = TownLayoutValidator.Validate(pieces, TownLayout.Walkways);
            Assert.That(errors.Any(e => e.StartsWith("[SLOT_SUPPORT]")), Is.True, string.Join("; ", errors));
        }

        [Test]
        public void LowBridge_Reported()
        {
            var pieces = new System.Collections.Generic.List<TownPiece>(TownLayout.Pieces);
            pieces.Add(new TownPiece("LowBridge", TownPieceKind.Bridge, new Float3(0f, 3.8f, 10f), new Float3(6f, 1.0f, 3f), 0f, TownPalette.Stone));
            var errors = TownLayoutValidator.Validate(pieces, TownLayout.Walkways);
            Assert.That(errors.Any(e => e.StartsWith("[CLEARANCE]") && e.Contains("LowBridge")), Is.True, string.Join("; ", errors));
        }
    }
}
