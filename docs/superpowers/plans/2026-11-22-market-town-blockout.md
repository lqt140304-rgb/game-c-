# Market Town Greybox Blockout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Sinh tự động scene `Market_Town.unity` — thị trấn phố đá châu Âu dạng greybox, có che khuất, tầng 2, cầu vượt, 4 điểm nhấn và 8 ô cửa hàng — bằng một menu Editor trong Unity 6 (6000.6.9f1) URP.

**Architecture:** Toàn bộ bố cục là **dữ liệu C# thuần** (không phụ thuộc `UnityEngine`) gồm danh sách khối (`TownPiece`) và các lối đi (`Walkway`), kèm bộ kiểm tra luật (`TownLayoutValidator`) chạy được bằng `dotnet test` ngay trong VM không có Unity. Phía Unity chỉ có một lớp mỏng: chuyển dữ liệu thành Prefab greybox theo loại khối, đặt vào scene, gắn ánh sáng và tái sử dụng Player/HUD của Sprint 0.

**Tech Stack:** Unity 6 (`6000.6.9f1`), URP, C# 9 (Unity) / .NET 8 + NUnit 3 (test ngoài Unity), Unity Editor API (`PrefabUtility`, `EditorSceneManager`).

**Spec:** [docs/superpowers/specs/2026-11-22-market-town-design.md](../specs/2026-11-22-market-town-design.md)

## Global Constraints

- Unity `6000.6.9f1`, URP; không thêm package ngoài [manifest.json](../../../Packages/manifest.json) hiện có.
- Code layout thuần nằm trong asmdef `ChoDoLa.Town.Layout` với `"noEngineReferences": true`, chỉ dùng C# 9 (không `record struct`, không file-scoped namespace) để Unity biên dịch được.
- Hệ trục: `+X` = Đông, `+Z` = Bắc, `+Y` = lên, đơn vị mét. Gốc `(0,0,0)` ở mép Nam giữa trục phố.
- Giới hạn thị trấn: `X ∈ [-22.5, 22.5]`, `Z ∈ [0, 55]` (45m × 55m).
- Phố chính rộng `6.0m` (`X ∈ [-3, 3]`); hẻm/ngõ phụ rộng `3.2m – 3.5m`.
- Cao độ tầng 2 và mặt cầu vượt: `+3.8m`. Thông thủy dưới mọi vòm/cầu/cổng: `≥ 3.2m`.
- Dốc lên tầng 2: góc `≤ 28°`; dùng **mặt dốc liền** (không bậc rời) để `CharacterController` (`stepOffset 0.35`) không bị kẹt.
- Ô cửa hàng: đúng 8 ô `ShopSlot_01`…`ShopSlot_08`, kích thước theo bảng ở mục 5.1b của spec (`4×3m` hoặc `5×4m`).
- Mỗi mặt tiền (`FacadeId`) mang tối đa 2 khối chi tiết dị giới (`Accent*`).
- Thư mục đầu ra: Prefab ở `Assets/_Project/Prefabs/Town/{Architecture,Verticality,Landmarks,AccentSockets,Slots}/`, scene ở `Assets/_Project/Scenes/Market_Town.unity`.
- Bảng màu greybox: đá `#8A8A85`, rêu `#6E7560`, gỗ `#5A4130`, ngói `#8C3F2E`, đồng `#B8862B`, sci-fi `#5FA8D3`, cổ tự `#8E6CCF`, marker `#3CCB5A` alpha `0.35`.

## Review Focus

1. **Marker chặn đường:** Người chơi đi xuyên được mọi ô `ShopSlot`, `Inn_Spawn`, `OpenTable_Zone` — marker không có collider rắn. (Test ở Task 4: `Markers_HaveNoSolidCollider`; kiểm tra Play Mode ở Task 6.)
2. **Kẹt ở đầu/chân dốc:** Mép trên dốc trùng khít cao độ `3.8m` của sàn tầng 2 và mép dưới chạm `y = 0`, không có khe hở > `0.05m`. (Test ở Task 2: `Ramps_MeetDeckAndGroundWithoutGap`.)
3. **Rơi ra ngoài bản đồ:** Toàn bộ chu vi có tường/nhà chặn, không có lối đi chạm biên. (Test ở Task 2: `Walkways_DoNotTouchTownBoundary`.)
4. **Chạy menu lần hai bị nhân đôi:** Chạy lại menu ghi đè scene và Prefab, không tạo bản sao `(1)`. (Test ở Task 4: `Build_IsIdempotent`.)
5. **Camera xuyên trần vòm thấp:** Đầu người chơi (`1.8m`) cộng biên an toàn vẫn dưới mọi vòm `≥ 3.2m`. (Test ở Task 2: `Overheads_HaveMinimumClearance`.)

