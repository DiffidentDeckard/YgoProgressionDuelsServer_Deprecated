using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Models
{
    public class ProgressionSeriesSettingsModel
    {
        [Required]
        [BindProperty]
        public bool AllowPurchaseBoosterPacks { get; set; }

        [Required]
        [BindProperty]
        [Range(1, int.MaxValue)]
        public int BoosterPackPrice { get; set; }

        [Required]
        [BindProperty]
        public bool AllowPurchaseStructureDecks { get; set; }

        [Required]
        [BindProperty]
        [Range(1, int.MaxValue)]
        public int StructureDeckPrice { get; set; }

        [Required]
        [BindProperty]
        public bool AllowPurchaseSpecialProducts { get; set; }

        [Required]
        [BindProperty]
        [Range(1, int.MaxValue)]
        public int SpecialProductPricePerCard { get; set; }

        [BindProperty]
        public BanListFormat BanListFormat { get; set; }

        [BindProperty]
        public Guid? CurrentBanListId { get; set; }

        [BindProperty]
        public bool AutoSetCurrentBanList { get; set; }

        [BindProperty]
        public TournamentStructure TournamentStructure { get; set; }

        [BindProperty]
        public TournamentBye TournamentBye { get; set; }

        public ProgressionSeriesSettingsModel(ProgressionSeries progressionSeries)
        {
            AllowPurchaseBoosterPacks = progressionSeries.AllowPurchaseBoosterPacks;
            BoosterPackPrice = (int)progressionSeries.BoosterPackPrice;
            AllowPurchaseStructureDecks = progressionSeries.AllowPurchaseStructureDecks;
            StructureDeckPrice = (int)progressionSeries.StructureDeckPrice;
            AllowPurchaseSpecialProducts = progressionSeries.AllowPurchaseSpecialProducts;
            SpecialProductPricePerCard = (int)progressionSeries.SpecialProductPricePerCard;
            BanListFormat = progressionSeries.BanListFormat;
            CurrentBanListId = progressionSeries.CurrentBanList?.BanListId;
            AutoSetCurrentBanList = progressionSeries.AutoSetCurrentBanList;
            TournamentStructure = progressionSeries.NextTournamentStructure;
            TournamentBye = progressionSeries.NextTournamentBye;
        }
    }
}
