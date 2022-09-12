using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Pages.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class DistributeBoosterPacks : AuthorizedComponentBase
    {
        [Parameter]
        public string ProgressionSeriesId { get; set; }

        [BindProperty]
        private ProgressionSeries _currentProgressionSeries { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private List<Duelist> _selectedDuelists { get; set; } = new List<Duelist>();

        [BindProperty]
        private List<BoosterPackInfo> _boosterPackInfos { get; set; }

        [BindProperty]
        private List<BoosterPackInfo> _selectedBoosterPackInfos { get; set; } = new List<BoosterPackInfo>();

        [BindProperty]
        private DistributePacksModel _distributePacksModel { get; set; } = new DistributePacksModel();

        [BindProperty]
        private string _distributePacksErrorMessage { get; set; }

        [BindProperty]
        private bool _isworking { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentProgressionSeriesAndBoosterPacks();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentProgressionSeriesAndBoosterPacks()
        {
            // Parse the Guid and find this progression series
            if (Guid.TryParse(ProgressionSeriesId, out Guid seriesId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentProgressionSeries = await dbContext.ProgressionSeries.AsNoTracking()
                        .Include(series => series.CurrentBoosterPack)
                        .Include(series => series.Duelists)
                        .ThenInclude(duelist => duelist.Owner)
                        .Include(series => series.Duelists)
                        .ThenInclude(duelist => duelist.BoosterPacks)
                        .ThenInclude(boosterPack => boosterPack.PackInfo)
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

            // Get the booster pack infos
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                _boosterPackInfos = await dbContext.BoosterPackInfos.AsNoTracking()
                    .OrderBy(info => info.ReleaseDate).ToListAsync();
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }

        private async Task OnDistributeBoosterPacksAsync()
        {
            _isworking = true;
            try
            {
                _distributePacksErrorMessage = string.Empty;

                if (!_currentProgressionSeries.HostId.Equals(CurrentUserId))
                {
                    _distributePacksErrorMessage = "You are not the Host of this Progression Series.";
                    return;
                }

                if (!_selectedDuelists.Any())
                {
                    _distributePacksErrorMessage = "Please select at least 1 Duelist to distribute packs to.";
                    return;
                }

                if (!_selectedBoosterPackInfos.Any())
                {
                    _distributePacksErrorMessage = "Please select at least 1 Booster Pack to distribute.";
                    return;
                }

                // Distribute the Booster Packs to selected Duelists
                foreach (Duelist selectedDuelist in _selectedDuelists)
                {
                    foreach (BoosterPackInfo selectedBoosterPackInfo in _selectedBoosterPackInfos)
                    {
                        // See if this user already has some of this booster pack.
                        using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                        {
                            BoosterPack boosterPack = await dbContext.BoosterPacks.FirstOrDefaultAsync(boosterpack => boosterpack.OwnerId.Equals(selectedDuelist.DuelistId)
                                && boosterpack.InfoId.Equals(selectedBoosterPackInfo.BoosterPackInfoId));

                            if (boosterPack == null)
                            {
                                // Make a new booster pack and save it
                                boosterPack = new BoosterPack()
                                {
                                    OwnerId = selectedDuelist.DuelistId,
                                    InfoId = selectedBoosterPackInfo.BoosterPackInfoId,
                                    NumAvailable = (uint)_distributePacksModel.NumPacksToDistribute
                                };
                                await dbContext.BoosterPacks.AddAsync(boosterPack);
                            }
                            else
                            {
                                // If the user already has some of this booster pack, just increase the amount
                                boosterPack.NumAvailable += (uint)_distributePacksModel.NumPacksToDistribute;
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }

                // Update latest booster pack and banlist if needed
                BoosterPackInfo latestBoosterPack = _selectedBoosterPackInfos.OrderByDescending(boosterPack => boosterPack.ReleaseDate).First();
                if (_currentProgressionSeries.CurrentBoosterPack == null || latestBoosterPack.ReleaseDate > _currentProgressionSeries.CurrentBoosterPack.ReleaseDate)
                {
                    using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                    {
                        // Kept getting an error telling me something was already being tracked so I couldnt save changes
                        dbContext.ChangeTracker.Clear();
                        ProgressionSeries series = await dbContext.ProgressionSeries.FirstAsync(series => series.ProgressionSeriesId.Equals(_currentProgressionSeries.ProgressionSeriesId));

                        // Update the Current BoosterPack
                        series.CurrentBoosterPackId = latestBoosterPack.BoosterPackInfoId;

                        if (_currentProgressionSeries.AutoSetCurrentBanList)
                        {
                            // We gotta update the banlist
                            BanList latestBanList = await dbContext.BanLists.AsNoTracking().Where(banList => banList.ReleaseDate <= latestBoosterPack.ReleaseDate)
                                .OrderByDescending(banList => banList.ReleaseDate).FirstOrDefaultAsync();

                            series.CurrentBanListId = latestBanList?.BanListId;
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                // Refresh
                NavManager.NavigateTo(NavManager.Uri, true);
            }
            finally
            {
                _isworking = false;
            }
        }
    }
}
