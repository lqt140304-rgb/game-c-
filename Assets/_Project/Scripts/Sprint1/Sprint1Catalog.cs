using System.Collections.Generic;
using ChoDoLa.Contracts;

namespace ChoDoLa.Sprint1
{
    public sealed class ArtifactTemplate
    {
        public string ItemId;
        public string DisplayName;
        public string Category;
        public int BaseValue;
        public RarityTier DefaultRarity;
        public string VisualPrefabKey;
        public string ClueAuthentic;
        public string ClueCounterfeit;
    }

    public static class Sprint1Catalog
    {
        public static readonly int[] DebtSchedule = { 200, 350, 500, 750, 1050, 1500, 2200 };

        public static List<HeirloomView> CreateHeirlooms()
        {
            return new List<HeirloomView>
            {
                new HeirloomView
                {
                    HeirloomId = "heirloom_clockwork_monocle",
                    DisplayName = "Sổ Ghi Nợ Cũ (GT1)",
                    LoreDesc = "Quyển sổ cái bìa da nâu ghi chép món nợ truyền đời của gia tộc.",
                    EffectSummary = "+12% giá bán hiện vật hệ Đồng hồ / Cơ khí & hoãn một kỳ nợ.",
                    PreferredSystem = TactSystemType.ChronoSteam
                },
                new HeirloomView
                {
                    HeirloomId = "heirloom_rift_seal",
                    DisplayName = "Đèn Dầu Thờ (GT2)",
                    LoreDesc = "Ngọn đèn dầu cổ soi thấu manh mối từ các thế giới dị biệt.",
                    EffectSummary = "+12% giá bán hiện vật Dị giới / Thần thoại & -50 xu nợ mỗi ngày.",
                    PreferredSystem = TactSystemType.ArcaneRift
                }
            };
        }

        public static List<TactArtifactView> CreateTactArtifacts()
        {
            return new List<TactArtifactView>
            {
                new TactArtifactView
                {
                    TactId = "tact_cs_t1_gear",
                    DisplayName = "Ốc Vít Dự Phòng (TC1)",
                    SystemType = TactSystemType.ChronoSteam,
                    Tier = 1,
                    Price = 120,
                    IconModelName = "Artifact_TC1",
                    EffectSummary = "+10% giá bán mọi hiện vật khi trang bị (Hệ Thiên Cơ Bậc I).",
                    EquippedSlotIndex = -1
                },
                new TactArtifactView
                {
                    TactId = "tact_cs_t2_watch",
                    DisplayName = "Mắt Máy Soi (TC2)",
                    SystemType = TactSystemType.ChronoSteam,
                    Tier = 2,
                    Price = 450,
                    IconModelName = "Artifact_TC2",
                    EffectSummary = "+18% giá bán hiện vật & +1 lượt mua hộp/ngày (Hệ Thiên Cơ Bậc II).",
                    EquippedSlotIndex = -1
                },
                new TactArtifactView
                {
                    TactId = "tact_cs_t3_bird",
                    DisplayName = "Lõi Năng Lượng (TC3)",
                    SystemType = TactSystemType.ChronoSteam,
                    Tier = 3,
                    Price = 500,
                    IconModelName = "Artifact_TC3",
                    EffectSummary = "+28% giá bán & tăng tỷ lệ ra đồ Hiếm/Thần thoại (Hệ Thiên Cơ Bậc III).",
                    EquippedSlotIndex = -1
                },
                new TactArtifactView
                {
                    TactId = "tact_ar_t1_amulet",
                    DisplayName = "Phiến Khắc Tre (CP1)",
                    SystemType = TactSystemType.ArcaneRift,
                    Tier = 1,
                    Price = 150,
                    IconModelName = "Artifact_CP1",
                    EffectSummary = "Giảm 40 xu tiền nợ mỗi ngày & +8% giá bán (Hệ Cổ Phong Bậc I).",
                    EquippedSlotIndex = -1
                },
                new TactArtifactView
                {
                    TactId = "tact_ar_t2_compass",
                    DisplayName = "Hũ Đựng Đồ (CP2)",
                    SystemType = TactSystemType.ArcaneRift,
                    Tier = 2,
                    Price = 400,
                    IconModelName = "Artifact_CP2",
                    EffectSummary = "+15% giá bán & giảm 15% tiền mua hộp mù (Hệ Cổ Phong Bậc II).",
                    EquippedSlotIndex = -1
                },
                new TactArtifactView
                {
                    TactId = "tact_ar_t3_prism",
                    DisplayName = "Ấn Triện Đồng (CP3)",
                    SystemType = TactSystemType.ArcaneRift,
                    Tier = 3,
                    Price = 500,
                    IconModelName = "Artifact_CP3",
                    EffectSummary = "+26% giá bán & giảm 90 xu tiền nợ mỗi ngày (Hệ Cổ Phong Bậc III).",
                    EquippedSlotIndex = -1
                }
            };
        }

