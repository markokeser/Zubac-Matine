using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zubac.Models
{
    [Table("Users")]
    public class Users
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        public string? Password { get; set; }

        public bool LoggedIn { get; set; }

        public int UserRank { get; set; }
        [Required]
        public int RestaurantId { get; set; }
    }

}
