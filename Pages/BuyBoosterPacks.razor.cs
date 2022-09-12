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
    public partial class BuyBoosterPacks : AuthorizedComponentBase
    {
        [Parameter]
        public string DuelistId { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private List<BuyBoosterPackModel> _availableBoosterPacks { get; set; } = new List<BuyBoosterPackModel>();

        [BindProperty]
        private bool _isProcessingPurchase { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentDuelistAndCardsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentDuelistAndCardsAsync()
        {
            // Parse the Guid and find this progression series
            if (Guid.TryParse(DuelistId, out Guid duelistId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentDuelist = await dbContext.Duelists.AsNoTracking()
                        .Include(duelist => duelist.Owner).AsNoTracking()
                        .Include(duelist => duelist.Series)
                        .ThenInclude(series => series.CurrentBoosterPack).AsNoTracking()
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(duelist => duelist.DuelistId.Equals(duelistId));
                }

                if (_currentDuelist != null)
                {
                    List<BoosterPackInfo> availableBoosterPackInfos = new List<BoosterPackInfo>();
                    List<BoosterPack> duelistPacks;
                    using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                    {
                        // Get all booster packs / structure decks in the database that were released at or before this latest pack
                        if (_currentDuelist.Series.CurrentBoosterPack != null)
                        {
                            availableBoosterPackInfos = await dbContext.BoosterPackInfos.AsNoTracking()
                                .Where(info => info.ReleaseDate <= _currentDuelist.Series.CurrentBoosterPack.ReleaseDate
                                    && ((_currentDuelist.Series.AllowPurchaseBoosterPacks && !info.IsStructureDeck)
                                    || (_currentDuelist.Series.AllowPurchaseStructureDecks && info.IsStructureDeck)))
                                .ToListAsync();
                        }

                        duelistPacks = await dbContext.BoosterPacks.AsNoTracking()
                            .Where(pack => pack.OwnerId.Equals(_currentDuelist.DuelistId))
                            .ToListAsync();
                    }

                    // Create the models for each of these booster packs
                    List<BuyBoosterPackModel> packModels = new List<BuyBoosterPackModel>();
                    foreach (BoosterPackInfo packInfo in availableBoosterPackInfos)
                    {
                        BoosterPack pack = duelistPacks.FirstOrDefault(pack => pack.InfoId.Equals(packInfo.BoosterPackInfoId));
                        packModels.Add(new BuyBoosterPackModel()
                        {
                            PackInfo = packInfo,
                            Pack = pack
                        });
                    }
                    _availableBoosterPacks = packModels;
                }
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }

        private async Task OnPurchaseBoosterPackAsync(BuyBoosterPackModel packModel)
        {
            try
            {
                _isProcessingPurchase = true;

                uint price = packModel.PackInfo.IsStructureDeck ? _currentDuelist.Series.StructureDeckPrice : _currentDuelist.Series.BoosterPackPrice;

                if (_currentDuelist.NumStarChips < price)
                {
                    return;
                }

                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    if (packModel.Pack == null)
                    {
                        // Duelist has not had any of this pack before, we need to make a new BoosterPack object
                        BoosterPack newPack = new BoosterPack()
                        {
                            OwnerId = _currentDuelist.DuelistId,
                            InfoId = packModel.PackInfo.BoosterPackInfoId,
                            NumAvailable = 1
                        };
                        await dbContext.BoosterPacks.AddAsync(newPack);
                        packModel.Pack = newPack;
                    }
                    else
                    {
                        // Update existing pack, adding one to available
                        packModel.Pack.NumAvailable++;
                        dbContext.BoosterPacks.Update(packModel.Pack);
                    }

                    // Subtract starchips from user
                    _currentDuelist.NumStarChips -= price;
                    dbContext.Duelists.Update(_currentDuelist);

                    // Save
                    await dbContext.SaveChangesAsync();
                }
            }
            finally
            {
                _isProcessingPurchase = false;
            }
        }
    }
}