---

## File Structure

| File | Trách nhiệm |
| --- | --- |
| `Assets/_Project/Scripts/Town/Layout/ChoDoLa.Town.Layout.asmdef` | Assembly thuần, không tham chiếu engine |
| `Assets/_Project/Scripts/Town/Layout/TownTypes.cs` | `Float3`, `Rect2`, `TownPieceKind`, `TownPalette`, `TownPiece`, `Walkway` |
| `Assets/_Project/Scripts/Town/Layout/TownSpec.cs` | Hằng số đo đạc lấy từ spec |
| `Assets/_Project/Scripts/Town/Layout/TownLayout.cs` | Dữ liệu bố cục: `Pieces`, `Walkways` |
| `Assets/_Project/Scripts/Town/Layout/TownLayoutValidator.cs` | Kiểm tra luật, trả danh sách vi phạm |
| `Assets/_Project/Scripts/Town/ShopSlotMarker.cs` | MonoBehaviour gắn lên marker ô cửa hàng (Id, kích thước) |
| `Assets/_Project/Editor/Town/TownPrefabFactory.cs` | Tạo/ghi đè Prefab greybox theo `TownPieceKind` |
| `Assets/_Project/Editor/Town/TownBlockoutBuilder.cs` | Menu `Tools/Cho Do La/Build Market Town Blockout` |
| `Assets/_Project/Editor/Sprint0SceneSetup.cs` | Sửa: mở `CreatePlayer`, `CreateHud` thành `internal static` để tái dùng |
| `Tests~/TownLayout.Tests/TownLayout.Tests.csproj` | Dự án NUnit .NET 8, link các file trong `Town/Layout/*.cs` (thư mục `~` Unity bỏ qua) |
| `Tests~/TownLayout.Tests/*.cs` | Test bố cục chạy bằng `dotnet test` |

---

### Task 1: Kiểu dữ liệu, hằng số và khung test .NET

**Files:**
- Create: `Assets/_Project/Scripts/Town/Layout/ChoDoLa.Town.Layout.asmdef`
- Create: `Assets/_Project/Scripts/Town/Layout/TownTypes.cs`
- Create: `Assets/_Project/Scripts/Town/Layout/TownSpec.cs`
- Create: `Tests~/TownLayout.Tests/TownLayout.Tests.csproj`
- Test: `Tests~/TownLayout.Tests/TownTypesTests.cs`

**Interfaces:**
- Produces (namespace `ChoDoLa.Town.Layout`):
  - `readonly struct Float3 { float X, Y, Z; Float3(float x, float y, float z) }`
  - `readonly struct Rect2 { float MinX, MinZ, MaxX, MaxZ; static Rect2 FromCenter(float cx, float cz, float w, float d); bool Overlaps(Rect2 o); bool Touches(Rect2 o, float tol); bool Contains(Rect2 o) }` — `Overlaps` là giao có diện tích > 0; `Touches` là chạm cạnh hoặc giao trong dung sai `tol`.
  - `enum TownPieceKind { Ground, Building, CoverBuilding, Wall, Arch, Deck, Ramp, Bridge, Railing, LandmarkTower, LandmarkWell, LandmarkHoloArch, AccentSteam, AccentSciFi, AccentArcane, ShopSlot, InnSpawn, OpenTableZone, StreetLight }`
  - `enum TownPalette { Stone, Moss, Wood, Roof, Brass, SciFi, Arcane, Marker }`
  - `sealed class TownPiece { string Id; TownPieceKind Kind; Float3 Center; Float3 Size; float YawDegrees; TownPalette Palette; string FacadeId; float ClearanceHeight; bool IsMarker => Kind is ShopSlot or InnSpawn or OpenTableZone; Rect2 Footprint }` — `Size` theo trục cục bộ (`X` = rộng mặt tiền, `Z` = sâu); `YawDegrees ∈ {0, 90, 180, 270}`; `Footprint` tính theo yaw. `ClearanceHeight` chỉ dùng cho `Arch`, `Bridge`, `LandmarkTower`, `LandmarkHoloArch` (độ cao gầm thông thủy), các loại khác là `0`. Với `Ramp`, `Center.Y` là tâm hình học của khối nghiêng, `Size.Z` là chiều dài theo phương ngang, cao độ chân/đỉnh lấy từ `RampBottomY`, `RampTopY`.
  - `sealed class Walkway { string Id; Rect2 Area; float Elevation }`
  - `static class TownSpec` với: `BoundsMinX=-22.5f, BoundsMaxX=22.5f, BoundsMinZ=0f, BoundsMaxZ=55f, MainStreetWidth=6f, AlleyMinWidth=3.2f, AlleyMaxWidth=3.5f, DeckElevation=3.8f, MinClearance=3.2f, MaxRampDegrees=28f, MaxAccentsPerFacade=2, SmallSlotWidth=4f, SmallSlotDepth=3f, LargeSlotWidth=5f, LargeSlotDepth=4f, PlayerHeight=1.8f, GapTolerance=0.05f`.

