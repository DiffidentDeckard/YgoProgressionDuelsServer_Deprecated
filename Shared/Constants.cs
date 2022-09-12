using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace YgoProgressionDuels.Shared
{
    public static class Constants
    {
        public const string ADMIN = nameof(ADMIN);

        public static readonly IReadOnlyCollection<string> AllowedImageExtensions = new ReadOnlyCollection<string>(new List<string>()
        {
            ".apng", ".avif", ".gif", ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".png", ".svg", ".webp", ".bmp", ".tif", ".tiff"
        });

        public const string YGOPRODECK_URL = @"https://ygoprodeck.com";
        public const string YGOPRODECK_DBURL = @"https://db.ygoprodeck.com/";
        public const string GET_CARDSETS_ENDPOINT = @"api/v7/cardsets.php";
        public const string GET_CARDINFO_ENDPOINT = @"api/v7/cardinfo.php";
        public const string OPENBOOSTERPACK_ENDPOINT = @"queries/pack-opener/pack-open.php";

        public const string PACKOPENER_URL = @"https://db.ygoprodeck.com/pack-open/";
        public const string CARDIMAGE_URL = @"https://storage.googleapis.com/ygoprodeck.com/pics/";

        public const string YUGIOH_BANLIST = @"https://www.yugioh-card.com/en/limited/";

        public const int DEFAULT_DATAGRID_PAGESIZE = 32;
        public static readonly List<int> DataGrid_PageSizes = new List<int>()
        {
            DEFAULT_DATAGRID_PAGESIZE,
            DEFAULT_DATAGRID_PAGESIZE * 2,
            DEFAULT_DATAGRID_PAGESIZE * 4,
            DEFAULT_DATAGRID_PAGESIZE * 8
        };

        public static readonly List<SubCategory> MonsterCategories = new List<SubCategory>()
        {
            SubCategory.All,
            SubCategory.Normal,
            SubCategory.Effect,
            SubCategory.Flip,
            SubCategory.Union,
            SubCategory.Toon,
            SubCategory.Spirit,
            SubCategory.Gemini,
            SubCategory.Tuner,
            SubCategory.Pendulum,
            SubCategory.Ritual,
            SubCategory.Fusion,
            SubCategory.Synchro,
            SubCategory.Xyz,
            SubCategory.Link
        };

        public static readonly List<SubCategory> SpellCategories = new List<SubCategory>()
        {
            SubCategory.All,
            SubCategory.Normal,
            SubCategory.QuickPlay,
            SubCategory.Continuous,
            SubCategory.Equip,
            SubCategory.Field
        };

        public static readonly List<SubCategory> TrapCategories = new List<SubCategory>()
        {
            SubCategory.All,
            SubCategory.Normal,
            SubCategory.Continuous,
            SubCategory.Counter
        };

        public static string GetDisplayName(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DisplayAttribute attr =
                           System.Attribute.GetCustomAttribute(field,
                             typeof(DisplayAttribute)) as DisplayAttribute;
                    if (attr != null)
                    {
                        return attr.Name;
                    }
                }
            }
            return value.ToString();
        }
    }

    public static class Application
    {
        public const string NAME = "YgoProgressionDuels";
    }

    public static class User
    {
        public const int MIN_NAME_LENGTH = 4;
        public const int MAX_NAME_LENGTH = 25;
        public const int MIN_PASSWORD_LENGTH = 6;
        public const int MAX_PASSWORD_LENGTH = 30;

        public const string ID = "UserId";
        public const string EMAIL_CONFIRMATION_TOKEN = "EmailConfirmationToken";
        public const string PASSWORD_RESET_TOKEN = "PasswordResetToken";
    }

    public enum BanListFormat
    {
        None,
        Traditional,
        Advanced
    }

    public enum BanListStatus
    {
        Forbidden,
        Limited,
        SemiLimited,
        Unlimited
    }

    public enum CardCategory
    {
        All,
        Monster,
        Spell,
        Trap
    }

    public enum SubCategory
    {
        All,
        Normal,
        Effect,
        Flip,
        Union,
        Toon,
        Spirit,
        Gemini,
        Tuner,
        Pendulum,
        Ritual,
        Fusion,
        Synchro,
        Xyz,
        Link,
        [Display(Name = "Quick-Play")]
        QuickPlay,
        Continuous,
        Equip,
        Field,
        Counter
    }

    public enum MonsterAttribute
    {
        All,
        EARTH,
        WATER,
        FIRE,
        WIND,
        LIGHT,
        DARK,
        DIVINE
    }

    public enum MonsterType
    {
        All,
        Warrior,
        Spellcaster,
        Fairy,
        Fiend,
        Zombie,
        Machine,
        Aqua,
        Pyro,
        Rock,
        [Display(Name = "Winged Beast")]
        WingedBeast,
        Plant,
        Insect,
        Thunder,
        Dragon,
        Beast,
        [Display(Name = "Beast-Warrior")]
        BeastWarrior,
        Dinosaur,
        Fish,
        [Display(Name = "Sea Serpent")]
        SeaSerpent,
        Reptile,
        Psychic,
        [Display(Name = "Divine-Beast")]
        DivineBeast,
        Wyrm,
        Cyberse
    }

    public enum SortOption
    {
        Default,
        Name,
        Level,
        Attack,
        Defense,
        Newest,
        Amount,
        Limit
    }

    public enum TournamentStructure
    {
        [Display(Name = "Single-Elimination")]
        SingleElimination,
        [Display(Name = "Swiss Round")]
        SwissRound
    }

    public enum TournamentBye
    {
        Random,
        [Display(Name = "Top Score")]
        TopScore,
        [Display(Name = "Bottom Score")]
        BottomScore
    }

    public enum Theme
    {
        Bootstrap,
        Cyborg,
        Darkly,
        Flatly,
        Minty,
        Quartz,
        United,
        Vapor
    }
}
