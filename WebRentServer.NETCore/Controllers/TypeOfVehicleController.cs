using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRentServer.NETCore.Authentication;
using WebRentServer.NETCore.ETagHelper;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;

namespace WebRentServer.NETCore.Controllers
{
    [Route("api/typeOfVehicle")]
    public class TypeOfVehicleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public TypeOfVehicleController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        [HttpGet]
        [Route("getVehicleTypes")]
        public async Task<IActionResult> GetVehicleTypesAsync()
        {
            var vehicleTypes = await _unitOfWork.TypesOfVehicles.GetAllAsync();

            if (vehicleTypes == null || !vehicleTypes.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Vehicle Types");
            }
           
            return StatusCode(StatusCodes.Status200OK, vehicleTypes);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("getVehicleTypesPaged/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetVehicleTypesPagedAsync(int pageIndex, int pageSize)
        {
            var vehicles= _unitOfWork.TypesOfVehicles.GetAllPaged(pageIndex, pageSize);
            if (vehicles == null || !vehicles.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Vehicle Types");
            }
           
            int TotalCount = await _unitOfWork.TypesOfVehicles.CountElementsAsync();

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("addVehicleType")]
        public async Task<IActionResult> AddVehicleTypeAsync(TypeOfVehicle typeOfVehicle)
        {
            IEnumerable<TypeOfVehicle> types = await _unitOfWork.TypesOfVehicles.GetAllAsync();

            if (typeOfVehicle == null || typeOfVehicle.Type == null || typeOfVehicle.Type == "")
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Type can not be empty");
            }

            foreach (TypeOfVehicle t in types)
            {
                if (t.Type == typeOfVehicle.Type)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "This Vehicle Type already exists");
                }
            }

            typeOfVehicle.Type = typeOfVehicle.Type.Trim();

            await _unitOfWork.TypesOfVehicles.AddAsync(typeOfVehicle);
            _unitOfWork.Complete();

            return Created("Vehicle type added", typeOfVehicle);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("deleteTypeOfVehicle/{typeId}")]
        public async Task<IActionResult> DeleteTypeOfVehicle(int typeId)
        {
            TypeOfVehicle typeOfVehicle = await _unitOfWork.TypesOfVehicles.GetAsync(typeId);
            if (typeOfVehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "This Vehicle Type cant be found");
            }

            try
            {
                _unitOfWork.TypesOfVehicles.Remove(typeOfVehicle);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle Type can't be added");
            }

            return StatusCode(StatusCodes.Status200OK, typeOfVehicle);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("getTypeOfVehicle/{typeId}")]
        [ETagFilter(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTypeOfVehicle(int typeId)
        {
            TypeOfVehicle typeOfVehicle = await _unitOfWork.TypesOfVehicles.GetAsync(typeId);
            if (typeOfVehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "This Vehicle Type cant be found");
            }

            //var jsonObj = JsonConvert.SerializeObject(typeOfVehicle, Formatting.None, setting);
            //var eTag = ETagHelper.GetETag(Encoding.UTF8.GetBytes(jsonObj));

            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", ETagHelper.ETAG_HEADER);
            //HttpContext.Current.Response.Headers.Add(ETagHelper.ETAG_HEADER, JsonConvert.SerializeObject(eTag));

            //if (HttpContext.Current.Request.Headers.Get(ETagHelper.MATCH_HEADER) != null && HttpContext.Current.Request.Headers[ETagHelper.MATCH_HEADER].Trim('"') == eTag)
            //    return new StatusCodeResult(HttpStatusCode.NotModified, new HttpRequestMessage());

            return StatusCode(StatusCodes.Status200OK, typeOfVehicle);
        }

        [HttpPatch]
        [Authorize(Roles = "Admin")]
        [Route("editTypeOfVehicle")]
        [ETagFilter(StatusCodes.Status200OK, StatusCodes.Status201Created)]
        public async Task<IActionResult> EditTypeOfVehicle(TypeOfVehicle type)
        {
            TypeOfVehicle typeOfVehicle = await _unitOfWork.TypesOfVehicles.GetAsync(type.TypeId);
            if (typeOfVehicle == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "This Vehicle Type can't be found");
            }
            if (typeOfVehicle.HasPreconditionFailed(HttpContext.Request))
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new Response { Status = "Error", Message = "Object was already modified" });
            }
            //var jsonObj = JsonConvert.SerializeObject(typeOfVehicle, Formatting.None, setting);
            //var eTag = ETagHelper.GetETag(Encoding.UTF8.GetBytes(jsonObj));



            //if (HttpContext.Current.Request.Headers.Get(ETagHelper.MATCH_HEADER) == null || HttpContext.Current.Request.Headers[ETagHelper.MATCH_HEADER].Trim('"') != eTag)
            //{
            //    HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", ETagHelper.ETAG_HEADER);
            //    HttpContext.Current.Response.Headers.Add(ETagHelper.ETAG_HEADER, JsonConvert.SerializeObject(eTag));

            //    return new StatusCodeResult(HttpStatusCode.PreconditionFailed, new HttpRequestMessage());

            //}

            typeOfVehicle.Type = type.Type.Trim();

            try
            {
                _unitOfWork.TypesOfVehicles.Update(typeOfVehicle);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Vehicle Type can't be edited");
            }

            //jsonObj = JsonConvert.SerializeObject(typeOfVehicle, Formatting.None, setting);
            //eTag = ETagHelper.GetETag(Encoding.UTF8.GetBytes(jsonObj));

            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", ETagHelper.ETAG_HEADER);
            //HttpContext.Current.Response.Headers.Add(ETagHelper.ETAG_HEADER, JsonConvert.SerializeObject(eTag));

            return StatusCode(StatusCodes.Status200OK, typeOfVehicle);
        }
    }
}
