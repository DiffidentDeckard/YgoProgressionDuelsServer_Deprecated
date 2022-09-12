using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using YgoProgressionDuels.Models;

namespace YgoProgressionDuels.Pages.Shared
{
    public partial class NewCardsList : ComponentBase
    {
        [Parameter]
        public IList<NewCardModel> NewCards { get; set; }

        [BindProperty]
        private List<NewCardModel> _sortedNewCards { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _sortedNewCards = NewCards.OrderByDescending(card => card.StarChipWorth).ThenBy(card => card.Rarity).ThenByDescending(card => card.CardIsNew).ToList();
        }
    }
}