- [ ] **Step 1: Tạo `TownLayout.Tests.csproj`** (`net8.0`, `NUnit` 3.14, `NUnit3TestAdapter` 4.5, `Microsoft.NET.Test.Sdk` 17.9; `<Compile Include="../../Assets/_Project/Scripts/Town/Layout/**/*.cs" />`; `LangVersion 9.0`).

- [ ] **Step 2: Viết test lỗi**

```csharp
[Test] public void Rect2_FromCenter_ComputesEdges() {
    var r = Rect2.FromCenter(0f, 22f, 4f, 4f);
    Assert.That(new[]{r.MinX, r.MinZ, r.MaxX, r.MaxZ}, Is.EqualTo(new[]{-2f, 20f, 2f, 24f}));
}
[Test] public void Rect2_Overlaps_FalseWhenOnlyEdgesTouch() {
    Assert.That(Rect2.FromCenter(0,0,2,2).Overlaps(Rect2.FromCenter(2,0,2,2)), Is.False);
    Assert.That(Rect2.FromCenter(0,0,2,2).Touches(Rect2.FromCenter(2,0,2,2), 0.05f), Is.True);
}
[Test] public void TownPiece_Footprint_SwapsAxesAt90Degrees() {
    var p = new TownPiece("s", TownPieceKind.ShopSlot, new Float3(10,0,20), new Float3(4,0.1f,3), 90f, TownPalette.Marker);
    Assert.That(p.Footprint.MaxX - p.Footprint.MinX, Is.EqualTo(3f).Within(1e-4));
    Assert.That(p.Footprint.MaxZ - p.Footprint.MinZ, Is.EqualTo(4f).Within(1e-4));
}
[Test] public void TownPiece_IsMarker_OnlyForMarkerKinds() {
    Assert.That(Make(TownPieceKind.ShopSlot).IsMarker, Is.True);
    Assert.That(Make(TownPieceKind.Building).IsMarker, Is.False);
}
```

- [ ] **Step 3: Chạy test, xác nhận lỗi**

Run: `dotnet test Tests~/TownLayout.Tests`
Expected: FAIL — lỗi biên dịch `Rect2` / `TownPiece` không tồn tại.

- [ ] **Step 4: Viết `TownTypes.cs`, `TownSpec.cs`, asmdef** theo Interfaces ở trên. Thêm thuộc tính tính toán cho `Ramp`: `float RampBottomY`, `float RampTopY`, `float RampDegrees` (lấy từ `Center.Y`, `Size.Y` = độ cao leo, `Size.Z` = chiều dài ngang).

- [ ] **Step 5: Chạy test, xác nhận đạt**

