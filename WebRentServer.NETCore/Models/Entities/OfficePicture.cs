using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebRentServer.NETCore.Models.Entities
{
    public class OfficePicture
    {
        [Key]
        public int OfficePictureId { get; set; }
        [Required]
        [ForeignKey("Office")]
        public int OfficeId { get; set; }
        [Required]
        public string Data { get; set; }
        [JsonIgnore]
        public virtual Office Office { get; set; }
    }
}