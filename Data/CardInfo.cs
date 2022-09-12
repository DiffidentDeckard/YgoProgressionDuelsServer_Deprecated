using System;
using System.ComponentModel.DataAnnotations;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents the information about a Card
    /// </summary>
    public class CardInfo : IComparable<CardInfo>
    {
        [Key]
        public ulong CardInfoId { get; set; }

        // The name of the card
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        // Monster/Spell/Trap
        public string Type { get; set; }
        public CardCategory CardCategory
        {
            get
            {
                if (Type.Contains("Monster", StringComparison.CurrentCultureIgnoreCase))
                {
                    return CardCategory.Monster;
                }
                if (Type.Contains("Spell", StringComparison.CurrentCultureIgnoreCase))
                {
                    return CardCategory.Spell;
                }
                if (Type.Contains("Trap", StringComparison.CurrentCultureIgnoreCase))
                {
                    return CardCategory.Trap;
                }
                return CardCategory.All;
            }
        }

        // This is the monster type, or the type of spell/trap
        public string Race { get; set; }

        // Monster only stats
        public string Attribute { get; set; }
        public uint? Level { get; set; } = null;
        public uint? Link { get; set; } = null;
        public uint? Scale { get; set; } = null;
        public uint? ATK { get; set; } = null;
        public uint? DEF { get; set; } = null;

        public string TreatedAs { get; set; }

        // The url to the image of the card
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        // The url to the information about the card
        [DataType(DataType.ImageUrl)]
        public string CardInfoUrl { get; set; }

        public override bool Equals(object obj)
        {
            return CardInfoId.Equals((obj as CardInfo)?.CardInfoId);
        }

        public override int GetHashCode()
        {
            return CardInfoId.GetHashCode();
        }

        public int CompareTo(CardInfo other)
        {
            int typeCompare = GetTypeSortOrder().CompareTo(other.GetTypeSortOrder());

            if (typeCompare != 0)
            {
                return typeCompare;
            }

            if (CardCategory.Equals(CardCategory.Monster))
            {
                // Both are Monster Cards
                if (Level != null)
                {
                    int levelCompare = Level.Value.CompareTo(other.Level.Value);
                    if (levelCompare != 0)
                    {
                        // Reverse sort
                        return levelCompare * -1;
                    }
                }
                else if (Link != null)
                {
                    int linkCompare = Link.Value.CompareTo(other.Link.Value);
                    if (linkCompare != 0)
                    {
                        // Reverse sort
                        return linkCompare * -1;
                    }
                }

                int atkCompare = ATK.Value.CompareTo(other.ATK.Value);
                if (atkCompare != 0)
                {
                    // Reverse sort
                    return atkCompare * -1;
                }

                if (DEF != null)
                {
                    int defCompare = DEF.Value.CompareTo(other.DEF.Value);
                    if (defCompare != 0)
                    {
                        // Reverse sort
                        return defCompare * -1;
                    }
                }

                int attributeCompare = Attribute.CompareTo(other.Attribute);
                if (attributeCompare != 0)
                {
                    return attributeCompare;
                }

                int monsterTypeCompare = Race.CompareTo(other.Race);
                if (monsterTypeCompare != 0)
                {
                    return monsterTypeCompare;
                }

                return Name.CompareTo(other.Name);
            }

            int raceCompare = GetRaceSortOrder().CompareTo(other.GetRaceSortOrder());

            if (raceCompare != 0)
            {
                return raceCompare;
            }

            return Name.CompareTo(other.Name);
        }

        public int GetTypeSortOrder()
        {
            switch (Type)
            {
                case "Normal Monster":
                    return 0;
                case "Normal Tuner Monster":
                    return 1;
                case "Pendulum Normal Monster":
                    return 2;
                case "Effect Monster":
                    return 3;
                case "Flip Effect Monster":
                    return 4;
                case "Union Effect Monster":
                    return 5;
                case "Toon Monster":
                    return 6;
                case "Spirit Monster":
                    return 7;
                case "Gemini Monster":
                    return 8;
                case "Tuner Monster":
                    return 9;
                case "Pendulum Effect Monster":
                    return 10;
                case "Pendulum Tuner Effect Monster":
                    return 11;
                case "Ritual Monster":
                    return 12;
                case "Ritual Effect Monster":
                    return 13;
                case "Spell Card":
                    return 14;
                case "Trap Card":
                    return 15;
                case "Fusion Monster":
                    return 16;
                case "Pendulum Effect Fusion Monster":
                    return 17;
                case "Synchro Monster":
                    return 18;
                case "Synchro Tuner Monster":
                    return 19;
                case "Synchro Pendulum Effect Monster":
                    return 20;
                case "XYZ Monster":
                    return 21;
                case "XYZ Pendulum Effect Monster":
                    return 22;
                case "Link Monster":
                    return 23;
                default:
                    return 24;
            }
        }

        public int GetRaceSortOrder()
        {
            switch (Race)
            {
                case "Normal":
                    return 1;
                case "Quick-Play":
                    return 2;
                case "Continuous":
                    return 3;
                case "Ritual":
                    return 4;
                case "Equip":
                    return 5;
                case "Field":
                    return 6;
                case "Counter":
                    return 7;
                default:
                    return 0;
            }
        }
    }
}