Run: `dotnet test Tests~/TownLayout.Tests`
Expected: PASS (4 tests).

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Town/Layout Tests~/TownLayout.Tests
git commit -m "feat(town): add pure layout types and spec constants"
```

---

### Task 2: Bộ kiểm tra luật bố cục (`TownLayoutValidator`)

**Files:**
- Create: `Assets/_Project/Scripts/Town/Layout/TownLayoutValidator.cs`
- Test: `Tests~/TownLayout.Tests/TownLayoutValidatorTests.cs`

**Interfaces:**
- Consumes: kiểu ở Task 1.
- Produces: `static class TownLayoutValidator { IReadOnlyList<string> Validate(IReadOnlyList<TownPiece> pieces, IReadOnlyList<Walkway> walkways) }` — trả danh sách rỗng khi hợp lệ; mỗi vi phạm là chuỗi bắt đầu bằng mã luật trong ngoặc vuông dưới đây.

Luật (mỗi luật một mã):
- `[BOUNDS]` mọi `Footprint` nằm trong giới hạn thị trấn.
- `[UNIQUE_ID]` không trùng `Id`.
- `[SLOTS]` đúng 8 `ShopSlot` với Id `ShopSlot_01..08`, kích thước khớp bảng spec (01,02,03,05,07 = `4×3`; 04,06,08 = `5×4`), 06 và 08 có `Center.Y == DeckElevation`.
- `[SLOT_ACCESS]` cạnh mặt tiền của mỗi ô (`Center` + nửa sâu theo hướng yaw) chạm một `Walkway` cùng cao độ.
- `[WALK_CLEAR]` không khối rắn nào (`!IsMarker`, không phải `Ground/Deck/Ramp/Bridge/Accent*/StreetLight`, và không phải khối có `ClearanceHeight ≥ MinClearance`) có `Footprint.Overlaps` một walkway mặt đất.
- `[BOUNDARY]` không walkway nào chạm biên thị trấn (dung sai `GapTolerance`).
- `[CONNECTED]` đồ thị walkway (nối khi `Touches` và cùng cao độ, hoặc qua một `Ramp` có chân chạm walkway thấp và đỉnh chạm walkway cao) liên thông từ walkway chứa `InnSpawn`.
- `[LOOP]` tồn tại chu trình đi qua walkway gầm tháp (`Id` chứa `TowerArch`) và một chu trình đi qua walkway `Bridge` ở tầng 2.
- `[RAMP]` mọi `Ramp`: `RampDegrees ≤ 28`, `|RampBottomY − 0| ≤ GapTolerance`, `|RampTopY − DeckElevation| ≤ GapTolerance`.
- `[CLEARANCE]` mọi khối có `ClearanceHeight > 0` thì `ClearanceHeight ≥ MinClearance`.
- `[ACCENTS]` mỗi `FacadeId` có ≤ 2 khối `Accent*`.
- `[WIDTH]` walkway có Id bắt đầu bằng `Alley_` rộng (cạnh ngắn) trong `[3.2, 3.5]`; walkway `MainStreet_*` rộng đúng `6.0`.

- [ ] **Step 1: Viết test lỗi** — mỗi luật một test dùng bố cục tối giản hợp lệ (helper `MinimalValidLayout()` dựng trong file test) rồi làm hỏng một điểm:

```csharp
[Test] public void MinimalLayout_IsValid() =>
    Assert.That(TownLayoutValidator.Validate(L.Pieces, L.Walkways), Is.Empty);
[Test] public void Slots_WrongSizeFor04_Reported() => AssertRule(Break.SlotSize("ShopSlot_04", 4f, 3f), "[SLOTS]");
[Test] public void Ramps_MeetDeckAndGroundWithoutGap() => AssertRule(Break.RampTopY(3.6f), "[RAMP]");
[Test] public void Ramp_Steeper_Than28_Reported() => AssertRule(Break.RampLength(6.5f), "[RAMP]"); // atan(3.8/6.5)=30.3°
[Test] public void Walkways_DoNotTouchTownBoundary() => AssertRule(Break.WalkwayTo(22.5f), "[BOUNDARY]");
[Test] public void Overheads_HaveMinimumClearance() => AssertRule(Break.ArchClearance(3.0f), "[CLEARANCE]");
[Test] public void Building_OnWalkway_Reported() => AssertRule(Break.BuildingOnStreet(), "[WALK_CLEAR]");
[Test] public void Marker_OnWalkway_Allowed() => Assert.That(Validate(Add.SlotOnStreet()), Is.Empty);
[Test] public void ThreeAccents_OnOneFacade_Reported() => AssertRule(Add.Accents("Facade_W1", 3), "[ACCENTS]");
[Test] public void Disconnected_Walkway_Reported() => AssertRule(Add.IsolatedWalkway(), "[CONNECTED]");
[Test] public void Alley_Width3_Reported() => AssertRule(Break.AlleyWidth(3.0f), "[WIDTH]");
```

- [ ] **Step 2: Chạy test, xác nhận lỗi**

Run: `dotnet test Tests~/TownLayout.Tests --filter TownLayoutValidatorTests`
Expected: FAIL — `TownLayoutValidator` không tồn tại.

- [ ] **Step 3: Viết `TownLayoutValidator.Validate`** — một hàm private cho mỗi luật; `[LOOP]` dùng kiểm tra cạnh cầu nối: một cạnh nằm trên chu trình khi bỏ cạnh đó đi đồ thị vẫn liên thông giữa hai đầu (BFS).

- [ ] **Step 4: Chạy test, xác nhận đạt**

Run: `dotnet test Tests~/TownLayout.Tests`
Expected: PASS (toàn bộ).

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/Town/Layout/TownLayoutValidator.cs Tests~/TownLayout.Tests
git commit -m "feat(town): add layout rule validator"
```

