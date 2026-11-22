using System;

namespace ChoDoLa.Town.Layout
{
    public readonly struct Float3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public readonly struct Rect2
    {
        public readonly float MinX;
        public readonly float MinZ;
        public readonly float MaxX;
        public readonly float MaxZ;

        public Rect2(float minX, float minZ, float maxX, float maxZ)
        {
            MinX = minX;
            MinZ = minZ;
            MaxX = maxX;
            MaxZ = maxZ;
        }

        public float Width => MaxX - MinX;
        public float Depth => MaxZ - MinZ;
        public float ShortSide => Math.Min(Width, Depth);

        public static Rect2 FromCenter(float cx, float cz, float width, float depth)
        {
            float hw = width * 0.5f;
            float hd = depth * 0.5f;
            return new Rect2(cx - hw, cz - hd, cx + hw, cz + hd);
        }

        public bool Overlaps(Rect2 o)
        {
            return MinX < o.MaxX && MaxX > o.MinX && MinZ < o.MaxZ && MaxZ > o.MinZ;
        }

        public bool Touches(Rect2 o, float tol)
        {
            bool xTouch = MinX <= o.MaxX + tol && MaxX >= o.MinX - tol;
            bool zTouch = MinZ <= o.MaxZ + tol && MaxZ >= o.MinZ - tol;
            if (!xTouch || !zTouch) return false;

            float xOverlap = Math.Min(MaxX, o.MaxX) - Math.Max(MinX, o.MinX);
            float zOverlap = Math.Min(MaxZ, o.MaxZ) - Math.Max(MinZ, o.MinZ);
            return xOverlap > tol || zOverlap > tol;
        }

        public bool Contains(Rect2 o)
        {
            return o.MinX >= MinX && o.MaxX <= MaxX && o.MinZ >= MinZ && o.MaxZ <= MaxZ;
        }
    }

    public enum TownPieceKind
    {
        Ground,
        Building,
        CoverBuilding,
        Wall,
        Arch,
        Deck,
        Ramp,
        Bridge,
        Railing,
        LandmarkTower,
        LandmarkWell,
        LandmarkHoloArch,
        AccentSteam,
        AccentSciFi,
        AccentArcane,
        ShopSlot,
        InnSpawn,
        OpenTableZone,
        StreetLight
    }

    public enum TownPalette
    {
        Stone,
        Moss,
        Wood,
        Roof,
        Brass,
        SciFi,
        Arcane,
        Marker
    }

    public sealed class TownPiece
    {
        public string Id { get; }
        public TownPieceKind Kind { get; }
        public Float3 Center { get; }
        public Float3 Size { get; }
        public float YawDegrees { get; }
        public TownPalette Palette { get; }
        public string FacadeId { get; }
        public float ClearanceHeight { get; }

        public TownPiece(
            string id,
            TownPieceKind kind,
            Float3 center,
            Float3 size,
            float yawDegrees,
            TownPalette palette,
            string facadeId = null,
            float clearanceHeight = 0f)
        {
            Id = id;
            Kind = kind;
            Center = center;
            Size = size;
            YawDegrees = yawDegrees;
            Palette = palette;
            FacadeId = facadeId;
            ClearanceHeight = clearanceHeight;
        }

        public bool IsMarker =>
            Kind == TownPieceKind.ShopSlot ||
            Kind == TownPieceKind.InnSpawn ||
            Kind == TownPieceKind.OpenTableZone;

        public Rect2 Footprint
        {
            get
            {
                int yaw = (((int)Math.Round(YawDegrees) % 360) + 360) % 360;
                bool swap = yaw == 90 || yaw == 270;
                float w = swap ? Size.Z : Size.X;
                float d = swap ? Size.X : Size.Z;
                return Rect2.FromCenter(Center.X, Center.Z, w, d);
            }
        }

        public float RampBottomY => Center.Y - Size.Y * 0.5f;
        public float RampTopY => Center.Y + Size.Y * 0.5f;
        public float RampDegrees => Size.Z > 0f ? (float)(Math.Atan2(Size.Y, Size.Z) * (180.0 / Math.PI)) : 90f;
    }

    public sealed class Walkway
    {
        public string Id { get; }
        public Rect2 Area { get; }
        public float Elevation { get; }

        public Walkway(string id, Rect2 area, float elevation = 0f)
        {
            Id = id;
            Area = area;
            Elevation = elevation;
        }
    }
}
