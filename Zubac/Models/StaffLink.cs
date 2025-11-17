using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Zubac.Models
{
    public class StaffLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Token { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiresAt { get; set; }

        [ForeignKey("StaffId")]
        public Users Staff { get; set; }
    }
}
