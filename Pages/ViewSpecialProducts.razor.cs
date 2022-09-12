using System;
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
    public partial class ViewSpecialProducts : ComponentBase
    {
        [Inject]
        public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [BindProperty]
        private IList<SpecialProduct> _allSpecialProducts { get; set; } = new List<SpecialProduct>();

        [BindProperty]
        private bool _isLoading { get; set; }

        [BindProperty]
        private HashSet<Guid> _retrievedSpecialProductIds { get; set; } = new HashSet<Guid>();

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _allSpecialProducts = await dbContext.SpecialProducts.AsNoTracking()
                        .Include(product => product.Cards).AsNoTracking()
                        .OrderBy(sp => sp.ReleaseDate).ToListAsync();
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

        private void ToggleRowDetails(Guid specialProductId)
        {
            if (_retrievedSpecialProductIds.Contains(specialProductId))
            {
                _retrievedSpecialProductIds.Remove(specialProductId);
            }
            else
            {
                _retrievedSpecialProductIds.Add(specialProductId);
            }
        }
    }
}
