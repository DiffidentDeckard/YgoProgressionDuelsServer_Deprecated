using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages
{
    [AllowAnonymous]
    public partial class OpenForFunPacks : ComponentBase
    {
        [Parameter]
        public string BoosterPackInfoId { get; set; }

        [Inject]
        public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [BindProperty]
        private BoosterPackInfo _currentBoosterPackInfo { get; set; }

        [BindProperty]
        private IList<NewCardModel> _newlyPulledCards { get; set; }

        [BindProperty]
        private bool _isWorking { get; set; } = false;

        [BindProperty]
        private string _openBoosterPackMessage { get; set; }

        [BindProperty]
        private string _openBoosterPackErrorMessage { get; set; }

        [BindProperty]
        private bool _isLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                if (Guid.TryParse(BoosterPackInfoId, out Guid packInfoId))
                {
                    using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                    {
                        _currentBoosterPackInfo = await dbContext.BoosterPackInfos.AsNoTracking().FirstOrDefaultAsync(info => info.BoosterPackInfoId.Equals(packInfoId));
                    }
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

        private async Task OnOpenBoosterPacksAsync(uint numPacksToOpen)
        {
            _openBoosterPackErrorMessage = string.Empty;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                // Start the loading indicators
                _isWorking = true;

                if (_currentBoosterPackInfo.IsStructureDeck)
                {
                    // Open the structure deck and get all the cards
                    await OpenStructureDecksAsync(numPacksToOpen);
                }
                else
                {
                    // Open the booster packs and get all the cards
                    await OpenBoosterPacksAsync(numPacksToOpen);
                }
            }
            catch (Exception ex)
            {
                _openBoosterPackErrorMessage = ex.Message + ex.InnerException?.Message;
            }
            finally
            {
                stopwatch.Stop();
                _openBoosterPackMessage = $"Time Elapsed: {stopwatch.Elapsed.ToString("g")}";

                // Make sure we always return to normal
                _isWorking = false;
            }
        }

        private async Task OpenBoosterPacksAsync(uint numPacksToOpen)
        {
            // Create request to open a booster pack
            RestClient packOpenerClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest packOpenerRequest = new(Constants.OPENBOOSTERPACK_ENDPOINT, Method.GET);
            packOpenerRequest.AddParameter("format", _currentBoosterPackInfo.SetName);
            packOpenerRequest.AddParameter("settype", _currentBoosterPackInfo.SetType);

            // Open the booster packs and create the Card objects to display to user
            ConcurrentDictionary<ulong, NewCardModel> pulledCardsById = new ConcurrentDictionary<ulong, NewCardModel>();
            var openBoosterPackTasks = Enumerable.Range(0, (int)numPacksToOpen).AsParallel().Select(async i =>
            {
                // Open a single booster pack
                IRestResponse packOpenerResponse = await packOpenerClient.ExecuteAsync(packOpenerRequest);
                JToken jPulledCards = JsonConvert.DeserializeObject<JArray>(packOpenerResponse.Content)?.First;

                // Work through the cards pulled in this pack
                foreach (JToken jPulledCard in jPulledCards)
                {
                    ulong cardInfoId = jPulledCard.Value<ulong>("id");

                    if (pulledCardsById.ContainsKey(cardInfoId))
                    {
                        // This card already exists, increment the count
                        pulledCardsById[cardInfoId].Count++;
                    }
                    else
                    {
                        // Add new card to list
                        CardInfo cardInfo = await GetCardInfoAsync(cardInfoId, jPulledCard.Value<string>("name"));

                        if (cardInfo != null)
                        {
                            // Create new card model for view
                            string rarity = jPulledCard.Value<string>("rarity");
                            NewCardModel newCardModel = new NewCardModel(cardInfo, 1, rarity);

                            if (!pulledCardsById.TryAdd(cardInfo.CardInfoId, newCardModel))
                            {
                                // If we failed to add, another thread beat us to it, just increment
                                pulledCardsById[cardInfo.CardInfoId].Count++;
                            }
                        }
                    }
                }
            });
            await Task.WhenAll(openBoosterPackTasks);
            _newlyPulledCards = pulledCardsById.Values.ToList();
        }

        /// <summary>
        /// Opens a structure deck, granting the player one copy of every card in that structure deck (per structure deck opened).
        /// </summary>
        /// <param name="numPacksToOpen"></param>
        /// <returns></returns>
        private async Task OpenStructureDecksAsync(uint numPacksToOpen)
        {
            // Get the list of cards in this structure deck
            List<StructureDeckCard> sdCards;
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                sdCards = await dbContext.StructureDeckCards.AsNoTracking()
                    .Where(sdc => sdc.StructureDeckSetCode.Equals(_currentBoosterPackInfo.SetCode))
                    .ToListAsync();
            }

            // Create the dictionary to return
            List<NewCardModel> newCards = new List<NewCardModel>();
            foreach (StructureDeckCard sdCard in sdCards)
            {
                CardInfo cardInfo = await GetCardInfoAsync((ulong)sdCard.CardInfoId, sdCard.CardInfoName);

                if (cardInfo == null)
                {
                    continue;
                }

                NewCardModel ncm = new NewCardModel(cardInfo, numPacksToOpen, sdCard.Rarity);
                newCards.Add(ncm);
            }

            _newlyPulledCards = newCards;
        }

        private async Task<CardInfo> GetCardInfoAsync(ulong cardInfoId, string cardName)
        {
            CardInfo cardInfo;
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                cardInfo = await dbContext.CardInfos.AsNoTracking().FirstOrDefaultAsync(info => info.CardInfoId.Equals(cardInfoId));

                // On rare occasions, the API call to Get all the cards will actually be missing a different ID of the same card.
                // For example, "Final Flame" has both IDs 73134081 and 73134082, but only one exists in the main call.
                // So for these one-off cases, we'll just search by card name instead
                if (cardInfo == null)
                {
                    cardInfo = await dbContext.CardInfos.AsNoTracking().FirstOrDefaultAsync(info => info.Name.Equals(cardName));
                }
            }

            return cardInfo;
        }
    }
}
