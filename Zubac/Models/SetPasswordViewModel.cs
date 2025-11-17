using System.ComponentModel.DataAnnotations;

namespace Zubac.Models
{
    public class SetPasswordViewModel
    {
        public int StaffId { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
