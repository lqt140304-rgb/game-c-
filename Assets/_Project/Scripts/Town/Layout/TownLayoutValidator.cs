using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoDoLa.Town.Layout
{
    public static class TownLayoutValidator
    {
        public static IReadOnlyList<string> Validate(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways)
        {
            var errors = new List<string>();
            CheckBounds(pieces, walkways, errors);
            CheckUniqueIds(pieces, walkways, errors);
            CheckSlots(pieces, walkways, errors);
            CheckWalkClear(pieces, walkways, errors);
            CheckBoundary(walkways, errors);
            CheckRamps(pieces, errors);
            CheckClearance(pieces, errors);
            CheckAccents(pieces, errors);
            CheckWidth(walkways, errors);
            CheckConnectivityAndLoops(pieces, walkways, errors);
            return errors;
        }

        private static void CheckBounds(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            foreach (var p in pieces)
            {
                if (!TownSpec.TownBounds.Contains(p.Footprint))
                {
                    errors.Add($"[BOUNDS] Piece '{p.Id}' footprint exceeds town bounds.");
                }
            }
            foreach (var w in walkways)
            {
                if (!TownSpec.TownBounds.Contains(w.Area))
                {
                    errors.Add($"[BOUNDS] Walkway '{w.Id}' exceeds town bounds.");
                }
            }
        }

        private static void CheckUniqueIds(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            var seen = new HashSet<string>();
            foreach (var p in pieces)
            {
                if (!seen.Add(p.Id)) errors.Add($"[UNIQUE_ID] Duplicate piece Id '{p.Id}'.");
            }
            foreach (var w in walkways)
            {
                if (!seen.Add(w.Id)) errors.Add($"[UNIQUE_ID] Duplicate walkway Id '{w.Id}'.");
            }
        }

        private static void CheckSlots(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            var slots = pieces.Where(p => p.Kind == TownPieceKind.ShopSlot).ToList();
            if (slots.Count != 8)
            {
                errors.Add($"[SLOTS] Expected 8 ShopSlots, found {slots.Count}.");
            }

            var expectedLarge = new HashSet<string> { "ShopSlot_04", "ShopSlot_06", "ShopSlot_08" };
            var expectedUpper = new HashSet<string> { "ShopSlot_06", "ShopSlot_08" };

            for (int i = 1; i <= 8; i++)
            {
                string id = $"ShopSlot_{i:00}";
                var slot = slots.FirstOrDefault(s => s.Id == id);
                if (slot == null)
                {
                    errors.Add($"[SLOTS] Missing '{id}'.");
                    continue;
                }

                float ew = expectedLarge.Contains(id) ? TownSpec.LargeSlotWidth : TownSpec.SmallSlotWidth;
                float ed = expectedLarge.Contains(id) ? TownSpec.LargeSlotDepth : TownSpec.SmallSlotDepth;
                if (Math.Abs(slot.Size.X - ew) > 0.01f || Math.Abs(slot.Size.Z - ed) > 0.01f)
                {
                    errors.Add($"[SLOTS] '{id}' size {slot.Size.X}x{slot.Size.Z} does not match expected {ew}x{ed}.");
                }

                float expectedY = expectedUpper.Contains(id) ? TownSpec.DeckElevation : 0f;
                if (Math.Abs(slot.Center.Y - expectedY) > TownSpec.GapTolerance)
                {
                    errors.Add($"[SLOTS] '{id}' elevation {slot.Center.Y} does not match expected {expectedY}.");
                }

                bool hasAccess = walkways.Any(w =>
                    Math.Abs(w.Elevation - slot.Center.Y) <= TownSpec.GapTolerance &&
                    FrontEdgeTouchesWalkway(slot, w.Area));
                if (!hasAccess)
                {
                    errors.Add($"[SLOT_ACCESS] '{id}' front edge does not touch any walkway at elevation {slot.Center.Y}.");
                }

                if (slot.Center.Y > TownSpec.GapTolerance)
                {
                    bool supported = walkways.Any(w =>
                        Math.Abs(w.Elevation - slot.Center.Y) <= TownSpec.GapTolerance &&
                        w.Area.Contains(slot.Footprint));
                    if (!supported)
                    {
                        errors.Add($"[SLOT_SUPPORT] Upper-floor '{id}' footprint overhangs supporting deck.");
                    }
                }
            }

            var markers = pieces.Where(p => p.IsMarker).ToList();
            var solids = pieces.Where(p =>
                p.Kind == TownPieceKind.Building ||
                p.Kind == TownPieceKind.CoverBuilding ||
                p.Kind == TownPieceKind.Wall).ToList();

            foreach (var m in markers)
            {
                foreach (var s in solids)
                {
                    float solidBottom = s.Center.Y - s.Size.Y * 0.5f;
                    float solidTop = s.Center.Y + s.Size.Y * 0.5f;
                    if (m.Center.Y >= solidBottom - TownSpec.GapTolerance &&
                        m.Center.Y < solidTop - TownSpec.GapTolerance &&
                        m.Footprint.Overlaps(s.Footprint))
                    {
                        errors.Add($"[SLOT_CLEAR] Marker '{m.Id}' overlaps solid '{s.Id}'.");
                    }
                }
            }
        }

        private static bool FrontEdgeTouchesWalkway(TownPiece slot, Rect2 area)
        {
            int yaw = (((int)Math.Round(slot.YawDegrees) % 360) + 360) % 360;
            float halfDepth = slot.Size.Z * 0.5f;
            float fx = slot.Center.X;
            float fz = slot.Center.Z;
            if (yaw == 0) fz += halfDepth;
            else if (yaw == 180) fz -= halfDepth;
            else if (yaw == 90) fx += halfDepth;
            else if (yaw == 270) fx -= halfDepth;

            float tol = TownSpec.GapTolerance;
            return fx >= area.MinX - tol && fx <= area.MaxX + tol &&
                   fz >= area.MinZ - tol && fz <= area.MaxZ + tol;
        }

        private static void CheckWalkClear(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            var groundWalkways = walkways.Where(w => Math.Abs(w.Elevation) <= TownSpec.GapTolerance).ToList();
            foreach (var p in pieces)
            {
                if (p.IsMarker) continue;
                if (p.Kind == TownPieceKind.Ground ||
                    p.Kind == TownPieceKind.Deck ||
                    p.Kind == TownPieceKind.Ramp ||
                    p.Kind == TownPieceKind.Bridge ||
                    p.Kind == TownPieceKind.AccentSteam ||
                    p.Kind == TownPieceKind.AccentSciFi ||
                    p.Kind == TownPieceKind.AccentArcane)
                {
                    continue;
                }

                if (p.ClearanceHeight >= TownSpec.MinClearance &&
                    (p.Kind == TownPieceKind.Arch ||
                     p.Kind == TownPieceKind.LandmarkTower ||
                     p.Kind == TownPieceKind.LandmarkHoloArch))
                {
                    foreach (var pillar in GetArchPillarFootprints(p))
                    {
                        foreach (var w in groundWalkways)
                        {
                            if (pillar.Overlaps(w.Area))
                            {
                                errors.Add($"[WALK_CLEAR] Pillar of '{p.Id}' overlaps ground walkway '{w.Id}'.");
                            }
                        }
                    }
                    continue;
                }

                float bottomY = p.Kind == TownPieceKind.LandmarkWell
                    ? p.Center.Y
                    : p.Center.Y - p.Size.Y * 0.5f;
                if (bottomY >= TownSpec.MinClearance) continue;

                foreach (var w in groundWalkways)
                {
                    if (p.Footprint.Overlaps(w.Area))
                    {
                        errors.Add($"[WALK_CLEAR] Solid piece '{p.Id}' overlaps ground walkway '{w.Id}'.");
                    }
                }
            }
        }

        private static IEnumerable<Rect2> GetArchPillarFootprints(TownPiece p)
        {
            float openingWidth = Math.Min(4.0f, p.Size.X * 0.55f);
            float pillarWidth = (p.Size.X - openingWidth) * 0.5f;
            float offset = openingWidth * 0.5f + pillarWidth * 0.5f;
            int yaw = (((int)Math.Round(p.YawDegrees) % 360) + 360) % 360;
            bool swap = yaw == 90 || yaw == 270;

            if (!swap)
            {
                yield return Rect2.FromCenter(p.Center.X - offset, p.Center.Z, pillarWidth, p.Size.Z);
                yield return Rect2.FromCenter(p.Center.X + offset, p.Center.Z, pillarWidth, p.Size.Z);
            }
            else
            {
                yield return Rect2.FromCenter(p.Center.X, p.Center.Z - offset, p.Size.Z, pillarWidth);
                yield return Rect2.FromCenter(p.Center.X, p.Center.Z + offset, p.Size.Z, pillarWidth);
            }
        }

        private static void CheckBoundary(IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            float tol = TownSpec.GapTolerance;
            foreach (var w in walkways)
            {
                if (w.Area.MinX <= TownSpec.BoundsMinX + tol ||
                    w.Area.MaxX >= TownSpec.BoundsMaxX - tol ||
                    w.Area.MinZ <= TownSpec.BoundsMinZ + tol ||
                    w.Area.MaxZ >= TownSpec.BoundsMaxZ - tol)
                {
                    errors.Add($"[BOUNDARY] Walkway '{w.Id}' touches town boundary.");
                }
            }
        }

        private static void CheckRamps(IReadOnlyList<TownPiece> pieces, List<string> errors)
        {
            foreach (var p in pieces.Where(x => x.Kind == TownPieceKind.Ramp))
            {
                if (p.RampDegrees > TownSpec.MaxRampDegrees)
                {
                    errors.Add($"[RAMP] '{p.Id}' slope {p.RampDegrees:F1}° exceeds {TownSpec.MaxRampDegrees}°.");
                }
                if (Math.Abs(p.RampBottomY) > TownSpec.GapTolerance)
                {
                    errors.Add($"[RAMP] '{p.Id}' bottom Y={p.RampBottomY:F2} does not meet ground 0.");
                }
                if (Math.Abs(p.RampTopY - TownSpec.DeckElevation) > TownSpec.GapTolerance)
                {
                    errors.Add($"[RAMP] '{p.Id}' top Y={p.RampTopY:F2} does not meet deck {TownSpec.DeckElevation}.");
                }
            }
        }

        private static void CheckClearance(IReadOnlyList<TownPiece> pieces, List<string> errors)
        {
            foreach (var p in pieces)
            {
                if (p.ClearanceHeight > 0f && p.ClearanceHeight < TownSpec.MinClearance)
                {
                    errors.Add($"[CLEARANCE] '{p.Id}' clearance {p.ClearanceHeight} < {TownSpec.MinClearance}.");
                }
                if (p.Kind == TownPieceKind.Bridge)
                {
                    float bottomY = p.Center.Y - p.Size.Y;
                    if (bottomY < TownSpec.MinClearance)
                    {
                        errors.Add($"[CLEARANCE] Bridge '{p.Id}' underside {bottomY:F2} < {TownSpec.MinClearance}.");
                    }
                }
                else if (p.Kind == TownPieceKind.Deck)
                {
                    float bottomY = p.Center.Y - p.Size.Y * 0.5f;
                    if (bottomY < TownSpec.MinClearance)
                    {
                        errors.Add($"[CLEARANCE] Deck '{p.Id}' underside {bottomY:F2} < {TownSpec.MinClearance}.");
                    }
                }
            }
        }

        private static void CheckAccents(IReadOnlyList<TownPiece> pieces, List<string> errors)
        {
            var counts = new Dictionary<string, int>();
            foreach (var p in pieces)
            {
                if (p.Kind != TownPieceKind.AccentSteam &&
                    p.Kind != TownPieceKind.AccentSciFi &&
                    p.Kind != TownPieceKind.AccentArcane)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(p.FacadeId)) continue;
                counts.TryGetValue(p.FacadeId, out int c);
                counts[p.FacadeId] = c + 1;
            }
            foreach (var kv in counts)
            {
                if (kv.Value > TownSpec.MaxAccentsPerFacade)
                {
                    errors.Add($"[ACCENTS] Facade '{kv.Key}' has {kv.Value} accents (max {TownSpec.MaxAccentsPerFacade}).");
                }
            }
        }

        private static void CheckWidth(IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            foreach (var w in walkways)
            {
                if (w.Id.StartsWith("Alley_"))
                {
                    float s = w.Area.ShortSide;
                    if (s < TownSpec.AlleyMinWidth - 0.01f || s > TownSpec.AlleyMaxWidth + 0.01f)
                    {
                        errors.Add($"[WIDTH] Alley '{w.Id}' short side {s:F2}m outside [{TownSpec.AlleyMinWidth}, {TownSpec.AlleyMaxWidth}].");
                    }
                }
                else if (w.Id.StartsWith("MainStreet_"))
                {
                    float s = w.Area.ShortSide;
                    if (Math.Abs(s - TownSpec.MainStreetWidth) > 0.01f)
                    {
                        errors.Add($"[WIDTH] Main street '{w.Id}' width {s:F2}m != {TownSpec.MainStreetWidth}.");
                    }
                }
            }
        }

        private static void CheckConnectivityAndLoops(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways, List<string> errors)
        {
            if (walkways.Count == 0)
            {
                errors.Add("[CONNECTED] No walkways defined.");
                return;
            }

            int n = walkways.Count;
            var adj = new List<int>[n];
            for (int i = 0; i < n; i++) adj[i] = new List<int>();

            float tol = TownSpec.GapTolerance;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(walkways[i].Elevation - walkways[j].Elevation) <= tol &&
                        walkways[i].Area.Touches(walkways[j].Area, tol))
                    {
                        adj[i].Add(j);
                        adj[j].Add(i);
                    }
                }
            }

            foreach (var r in pieces.Where(p => p.Kind == TownPieceKind.Ramp))
            {
                var lowIdx = new List<int>();
                var highIdx = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    if (!walkways[i].Area.Touches(r.Footprint, tol)) continue;
                    if (Math.Abs(walkways[i].Elevation) <= tol) lowIdx.Add(i);
                    else if (Math.Abs(walkways[i].Elevation - TownSpec.DeckElevation) <= tol) highIdx.Add(i);
                }
                foreach (int l in lowIdx)
                {
                    foreach (int h in highIdx)
                    {
                        if (!adj[l].Contains(h)) { adj[l].Add(h); adj[h].Add(l); }
                    }
                }
            }

            var spawn = pieces.FirstOrDefault(p => p.Kind == TownPieceKind.InnSpawn);
            int startIdx = spawn == null ? 0 : walkways.ToList().FindIndex(w =>
                Math.Abs(w.Elevation - spawn.Center.Y) <= tol &&
                (w.Area.Touches(spawn.Footprint, tol) || w.Area.Overlaps(spawn.Footprint)));
            if (startIdx < 0)
            {
                errors.Add("[CONNECTED] InnSpawn does not touch any walkway.");
                startIdx = 0;
            }

            var visited = Bfs(adj, startIdx, -1, -1);
            if (visited.Count < n)
            {
                for (int i = 0; i < n; i++)
                {
                    if (!visited.Contains(i))
                    {
                        errors.Add($"[CONNECTED] Walkway '{walkways[i].Id}' is unreachable from spawn.");
                    }
                }
            }

            int towerIdx = walkways.ToList().FindIndex(w => w.Id.IndexOf("TowerArch", StringComparison.OrdinalIgnoreCase) >= 0);
            if (towerIdx < 0 || !NodeIsOnCycle(adj, towerIdx))
            {
                errors.Add("[LOOP] No loop passes through TowerArch walkway.");
            }

            int bridgeIdx = walkways.ToList().FindIndex(w =>
                w.Id.IndexOf("Bridge", StringComparison.OrdinalIgnoreCase) >= 0 &&
                Math.Abs(w.Elevation - TownSpec.DeckElevation) <= tol);
            if (bridgeIdx < 0 || !NodeIsOnCycle(adj, bridgeIdx))
            {
                errors.Add("[LOOP] No upper-floor loop passes through Bridge walkway.");
            }
        }

        private static bool NodeIsOnCycle(List<int>[] adj, int node)
        {
            foreach (int neighbor in adj[node])
            {
                var visited = Bfs(adj, node, node, neighbor);
                if (visited.Contains(neighbor)) return true;
            }
            return false;
        }

        private static HashSet<int> Bfs(List<int>[] adj, int start, int skipU, int skipV)
        {
            var visited = new HashSet<int> { start };
            var queue = new Queue<int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                foreach (int v in adj[u])
                {
                    if ((u == skipU && v == skipV) || (u == skipV && v == skipU)) continue;
                    if (visited.Add(v)) queue.Enqueue(v);
                }
            }
            return visited;
        }
    }
}
