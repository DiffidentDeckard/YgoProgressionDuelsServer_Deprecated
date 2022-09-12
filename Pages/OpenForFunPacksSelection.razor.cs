using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Pages
{
    [AllowAnonymous]
    public partial class OpenForFunPacksSelection : ComponentBase
    {
        [Inject]
        public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [BindProperty]
        private IList<BoosterPackInfo> _allBoosterPackInfos { get; set; } = new List<BoosterPackInfo>();

        [BindProperty]
        private bool _isLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _allBoosterPackInfos = await dbContext.BoosterPackInfos.AsNoTracking()
                        .OrderBy(info => info.ReleaseDate).ToListAsync();
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }
    }
}
