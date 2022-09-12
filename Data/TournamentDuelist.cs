using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    [Index(nameof(TournamentId), nameof(Username), IsUnique = true)]
    public class TournamentDuelist : IComparable<TournamentDuelist>
    {
        [Key]
        public Guid TournamentDuelistId { get; set; }

        [ForeignKey(nameof(OwnerTournament))]
        public Guid TournamentId { get; set; }

        [InverseProperty(nameof(Tournament.Duelists))]
        public Tournament OwnerTournament { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }

        public int Score { get; set; }

        public int OpponentsDefeated { get; set; }

        public int OpponentsOpponentsDefeated { get; set; }

        public int CompareTo(TournamentDuelist other)
        {
            // Order by descending Score
            int scoreCompare = other.Score.CompareTo(Score);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            // Order by descending OpponentsDefeated
            int opponentScoreCompare = other.OpponentsDefeated.CompareTo(OpponentsDefeated);
            if (opponentScoreCompare != 0)
            {
                return opponentScoreCompare;
            }

            // Order by descending LastOpponentDefeatedScore
            int opponentOpponentScoreCompare = other.OpponentsOpponentsDefeated.CompareTo(OpponentsOpponentsDefeated);
            if (opponentOpponentScoreCompare != 0)
            {
                return opponentOpponentScoreCompare;
            }

            return Username.CompareTo(other.Username);
        }

        public override bool Equals(object obj)
        {
            return Username.Equals((obj as TournamentDuelist)?.Username);
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }
    }
}
