using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface IOfficeRepository : IRepository<Office, int>
    {
        IEnumerable<Office> GetAll(int pageIndex, int pageSize, int rentServiceId);
    }
}
