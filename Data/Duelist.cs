using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// A Duelist represents an ApplicationUser in a single ProgressionSeries as a participant
    /// </summary>
    [Index(nameof(OwnerId), nameof(SeriesId), IsUnique = true)]
    public class Duelist
    {
        [Key]
        public Guid DuelistId { get; set; }

        [ForeignKey(nameof(Owner))]
        public Guid OwnerId { get; set; }

        // The actual User that this Duelist represents in a progression series
        [InverseProperty(nameof(ApplicationUser.Duelists))]
        public ApplicationUser Owner { get; set; }

        [ForeignKey(nameof(Series))]
        public Guid SeriesId { get; set; }

        // The progression series this duelist is participating in
        [InverseProperty(nameof(ProgressionSeries.Duelists))]
        public ProgressionSeries Series { get; set; }

        // A list of booster packs that this duelist has available to them to open
        [InverseProperty(nameof(BoosterPack.Owner))]
        public ICollection<BoosterPack> BoosterPacks { get; set; } = new List<BoosterPack>();

        [NotMapped]
        public int NumBoosterPacksAvailable { get { return BoosterPacks.Sum(boosterPack => (int)boosterPack.NumAvailable); } }

        [NotMapped]
        public int NumBoosterPacksOpened { get { return BoosterPacks.Sum(boosterPack => (int)boosterPack.NumOpened); } }

        // The number of Booster Packs this Duelist has opened in this Progression Series so far
        public uint NumPacksOpened { get; set; }

        // A list of cards that this user has available to them for a deck
        [InverseProperty(nameof(Card.Owner))]
        public ICollection<Card> CardCollection { get; set; } = new List<Card>();

        public uint NumUniqueCards { get; set; }

        public bool CardCollectionIsPublic { get; set; } = false;
        public bool DeckIsPublic { get; set; } = false;

        // How much currency this duelist has, to spend on bonus packs
        public uint NumStarChips { get; set; }

        public override bool Equals(object obj)
        {
            return DuelistId.Equals((obj as Duelist)?.DuelistId);
        }

        public override int GetHashCode()
        {
            return DuelistId.GetHashCode();
        }
    }
}
