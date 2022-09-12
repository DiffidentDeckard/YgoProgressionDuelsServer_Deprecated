using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Pages.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class MyProgressionSeries : AuthorizedComponentBase
    {
        [BindProperty]
        private NewProgressionSeriesModel _newSeriesModel { get; set; } = new NewProgressionSeriesModel();

        [BindProperty]
        private string _addNewSeriesErrorMessage { get; set; }

        [BindProperty]
        private IList<ProgressionSeries> _myProgressionSeries { get; set; } = new List<ProgressionSeries>();

        private Modal _confirmDeleteSeriesDialog;
        private ProgressionSeries _seriesToDelete;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignMyProgressionSeries();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignMyProgressionSeries()
        {
            // Get the user's progression series
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                _myProgressionSeries = await dbContext.ProgressionSeries.AsNoTracking()
                    .Include(series => series.Host).AsNoTracking()
                    .Include(series => series.Duelists).AsNoTracking()
                    .Where(series => series.HostId.Equals(CurrentUserId) || series.Duelists.Any(duelist => duelist.OwnerId.Equals(CurrentUserId)))
                        .AsSplitQuery()
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Creates a new ProgressionSeries and saves it to database with the current user as its host.
        /// Optionally will also add the current user as a participant in the series, if that option is selected.
        /// </summary>
        /// <returns></returns>
        public async Task OnAddNewProgressionSeriesAsync()
        {
            _addNewSeriesErrorMessage = string.Empty;

            // First, confirm that this user isn't already hosting a progression series with this same name
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                if (await dbContext.ProgressionSeries.AsNoTracking()
                    .AnyAsync(series => series.HostId.Equals(CurrentUserId)
                        && series.Name.Equals(_newSeriesModel.Name)))
                {
                    // User already owns a series with this name, don't let them continue
                    _addNewSeriesErrorMessage = "You are already hosting a Progression Series with this name";
                    return;
                }
            }

            // Create the ProgressionSeries object to add to database
            ProgressionSeries newSeries = new ProgressionSeries()
            {
                HostId = CurrentUserId,
                Name = _newSeriesModel.Name,
                DateStarted = DateTime.Today
            };

            // Add ProgressionSeries to database
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                await dbContext.ProgressionSeries.AddAsync(newSeries);
                await dbContext.SaveChangesAsync();
            }

            // If the host is participating, we want to add them to this series as a duelist
            if (_newSeriesModel.HostIsParticipating)
            {
                // Create a Duelist for this User, and add it to the ProgressionSeries
                Duelist newDuelist = new Duelist()
                {
                    OwnerId = CurrentUserId,
                    SeriesId = newSeries.ProgressionSeriesId
                };

                // Add duelist to database
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    await dbContext.Duelists.AddAsync(newDuelist);
                    newSeries.Duelists.Add(newDuelist);
                    await dbContext.SaveChangesAsync();
                }
            }

            // Refresh
            _newSeriesModel = new NewProgressionSeriesModel();
            await AssignMyProgressionSeries();
        }

        private void OnDeleteThisSeries(ProgressionSeries series)
        {
            if (!series.HostId.Equals(CurrentUserId))
            {
                // This is not the host
                return;
            }

            // Show a confirmation dialog for the user
            _seriesToDelete = series;
            _confirmDeleteSeriesDialog.Show();
        }

        private async Task CloseConfirmDeleteSeriesDialogAsync(bool shouldDeleteSeries)
        {
            _confirmDeleteSeriesDialog.Hide();
            if (shouldDeleteSeries && _seriesToDelete.HostId.Equals(CurrentUserId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    dbContext.ProgressionSeries.Remove(_seriesToDelete);
                    await dbContext.SaveChangesAsync();
                }

                _seriesToDelete = null;
                await AssignMyProgressionSeries();
            }
        }
    }
}
