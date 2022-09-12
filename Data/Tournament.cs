using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Data
{
    [Index(nameof(SeriesId), nameof(Number), IsUnique = true)]
    public class Tournament : IComparable<Tournament>
    {
        [Key]
        public Guid TournamentId { get; set; }

        [ForeignKey(nameof(Series))]
        public Guid SeriesId { get; set; }

        [InverseProperty(nameof(ProgressionSeries.Tournaments))]
        public ProgressionSeries Series { get; set; }

        public uint Number { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateStarted { get; set; }

        public string BoosterPackCode { get; set; }

        public TournamentStructure Structure { get; set; }

        public TournamentBye Bye { get; set; }

        [InverseProperty(nameof(TournamentDuelist.OwnerTournament))]
        public SortedSet<TournamentDuelist> Duelists { get; set; } = new SortedSet<TournamentDuelist>();

        [InverseProperty(nameof(TournamentPairing.OwnerTournament))]
        public SortedSet<TournamentPairing> Pairings { get; set; } = new SortedSet<TournamentPairing>();

        public int CompareTo(Tournament other)
        {
            // Order by descending tournament number
            return other.Number.CompareTo(Number);
        }

        public uint GetExpectedNumRounds()
        {
            if (Structure == TournamentStructure.SwissRound && Duelists.Count == 4)
            {
                return 3;
            }
            return (uint)Math.Ceiling(Math.Log2(Duelists.Count));
        }
    }
}