---

### Task 3: Dữ liệu bố cục thị trấn (`TownLayout`)

**Files:**
- Create: `Assets/_Project/Scripts/Town/Layout/TownLayout.cs`
- Test: `Tests~/TownLayout.Tests/TownLayoutDataTests.cs`

**Interfaces:**
- Consumes: Task 1, Task 2.
- Produces: `static class TownLayout { IReadOnlyList<TownPiece> Pieces; IReadOnlyList<Walkway> Walkways; TownPiece Spawn => Pieces.Single(p => p.Kind == InnSpawn) }`.

Mốc cố định (tâm `x, y, z`; kích thước `rộng × sâu`; yaw quay mặt tiền về lối đi):

| Id | Kind | Tâm | Kích thước | Yaw | Ghi chú |
| --- | --- | --- | --- | --- | --- |
| `Inn_Spawn` | InnSpawn | (0, 0, 4) | 4 × 3 | 0 | Hiên trọ ngoài trời, mặt tiền nhà trọ đóng ở `Z ∈ [0, 2.5]` |
| `MainStreet_S` | Walkway | `X[-3,3] Z[2.5,17]` | — | — | Trục phố Nam |
| `Plaza` | Walkway | `X[-6,6] Z[17,27]` | — | — | Sân trung tâm |
| `OpenTable_Zone` | OpenTableZone | (0, 0, 22) | 4 × 4 | 0 | Đặt lại quầy + bàn Sprint 0 ở đây |
| `MainStreet_N` | Walkway | `X[-3,3] Z[27,43]` | — | — | Trục phố Bắc, đi dưới cầu vượt |
| `Bridge_Main` | Bridge | (0, 3.8, 36) | 6 × 3.2 | 0 | Thông thủy 3.4 |
| `Tower_Main` | LandmarkTower | (0, 0, 47) | 8 × 8, cao 15 | 0 | Vòm xuyên `X[-2,2]` thông thủy 3.6 |
| `TowerArch` | Walkway | `X[-2,2] Z[43,51]` | — | — | Lối đi xuyên tháp |
| `RearLane` | Walkway | `X[-12.5,12.5] Z[51,54.5]` | — | — | Nối vòng sau tháp sang hai hẻm |
| `Alley_W` | Walkway | `X[-12.5,-9] Z[8,54.5]` | — | — | Hẻm Tây chạy suốt |
| `Alley_E` | Walkway | `X[9,12.5] Z[8,54.5]` | — | — | Hẻm Đông chạy suốt |
| `Arch_W` | Arch | (-7.5, 0, 19.75) | 3 × 3.5 | 90 | Hẻm vòm Tây nối sân → `Alley_W`, thông thủy 3.4 |
| `Lane_Well` | Walkway | `X[6,9] Z[19,25]` | — | — | Ngõ rẽ Đông |
| `Well_Arcane` | LandmarkWell | (7.5, 0, 24) | 1.6 × 1.6 | 0 | Điểm nhấn phụ 2 |
| `HoloArch_SE` | LandmarkHoloArch | (6, 0, 10.75) | 6 × 3.5 | 0 | Lối tắt Đông Nam `X[3,9] Z[9,12.5]`, thông thủy 3.4 |
| `Deck_W` / `Deck_E` | Deck + Walkway | `X[-16.5,-3] / X[3,16.5]`, `Z[33,39]`, y 3.8 | — | — | Sân hiên tầng 2 |
| `Ramp_W` / `Ramp_E` | Ramp | (∓14.75, 1.9, 29) | 3 × 8, leo 3.8 | 0 | Chân `Z=25`, đỉnh `Z=33`, góc 25.4° |
| `ShopSlot_01` | ShopSlot | (-4.5, 0, 9) | 4 × 3 | 90 | Mặt tiền quay Đông |
| `ShopSlot_02` | ShopSlot | (4.5, 0, 9) | 4 × 3 | 270 | Quay Tây |
| `ShopSlot_03` | ShopSlot | (-14, 0, 18) | 4 × 3 | 90 | Hẻm vòm Tây |
| `ShopSlot_04` | ShopSlot | (14.5, 0, 22) | 5 × 4 | 270 | Ngõ Giếng |
| `ShopSlot_05` | ShopSlot | (-14, 0, 31) | 4 × 3 | 90 | Ngõ khuất Tây (không đè `Ramp_W`, xem ghi chú) |
| `ShopSlot_06` | ShopSlot | (9.5, 3.8, 37) | 5 × 4 | 270 | Tầng 2 Đông |
| `ShopSlot_07` | ShopSlot | (-14, 0, 48) | 4 × 3 | 90 | Tây Bắc sau tháp |
| `ShopSlot_08` | ShopSlot | (-9.5, 3.8, 37) | 5 × 4 | 90 | Tầng 2 Tây |

