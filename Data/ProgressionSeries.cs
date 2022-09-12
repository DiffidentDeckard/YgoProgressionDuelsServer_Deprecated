using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents a single series of BoosterPack openings, Deck creations, and Duels/Tourney between any number of Duelists
    /// </summary>
    [Index(nameof(HostId), nameof(Name), IsUnique = true)]
    public class ProgressionSeries
    {
        [Key]
        public Guid ProgressionSeriesId { get; set; }

        [ForeignKey(nameof(Host))]
        public Guid HostId { get; set; }

        // The user that created this progression series, and who will be administering it
        [InverseProperty(nameof(ApplicationUser.HostedSeries))]
        public ApplicationUser Host { get; set; }

        // The name of this progressio nseries
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; }

        // The date this progression series was created
        [DataType(DataType.Date)]
        public DateTime DateStarted { get; set; }

        // The list of duelists, where a duelist represents a user who is participating in this progression series
        [InverseProperty(nameof(Duelist.Series))]
        public ICollection<Duelist> Duelists { get; set; } = new List<Duelist>();

        public BanListFormat BanListFormat { get; set; } = BanListFormat.None;

        [ForeignKey(nameof(CurrentBanList))]
        public Guid? CurrentBanListId { get; set; }

        public bool AutoSetCurrentBanList { get; set; } = true;

        public BanList CurrentBanList { get; set; }

        [ForeignKey(nameof(CurrentBoosterPack))]
        public Guid? CurrentBoosterPackId { get; set; }

        public BoosterPackInfo CurrentBoosterPack { get; set; }

        // Can duelists buy booster packs with their star chips?
        public bool AllowPurchaseBoosterPacks { get; set; } = false;

        // How much would it cost for a single booster pack?
        public uint BoosterPackPrice { get; set; } = 100;

        // Can duelists buy structure decks with their star chips?
        public bool AllowPurchaseStructureDecks { get; set; } = false;

        // How much would it cost for a structure deck?
        public uint StructureDeckPrice { get; set; } = 300;

        // Can duelists buy special product with their star chips?
        public bool AllowPurchaseSpecialProducts { get; set; } = false;

        // How much would it cost, per card, for a special product?
        public uint SpecialProductPricePerCard { get; set; } = 30;

        public TournamentStructure NextTournamentStructure { get; set; }

        public TournamentBye NextTournamentBye { get; set; }

        public SortedSet<Tournament> Tournaments { get; set; } = new SortedSet<Tournament>();

        public override bool Equals(object obj)
        {
            return ProgressionSeriesId.Equals((obj as ProgressionSeries)?.ProgressionSeriesId);
        }

        public override int GetHashCode()
        {
            return ProgressionSeriesId.GetHashCode();
        }
    }
}
