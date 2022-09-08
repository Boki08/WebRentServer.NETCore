using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WebRentServer.NETCore.Models
{
    public class BindingModels
    {
        public class RentServiceBindingModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Description")]
            public string Description { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }
        public class EditRentServiceBindingModel : RentServiceBindingModel
        {
            [Required]
            [Display(Name = "RentServiceId")]
            public int RentServiceId { get; set; }
        }
        public class OfficeBindingModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [Required]
            [StringLength(30, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Latitude")]
            public string Latitude { get; set; }

            [Required]
            [StringLength(30, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Longitude")]
            public string Longitude { get; set; }
        }
        public class AddOfficeBindingModel : OfficeBindingModel
        {
            [Required]
            [Display(Name = "RentServiceId")]
            public int RentServiceId { get; set; }
        }
        public class EditOfficeBindingModel : OfficeBindingModel
        {
            [Required]
            [Display(Name = "OfficeId")]
            public int OfficeId { get; set; }
        }
        public class PicData
        {
            [Required]
            [Display(Name = "Position")]
            public int Position { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Name")]
            public string Name { get; set; }
        }
        public class VehicleBindingModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Model")]
            public string Model { get; set; }

            [Required]
            [StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Description")]
            public string Description { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Manufacturer")]
            public string Manufacturer { get; set; }

            [Required]
            [StringLength(4, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "YearOfManufacturing")]
            public string YearOfManufacturing { get; set; }

            [Required]
            [Display(Name = "ImagesNum")]
            public int ImagesNum { get; set; }

            [Required]
            [Display(Name = "TypeId")]
            public int TypeId { get; set; }

            [Required]
            [Display(Name = "HourlyPrice")]
            public double HourlyPrice { get; set; }
        }
        public class EditVehicleBindingModel : VehicleBindingModel
        {
            [Required]
            [Display(Name = "VehicleId")]
            public int VehicleId { get; set; }
        }
        public class AddVehicleBindingModel : VehicleBindingModel
        {
            [Required]
            [Display(Name = "RentServiceId")]
            public int RentServiceId { get; set; }
        }
    }
}