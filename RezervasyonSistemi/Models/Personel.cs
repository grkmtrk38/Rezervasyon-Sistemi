namespace RezervasyonSistemi.Models
{
    public class Personel
    {
        public int Id { get; set; } // Veritabanında her personelin eşsiz numarası (Primary Key)
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Birim { get; set; } // Müdürlük vb.
    }
}
