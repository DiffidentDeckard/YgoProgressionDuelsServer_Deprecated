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
    public partial class BuySpecialProducts : AuthorizedComponentBase
    {
        [Parameter]
        public string DuelistId { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private List<SpecialProduct> _availableSpecialProducts { get; set; } = new List<SpecialProduct>();

        [BindProperty]
        private Dictionary<Guid, uint> _retrievedSpecialProductIds { get; set; } = new Dictionary<Guid, uint>();

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
                        .Include(duelist => duelist.CardCollection).AsNoTracking()
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(duelist => duelist.DuelistId.Equals(duelistId));
                }

                if (_currentDuelist?.Series?.CurrentBoosterPack != null && _currentDuelist.Series.AllowPurchaseSpecialProducts)
                {
                    // Get a list of all the special products this user would be allowed to purchase
                    using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                    {
                        _availableSpecialProducts = await dbContext.SpecialProducts.AsNoTracking()
                            .Where(sp => sp.ReleaseDate <= _currentDuelist.Series.CurrentBoosterPack.ReleaseDate).AsNoTracking()
                            .Include(product => product.Cards).AsNoTracking()
                            .OrderByDescending(sp => sp.ReleaseDate).ToListAsync();
                    }
                }
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }

        private void ToggleRowDetails(Guid specialProductId)
        {
            if (_retrievedSpecialProductIds.ContainsKey(specialProductId))
            {
                _retrievedSpecialProductIds.Remove(specialProductId);
            }
            else
            {
                _retrievedSpecialProductIds.Add(specialProductId, 0);
            }
        }

        private async Task OnPurchaseSpecialProductAsync(SpecialProduct sp)
        {
            try
            {
                _isProcessingPurchase = true;

                // Make sure duelsit has enough star chips
                uint price = _currentDuelist.Series.SpecialProductPricePerCard * sp.NumCards;
                if (_currentDuelist.NumStarChips < price)
                {
                    return;
                }

                // If not currently showing cards for this product, show them
                if (!_retrievedSpecialProductIds.ContainsKey(sp.SpecialProductId))
                {
                    _retrievedSpecialProductIds.Add(sp.SpecialProductId, 0);
                }

                uint newStarChips = 0;
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    foreach (SpecialProductCard spCard in sp.Cards)
                    {
                        Card card = _currentDuelist.CardCollection.FirstOrDefault(card => card.InfoId == (ulong)spCard.CardInfoId);

                        if (card == null)
                        {
                            // We need to make a new card for this user
                            await dbContext.Cards.AddAsync(new Card()
                            {
                                OwnerId = _currentDuelist.DuelistId,
                                InfoId = (ulong)spCard.CardInfoId,
                                NumCollection = 1,
                                DateObtained = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            // This card exists already for this duelist, update the card count
                            card.NumCollection++;

                            // If we have more than 3, convert the excess into Star Chips
                            if (card.NumCollection > 3)
                            {
                                newStarChips += NewCardModel.GetStarChipWorthForRarity(spCard.Rarity);
                                card.NumCollection = 3;
                            }

                            dbContext.Cards.Update(card);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    _retrievedSpecialProductIds[sp.SpecialProductId] = newStarChips;
                }

                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    // Add any new starchips to the duelists balance
                    if (newStarChips > 0)
                    {
                        _currentDuelist.NumStarChips += newStarChips;
                    }

                    // Don't forget to reduce the user's star chips by the price of the product
                    if (_currentDuelist.NumStarChips < price)
                    {
                        _currentDuelist.NumStarChips = 0;
                    }
                    else
                    {
                        _currentDuelist.NumStarChips -= price;
                    }

                    dbContext.Duelists.Update(_currentDuelist);
                    await dbContext.SaveChangesAsync();
                }

                await AssignCurrentDuelistAndCardsAsync();
                StateHasChanged();
            }
            finally
            {
                _isProcessingPurchase = false;
            }
        }
    }
}
