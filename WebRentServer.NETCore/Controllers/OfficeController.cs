using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.ETagHelper;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using static WebRentServer.NETCore.Models.BindingModels;

namespace WebRentServer.NETCore.Controllers
{
    [Route("api/office")]
    public class OfficeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public OfficeController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        [HttpGet]
        [Route("allServiceOffices/{pageIndex}/{pageSize}/{serviceID}")]
        public IActionResult GetServiceOfficesAsync(int pageIndex, int pageSize, int serviceID)
        {
            var offices = _unitOfWork.Offices.GetAll(pageIndex,  pageSize,  serviceID);

            if (offices == null || offices.Count() < 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Offices");
            }

            // Get's No of Rows Count   
            int count = offices.Count();

            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)pageSize);


            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize,
                currentPage = pageIndex,
                totalPages = TotalPages,
               
            };

            // Setting Header  
            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            // Returing List of Customers Collections  
            return StatusCode(StatusCodes.Status200OK, offices);

        }

        [HttpGet]
        [Route("getOffices/{serviceID}")]
        public async Task<IActionResult> GetAllServiceOffices(int serviceID)
        {
            List<Office> offices;
            try
            {
                offices = (List<Office>)await _unitOfWork.Offices.FindAsync(x => x.RentServiceId == serviceID);
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Offices");
            }
            if (offices == null || offices.Count<1)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Offices");
            }

            return StatusCode(StatusCodes.Status200OK, offices);
        }

        [HttpGet]
        [ETagFilter(StatusCodes.Status200OK)]
        [Route("getOffice/{officeID}")]
        public async Task<IActionResult> GetServiceOffice( int officeID)
        {
            Office office;
            try
            {
                office = await _unitOfWork.Offices.GetAsync( officeID);
            }
            catch 
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office does not exist");
            }
            if (office == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office does not exist");
            }

            return StatusCode(StatusCodes.Status200OK, office);
        }


        [HttpGet]
        [Route("getOfficePicture")]
        public async Task<IActionResult> GetOfficePicture(string path)
        {
            if (path == null)
            {
                path = "default-placeholder.png";
            }

            var filePath = Path.Combine(_environment.ContentRootPath, "Images", $"{path}");
            if (!System.IO.File.Exists(filePath))
            {
                path = "default-placeholder.png";
                filePath = Path.Combine(_environment.ContentRootPath, "Images", $"{path}");
            }
            var ext = Path.GetExtension(filePath);

            var contents = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(contents, String.Format("image/{0}", ext));
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Route("addOffice")]
        public async Task<IActionResult> AddOfficeAsync([FromForm] AddOfficeBindingModel officeBindingModel)
        {
            Office office = new Office();
            office.Address = officeBindingModel.Address;

            var numberFormat = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InstalledUICulture.NumberFormat.Clone();
            
            numberFormat.NumberDecimalSeparator = ".";

            office.Latitude = double.Parse(officeBindingModel.Latitude, numberFormat);
            office.Longitude = double.Parse(officeBindingModel.Longitude, numberFormat);
            office.RentServiceId = officeBindingModel.RentServiceId;

            var postedFile = Request.Form.Files["Picture"];
            string imageName = Path.GetRandomFileName().Remove(8, 1);
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);

            var filePath = Path.Combine(_environment.ContentRootPath, "Images", imageName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await postedFile.CopyToAsync(stream);
            }
                office.Picture = imageName;

            try
            {
                await _unitOfWork.Offices.AddAsync(office);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office could not be added");
            }
            return Created("Office was created", office);
        }

        
        [HttpPatch]
        [Route("editOffice/{officeId:int}")]
        [Authorize(Roles = "Manager")]
        [ETagFilter(StatusCodes.Status200OK, StatusCodes.Status201Created)]
        public async Task<IActionResult> EditOffice(int officeId, [FromForm] OfficeBindingModel officeBindingModel)
        {
            Office office  = await _unitOfWork.Offices.GetAsync(officeId);

            if (office == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office does not exist");
            }

            if (office.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }

            office.Address = officeBindingModel.Address;

            var numberFormat = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InstalledUICulture.NumberFormat.Clone();

            numberFormat.NumberDecimalSeparator = ".";

            office.Latitude = double.Parse(officeBindingModel.Latitude, numberFormat);
            office.Longitude = double.Parse(officeBindingModel.Longitude, numberFormat);
         
            try
            {
                _unitOfWork.Offices.Update(office);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office could not be edited");
            }

            var postedFile = Request.Form.Files["Picture"];
            if (postedFile != null)
            {
                string imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");

                if (office.Picture != imageName && System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + office.Picture))
                {

                    System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + office.Picture);
                    
                    imageName = Path.GetRandomFileName().Remove(8, 1);
                    imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var filePath = Path.Combine(_environment.ContentRootPath, "~/Images/" + imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await postedFile.CopyToAsync(stream);
                    }

                    office.Picture = imageName;
                }
            }
            return Created("Office was edited", office);
        }

        [HttpDelete]
        [Authorize(Roles = "Manager")]
        [Route("deleteOffice/{officeId}")]
        public async Task<IActionResult> DeleteOffice(int officeId)
        {
            Office office = await _unitOfWork.Offices.GetAsync(officeId);
            if (office == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office does not exist");
            }
            try
            {
                if (System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + office.Picture))
                {
                    System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + office.Picture);
                }

                _unitOfWork.Offices.Remove(office);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Office could not be deleted");
            }
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
