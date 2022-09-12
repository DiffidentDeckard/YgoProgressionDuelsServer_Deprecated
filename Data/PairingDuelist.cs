using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    [Index(nameof(PairingId), nameof(Username), IsUnique = true)]
    public class PairingDuelist : IComparable<PairingDuelist>
    {
        [Key]
        public Guid PairingDuelistId { get; set; }

        [ForeignKey(nameof(OwnerPairing))]
        public Guid PairingId { get; set; }

        [InverseProperty(nameof(TournamentPairing.Duelists))]
        public TournamentPairing OwnerPairing { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }

        public int Score { get; set; }

        public int CompareTo(PairingDuelist other)
        {
            // Order by descending Score
            int scoreCompare = other.Score.CompareTo(Score);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            return Username.CompareTo(other.Username);
        }
    }
}
