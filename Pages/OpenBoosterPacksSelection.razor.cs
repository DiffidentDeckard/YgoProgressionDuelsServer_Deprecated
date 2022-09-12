using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Pages.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class OpenBoosterPacksSelection : AuthorizedComponentBase
    {
        [Parameter]
        public string DuelistId { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private List<BoosterPack> _availableBoosterPacks { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentDuelistAndBoosterPacks();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentDuelistAndBoosterPacks()
        {
            // Parse the Guid and find this progression series
            if (Guid.TryParse(DuelistId, out Guid duelistId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentDuelist = await dbContext.Duelists.AsNoTracking()
                        .Include(duelist => duelist.Series)
                        .Include(duelist => duelist.BoosterPacks)
                        .ThenInclude(boosterPack => boosterPack.PackInfo)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(duelist => duelist.OwnerId.Equals(CurrentUserId)
                            && duelist.DuelistId.Equals(duelistId));
                }

                if (_currentDuelist != null)
                {
                    _availableBoosterPacks = _currentDuelist.BoosterPacks
                        .Where(BoosterPack => BoosterPack.NumAvailable > 0)
                        .OrderBy(pack => pack.PackInfo.ReleaseDate).ToList();
                }
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }
    }
}