        public static List<ComboView> CreateCombos()
        {
            return new List<ComboView>
            {
                new ComboView
                {
                    ComboId = "combo_chrono_overdrive",
                    DisplayName = "Cộng Hưởng Dây Cót (2 ChronoSteam)",
                    ConditionSummary = "Trang bị tối thiểu 2 Tạo Tác hệ ChronoSteam.",
                    EffectSummary = "Thưởng thêm +15% giá bán & +1 lượt mua hộp mỗi ngày.",
                    RequiresMixedSystems = false,
                    RequiredSystem = TactSystemType.ChronoSteam,
                    RequiredCount = 2,
                    IsActive = false
                },
                new ComboView
                {
                    ComboId = "combo_rift_convergence",
                    DisplayName = "Giao Thoa Ma Năng (2 ArcaneRift)",
                    ConditionSummary = "Trang bị tối thiểu 2 Tạo Tác hệ ArcaneRift.",
                    EffectSummary = "Giảm thêm 120 xu tiền nợ mỗi ngày & +12% giá bán.",
                    RequiresMixedSystems = false,
                    RequiredSystem = TactSystemType.ArcaneRift,
                    RequiredCount = 2,
                    IsActive = false
                },
                new ComboView
                {
                    ComboId = "combo_multiverse_grand",
                    DisplayName = "Đại Cộng Hưởng Dị Giới (Đủ 2 Hệ · 3 Ô)",
                    ConditionSummary = "Trang bị đủ 3 ô gồm cả 2 hệ ChronoSteam & ArcaneRift.",
                    EffectSummary = "Bùng nổ: +25% giá bán toàn bộ hiện vật & +80 xu khi mở hộp Hiếm+.",
                    RequiresMixedSystems = true,
                    RequiredSystem = TactSystemType.ChronoSteam,
                    RequiredCount = 3,
                    IsActive = false
                }
            };
        }

        public static List<BikheView> CreateBikheList()
        {
            return new List<BikheView>
            {
                new BikheView
                {
                    BikheId = "bikhe_box_opener",
                    Title = "Lời Thề Bảy Ngày (BK1)",
                    ConditionSummary = "Mở tổng cộng 5 hộp mù trong một ván chơi.",
                    RewardSummary = "Thưởng nóng +250 xu & mở rộng khả năng phối Tạo Tác.",
                    CurrentProgress = 0,
                    TargetProgress = 5,
                    IsUnlocked = false,
                    IsRewardClaimed = false
                },
                new BikheView
                {
                    BikheId = "bikhe_combo_smith",
                    Title = "Mắt Người Sành (BK2)",
                    ConditionSummary = "Kích hoạt ít nhất 1 Combo Tạo Tác trên 3 ô trang bị.",
                    RewardSummary = "Thưởng +200 xu & +1 lượt mua hộp/ngày vĩnh viễn.",
                    CurrentProgress = 0,
                    TargetProgress = 1,
                    IsUnlocked = false,
                    IsRewardClaimed = false
                }
            };
        }

