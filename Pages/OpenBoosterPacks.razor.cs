using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;
using YgoProgressionDuels.Pages.Shared;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class OpenBoosterPacks : AuthorizedComponentBase
    {
        [Parameter]
        public string DuelistId { get; set; }

        [Parameter]
        public string BoosterPackInfoId { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private BoosterPack _currentBoosterPack { get; set; }

        [BindProperty]
        private IList<NewCardModel> _newlyPulledCards { get; set; }

        [BindProperty]
        private uint _newStarChips { get; set; }

        [BindProperty]
        private bool _isWorking { get; set; } = false;

        [BindProperty]
        private string _openBoosterPackMessage { get; set; }

        [BindProperty]
        private string _openBoosterPackErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentDuelistAndBoosterPack();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentDuelistAndBoosterPack()
        {
            // Parse the Guid and find this duelist
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

                if (_currentDuelist != null && Guid.TryParse(BoosterPackInfoId, out Guid packInfoId))
                {
                    _currentBoosterPack = _currentDuelist.BoosterPacks.FirstOrDefault(BoosterPack => BoosterPack.InfoId.Equals(packInfoId));
                }
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }

        private async Task OnOpenBoosterPacksAsync(uint numPacksToOpen)
        {
            if (numPacksToOpen > _currentBoosterPack.NumAvailable)
            {
                _openBoosterPackErrorMessage = "You do not have that many booster packs available to open.";
                return;
            }
            else
            {
                _openBoosterPackErrorMessage = string.Empty;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                // Start the loading indicators
                _isWorking = true;

                ConcurrentDictionary<ulong, NewCardModel> pulledCardsById;
                if (_currentBoosterPack.PackInfo.IsStructureDeck)
                {
                    // Open the structure deck and get all the cards
                    pulledCardsById = await OpenStructureDecksAsync(numPacksToOpen);
                }
                else
                {
                    // Open the booster packs and get all the cards
                    pulledCardsById = await OpenBoosterPacksAsync(numPacksToOpen);
                }

                // Update the database of cards, and the list of cards used for the view
                await UpdateCardDatabase(pulledCardsById);

                // We've saved the cards to database, so now lets subtract the opened packs from the available ones and save it
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    BoosterPack boosterPack = await dbContext.BoosterPacks.FirstAsync(pack => pack.BoosterPackId.Equals(_currentBoosterPack.BoosterPackId));
                    boosterPack.NumAvailable -= numPacksToOpen;
                    boosterPack.NumOpened += numPacksToOpen;
                    await dbContext.SaveChangesAsync();
                }

                await AssignCurrentDuelistAndBoosterPack();

                // Update this duelist's unique card count
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentDuelist.NumUniqueCards = (uint)dbContext.Cards.AsNoTracking().Count(card => card.OwnerId.Equals(_currentDuelist.DuelistId));
                    dbContext.Duelists.Update(_currentDuelist);
                    await dbContext.SaveChangesAsync();
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

        private async Task<ConcurrentDictionary<ulong, NewCardModel>> OpenBoosterPacksAsync(uint numPacksToOpen)
        {
            // Create request to open a booster pack
            RestClient packOpenerClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest packOpenerRequest = new(Constants.OPENBOOSTERPACK_ENDPOINT, Method.GET);
            packOpenerRequest.AddParameter("format", _currentBoosterPack.PackInfo.SetName);
            packOpenerRequest.AddParameter("settype", _currentBoosterPack.PackInfo.SetType);

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
                        string cardName = jPulledCard.Value<string>("name");
                        CardInfo cardInfo = await GetCardInfoAsync(cardInfoId, cardName);

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
            return pulledCardsById;
        }

        /// <summary>
        /// Opens a structure deck, granting the player one copy of every card in that structure deck (per structure deck opened).
        /// </summary>
        /// <param name="numPacksToOpen"></param>
        /// <returns></returns>
        private async Task<ConcurrentDictionary<ulong, NewCardModel>> OpenStructureDecksAsync(uint numPacksToOpen)
        {
            // Get the list of cards in this structure deck
            List<StructureDeckCard> sdCards;
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                sdCards = await dbContext.StructureDeckCards.AsNoTracking()
                    .Where(sdc => sdc.StructureDeckSetCode.Equals(_currentBoosterPack.PackInfo.SetCode))
                    .ToListAsync();
            }

            // Create the dictionary to return
            ConcurrentDictionary<ulong, NewCardModel> newCardsById = new ConcurrentDictionary<ulong, NewCardModel>();
            foreach (StructureDeckCard sdCard in sdCards)
            {
                CardInfo cardInfo = await GetCardInfoAsync((ulong)sdCard.CardInfoId, sdCard.CardInfoName);

                if (cardInfo == null)
                {
                    continue;
                }

                NewCardModel ncm = new NewCardModel(cardInfo, numPacksToOpen, sdCard.Rarity);
                newCardsById.TryAdd(ncm.CardInfo.CardInfoId, ncm);
            }

            // Return the dictionary
            return newCardsById;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pulledCardsbyId"></param>
        /// <returns></returns>
        private async Task UpdateCardDatabase(ConcurrentDictionary<ulong, NewCardModel> pulledCardsbyId)
        {
            _newStarChips = 0;
            var saveCardToDbTasks = pulledCardsbyId.Values.AsParallel().Select(async newCardModel =>
            {
                // Ignore Skill Cards and Tokens
                if (!newCardModel.CardInfo.Type.Equals("Skill Card", StringComparison.CurrentCultureIgnoreCase)
                    && !newCardModel.CardInfo.Type.Equals("Token", StringComparison.CurrentCultureIgnoreCase))
                {
                    using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                    {
                        Card savedCard = await dbContext.Cards.AsNoTracking().FirstOrDefaultAsync(card => card.InfoId.Equals(newCardModel.CardInfo.CardInfoId)
                            && card.OwnerId.Equals(_currentDuelist.DuelistId));

                        if (savedCard == null)
                        {
                            // This card doesn't exist in the db, save it
                            // I use a separate variable to determine how many we save, because I still want the UI to display the total amount pulled (even if it exceeds 3)
                            uint amountToSave = newCardModel.Count;
                            newCardModel.CardIsNew = true;

                            // Any more than 3 get converted
                            if (amountToSave > 3)
                            {
                                _newStarChips += (amountToSave - 3) * newCardModel.StarChipWorth;
                                amountToSave = 3;
                            }

                            await dbContext.Cards.AddAsync(new Card()
                            {
                                OwnerId = _currentDuelist.DuelistId,
                                InfoId = newCardModel.CardInfo.CardInfoId,
                                NumCollection = amountToSave,
                                DateObtained = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            // Update the available number of this card
                            savedCard.NumCollection += newCardModel.Count;

                            // If we have more than 3, convert the excess into Star Chips
                            if (savedCard.NumCollection > 3)
                            {
                                _newStarChips += (savedCard.NumCollection - 3) * newCardModel.StarChipWorth;
                                savedCard.NumCollection = 3;
                            }

                            dbContext.Cards.Update(savedCard);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }
            });
            await Task.WhenAll(saveCardToDbTasks);

            // Update the duelist's star chips
            using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                _currentDuelist.NumStarChips += _newStarChips;
                dbContext.Duelists.Update(_currentDuelist);
                await dbContext.SaveChangesAsync();
            }

            // Update the view
            _newlyPulledCards = pulledCardsbyId.Values.ToList();
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