Ghi chú cho người thực hiện: `Ramp_W` chạy trong dải `X[-16.25,-13.25]`, trùng dải của `ShopSlot_03/05/07`; dời `Ramp_W`/`Ramp_E` ra mép ngoài `X ±[16.5,19.5]` hoặc dời ô sang cạnh đối diện của hẻm — chọn một phương án miễn mọi luật validator đạt và bảng trên chỉ đổi vị trí, không đổi kích thước/tầng. Phần không gian còn lại trong giới hạn được lấp bằng `Building`/`CoverBuilding` cao 7–11m (mái `Roof`), riêng các khối mặt tiền chắn tầm nhìn từ trục chính vào `Alley_W/E` dùng `CoverBuilding`. Rải `Accent*` lên mặt tiền: tổng 18–24 khối, chia đều 3 loại, không quá 2 khối/mặt tiền. `StreetLight` mỗi 8–10m dọc mọi walkway mặt đất.

- [ ] **Step 1: Viết test lỗi**

```csharp
[Test] public void TownLayout_PassesAllRules() =>
    Assert.That(TownLayoutValidator.Validate(TownLayout.Pieces, TownLayout.Walkways), Is.Empty);
[Test] public void TownLayout_HasFourLandmarks() =>
    Assert.That(TownLayout.Pieces.Count(p => p.Kind is LandmarkTower or LandmarkWell or LandmarkHoloArch) + TownLayout.Pieces.Count(p => p.Kind == Bridge), Is.EqualTo(4));
[Test] public void TownLayout_TowerIsTallestPiece() =>
    Assert.That(TownLayout.Pieces.Max(p => p.Size.Y), Is.EqualTo(TownLayout.Pieces.Single(p => p.Kind == LandmarkTower).Size.Y));
[Test] public void TownLayout_AccentCountBetween18And24_EachKindAtLeast6() { ... }
[Test] public void TownLayout_CoverBuildingsExist_OnBothSidesOfMainStreet() =>
    Assert.That(Cover(x => x < 0).Any() && Cover(x => x > 0).Any(), Is.True);
[Test] public void TownLayout_SpawnIsInnPorch() =>
    Assert.That((TownLayout.Spawn.Center.X, TownLayout.Spawn.Center.Z), Is.EqualTo((0f, 4f)));
```

- [ ] **Step 2: Chạy test, xác nhận lỗi**

Run: `dotnet test Tests~/TownLayout.Tests --filter TownLayoutDataTests`
Expected: FAIL — `TownLayout` không tồn tại.

- [ ] **Step 3: Viết `TownLayout.cs`** — khởi tạo tĩnh hai danh sách theo bảng và ghi chú trên.

