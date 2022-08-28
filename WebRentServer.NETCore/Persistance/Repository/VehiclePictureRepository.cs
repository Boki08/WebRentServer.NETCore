using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class VehiclePictureRepository : Repository<VehiclePicture, int>, IVehiclePictureRepository
    {
        public VehiclePictureRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }
    }
}