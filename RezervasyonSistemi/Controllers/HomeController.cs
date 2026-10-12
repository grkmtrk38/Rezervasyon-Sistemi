using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezervasyonSistemi.Models;
using RezervasyonSistemi.Models.ViewModels;

namespace RezervasyonSistemi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var viewModel = new DashboardViewModel
                {
                    ToplamKaynakSayisi = await _context.Kaynaklar.CountAsync(k => k.Aktif),
                    ToplamAktifRezervasyonSayisi = await _context.Rezervasyonlar.CountAsync(r => r.Durum != "İptal Edildi" && r.Durum != "Iptal"),
                    BugunkuRezervasyonSayisi = await _context.Rezervasyonlar.CountAsync(r => r.BaslangicTarihi >= today && r.BaslangicTarihi < tomorrow && r.Durum != "İptal Edildi" && r.Durum != "Iptal"),
                    IptalEdilenRezervasyonSayisi = await _context.Rezervasyonlar.CountAsync(r => r.Durum == "İptal Edildi" || r.Durum == "Iptal"),
                    YaklasanRezervasyonlar = await _context.Rezervasyonlar
                        .Include(r => r.Personel)
                        .Include(r => r.Kaynak)
                        .Where(r => r.BaslangicTarihi >= DateTime.Now && r.Durum != "İptal Edildi" && r.Durum != "Iptal")
                        .OrderBy(r => r.BaslangicTarihi)
                        .Take(5)
                        .ToListAsync()
                };

                return View(viewModel);
            }

            return View(new DashboardViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
