using System;
using System.Collections.Generic;

namespace ChoDoLa.Contracts
{
    public enum QualityTier
    {
        Damaged,
        Worn,
        Good,
        Pristine
    }

    public enum AuthenticityState
    {
        Unverified,
        Authentic,
        Counterfeit
    }

    public enum RarityTier
    {
        Common,
        Uncommon,
        Rare,
        Mythic
    }

    public enum AppraisalState
    {
        Unappraised,
        QuickChecked,
        Appraised
    }

    [Serializable]
    public sealed class ActiveEventView
    {
        public string EventId;
        public string Title;
        public string Description;
    }

    public enum TactSystemType
    {
        ChronoSteam,
        ArcaneRift
    }

    [Serializable]
    public sealed class HeirloomView
    {
        public string HeirloomId;
        public string DisplayName;
        public string LoreDesc;
        public string EffectSummary;
        public TactSystemType PreferredSystem;
    }

    [Serializable]
    public sealed class TactArtifactView
    {
        public string TactId;
        public string DisplayName;
        public TactSystemType SystemType;
        public int Tier;
        public int Price;
        public string IconModelName;
        public string EffectSummary;
        public bool IsOwned;
        public bool IsEquipped;
        public int EquippedSlotIndex;
    }

    [Serializable]
    public sealed class ComboView
    {
        public string ComboId;
        public IReadOnlyList<string> MissingTactIds;
        public string DisplayName;
        public string ConditionSummary;
        public string EffectSummary;
        public bool RequiresMixedSystems;
        public TactSystemType RequiredSystem;
        public int RequiredCount;
        public bool IsActive;
    }

    [Serializable]
    public sealed class BikheView
    {
        public string BikheId;
        public string Title;
        public string ConditionSummary;
        public string RewardSummary;
        public int CurrentProgress;
        public int TargetProgress;
        public bool IsUnlocked;
        public bool IsRewardClaimed;
    }

    [Serializable]
    public sealed class RunStateSnapshot
    {
        public int CurrentDay;
        public int MaxDays;
        public int Cash;
        public int DailyBoxesPurchased;
        public int MaxDailyBoxes;
        public int DebtDueToday;
        public int Arrears;
        public int CarryDebt;
        public bool PostponeAvailable;
        public bool IsDebtPaidToday;
        public bool IsRunEnded;
        public bool IsVictory;
        public string EndReason;
        [NonSerialized] public IReadOnlyList<ActiveEventView> ActiveEvents;

        public string ActiveHeirloomId;
        public string ActiveHeirloomName;
        public int EquippedTactCount;
        public int MaxEquipSlots;
        public int ActiveComboCount;
        public int UnlockedBikheCount;
        public int TotalBikheCount;
        public int InventoryTotalValue;
        public bool HasSavedRun;
    }

    [Serializable]
    public sealed class BoxOfferView
    {
        public string OfferId;
        public string BoxTypeId;
        public string DisplayName;
        public int Price;
        public int TodayPrice;
        public decimal FakeRate;
        public string CertainOrigin;
        public string PotentialCategory;
        public string ClueText;
        public string VisualVariantKey;
    }

    [Serializable]
    public sealed class BoxInstanceView
    {
        public string BoxInstanceId;
        public string OfferId;
        public string BoxTypeId;
        public string DisplayName;
        public int PricePaid;
        public string VisualVariantKey;
        public string ClueText;
        public bool IsOpened;
    }

    [Serializable]
    public sealed class ArtifactInstanceView
    {
        public string InstanceId;
        public string ItemId;
        public string DisplayName;
        public string Category;
        public QualityTier Quality;
        public AuthenticityState Authenticity;
        public RarityTier Rarity;
        public int BaseValue;
        public int CurrentSellPrice;
        public AppraisalState AppraisalState;
        public string VisualPrefabKey;
        public string InspectClueText;
        public bool IsKeptInInventory;
        public bool IsSold;
    }

    /// <summary>
    /// Data contract between 3D/UI presentation (Antigravity) and gameplay/economy (Codex).
    /// </summary>
    public interface IMarketGameService
    {
        RunStateSnapshot CurrentRunState { get; }
        IReadOnlyList<BoxOfferView> CurrentOffers { get; }
        IReadOnlyList<BoxInstanceView> UnopenedBoxes { get; }
        IReadOnlyList<ArtifactInstanceView> InventoryItems { get; }
        int[] DebtSchedule { get; }

        IReadOnlyList<HeirloomView> AvailableHeirlooms { get; }
        HeirloomView ActiveHeirloom { get; }
        IReadOnlyList<TactArtifactView> TactCatalog { get; }
        IReadOnlyList<TactArtifactView> EquippedArtifacts { get; }
        IReadOnlyList<ComboView> ActiveCombos { get; }
        IReadOnlyList<BikheView> BikheProgressList { get; }

        event Action<RunStateSnapshot> OnRunStateChanged;
        event Action<BoxInstanceView> OnBoxPurchased;
        event Action<BoxInstanceView, ArtifactInstanceView> OnBoxOpened;
        event Action OnInventoryChanged;
        event Action<string, string> OnActionRejected;
        event Action<string> OnNotice;

        bool RequestBuyBox(string offerId);
        bool RequestOpenBox(string boxInstanceId);
        bool RequestSellArtifact(string artifactInstanceId);
        bool RequestKeepArtifact(string artifactInstanceId);
        bool RequestEndDay(int expectedDay, bool postpone = false);
        bool RequestBuyClue(string offerId);
        void ResetRun();

        bool RequestSelectHeirloom(string heirloomId);
        bool RequestBuyTactArtifact(string tactId);
        bool RequestEquipTactArtifact(string tactId, int slotIndex = -1);
        bool RequestUnequipTactArtifact(int slotIndex);
    }
}
