using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    [Index(nameof(OwnerProductId), nameof(CardInfoId), IsUnique = true)]
    public class SpecialProductCard
    {
        [Key]
        public Guid SpecialProductCardId { get; set; }

        [ForeignKey(nameof(OwnerProduct))]
        public Guid OwnerProductId { get; set; }

        [InverseProperty(nameof(SpecialProduct.Cards))]
        public SpecialProduct OwnerProduct { get; set; }

        public long CardInfoId { get; set; }

        public string CardInfoName { get; set; }

        public string Rarity { get; set; }
    }
}
