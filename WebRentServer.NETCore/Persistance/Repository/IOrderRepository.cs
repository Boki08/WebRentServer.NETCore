using WebRentServer.NETCore.Models.Entities;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface IOrderRepository:IRepository<Order, int>
    {
        IEnumerable<Order> GetAllUserOrders(int pageIndex, int pageSize, int userId);
        int CountAllUserOrders(int userId);
        Order GetWithVehicles(int orderId);
        ///<summary>
        ///<para>gets all orders by Order.Vehicle.RentServiceId</para>
        ///</summary>
        IEnumerable<Order> GetServiceOrders(int serviceId);
    }
}