- [ ] **Step 4: Chạy test, lặp sửa dữ liệu tới khi đạt**

Run: `dotnet test Tests~/TownLayout.Tests`
Expected: PASS (toàn bộ); thông báo vi phạm của validator chỉ rõ khối cần dời.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/Town/Layout/TownLayout.cs Tests~/TownLayout.Tests
git commit -m "feat(town): add market town layout data"
```

---

### Task 4: Prefab greybox và trình dựng scene trong Unity

**Files:**
- Create: `Assets/_Project/Scripts/Town/ShopSlotMarker.cs`
- Create: `Assets/_Project/Editor/Town/TownPrefabFactory.cs`
- Create: `Assets/_Project/Editor/Town/TownBlockoutBuilder.cs`
- Modify: `Assets/_Project/Editor/Sprint0SceneSetup.cs` (`CreateHud`, `CreatePlayer` → `internal static`)
- Test: `Assets/_Project/Editor/Town/Tests/TownBlockoutBuilderTests.cs` (+ asmdef `ChoDoLa.Town.Editor.Tests`, Editor-only, tham chiếu `UnityEngine.TestRunner`, `UnityEditor.TestRunner`)

**Interfaces:**
- Consumes: `TownLayout.Pieces`, `TownLayout.Spawn`, `TownPieceKind`, `TownPalette`; `Sprint0SceneSetup.CreateHud() : Sprint0HudView`, `Sprint0SceneSetup.CreatePlayer(Sprint0HudView)`.
- Produces:
  - `public class ShopSlotMarker : MonoBehaviour { public string SlotId; public Vector2 SizeMeters; public bool IsUpperFloor; }`
  - `static class TownPrefabFactory { GameObject GetOrCreate(TownPieceKind kind, TownPalette palette) }` — một Prefab khối đơn vị `1×1×1` cho mỗi cặp (loại, màu) tại thư mục con theo bảng: Architecture = `Ground, Building, CoverBuilding, Wall, Arch, Railing, StreetLight`; Verticality = `Deck, Ramp, Bridge`; Landmarks = `Landmark*`; AccentSockets = `Accent*`; Slots = marker. Vật liệu URP Lit lưu ở `Assets/_Project/Art/Greybox/Materials/GB_<Palette>.mat`; marker dùng chế độ Transparent.
  - `static class TownBlockoutBuilder { [MenuItem("Tools/Cho Do La/Build Market Town Blockout")] void Build(); const string ScenePath = "Assets/_Project/Scenes/Market_Town.unity" }`.

Quy tắc dựng: mỗi `TownPiece` → instance Prefab, `localScale = Size`, `rotation = Euler(0, Yaw, 0)`, parent theo `Kind`. `Ramp` xoay thêm quanh trục X một góc `RampDegrees` và scale `Z` theo chiều dài nghiêng. `Arch`, `LandmarkTower`, `LandmarkHoloArch` dựng bằng 2 trụ + 1 lanh tô theo `ClearanceHeight` (không đặt khối đặc chặn lối). Marker: `Collider` bị xoá, thêm `ShopSlotMarker` cho `ShopSlot`. Thêm Directional Light `#FFE2B8` cường độ 1.2, góc `(50, -30, 0)`; Point Light vàng ấm tại mỗi `StreetLight`. Gọi `CreateHud`, `CreatePlayer`, đặt Player tại `Spawn.Center + (0, 1, 0)` quay hướng Bắc. Đặt lại `Counter_NiemPhong_Placeholder`, `Table_OpenBox_Placeholder`, `TestBox_Placeholder` vào `OpenTable_Zone`. Lưu scene, thêm vào Build Settings không trùng.

- [ ] **Step 1: Viết test Editor lỗi**

