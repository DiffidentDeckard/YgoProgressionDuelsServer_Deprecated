using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Pages.Shared;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class SeriesTournament : AuthorizedComponentBase
    {
        [Parameter]
        public string ProgressionSeriesId { get; set; }

        [BindProperty]
        private ProgressionSeries _currentProgressionSeries { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private bool _isworking { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentProgressionSeriesAndDuelist();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentProgressionSeriesAndDuelist()
        {
            // Parse the Guid and find this progression series
            if (Guid.TryParse(ProgressionSeriesId, out Guid seriesId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentProgressionSeries = await dbContext.ProgressionSeries.AsNoTracking()
                        .Include(series => series.Duelists)
                        .ThenInclude(duelist => duelist.Owner)
                        .Include(series => series.CurrentBoosterPack)
                        .Include(series => series.Tournaments)
                        .ThenInclude(tournament => tournament.Duelists)
                        .Include(series => series.Tournaments)
                        .ThenInclude(tournament => tournament.Pairings)
                        .ThenInclude(pairing => pairing.Duelists)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(series => series.ProgressionSeriesId.Equals(seriesId));
                }

                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentDuelist = await dbContext.Duelists.AsNoTracking()
                        .FirstOrDefaultAsync(duelist => duelist.OwnerId.Equals(CurrentUserId)
                        && duelist.SeriesId.Equals(seriesId));
                }
            }
        }

        private async Task OnNewTournamentAsync()
        {
            _isworking = true;
            try
            {
                if (_currentProgressionSeries.Duelists.Count < 4)
                {
                    return;
                }

                // Create the tournament
                uint newTournamentNumber = (_currentProgressionSeries.Tournaments.FirstOrDefault()?.Number ?? 0) + 1;
                var tournamentDuelists = _currentProgressionSeries.Duelists.Select(duelist => new TournamentDuelist()
                {
                    Username = duelist.Owner.UserName,
                    AvatarUrl = duelist.Owner.AvatarUrl,
                });

                Tournament newTournament = new Tournament()
                {
                    SeriesId = _currentProgressionSeries.ProgressionSeriesId,
                    Number = newTournamentNumber,
                    DateStarted = DateTime.Today,
                    BoosterPackCode = _currentProgressionSeries.CurrentBoosterPack?.SetCode,
                    Duelists = new SortedSet<TournamentDuelist>(tournamentDuelists),
                    Structure = _currentProgressionSeries.NextTournamentStructure,
                    Bye = _currentProgressionSeries.NextTournamentBye
                };

                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    await dbContext.Tournaments.AddAsync(newTournament);
                    await dbContext.SaveChangesAsync();
                    _currentProgressionSeries.Tournaments.Add(newTournament);
                }

                // Generate the first round of this tournament
                await OnNextRoundAsync();
            }
            finally
            {
                _isworking = false;
            }
        }

        private async Task OnNextRoundAsync()
        {
            _isworking = true;
            try
            {
                Tournament currentTournament = _currentProgressionSeries.Tournaments.FirstOrDefault();
                if (currentTournament == null)
                {
                    // We can't make pairings
                    return;
                }

                int numRounds = currentTournament.Pairings.GroupBy(pairing => pairing.RoundNumber).Count();
                if (numRounds >= currentTournament.GetExpectedNumRounds())
                {
                    // We can't make pairings
                    return;
                }

                // Keep attempting to generate pairings until we get a valid combination
                while (!await GenerateNewPairingsAsync()) { }

                // Refresh
                NavManager.NavigateTo(NavManager.Uri, true);
            }
            finally
            {
                _isworking = false;
            }
        }

        private async Task<bool> GenerateNewPairingsAsync()
        {
            Tournament currentTournament = _currentProgressionSeries.Tournaments.First();
            uint roundNum = (uint)currentTournament.Pairings.GroupBy(pairing => pairing.RoundNumber).Count() + 1;
            List<string> duelistByes = currentTournament.Pairings.Where(pairing => pairing.Duelists.Count == 1).Select(pairing => pairing.Duelists.First().Username).ToList();
            List<TournamentPairing> newPairings = new List<TournamentPairing>();
            TournamentPairing byePairing = null;
            Random rng = new Random(DateTime.UtcNow.Second);

            if (roundNum == 1)
            {
                // This is the first round, just generate random pairings
                List<TournamentDuelist> duelists = currentTournament.Duelists.OrderBy(duelist => rng.Next()).ToList();

                if (duelists.Count % 2 == 1)
                {
                    // If we have an odd number of duelists, randomly assign the BYE
                    PairingDuelist pairingDuelist = MakePairingDuelistFromListAtIndex(duelists, 0);
                    pairingDuelist.Score = 3;

                    byePairing = new TournamentPairing()
                    {
                        TournamentId = currentTournament.TournamentId,
                        RoundNumber = roundNum,
                        Duelists = new List<PairingDuelist>() { pairingDuelist }
                    };
                    newPairings.Add(byePairing);
                }

                while (duelists.Any())
                {
                    PairingDuelist pairingDuelist1 = MakePairingDuelistFromListAtIndex(duelists, 0);
                    PairingDuelist pairingDuelist2 = MakePairingDuelistFromListAtIndex(duelists, 0);

                    TournamentPairing newPairing = new TournamentPairing()
                    {
                        TournamentId = currentTournament.TournamentId,
                        RoundNumber = roundNum,
                        Duelists = new List<PairingDuelist>() { pairingDuelist1, pairingDuelist2 }
                    };
                    newPairings.Add(newPairing);
                }
            }
            else if (currentTournament.Structure == TournamentStructure.SingleElimination)
            {
                // Generate next round pairings for Single-Elimination Tournament

                var lastRoundPairings = currentTournament.Pairings
                    .GroupBy(pairing => pairing.RoundNumber)
                    .OrderByDescending(group => group.Key)
                    .First();

                List<string> lastRoundWinners = lastRoundPairings
                    .Select(pairing => pairing.Duelists.OrderByDescending(duelist => duelist.Score).First().Username)
                    .ToList();

                List<TournamentDuelist> duelists = currentTournament.Duelists
                    .Where(duelist => lastRoundWinners.Contains(duelist.Username))
                    .OrderBy(duelist => rng.Next()).ToList();

                // The number of participants should equal the number of pairings in the last round
                if (duelists.Count != lastRoundPairings.Count())
                {
                    return false;
                }

                if (duelists.Count % 2 == 1)
                {
                    // If we have an odd number of duelists, randomly assign the BYE to someone who hasn't had it before
                    int byeIndex = duelists.FindIndex(duelist => !duelistByes.Contains(duelist.Username));
                    if (byeIndex < 0) { byeIndex = 0; }
                    PairingDuelist pairingDuelist = MakePairingDuelistFromListAtIndex(duelists, byeIndex);
                    pairingDuelist.Score = 3;

                    byePairing = new TournamentPairing()
                    {
                        TournamentId = currentTournament.TournamentId,
                        RoundNumber = roundNum,
                        Duelists = new List<PairingDuelist>() { pairingDuelist }
                    };
                    newPairings.Add(byePairing);
                }

                while (duelists.Any())
                {
                    PairingDuelist pairingDuelist1 = MakePairingDuelistFromListAtIndex(duelists, 0);
                    PairingDuelist pairingDuelist2 = MakePairingDuelistFromListAtIndex(duelists, 0);

                    TournamentPairing newPairing = new TournamentPairing()
                    {
                        TournamentId = currentTournament.TournamentId,
                        RoundNumber = roundNum,
                        Duelists = new List<PairingDuelist>() { pairingDuelist1, pairingDuelist2 }
                    };
                    newPairings.Add(newPairing);
                }
            }
            else
            {
                // Generate next round pairings for Swiss Rounds Tournament
                List<TournamentDuelist> duelists = currentTournament.Duelists.OrderByDescending(duelist => duelist.Score)
                    .ThenByDescending(duelist => duelist.OpponentsDefeated)
                    .ThenByDescending(duelist => duelist.OpponentsOpponentsDefeated)
                    .ThenBy(duelist => rng.Next()).ToList();

                if (duelists.Count % 2 == 1)
                {
                    // If we have an odd number of duelists, assign the BYE to someone who hasn't had it before
                    int byeIndex = 0;
                    if (currentTournament.Bye == TournamentBye.BottomScore)
                    {
                        byeIndex = duelists.FindLastIndex(duelist => !duelistByes.Contains(duelist.Username));
                    }
                    else if (currentTournament.Bye == TournamentBye.TopScore)
                    {
                        byeIndex = duelists.FindIndex(duelist => !duelistByes.Contains(duelist.Username));
                    }
                    else
                    {
                        while (true)
                        {
                            byeIndex = rng.Next(duelists.Count);
                            if (!duelistByes.Contains(duelists.ElementAt(byeIndex).Username))
                            {
                                break;
                            }
                        }
                    }

                    PairingDuelist pairingDuelist = MakePairingDuelistFromListAtIndex(duelists, byeIndex);
                    pairingDuelist.Score = 3;

                    byePairing = new TournamentPairing()
                    {
                        TournamentId = currentTournament.TournamentId,
                        RoundNumber = roundNum,
                        Duelists = new List<PairingDuelist>() { pairingDuelist }
                    };
                }

                // Make copies of our list that wil be iterated through and modified instead
                var duelistsCopy = duelists.ToList();
                uint numFailed = 0; // Keep track of how many times we failed

                while (!GenerateNewPairings(duelistsCopy, currentTournament, newPairings))
                {
                    // We have failed, increment the failCount
                    numFailed++;

                    if (numFailed == 5)
                    {
                        // We have failed too many times, start over and hope for the best
                        return false;
                    }

                    if (numFailed == 3)
                    {
                        // We have failed thrice already, randomize the duelist list in hopes to fix this
                        duelistsCopy = duelists.OrderBy(duelist => rng.Next()).ToList();
                        newPairings = new List<TournamentPairing>();
                    }
                }

                // Add the BYE pairing if it exists
                if (byePairing != null)
                {
                    newPairings.Add(byePairing);
                }
            }

            // Save the new pairings!
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                await dbContext.TournamentPairings.AddRangeAsync(newPairings);
                await dbContext.SaveChangesAsync();
            }

            if (byePairing != null)
            {
                // If there was a BYE, we want to save that tournament score immediately
                TournamentDuelist tournamentDuelist = currentTournament.Duelists.First(duelist => duelist.Username.Equals(byePairing.Duelists.First().Username));
                tournamentDuelist.Score += 3;
                tournamentDuelist.OpponentsDefeated -= 1;
                tournamentDuelist.OpponentsOpponentsDefeated -= 1;

                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    dbContext.TournamentDuelists.Update(tournamentDuelist);
                    await dbContext.SaveChangesAsync();
                }
            }

            return true;
        }

        private bool GenerateNewPairings(List<TournamentDuelist> duelists, Tournament currentTournament, List<TournamentPairing> newPairings)
        {
            while (duelists.Any())
            {
                PairingDuelist pairingDuelist1 = MakePairingDuelistFromListAtIndex(duelists, 0);

                // Get the list of this duelists past opponents, we do not want repeats
                List<string> pastOpponents = currentTournament.Pairings
                    .Where(pairing => pairing.Duelists.Count > 1 && pairing.Duelists.Any(duelist => duelist.Username.Equals(pairingDuelist1.Username)))
                    .Select(pairing => pairing.Duelists.First(duelist => !duelist.Username.Equals(pairingDuelist1.Username)).Username).ToList();

                // Find the next available duelist we can duel
                int opponentIndex = duelists.FindIndex(duelist => !pastOpponents.Contains(duelist.Username));
                if (opponentIndex < 0)
                {
                    // We have reached a duelist for which there is no valid opponent, start over
                    return false;
                }

                PairingDuelist pairingDuelist2 = MakePairingDuelistFromListAtIndex(duelists, opponentIndex);
                uint numRounds = (uint)currentTournament.Pairings.GroupBy(pairing => pairing.RoundNumber).Count();

                TournamentPairing newPairing = new TournamentPairing()
                {
                    TournamentId = currentTournament.TournamentId,
                    RoundNumber = numRounds + 1,
                    Duelists = new List<PairingDuelist>() { pairingDuelist1, pairingDuelist2 }
                };
                newPairings.Add(newPairing);
            }

            // Double check that we have the correct number of pairings
            if (newPairings.Count != duelists.Count / 2)
            {
                // We fudged up somewhere
                return false;
            }

            return true;
        }

        private PairingDuelist MakePairingDuelistFromListAtIndex(List<TournamentDuelist> duelists, int index)
        {
            TournamentDuelist tournamentDuelist = duelists.ElementAt(index);
            PairingDuelist pairingDuelist = new PairingDuelist()
            {
                Username = tournamentDuelist.Username,
                AvatarUrl = tournamentDuelist.AvatarUrl
            };

            duelists.RemoveAt(index);

            return pairingDuelist;
        }
    }
}

