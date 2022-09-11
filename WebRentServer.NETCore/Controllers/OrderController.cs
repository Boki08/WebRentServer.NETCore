using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using static WebRentServer.NETCore.Models.BindingModels;

namespace RentApp.Controllers
{
    [Route("api/order")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private static readonly Object orderLock = new Object();

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Authorize(Roles = "AppUser")]
        [Route("postOrder")]
        public IActionResult PostOrder([FromForm] OrderBindingModel orderBindingModel)
        {
            lock (orderLock)
            {
                if (orderBindingModel.DepartureDate.Date < DateTime.Now.Date || orderBindingModel.ReturnDate < orderBindingModel.DepartureDate || orderBindingModel.ReturnDate < DateTime.Now)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "You can not add an Order with these dates!");
                }

                Vehicle vehicle = unitOfWork.Vehicles.Get(orderBindingModel.VehicleId);
                if (vehicle == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Vehicle not found");
                }

                AppUser appUser;
                try
                {
                    var username = User.Identity.Name;


                    var user = unitOfWork.AppUsers.Find(u => u.Email == username).FirstOrDefault();
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
                    return StatusCode(StatusCodes.Status400BadRequest, "User not found, try to relog");
                }
                else if (appUser.Activated == false)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Your profile is not activated");
                }
                orderBindingModel.UserId = appUser.UserId;

                var hours = (orderBindingModel.ReturnDate - orderBindingModel.DepartureDate).TotalHours;
                orderBindingModel.Price = vehicle.HourlyPrice * hours;

                if (vehicle.Available == true)
                {
                    vehicle.Available = false;
                    unitOfWork.Vehicles.Update(vehicle);
                    unitOfWork.Complete();
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Vehicle isn't available.");
                }

                Order order = new Order()
                {
                    VehicleId = orderBindingModel.VehicleId,
                    UserId = orderBindingModel.UserId,
                    DepartureOfficeId = orderBindingModel.DepartureOfficeId,
                    VehicleReturned = false,
                    ReturnOfficeId = orderBindingModel.ReturnOfficeId,
                    DepartureDate = orderBindingModel.DepartureDate,
                    ReturnDate = orderBindingModel.ReturnDate,
                    Price = orderBindingModel.Price
                };

                unitOfWork.Orders.Add(order);
                try
                {
                    unitOfWork.Complete();
                }
                catch
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Cannot add new Order.");
                }
                return Created("Order was created", order);
            }
        }

        [HttpGet]
        [Authorize(Roles = "AppUser")]
        [Route("getAllUserOrders/{pageIndex}/{pageSize}")]
        public IActionResult GetAllUserOrders(int pageIndex, int pageSize)
        {
            int userId;
            AppUser appUser;
            try
            {
                var username = User.Identity.Name;

                var user = unitOfWork.AppUsers.Find(u => u.Email == username).FirstOrDefault();
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }
                appUser = user;
                userId = appUser.UserId;

            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User not found. Try to relog");
            }

            var orders = unitOfWork.Orders.GetAllUserOrders(pageIndex, pageSize, userId).ToList();

            if (orders == null || orders.Count < 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "There are no Orders");
            }

            // Display TotalCount to Records to User  
            int TotalCount = unitOfWork.Orders.CountAllUserOrders(userId);

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

            // Setting Header  
            //HttpContext.Current.Response.Headers.Add("Access-Control-Expose-Headers", "Paging-Headers");
            //HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
            // Returing List of Customers Collections  
            return StatusCode(StatusCodes.Status200OK, orders);
        }

        [HttpPatch]
        [Authorize(Roles = "AppUser")]
        [Route("returnVehicle/{orderId:int}")]
        public IActionResult ReturnVehicle(int orderId)
        {
            int userId;
            AppUser appUser;
            Order order;
            try
            {
                var username = User.Identity.Name;

                var user = unitOfWork.AppUsers.Find(u => u.Email == username).FirstOrDefault();
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }
                appUser = user;
                userId = appUser.UserId;
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User not found. Try to relog");
            }
            try
            {
                var order1 = unitOfWork.Orders.Get(orderId);
                if (order1 == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Order does not exist.");
                }
                order = order1;
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Order could not be found.");
            }

            Vehicle vehicle = order.Vehicle;

            if (vehicle.Available == false && order.UserId == userId && order.VehicleReturned == false && order.ReturnDate.Date <= DateTime.Now.Date)
            {
                vehicle.Available = true;
                unitOfWork.Vehicles.Update(vehicle);

                order.VehicleReturned = true;
                unitOfWork.Orders.Update(order);

                try
                {
                    unitOfWork.Complete();
                }
                catch
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Can't return the Vehicle.");
                }
                return StatusCode(StatusCodes.Status200OK, vehicle);
            }
            return StatusCode(StatusCodes.Status400BadRequest, "Can't return the Vehicle.");
        }
    }
}
