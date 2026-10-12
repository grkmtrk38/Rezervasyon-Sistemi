using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RezervasyonSistemi.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak tablolarımız:
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<Kaynak> Kaynaklar { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}