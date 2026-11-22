using System.Collections.Generic;

namespace ChoDoLa.Town.Layout
{
    public static class TownLayout
    {
        public static readonly IReadOnlyList<Walkway> Walkways = new List<Walkway>
        {
            new Walkway("MainStreet_S", new Rect2(-3f, 2.5f, 3f, 17f), 0f),
            new Walkway("Plaza", new Rect2(-5.8f, 17f, 5.8f, 27f), 0f),
            new Walkway("MainStreet_N", new Rect2(-3f, 27f, 3f, 43f), 0f),
            new Walkway("TowerArch", new Rect2(-2f, 43f, 2f, 51f), 0f),
            new Walkway("Alley_RearLane", new Rect2(-12.5f, 51f, 12.5f, 54.5f), 0f),
            new Walkway("Alley_W", new Rect2(-12.5f, 8f, -9f, 54.5f), 0f),
            new Walkway("Alley_E", new Rect2(9f, 8f, 12.5f, 54.5f), 0f),
            new Walkway("Alley_ArchW", new Rect2(-9f, 18f, -5.8f, 21.5f), 0f),
            new Walkway("Alley_LaneWell", new Rect2(5.8f, 19f, 9f, 22.5f), 0f),
            new Walkway("Alley_HoloSE", new Rect2(3f, 9f, 9f, 12.5f), 0f),
            new Walkway("Alley_RampBaseW", new Rect2(-19.5f, 21.5f, -12.5f, 25f), 0f),
            new Walkway("Alley_RampBaseE", new Rect2(12.5f, 21.5f, 19.5f, 25f), 0f),
            new Walkway("Deck_W", new Rect2(-19.5f, 33f, -3f, 39f), TownSpec.DeckElevation),
            new Walkway("Bridge", new Rect2(-3f, 34.4f, 3f, 37.6f), TownSpec.DeckElevation),
            new Walkway("Deck_E", new Rect2(3f, 33f, 19.5f, 39f), TownSpec.DeckElevation)
        };

