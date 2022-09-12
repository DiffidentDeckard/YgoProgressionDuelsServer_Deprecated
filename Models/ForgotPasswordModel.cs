using System.ComponentModel.DataAnnotations;

namespace YgoProgressionDuels.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
