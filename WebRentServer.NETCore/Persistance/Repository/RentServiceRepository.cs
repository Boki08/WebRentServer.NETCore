using Microsoft.EntityFrameworkCore;
using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class RentServiceRepository : Repository<RentService, int>, IRentServiceRepository
    {
        public RentServiceRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }

        public IEnumerable<RentService> GetAllServicesWithSorting(int pageIndex, int pageSize, int sortingType)
        {
            if (sortingType == 1)//noSorting
            {
                return repoContext.RentServices.Where(s => s.Activated == true).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            else if (sortingType == 2)// bestGades
            {
                return repoContext.RentServices.Where(s => s.Activated == true).OrderByDescending(x => x.Grade).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            else if (sortingType == 3)//mostVehicles
            {
                return repoContext.RentServices.Where(s => s.Activated == true).OrderByDescending(x => x.Vehicles.Count).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            else//mostOrders
            {
                return repoContext.RentServices.Include(v2 => v2.Vehicles).Where(s => s.Activated == true).OrderByDescending(x => x.Vehicles.Sum(o => o.Orders.Count)).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }

        }
        public RentService GetServiceWithVehicles(int serviceId)
        {
            return repoContext.RentServices.Include(x => x.Vehicles).Where(r => r.RentServiceId == serviceId).FirstOrDefault();
        }

        public RentService GetServiceWithComments(int serviceId)
        {
            return repoContext.RentServices.Include(x => x.Comments).Where(r => r.RentServiceId == serviceId).FirstOrDefault();
        }

        public async Task<RentService> GetServiceWithVehiclesAsync(int serviceId)
        {
            return await repoContext.RentServices.Include(x => x.Vehicles).Where(r => r.RentServiceId == serviceId).FirstOrDefaultAsync();
        }

        public async Task<RentService> GetServiceWithCommentsAsync(int serviceId)
        {
            return await repoContext.RentServices.Include(x => x.Comments).Where(r => r.RentServiceId == serviceId).FirstOrDefaultAsync();
        }
    }
}