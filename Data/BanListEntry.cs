using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents a single card on a banlist for a single ProgressionSeries
    /// </summary>
    [Index(nameof(OwnerBanListId), nameof(CardInfoId), IsUnique = true)]
    public class BanListEntry
    {
        [Key]
        public Guid BanListEntryId { get; set; }

        [ForeignKey(nameof(OwnerBanList))]
        public Guid OwnerBanListId { get; set; }

        [InverseProperty(nameof(BanList.Entries))]
        public BanList OwnerBanList { get; set; }

        [ForeignKey(nameof(CardInfo))]
        public ulong CardInfoId { get; set; }

        public CardInfo CardInfo { get; set; }

        public BanListStatus BanListStatus { get; set; }
    }
}
