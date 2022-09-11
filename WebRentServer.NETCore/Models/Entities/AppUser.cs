using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebRentServer.NETCore.ETagHelper;

namespace WebRentServer.NETCore.Models.Entities
{
    public class AppUser : IModifiableResource
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        public bool Activated { get; set; }
        [Required]
        public bool ProfileEdited { get; set; }
        public string DocumentPicture { get; set; }
        public virtual List<RentService> RentServices { get; set; }
        [NotMapped]
        string IModifiableResource.ETag => this.GetWeakETag(JsonConvert.SerializeObject(this));
    }
}