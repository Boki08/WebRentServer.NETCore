using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.Encrypting;
using WebRentServer.NETCore.ETagHelper;
using WebRentServer.NETCore.Models;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using static WebRentServer.NETCore.Models.BindingModels;

namespace RentApp.Controllers
{
    [Route("api/appUser")]
    public class AppUserController : Controller
    {

        JsonSerializerSettings setting = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<RAIdentityUser> _userManager;
        private readonly IOptions<AESConfig> _AESConfig;
        public AppUserController(IUnitOfWork unitOfWork, IOptions<AESConfig> aesConfig, IWebHostEnvironment environment, UserManager<RAIdentityUser> userManager)
        {

            _unitOfWork = unitOfWork;
            _environment = environment;
            _userManager = userManager;
            _AESConfig = aesConfig;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("allUsers/{pageIndex}/{pageSize}/{type}/{editedFirst}/{approvedFirst}")]
        public async Task<IActionResult> GetAllUsersAsync(int pageIndex, int pageSize, string type, bool editedFirst, bool approvedFirst)
        {
            var usersByRole = await _userManager.GetUsersInRoleAsync(type);
            
            List<AppUser> source = usersByRole.Select(x => x.AppUser).ToList();

            if (source == null || source.Count() < 1)
            {
                if (type == "AppUser")
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "There are no Users");
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "There are no Managers");
                }
            }

            if (editedFirst)
            {
                source = source.OrderByDescending(x => x.ProfileEdited).ToList();

            }
            else if (approvedFirst)
            {
                source = source.OrderByDescending(x => x.Activated).ToList();
            }
            

            // Get's No of Rows Count   
            int count = source.Count();


            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Returns List of Customer after applying Paging   
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();


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

            return StatusCode(StatusCodes.Status200OK, items);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("getDocumentPicture")]
        public async Task<IActionResult> GetDocumentPictureAsync(string path)
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

            byte[] contents;

            if(!path.Equals("default-placeholder.png"))
                AES_Symm_Algorithm.DecryptFile(filePath, out contents, _AESConfig.Value.Key);
            else
                contents = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(contents, String.Format("image/{0}", ext));

        }
        

        [HttpGet]
        [ETagFilter(StatusCodes.Status200OK)]
        [Route("getCurrentUser")]
        public async Task<IActionResult> GetAppUserAsync()
        {
            AppUser appUser;
            try
            {
                var username = User.Identity.Name;

                var user = await _unitOfWork.AppUsers.FindAsync(u => u.Email == username);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }
                appUser = user as AppUser;

                return StatusCode(StatusCodes.Status200OK, appUser);
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
            }
        }

        [HttpGet]
        [ETagFilter(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, AppUser")]
        [Route("getUserById/{userId}")]
        public async Task<IActionResult> GetAppUserByIdAsync(int userId)
        {
            try
            {
                var appUser = await _unitOfWork.AppUsers.GetAsync(userId);
                return StatusCode(StatusCodes.Status200OK, appUser);
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
            }
        }

        [HttpPatch]
        [ETagFilter(StatusCodes.Status200OK)]
        [Route("editAppUser")]
        public IActionResult EditUser([FromForm]AppUserBindingModel appUserBindingModel)
        {
            string imageName = null;
            AppUser appUser;
            try
            {
                var username = User.Identity.Name;

                var user = _unitOfWork.AppUsers.Find(u => u.Email == username).FirstOrDefault();
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }
                appUser = user;

            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
            }

            if (appUser.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }

            appUser.FullName = appUserBindingModel.FullName;
            appUser.BirthDate = appUserBindingModel.BirthDate;
            appUser.Email = appUserBindingModel.Email;
            appUser.ProfileEdited = true;

            if (appUser.DocumentPicture == null || appUser.DocumentPicture == "")
            {
                var postedFile = Request.Form.Files["Image"];
                if (postedFile != null)
                {
                    imageName = Path.GetRandomFileName().Remove(8, 1);
                    imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var filePath = Path.Combine(_environment.ContentRootPath, "Images", imageName);

                    appUser.DocumentPicture = imageName;

                    byte[] fileData;

                    Stream stream = postedFile.OpenReadStream();
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        fileData =  memoryStream.ToArray();
                    }

                    AES_Symm_Algorithm.EncryptFile(fileData, filePath, _AESConfig.Value.Key);
                }
            }

            try
            {
                _unitOfWork.AppUsers.Update(appUser);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Profile could not be edited.");
            }
            return StatusCode(StatusCodes.Status200OK, appUser);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin, AppUser")]
        [Route("deleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            AppUser appUser;
            try
            {
                appUser = _unitOfWork.AppUsers.Get(userId);
                if (appUser == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "User could not found.");
                }

                if (System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + appUser.DocumentPicture))
                {
                    System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + appUser.DocumentPicture);
                }

                List<Order> source = _unitOfWork.Orders.Find(x=>x.UserId== appUser.UserId).ToList();

                foreach (Order o in source)
                {
                    _unitOfWork.Orders.Remove(o);
                    _unitOfWork.Complete();
                }

                _unitOfWork.AppUsers.Remove(appUser);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User could not be deleted");
            }
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPatch]
        [Authorize(Roles = "Admin")]
        [ETagFilter(StatusCodes.Status200OK)]
        [Route("activateUser/{userId:int}")]
        public async Task<IActionResult> ActivateUserAsync(int userId, [FromForm] bool activated)
        {
            AppUser appUser = await _unitOfWork.AppUsers.GetAsync(userId);
            if (appUser == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User does not exist");
            }

            if (appUser.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
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
            mail.To.Add(appUser.Email);


            if (activated)
            {
                appUser.Activated = true;

                mail.Subject = "Profile approved";
                mail.Body = "Your profile was approved by our administrators!";

            }
            else
            {
                appUser.Activated = false;

                mail.Subject = "Profile wasn't approved";
                mail.Body = "Unfortunately your profile wasn't approved. Try changing your personal information.";
                if (System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + appUser.DocumentPicture))
                {
                    System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + appUser.DocumentPicture);
                    appUser.DocumentPicture = null;
                }
            }
            appUser.ProfileEdited = false;

            try
            {
                _unitOfWork.AppUsers.Update(appUser);
                _unitOfWork.Complete();

            }
            catch
            {
                return BadRequest(string.Format("User could not be {0}", activated?"activated":"deactivated"));
            }


            try
            {
                client.Send(mail);
            }
            catch
            {

            }

            return StatusCode(StatusCodes.Status200OK, appUser);
        }

        [HttpGet]
        [Route("canUserOrder")]
        public IActionResult CanUserOrder()
        {
            AppUser appUser;
            try
            {
                var username = User.Identity.Name;

                var user = _unitOfWork.AppUsers.Find(u => u.Email == username).FirstOrDefault();
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }
                appUser = user;
                if (appUser.Activated)
                {
                    return StatusCode(StatusCodes.Status200OK, true);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Your profile is not activated");
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User not found, try to relog");
            }
        }
    }
}