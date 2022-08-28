using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface ICommentRepository : IRepository<Comment, int>
    {
        IEnumerable<Comment> GetServiceComments(int serviceId);
        Comment GetCommentWithOrder(int commentId);
    }
}
