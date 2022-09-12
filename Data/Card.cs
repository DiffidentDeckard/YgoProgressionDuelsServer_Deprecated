using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents a physical Card that a Duelist has available to them to use
    /// </summary>
    [Index(nameof(OwnerId), nameof(InfoId), IsUnique = true)]
    public class Card : IComparable<Card>
    {
        [Key]
        public Guid CardId { get; set; }

        [ForeignKey(nameof(Owner))]
        public Guid OwnerId { get; set; }

        // The duelist that owns this particular card, to use in a deck
        [InverseProperty(nameof(Duelist.CardCollection))]
        public Duelist Owner { get; set; }

        [ForeignKey(nameof(Info))]
        public ulong InfoId { get; set; }

        // The info about the card, such as name and the image to show
        public CardInfo Info { get; set; }

        // How many copies of this card this duelist has available to use in the deck
        public uint NumCollection { get; set; }

        // How many copies of this card are currently in the deck
        public uint NumMainDeck { get; set; }
        public uint NumExtraDeck { get; set; }
        public uint NumSideDeck { get; set; }

        // The date that this Duelist first attained this card (regardless of what set it is from)
        public DateTime DateObtained { get; set; }

        [NotMapped]
        public bool IsNew
        {
            get
            {
                return DateTime.UtcNow.Subtract(DateObtained).TotalHours < 24;
            }
        }

        public uint GetTotalInDeck()
        {
            return NumMainDeck + NumExtraDeck + NumSideDeck;
        }

        public uint GetCardLimit()
        {
            if (Owner.Series.BanListFormat == BanListFormat.None)
            {
                return 3;
            }

            BanListEntry banListEntry = Owner.Series.CurrentBanList?.Entries?.FirstOrDefault(entry => entry.CardInfoId == Info.CardInfoId);

            if (banListEntry == null || banListEntry.BanListStatus == BanListStatus.Unlimited)
            {
                return 3;
            }

            if (banListEntry.BanListStatus == BanListStatus.SemiLimited)
            {
                return 2;
            }

            if (banListEntry.BanListStatus == BanListStatus.Limited || Owner.Series.BanListFormat == BanListFormat.Traditional)
            {
                return 1;
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            return CardId.Equals((obj as Card)?.CardId);
        }

        public override int GetHashCode()
        {
            return CardId.GetHashCode();
        }

        public int CompareTo(Card other)
        {
            return Info.CompareTo(other.Info);
        }
    }
}
