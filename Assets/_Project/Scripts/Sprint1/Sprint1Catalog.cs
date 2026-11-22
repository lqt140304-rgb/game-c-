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

        public static List<BoxOfferView> CreateDailyOffers()
        {
            return new List<BoxOfferView>
            {
                new BoxOfferView
                {
                    OfferId = "offer_wood_crate",
                    BoxTypeId = "box_wood_old",
                    DisplayName = "Hộp Gỗ Sồi Cũ",
                    Price = 100,
                    PotentialCategory = "Cổ vật & Di tích",
                    ClueText = "Dì Mận: \"Mùi gỗ mục và sáp ong, bên trong nhẹ tay — dễ là đồ cổ nhỏ hoặc trang sức cũ.\"",
                    VisualVariantKey = "WoodCrate_Old"
                },
                new BoxOfferView
                {
                    OfferId = "offer_brass_chest",
                    BoxTypeId = "box_brass_steam",
                    DisplayName = "Rương Đồng Bít Chì",
                    Price = 350,
                    PotentialCategory = "Cơ khí Hơi nước",
                    ClueText = "Dì Mận: \"Nặng trịch, lắc nghe tiếng bánh răng lạch cạch. Chốt đồng còn nguyên dấu xưởng cũ.\"",
                    VisualVariantKey = "BrassChest_Steam"
                },
                new BoxOfferView
                {
                    OfferId = "offer_arcane_casket",
                    BoxTypeId = "box_runic_arcane",
                    DisplayName = "Hộp Ấn Ký Cổ Tự",
                    Price = 550,
                    PotentialCategory = "Thần thoại & Cổ tự",
                    ClueText = "Dì Mận: \"Phù văn tím vẫn âm ỉ sáng khi trời sẩm tối. Hàng hiếm từ ngõ Giếng Cổ Tự.\"",
                    VisualVariantKey = "RunicBox_Arcane"
                },
                new BoxOfferView
                {
                    OfferId = "offer_holo_pod",
                    BoxTypeId = "box_holo_scifi",
                    DisplayName = "Kiện Nhựa Chân Không",
                    Price = 800,
                    PotentialCategory = "Dị giới & Tương lai",
                    ClueText = "Dì Mận: \"Vỏ hợp kim lạnh buốt, dải đèn xanh chưa tắt. Có thể là lõi năng lượng hoặc khối ký ức.\"",
                    VisualVariantKey = "HoloPod_SciFi"
                }
            };
        }

        public static IReadOnlyList<ArtifactTemplate> GetPoolForBox(string boxTypeId)
        {
            switch (boxTypeId)
            {
                case "box_wood_old":
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "art_amber_fossil",
                            DisplayName = "Hổ Phách Hóa Thạch",
                            Category = "Cổ vật",
                            BaseValue = 160,
                            DefaultRarity = RarityTier.Common,
                            VisualPrefabKey = "FossilAmber_Ancient",
                            ClueAuthentic = "Vân nhựa cây tự nhiên, lõi côn trùng cổ nguyên vẹn dưới ánh đèn.",
                            ClueCounterfeit = "Bọt khí tròn đều bất thường — dấu hiệu nhựa đúc công nghiệp."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "art_jade_amulet",
                            DisplayName = "Bùa Ngọc Bích Khắc Chữ",
                            Category = "Cổ vật",
                            BaseValue = 220,
                            DefaultRarity = RarityTier.Uncommon,
                            VisualPrefabKey = "Amulet_Jade",
                            ClueAuthentic = "Nét khắc tay sâu, viền vàng ngả màu thời gian tự nhiên.",
                            ClueCounterfeit = "Màu xanh quá gắt và đường mài máy sắc cạnh."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "art_royal_chalice",
                            DisplayName = "Chén Lễ Hoàng Gia",
                            Category = "Cổ vật",
                            BaseValue = 340,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "Chalice_Royal",
                            ClueAuthentic = "Đế chén có dấu triện thợ kim hoàn thế kỷ XVII và ngọc nạm thật.",
                            ClueCounterfeit = "Lớp mạ vàng mỏng bong nhẹ ở chân đế, thân rỗng bất thường."
                        }
                    };

                case "box_brass_steam":
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "art_pocket_watch",
                            DisplayName = "Đồng Hồ Quả Quýt Đồng",
                            Category = "Hơi nước",
                            BaseValue = 420,
                            DefaultRarity = RarityTier.Uncommon,
                            VisualPrefabKey = "PocketWatch_Brass",
                            ClueAuthentic = "Kim giây chạy đều nhịp, mặt men trắng không rạn chân chim.",
                            ClueCounterfeit = "Mặt số in lệch tâm, vỏ đồng pha kẽm nhẹ tay."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "art_gear_core",
                            DisplayName = "Lõi Bánh Răng Hơi Nước",
                            Category = "Hơi nước",
                            BaseValue = 520,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "GearCore_Steam",
                            ClueAuthentic = "Ba tầng bánh răng ăn khớp mượt, lõi nhiệt còn giữ ánh hồng.",
                            ClueCounterfeit = "Răng cưa đúc thô, trục giữa bị kẹt cứng."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "art_automaton_bird",
                            DisplayName = "Chim Máy Dây Cót",
                            Category = "Hơi nước",
                            BaseValue = 680,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "AutomatonBird_Brass",
                            ClueAuthentic = "Cánh đồng xếp lớp tinh xảo, mắt lam ngọc phản quang sắc nét.",
                            ClueCounterfeit = "Khớp cánh hàn thô, mắt kính thủy tinh đục."
                        }
                    };

                case "box_runic_arcane":
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "art_starlight_compass",
                            DisplayName = "La Bàn Ánh Sao",
                            Category = "Cổ tự",
                            BaseValue = 640,
                            DefaultRarity = RarityTier.Uncommon,
                            VisualPrefabKey = "Compass_Starlight",
                            ClueAuthentic = "Kim sao tự xoay nhẹ về hướng Bắc bất kể góc nghiêng.",
                            ClueCounterfeit = "Kim bị dính keo cố định, vòng chia độ khắc sai hướng."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "art_runic_mask",
                            DisplayName = "Mặt Nạ Cổ Tự Tím",
                            Category = "Thần thoại",
                            BaseValue = 860,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "RunicMask_Arcane",
                            ClueAuthentic = "Khe mắt phát sáng nhịp nhàng, ấn ký trán liền mạch đá phiến.",
                            ClueCounterfeit = "Sơn dạ quang phủ ngoài, rãnh khắc nông và không có từ trường."
                        }
                    };

                default: // box_holo_scifi
                    return new[]
                    {
                        new ArtifactTemplate
                        {
                            ItemId = "art_holo_cube",
                            DisplayName = "Khối Ký Ức Quang Ảnh",
                            Category = "Tương lai",
                            BaseValue = 980,
                            DefaultRarity = RarityTier.Rare,
                            VisualPrefabKey = "HoloCube_SciFi",
                            ClueAuthentic = "Lõi tinh thể lơ lửng ổn định, khung hợp kim không một vết hàn.",
                            ClueCounterfeit = "Lõi gắn trục nhựa trong suốt, ánh sáng nhấp nháy yếu."
                        },
                        new ArtifactTemplate
                        {
                            ItemId = "art_void_prism",
                            DisplayName = "Lăng Kính Hư Không",
                            Category = "Ngoài hành tinh",
                            BaseValue = 1350,
                            DefaultRarity = RarityTier.Mythic,
                            VisualPrefabKey = "VoidPrism_Alien",
                            ClueAuthentic = "Vòng năng lượng quay ngược chiều nhau, bề mặt đá đen hút sáng.",
                            ClueCounterfeit = "Thủy tinh nhuộm đen nặng tay, vòng ngoài là dây đồng sơn bạc."
                        }
                    };
            }
        }
    }
}
