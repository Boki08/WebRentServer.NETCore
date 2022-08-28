using WebRentServer.NETCore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class VehicleRepository : Repository<Vehicle, int>, IVehicleRepository
    {
        public VehicleRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }

        public IEnumerable<Vehicle> GetAllWithPics(int pageIndex, int pageSize,int rentServiceId)
        {
            return repoContext.Vehicles.Include(s=>s.VehiclePictures).Where(x=>x.RentServiceId== rentServiceId).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IEnumerable<Vehicle> GetAllWithPicsUser(int pageIndex, int pageSize, int rentServiceId, bool available, string price, int type)
        {
            if (available == true) {
                if (price == "Low") {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true).OrderBy(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true && x.TypeId==type).OrderBy(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
                if (price == "High")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true).OrderByDescending(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true && x.TypeId == type).OrderByDescending(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
                else
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true && x.TypeId == type).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
            }
            else
            {
                if (price == "Low")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true ).OrderBy(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.TypeId == type).OrderBy(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
                if (price == "High")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true ).OrderByDescending(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true &&  x.TypeId == type).OrderByDescending(x => x.HourlyPrice).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
                else
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true ).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.TypeId == type).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
            }
        }

        public int CountAllWithPicsUser( int rentServiceId, bool available, string price, int type)
        {
            if (available == true)
            {
                if (price == "Low")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true).OrderBy(x => x.HourlyPrice).ToList().Count;
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true && x.TypeId == type).OrderBy(x => x.HourlyPrice).ToList().Count;
                    }
                }
                if (price == "High")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true).OrderByDescending(x => x.HourlyPrice).ToList().Count;
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true && x.TypeId == type).OrderByDescending(x => x.HourlyPrice).ToList().Count;
                    }
                }
                else
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true).ToList().Count;
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.Available == true && x.TypeId == type).ToList().Count;
                    }
                }
            }
            else
            {
                if (price == "Low")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true).OrderBy(x => x.HourlyPrice).ToList().Count;
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.TypeId == type).OrderBy(x => x.HourlyPrice).ToList().Count;
                    }
                }
                if (price == "High")
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true).OrderByDescending(x => x.HourlyPrice).ToList().Count;
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.TypeId == type).OrderByDescending(x => x.HourlyPrice).ToList().Count;
                    }
                }
                else
                {
                    if (type == -1)
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true).ToList().Count;
                    }
                    else
                    {
                        return repoContext.Vehicles.Include(s => s.VehiclePictures).Where(x => x.RentServiceId == rentServiceId && x.Enabled == true && x.TypeId == type).ToList().Count;
                    }
                }
            }
        }

        public int CountServiceVehicles(int rentServiceId)
        {
            return repoContext.Vehicles.Where(x => x.RentServiceId == rentServiceId).Count();
        }
    }
}