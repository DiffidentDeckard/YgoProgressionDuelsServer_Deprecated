using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Pages.Shared
{
    public partial class TournamentComponent : AuthorizedComponentBase
    {
        [Parameter]
        public Tournament Tournament { get; set; }

        [Parameter]
        public bool IsCurrentTournament { get; set; }

        [BindProperty]
        private bool _isVisible { get; set; } = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _isVisible = IsCurrentTournament;
        }

        private async Task OnCalculateAndSaveScoresAsync(TournamentPairing pairing, bool? winner)
        {
            PairingDuelist pairingDuelist1 = pairing.Duelists.First();
            PairingDuelist pairingDuelist2 = pairing.Duelists.Last();

            if (winner == true)
            {
                pairingDuelist1.Score = 3;
                pairingDuelist2.Score = 0;
            }
            else if (winner == null)
            {
                pairingDuelist1.Score = 0;
                pairingDuelist2.Score = 0;
            }
            else
            {
                pairingDuelist1.Score = 0;
                pairingDuelist2.Score = 3;
            }

            // Save the pairings scores
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                dbContext.PairingDuelists.Update(pairingDuelist1);
                dbContext.PairingDuelists.Update(pairingDuelist2);
                await dbContext.SaveChangesAsync();
            }

            // Calculate All Tournament Scores
            foreach (TournamentDuelist tournamentDuelist in Tournament.Duelists.ToArray())
            {
                List<TournamentPairing> duelsWon = Tournament.Pairings
                    .Where(pairing => pairing.Duelists.Any(duelist => duelist.Username.Equals(tournamentDuelist.Username))
                    && (pairing.Duelists.Count < 2 || pairing.Duelists.First(duelist => duelist.Username.Equals(tournamentDuelist.Username)).Score
                    > pairing.Duelists.First(duelist => !duelist.Username.Equals(tournamentDuelist.Username)).Score)).ToList();

                tournamentDuelist.Score = duelsWon.Count * 3;

                List<TournamentPairing> duelistsDefeated = duelsWon
                    .Where(pairing => pairing.Duelists.Count > 1).ToList();

                tournamentDuelist.OpponentsDefeated = duelistsDefeated.Count;
            }

            // Now that all scores have been calculated, get each OpponentsOpponentsDefeated
            foreach (TournamentDuelist tournamentDuelist in Tournament.Duelists.ToArray())
            {
                tournamentDuelist.OpponentsOpponentsDefeated = 0;

                List<string> defeatedOpponentsUsernames = Tournament.Pairings
                    .Where(pairing => pairing.Duelists.Any(duelist => duelist.Username.Equals(tournamentDuelist.Username))
                    && pairing.Duelists.Count > 1 && pairing.Duelists.First(duelist => duelist.Username.Equals(tournamentDuelist.Username)).Score
                    > pairing.Duelists.First(duelist => !duelist.Username.Equals(tournamentDuelist.Username)).Score)
                    .Select(pairing => pairing.Duelists.First(duelist => !duelist.Username.Equals(tournamentDuelist.Username)).Username).ToList();

                List<TournamentDuelist> defeatedOpponents = Tournament.Duelists.Where(duelist => defeatedOpponentsUsernames.Contains(duelist.Username)).ToList();

                if (defeatedOpponents.Any())
                {
                    tournamentDuelist.OpponentsOpponentsDefeated = defeatedOpponents.Max(duelist => duelist.OpponentsDefeated);
                }
            }

            // Save the tournament scores
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                dbContext.TournamentDuelists.UpdateRange(Tournament.Duelists.ToArray());
                await dbContext.SaveChangesAsync();
            }

            // Refresh
            NavManager.NavigateTo(NavManager.Uri, true);
        }

        private async Task OnDeleteTournament()
        {
            if (!await JSRuntime.InvokeAsync<bool>("confirm", new object[] { $"Are you sure you want to delete Tournament #{Tournament.Number}?" }))
            {
                return;
            }

            // Delete this tournament
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                dbContext.Tournaments.Remove(Tournament);
                await dbContext.SaveChangesAsync();
            }

            // Refresh
            NavManager.NavigateTo(NavManager.Uri, true);
        }

    }
}
