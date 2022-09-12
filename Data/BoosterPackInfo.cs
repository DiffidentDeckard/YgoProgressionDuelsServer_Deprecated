using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    /// <summary>
    /// Represents the information about a Booster Pack Set
    /// </summary>
    [Index(nameof(SetName), IsUnique = true)]
    public class BoosterPackInfo : IComparable<BoosterPackInfo>
    {
        [Key]
        public Guid BoosterPackInfoId { get; set; }

        // The name of this booster pack
        [DataType(DataType.Text)]
        public string SetName { get; set; }

        // This is a value needed to correctly open up a booster pack using the PackOpener site
        // I am not sure what it means
        public int SetType { get; set; }

        // The code for this set, which is a unique shorthand identifier
        [DataType(DataType.Text)]
        public string SetCode { get; set; }

        // The image to show for this booster pack
        public string ImageUrl { get; set; }

        // The url that a user can navigate to to see all the possible cards in this booster pack
        public string SetInfoUrl { get; set; }

        // The date this booster pack was released
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        // Is this a structure deck
        public bool IsStructureDeck { get; set; } = false;

        // Default Constructor for Entity Framework Core
        public BoosterPackInfo() { }

        public BoosterPackInfo(string packOpenerName, int packOpenerType, string imageUrl)
        {
            SetName = packOpenerName;
            SetType = packOpenerType;
            ImageUrl = imageUrl;

            string setCode = Path.GetFileNameWithoutExtension(imageUrl);

            // One image in particular has "_alt" in it, we dont want that part
            int underScoreIndex = setCode.IndexOf('_');
            if (underScoreIndex > 0)
            {
                setCode = setCode.Substring(0, underScoreIndex);
            }

            // For some stupid reason, the first McDonald's pack is MP1
            // But the second McDonald's pack is MDP2 (not MP2)
            // But the image from pack opener site is named MP2
            // So I will manually rename it here for this special case
            if (setCode.Equals("MP2", StringComparison.CurrentCultureIgnoreCase))
            {
                setCode = "MDP2";
            }

            SetCode = setCode;
        }

        public override bool Equals(object obj)
        {
            return SetName.Equals((obj as BoosterPackInfo)?.SetName);
        }

        public override int GetHashCode()
        {
            return SetName.GetHashCode();
        }

        public int CompareTo(BoosterPackInfo other)
        {
            return ReleaseDate.CompareTo(other.ReleaseDate);
        }
    }
}
