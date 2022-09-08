using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebRentServer.NETCore.ETagHelper;

namespace WebRentServer.NETCore.Models.Entities
{
    public class Vehicle : IModifiableResource
    {
        [Key]
        public int VehicleId { get; set; }
        [Required]
        [ForeignKey("RentService")]
        public int RentServiceId { get; set; }
        [Required]
        [ForeignKey("TypeOfVehicle")]
        public int TypeId { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public string YearOfManufacturing { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        public string Pictures { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public double HourlyPrice { get; set; }
        [Required]
        public bool Available { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [JsonIgnore]
        public virtual RentService RentService { get; set; }
        public virtual List<VehiclePicture> VehiclePictures { get; set; }
        [JsonIgnore]
        public virtual List<Order> Orders { get; set; }
        [JsonIgnore]
        public virtual TypeOfVehicle TypeOfVehicle { get; set; }
        [NotMapped]
        string IModifiableResource.ETag => this.GetWeakETag(JsonConvert.SerializeObject(this));
    }
}