        public static List<BoxOfferView> CreateDailyOffers()
        {
            return new List<BoxOfferView>
            {
                new BoxOfferView
                {
                    OfferId = "offer_h1_wood",
                    BoxTypeId = "H1",
                    DisplayName = "Hộp Gỗ Nhỏ",
                    Price = 100,
                    PotentialCategory = "Quá khứ",
                    ClueText = "Dì Mận: \"Sáp đỏ đã sậm màu, tem giấy in chữ mờ — hàng cổ Quá khứ (V01–V03).\"",
                    VisualVariantKey = "Box_H1"
                },
                new BoxOfferView
                {
                    OfferId = "offer_h2_metal",
                    BoxTypeId = "H2",
                    DisplayName = "Hộp Kim Loại Mờ",
                    Price = 350,
                    PotentialCategory = "Hiện đại & Tương lai",
                    ClueText = "Dì Mận: \"Titan phay xước, chạm vào hơi ấm tay — hàng Hiện đại & Tương lai (V04–V06).\"",
                    VisualVariantKey = "Box_H2"
                },
                new BoxOfferView
                {
                    OfferId = "offer_h3_cloth",
                    BoxTypeId = "H3",
                    DisplayName = "Hộp Vải Dày",
                    Price = 800,
                    PotentialCategory = "Cổ tích & Thần thoại",
                    ClueText = "Dì Mận: \"Dây buộc có mùi thuốc bắc, chỉ thêu vàng — hàng Cổ tích & Thần thoại (V08–V10).\"",
                    VisualVariantKey = "Box_H3"
                },
                new BoxOfferView
                {
                    OfferId = "offer_rd_black",
                    BoxTypeId = "RD",
                    DisplayName = "Rương Đen",
                    Price = 1500,
                    PotentialCategory = "Đa thế giới & Cực hiếm",
                    ClueText = "Dì Mận: \"Sơn mài đen viền vàng khóa đồng — có thể chứa Tinh thể lam hoặc Vảy long!\"",
                    VisualVariantKey = "Box_RD"
                },
                new BoxOfferView
                {
                    OfferId = "offer_h1_evening",
                    BoxTypeId = "H1",
                    DisplayName = "Hộp Gỗ Nhỏ (Lô Chiều)",
                    Price = 100,
                    PotentialCategory = "Quá khứ",
                    ClueText = "Dì Mận: \"Kiện gỗ sồi góc đồng quai thừng, dễ ra Bình sứ men xanh hoặc Đồng hồ cũ.\"",
                    VisualVariantKey = "Box_H1"
                },
                new BoxOfferView
                {
                    OfferId = "offer_h2_port",
                    BoxTypeId = "H2",
                    DisplayName = "Hộp Kim Loại Mờ (Lô Cảng)",
                    Price = 350,
                    PotentialCategory = "Hiện đại & Tương lai",
                    ClueText = "Dì Mận: \"Hộp rỗng tiếng kim loại vang, có cơ hội mở trúng Bộ pin lượng tử hiếm.\"",
                    VisualVariantKey = "Box_H2"
                }
            };
        }

        public static IReadOnlyList<ArtifactTemplate> GetPoolForBox(string boxTypeId)
        {
            switch (boxTypeId)
            {
                case "H1":
                case "box_wood_old":
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "V01",
                            DisplayName = "Bình sứ men xanh",
                            Category = "Quá khứ",
                            BaseValue = 230,
                            DefaultRarity = RarityTier.Common,
                            VisualPrefabKey = "Item_V01",
                            ClueAuthentic = "Vết rạn men tự nhiên gần miệng, dấu triện khắc sắc dưới đáy.",
                            ClueCounterfeit = "Nước men bóng công nghiệp quá đều, triện đáy nông mờ."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V02",
                            DisplayName = "Sách cổ bìa da",
                            Category = "Quá khứ",
                            BaseValue = 385,
                            DefaultRarity = RarityTier.Common,
                            VisualPrefabKey = "Item_V02",
                            ClueAuthentic = "Chữ mờ theo năm tháng nhưng còn nguyên mục lục, gáy khâu chỉ tay.",
                            ClueCounterfeit = "Giấy nhuộm màu cà phê mới, gáy dán keo hiện đại."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V03",
                            DisplayName = "Đồng hồ quả lắc cũ",
                            Category = "Quá khứ · Đồng hồ",
                            BaseValue = 495,
                            DefaultRarity = RarityTier.Uncommon,
                            VisualPrefabKey = "Item_V03",
                            ClueAuthentic = "Quả lắc đồng đặc, mặt kính ố vàng đều có vết xước thời gian.",
                            ClueCounterfeit = "Vỏ gỗ ép nhẹ tay, quả lắc đúc rỗng không ăn nhịp."
                        }
                    };