```csharp
[Test] public void Build_CreatesSceneWithEightSlotMarkers() {
    TownBlockoutBuilder.Build();
    Assert.That(Object.FindObjectsByType<ShopSlotMarker>(FindObjectsSortMode.None).Length, Is.EqualTo(8));
}
[Test] public void Markers_HaveNoSolidCollider() {
    TownBlockoutBuilder.Build();
    foreach (var m in Object.FindObjectsByType<ShopSlotMarker>(FindObjectsSortMode.None))
        Assert.That(m.GetComponentsInChildren<Collider>().All(c => c.isTrigger), Is.True);
}
[Test] public void Build_IsIdempotent() {
    TownBlockoutBuilder.Build(); TownBlockoutBuilder.Build();
    Assert.That(AssetDatabase.FindAssets("t:Prefab (1)", new[]{"Assets/_Project/Prefabs/Town"}), Is.Empty);
    Assert.That(Object.FindObjectsByType<ShopSlotMarker>(FindObjectsSortMode.None).Length, Is.EqualTo(8));
}
[Test] public void Build_PlacesPlayerAtInnPorch() {
    TownBlockoutBuilder.Build();
    var p = GameObject.Find("Player").transform.position;
    Assert.That(new Vector2(p.x, p.z), Is.EqualTo(new Vector2(0f, 4f)));
}
```

- [ ] **Step 2: Chạy test, xác nhận lỗi** — *cần máy có Unity 6000.6.9f1*.

Run: `<Unity> -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/town-editmode.xml`
Expected: FAIL — `TownBlockoutBuilder` không tồn tại. Trong VM không có Unity: bỏ qua bước chạy, ghi rõ trong báo cáo task rằng test Editor chưa chạy.

- [ ] **Step 3: Viết `ShopSlotMarker`, `TownPrefabFactory`, `TownBlockoutBuilder`; sửa `Sprint0SceneSetup`** theo Interfaces và quy tắc dựng.

- [ ] **Step 4: Kiểm tra biên dịch tối thiểu trong VM** — `dotnet test Tests~/TownLayout.Tests` vẫn PASS (đảm bảo không vỡ assembly thuần); rà thủ công rằng mọi API Unity dùng tồn tại ở Unity 6 (`FindObjectsByType`, `PrefabUtility.SaveAsPrefabAsset`, `EditorSceneManager.SaveScene`).

- [ ] **Step 5: Chạy test Editor, xác nhận đạt** — *trên máy có Unity*. Expected: PASS 4/4.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Town/ShopSlotMarker.cs Assets/_Project/Editor
git commit -m "feat(town): build greybox prefabs and Market_Town scene from layout"
```

---

### Task 5: Tài liệu và cập nhật checklist

**Files:**
- Modify: `Docs/Architecture/DataContract.md` (thêm mục "Thị trấn: ô cửa hàng" — Codex đọc `ShopSlotMarker.SlotId` để gắn NPC/dịch vụ sau này)
- Modify: `README.md` (thêm liên kết spec thị trấn)

- [ ] **Step 1: Viết mục tài liệu** gồm: menu dựng, hệ trục, bảng 8 ô (Id, vị trí, kích thước, tầng), cách thay mesh: mở Prefab trong `Prefabs/Town/...`, thay `MeshFilter` bằng mesh Blender/Meshy, giữ pivot tâm khối và kích thước đơn vị `1×1×1`.

- [ ] **Step 2: Kiểm tra liên kết** — Run: `grep -o "](\.\{0,2\}[^)]*)" README.md Docs/Architecture/DataContract.md` và xác nhận mọi đường dẫn tương đối tồn tại.

- [ ] **Step 3: Commit**

```bash
git add Docs/Architecture/DataContract.md README.md
git commit -m "docs(town): document market town blockout and shop slots"
```

---

### Task 6: Nghiệm thu Play Mode (chủ dự án, trên máy có Unity)

Không viết code. Danh sách kiểm tra gửi kèm báo cáo cuối:

- [ ] Menu `Tools → Cho Do La → Build Market Town Blockout` chạy không lỗi Console.
- [ ] Từ hiên trọ, nhìn thẳng thấy tháp; **không** thấy trực tiếp `ShopSlot_03/05/07` và ngõ Giếng (bị nhà che).
- [ ] Đi một vòng: hiên trọ → sân → hẻm vòm Tây → hẻm Tây → sau tháp → xuyên vòm tháp → trục Bắc → sân, không gặp ngõ cụt.
- [ ] Lên dốc Tây → sân hiên Tây → cầu vượt → sân hiên Đông → xuống dốc Đông mà không kẹt hoặc giật camera.
- [ ] Đi xuyên qua mọi marker xanh; không rơi ra khỏi bản đồ ở bất kỳ mép nào.
- [ ] Thời gian chạy từ hiên trọ đến sau tháp khoảng 10–15 giây.
