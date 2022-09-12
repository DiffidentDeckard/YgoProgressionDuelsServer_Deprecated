using System;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Models
{
    public class NewCardModel
    {
        public CardInfo CardInfo { get; }

        public uint Count { get; set; }

        public string Rarity { get; }

        public string BgColor { get; }

        public uint StarChipWorth { get; }

        public bool CardIsNew { get; set; } = false;

        public NewCardModel(CardInfo cardInfo, uint count, string rarity)
        {
            CardInfo = cardInfo;
            Count = count;
            Rarity = rarity;
            BgColor = GetColorForRarity(Rarity);
            StarChipWorth = GetStarChipWorthForRarity(Rarity);
        }

        private static string GetColorForRarity(string rarity)
        {
            if (rarity.Contains("ultimate", StringComparison.CurrentCultureIgnoreCase)
                || rarity.Contains("ghost", StringComparison.CurrentCultureIgnoreCase)
                || rarity.Contains("prismatic", StringComparison.CurrentCultureIgnoreCase))
            {
                return "bg-dark";
            }

            if (rarity.Contains("secret", StringComparison.CurrentCultureIgnoreCase))
            {
                return "bg-danger";
            }

            if (rarity.Contains("gold", StringComparison.CurrentCultureIgnoreCase))
            {
                return "bg-warning";
            }

            if (rarity.Contains("ultra", StringComparison.CurrentCultureIgnoreCase))
            {
                return "bg-secondary";
            }

            if (rarity.Contains("super", StringComparison.CurrentCultureIgnoreCase))
            {
                return "bg-primary";
            }

            if (rarity.Contains("rare", StringComparison.CurrentCultureIgnoreCase))
            {
                return "bg-success";
            }

            return "bg-light";
        }

        public static uint GetStarChipWorthForRarity(string rarity)
        {
            if (rarity.Contains("ultimate", StringComparison.CurrentCultureIgnoreCase)
                    || rarity.Contains("ghost", StringComparison.CurrentCultureIgnoreCase)
                    || rarity.Contains("prismatic", StringComparison.CurrentCultureIgnoreCase))
            {
                return 10;
            }

            if (rarity.Contains("secret", StringComparison.CurrentCultureIgnoreCase))
            {
                return 5;
            }

            if (rarity.Contains("ultra", StringComparison.CurrentCultureIgnoreCase))
            {
                return 4;
            }

            if (rarity.Contains("super", StringComparison.CurrentCultureIgnoreCase)
                    || rarity.Contains("gold", StringComparison.CurrentCultureIgnoreCase))
            {
                return 3;
            }

            if (rarity.Contains("rare", StringComparison.CurrentCultureIgnoreCase))
            {
                return 2;
            }

            if (rarity.Contains("common", StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }

            // If I dont know what this is, make it the same as a super
            return 3;
        }
    }
}
