using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using WebRentServer.NETCore.ETagHelper;
using WebRentServer.NETCore.Authentication;
using static WebRentServer.NETCore.Models.BindingModels;

namespace WebRentServer.NETCore.Controllers
{
    [ApiController]
    [Route("api/rentService")]
    public class RentServiceController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        public RentServiceController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        [HttpGet]
        [ETagFilter]
        [Route("getRentService/{serviceId}")]
        public async Task<IActionResult> GetRentService(int serviceId)
        {
            RentService service;
            try
            {
                service = await _unitOfWork.RentServices.GetAsync(serviceId);
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service does not exist");
            }
            if (service == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service does not exist");
            }

            return StatusCode(StatusCodes.Status200OK, service);
        }

        [HttpGet]
        [Route("getAll/{pageIndex}/{pageSize}/{sortType}")]
        public IActionResult getRentServices(int pageIndex, int pageSize, int sortType)
        {
         
            var items = _unitOfWork.RentServices.GetAllServicesWithSorting(pageIndex, pageSize, sortType);

            if(items==null || !items.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Rent Services");
            }

            int count = items.Count();

            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)pageSize);


            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize,
                currentPage = pageIndex,
                totalPages = TotalPages
            };

            // Setting Header  
            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            // Returing List of Customers Collections  
            return StatusCode(StatusCodes.Status200OK, items);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Route("addRentService")]
        public async Task<IActionResult> AddRentService([FromBody] RentServiceBindingModel rentServiceBindingModel)
        {
            AppUser appUser;
            try
            {
                var username = User.Identity.Name;

                var user = (AppUser)await _unitOfWork.AppUsers.FindAsync(u => u.Email == username);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }
                appUser = user;

            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User not found, try to relog");
            }

            if (appUser == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Try to relog");
            }

            if (appUser.Activated == false)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "You can't add new Rent Services right now");
            }

            RentService service = new RentService();
            service.Name = rentServiceBindingModel.Name;
            service.Description = rentServiceBindingModel.Description;
            service.Email = rentServiceBindingModel.Email;
            service.Activated = false;
            service.ServiceEdited = true;
            service.UserId = appUser.UserId;


            var postedFile = Request.Form.Files["Logo"];
            string imageName = Path.GetRandomFileName().Remove(8, 1);
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            var filePath = Path.Combine(_environment.ContentRootPath, "Images", imageName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await postedFile.CopyToAsync(stream);
            }
            service.Logo = imageName;

            try
            {
                await _unitOfWork.RentServices.AddAsync(service);
                _unitOfWork.Complete();

                appUser.RentServices.Add(service);
                _unitOfWork.AppUsers.Update(appUser);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service could not be added");
            }
            ////NotificationsHub.NotifyAdmin("New Rent Service was added");

            return Created("Rent Service was created", service);
        }

        [HttpPatch]
        [Authorize(Roles = "Manager")]
        [Route("editRentService")]
        [ETagFilter(StatusCodes.Status200OK, StatusCodes.Status201Created)]
        public async Task<IActionResult> EditRentService([FromForm] EditRentServiceBindingModel editRentServiceModel)
        {

            int serviceId = editRentServiceModel.RentServiceId;
            RentService rentService = await _unitOfWork.RentServices.GetAsync(serviceId);

            if (rentService == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service does not exist");
            }

            if (rentService.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }

            rentService.Name = editRentServiceModel.Name;
            rentService.Description = editRentServiceModel.Description;
            rentService.Email = editRentServiceModel.Email;
            rentService.Activated = false;
            rentService.ServiceEdited = true;

            var postedFile = Request.Form.Files["Logo"];
            if (postedFile != null)
            {
                string imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");

                if (rentService.Logo != imageName && System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + rentService.Logo))
                {
                    System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + rentService.Logo);

                    imageName = Path.GetRandomFileName().Remove(8, 1);
                    imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var filePath = Path.Combine(_environment.ContentRootPath, "Images", imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await postedFile.CopyToAsync(stream);
                    }
                    rentService.Logo = imageName;
                }
            }

            try
            {
                _unitOfWork.RentServices.Update(rentService);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service could not be edited");
            }
            //NotificationsHub.NotifyAdmin("New Rent Service was edited");

            return Created("Rent Service was edited", rentService);
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        [Route("getAllRentServicesManager/{pageIndex}/{pageSize}/{isApproved}/{noOffices}/{noVehicles}")]
        public async Task<IActionResult> getAllRentServicesManager(int pageIndex, int pageSize, bool isApproved, bool noOffices, bool noVehicles)
        {
            IEnumerable<RentService> source = new List<RentService>();
            if (isApproved)
            {
                source = await _unitOfWork.RentServices.FindAsync(x => x.Activated == true);
            }
            else if (noOffices == true && noVehicles == true)
            {
                source = await _unitOfWork.RentServices.FindAsync(x => x.Activated == false && x.Offices.Count == 0 && x.Vehicles.Count == 0);
            }
            else if (noOffices == true)
            {
                source = await _unitOfWork.RentServices.FindAsync(x => x.Activated == false && x.Offices.Count == 0 && x.Vehicles.Count > 0);
            }
            else if (noVehicles == true)
            {
                source = await _unitOfWork.RentServices.FindAsync(x => x.Activated == false && x.Offices.Count > 0 && x.Vehicles.Count == 0);
            }
            else
            {
                source = await _unitOfWork.RentServices.FindAsync(x => x.Activated == false && x.Offices.Count > 0 && x.Vehicles.Count > 0);
            }

            if(source==null || source.Count() < 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Rent Services");
            }
            // Get's No of Rows Count   
            int count = source.Count();


            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Returns List of Customer after applying Paging   
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            //var items = _unitOfWork.RentServices.GetAll(pageIndex, pageSize);


            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize,
                currentPage = pageIndex,
                totalPages = TotalPages
            };
           
            // Setting Header  
            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            //// Returing List of Customers Collections  
            return StatusCode(StatusCodes.Status200OK, items);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("getAllRentServicesAdmin/{pageIndex}/{pageSize}/{approved}/{notApproved}/{edited}/{notEdited}/{sort}")]
        public async Task<IActionResult> getAllRentServicesAdmin(int pageIndex, int pageSize, bool approved, bool notApproved, bool edited, bool notEdited, string sort)
        {
            IEnumerable < RentService> source= new List<RentService>();
            if (approved)
            {
                source = await _unitOfWork.RentServices.FindAsync(x => x.Activated == true);
            }
            if (notApproved)
            {
                source = source.Union(await _unitOfWork.RentServices.FindAsync(x => x.Activated == false), new RentServiceComparer());
            }
            if (edited)
            {
                source = source.Union(await _unitOfWork.RentServices.FindAsync(x => x.ServiceEdited == true), new RentServiceComparer());
               
            }
            if (notEdited)
            {
                source = source.Union(await _unitOfWork.RentServices.FindAsync(x => x.ServiceEdited == false), new RentServiceComparer());
               
            }

            if (!approved && !notApproved && !edited && !notEdited)
            {
                source = await _unitOfWork.RentServices.GetAllAsync();

            }

            if(source==null || !source.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Rent Services");
            }

            if (sort == "approvedFirst")
            {
                source = source.OrderByDescending(x=>x.Activated==true);
            }
            else if (sort == "notApprovedFirst")
            {
                source = source.OrderByDescending(x => x.Activated == false);
            }
            else if (sort == "editedFirst")
            {
                source = source.OrderByDescending(x => x.ServiceEdited == true);
            }
            else if (sort == "notEditedFirst")
            {
                source = source.OrderByDescending(x => x.ServiceEdited == false);
            }


            // Get's No of Rows Count   
            int count = source.Count();


            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Returns List of Customer after applying Paging   
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            //var items = _unitOfWork.RentServices.GetAll(pageIndex, pageSize);



            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize,
                currentPage = pageIndex,
                totalPages = TotalPages
            };

            // Setting Header  
            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            // Returing List of Customers Collections  
            return StatusCode(StatusCodes.Status200OK, items);
        }

        [HttpGet]
        [Route("getServiceLogo")]
        public async Task<IActionResult> GetServiceLogo(string path)
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

        [HttpDelete]
        [Authorize(Roles = "Manager")]
        [Route("deleteRentService/{serviceId}")]
        public IActionResult DeleteRentService(int serviceId)
        {
            RentService rentService = _unitOfWork.RentServices.GetAsync(serviceId).Result;
            if (rentService == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service could not be found.");
            }

            try
            {
                if (System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + rentService.Logo))
                {
                    System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + rentService.Logo);
                }

                _unitOfWork.RentServices.Remove(rentService);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service could not be deleted");
            }
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet]
        [ETagFilter]
        [Authorize(Roles = "Admin")]
        [Route("activateRentService/{serviceId}/{activated}")]
        public async Task<IActionResult> ActivateRentService(int serviceId,bool activated)
        {
            RentService rentService = await _unitOfWork.RentServices.GetAsync(serviceId);
            if (rentService == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service does not exist");
            }

            if (rentService.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }

            rentService.Activated = activated;
            rentService.ServiceEdited = false;

            try
            {

                _unitOfWork.RentServices.Update(rentService);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service cound not be activated");
            }


            MailMessage mail = new MailMessage("easyrent.e3@gmail.com", "easyrent.e3@gmail.com");
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("easyrent.e3@gmail.com", "e3942014pusgs2018");
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            mail.From = new MailAddress("easyrent.e3@gmail.com");
            mail.To.Add(rentService.User.Email);

            if (activated)
            {
                mail.Subject = "Rent Service approved";
                mail.Body = string.Format("Your Rent Service '{0}' was approved by our administrators!",rentService.Name);

            }
            else
            {
                mail.Subject = "Rent Service wasn't approved";
                mail.Body = string.Format("Unfortunately your Rent Service '{0}' wasn't approved!", rentService.Name);
            }

            try
            {
                client.Send(mail);
            }
            catch
            {

            }

            return StatusCode(StatusCodes.Status200OK, string.Format("Rent Service was {0}", activated == true ? "activated" : "deactivated"));
        }
    }

    public class RentServiceComparer : IEqualityComparer<RentService>
    {
        public bool Equals(RentService x, RentService y)
        {
            //Check whether the objects are the same object. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether the products' properties are equal. 
            return x != null && y != null && x.RentServiceId.Equals(y.RentServiceId);
        }

        public int GetHashCode(RentService obj)
        {
            //Get hash code for the Name field if it is not null. 
            int hashProductName = obj.Name == null ? 0 : obj.Name.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = obj.RentServiceId.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }
}
