using Microsoft.EntityFrameworkCore;

namespace ToastCore.Models
{
    public class ToastCoreContext : DbContext
    {
        public ToastCoreContext(DbContextOptions<ToastCoreContext> options) 
            : base(options)
        {
        }

        public DbSet<Toaster> Toasters { get; set; }

        public DbSet<Pantry> Pantries { get; set; }

        public DbSet<SuperMarket> SuperMarkets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Toaster>().ToTable("Toasters");

            modelBuilder.Entity<Pantry>().ToTable("Pantries");

            modelBuilder.Entity<SuperMarket>().ToTable("SuperMarkets");
        }
    }
}
