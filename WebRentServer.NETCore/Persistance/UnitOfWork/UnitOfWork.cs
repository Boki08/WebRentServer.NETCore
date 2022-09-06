using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;
using WebRentServer.NETCore.Persistance.Repository;

namespace WebRentServer.NETCore.Persistance.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RVDBContext _context;

        public IRentServiceRepository RentServices { get; private set; }
        public ICommentRepository Comments { get; private set; }
        public IAppUserRepository AppUsers { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IVehicleRepository Vehicles { get; private set; }
        public IVehiclePictureRepository VehiclePictures { get; private set; }
        public IOfficeRepository Offices { get; private set; }
        public ITypeOfVehicleRepository TypesOfVehicles { get; private set; }

        public UnitOfWork(RVDBContext context)
        {
            _context = context;
            RentServices = new RentServiceRepository(context);
            Comments = new CommentRepository(context);
            AppUsers = new AppUserRepository(context);
            Orders = new OrderRepository(context);
            Vehicles = new VehicleRepository(context);
            VehiclePictures = new VehiclePictureRepository(context);
            Offices = new OfficeRepository(context);
            TypesOfVehicles = new TypeOfVehicleRepository(context);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}