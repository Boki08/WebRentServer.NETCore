using WebRentServer.NETCore.Persistance.Repository;

namespace WebRentServer.NETCore.Persistance.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRentServiceRepository RentServices { get; }
        ICommentRepository Comments { get;  }
        IAppUserRepository AppUsers { get;  }
        IOrderRepository Orders { get;  }
        IVehicleRepository Vehicles { get;  }
        IVehiclePictureRepository VehiclePictures { get;  }
        IOfficeRepository Offices { get;  }
        ITypeOfVehicleRepository TypesOfVehicles { get;  }
        int Complete();
    }
}
