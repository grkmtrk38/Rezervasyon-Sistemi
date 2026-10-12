using Microsoft.AspNetCore.Identity;

namespace RezervasyonSistemi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string? Birim { get; set; }

        public int? PersonelId { get; set; }
        public Personel? Personel { get; set; }
    }
}
