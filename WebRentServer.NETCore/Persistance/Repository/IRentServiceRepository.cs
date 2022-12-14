using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface IRentServiceRepository : IRepository<RentService, int>
    {
        IEnumerable<RentService> GetAllServicesWithSorting(int pageIndex, int pageSize, int sortingType);
        RentService GetServiceWithVehicles(int serviceId);
        RentService GetServiceWithComments(int serviceId);
        Task<RentService> GetServiceWithVehiclesAsync(int serviceId);
        Task<RentService> GetServiceWithCommentsAsync(int serviceId);
        Task<RentService> GetWithUserAsync(int id);
    }
}
