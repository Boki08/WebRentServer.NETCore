using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface ITypeOfVehicleRepository : IRepository<TypeOfVehicle, int>
    {
        IEnumerable<TypeOfVehicle> GetAllPaged(int pageIndex, int pageSize);
    }
}
