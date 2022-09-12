using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Pages.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class SeriesBanList : AuthorizedComponentBase
    {
        [Parameter]
        public string ProgressionSeriesId { get; set; }

        [BindProperty]
        private ProgressionSeries _currentProgressionSeries { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

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
                        .Include(series => series.CurrentBanList)
                        .ThenInclude(banlist => banlist.Entries)
                        .ThenInclude(entry => entry.CardInfo)
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
    }
}
