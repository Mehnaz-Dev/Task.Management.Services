using Microsoft.EntityFrameworkCore;
using Task.Management.Services.Model;
using Task.Management.Services.Models;

namespace Task.Management.Services
{
    public class TaskManagementDbContext : DbContext
    {
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }  // Add this line
    }

}
