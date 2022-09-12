using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Pages.Shared
{
    public partial class ViewBanList : ComponentBase
    {

        [Inject]
        protected IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Parameter]
        public Guid? BanListId { get; set; }

        [BindProperty]
        private BanList _currentBanList { get; set; }

        [BindProperty]
        private List<BanListEntry> _currentBanListEntries { get; set; } = new List<BanListEntry>();

        protected override async Task OnInitializedAsync()
        {
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                _currentBanList = await dbContext.BanLists.AsNoTracking()
                    .Include(banList => banList.Entries)
                    .ThenInclude(entry => entry.CardInfo)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(banList => banList.BanListId.Equals(BanListId));

                if (_currentBanList != null)
                {
                    _currentBanListEntries = _currentBanList.Entries
                        .OrderBy(entry => entry.BanListStatus)
                        .ThenBy(entry => entry.CardInfo.CardCategory)
                        .ThenBy(entry => entry.CardInfo).ToList();
                }
            }
        }
    }
}
