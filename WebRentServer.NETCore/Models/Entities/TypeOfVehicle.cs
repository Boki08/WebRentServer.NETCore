using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebRentServer.NETCore.ETagHelper;

namespace WebRentServer.NETCore.Models.Entities
{
    public class TypeOfVehicle : IModifiableResource
    {
        [Key]
        public int TypeId { get; set; }
        [Required]
        public string Type { get; set; }
        [JsonIgnore]
        public virtual List<Vehicle> Vehicles { get; set; }
        [NotMapped]
        string IModifiableResource.ETag => this.GetWeakETag(JsonConvert.SerializeObject(this));
    }
}