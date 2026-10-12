namespace RezervasyonSistemi.Models
{
    public class Kaynak
    {
        public int Id { get; set; } // Kaynağın eşsiz numarası
        public string Ad { get; set; } // Örn: A Toplantı Salonu, Fiat Egea
        public string Tip { get; set; } // Araç mı? Toplantı Salonu mu?
        public int Kapasite { get; set; } // Kaç kişilik?
        public bool Aktif { get; set; } // Kullanımda mı, arızalı/iptal mi? (True/False döner)
    }
}
