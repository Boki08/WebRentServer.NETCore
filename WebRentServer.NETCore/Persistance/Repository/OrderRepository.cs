using WebRentServer.NETCore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class OrderRepository:Repository<Order,int>,IOrderRepository
    {
        public OrderRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }

        public IEnumerable<Order> GetAllUserOrders(int pageIndex, int pageSize, int userId)
        {
            return repoContext.Orders.Include(s => s.DepartureOffice).Include(r=>r.ReturnOffice).Include(v=>v.Vehicle).Where(x => x.UserId == userId).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public int CountAllUserOrders( int userId)
        {
            return repoContext.Orders.Where(x => x.UserId == userId).ToList().Count;
        }

        public Order GetWithVehicles(int orderId)
        {
            return repoContext.Orders.Include(v => v.Vehicle).Where(x=>x.OrderId==orderId).FirstOrDefault();
        }

        public IEnumerable<Order> GetServiceOrders(int serviceId)
        {
            return repoContext.Orders.Include(v => v.Vehicle).Where(x => x.Vehicle.RentServiceId == serviceId);
        }
    }
}
