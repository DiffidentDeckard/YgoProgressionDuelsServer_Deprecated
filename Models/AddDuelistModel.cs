using System.ComponentModel.DataAnnotations;

namespace YgoProgressionDuels.Models
{
    public class AddDuelistModel
    {
        [Required(ErrorMessage = "Please input a Duelist's Username to add them")]
        public string Username { get; set; }
    }
}
