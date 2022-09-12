using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    [Index(nameof(StructureDeckSetCode), nameof(CardInfoId), IsUnique = true)]
    public class StructureDeckCard
    {
        [Key]
        public Guid StructureDeckCardId { get; set; }

        public string StructureDeckSetCode { get; set; }

        public long CardInfoId { get; set; }

        public string CardInfoName { get; set; }

        public string Rarity { get; set; }
    }
}