        public static readonly IReadOnlyList<TownPiece> Pieces = new List<TownPiece>
        {
            // Ground base
            new TownPiece("Ground_Town", TownPieceKind.Ground, new Float3(0f, -0.05f, 27.5f), new Float3(45f, 0.1f, 55f), 0f, TownPalette.Stone),

            // Markers
            new TownPiece("Inn_Spawn", TownPieceKind.InnSpawn, new Float3(0f, 0f, 4f), new Float3(4f, 0.1f, 3f), 0f, TownPalette.Marker),
            new TownPiece("OpenTable_Zone", TownPieceKind.OpenTableZone, new Float3(0f, 0f, 22f), new Float3(4f, 0.1f, 4f), 0f, TownPalette.Marker),

            // ShopSlots (8)
            new TownPiece("ShopSlot_01", TownPieceKind.ShopSlot, new Float3(-4.5f, 0f, 9f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_02", TownPieceKind.ShopSlot, new Float3(4.5f, 0f, 5.5f), new Float3(4f, 0.1f, 3f), 270f, TownPalette.Marker),
            new TownPiece("ShopSlot_03", TownPieceKind.ShopSlot, new Float3(-14f, 0f, 16f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_04", TownPieceKind.ShopSlot, new Float3(14.5f, 0f, 17f), new Float3(5f, 0.1f, 4f), 270f, TownPalette.Marker),
            new TownPiece("ShopSlot_05", TownPieceKind.ShopSlot, new Float3(-14f, 0f, 31f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_06", TownPieceKind.ShopSlot, new Float3(9.5f, TownSpec.DeckElevation, 36f), new Float3(5f, 0.1f, 4f), 270f, TownPalette.Marker),
            new TownPiece("ShopSlot_07", TownPieceKind.ShopSlot, new Float3(-14f, 0f, 48f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_08", TownPieceKind.ShopSlot, new Float3(-9.5f, TownSpec.DeckElevation, 36f), new Float3(5f, 0.1f, 4f), 90f, TownPalette.Marker),

            // Landmarks (4)
            new TownPiece("Tower_Main", TownPieceKind.LandmarkTower, new Float3(0f, 0f, 47f), new Float3(8f, 15f, 8f), 0f, TownPalette.Stone, "Facade_Tower", clearanceHeight: 3.6f),
            new TownPiece("Bridge_Main", TownPieceKind.Bridge, new Float3(0f, TownSpec.DeckElevation, 36f), new Float3(6f, 0.4f, 3.2f), 0f, TownPalette.Stone, "Facade_Bridge", clearanceHeight: 3.4f),
            new TownPiece("Well_Arcane", TownPieceKind.LandmarkWell, new Float3(7.5f, 0f, 24.5f), new Float3(1.6f, 1.2f, 1.6f), 0f, TownPalette.Arcane, "Facade_Well"),
            new TownPiece("HoloArch_SE", TownPieceKind.LandmarkHoloArch, new Float3(6f, 0f, 10.75f), new Float3(6.4f, 4.5f, 6f), 90f, TownPalette.SciFi, "Facade_HoloArch", clearanceHeight: 3.4f),

            // Arches & Verticality
            new TownPiece("Arch_W", TownPieceKind.Arch, new Float3(-7.4f, 0f, 19.75f), new Float3(6.4f, 4.5f, 3.2f), 90f, TownPalette.Stone, "Facade_ArchW", clearanceHeight: 3.4f),
            new TownPiece("Deck_W_Floor", TownPieceKind.Deck, new Float3(-11.25f, TownSpec.DeckElevation - 0.15f, 36f), new Float3(16.5f, 0.3f, 6f), 0f, TownPalette.Wood),
            new TownPiece("Deck_E_Floor", TownPieceKind.Deck, new Float3(11.25f, TownSpec.DeckElevation - 0.15f, 36f), new Float3(16.5f, 0.3f, 6f), 0f, TownPalette.Wood),
            new TownPiece("Ramp_W", TownPieceKind.Ramp, new Float3(-18f, 1.9f, 29f), new Float3(3f, TownSpec.DeckElevation, 8f), 0f, TownPalette.Stone),
            new TownPiece("Ramp_E", TownPieceKind.Ramp, new Float3(18f, 1.9f, 29f), new Float3(3f, TownSpec.DeckElevation, 8f), 0f, TownPalette.Stone),
            new TownPiece("Railing_Bridge_N", TownPieceKind.Railing, new Float3(0f, TownSpec.DeckElevation + 0.5f, 37.5f), new Float3(6f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_Bridge_S", TownPieceKind.Railing, new Float3(0f, TownSpec.DeckElevation + 0.5f, 34.5f), new Float3(6f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckW_N", TownPieceKind.Railing, new Float3(-11.25f, TownSpec.DeckElevation + 0.5f, 38.9f), new Float3(16.5f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckW_S", TownPieceKind.Railing, new Float3(-9.75f, TownSpec.DeckElevation + 0.5f, 33.1f), new Float3(13.5f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckE_N", TownPieceKind.Railing, new Float3(11.25f, TownSpec.DeckElevation + 0.5f, 38.9f), new Float3(16.5f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckE_S", TownPieceKind.Railing, new Float3(9.75f, TownSpec.DeckElevation + 0.5f, 33.1f), new Float3(13.5f, 1f, 0.2f), 0f, TownPalette.Wood),

            // Cover Buildings (occlude alleys & slots from Main Street)
            new TownPiece("Cover_W1", TownPieceKind.CoverBuilding, new Float3(-6f, 4.5f, 14.5f), new Float3(5.5f, 9f, 5f), 0f, TownPalette.Stone, "Facade_CoverW1"),
            new TownPiece("Cover_W2", TownPieceKind.CoverBuilding, new Float3(-6f, 4.0f, 30f), new Float3(5.5f, 8f, 5f), 0f, TownPalette.Moss, "Facade_CoverW2"),
            new TownPiece("Cover_E1", TownPieceKind.CoverBuilding, new Float3(6f, 4.5f, 15.45f), new Float3(5.5f, 9f, 3f), 0f, TownPalette.Stone, "Facade_CoverE1"),
            new TownPiece("Cover_E2", TownPieceKind.CoverBuilding, new Float3(6f, 4.0f, 29.5f), new Float3(5.5f, 8f, 5f), 0f, TownPalette.Moss, "Facade_CoverE2"),

            // Inn Facade & Perimeter Buildings (recessed behind ShopSlots 03, 04, 07)
            new TownPiece("Building_InnFacade", TownPieceKind.Building, new Float3(0f, 4.5f, 1.25f), new Float3(14f, 9f, 2.5f), 0f, TownPalette.Wood, "Facade_Inn"),
            new TownPiece("Wall_South_W", TownPieceKind.Wall, new Float3(-14.75f, 3.5f, 1.25f), new Float3(15.5f, 7f, 2.5f), 0f, TownPalette.Stone),
            new TownPiece("Wall_South_E", TownPieceKind.Wall, new Float3(14.75f, 3.5f, 1.25f), new Float3(15.5f, 7f, 2.5f), 0f, TownPalette.Stone),
            new TownPiece("Wall_North", TownPieceKind.Wall, new Float3(0f, 4f, 54.75f), new Float3(45f, 8f, 0.5f), 0f, TownPalette.Stone, "Facade_NorthWall"),
            new TownPiece("Building_West_S", TownPieceKind.Building, new Float3(-17.5f, 5f, 8.5f), new Float3(10f, 10f, 11f), 0f, TownPalette.Stone, "Facade_WestS"),
            new TownPiece("Wall_West_Slot03Back", TownPieceKind.Wall, new Float3(-19.0f, 4f, 16f), new Float3(7f, 8f, 4f), 0f, TownPalette.Stone),
            new TownPiece("Building_West_N", TownPieceKind.Building, new Float3(-19.0f, 5f, 47f), new Float3(7f, 10f, 14f), 0f, TownPalette.Roof, "Facade_WestN"),
            new TownPiece("Wall_West_Edge", TownPieceKind.Wall, new Float3(-21.25f, 4f, 28.5f), new Float3(2.5f, 8f, 23f), 0f, TownPalette.Stone),
            new TownPiece("Building_East_S", TownPieceKind.Building, new Float3(17.5f, 5f, 8.75f), new Float3(10f, 10f, 11.5f), 0f, TownPalette.Stone, "Facade_EastS"),
            new TownPiece("Wall_East_Slot04Back", TownPieceKind.Wall, new Float3(19.5f, 4f, 17f), new Float3(6f, 8f, 5f), 0f, TownPalette.Stone),
            new TownPiece("Building_East_N", TownPieceKind.Building, new Float3(17.5f, 5f, 47f), new Float3(10f, 10f, 14f), 0f, TownPalette.Roof, "Facade_EastN"),
            new TownPiece("Wall_East_Edge", TownPieceKind.Wall, new Float3(21.25f, 4f, 28.5f), new Float3(2.5f, 8f, 23f), 0f, TownPalette.Stone),

            // Scattered 20% Accents (21 total: 7 Steam, 7 SciFi, 7 Arcane; <= 2 per FacadeId)
            new TownPiece("Acc_Steam_01", TownPieceKind.AccentSteam, new Float3(-3.2f, 3.2f, 14f), new Float3(0.6f, 1.4f, 0.6f), 0f, TownPalette.Brass, "Facade_CoverW1"),
            new TownPiece("Acc_SciFi_01", TownPieceKind.AccentSciFi, new Float3(-3.2f, 2.8f, 16f), new Float3(1.2f, 0.6f, 0.3f), 0f, TownPalette.SciFi, "Facade_CoverW1"),
            new TownPiece("Acc_Arcane_01", TownPieceKind.AccentArcane, new Float3(-3.2f, 2.5f, 29f), new Float3(0.5f, 0.9f, 0.5f), 0f, TownPalette.Arcane, "Facade_CoverW2"),
            new TownPiece("Acc_Steam_02", TownPieceKind.AccentSteam, new Float3(-3.2f, 3.4f, 31f), new Float3(0.8f, 0.8f, 0.5f), 0f, TownPalette.Brass, "Facade_CoverW2"),
            new TownPiece("Acc_SciFi_02", TownPieceKind.AccentSciFi, new Float3(3.2f, 2.8f, 14.5f), new Float3(1.0f, 0.6f, 0.3f), 0f, TownPalette.SciFi, "Facade_CoverE1"),
            new TownPiece("Acc_Arcane_02", TownPieceKind.AccentArcane, new Float3(3.2f, 3.2f, 16.2f), new Float3(0.5f, 0.8f, 0.5f), 0f, TownPalette.Arcane, "Facade_CoverE1"),
            new TownPiece("Acc_Steam_03", TownPieceKind.AccentSteam, new Float3(3.2f, 3.0f, 29f), new Float3(0.7f, 1.2f, 0.5f), 0f, TownPalette.Brass, "Facade_CoverE2"),
            new TownPiece("Acc_SciFi_03", TownPieceKind.AccentSciFi, new Float3(3.2f, 2.4f, 31f), new Float3(0.9f, 0.5f, 0.3f), 0f, TownPalette.SciFi, "Facade_CoverE2"),
            new TownPiece("Acc_Steam_04", TownPieceKind.AccentSteam, new Float3(-2.0f, 5.0f, 43.0f), new Float3(1.2f, 1.2f, 0.5f), 0f, TownPalette.Brass, "Facade_Tower"),
            new TownPiece("Acc_Arcane_03", TownPieceKind.AccentArcane, new Float3(2.0f, 5.5f, 43.0f), new Float3(1.0f, 1.0f, 0.5f), 0f, TownPalette.Arcane, "Facade_Tower"),
            new TownPiece("Acc_Steam_05", TownPieceKind.AccentSteam, new Float3(-1.5f, 3.6f, 34.4f), new Float3(2.0f, 0.4f, 0.4f), 0f, TownPalette.Brass, "Facade_Bridge"),
            new TownPiece("Acc_SciFi_04", TownPieceKind.AccentSciFi, new Float3(1.5f, 3.6f, 34.4f), new Float3(2.0f, 0.3f, 0.3f), 0f, TownPalette.SciFi, "Facade_Bridge"),
            new TownPiece("Acc_Arcane_04", TownPieceKind.AccentArcane, new Float3(-2.5f, 3.2f, 2.5f), new Float3(0.5f, 0.9f, 0.5f), 0f, TownPalette.Arcane, "Facade_Inn"),
            new TownPiece("Acc_Steam_06", TownPieceKind.AccentSteam, new Float3(2.5f, 3.2f, 2.5f), new Float3(0.6f, 0.8f, 0.5f), 0f, TownPalette.Brass, "Facade_Inn"),
            new TownPiece("Acc_SciFi_05", TownPieceKind.AccentSciFi, new Float3(3.0f, 3.8f, 10.75f), new Float3(0.2f, 0.6f, 1.6f), 0f, TownPalette.SciFi, "Facade_HoloArch"),
            new TownPiece("Acc_Arcane_05", TownPieceKind.AccentArcane, new Float3(-6.0f, 3.5f, 19.75f), new Float3(0.5f, 0.8f, 0.5f), 0f, TownPalette.Arcane, "Facade_ArchW"),
            new TownPiece("Acc_Steam_07", TownPieceKind.AccentSteam, new Float3(-12.5f, 3.2f, 11f), new Float3(0.6f, 1.2f, 0.5f), 0f, TownPalette.Brass, "Facade_WestS"),
            new TownPiece("Acc_SciFi_06", TownPieceKind.AccentSciFi, new Float3(-15.5f, 2.8f, 45f), new Float3(0.4f, 1.0f, 1.0f), 0f, TownPalette.SciFi, "Facade_WestN"),
            new TownPiece("Acc_Arcane_06", TownPieceKind.AccentArcane, new Float3(12.5f, 3.0f, 11f), new Float3(0.5f, 0.9f, 0.5f), 0f, TownPalette.Arcane, "Facade_EastS"),
            new TownPiece("Acc_SciFi_07", TownPieceKind.AccentSciFi, new Float3(12.5f, 3.0f, 45f), new Float3(0.4f, 1.0f, 1.0f), 0f, TownPalette.SciFi, "Facade_EastN"),
            new TownPiece("Acc_Arcane_07", TownPieceKind.AccentArcane, new Float3(0f, 3.5f, 54.4f), new Float3(1.4f, 1.4f, 0.3f), 0f, TownPalette.Arcane, "Facade_NorthWall"),

            // Street Lights (grounded on walkway edges, Y = 0..3.0)
            new TownPiece("Light_Main_S", TownPieceKind.StreetLight, new Float3(-3.2f, 1.5f, 7f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Plaza_W", TownPieceKind.StreetLight, new Float3(-6.0f, 1.5f, 23f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Plaza_E", TownPieceKind.StreetLight, new Float3(6.0f, 1.5f, 23f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Main_N", TownPieceKind.StreetLight, new Float3(3.2f, 1.5f, 32f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Alley_W", TownPieceKind.StreetLight, new Float3(-12.7f, 1.5f, 27f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Alley_E", TownPieceKind.StreetLight, new Float3(12.7f, 1.5f, 27f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Rear", TownPieceKind.StreetLight, new Float3(0f, 1.5f, 54.65f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood)
        };

        public static TownPiece Spawn
        {
            get
            {
                for (int i = 0; i < Pieces.Count; i++)
                {
                    if (Pieces[i].Kind == TownPieceKind.InnSpawn) return Pieces[i];
                }
                return null;
            }
        }
    }
}
