using System.Collections.Generic;

namespace ChoDoLa.Town.Layout
{
    public static class TownLayout
    {
        public static readonly IReadOnlyList<Walkway> Walkways = new List<Walkway>
        {
            new Walkway("MainStreet_S", new Rect2(-3f, 2.5f, 3f, 17.2f), 0f),
            new Walkway("Plaza", new Rect2(-4.7f, 17.2f, 4.7f, 30.0f), 0f),
            new Walkway("MainStreet_N", new Rect2(-3f, 30.0f, 3f, 48.0f), 0f),
            new Walkway("TowerArch", new Rect2(-2f, 48.0f, 2f, 56.0f), 0f),
            new Walkway("Alley_RearLane", new Rect2(-13.5f, 56.0f, 13.5f, 59.5f), 0f),
            new Walkway("Alley_W", new Rect2(-13.5f, 8.0f, -10.2f, 59.5f), 0f),
            new Walkway("Alley_E", new Rect2(10.2f, 8.0f, 13.5f, 59.5f), 0f),
            new Walkway("Alley_ArchW", new Rect2(-10.2f, 26.6f, -4.7f, 29.8f), 0f),
            new Walkway("Alley_LaneWell", new Rect2(4.7f, 25.6f, 10.2f, 29.0f), 0f),
            new Walkway("Alley_HoloSE", new Rect2(3f, 9.0f, 10.2f, 12.4f), 0f),
            new Walkway("Alley_RampBaseW", new Rect2(-20.5f, 23.5f, -13.5f, 27.0f), 0f),
            new Walkway("Alley_RampBaseE", new Rect2(13.5f, 23.5f, 20.5f, 27.0f), 0f),
            new Walkway("Deck_W", new Rect2(-20.5f, 35.0f, -3f, 41.0f), TownSpec.DeckElevation),
            new Walkway("Bridge", new Rect2(-3f, 36.4f, 3f, 39.6f), TownSpec.DeckElevation),
            new Walkway("Deck_E", new Rect2(3f, 35.0f, 20.5f, 41.0f), TownSpec.DeckElevation)
        };

