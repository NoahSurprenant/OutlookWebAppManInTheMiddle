using Microsoft.EntityFrameworkCore;
using OutlookWebAppManInTheMiddle.Models;

namespace OutlookWebAppManInTheMiddle
{
    public class OutlookWebAppDbContext : DbContext
    {
        public OutlookWebAppDbContext(DbContextOptions<OutlookWebAppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }
    }
}
