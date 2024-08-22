using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api_Mail_gondericili.Model
{
    public class AppDbCont : IdentityDbContext<AppUser>
    {
        public AppDbCont(DbContextOptions<AppDbCont> options): base(options)
        {
        }
    }
}
