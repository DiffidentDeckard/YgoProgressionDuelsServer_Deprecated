using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class CardSearch : ComponentBase
    {
        [Inject]
        protected IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [BindProperty]
        List<CardInfo> _allCards { get; set; } = new List<CardInfo>();

        [BindProperty]
        private bool _isLoading { get; set; }

        [BindProperty]
        private CardCategory _selectedCardCategory { get; set; } = CardCategory.All;

        [BindProperty]
        private SubCategory _selectedSubCategory { get; set; } = SubCategory.All;

        [BindProperty]
        private MonsterAttribute _selectedMonsterAttribute { get; set; } = MonsterAttribute.All;

        [BindProperty]
        private MonsterType _selectedMonsterType { get; set; } = MonsterType.All;

        [BindProperty]
        private string _levelRankLinkFilterString { get; set; }

        [BindProperty]
        private string _scaleFilterString { get; set; }

        [BindProperty]
        private string _atkFilterString { get; set; }

        [BindProperty]
        private string _defFilterString { get; set; }

        [BindProperty]
        private string _textSearchString { get; set; }

        [BindProperty]
        private SortOption _sortOption1 { get; set; } = SortOption.Default;

        [BindProperty]
        private SortOption _sortOption2 { get; set; } = SortOption.Default;


        public static List<SortOption> SortOptions = new List<SortOption>()
        {
            SortOption.Default,
            SortOption.Name,
            SortOption.Level,
            SortOption.Attack,
            SortOption.Defense
        };

        [BindProperty]
        private Dictionary<ulong, Tuple<string, string, string>> _retrievedCardInfos { get; } = new Dictionary<ulong, Tuple<string, string, string>>();

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _allCards = await dbContext.CardInfos.AsNoTracking()
                        .Where(cardInfo => !cardInfo.Type.Equals("Skill Card")
                            && !cardInfo.Type.Equals("Token"))
                        .ToListAsync();

                    _allCards.Sort();
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

        private async Task ShowRowDetails(ulong cardInfoId)
        {
            Tuple<string, string, string> loadingStrings = new Tuple<string, string, string>("Loading Sets...", "Loading Special Products...", "Loading Banlists...");
            if (_retrievedCardInfos.ContainsKey(cardInfoId))
            {
                _retrievedCardInfos[cardInfoId] = loadingStrings;
            }
            else
            {
                _retrievedCardInfos.Add(cardInfoId, loadingStrings);
            }

            // These will run in the background and we will not wait for them to return before completing code execution
            await Task.Run(async () =>
            {
                await InvokeAsync(async () =>
                {
                    await GetCardSetsAsync(cardInfoId);
                    StateHasChanged();
                    await GetCardBanListsAsync(cardInfoId);
                    StateHasChanged();
                });
            }).ConfigureAwait(false);
        }

        private async Task GetCardSetsAsync(ulong cardInfoId)
        {
            try
            {
                SortedDictionary<BoosterPackInfo, string> cardSets = new SortedDictionary<BoosterPackInfo, string>();
                SortedDictionary<SpecialProduct, string> cardSpecialProducts = new SortedDictionary<SpecialProduct, string>();

                // Execute an HTTP Request to get all of the card information
                RestClient ygoProDeckClient = new(Constants.YGOPRODECK_DBURL);
                RestRequest getCardInfoRequest = new(Constants.GET_CARDINFO_ENDPOINT, Method.GET);
                getCardInfoRequest.AddParameter("id", cardInfoId);
                IRestResponse getCardInfoResponse = await ygoProDeckClient.ExecuteAsync(getCardInfoRequest);
                JToken jCardSets = JsonConvert.DeserializeObject<JToken>(getCardInfoResponse.Content).First.First.First.Value<JToken>("card_sets");

                // Get the full list of products from db so we can only show the ones we have in app
                List<BoosterPackInfo> dbSets;
                List<SpecialProduct> dbSpecialProducts;
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    dbSets = dbContext.BoosterPackInfos.AsNoTracking().ToList();
                    dbSpecialProducts = dbContext.SpecialProducts.AsNoTracking().ToList();
                }

                // Get the relevant parts of the info we want for the string
                foreach (JToken jCardSet in jCardSets)
                {
                    string setCode = jCardSet.Value<string>("set_code");
                    setCode = setCode.Split(new char[] { '-' }).First(); // We only want the setcode, not the full card code

                    string setRarity = jCardSet.Value<string>("set_rarity_code");
                    setRarity = setRarity.Trim(new char[] { '(', ')' }); // We do not want the parentheses at either end
                    if (string.IsNullOrWhiteSpace(setRarity))
                    {
                        setRarity = jCardSet.Value<string>("set_rarity");
                    }

                    // If this is a special product, add it to the special product list and move on
                    string setName = jCardSet.Value<string>("set_name");
                    SpecialProduct specialProduct = dbSpecialProducts.FirstOrDefault(sp => sp.SetName.Equals(setName, StringComparison.CurrentCultureIgnoreCase));
                    if (specialProduct != null)
                    {
                        if (!cardSpecialProducts.ContainsKey(specialProduct))
                        {
                            cardSpecialProducts.Add(specialProduct, $"[{setName}|{setRarity}]");
                        }
                        continue;
                    }

                    // We want to ignore "special editions" and other such editions from this search list
                    if (jCardSet.Value<string>("set_name").Contains("Edition", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    // Get the db info
                    BoosterPackInfo packInfo = dbSets.FirstOrDefault(info => info.SetCode.Equals(setCode));

                    if (packInfo == null)
                    {
                        continue; // This set is not in our DB, ignore it
                    }
                    if (cardSets.ContainsKey(packInfo))
                    {
                        continue; // We've already added this set, ignore it
                    }

                    cardSets.Add(packInfo, $"[{setCode}|{setRarity}]");
                }

                string setsString = cardSets.Any() ? $"Sets/Decks: {string.Join(" ● ", cardSets.Values)}" : string.Empty;
                string productsString = cardSpecialProducts.Any() ? $"Special Products: {string.Join(" ● ", cardSpecialProducts.Values)}" : string.Empty;

                _retrievedCardInfos[cardInfoId] = new Tuple<string, string, string>(setsString, productsString, _retrievedCardInfos[cardInfoId].Item3);
            }
            catch
            {
                _retrievedCardInfos[cardInfoId] = new Tuple<string, string, string>("Error retrieving Card Set info, please try again", string.Empty, _retrievedCardInfos[cardInfoId].Item3);
            }
        }

        private async Task GetCardBanListsAsync(ulong cardInfoId)
        {
            try
            {
                // Get all banlist entries that list this card
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    bool anyBanListWithCard = await dbContext.BanListEntries.AsNoTracking().AnyAsync(entry => entry.CardInfoId == cardInfoId);

                    // If no banlists with this card, don't even show the line
                    if (!anyBanListWithCard)
                    {
                        _retrievedCardInfos[cardInfoId] = new Tuple<string, string, string>(_retrievedCardInfos[cardInfoId].Item1, _retrievedCardInfos[cardInfoId].Item2, string.Empty);
                        return;
                    }
                }

                // Get all banLists
                List<BanList> allBanLists;
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    allBanLists = await dbContext.BanLists.AsNoTracking()
                        .Include(banList => banList.Entries.Where(entry => entry.CardInfoId == cardInfoId)).AsNoTracking()
                        .OrderBy(banList => banList.ReleaseDate)
                        .ToListAsync();
                }

                // For each banlist, if the banlist status of this card is different from the previous banlist, add it in our list of banlsits to show the user
                BanListStatus previousStatus = BanListStatus.Unlimited;
                List<string> cardBanLists = new List<string>();

                foreach (BanList banList in allBanLists)
                {
                    BanListStatus newStatus = banList.Entries.FirstOrDefault()?.BanListStatus ?? BanListStatus.Unlimited;

                    if (previousStatus != newStatus)
                    {
                        cardBanLists.Add($"[{banList.ReleaseDate.ToShortDateString()}|{newStatus}]");
                        previousStatus = newStatus;
                    }
                }

                // Return the relevant banlist entries, ordered by date
                _retrievedCardInfos[cardInfoId] = new Tuple<string, string, string>(_retrievedCardInfos[cardInfoId].Item1, _retrievedCardInfos[cardInfoId].Item2, $"BanLists: {string.Join(" ● ", cardBanLists)}");
            }
            catch
            {
                _retrievedCardInfos[cardInfoId] = new Tuple<string, string, string>(_retrievedCardInfos[cardInfoId].Item1, _retrievedCardInfos[cardInfoId].Item2, "Error retrieving Card BanList info, please try again");
            }
        }

        private void OnSelectedCategoryChanged(CardCategory cardCategory)
        {


            if (_selectedCardCategory != cardCategory)
            {
                _selectedCardCategory = cardCategory;
                _selectedSubCategory = SubCategory.All;
            }
        }

        private bool OnCustomFilter(CardInfo cardInfo)
        {
            return MatchesCategory(cardInfo)
                && MatchesSubCategory(cardInfo)
                && MatchesMonsterAttributeFilter(cardInfo)
                && MatchesMonsterTypeFilter(cardInfo)
                && (MatchesNumericFilter(cardInfo.Level, _levelRankLinkFilterString) || MatchesNumericFilter(cardInfo.Link, _levelRankLinkFilterString))
                && MatchesNumericFilter(cardInfo.Scale, _scaleFilterString)
                && MatchesNumericFilter(cardInfo.ATK, _atkFilterString)
                && MatchesNumericFilter(cardInfo.DEF, _defFilterString)
                && MatchesTextSearch(cardInfo);
        }

        private bool MatchesCategory(CardInfo cardInfo)
        {
            if (_selectedCardCategory == CardCategory.All)
            {
                return true;
            }

            return cardInfo.CardCategory == _selectedCardCategory;
        }

        private bool MatchesSubCategory(CardInfo cardInfo)
        {
            if (_selectedSubCategory == SubCategory.All)
            {
                return true;
            }

            string subCategory = _selectedSubCategory.GetDisplayName();

            if (cardInfo.CardCategory == CardCategory.Monster)
            {
                return cardInfo.Type.Contains(subCategory, StringComparison.CurrentCultureIgnoreCase);
            }

            return cardInfo.Race.Contains(subCategory, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool MatchesMonsterAttributeFilter(CardInfo cardInfo)
        {
            if (_selectedMonsterAttribute == MonsterAttribute.All)
            {
                return true;
            }

            if (cardInfo.Attribute == null)
            {
                return false;
            }

            return cardInfo.Attribute.Equals(_selectedMonsterAttribute.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        private bool MatchesMonsterTypeFilter(CardInfo cardInfo)
        {
            if (_selectedMonsterType == MonsterType.All)
            {
                return true;
            }

            return cardInfo.Race.Equals(_selectedMonsterType.GetDisplayName(), StringComparison.CurrentCultureIgnoreCase);
        }

        private bool MatchesNumericFilter(uint? numericValue, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || !filter.Any(char.IsDigit))
            {
                return true;
            }

            if (numericValue == null)
            {
                return false;
            }

            if (filter.First() == '<')
            {
                filter = filter.Substring(1);
                if (filter.First() == '=')
                {
                    filter = filter.Substring(1);
                    uint filterValue = uint.Parse(filter);
                    return numericValue <= filterValue;
                }
                else
                {
                    uint filterValue = uint.Parse(filter);
                    return numericValue < filterValue;
                }
            }
            else if (filter.First() == '>')
            {
                filter = filter.Substring(1);
                if (filter.First() == '=')
                {
                    filter = filter.Substring(1);
                    uint filterValue = uint.Parse(filter);
                    return numericValue >= filterValue;
                }
                else
                {
                    uint filterValue = uint.Parse(filter);
                    return numericValue > filterValue;
                }
            }
            else
            {
                if (filter.First() == '=')
                {
                    filter = filter.Substring(1);
                }

                uint filterValue = uint.Parse(filter);
                return numericValue == filterValue;
            }
        }

        private bool MatchesTextSearch(CardInfo cardInfo)
        {
            if (string.IsNullOrWhiteSpace(_textSearchString))
            {
                _textSearchString = null;
                return true;
            }

            return cardInfo.Name.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || cardInfo.Description.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || cardInfo.Type.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || cardInfo.Race.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || cardInfo.Attribute?.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase) == true;
        }

        private void OnClearFilters()
        {
            _selectedCardCategory = CardCategory.All;
            _selectedSubCategory = SubCategory.All;
            _selectedMonsterAttribute = MonsterAttribute.All;
            _selectedMonsterType = MonsterType.All;
            _levelRankLinkFilterString = null;
            _scaleFilterString = null;
            _atkFilterString = null;
            _defFilterString = null;
            _textSearchString = null;
            OnSortOption1Changed(SortOption.Default);
        }

        private void OnSortOption1Changed(SortOption sortOption)
        {
            if (_sortOption1 != sortOption)
            {
                _sortOption1 = sortOption;

                if (_sortOption1 == SortOption.Default || _sortOption1 == _sortOption2)
                {
                    _sortOption2 = SortOption.Default;
                }

                SortCardCollection();
            }
        }

        private void OnSortOption2Changed(SortOption sortOption)
        {
            if (_sortOption1 == SortOption.Default)
            {
                OnSortOption1Changed(sortOption);
            }
            else if (_sortOption2 != sortOption)
            {
                _sortOption2 = sortOption;

                if (_sortOption1 == _sortOption2)
                {
                    _sortOption2 = SortOption.Default;
                }

                SortCardCollection();
            }
        }

        private void SortCardCollection()
        {
            if (_sortOption1 == SortOption.Default)
            {
                _allCards = _allCards.OrderBy(cardInfo => cardInfo).ToList();
            }
            else if (_sortOption2 == SortOption.Default)
            {
                _allCards = _allCards.OrderBy(cardInfo => cardInfo, new CardInfoComparer(_sortOption1))
                    .ThenBy(cardInfo => cardInfo).ToList();
            }
            else
            {
                _allCards = _allCards.OrderBy(card => card, new CardInfoComparer(_sortOption1))
                    .ThenBy(cardInfo => cardInfo, new CardInfoComparer(_sortOption2))
                    .ThenBy(cardInfo => cardInfo).ToList();
            }
        }
    }

    public class CardInfoComparer : IComparer<CardInfo>
    {
        private SortOption _sortOption { get; set; } = SortOption.Default;

        public CardInfoComparer(SortOption sortOption)
        {
            _sortOption = sortOption;
        }

        public int Compare(CardInfo x, CardInfo y)
        {
            switch (_sortOption)
            {
                case SortOption.Name:
                    return x.Name.CompareTo(y.Name);

                case SortOption.Level:
                    if (x.Level != null && y.Level == null) { return -1; }
                    else if (x.Level == null && y.Level == null) { return x.CompareTo(y); }
                    else if (x.Level == null && y.Level != null) { return 1; }
                    return y.Level.Value.CompareTo(x.Level.Value);

                case SortOption.Attack:
                    if (x.ATK != null && y.ATK == null) { return -1; }
                    else if (x.ATK == null && y.ATK == null) { return x.CompareTo(y); }
                    else if (x.ATK == null && y.ATK != null) { return 1; }
                    return y.ATK.Value.CompareTo(x.ATK.Value);

                case SortOption.Defense:
                    if (x.DEF != null && y.DEF == null) { return -1; }
                    else if (x.DEF == null && y.DEF == null) { return x.CompareTo(y); }
                    else if (x.DEF == null && y.DEF != null) { return 1; }
                    return y.DEF.Value.CompareTo(x.DEF.Value);

                default:
                    return x.CompareTo(y);
            }
        }
    }
}
