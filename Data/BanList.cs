using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents an entire banlist in yugioh at some point in time
    /// </summary>
    [Index(nameof(ReleaseDate), IsUnique = true)]
    public class BanList : IComparable<BanList>
    {
        [Key]
        public Guid BanListId { get; set; }

        // The date this banlist was released
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [InverseProperty(nameof(BanListEntry.OwnerBanList))]
        public IList<BanListEntry> Entries { get; set; } = new List<BanListEntry>();

        public int CompareTo(BanList other)
        {
            return ReleaseDate.CompareTo(other?.ReleaseDate);
        }
    }
}