                case "H2":
                case "box_brass_steam":
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "V04",
                            DisplayName = "Mảnh kính đồng hồ thiên văn",
                            Category = "Hiện đại · Đồng hồ",
                            BaseValue = 415,
                            DefaultRarity = RarityTier.Common,
                            VisualPrefabKey = "Item_V04",
                            ClueAuthentic = "Khung kim loại khắc số chuẩn xác, mép kính cắt mài tự nhiên.",
                            ClueCounterfeit = "Số in sơn mờ nhòe, kính nhựa nhẹ dễ trầy."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V05",
                            DisplayName = "Thiết bị liên lạc hỏng",
                            Category = "Tương lai · Cơ khí",
                            BaseValue = 310,
                            DefaultRarity = RarityTier.Common,
                            VisualPrefabKey = "Item_V05",
                            ClueAuthentic = "Dù màn hình vỡ nhưng mã máy khắc laser vẫn đọc rõ nét.",
                            ClueCounterfeit = "Vỏ nhựa rỗng ruột, tem mã dán lệch lạc."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V06",
                            DisplayName = "Bộ pin lượng tử",
                            Category = "Tương lai · Cơ khí",
                            BaseValue = 1845,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "Item_V06",
                            ClueAuthentic = "Linh kiện bạc bên trong còn sáng, lõi cyan phát quang ổn định.",
                            ClueCounterfeit = "Nhãn in lại thô, ánh xanh chỉ là đèn LED gắn vội."
                        }
                    };

                case "H3":
                case "box_runic_arcane":
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "V08",
                            DisplayName = "Gương thủy tinh cổ tích",
                            Category = "Cổ tích",
                            BaseValue = 1590,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "Item_V08",
                            ClueAuthentic = "Ảnh phản chiếu hơi lệch góc so với người soi, lưng khảm đồng tối.",
                            ClueCounterfeit = "Mặt gương phẳng bình thường, khung đúc nhựa giả cổ."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V09",
                            DisplayName = "Lá bùa giấy vàng",
                            Category = "Thần thoại",
                            BaseValue = 895,
                            DefaultRarity = RarityTier.Common,
                            VisualPrefabKey = "Item_V09",
                            ClueAuthentic = "Giấy dó dai thớ dài, mực chu sa thấm sâu từng nét bút.",
                            ClueCounterfeit = "Mực đỏ dễ phai lem nhẹ, dấu hiệu hàng làm giả trôi nổi."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V10",
                            DisplayName = "Mảnh vảy long",
                            Category = "Thần thoại",
                            BaseValue = 2860,
                            DefaultRarity = RarityTier.Mythic,
                            VisualPrefabKey = "Item_V10",
                            ClueAuthentic = "Vết cắt sắc lẹm, ánh kim đổi màu rõ rệt theo từng góc nhìn.",
                            ClueCounterfeit = "Sơn nhũ phủ bề mặt, góc nhìn nghiêng xỉn màu."
                        }
                    };

                default: // RD / box_holo_scifi
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "V07",
                            DisplayName = "Tinh thể lam cầm tay",
                            Category = "Ngoài hành tinh",
                            BaseValue = 2695,
                            DefaultRarity = RarityTier.Mythic,
                            VisualPrefabKey = "Item_V07",
                            ClueAuthentic = "Phát sáng dịu khi xoay nhẹ dưới đèn, bề mặt có vân xoắn tự nhiên.",
                            ClueCounterfeit = "Thủy tinh nhuộm màu nặng tay, vân bên trong là bọt khí đúc."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V10",
                            DisplayName = "Mảnh vảy long",
                            Category = "Thần thoại",
                            BaseValue = 2860,
                            DefaultRarity = RarityTier.Mythic,
                            VisualPrefabKey = "Item_V10",
                            ClueAuthentic = "Ánh kim đổi màu đa tầng và mép vảy cứng như thép rèn cổ.",
                            ClueCounterfeit = "Mảnh sừng nhuộm màu phủ sơn bóng."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "V08",
                            DisplayName = "Gương thủy tinh cổ tích",
                            Category = "Cổ tích",
                            BaseValue = 1590,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "Item_V08",
                            ClueAuthentic = "Mặt kính sâu thẳm phản chiếu ánh sáng mờ ảo kỳ lạ.",
                            ClueCounterfeit = "Kính tráng thủy thường cắt viền máy."
                        }
                    };
            }
        }
    }
}
