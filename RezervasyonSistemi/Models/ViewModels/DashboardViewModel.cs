using System.Collections.Generic;
using RezervasyonSistemi.Models;

namespace RezervasyonSistemi.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int ToplamKaynakSayisi { get; set; }
        public int ToplamAktifRezervasyonSayisi { get; set; }
        public int BugunkuRezervasyonSayisi { get; set; }
        public int IptalEdilenRezervasyonSayisi { get; set; }
        public List<Rezervasyon> YaklasanRezervasyonlar { get; set; } = new List<Rezervasyon>();
    }
}
