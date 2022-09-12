using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents the information about a Yugioh Product that is really weird and needs special attention
    /// </summary>
    [Index(nameof(SetName), IsUnique = true)]
    public class SpecialProduct : IComparable<SpecialProduct>
    {
        [Key]
        public Guid SpecialProductId { get; set; }

        public string SetName { get; set; }

        public string SetCode { get; set; }

        public uint NumCards { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string ImageUrl { get; set; }

        public string SetInfoUrl { get; set; }

        [InverseProperty(nameof(SpecialProductCard.OwnerProduct))]
        public List<SpecialProductCard> Cards { get; set; } = new List<SpecialProductCard>();

        public override bool Equals(object obj)
        {
            return SetName.Equals((obj as SpecialProduct)?.SetName);
        }

        public override int GetHashCode()
        {
            return SetName.GetHashCode();
        }

        public int CompareTo(SpecialProduct other)
        {
            return ReleaseDate.CompareTo(other.ReleaseDate);
        }

    }
}
