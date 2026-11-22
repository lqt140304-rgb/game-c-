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

    [Serializable]
    public sealed class RunStateSnapshot
    {
        public int CurrentDay;
        public int MaxDays;
        public int Cash;
        public int DailyBoxesPurchased;
        public int MaxDailyBoxes;
        public int DebtDueToday;
        public bool IsDebtPaidToday;
        public bool IsRunEnded;
        public bool IsVictory;
        public string EndReason;
        public IReadOnlyList<ActiveEventView> ActiveEvents;
    }

    [Serializable]
    public sealed class BoxOfferView
    {
        public string OfferId;
        public string BoxTypeId;
        public string DisplayName;
        public int Price;
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

        event Action<RunStateSnapshot> OnRunStateChanged;
        event Action<BoxInstanceView> OnBoxPurchased;
        event Action<BoxInstanceView, ArtifactInstanceView> OnBoxOpened;
        event Action OnInventoryChanged;
        event Action<bool, int> OnDebtSettled;
        event Action<string, string> OnActionRejected;
        event Action<string> OnNotice;

        bool RequestBuyBox(string offerId);
        bool RequestOpenBox(string boxInstanceId);
        bool RequestSellArtifact(string artifactInstanceId);
        bool RequestKeepArtifact(string artifactInstanceId);
        bool RequestPayDebt();
        bool RequestEndDay();
        void ResetRun();
    }
}
