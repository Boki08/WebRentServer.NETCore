using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class TypeOfVehicleRepository : Repository<TypeOfVehicle, int>, ITypeOfVehicleRepository
    {
        public TypeOfVehicleRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }

        public IEnumerable<TypeOfVehicle> GetAllPaged(int pageIndex, int pageSize)
        {
            return repoContext.TypesOfVehicles.ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}