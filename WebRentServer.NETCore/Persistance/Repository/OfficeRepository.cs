using WebRentServer.NETCore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class OfficeRepository : Repository<Office, int>, IOfficeRepository
    {
        public OfficeRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }

        public IEnumerable<Office> GetAll(int pageIndex, int pageSize, int rentServiceId)
        {
            return repoContext.Offices.Where(x => x.RentServiceId == rentServiceId).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}