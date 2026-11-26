using ChoDoLa.Town.Layout;
using NUnit.Framework;

namespace ChoDoLa.Town.Layout.Tests
{
    public class TownTypesTests
    {
        private static TownPiece Make(TownPieceKind kind)
        {
            return new TownPiece("id", kind, new Float3(0f, 0f, 0f), new Float3(2f, 2f, 2f), 0f, TownPalette.Stone);
        }

        [Test]
        public void Rect2_FromCenter_ComputesEdges()
        {
            var r = Rect2.FromCenter(0f, 22f, 4f, 4f);
            Assert.That(new[] { r.MinX, r.MinZ, r.MaxX, r.MaxZ }, Is.EqualTo(new[] { -2f, 20f, 2f, 24f }));
        }

        [Test]
        public void Rect2_Overlaps_FalseWhenOnlyEdgesTouch()
        {
            Assert.That(Rect2.FromCenter(0f, 0f, 2f, 2f).Overlaps(Rect2.FromCenter(2f, 0f, 2f, 2f)), Is.False);
            Assert.That(Rect2.FromCenter(0f, 0f, 2f, 2f).Touches(Rect2.FromCenter(2f, 0f, 2f, 2f), 0.05f), Is.True);
        }

        [Test]
        public void TownPiece_Footprint_SwapsAxesAt90Degrees()
        {
            var p = new TownPiece("s", TownPieceKind.ShopSlot, new Float3(10f, 0f, 20f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker);
            Assert.That(p.Footprint.MaxX - p.Footprint.MinX, Is.EqualTo(3f).Within(1e-4));
            Assert.That(p.Footprint.MaxZ - p.Footprint.MinZ, Is.EqualTo(4f).Within(1e-4));
        }

        [Test]
        public void TownPiece_IsMarker_OnlyForMarkerKinds()
        {
            Assert.That(Make(TownPieceKind.ShopSlot).IsMarker, Is.True);
            Assert.That(Make(TownPieceKind.InnSpawn).IsMarker, Is.True);
            Assert.That(Make(TownPieceKind.OpenTableZone).IsMarker, Is.True);
            Assert.That(Make(TownPieceKind.Building).IsMarker, Is.False);
        }
    }
}
