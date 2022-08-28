using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace WebRentServer.NETCore.Models.Entities
{
    public class TypeOfVehicle
    {
        [Key]
        public int TypeId { get; set; }
        [Required]
        public string Type { get; set; }
        [JsonIgnore]
        public virtual List<Vehicle> Vehicles { get; set; }
    }
}