using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.ETagHelper;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using static WebRentServer.NETCore.Models.BindingModels;

namespace WebRentServer.NETCore.Controllers
{
    [Route("api/vehicle")]
    public class VehicleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public VehicleController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }
        
        [HttpGet]
        [ETagFilter(StatusCodes.Status200OK)]
        [Route("getVehicle/{vehicleId}")]
        public IActionResult GetVehicle(int vehicleId)
        {
            Vehicle vehicle;
            try
            {
                vehicle = _unitOfWork.Vehicles.Get(vehicleId);
            }
            catch 
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle does not exist");
            }
            if (vehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle does not exist");
            }

            return StatusCode(StatusCodes.Status200OK, vehicle);
        }

        [HttpGet]
        [Authorize(Roles = "Manager, Admin")]
        [Route("allServiceVehicles/{pageIndex}/{pageSize}/{serviceID}")]
        public IActionResult GetServiceVehiclesAsync(int pageIndex, int pageSize, int serviceID)
        {
            var vehicles = _unitOfWork.Vehicles.GetAllWithPics(pageIndex,pageSize, serviceID).ToList();

            if (vehicles == null || !vehicles.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Vehicles");
            }

            // Display TotalCount to Records to User  
            int TotalCount = _unitOfWork.Vehicles.CountServiceVehicles(serviceID);

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);


            // if CurrentPage is greater than 1 means it has previousPage  
            var previousPage = pageIndex > 1 ? "Yes" : "No";

            // if TotalPages is greater than CurrentPage means it has nextPage  
            var nextPage = pageIndex < TotalPages ? "Yes" : "No";

            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize,
                currentPage = pageIndex,
                totalPages = TotalPages,
            };

            //// Setting Header  
            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            //// Returing List of Customers Collections  
            return StatusCode(StatusCodes.Status200OK, vehicles);
        }

        [HttpGet]
        [Route("getServiceVehiclesSort/{pageIndex}/{pageSize}/{serviceID}/{available}/{price}/{type}")]
        public IActionResult GetServiceVehiclesSort(int pageIndex, int pageSize, int serviceID,bool available,string price,int type)
        {
            var vehicles = _unitOfWork.Vehicles.GetAllWithPicsUser(pageIndex, pageSize, serviceID,available,price,type).ToList();

            if (vehicles == null || !vehicles.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Vehicles");
            }

            int TotalCount = _unitOfWork.Vehicles.CountAllWithPicsUser(serviceID,available, price, type);
           
            int TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);
           
            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize,
                currentPage = pageIndex,
                totalPages = TotalPages,
              
            };

            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));

            return StatusCode(StatusCodes.Status200OK, vehicles);

        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        [ETagFilter(StatusCodes.Status200OK)]
        [Route("disableVehicle/{vehicleId}/{enabled}")]
        public IActionResult DisableVehicle(int vehicleId, bool enabled)
        {
            Vehicle vehicle = _unitOfWork.Vehicles.Get(vehicleId);

            if (vehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle does not exist");
            }
            if (vehicle.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }

            if (vehicle.Available == true)
            {

                vehicle.Enabled = enabled;
                _unitOfWork.Vehicles.Update(vehicle);
                _unitOfWork.Complete();

                return StatusCode(StatusCodes.Status200OK, vehicle);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle is currently rented");
            }
        }

        [HttpGet]
        [Route("getVehiclePicture")]
        public async Task<IActionResult> GetVehiclePictureAsync(string path)
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

        [HttpGet]
        [Route("getVehiclePictures/{vehicleId}")]
        public IActionResult GetVehiclePictures(int vehicleId)
        {

            var photos = _unitOfWork.VehiclePictures.Find(x=>x.VehicleId==vehicleId).ToList();

            if (photos.Count == 0)
            {
                photos.Add(new VehiclePicture() { Data= "default-placeholder.png" ,VehicleId=vehicleId});
            }

            return StatusCode(StatusCodes.Status200OK, photos);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Route("addVehicle")]
        public async Task<IActionResult> AddVehicleAsync([FromBody] AddVehicleBindingModel vehicleBindingModel)
        {
            int numberOfImages = vehicleBindingModel.ImagesNum;
            Vehicle vehicle = new Vehicle();
            vehicle.Model = vehicleBindingModel.Model;
            vehicle.Description = vehicleBindingModel.Description;
            vehicle.Manufacturer = vehicleBindingModel.Manufacturer;
            vehicle.YearOfManufacturing = vehicleBindingModel.YearOfManufacturing;
            vehicle.RentServiceId = vehicleBindingModel.RentServiceId;
            vehicle.Available = true;
            vehicle.Enabled = false;
            vehicle.TypeId = vehicleBindingModel.TypeId;
            vehicle.HourlyPrice = vehicleBindingModel.HourlyPrice;

            _unitOfWork.Vehicles.Add(vehicle);
            _unitOfWork.Complete();

            if (numberOfImages < 1)
            {
                _unitOfWork.VehiclePictures.Add(new VehiclePicture() { Data = "default-placeholder.png", VehicleId = vehicle.VehicleId });
                _unitOfWork.Complete();
            }
            else
            {
                string imageName;
                for (int i = 0; i < numberOfImages; i++)
                {

                    var postedFile = Request.Form.Files[String.Format("Image{0}", i)];
                    imageName = Path.GetRandomFileName().Remove(8, 1);
                    imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var filePath = Path.Combine(_environment.ContentRootPath, "Images", imageName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await postedFile.CopyToAsync(stream);
                    }
                    _unitOfWork.VehiclePictures.Add(new VehiclePicture() { Data = imageName, VehicleId = vehicle.VehicleId });
                    _unitOfWork.Complete();
                }
            }
            return Created("Rent Service was created", vehicle);
        }

        [HttpPatch]
        [Authorize(Roles = "Manager")]
        [ETagFilter(StatusCodes.Status200OK, StatusCodes.Status201Created)]
        [Route("editVehicle")]
        public async Task<IActionResult> EditVehicleAsync([FromBody] EditVehicleBindingModel vehicleBindingModel)
        {
            int vehicleId = vehicleBindingModel.VehicleId;
            Vehicle vehicle = _unitOfWork.Vehicles.Get(vehicleId);

            if (vehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle does not exist");
            }
            if (vehicle.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }

            int numberOfImages = vehicleBindingModel.ImagesNum;
            vehicle.Model = vehicleBindingModel.Model;
            vehicle.Description = vehicleBindingModel.Description;
            vehicle.Manufacturer = vehicleBindingModel.Manufacturer;
            vehicle.YearOfManufacturing = vehicleBindingModel.YearOfManufacturing;
            vehicle.TypeId = vehicleBindingModel.TypeId;
            vehicle.HourlyPrice = vehicleBindingModel.HourlyPrice;

            try
            {
                _unitOfWork.Vehicles.Update(vehicle);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle could not be editer");
            }

            List<VehiclePicture> pictures = _unitOfWork.VehiclePictures.Find(x => x.VehicleId == vehicleId).ToList();

            if (numberOfImages > 0)
            {
                List<PicData> picsData = new List<PicData>();
                for (int i = 0; i < numberOfImages; i++)
                {
                    var postedFile = Request.Form.Files[String.Format("Image{0}", i)];
                    var imgName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).ToArray()).Replace(" ", "-") + Path.GetExtension(postedFile.FileName);
                    if (imgName == "default-placeholder.png")
                        continue;
                    picsData.Add(new PicData() { Name = imgName, Position = i});
                }
                foreach (VehiclePicture picture in pictures)
                {
                    PicData picData = picsData.Find(x => x.Name == picture.Data);
                    if (picData == null)
                    {
                        if (System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + picture.Data))
                        {
                            System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + picture.Data);
                        }
                        _unitOfWork.VehiclePictures.Remove(picture);
                        _unitOfWork.Complete();
                    }
                    else
                    {
                        picsData.Remove(picData);
                    }
                }

                foreach (PicData picData in picsData)
                {
                    var postedFile = Request.Form.Files[String.Format("Image{0}", picData.Position)];
                    picData.Name = Path.GetRandomFileName().Remove(8, 1);
                    picData.Name = picData.Name + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                    var filePath = Path.Combine(_environment.ContentRootPath, "Images", picData.Name);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await postedFile.CopyToAsync(stream);
                    }

                    _unitOfWork.VehiclePictures.Add(new VehiclePicture() { Data = picData.Name, VehicleId = vehicle.VehicleId });
                    _unitOfWork.Complete();
                }

            }
            return Created("Vehicle was edited", vehicle);
        }

        [HttpDelete]
        [Authorize(Roles = "Manager")]
        [Route("deleteVehicle/{vehicleId}")]
        public async Task<IActionResult> DeleteVehicleAsync(int vehicleId)
        {
            Vehicle vehicle = await _unitOfWork.Vehicles.GetAsync(vehicleId);
            if (vehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle could not be found");
            }

            try
            {
                IEnumerable<VehiclePicture> pictures = await _unitOfWork.VehiclePictures.FindAsync(x=>x.VehicleId==vehicleId);
                foreach (VehiclePicture picture in pictures)
                {
                    if (System.IO.File.Exists(_environment.ContentRootPath + "Images\\" + picture.Data))
                    {
                        System.IO.File.Delete(_environment.ContentRootPath + "Images\\" + picture.Data);
                    }
                    _unitOfWork.VehiclePictures.Remove(picture);
                }

                _unitOfWork.Vehicles.Remove(vehicle);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle could not be deleted");
            }
            return StatusCode(StatusCodes.Status200OK, "Vehicle was deleted");
        }
    }
}
