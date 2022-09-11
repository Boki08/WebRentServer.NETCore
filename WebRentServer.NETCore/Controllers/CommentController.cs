using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.UnitOfWork;
using static WebRentServer.NETCore.Models.BindingModels;

namespace RentApp.Controllers
{
    [Route("api/comments")]
    public class CommentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "AppUser")]
        [Route("getComment/{orderId}/{userId}")]
        public async Task<IActionResult> GetUserCommentAsync(int orderId, int userId)
        {
            Comment comment;
            try
            {
                if (!(await _unitOfWork.Orders.FindAsync(x => x.UserId == userId && x.OrderId == orderId)).Any())
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Comment could not be found.");
                }

                comment = (Comment)await _unitOfWork.Comments.FindAsync(x => x.OrderId == orderId);
                if (comment == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Comment could not be found.");
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Comment could not be found.");
            }
            return StatusCode(StatusCodes.Status200OK, comment);
        }

        [HttpGet]
        [Authorize(Roles = "AppUser")]
        [Route("getCanUserComment/{orderId}/{userId}")]
        public async Task<IActionResult> GetCanUserCommentAsync(int orderId, int userId)
        {
            Comment comment;
            try
            {
                if (!(await _unitOfWork.Orders.FindAsync(x => x.UserId == userId && x.OrderId == orderId)).Any())
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Order does not exist.");
                }
                comment = (Comment)await _unitOfWork.Comments.FindAsync(x => x.OrderId == orderId);
                if (comment == null)
                {
                    Order order = await _unitOfWork.Orders.GetAsync(orderId);
                    if (order.ReturnDate <= DateTime.Now)
                    {
                        return StatusCode(StatusCodes.Status200OK, "canComment");
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, "can'tCommentYet");
                    }
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Comment could not be found.");
            }
            return StatusCode(StatusCodes.Status200OK, "commentExists");
        }

        [HttpPost]
        [Authorize(Roles = "AppUser")]
        [Route("postComment")]
        public async Task<IActionResult> PostCommentAsync([FromForm] CommentBindingModel commentBindingModel)
        {

            int userId;
            try
            {
                var username = User.Identity.Name;

                var user = await _unitOfWork.AppUsers.FindAsync(u => u.Email == username);
                if (user == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Data could not be retrieved, try to relog.");
                }

                userId = (user as AppUser).UserId;
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User not found, try to relog");
            }

            commentBindingModel.PostedDate = DateTime.Now;

            Order order;
            try
            {
                order = _unitOfWork.Orders.GetWithVehicles(commentBindingModel.OrderId);
                if (order == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Order could not be found.");
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Order could not be found.");
            }

            if (order.ReturnDate > commentBindingModel.PostedDate)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Can't comment before the return date");
            }

            RentService service;

            try
            {
                service = await _unitOfWork.RentServices.GetServiceWithCommentsAsync(order.Vehicle.RentServiceId);
                if (service == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Rent Service could not be found.");
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Rent Service could not be found.");
            }


            int numOfComments = service.Comments.Count;
            double sumGrades = 0;
            if (numOfComments > 0)
            {
                foreach (Comment c in service.Comments)
                {
                    numOfComments++;
                    sumGrades += c.Grade;
                }
            }

            sumGrades += commentBindingModel.Grade;

            service.Grade = sumGrades / (numOfComments + 1);

            Comment comment = new Comment()
            {
                Grade = commentBindingModel.Grade,
                OrderId = commentBindingModel.OrderId,
                PostedDate = commentBindingModel.PostedDate,
                Review = commentBindingModel.Review.Trim()
            };


            service.Comments.Add(comment);

            try
            {
                _unitOfWork.RentServices.Update(service);
                _unitOfWork.Complete();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Can't refresh average grade.");
            }
            return Created("Comment was posted", comment);
        }

        [HttpGet]
        [Route("getServiceComments/{serviceId}")]
        public async Task<IActionResult> GetServiceCommentsAsync(int serviceId)
        {
            List<Comment> comments;
            try
            {
                RentService service = await _unitOfWork.RentServices.GetServiceWithCommentsAsync(serviceId);
                comments = service.Comments;
                if (comments == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "There are no comments for this service");
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Can't get comments right now");
            }
            return StatusCode(StatusCodes.Status200OK, comments);
        }
    }
}