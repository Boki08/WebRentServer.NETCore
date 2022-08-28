using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface IVehicleRepository : IRepository<Vehicle, int>
    {
        IEnumerable<Vehicle> GetAllWithPics(int pageIndex, int pageSize, int rentServiceId);
        int CountServiceVehicles(int rentServiceId);
        IEnumerable<Vehicle> GetAllWithPicsUser(int pageIndex, int pageSize, int rentServiceId, bool available, string price, int type);
        int CountAllWithPicsUser(int rentServiceId, bool available, string price, int type);
    }
}
