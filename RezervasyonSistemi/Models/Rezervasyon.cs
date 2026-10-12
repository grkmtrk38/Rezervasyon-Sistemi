using System;
using System.ComponentModel.DataAnnotations;

namespace RezervasyonSistemi.Models
{
    public class Rezervasyon
    {
        [Key]
        public int Id { get; set; }

        // Kim rezerve ediyor? (Dropdown için Personel ilişkisi şart)
        [Required(ErrorMessage = "Personel seçimi zorunludur.")]
        public int PersonelId { get; set; }
        public Personel? Personel { get; set; }

        // Neyi rezerve ediyor? (Senin modelde bu eksik kalmış)
        [Required(ErrorMessage = "Kaynak seçimi zorunludur.")]
        public int KaynakId { get; set; }
        public Kaynak? Kaynak { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public DateTime BaslangicTarihi { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public DateTime BitisTarihi { get; set; }

        // string yanındaki "?" işareti bu alanların "null" (boş) geçilebileceğini belirtir.
        [MaxLength(500, ErrorMessage = "Açıklama alanı en fazla 500 karakter olabilir.")]
        public string? Aciklama { get; set; }

        [MaxLength(50)]
        public string? Durum { get; set; }

        // Oluşturma tarihi formda sorulmasın, otomatik şu anki zamanı alsın.
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}