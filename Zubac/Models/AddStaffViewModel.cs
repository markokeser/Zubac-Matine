using System.ComponentModel.DataAnnotations;

namespace Zubac.Models
{
    public class AddStaffViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public int UserRank { get; set; }
    }
}
