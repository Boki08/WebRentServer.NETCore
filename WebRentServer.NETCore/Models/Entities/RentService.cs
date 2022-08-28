using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebRentServer.NETCore.Models.Entities
{
    public class RentService
    {
        [Key]
        public int RentServiceId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Logo { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool Activated { get; set; }
        [Required]
        public bool ServiceEdited { get; set; }
        public double Grade { get; set; }
        public virtual AppUser User { get; set; }
        public List<Office> Offices { get; set; }
        public List<Vehicle> Vehicles { get; set; }
        public List<Comment> Comments { get; set; }
    }
}