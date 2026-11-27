using System;
using System.Linq;
using ChoDoLa.Town.Layout;
using NUnit.Framework;

namespace ChoDoLa.Town.Layout.Tests
{
    public class TownLayoutDataTests
    {
        [Test]
        public void TownLayout_PassesAllRules()
        {
            var errors = TownLayoutValidator.Validate(TownLayout.Pieces, TownLayout.Walkways);
            Assert.That(errors, Is.Empty, string.Join("\n", errors));
        }

        [Test]
        public void TownLayout_HasFourLandmarks()
        {
            int count = TownLayout.Pieces.Count(p =>
                p.Kind == TownPieceKind.LandmarkTower ||
                p.Kind == TownPieceKind.LandmarkWell ||
                p.Kind == TownPieceKind.LandmarkHoloArch ||
                p.Kind == TownPieceKind.Bridge);
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public void TownLayout_TowerIsTallestPiece()
        {
            float maxH = TownLayout.Pieces.Max(p => p.Size.Y);
            var tower = TownLayout.Pieces.Single(p => p.Kind == TownPieceKind.LandmarkTower);
            Assert.That(maxH, Is.EqualTo(tower.Size.Y));
        }

        [Test]
        public void TownLayout_AccentCountBetween18And24_EachKindAtLeast6()
        {
            int steam = TownLayout.Pieces.Count(p => p.Kind == TownPieceKind.AccentSteam);
            int scifi = TownLayout.Pieces.Count(p => p.Kind == TownPieceKind.AccentSciFi);
            int arcane = TownLayout.Pieces.Count(p => p.Kind == TownPieceKind.AccentArcane);
            int total = steam + scifi + arcane;

            Assert.That(total, Is.InRange(18, 24));
            Assert.That(steam, Is.GreaterThanOrEqualTo(6));
            Assert.That(scifi, Is.GreaterThanOrEqualTo(6));
            Assert.That(arcane, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void TownLayout_CoverBuildingsExist_OnBothSidesOfMainStreet()
        {
            bool hasWest = TownLayout.Pieces.Any(p => p.Kind == TownPieceKind.CoverBuilding && p.Center.X < 0f);
            bool hasEast = TownLayout.Pieces.Any(p => p.Kind == TownPieceKind.CoverBuilding && p.Center.X > 0f);
            Assert.That(hasWest && hasEast, Is.True);
        }

        [Test]
        public void TownLayout_SpawnIsInnPorch()
        {
            Assert.That((TownLayout.Spawn.Center.X, TownLayout.Spawn.Center.Z), Is.EqualTo((0f, 4f)));
        }
    }
}
