using System.ComponentModel.DataAnnotations;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Models
{
    public class RegisterUserModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }

        [Required]
        [StringLength(User.MAX_NAME_LENGTH, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = User.MIN_NAME_LENGTH)]
        [DataType(DataType.Text)]
        [Display(Name = nameof(UserName))]
        public string UserName { get; set; }

        [Required]
        [StringLength(User.MAX_PASSWORD_LENGTH, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = User.MIN_PASSWORD_LENGTH)]
        [DataType(DataType.Password)]
        [Display(Name = nameof(Password))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.ImageUrl)]
        [Display(Name = "Avatar")]
        public string AvatarUrl { get; set; }
    }
}
