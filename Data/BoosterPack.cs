using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents a physical BoosterPack that a Duelist has available to them to open
    /// </summary>
    [Index(nameof(OwnerId), nameof(InfoId), IsUnique = true)]
    public class BoosterPack
    {
        [Key]
        public Guid BoosterPackId { get; set; }

        [ForeignKey(nameof(Owner))]
        public Guid OwnerId { get; set; }

        // The duelist that owns this particular pack, to open
        [InverseProperty(nameof(Duelist.BoosterPacks))]
        public Duelist Owner { get; set; }

        [ForeignKey(nameof(PackInfo))]
        public Guid InfoId { get; set; }

        // The information about this particular pack, such as name and the image to show
        public BoosterPackInfo PackInfo { get; set; }

        // How many of this pack this duelist has available to open
        public uint NumAvailable { get; set; }

        // How many of this pack this duelist has already opened
        public uint NumOpened { get; set; }

        public override bool Equals(object obj)
        {
            return BoosterPackId.Equals((obj as BoosterPack)?.BoosterPackId);
        }

        public override int GetHashCode()
        {
            return BoosterPackId.GetHashCode();
        }
    }
}
