using WebRentServer.NETCore.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class AppUserRepository : Repository<AppUser, int>, IAppUserRepository
    {
        public AppUserRepository(RVDBContext context) : base(context){}

        protected RVDBContext repoContext { get { return context as RVDBContext; } }
    }
}