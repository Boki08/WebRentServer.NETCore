using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebRentServer.NETCore.Models.Entities
{
    public class VehiclePicture
    {
        [Key]
        public int VehiclePictureId { get; set; }
        [Required]
        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }
        [Required]
        public string Data { get; set; }
        [JsonIgnore]
        public virtual Vehicle Vehicle { get; set; }
    }
}