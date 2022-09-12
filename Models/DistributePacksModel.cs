using System;
using System.ComponentModel.DataAnnotations;

namespace YgoProgressionDuels.Models
{
    public class DistributePacksModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must distribute at least 1 booster pack")]
        public int NumPacksToDistribute { get; set; } = 24;
    }
}
