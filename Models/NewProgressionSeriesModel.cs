using System.ComponentModel.DataAnnotations;

namespace YgoProgressionDuels.Models
{
    public class NewProgressionSeriesModel
    {
        [Required(ErrorMessage = "Please input a name for this Progression Series")]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }

        public bool HostIsParticipating { get; set; } = true;
    }
}
