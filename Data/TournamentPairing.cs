using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace YgoProgressionDuels.Data
{
    public class TournamentPairing : IComparable<TournamentPairing>
    {
        [Key]
        public Guid TournamentPairingId { get; set; }

        [ForeignKey(nameof(OwnerTournament))]
        public Guid TournamentId { get; set; }

        [InverseProperty(nameof(Tournament.Pairings))]
        public Tournament OwnerTournament { get; set; }

        public uint RoundNumber { get; set; }

        [InverseProperty(nameof(PairingDuelist.OwnerPairing))]
        public List<PairingDuelist> Duelists { get; set; } = new List<PairingDuelist>();

        public int CompareTo(TournamentPairing other)
        {
            // Order by descending RoundNumber
            int roundCompare = other.RoundNumber.CompareTo(RoundNumber);
            if (roundCompare != 0)
            {
                return roundCompare;
            }

            // The BYE Pairing goes last
            int countCompare = other.Duelists.Count.CompareTo(Duelists.Count);
            if (countCompare != 0)
            {
                return countCompare;
            }


            return Duelists.First().Username.CompareTo(other.Duelists.First().Username);
        }
    }
}
