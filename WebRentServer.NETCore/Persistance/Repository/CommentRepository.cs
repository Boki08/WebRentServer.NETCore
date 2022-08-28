using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class CommentRepository : Repository<Comment, int>, ICommentRepository
    {
        public CommentRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }

        public IEnumerable<Comment> GetServiceComments(int serviceId)
        {
            return repoContext.Comments.Where(cm => cm.Order.Vehicle.RentServiceId == serviceId);
        }

        public Comment GetCommentWithOrder(int commentId)
        {
            return repoContext.Comments.Include(x =>x.Order).Where(x => x.CommentId == commentId).FirstOrDefault();
        }
    }
}