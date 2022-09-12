using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Pages.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class ProgressionSeriesSettings : AuthorizedComponentBase
    {
        [Parameter]
        public string ProgressionSeriesId { get; set; }

        [BindProperty]
        private ProgressionSeries _currentProgressionSeries { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private ProgressionSeriesSettingsModel _progressionSeriesSettingsModel { get; set; }

        [BindProperty]
        private SortedSet<BanList> _banLists { get; set; } = new SortedSet<BanList>() { null };

        [BindProperty]
        private Snackbar _saveConfirmationSnackbar { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentProgressionSeries();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentProgressionSeries()
        {
            // Parse the Guid and find this progression series
            if (Guid.TryParse(ProgressionSeriesId, out Guid seriesId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentProgressionSeries = await dbContext.ProgressionSeries.AsNoTracking()
                        .Include(series => series.Host)
                        .Include(series => series.CurrentBanList)
                        .Include(series => series.CurrentBoosterPack)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(series => series.ProgressionSeriesId.Equals(seriesId));
                }

                if (_currentProgressionSeries != null)
                {
                    _progressionSeriesSettingsModel = new ProgressionSeriesSettingsModel(_currentProgressionSeries);

                    using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                    {
                        _currentDuelist = await dbContext.Duelists.AsNoTracking()
                            .FirstOrDefaultAsync(duelist => duelist.OwnerId.Equals(CurrentUserId)
                            && duelist.SeriesId.Equals(seriesId));

                        _banLists.UnionWith(dbContext.BanLists.AsNoTracking());
                    }
                }
            }
        }

        private async Task OnSaveChanges()
        {
            // Apply all the changes to the current progression series object
            _currentProgressionSeries.AllowPurchaseBoosterPacks = _progressionSeriesSettingsModel.AllowPurchaseBoosterPacks;
            _currentProgressionSeries.BoosterPackPrice = (uint)_progressionSeriesSettingsModel.BoosterPackPrice;
            _currentProgressionSeries.AllowPurchaseStructureDecks = _progressionSeriesSettingsModel.AllowPurchaseStructureDecks;
            _currentProgressionSeries.StructureDeckPrice = (uint)_progressionSeriesSettingsModel.StructureDeckPrice;
            _currentProgressionSeries.AllowPurchaseSpecialProducts = _progressionSeriesSettingsModel.AllowPurchaseSpecialProducts;
            _currentProgressionSeries.SpecialProductPricePerCard = (uint)_progressionSeriesSettingsModel.SpecialProductPricePerCard;
            _currentProgressionSeries.BanListFormat = _progressionSeriesSettingsModel.BanListFormat;
            _currentProgressionSeries.AutoSetCurrentBanList = _progressionSeriesSettingsModel.AutoSetCurrentBanList;
            _currentProgressionSeries.NextTournamentStructure = _progressionSeriesSettingsModel.TournamentStructure;
            _currentProgressionSeries.NextTournamentBye = _progressionSeriesSettingsModel.TournamentBye;

            if (_currentProgressionSeries.CurrentBanListId != _progressionSeriesSettingsModel.CurrentBanListId)
            {
                _currentProgressionSeries.CurrentBanList = null;
                _currentProgressionSeries.CurrentBanListId = _progressionSeriesSettingsModel.CurrentBanListId;
            }

            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                // Update in the database
                dbContext.ProgressionSeries.Update(_currentProgressionSeries);
                await dbContext.SaveChangesAsync();
            }

            // Show confirmation for user
            _saveConfirmationSnackbar.Show();
        }

        private void SelectedBanListChanged(Guid? banListId)
        {
            _progressionSeriesSettingsModel.CurrentBanListId = banListId;
        }

        private void OnCancelChanges()
        {
            // Reset the model
            _progressionSeriesSettingsModel = new ProgressionSeriesSettingsModel(_currentProgressionSeries);
        }

        private void OnAutoSetBanListChanged(bool autoSet)
        {
            if (_progressionSeriesSettingsModel.AutoSetCurrentBanList != autoSet)
            {
                _progressionSeriesSettingsModel.AutoSetCurrentBanList = autoSet;

                if (autoSet && _currentProgressionSeries.CurrentBoosterPack != null)
                {
                    _progressionSeriesSettingsModel.CurrentBanListId = _banLists.Where(banlist => banlist?.ReleaseDate <= _currentProgressionSeries.CurrentBoosterPack.ReleaseDate)
                        .OrderByDescending(banList => banList.ReleaseDate).FirstOrDefault()?.BanListId;
                }
            }
        }
    }
}
