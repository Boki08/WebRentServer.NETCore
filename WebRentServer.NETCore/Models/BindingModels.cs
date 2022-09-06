using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WebRentServer.NETCore.Models
{
    public class BindingModels
    {
        public class EditRentServiceBindingModel
        {
            [Required]
            //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
            [Display(Name = "RentServiceId")]
            public int RentServiceId { get; set; }

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
    }
}