        public static readonly IReadOnlyList<TownPiece> Pieces = new List<TownPiece>
        {
            // Expanded Ground base (60m x 68m)
            new TownPiece("Ground_Town", TownPieceKind.Ground, new Float3(0f, -0.05f, 34f), new Float3(60f, 0.1f, 68f), 0f, TownPalette.Stone),

            // Markers
            new TownPiece("Inn_Spawn", TownPieceKind.InnSpawn, new Float3(0f, 0f, 4f), new Float3(4f, 0.1f, 3f), 0f, TownPalette.Marker),
            new TownPiece("OpenTable_Zone", TownPieceKind.OpenTableZone, new Float3(0f, 0f, 22f), new Float3(4f, 0.1f, 4f), 0f, TownPalette.Marker),

            // ShopSlots (8)
            new TownPiece("ShopSlot_01", TownPieceKind.ShopSlot, new Float3(-4.5f, 0f, 9f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_02", TownPieceKind.ShopSlot, new Float3(4.5f, 0f, 5.5f), new Float3(4f, 0.1f, 3f), 270f, TownPalette.Marker),
            new TownPiece("ShopSlot_03", TownPieceKind.ShopSlot, new Float3(-15f, 0f, 16f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_04", TownPieceKind.ShopSlot, new Float3(15.5f, 0f, 17f), new Float3(5f, 0.1f, 4f), 270f, TownPalette.Marker),
            new TownPiece("ShopSlot_05", TownPieceKind.ShopSlot, new Float3(-15f, 0f, 35f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_06", TownPieceKind.ShopSlot, new Float3(9.5f, TownSpec.DeckElevation, 38f), new Float3(5f, 0.1f, 4f), 270f, TownPalette.Marker),
            new TownPiece("ShopSlot_07", TownPieceKind.ShopSlot, new Float3(-15f, 0f, 52f), new Float3(4f, 0.1f, 3f), 90f, TownPalette.Marker),
            new TownPiece("ShopSlot_08", TownPieceKind.ShopSlot, new Float3(-9.5f, TownSpec.DeckElevation, 38f), new Float3(5f, 0.1f, 4f), 90f, TownPalette.Marker),

            // Landmarks (4)
            new TownPiece("Tower_Main", TownPieceKind.LandmarkTower, new Float3(0f, 0f, 52f), new Float3(8f, 15f, 8f), 0f, TownPalette.Stone, "Facade_Tower", clearanceHeight: 3.6f),
            new TownPiece("Bridge_Main", TownPieceKind.Bridge, new Float3(0f, TownSpec.DeckElevation, 38f), new Float3(6f, 0.4f, 3.2f), 0f, TownPalette.Stone, "Facade_Bridge", clearanceHeight: 3.4f),
            new TownPiece("Well_Arcane", TownPieceKind.LandmarkWell, new Float3(7.5f, 0f, 30.2f), new Float3(1.6f, 1.2f, 1.6f), 0f, TownPalette.Arcane, "Facade_Well"),
            new TownPiece("HoloArch_SE", TownPieceKind.LandmarkHoloArch, new Float3(6.6f, 0f, 10.7f), new Float3(6.4f, 4.5f, 6f), 90f, TownPalette.SciFi, "Facade_HoloArch", clearanceHeight: 3.4f),

            // Arches & Verticality
            new TownPiece("Arch_W", TownPieceKind.Arch, new Float3(-7.45f, 0f, 28.2f), new Float3(6.0f, 4.5f, 3.2f), 90f, TownPalette.Stone, "Facade_ArchW", clearanceHeight: 3.4f),
            new TownPiece("Deck_W_Floor", TownPieceKind.Deck, new Float3(-11.75f, TownSpec.DeckElevation - 0.15f, 38f), new Float3(17.5f, 0.3f, 6f), 0f, TownPalette.Wood),
            new TownPiece("Deck_E_Floor", TownPieceKind.Deck, new Float3(11.75f, TownSpec.DeckElevation - 0.15f, 38f), new Float3(17.5f, 0.3f, 6f), 0f, TownPalette.Wood),
            new TownPiece("Ramp_W", TownPieceKind.Ramp, new Float3(-19f, 1.9f, 31f), new Float3(3f, TownSpec.DeckElevation, 8f), 0f, TownPalette.Stone),
            new TownPiece("Ramp_E", TownPieceKind.Ramp, new Float3(19f, 1.9f, 31f), new Float3(3f, TownSpec.DeckElevation, 8f), 0f, TownPalette.Stone),
            new TownPiece("Railing_Bridge_N", TownPieceKind.Railing, new Float3(0f, TownSpec.DeckElevation + 0.5f, 39.5f), new Float3(6f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_Bridge_S", TownPieceKind.Railing, new Float3(0f, TownSpec.DeckElevation + 0.5f, 36.5f), new Float3(6f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckW_N", TownPieceKind.Railing, new Float3(-11.75f, TownSpec.DeckElevation + 0.5f, 40.9f), new Float3(17.5f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckW_S", TownPieceKind.Railing, new Float3(-10.25f, TownSpec.DeckElevation + 0.5f, 35.1f), new Float3(14.5f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckE_N", TownPieceKind.Railing, new Float3(11.75f, TownSpec.DeckElevation + 0.5f, 40.9f), new Float3(17.5f, 1f, 0.2f), 0f, TownPalette.Wood),
            new TownPiece("Railing_DeckE_S", TownPieceKind.Railing, new Float3(10.25f, TownSpec.DeckElevation + 0.5f, 35.1f), new Float3(14.5f, 1f, 0.2f), 0f, TownPalette.Wood),

            // Cover Buildings (frame the widened Plaza & occlude side alleys cleanly without touching Shop_BlindBox/OpenBox houses at Z=19..25)
            new TownPiece("Cover_W1", TownPieceKind.CoverBuilding, new Float3(-6.5f, 4.5f, 15.0f), new Float3(6.0f, 9f, 4.0f), 0f, TownPalette.Stone, "Facade_CoverW1"),
            new TownPiece("Cover_W2", TownPieceKind.CoverBuilding, new Float3(-6.5f, 4.0f, 32.8f), new Float3(6.0f, 8f, 3.4f), 0f, TownPalette.Moss, "Facade_CoverW2"),
            new TownPiece("Cover_E1", TownPieceKind.CoverBuilding, new Float3(6.5f, 4.5f, 15.5f), new Float3(6.0f, 9f, 3.0f), 0f, TownPalette.Stone, "Facade_CoverE1"),
            new TownPiece("Cover_E2", TownPieceKind.CoverBuilding, new Float3(6.5f, 4.0f, 33.0f), new Float3(6.0f, 8f, 3.2f), 0f, TownPalette.Moss, "Facade_CoverE2"),

            // Interspersed Secondary Buildings (add architectural rhythm & skyline variety in widened 60x68m town)
            new TownPiece("Building_Inter_W1", TownPieceKind.Building, new Float3(-21.5f, 3.5f, 20.0f), new Float3(8f, 7f, 6.0f), 0f, TownPalette.Wood, "Facade_InterW1"),
            new TownPiece("Building_Inter_W2", TownPieceKind.Building, new Float3(-24.5f, 5.5f, 32.0f), new Float3(7f, 11f, 10.0f), 0f, TownPalette.Roof, "Facade_InterW2"),
            new TownPiece("Building_Inter_W3", TownPieceKind.Building, new Float3(-22.5f, 4.5f, 46.0f), new Float3(8f, 9f, 8.0f), 0f, TownPalette.Moss, "Facade_InterW3"),
            new TownPiece("Building_Inter_E1", TownPieceKind.Building, new Float3(21.5f, 3.5f, 20.0f), new Float3(8f, 7f, 6.0f), 0f, TownPalette.Moss, "Facade_InterE1"),
            new TownPiece("Building_Inter_E2", TownPieceKind.Building, new Float3(24.5f, 5.0f, 32.0f), new Float3(7f, 10f, 10.0f), 0f, TownPalette.Wood, "Facade_InterE2"),
            new TownPiece("Building_Inter_E3", TownPieceKind.Building, new Float3(22.5f, 4.5f, 46.0f), new Float3(8f, 9f, 8.0f), 0f, TownPalette.Stone, "Facade_InterE3"),
            new TownPiece("Building_Inter_N1", TownPieceKind.Building, new Float3(-10.5f, 4.2f, 63.2f), new Float3(8f, 8.4f, 6.5f), 0f, TownPalette.Roof, "Facade_InterN1"),
            new TownPiece("Building_Inter_N2", TownPieceKind.Building, new Float3(10.5f, 4.2f, 63.2f), new Float3(8f, 8.4f, 6.5f), 0f, TownPalette.Wood, "Facade_InterN2"),

            // Inn Facade & Expanded Perimeter Buildings
            new TownPiece("Building_InnFacade", TownPieceKind.Building, new Float3(0f, 4.5f, 1.25f), new Float3(14f, 9f, 2.5f), 0f, TownPalette.Wood, "Facade_Inn"),
            new TownPiece("Wall_South_W", TownPieceKind.Wall, new Float3(-18.5f, 3.5f, 1.25f), new Float3(23f, 7f, 2.5f), 0f, TownPalette.Stone),
            new TownPiece("Wall_South_E", TownPieceKind.Wall, new Float3(18.5f, 3.5f, 1.25f), new Float3(23f, 7f, 2.5f), 0f, TownPalette.Stone),
            new TownPiece("Wall_North", TownPieceKind.Wall, new Float3(0f, 4f, 67.5f), new Float3(60f, 8f, 1.0f), 0f, TownPalette.Stone, "Facade_NorthWall"),
            new TownPiece("Building_West_S", TownPieceKind.Building, new Float3(-21.5f, 5f, 8.5f), new Float3(15f, 10f, 11f), 0f, TownPalette.Stone, "Facade_WestS"),
            new TownPiece("Wall_West_Slot03Back", TownPieceKind.Wall, new Float3(-22.5f, 4f, 15.5f), new Float3(10f, 8f, 3f), 0f, TownPalette.Stone),
            new TownPiece("Building_West_N", TownPieceKind.Building, new Float3(-22.5f, 5.5f, 56f), new Float3(11f, 11f, 12f), 0f, TownPalette.Roof, "Facade_WestN"),
            new TownPiece("Wall_West_Edge", TownPieceKind.Wall, new Float3(-29.25f, 4f, 34.5f), new Float3(1.5f, 8f, 65f), 0f, TownPalette.Stone),
            new TownPiece("Building_East_S", TownPieceKind.Building, new Float3(21.5f, 5f, 8.75f), new Float3(15f, 10f, 11.5f), 0f, TownPalette.Stone, "Facade_EastS"),
            new TownPiece("Wall_East_Slot04Back", TownPieceKind.Wall, new Float3(23.0f, 4f, 16.5f), new Float3(10f, 8f, 4f), 0f, TownPalette.Stone),
            new TownPiece("Building_East_N", TownPieceKind.Building, new Float3(22.5f, 5.5f, 56f), new Float3(11f, 11f, 12f), 0f, TownPalette.Roof, "Facade_EastN"),
            new TownPiece("Wall_East_Edge", TownPieceKind.Wall, new Float3(29.25f, 4f, 34.5f), new Float3(1.5f, 8f, 65f), 0f, TownPalette.Stone),

            // Scattered 20% Accents (21 total: 7 Steam, 7 SciFi, 7 Arcane; <= 2 per FacadeId)
            new TownPiece("Acc_Steam_01", TownPieceKind.AccentSteam, new Float3(-3.3f, 3.2f, 14f), new Float3(0.6f, 1.4f, 0.6f), 0f, TownPalette.Brass, "Facade_CoverW1"),
            new TownPiece("Acc_SciFi_01", TownPieceKind.AccentSciFi, new Float3(-3.3f, 2.8f, 16f), new Float3(1.2f, 0.6f, 0.3f), 0f, TownPalette.SciFi, "Facade_CoverW1"),
            new TownPiece("Acc_Arcane_01", TownPieceKind.AccentArcane, new Float3(-3.3f, 2.5f, 32f), new Float3(0.5f, 0.9f, 0.5f), 0f, TownPalette.Arcane, "Facade_CoverW2"),
            new TownPiece("Acc_Steam_02", TownPieceKind.AccentSteam, new Float3(-3.3f, 3.4f, 33.5f), new Float3(0.8f, 0.8f, 0.5f), 0f, TownPalette.Brass, "Facade_CoverW2"),
            new TownPiece("Acc_SciFi_02", TownPieceKind.AccentSciFi, new Float3(3.3f, 2.8f, 14.5f), new Float3(1.0f, 0.6f, 0.3f), 0f, TownPalette.SciFi, "Facade_CoverE1"),
            new TownPiece("Acc_Arcane_02", TownPieceKind.AccentArcane, new Float3(3.3f, 3.2f, 16.2f), new Float3(0.5f, 0.8f, 0.5f), 0f, TownPalette.Arcane, "Facade_CoverE1"),
            new TownPiece("Acc_Steam_03", TownPieceKind.AccentSteam, new Float3(3.3f, 3.0f, 32f), new Float3(0.7f, 1.2f, 0.5f), 0f, TownPalette.Brass, "Facade_CoverE2"),
            new TownPiece("Acc_SciFi_03", TownPieceKind.AccentSciFi, new Float3(3.3f, 2.4f, 33.8f), new Float3(0.9f, 0.5f, 0.3f), 0f, TownPalette.SciFi, "Facade_CoverE2"),
            new TownPiece("Acc_Steam_04", TownPieceKind.AccentSteam, new Float3(-2.0f, 5.0f, 48.0f), new Float3(1.2f, 1.2f, 0.5f), 0f, TownPalette.Brass, "Facade_Tower"),
            new TownPiece("Acc_Arcane_03", TownPieceKind.AccentArcane, new Float3(2.0f, 5.5f, 48.0f), new Float3(1.0f, 1.0f, 0.5f), 0f, TownPalette.Arcane, "Facade_Tower"),
            new TownPiece("Acc_Steam_05", TownPieceKind.AccentSteam, new Float3(-1.5f, 3.6f, 36.4f), new Float3(2.0f, 0.4f, 0.4f), 0f, TownPalette.Brass, "Facade_Bridge"),
            new TownPiece("Acc_SciFi_04", TownPieceKind.AccentSciFi, new Float3(1.5f, 3.6f, 36.4f), new Float3(2.0f, 0.3f, 0.3f), 0f, TownPalette.SciFi, "Facade_Bridge"),
            new TownPiece("Acc_Arcane_04", TownPieceKind.AccentArcane, new Float3(-2.5f, 3.2f, 2.5f), new Float3(0.5f, 0.9f, 0.5f), 0f, TownPalette.Arcane, "Facade_Inn"),
            new TownPiece("Acc_Steam_06", TownPieceKind.AccentSteam, new Float3(2.5f, 3.2f, 2.5f), new Float3(0.6f, 0.8f, 0.5f), 0f, TownPalette.Brass, "Facade_Inn"),
            new TownPiece("Acc_SciFi_05", TownPieceKind.AccentSciFi, new Float3(3.3f, 3.8f, 10.7f), new Float3(0.2f, 0.6f, 1.6f), 0f, TownPalette.SciFi, "Facade_HoloArch"),
            new TownPiece("Acc_Arcane_05", TownPieceKind.AccentArcane, new Float3(-6.0f, 3.5f, 28.2f), new Float3(0.5f, 0.8f, 0.5f), 0f, TownPalette.Arcane, "Facade_ArchW"),
            new TownPiece("Acc_Steam_07", TownPieceKind.AccentSteam, new Float3(-13.8f, 3.2f, 11f), new Float3(0.6f, 1.2f, 0.5f), 0f, TownPalette.Brass, "Facade_WestS"),
            new TownPiece("Acc_SciFi_06", TownPieceKind.AccentSciFi, new Float3(-17.0f, 2.8f, 54f), new Float3(0.4f, 1.0f, 1.0f), 0f, TownPalette.SciFi, "Facade_WestN"),
            new TownPiece("Acc_Arcane_06", TownPieceKind.AccentArcane, new Float3(13.8f, 3.0f, 11f), new Float3(0.5f, 0.9f, 0.5f), 0f, TownPalette.Arcane, "Facade_EastS"),
            new TownPiece("Acc_SciFi_07", TownPieceKind.AccentSciFi, new Float3(17.0f, 3.0f, 54f), new Float3(0.4f, 1.0f, 1.0f), 0f, TownPalette.SciFi, "Facade_EastN"),
            new TownPiece("Acc_Arcane_07", TownPieceKind.AccentArcane, new Float3(0f, 3.5f, 66.8f), new Float3(1.4f, 1.4f, 0.3f), 0f, TownPalette.Arcane, "Facade_NorthWall"),

            // Street Lights (grounded on walkway edges, Y = 0..3.0)
            new TownPiece("Light_Main_S", TownPieceKind.StreetLight, new Float3(-3.2f, 1.5f, 7f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Plaza_W", TownPieceKind.StreetLight, new Float3(-4.9f, 1.5f, 18f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Plaza_E", TownPieceKind.StreetLight, new Float3(4.9f, 1.5f, 18f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Main_N", TownPieceKind.StreetLight, new Float3(3.2f, 1.5f, 33f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Alley_W", TownPieceKind.StreetLight, new Float3(-13.7f, 1.5f, 30f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Alley_E", TownPieceKind.StreetLight, new Float3(13.7f, 1.5f, 30f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood),
            new TownPiece("Light_Rear", TownPieceKind.StreetLight, new Float3(0f, 1.5f, 59.7f), new Float3(0.3f, 3.0f, 0.3f), 0f, TownPalette.Wood)
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
