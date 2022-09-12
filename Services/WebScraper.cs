using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Services
{
    public class WebScraper : IWebScraper
    {
        private static Timer _boosterPackWebScrapeTimer;

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public WebScraper(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Initialize()
        {
            if (_boosterPackWebScrapeTimer == null)
            {
                _boosterPackWebScrapeTimer = new Timer(ScrapeWebAsync, null, 0, (int)TimeSpan.FromHours(24).TotalMilliseconds);
            }
        }

        private async void ScrapeWebAsync(object state)
        {
            try
            {
                // Check for any new cards to save to database
                await GetNewCardsAsync();
            }
            catch { }

            try
            {
                // First, get the new booster packs available to us from the pack opener app
                // If this app is starting up, all the packs will be "new" to us
                HashSet<BoosterPackInfo> newPacks = await GetNewPackOpenerPacksAsync();

                if (newPacks.Any())
                {
                    // For each of the new packs, we want to get their release dates and add them to our database
                    await GetPackReleaseDatesAndAddToDatabaseAsync(newPacks);
                }
            }
            catch { }

            try
            {
                // Check for any new structure decks to save to database
                await GetNewStructureDecksAsync();
            }
            catch { }

            try
            {
                // Check for any new special product to save to the database
                await GetNewSpecialProductAsync();
            }
            catch { }

            try
            {
                // Check for new banlist
                await GetNewBanlistAsync();
            }
            catch { }
        }

        /// <summary>
        /// Gets a list of all existing structure decks, and adds any that we are missing to the database
        /// </summary>
        /// <returns></returns>
        private async Task GetNewStructureDecksAsync()
        {
            // Execute an HTTP Request to get all of the set information
            // (this endpoint doesn't accept any parameters for me to filter it down)
            RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest getAllSetsRequest = new(Constants.GET_CARDSETS_ENDPOINT, Method.GET);
            IRestResponse getAllSetsResponse = await ygoProDeckClient.ExecuteAsync(getAllSetsRequest);

            // Load the response into JArray so we don't have to dig through raw JSON ourselves
            JArray jAllSets = JsonConvert.DeserializeObject<JArray>(getAllSetsResponse.Content);

            // Find all existing Structure Decks
            List<JToken> existingStructureDecks = jAllSets
                .Where(set => (set.Value<string>("set_name").Contains("structure", StringComparison.CurrentCultureIgnoreCase) // structure decks
                        || set.Value<string>("set_name").Contains("starter", StringComparison.CurrentCultureIgnoreCase) // starter decks
                        || set.Value<string>("set_name").Contains("deck", StringComparison.CurrentCultureIgnoreCase)) // other decks that don't have starter or structure in the name
                    && !set.Value<string>("set_name").Contains("speed duel", StringComparison.CurrentCultureIgnoreCase) // ignore speed duel decks
                    && !(set.Value<string>("set_code").Equals("YS15", StringComparison.CurrentCultureIgnoreCase)
                        && !set.Value<string>("set_name").Contains("player", StringComparison.CurrentCultureIgnoreCase)) // There are two starter decks, and a double pack with both of them, that all contain the same set code. We want only the combined product
                    && set.Value<int>("num_of_cards") > 25) // ignore the "special editions" with only a couple cards in it
                .ToList();

            // Get all of the Structure Decks currently in our database
            List<string> dbStructureDecks;
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                dbStructureDecks = await dbContext.BoosterPackInfos.AsNoTracking().Where(packinfo => packinfo.IsStructureDeck).Select(packinfo => packinfo.SetCode).ToListAsync();
            }

            // Get only the structure decks we dont have in the database yet
            List<JToken> newStructureDeckTokens = existingStructureDecks.Where(sd => !dbStructureDecks.Contains(sd.Value<string>("set_code"))).ToList();

            if (!newStructureDeckTokens.Any())
            {
                // There are no new structure decks
                return;
            }

            List<BoosterPackInfo> newStructureDecks = new List<BoosterPackInfo>();
            foreach (JToken sdToken in newStructureDeckTokens)
            {
                string setName = sdToken.Value<string>("set_name");
                string setCode = sdToken.Value<string>("set_code");

                BoosterPackInfo newStructureDeck = new BoosterPackInfo()
                {
                    SetName = setName,
                    SetCode = setCode,
                    ImageUrl = $"{Constants.YGOPRODECK_URL}/pics_sets/{setCode}.jpg",
                    SetInfoUrl = $"{Constants.YGOPRODECK_DBURL}/set/?search={HttpUtility.UrlEncodeUnicode(setName)}",
                    ReleaseDate = DateTime.ParseExact(sdToken.Value<string>("tcg_date"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    IsStructureDeck = true
                };

                newStructureDecks.Add(newStructureDeck);
            }

            // Now that we have all the info for the structure decks, save to database
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.BoosterPackInfos.AddRangeAsync(newStructureDecks);
                await dbContext.SaveChangesAsync();
            }

            // Save all of the cards in each new structure deck to db
            foreach (BoosterPackInfo newStructureDeck in newStructureDecks)
            {
                await GetCardsForStructureDeckAsync(newStructureDeck);
            }
        }

        private async Task GetCardsForStructureDeckAsync(BoosterPackInfo structureDeck)
        {
            // Get the response with all the card infos for this structure deck
            RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest getStructureDeckCardsRequest = new(Constants.GET_CARDINFO_ENDPOINT, Method.GET);
            getStructureDeckCardsRequest.AddParameter("cardset", structureDeck.SetName);
            IRestResponse getStructureDeckCardsResponse = await ygoProDeckClient.ExecuteAsync(getStructureDeckCardsRequest);
            JToken jCards = JsonConvert.DeserializeObject<JToken>(getStructureDeckCardsResponse.Content).First.First;

            List<StructureDeckCard> newStructureDeckCards = new List<StructureDeckCard>();
            foreach (JToken jCard in jCards)
            {
                // Get the rarity of this card within the structure deck
                string rarity = jCard.Value<JToken>("card_sets")
                    .First(token => token.Value<string>("set_name").Equals(structureDeck.SetName))
                    .Value<string>("set_rarity").Trim();

                StructureDeckCard newSDC = new StructureDeckCard()
                {
                    StructureDeckSetCode = structureDeck.SetCode,
                    CardInfoId = jCard.Value<long>("id"),
                    CardInfoName = jCard.Value<string>("name"),
                    Rarity = rarity
                };

                newStructureDeckCards.Add(newSDC);
            }

            // Now that we have all the info for the structure deck cards, save to database
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.StructureDeckCards.AddRangeAsync(newStructureDeckCards);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets a list of all existing special products, and adds any that we are missing to the database
        /// </summary>
        /// <returns></returns>
        private async Task GetNewSpecialProductAsync()
        {
            // Execute an HTTP Request to get all of the set information
            // (this endpoint doesn't accept any parameters for me to filter it down)
            RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest getAllSetsRequest = new(Constants.GET_CARDSETS_ENDPOINT, Method.GET);
            IRestResponse getAllSetsResponse = await ygoProDeckClient.ExecuteAsync(getAllSetsRequest);

            // Load the response into JArray so we don't have to dig through raw JSON ourselves
            JArray jAllSets = JsonConvert.DeserializeObject<JArray>(getAllSetsResponse.Content);

            // Get all of the Booster Packs and Structure Decks currently in our database
            List<string> dbProducts;
            List<string> dbSpecialProductSetNames;
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                dbProducts = await dbContext.BoosterPackInfos.AsNoTracking().Select(packinfo => packinfo.SetName).ToListAsync();
                dbSpecialProductSetNames = await dbContext.SpecialProducts.AsNoTracking().Select(sp => sp.SetName).ToListAsync();
            }


            // We will consider any product that has 15 or less cards to be a product containing promo cards, such as "Special Editions" of a Strucker Deck or Booster Pack
            // Ignore anything that has the exact same name as something in the actual boosterpack/structuredeck table
            List<JToken> newSpecialProductTokens = jAllSets
                .Where(set => set.Value<int>("num_of_cards") < 20
                    && !set.Value<string>("set_name").Contains("speed duel", StringComparison.CurrentCultureIgnoreCase)
                    && !set.Value<string>("set_name").Contains("mcdonald", StringComparison.CurrentCultureIgnoreCase)
                    && !dbProducts.Any(product => product.Equals(set.Value<string>("set_name"), StringComparison.CurrentCultureIgnoreCase))
                    && !dbSpecialProductSetNames.Any(product => product.Equals(set.Value<string>("set_name"), StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

            if (!newSpecialProductTokens.Any())
            {
                // There are no new special products
                return;
            }

            List<SpecialProduct> newSpecialProducts = new List<SpecialProduct>();
            foreach (JToken spToken in newSpecialProductTokens)
            {
                string setName = spToken.Value<string>("set_name");
                string setCode = spToken.Value<string>("set_code");
                uint numCards = spToken.Value<uint>("num_of_cards");

                string tcgDate = spToken.Value<string>("tcg_date");
                bool dateTimeParsed = DateTime.TryParseExact(tcgDate, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime releaseDate);
                if (!dateTimeParsed)
                {
                    // Some of these products have null dates, ignore them entirely
                    continue;
                }

                SpecialProduct newSpecialProduct = new SpecialProduct()
                {
                    SetName = setName,
                    SetCode = setCode,
                    NumCards = numCards,
                    ReleaseDate = releaseDate,
                    ImageUrl = $"{Constants.YGOPRODECK_URL}/pics_sets/{setCode}.jpg",
                    SetInfoUrl = $"{Constants.YGOPRODECK_DBURL}/set/?search={HttpUtility.UrlEncodeUnicode(setName)}"
                };

                newSpecialProducts.Add(newSpecialProduct);
            }

            // Now that we have all the info for the special products, save to database
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.SpecialProducts.AddRangeAsync(newSpecialProducts);
                await dbContext.SaveChangesAsync();
            }

            // Save all of the cards in each new structure deck to db
            foreach (SpecialProduct specialProduct in newSpecialProducts)
            {
                await GetCardsForSpecialProductAsync(specialProduct);
            }
        }

        private async Task GetCardsForSpecialProductAsync(SpecialProduct specialProduct)
        {
            // Get the response with all the card infos for this special product
            RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest getSpecialProductCardsRequest = new(Constants.GET_CARDINFO_ENDPOINT, Method.GET);
            getSpecialProductCardsRequest.AddParameter("cardset", specialProduct.SetName);
            IRestResponse getSpecialProductCardsResponse = await ygoProDeckClient.ExecuteAsync(getSpecialProductCardsRequest);
            JToken jCards = JsonConvert.DeserializeObject<JToken>(getSpecialProductCardsResponse.Content).First.First;

            List<SpecialProductCard> newSpecialProductCards = new List<SpecialProductCard>();
            foreach (JToken jCard in jCards)
            {
                // Get the rarity of this card within the special product
                string rarity = jCard.Value<JToken>("card_sets")
                    .First(token => token.Value<string>("set_name").Equals(specialProduct.SetName, StringComparison.CurrentCultureIgnoreCase))
                    .Value<string>("set_rarity").Trim();

                SpecialProductCard newSPC = new SpecialProductCard()
                {
                    OwnerProductId = specialProduct.SpecialProductId,
                    CardInfoId = jCard.Value<long>("id"),
                    CardInfoName = jCard.Value<string>("name"),
                    Rarity = rarity
                };

                // Sometimes the ID we have for this card wont be one that exists in the database, it might be a slightly different one. Check it
                using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
                {
                    CardInfo correctInfo = await dbContext.CardInfos.AsNoTracking().FirstOrDefaultAsync(cardInfo => cardInfo.Name.Equals(newSPC.CardInfoName));

                    if (correctInfo != null)
                    {
                        // Give it the correct Id
                        newSPC.CardInfoId = (long)correctInfo.CardInfoId;
                    }
                    else
                    {
                        // No such card exists in the db, ignore this card
                        continue;
                    }
                }

                newSpecialProductCards.Add(newSPC);
            }

            // Now that we have all the info for the special product cards, save to database
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.SpecialProductCards.AddRangeAsync(newSpecialProductCards);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This will scrape through the booster packs in the simulated pack opening site.
        /// It will then return only those that we do not already have in our database.
        /// </summary>
        /// <returns></returns>
        private async Task<HashSet<BoosterPackInfo>> GetNewPackOpenerPacksAsync()
        {
            IList<BoosterPackInfo> BoosterPackInfos;
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                BoosterPackInfos = await dbContext.BoosterPackInfos.AsNoTracking().ToListAsync();
            }

            HtmlWeb web = new();
            HtmlDocument document = web.Load(Constants.PACKOPENER_URL);

            // First remove all the OCG packs, we don't want them
            document.DocumentNode.Descendants("span").Where(s => s.GetAttributeValue("class", string.Empty).Contains("ocg-sets")).SingleOrDefault()?.Remove();

            // All of the buttons in the pack opener site that open packs have the "format-button-sets" class
            IEnumerable<HtmlNode> setButtons = document.DocumentNode.Descendants("button").Where(d => d.GetAttributeValue("class", string.Empty).Contains("format-button-sets"));

            HashSet<BoosterPackInfo> packOpenerPacks = new();
            string startgame = "startgame('"; // This is the string we will be searching for in the button command

            foreach (HtmlNode setButton in setButtons)
            {
                // Get the exact name we will need to use in order to open one of these packs (very specific, in the onClick command)
                string onClickString = setButton.GetAttributeValue("onclick", string.Empty);
                int startIndex = onClickString.IndexOf(startgame) + startgame.Length;
                int endIndex = onClickString.IndexOf("',", startIndex); // Search for the next "'," after our start position. Thats the end of the string we are looking for
                string packOpenerName = onClickString[startIndex..endIndex].Replace("\\", string.Empty).Trim();

                if (packOpenerName.Contains("speed duel", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Ignore Speed Duel packs
                    continue;
                }

                if (BoosterPackInfos.Any(boosterPack => boosterPack.SetName.Equals(packOpenerName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    // We already have this booster pack in our collection, move on
                    continue;
                }

                // Get the Set Type 
                string[] onclickSegments = onClickString.Split(',');
                string typeString = onclickSegments[1].Trim(' ', '\t', '\n', '\'', ',');
                int packOpenerType = int.Parse(typeString);

                // Get the image url used for this booster pack
                string imageUrl = setButton.Descendants("img").Single().GetAttributeValue("data-src", string.Empty);

                // Make the BoosterPack object
                packOpenerPacks.Add(new BoosterPackInfo(packOpenerName, packOpenerType, imageUrl));
            }

            return packOpenerPacks;
        }

        private async Task GetPackReleaseDatesAndAddToDatabaseAsync(HashSet<BoosterPackInfo> newPacks)
        {
            // Execute an HTTP Request to get all of the set information
            // (this endpoint doesn't accept any parameters for me to filter it down)
            RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest getAllSetsRequest = new(Constants.GET_CARDSETS_ENDPOINT, Method.GET);
            IRestResponse getAllSetsResponse = await ygoProDeckClient.ExecuteAsync(getAllSetsRequest);

            // Load the response into JArray so we don't have to dig through raw JSON ourselves
            JArray jAllSets = JsonConvert.DeserializeObject<JArray>(getAllSetsResponse.Content);

            foreach (BoosterPackInfo boosterPack in newPacks)
            {
                // Find all our jSets that match this booster pack's name (though the name is often wrong from the pack opener site)
                List<JToken> matchingSets = jAllSets.Where(set => set.Value<string>("set_name").Equals(boosterPack.SetName)).ToList();

                if (!matchingSets.Any())
                {
                    // Often the name is wrong, try checking the code instead (also wrong sometimes)
                    matchingSets = jAllSets.Where(set => set.Value<string>("set_code").Equals(boosterPack.SetCode)).ToList();
                }

                if (!matchingSets.Any())
                {
                    // We failed to find release date for this set, I'll have to look into this
                    // At the time of writing this, all the sets should be found
                    continue;
                }

                // If we have multiple matches we want the one with the shortest name since that will be the "main" set 
                // For example, we dont want the "Special Edition" or "Sneak Peek" values of a set, just the plain main set
                JToken matchingSet = matchingSets.OrderBy(set => set.Value<string>("set_name").Length).First();

                // We grabbed an "initial" set code from the image url, but that purely to help us find the sets where the name didnt match up
                // In truth, many of those set codes would be incorrect, so lets use the set code we grab here instead
                boosterPack.SetCode = matchingSet.Value<string>("set_code");
                boosterPack.SetInfoUrl = $"{Constants.YGOPRODECK_DBURL}/set/?search={HttpUtility.UrlEncodeUnicode(matchingSet.Value<string>("set_name"))}";
                boosterPack.ReleaseDate = DateTime.ParseExact(matchingSet.Value<string>("tcg_date"), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            // Now that we have all the info for the booster packs, save to database
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.BoosterPackInfos.AddRangeAsync(newPacks);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task GetNewCardsAsync()
        {
            // Get the response with all the card infos
            RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
            RestRequest getAllCardsRequest = new(Constants.GET_CARDINFO_ENDPOINT, Method.GET);
            IRestResponse getAllCardsResponse = await ygoProDeckClient.ExecuteAsync(getAllCardsRequest);
            JToken jCards = JsonConvert.DeserializeObject<JToken>(getAllCardsResponse.Content).First.First;

            List<ulong> cardInfoIds;
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                cardInfoIds = await dbContext.CardInfos.AsNoTracking().Select(ci => ci.CardInfoId).ToListAsync();
            }

            // Go through all of the cards and save any new ones
            var saveNewCardTasks = jCards.AsParallel().Select(async jCardInfo =>
            {
                ulong cardInfoId = jCardInfo.Value<ulong>("id");

                if (!cardInfoIds.Contains(cardInfoId))
                {
                    // New card, save it
                    string cardName = jCardInfo.Value<string>("name");

                    CardInfo newCardInfo = new CardInfo()
                    {
                        CardInfoId = cardInfoId,
                        Name = cardName,
                        CardInfoUrl = $"{Constants.YGOPRODECK_DBURL}/card/?search={HttpUtility.UrlEncodeUnicode(cardName)}",
                        ImageUrl = $"{Constants.CARDIMAGE_URL}{cardInfoId}.jpg",
                        Type = jCardInfo.Value<string>("type"),
                        Description = jCardInfo.Value<string>("desc"),
                        Race = jCardInfo.Value<string>("race"),
                        Attribute = jCardInfo.Value<string>("attribute"),
                        Scale = jCardInfo.Value<uint?>("scale"),
                        Link = jCardInfo.Value<uint?>("linkval"),
                        Level = jCardInfo.Value<uint?>("level"),
                        ATK = jCardInfo.Value<uint?>("atk"),
                        DEF = jCardInfo.Value<uint?>("def"),
                        TreatedAs = jCardInfo.Value<string>("treated_as")
                    };

                    using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
                    {
                        await dbContext.CardInfos.AddAsync(newCardInfo);
                        await dbContext.SaveChangesAsync();
                    }
                }
            });
            await Task.WhenAll(saveNewCardTasks);
        }

        /// <summary>
        /// Checks the web for a new banlist, and adds it to our database
        /// </summary>
        /// <returns></returns>
        private async Task GetNewBanlistAsync()
        {
            HtmlWeb web = new();
            HtmlDocument document = web.Load(Constants.YUGIOH_BANLIST);

            // First let's check what date this banlist is for
            string effectiveFrom = "Effective from";
            string effectiveFromDateString = document.GetElementbyId("Titlebox").Descendants("td").FirstOrDefault().InnerText;
            effectiveFromDateString = effectiveFromDateString.Substring(effectiveFromDateString.IndexOf(effectiveFrom) + effectiveFrom.Length).Trim();
            DateTime effectiveFromDate = DateTime.Parse(effectiveFromDateString);

            // Check if we already have a banlist for this date
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                if (await dbContext.BanLists.AsNoTracking().AnyAsync(banList => banList.ReleaseDate.Equals(effectiveFromDate)))
                {
                    // We already have this banlist, leave
                    return;
                }
            }

            // New banlist, create it
            BanList newBanList = new BanList() { ReleaseDate = effectiveFromDate };

            // Grab all the <tr> elements from the page for processing
            IEnumerable<HtmlNode> tableRows = document.DocumentNode.Descendants("tr");

            foreach (HtmlNode tableRow in tableRows)
            {
                HtmlNode[] cols = tableRow.Descendants("td").ToArray();

                if (cols.Length < 3)
                {
                    continue;
                }

                // Get card name and limit
                string cardName = HttpUtility.HtmlDecode(cols[1].InnerText.Trim());

                while (cardName.IndexOf("  ") > -1)
                {
                    // Some of the card's have html with extra spaces inside the name, so we gotta fix that or the db won't find this name
                    cardName = cardName.Replace("  ", " ");
                }
                cardName = cardName.Replace("â\u0080\u0090", "-"); // For some reason "Draco Face-Off" has stupid crap instead of the hyphen

                string cardLimit = HttpUtility.HtmlDecode(cols[2].InnerText.Trim());
                BanListStatus banListStatus = BanListStatus.Unlimited;

                if (cardLimit.Contains("Semi-Limited", StringComparison.CurrentCultureIgnoreCase))
                {
                    banListStatus = BanListStatus.SemiLimited;
                }
                else if (cardLimit.Contains("Limited", StringComparison.CurrentCultureIgnoreCase))
                {
                    banListStatus = BanListStatus.Limited;
                }
                else if (cardLimit.Contains("Forbidden", StringComparison.CurrentCultureIgnoreCase))
                {
                    banListStatus = BanListStatus.Forbidden;
                }
                else
                {
                    // Not a row that has a banlist entry on it
                    continue;
                }

                // Get the cardinfo id for this card
                ulong cardInfoId = 0;
                using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
                {
                    CardInfo info = await dbContext.CardInfos.AsNoTracking().FirstOrDefaultAsync(info => info.Name.Equals(cardName));
                    if (info == null)
                    {
                        // For some reason this card does not exist in our database, we shall skip it
                        continue;
                    }
                    cardInfoId = info.CardInfoId;
                }

                BanListEntry newEntry = new BanListEntry()
                {
                    OwnerBanList = newBanList,
                    CardInfoId = cardInfoId,
                    BanListStatus = banListStatus
                };

                newBanList.Entries.Add(newEntry);
            }

            // Add the new banlist and its entries to the db
            using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
            {
                await dbContext.BanLists.AddAsync(newBanList);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task ReadFileForBanlists()
        {
            string filepath = @"C:\Users\arria\Downloads\TCGCombiList.conf";

            System.IO.StreamReader reader = new System.IO.StreamReader(filepath);
            string line;

            BanList banList = null;

            do
            {
                line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line[0] == '!')
                {
                    // This is a new banlist
                    line = line.Substring(1).Trim();
                    try
                    {
                        DateTime releaseDate = DateTime.ParseExact(line, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        banList = new BanList() { ReleaseDate = releaseDate };

                        using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
                        {
                            await dbContext.BanLists.AddAsync(banList);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    catch { /* ignore */ }
                }
                else
                {
                    try
                    {
                        var lineParts = line.Trim().Split(new char[] { ' ' });
                        ulong cardId = ulong.Parse(lineParts[0]);

                        using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
                        {
                            CardInfo info = await dbContext.CardInfos.AsNoTracking().FirstOrDefaultAsync(info => info.CardInfoId == cardId);
                            if (info == null)
                            {
                                // ignore
                            }
                        }

                        uint limit = uint.Parse(lineParts[1]);
                        BanListEntry entry = new BanListEntry() { OwnerBanListId = banList.BanListId, CardInfoId = cardId, BanListStatus = (BanListStatus)limit };
                        banList.Entries.Add(entry);

                        using (ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext())
                        {
                            await dbContext.BanListEntries.AddAsync(entry);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    catch { /* ignore */ }
                }
            }
            while (line != null);

            reader.Close();
        }
    }
}
