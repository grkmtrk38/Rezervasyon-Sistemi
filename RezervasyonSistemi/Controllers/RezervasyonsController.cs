using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezervasyonSistemi.Models;

namespace RezervasyonSistemi.Controllers
{
    [Authorize]
    public class RezervasyonsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RezervasyonsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper: Kullanıcının Admin veya Rezervasyon Sahibi olup olmadığını doğrular
        private async Task<bool> CanUserModifyReservationAsync(Rezervasyon rezervasyon)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.PersonelId.HasValue && rezervasyon.PersonelId == user.PersonelId.Value)
            {
                return true;
            }

            return false;
        }

        // Helper: Oturum açan kullanıcının otomatik Personel kaydını tespit eder veya oluşturur
        private async Task<int> EnsurePersonelForCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.PersonelId.HasValue)
            {
                var existing = await _context.Personeller.FindAsync(user.PersonelId.Value);
                if (existing != null)
                {
                    return existing.Id;
                }
            }

            string ad = user?.Ad ?? "Kullanıcı";
            string soyad = user?.Soyad ?? "";
            string birim = user?.Birim ?? "Genel";

            var matchedPersonel = await _context.Personeller.FirstOrDefaultAsync(p => p.Ad == ad && p.Soyad == soyad);
            if (matchedPersonel != null)
            {
                if (user != null)
                {
                    user.PersonelId = matchedPersonel.Id;
                    await _userManager.UpdateAsync(user);
                }
                return matchedPersonel.Id;
            }

            var newPersonel = new Personel
            {
                Ad = ad,
                Soyad = soyad,
                Birim = birim
            };
            _context.Personeller.Add(newPersonel);
            await _context.SaveChangesAsync();

            if (user != null)
            {
                user.PersonelId = newPersonel.Id;
                await _userManager.UpdateAsync(user);
            }

            return newPersonel.Id;
        }

        // GET: Rezervasyons (Görünürlük Filtreli)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var currentPersonelId = user?.PersonelId;
            bool isAdmin = User.IsInRole("Admin");

            IQueryable<Rezervasyon> query = _context.Rezervasyonlar
                .Include(r => r.Kaynak)
                .Include(r => r.Personel);

            if (!isAdmin)
            {
                // Standart kullanıcı: Tüm Onaylanmış rezervasyonları + Kendi yaptığı kayıtları görür
                query = query.Where(r => r.Durum == "Onaylandı" || (currentPersonelId.HasValue && r.PersonelId == currentPersonelId.Value));
            }

            var list = await query.OrderByDescending(r => r.BaslangicTarihi).ToListAsync();
            return View(list);
        }

        // GET: Rezervasyons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervasyon = await _context.Rezervasyonlar
                .Include(r => r.Kaynak)
                .Include(r => r.Personel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rezervasyon == null)
            {
                return NotFound();
            }

            return View(rezervasyon);
        }

        // GET: Rezervasyons/Create
        public async Task<IActionResult> Create()
        {
            var kaynakTipleri = await _context.Kaynaklar
                .Where(k => k.Aktif)
                .Select(k => k.Tip)
                .Distinct()
                .ToListAsync();

            ViewData["KaynakTipleri"] = new SelectList(kaynakTipleri);
            return View();
        }

        // JSON API: Kaynak türüne göre asenkron kaynak listesi (Cascading Dropdown)
        [HttpGet]
        public async Task<IActionResult> GetKaynaklarByTip(string tip)
        {
            if (string.IsNullOrEmpty(tip))
            {
                return Json(new List<object>());
            }

            var kaynaklar = await _context.Kaynaklar
                .Where(k => k.Aktif && k.Tip == tip)
                .Select(k => new { id = k.Id, ad = $"{k.Ad} ({k.Kapasite} Kişilik)" })
                .ToListAsync();

            return Json(kaynaklar);
        }

        // JSON API: Seçilen kaynağa ait dolu tarih/saat aralıklarını Flatpickr formatında döner
        [HttpGet]
        public async Task<IActionResult> GetDoluTarihler(int kaynakId)
        {
            if (kaynakId <= 0)
            {
                return Json(new List<object>());
            }

            var doluTarihler = await _context.Rezervasyonlar
                .Where(r => r.KaynakId == kaynakId && r.Durum != "İptal Edildi" && r.Durum != "Iptal" && r.Durum != "Reddedildi")
                .Select(r => new
                {
                    from = r.BaslangicTarihi.ToString("yyyy-MM-dd HH:mm"),
                    to = r.BitisTarihi.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(doluTarihler);
        }

        // POST: Rezervasyons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KaynakId,BaslangicTarihi,BitisTarihi,Aciklama")] Rezervasyon rezervasyon)
        {
            rezervasyon.PersonelId = await EnsurePersonelForCurrentUserAsync();

            // Çakışma Kontrolü (Double-Booking Check): Beklemede ve Onaylandı durumdakiler çakışmayı engeller
            bool cakisimVar = await _context.Rezervasyonlar.AnyAsync(r =>
                r.KaynakId == rezervasyon.KaynakId &&
                r.Durum != "İptal Edildi" && r.Durum != "Iptal" && r.Durum != "Reddedildi" &&
                ((rezervasyon.BaslangicTarihi >= r.BaslangicTarihi && rezervasyon.BaslangicTarihi < r.BitisTarihi) ||
                 (rezervasyon.BitisTarihi > r.BaslangicTarihi && rezervasyon.BitisTarihi <= r.BitisTarihi) ||
                 (rezervasyon.BaslangicTarihi <= r.BaslangicTarihi && rezervasyon.BitisTarihi >= r.BitisTarihi))
            );

            if (cakisimVar)
            {
                ModelState.AddModelError(string.Empty, "Seçilen kaynak belirtilen tarih ve saat aralığında zaten rezerve edilmiştir!");
            }

            if (ModelState.IsValid)
            {
                // Varsayılan onay statüsü: Beklemede
                rezervasyon.Durum = "Beklemede";
                rezervasyon.OlusturmaTarihi = DateTime.Now;
                _context.Add(rezervasyon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var kaynakTipleri = await _context.Kaynaklar
                .Where(k => k.Aktif)
                .Select(k => k.Tip)
                .Distinct()
                .ToListAsync();

            ViewData["KaynakTipleri"] = new SelectList(kaynakTipleri);
            return View(rezervasyon);
        }

        // POST: Rezervasyons/Approve/5 (Admin Onay)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? returnUrl = null)
        {
            var rezervasyon = await _context.Rezervasyonlar.FindAsync(id);
            if (rezervasyon != null)
            {
                rezervasyon.Durum = "Onaylandı";
                _context.Update(rezervasyon);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Rezervasyons/Reject/5 (Admin Red)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? returnUrl = null)
        {
            var rezervasyon = await _context.Rezervasyonlar.FindAsync(id);
            if (rezervasyon != null)
            {
                rezervasyon.Durum = "Reddedildi";
                _context.Update(rezervasyon);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> OnayBekleyenler()
        {
            var beklemedekiler = await _context.Rezervasyonlar
                .Include(r => r.Personel)
                .Include(r => r.Kaynak)
                .Where(r => r.Durum == "Beklemede")
                .OrderBy(r => r.BaslangicTarihi)
                .ToListAsync();
            return View(beklemedekiler);
        }

        // GET: Rezervasyons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervasyon = await _context.Rezervasyonlar.FindAsync(id);
            if (rezervasyon == null)
            {
                return NotFound();
            }

            if (!await CanUserModifyReservationAsync(rezervasyon))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewData["KaynakId"] = new SelectList(_context.Kaynaklar, "Id", "Ad", rezervasyon.KaynakId);
            ViewData["PersonelId"] = new SelectList(_context.Personeller.Select(p => new { Id = p.Id, TamAd = p.Ad + " " + p.Soyad }), "Id", "TamAd", rezervasyon.PersonelId);
            return View(rezervasyon);
        }

        // POST: Rezervasyons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PersonelId,KaynakId,BaslangicTarihi,BitisTarihi,Aciklama,Durum,OlusturmaTarihi")] Rezervasyon rezervasyon)
        {
            if (id != rezervasyon.Id)
            {
                return NotFound();
            }

            var existing = await _context.Rezervasyonlar.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!await CanUserModifyReservationAsync(existing))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rezervasyon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezervasyonExists(rezervasyon.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KaynakId"] = new SelectList(_context.Kaynaklar, "Id", "Ad", rezervasyon.KaynakId);
            ViewData["PersonelId"] = new SelectList(_context.Personeller.Select(p => new { Id = p.Id, TamAd = p.Ad + " " + p.Soyad }), "Id", "TamAd", rezervasyon.PersonelId);
            return View(rezervasyon);
        }

        // GET: Rezervasyons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervasyon = await _context.Rezervasyonlar
                .Include(r => r.Kaynak)
                .Include(r => r.Personel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rezervasyon == null)
            {
                return NotFound();
            }

            if (!await CanUserModifyReservationAsync(rezervasyon))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(rezervasyon);
        }

        // POST: Rezervasyons/Delete/5 (SOFT DELETE)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervasyon = await _context.Rezervasyonlar.FindAsync(id);
            if (rezervasyon != null)
            {
                if (!await CanUserModifyReservationAsync(rezervasyon))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                rezervasyon.Durum = "İptal Edildi";
                _context.Update(rezervasyon);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RezervasyonExists(int id)
        {
            return _context.Rezervasyonlar.Any(e => e.Id == id);
        }

        public IActionResult Takvim()
        {
            return View();
        }

        // JSON API: Takvim Verisi (Görünürlük Filtreli, Durum Renkli ve Detaylı extendedProps)
        [HttpGet]
        public async Task<IActionResult> GetRezervasyonlar()
        {
            var user = await _userManager.GetUserAsync(User);
            var currentPersonelId = user?.PersonelId;
            bool isAdmin = User.IsInRole("Admin");

            IQueryable<Rezervasyon> query = _context.Rezervasyonlar
                .Include(r => r.Personel)
                .Include(r => r.Kaynak);

            if (!isAdmin)
            {
                query = query.Where(r => r.Durum == "Onaylandı" || (currentPersonelId.HasValue && r.PersonelId == currentPersonelId.Value));
            }

            var rawList = await query.ToListAsync();

            var rezervasyonlar = rawList.Select(r => {
                string durum = r.Durum ?? "Beklemede";
                string personelAdi = r.Personel != null ? $"{r.Personel.Ad} {r.Personel.Soyad}" : "Belirtilmedi";
                string kaynakAdi = r.Kaynak != null ? r.Kaynak.Ad : "Belirtilmedi";

                string colorHex = "#198754"; // Onaylandı: Yeşil
                string prefix = "";

                if (durum == "Beklemede")
                {
                    colorHex = "#ffc107"; // Beklemede: Sarı/Turuncu
                    prefix = "[BEKLEMEDE] ";
                }
                else if (durum == "Onaylandı")
                {
                    colorHex = "#198754"; // Onaylandı: Yeşil
                    prefix = "";
                }
                else if (durum == "Reddedildi")
                {
                    colorHex = "#dc3545"; // Reddedildi: Kırmızı
                    prefix = "[REDDEDİLDİ] ";
                }
                else if (durum == "İptal Edildi" || durum == "Iptal")
                {
                    colorHex = "#6c757d"; // İptal: Gri
                    prefix = "[İPTAL] ";
                }

                bool isOwnerOrAdmin = isAdmin || (currentPersonelId.HasValue && r.PersonelId == currentPersonelId.Value);

                return new
                {
                    id = r.Id,
                    title = $"{prefix}{personelAdi} ({kaynakAdi})",
                    start = r.BaslangicTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.BitisTarihi.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = r.Aciklama,
                    backgroundColor = colorHex,
                    borderColor = colorHex,
                    textColor = (durum == "Beklemede") ? "#000000" : "#ffffff",
                    extendedProps = new
                    {
                        personel = personelAdi,
                        kaynak = kaynakAdi,
                        baslangic = r.BaslangicTarihi.ToString("dd.MM.yyyy HH:mm"),
                        bitis = r.BitisTarihi.ToString("dd.MM.yyyy HH:mm"),
                        durum = durum,
                        aciklama = string.IsNullOrEmpty(r.Aciklama) ? "-" : r.Aciklama,
                        olusturma = r.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm"),
                        canEditDelete = isOwnerOrAdmin,
                        isAdmin = isAdmin
                    }
                };
            }).ToList();

            return Json(rezervasyonlar);
        }

        // EXCEL / CSV DIŞA AKTARMA (REPORTING)
        [HttpGet]
        public async Task<IActionResult> ExportToCsv()
        {
            var user = await _userManager.GetUserAsync(User);
            var currentPersonelId = user?.PersonelId;
            bool isAdmin = User.IsInRole("Admin");

            IQueryable<Rezervasyon> query = _context.Rezervasyonlar
                .Include(r => r.Personel)
                .Include(r => r.Kaynak);

            if (!isAdmin)
            {
                query = query.Where(r => r.Durum == "Onaylandı" || (currentPersonelId.HasValue && r.PersonelId == currentPersonelId.Value));
            }

            var rezervasyonlar = await query.OrderByDescending(r => r.BaslangicTarihi).ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id;Personel;Kaynak;BaslangicTarihi;BitisTarihi;Durum;Aciklama;OlusturmaTarihi");

            foreach (var item in rezervasyonlar)
            {
                string personel = item.Personel != null ? $"{item.Personel.Ad} {item.Personel.Soyad}" : "";
                string kaynak = item.Kaynak != null ? item.Kaynak.Ad : "";
                string baslangic = item.BaslangicTarihi.ToString("dd.MM.yyyy HH:mm");
                string bitis = item.BitisTarihi.ToString("dd.MM.yyyy HH:mm");
                string durum = item.Durum ?? "Beklemede";
                string aciklama = (item.Aciklama ?? "").Replace(";", " ").Replace("\r", " ").Replace("\n", " ");
                string olusturma = item.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm");

                builder.AppendLine($"{item.Id};\"{personel}\";\"{kaynak}\";{baslangic};{bitis};\"{durum}\";\"{aciklama}\";{olusturma}");
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var bom = Encoding.UTF8.GetPreamble();
            var fileBytes = bom.Concat(bytes).ToArray();

            string fileName = $"RezervasyonListesi_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            return File(fileBytes, "text/csv; charset=utf-8", fileName);
        }
    }
}
