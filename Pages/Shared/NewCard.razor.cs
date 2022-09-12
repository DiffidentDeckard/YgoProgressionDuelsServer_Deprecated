using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YgoProgressionDuels.Models;

namespace YgoProgressionDuels.Pages.Shared
{
    public partial class NewCard : ComponentBase
    {
        [Parameter]
        public NewCardModel CardModel { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }
    }
}
