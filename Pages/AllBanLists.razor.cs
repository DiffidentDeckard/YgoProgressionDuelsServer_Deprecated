using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Pages
{
    public partial class AllBanLists : ComponentBase
    {
        [Inject]
        protected IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [BindProperty]
        List<BanList> _banLists { get; set; } = new List<BanList>();

        [BindProperty]
        private BanList _currentBanList { get; set; }

        [BindProperty]
        private Guid? _currentBanListId { get; set; }

        [BindProperty]
        private bool _isLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                await base.OnInitializedAsync();

                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _banLists = await dbContext.BanLists.AsNoTracking()
                        .Include(banlist => banlist.Entries)
                        .ThenInclude(entry => entry.CardInfo)
                        .AsSplitQuery()
                        .OrderBy(banlist => banlist.ReleaseDate).ToListAsync();
                }
                _currentBanList = _banLists.First();
                _currentBanListId = _currentBanList.BanListId;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void OnSelectedBanListChanged(Guid? banlistId)
        {
            if (_currentBanListId != banlistId)
            {
                _currentBanListId = banlistId;
                _currentBanList = _banLists.First(banList => banList.BanListId == banlistId);
            }
        }
    }
}
