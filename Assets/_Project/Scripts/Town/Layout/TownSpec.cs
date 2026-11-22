namespace ChoDoLa.Town.Layout
{
    public static class TownSpec
    {
        public const float BoundsMinX = -45f;
        public const float BoundsMaxX = 45f;
        public const float BoundsMinZ = 0f;
        public const float BoundsMaxZ = 96f;

        public static readonly Rect2 TownBounds = new Rect2(BoundsMinX, BoundsMinZ, BoundsMaxX, BoundsMaxZ);

        public const float MainStreetWidth = 6f;
        public const float AlleyMinWidth = 3.2f;
        public const float AlleyMaxWidth = 3.5f;
        public const float DeckElevation = 3.8f;
        public const float MinClearance = 3.2f;
        public const float MaxRampDegrees = 28f;
        public const int MaxAccentsPerFacade = 2;

        public const float SmallSlotWidth = 4f;
        public const float SmallSlotDepth = 3f;
        public const float LargeSlotWidth = 5f;
        public const float LargeSlotDepth = 4f;

        public const float PlayerHeight = 1.8f;
        public const float GapTolerance = 0.05f;
    }
}
