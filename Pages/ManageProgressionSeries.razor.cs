using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Pages.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class ManageProgressionSeries : AuthorizedComponentBase
    {
        [Parameter]
        public string ProgressionSeriesId { get; set; }

        [BindProperty]
        private ProgressionSeries _currentProgressionSeries { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private AddDuelistModel _addDuelistModel { get; set; } = new AddDuelistModel();

        [BindProperty]
        private string _addDuelistErrorMessage { get; set; }

        [BindProperty]
        private HashSet<Guid> _duelistsShowingDetails { get; } = new HashSet<Guid>();

        private Modal _confirmRemoveDuelistDialog;
        private Duelist _duelistToRemove;

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
                        .Include(series => series.Host).AsNoTracking()
                        .Include(series => series.Duelists)
                        .ThenInclude(duelist => duelist.Owner).AsNoTracking()
                        .Include(series => series.Duelists)
                        .ThenInclude(duelist => duelist.BoosterPacks)
                        .ThenInclude(boosterPack => boosterPack.PackInfo).AsNoTracking()
                        .Include(series => series.CurrentBoosterPack).AsNoTracking()
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

        private async Task OnAddDuelistAsync()
        {
            _addDuelistErrorMessage = string.Empty;

            if (!_currentProgressionSeries.HostId.Equals(CurrentUserId))
            {
                // This user is not the host of this series
                _addDuelistErrorMessage = "You are not Host of this Progression Series";
                return;
            }

            if (_currentProgressionSeries.Duelists.Any(duelist => duelist.Owner.NormalizedUserName.Equals(_addDuelistModel.Username.ToUpper())))
            {
                // This Duelist already exists in this Progression Series
                _addDuelistErrorMessage = "This Duelist is already a Participant of this Progression Series";
                return;
            }

            Guid userIdToAdd;
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                ApplicationUser userToAdd = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.UserName.Equals(_addDuelistModel.Username));

                if (userToAdd == null)
                {
                    // This Duelist does not exist
                    _addDuelistErrorMessage = "This Duelist does not exist";
                    return;
                }

                userIdToAdd = userToAdd.Id;
            }

            // All checks have passed, lets add this Duelist to this ProgressionSeries!
            Duelist newDuelist = new Duelist()
            {
                OwnerId = userIdToAdd,
                SeriesId = _currentProgressionSeries.ProgressionSeriesId
            };

            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                await dbContext.Duelists.AddAsync(newDuelist);
                await dbContext.SaveChangesAsync();
            }

            // Refresh
            _addDuelistModel = new AddDuelistModel();
            await AssignCurrentProgressionSeriesAndDuelist();
        }

        private void OnRemoveThisDuelist(Duelist duelist)
        {
            if (!CurrentUserId.Equals(_currentProgressionSeries.HostId))
            {
                // This is not the host
                return;
            }

            // Show a confirmation dialog for the user
            _duelistToRemove = duelist;
            _confirmRemoveDuelistDialog.Show();
        }

        private async Task CloseConfirmRemoveDuelistDialogAsync(bool shouldDeleteDuelist)
        {
            _confirmRemoveDuelistDialog.Hide();
            if (shouldDeleteDuelist && CurrentUserId.Equals(_currentProgressionSeries.HostId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM BoosterPacks WHERE OwnerID = '{_duelistToRemove.DuelistId}'");
                    await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM Cards WHERE OwnerID = '{_duelistToRemove.DuelistId}'");
                    await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM Duelists WHERE DuelistId = '{_duelistToRemove.DuelistId}'");
                }

                _duelistToRemove = null;
                await AssignCurrentProgressionSeriesAndDuelist();
            }
        }

        private void ToggleDuelistPackDetails(Guid duelistId)
        {
            if (_duelistsShowingDetails.Contains(duelistId))
            {
                _duelistsShowingDetails.Remove(duelistId);
            }
            else
            {
                _duelistsShowingDetails.Add(duelistId);
            }
        }
    }
}
