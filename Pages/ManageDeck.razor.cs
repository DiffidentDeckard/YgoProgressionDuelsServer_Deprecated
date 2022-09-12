using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Pages.Shared;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Pages
{
    public partial class ManageDeck : AuthorizedComponentBase
    {
        [Parameter]
        public string DuelistId { get; set; }

        [BindProperty]
        private Duelist _currentDuelist { get; set; }

        [BindProperty]
        private IList<Card> _cardCollection { get; set; } = new List<Card>();

        [BindProperty]
        private CardInfo _currentCard { get; set; }

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

        [BindProperty]
        private string _exportDeckErrorMessage { get; set; }

        [BindProperty]
        private Snackbar _saveConfirmationSnackbar { get; set; }

        private HashSet<Card> _cardsToUpdate { get; set; } = new HashSet<Card>();

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // This sets the current user id
                await base.OnInitializedAsync();

                await AssignCurrentDuelistAndCardsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignCurrentDuelistAndCardsAsync()
        {
            // Parse the Guid and find this progression series
            if (Guid.TryParse(DuelistId, out Guid duelistId))
            {
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    _currentDuelist = await dbContext.Duelists.AsNoTracking()
                        .Include(duelist => duelist.Owner).AsNoTracking()
                        .Include(duelist => duelist.Series)
                        .ThenInclude(series => series.CurrentBanList)
                        .ThenInclude(banList => banList.Entries).AsNoTracking()
                        .Include(duelist => duelist.CardCollection)
                        .ThenInclude(card => card.Info).AsNoTracking()
                        .AsSplitQuery().AsNoTracking()
                        .FirstOrDefaultAsync(duelist => duelist.DuelistId.Equals(duelistId));
                }

                if (_currentDuelist != null)
                {
                    _cardCollection = _currentDuelist.CardCollection
                        .OrderByDescending(card => card.IsNew)
                        .ThenBy(card => card)
                        .ToList();

                    if (_currentDuelist.OwnerId.Equals(CurrentUserId) || _currentDuelist.Series.HostId.Equals(CurrentUserId) || _currentDuelist.DeckIsPublic)
                    {
                        _currentCard = _cardCollection.Where(card => card.GetTotalInDeck() > 0).FirstOrDefault()?.Info;
                    }
                    if (_currentCard == null && (_currentDuelist.OwnerId.Equals(CurrentUserId) || _currentDuelist.Series.HostId.Equals(CurrentUserId) || _currentDuelist.CardCollectionIsPublic))
                    {
                        _currentCard = _cardCollection.FirstOrDefault()?.Info;
                    }
                }
            }
        }

        private async Task OnOpenUrlInNewTabAsync(string url)
        {
            await JSRuntime.InvokeAsync<object>("open", new object[] { url, "_blank" });
        }

        private void OnClearDeck()
        {
            Parallel.ForEach(_cardCollection.Where(card => card.GetTotalInDeck() > 0), card =>
            {
                card.NumMainDeck = 0;
                card.NumExtraDeck = 0;
                card.NumSideDeck = 0;
                _cardsToUpdate.Add(card);
            });
        }

        private async Task OnCancelChangesAsync()
        {
            await AssignCurrentDuelistAndCardsAsync();
            _cardsToUpdate.Clear();
        }

        private async Task OnSaveChangesAsync()
        {
            if (_cardsToUpdate.Any())
            {
                // Save all changes
                using (ApplicationDbContext dbContext = DbContextFactory.CreateDbContext())
                {
                    dbContext.Cards.UpdateRange(_cardsToUpdate);
                    await dbContext.SaveChangesAsync();
                    _cardsToUpdate.Clear();
                }

                // Show confirmation for user
                _saveConfirmationSnackbar.Show();
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

        private bool OnCustomFilter(Card card)
        {
            return MatchesCategory(card)
                && MatchesSubCategory(card)
                && MatchesMonsterAttributeFilter(card)
                && MatchesMonsterTypeFilter(card)
                && (MatchesNumericFilter(card.Info.Level, _levelRankLinkFilterString) || MatchesNumericFilter(card.Info.Link, _levelRankLinkFilterString))
                && MatchesNumericFilter(card.Info.Scale, _scaleFilterString)
                && MatchesNumericFilter(card.Info.ATK, _atkFilterString)
                && MatchesNumericFilter(card.Info.DEF, _defFilterString)
                && MatchesTextSearch(card);
        }

        private bool MatchesCategory(Card card)
        {
            if (_selectedCardCategory == CardCategory.All)
            {
                return true;
            }

            return card.Info.CardCategory == _selectedCardCategory;
        }

        private bool MatchesSubCategory(Card card)
        {
            if (_selectedSubCategory == SubCategory.All)
            {
                return true;
            }

            string subCategory = _selectedSubCategory.GetDisplayName();

            if (card.Info.CardCategory == CardCategory.Monster)
            {
                return card.Info.Type.Contains(subCategory, StringComparison.CurrentCultureIgnoreCase);
            }

            return card.Info.Race.Contains(subCategory, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool MatchesMonsterAttributeFilter(Card card)
        {
            if (_selectedMonsterAttribute == MonsterAttribute.All)
            {
                return true;
            }

            if (card.Info.Attribute == null)
            {
                return false;
            }

            return card.Info.Attribute.Equals(_selectedMonsterAttribute.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        private bool MatchesMonsterTypeFilter(Card card)
        {
            if (_selectedMonsterType == MonsterType.All)
            {
                return true;
            }

            return card.Info.Race.Equals(_selectedMonsterType.GetDisplayName(), StringComparison.CurrentCultureIgnoreCase);
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

        private bool MatchesTextSearch(Card card)
        {
            if (string.IsNullOrWhiteSpace(_textSearchString))
            {
                _textSearchString = null;
                return true;
            }

            return card.Info.Name.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Description.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Race.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Attribute?.Contains(_textSearchString, StringComparison.CurrentCultureIgnoreCase) == true;
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

        private void OnMouseOverCard(Card card)
        {
            if (_currentCard.CardInfoId != card.InfoId)
            {
                _currentCard = card.Info;
            }
        }

        private void OnLeftClickCardCollection(Card card)
        {
            AddToMainExtraDeck(card);
            _cardsToUpdate.Add(card);
        }

        private void OnRightClickCardCollection(Card card)
        {
            AddToSideDeck(card);
            _cardsToUpdate.Add(card);
        }

        private void OnLeftClickMainExtraDeck(Card card)
        {
            RemoveFromMainExtraDeck(card);
            _cardsToUpdate.Add(card);
        }

        private void OnRightClickMainExtraDeck(Card card)
        {
            RemoveFromMainExtraDeck(card);
            AddToSideDeck(card);
            _cardsToUpdate.Add(card);
        }

        private void OnLeftClickSideDeck(Card card)
        {
            RemoveFromSideDeck(card);
            AddToMainExtraDeck(card);
            _cardsToUpdate.Add(card);
        }

        private void OnRightClickSideDeck(Card card)
        {
            RemoveFromSideDeck(card);
            _cardsToUpdate.Add(card);
        }

        private void AddToMainExtraDeck(Card card)
        {
            if (card.GetTotalInDeck() < card.NumCollection
                && card.GetTotalInDeck() < card.GetCardLimit())
            {
                if (card.Info.Type.Contains("Fusion", StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains("Synchro", StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains("Xyz", StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains("Link", StringComparison.CurrentCultureIgnoreCase))
                {
                    card.NumExtraDeck++;
                }
                else
                {
                    card.NumMainDeck++;
                }
            }
        }

        private void RemoveFromMainExtraDeck(Card card)
        {
            if (card.Info.Type.Contains("Fusion", StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains("Synchro", StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains("Xyz", StringComparison.CurrentCultureIgnoreCase)
                || card.Info.Type.Contains("Link", StringComparison.CurrentCultureIgnoreCase))
            {
                if (card.NumExtraDeck > 0)
                {
                    card.NumExtraDeck--;
                }
            }
            else if (card.NumMainDeck > 0)
            {
                card.NumMainDeck--;
            }
        }

        private void AddToSideDeck(Card card)
        {
            if (card.GetTotalInDeck() < card.NumCollection
                && card.GetTotalInDeck() < card.GetCardLimit())
            {
                card.NumSideDeck++;
            }
        }

        private void RemoveFromSideDeck(Card card)
        {
            if (card.NumSideDeck > 0)
            {
                card.NumSideDeck--;
            }
        }

        private async Task OnExportDeckAsync()
        {
            // Validate the deck state
            long mainDeckCount = _cardCollection.Sum(card => card.NumMainDeck);
            if (mainDeckCount < 40)
            {
                _exportDeckErrorMessage = "Main Deck must have at least 40 cards";
                return;
            }
            if (mainDeckCount > 60)
            {
                _exportDeckErrorMessage = "Main Deck must have at most 60 cards";
                return;
            }

            if (_cardCollection.Sum(card => card.NumExtraDeck) > 15)
            {
                _exportDeckErrorMessage = "Extra Deck must have at most 15 cards";
                return;
            }

            if (_cardCollection.Sum(card => card.NumSideDeck) > 15)
            {
                _exportDeckErrorMessage = "Side Deck must have at most 15 cards";
                return;
            }

            Card illegalCard = _cardCollection.FirstOrDefault(card => card.GetTotalInDeck() > card.GetCardLimit());
            if (illegalCard != null)
            {
                _exportDeckErrorMessage = $"{illegalCard.Info.Name} must have at most {illegalCard.GetCardLimit()} copies";
                return;
            }
            _exportDeckErrorMessage = string.Empty;

            // Write the ydk file content
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"#created by {_currentDuelist.Owner.UserName}");
            sb.AppendLine("#main");
            foreach (Card card in _cardCollection.Where(card => card.NumMainDeck > 0))
            {
                for (int i = 0; i < card.NumMainDeck; i++)
                {
                    sb.AppendLine(card.InfoId.ToString());
                }
            }
            sb.AppendLine("#extra");
            foreach (Card card in _cardCollection.Where(card => card.NumExtraDeck > 0))
            {
                for (int i = 0; i < card.NumExtraDeck; i++)
                {
                    sb.AppendLine(card.InfoId.ToString());
                }
            }
            sb.AppendLine("!side");
            foreach (Card card in _cardCollection.Where(card => card.NumSideDeck > 0))
            {
                for (int i = 0; i < card.NumSideDeck; i++)
                {
                    sb.AppendLine(card.InfoId.ToString());
                }
            }

            // Get the file bytes
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            string ydk = Convert.ToBase64String(bytes);

            // Get a proper filename
            string filename = $"{_currentDuelist.Series.Name}_{_currentDuelist.Owner.UserName}.ydk";
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(invalidChar.ToString(), string.Empty);
            }

            // Download the file
            await JSRuntime.InvokeAsync<object>("saveAsFile", new object[] { filename, ydk });
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
                _cardCollection = _cardCollection.OrderBy(card => card).ToList();
            }
            else if (_sortOption2 == SortOption.Default)
            {
                _cardCollection = _cardCollection.OrderBy(card => card, new CardComparer(_sortOption1))
                    .ThenBy(card => card).ToList();
            }
            else
            {
                _cardCollection = _cardCollection.OrderBy(card => card, new CardComparer(_sortOption1))
                    .ThenBy(card => card, new CardComparer(_sortOption2))
                    .ThenBy(card => card).ToList();
            }
        }
    }

    public class CardComparer : IComparer<Card>
    {
        private SortOption _sortOption { get; set; } = SortOption.Default;

        public CardComparer(SortOption sortOption)
        {
            _sortOption = sortOption;
        }

        public int Compare(Card x, Card y)
        {
            switch (_sortOption)
            {
                case SortOption.Name:
                    return x.Info.Name.CompareTo(y.Info.Name);

                case SortOption.Level:
                    if (x.Info.Level != null && y.Info.Level == null) { return -1; }
                    else if (x.Info.Level == null && y.Info.Level == null) { return x.CompareTo(y); }
                    else if (x.Info.Level == null && y.Info.Level != null) { return 1; }
                    return y.Info.Level.Value.CompareTo(x.Info.Level.Value);

                case SortOption.Attack:
                    if (x.Info.ATK != null && y.Info.ATK == null) { return -1; }
                    else if (x.Info.ATK == null && y.Info.ATK == null) { return x.CompareTo(y); }
                    else if (x.Info.ATK == null && y.Info.ATK != null) { return 1; }
                    return y.Info.ATK.Value.CompareTo(x.Info.ATK.Value);

                case SortOption.Defense:
                    if (x.Info.DEF != null && y.Info.DEF == null) { return -1; }
                    else if (x.Info.DEF == null && y.Info.DEF == null) { return x.CompareTo(y); }
                    else if (x.Info.DEF == null && y.Info.DEF != null) { return 1; }
                    return y.Info.DEF.Value.CompareTo(x.Info.DEF.Value);

                case SortOption.Newest:
                    return y.DateObtained.CompareTo(x.DateObtained);

                case SortOption.Amount:
                    return y.NumCollection.CompareTo(x.NumCollection);

                case SortOption.Limit:
                    return x.GetCardLimit().CompareTo(y.GetCardLimit());

                default:
                    return x.CompareTo(y);
            }
        }
    }
}
